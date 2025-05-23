import { Component, OnInit, Inject, HostListener, DoCheck, KeyValueDiffers, KeyValueDiffer } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute } from '@angular/router';
import { AdvancedRouter } from '@mintplayer/ng-router';
import { SERVER_SIDE } from '@mintplayer/ng-server-side';
import { Song, SongService } from '@mintplayer/ng-client';
import { SlugifyHelper } from '../../../helpers/slugify.helper';
import { HasChanges } from '../../../interfaces/has-changes';
import { IBeforeUnloadEvent } from '../../../events/my-before-unload.event';

@Component({
  selector: 'app-sync',
  templateUrl: './sync.component.html',
  styleUrls: ['./sync.component.scss']
})
export class SyncComponent implements OnInit, DoCheck, HasChanges {

  constructor(
    @Inject(SERVER_SIDE) serverSide: boolean,
    @Inject('SONG') private songInj: Song,
    private songService: SongService,
    private router: AdvancedRouter,
    private route: ActivatedRoute,
    private titleService: Title,
    private slugifyHelper: SlugifyHelper,
    private differs: KeyValueDiffers,
  ) {
    if (serverSide === true) {
      this.setSong(songInj);
    } else {
      var id = parseInt(this.route.snapshot.paramMap.get('id'));
      this.loadSong(id);
    }
  }

  private loadSong(id: number) {
    this.songService.getSong(id, true).subscribe({
      next: (song) => {
        if (song.lyrics.timeline === null) {
          song.lyrics.timeline = [];
        }
        this.setSong(song);
      }, error: (error) => {
        console.error('Could not fetch song', error);
      }
    });
  }

  private setSong(song: Song) {
    this.song = song;
    if (song != null) {
      this.titleService.setTitle(`${song.title}: Video and lyrics`);
    }
    this.timelineDiffer = this.differs.find(this.song.lyrics.timeline).create();
    setTimeout(() => this.hasChanges = false);
  }

  updateTimeline() {
    this.songService.updateTimeline(this.song).subscribe({
      next: () => {
        this.hasChanges = false;
        this.router.navigate(['/song', this.song.id, this.slugifyHelper.slugify(this.song.text)]);
      }, error: (error) => {
        console.error(error);
      }
    });
  }

  song: Song = {
    id: 0,
    title: '',
    released: null,
    artists: [],
    uncreditedArtists: [],
    media: [],
    tags: [],
    lyrics: {
      text: '',
      timeline: []
    },
    text: '',
    youtubeId: '',
    dailymotionId: '',
    vimeoId: '',
    soundCloudUrl: '',
    playerInfos: [],
    description: '',
    dateUpdate: null
  };

  ngOnInit() {
  }

  //#region Prevent loss of changes
  hasChanges: boolean = false;
  private timelineDiffer: KeyValueDiffer<string, any> = null;
  @HostListener('window:beforeunload', ['$event'])
  beforeUnload($event: IBeforeUnloadEvent) {
    if (this.hasChanges) {
      $event.returnValue = '';
      if (!confirm("There are unsaved changes. Are you sure you want to quit?")) {
        $event.preventDefault();
      }
    }
  }

  ngDoCheck() {
    if (this.timelineDiffer !== null) {
      const changes = this.timelineDiffer.diff(this.song.lyrics.timeline);
      if (changes) {
        this.hasChanges = true;
      }
    }
  }
  //#endregion

}
