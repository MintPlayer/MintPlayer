import { EventEmitter } from '@angular/core';
import { eRepeatMode } from '../enums/eRepeatMode';

export class PlaylistControl<TVideo> {
  constructor(data?: Partial<PlaylistControl<TVideo>>) {
    Object.assign(this, data);
  }

  //#region Public

  //#region Methods
  //#region AddToPlaylist
  public async addToPlaylist(...videos: TVideo[]) {
    let clones = videos.map((video) => Object.assign({}, video));
    this._playlist.push(...clones);
    if (this._currentPlayedVideo === null) {
      let playedVideo: PlayedVideo<TVideo>;
      if (this.shuffle) {
        playedVideo = { video: this.findRandomVideo() };
      } else {
        playedVideo = { video: clones[0] };
      }
      this._actualPlaylist.push(playedVideo);
      await this.playVideo(playedVideo);
    }
  }
  //#endregion
  //#region SetPlaylist
  public async setPlaylist(videos: TVideo[]) {
    this._playlist.splice(0);
    this._actualPlaylist.splice(0);
    this._currentPlayedVideo = null;
    this.addToPlaylist(...videos);
  }
  //#endregion
  //#region Remove
  public async removeFromPlaylist(video: TVideo) {
    // Check if video to be removed is currently playing
    debugger;
    if (this._currentPlayedVideo !== null) {
      if (this._currentPlayedVideo.video === video) {
        let next = this.findNextNotCurrentVideo();
        if (next === null) {
          await this.playNextVideo(true);
        } else {
          await this.playVideo(next);
        }
      }
    }

    // Remove from the playlist
    this._playlist.splice(this._playlist.indexOf(video), 1);

    // Remove from the actual playlist
    this.removeSongFromActualPlaylist(video);
  }
  private removeSongFromActualPlaylist(video: TVideo) {
    let i = 0;
    while (i < this._actualPlaylist.length) {
      if (this._actualPlaylist[i].video === video) {
        this._actualPlaylist.splice(i, 1);
      } else {
        i++;
      }
    }
  }
  //#endregion
  //#region Previous
  public previous() {
    this.playPreviousVideo();
  }
  //#endregion
  //#region Next
  public next(force: boolean = false) {
    this.playNextVideo(force);
  }
  //#endregion
  //#endregion

  //#region Delegates
  public onGetCurrentPosition: () => number;
  public onPlayVideo: (video: TVideo) => void;
  public onStopVideo: () => void;
  //#endregion

  //#region Properties
  //#region Playlist
  public get playlist() {
    return this._playlist.map(v => v);
  }
  //#endregion
  //#region CurrentVideo
  public currentVideoChange: EventEmitter<TVideo> = new EventEmitter<TVideo>();
  public get currentVideo() {
    if (this._currentPlayedVideo === null) {
      return null;
    } else {
      return this._currentPlayedVideo.video;
    }
  }
  //#endregion
  //#region Repeat
  public repeatChange = new EventEmitter<eRepeatMode>();
  public get repeat() {
    if (typeof window !== 'undefined') {
      let setting = localStorage.getItem('mintplayer.repeat');
      if (setting === null) {
        return eRepeatMode.noRepeat;
      } else {
        return <eRepeatMode>(eRepeatMode[setting]);
      }
    } else {
      return eRepeatMode.noRepeat;
    }
  }
  public set repeat(value: eRepeatMode) {
    if (typeof window !== 'undefined') {
      localStorage.setItem('mintplayer.repeat', eRepeatMode[value]);
    }
    this.repeatChange.emit(value);
  }
  //#endregion
  //#region Shuffle
  public shuffleChange = new EventEmitter<boolean>();
  public get shuffle() {
    if (typeof window !== 'undefined') {
      let setting = localStorage.getItem('mintplayer.shuffle');
      if (setting === null) {
        return false;
      } else {
        return setting === 'true';
      }
    } else {
      return false;
    }
  }
  public set shuffle(value: boolean) {
    if (typeof window !== 'undefined') {
      localStorage.setItem('mintplayer.shuffle', String(value));
    }
    this.shuffleChange.emit(value);
  }
  //#endregion
  //#endregion

  //#endregion

  //#region Private

  //#region Fields
  private _playlist: TVideo[] = [];
  private _actualPlaylist: PlayedVideo<TVideo>[] = [];
  private _currentPlayedVideo: PlayedVideo<TVideo> = null;
  //#endregion

  //#region Methods
  private async playVideo(video: PlayedVideo<TVideo>) {
    this.onSetCurrentPlayedVideo(video);
    this.onPlayVideo(video.video);
  }
  private async onSetCurrentPlayedVideo(video: PlayedVideo<TVideo>) {
    this._currentPlayedVideo = video;
    await new Promise(resolve => setTimeout(resolve, 10));
    if (video === null) {
      this.currentVideoChange.emit(null);
    } else {
      this.currentVideoChange.emit(video.video);
    }
  }
  private async playNextVideo(force: boolean) {
    let result = this.findNextVideo(force);
    if (result === null) {
      this.onStopVideo();
      this.onSetCurrentPlayedVideo(null);
      return;
    }

    if (result.fromActualPlaylist === false) {
      this._actualPlaylist.push(result.playedVideo);
    }
    await this.playVideo(result.playedVideo);
  }
  private async playPreviousVideo() {
    var currentIndex = this._actualPlaylist.indexOf(this._currentPlayedVideo);
    if (currentIndex < 1) {
      if (this._actualPlaylist.length > 0) {
        this.playVideo(this._actualPlaylist[0]);
      }
    } else if (this.onGetCurrentPosition() < 5) {
      this.playVideo(this._actualPlaylist[currentIndex - 1]);
    } else {
      this.playVideo(this._actualPlaylist[currentIndex]);
    }
  }
  private findNextVideo(force: boolean): NextVideoResult<PlayedVideo<TVideo>> {
    debugger;
    if (this._playlist.length === 0) {
      // There are no more songs in the playlist
      return null;
    }

    if (this._currentPlayedVideo === null) {
      // The playlist is done playing. Play the first song from the playlist.
      return {
        playedVideo: {
          video: this._playlist[0]
        },
        fromActualPlaylist: false
      };
    }

    // If RepeatMode is set to RepeatOne, unconditionally return the same video
    if (this.repeat === eRepeatMode.repeatOne) {
      if (force === false) {
        return { playedVideo: this._currentPlayedVideo, fromActualPlaylist: true };
      }
    }

    let currentPlayedVideoIndex = this._actualPlaylist.indexOf(this._currentPlayedVideo);
    if (currentPlayedVideoIndex < this._actualPlaylist.length - 1) {
      // There are still videos left in the ActualPlaylist
      return {
        playedVideo: this._actualPlaylist[currentPlayedVideoIndex + 1],
        fromActualPlaylist: true
      };
    }


    //#region Helper methods
    let createPlayedVideo = (playlistIndex: number): NextVideoResult<PlayedVideo<TVideo>> => {
      return {
        playedVideo: { video: this._playlist[playlistIndex] },
        fromActualPlaylist: false
      };
    };
    let findNextVideoBase = (currentVideoIndex: number) => {
      // Random?
      if (this.shuffle) {
        let video = this.findRandomVideo();
        return <NextVideoResult<PlayedVideo<TVideo>>>{
          playedVideo: { video },
          fromActualPlaylist: false
        };
      } else {
        if (currentVideoIndex === this._playlist.length - 1) {
          return createPlayedVideo(0);
        } else {
          return createPlayedVideo(currentVideoIndex + 1);
        }
      }
    };
    //#endregion


    let currentVideoIndex = this._playlist.indexOf(this._currentPlayedVideo.video);
    switch (this.repeat) {
      case eRepeatMode.repeatOne:
        if (force === true) {
          let result = findNextVideoBase(currentVideoIndex);
          return result;
          //} else {
          //  return { playedVideo: this._currentPlayedVideo, fromActualPlaylist: true };
        }
      case eRepeatMode.repeatAll:
        {
          let video = findNextVideoBase(currentVideoIndex);
          return video;
        }
      case eRepeatMode.noRepeat:
        if (this.shuffle) {
          let video = this.findRandomVideo();
          return <NextVideoResult<PlayedVideo<TVideo>>>{
            playedVideo: { video },
            fromActualPlaylist: false
          };
        } else if (currentVideoIndex === this._playlist.length - 1) {
          return null;
        } else {
          return createPlayedVideo(currentVideoIndex + 1);
        }
    }
  }
  private findNextNotCurrentVideo() {
    let currentPlayedIndex = this._actualPlaylist.indexOf(this._currentPlayedVideo);
    let nextNotcurrentPlayedvideos = this._actualPlaylist.filter((item, index) => {
      if (index <= currentPlayedIndex) return false;
      if (item.video === this._currentPlayedVideo.video) return false;
      return true;
    });

    if (nextNotcurrentPlayedvideos.length === 0) return null;
    else return nextNotcurrentPlayedvideos[0];
  }

  private findRandomVideo() {
    let rnd = Math.floor(Math.random() * this._playlist.length);
    return this._playlist[rnd];
  }
  //#endregion

  //#endregion
}

interface PlayedVideo<TVideo> {
  video: TVideo;
}

interface NextVideoResult<TPlayedVideo> {
  playedVideo: TPlayedVideo;
  fromActualPlaylist: boolean;
}
