import { Component, OnInit, Inject } from '@angular/core';
import { Song } from '../../../entities/song';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute, Router } from '@angular/router';
import { SongService } from '../../../services/song/song.service';
import { SlugifyHelper } from '../../../helpers/slugify.helper';

@Component({
  selector: 'app-sync',
  templateUrl: './sync.component.html',
  styleUrls: ['./sync.component.scss']
})
export class SyncComponent implements OnInit {

  constructor(@Inject('SERVERSIDE') serverSide: boolean, @Inject('SONG') private songInj: Song, private songService: SongService, private router: Router, private route: ActivatedRoute, private titleService: Title, private slugifyHelper: SlugifyHelper) {
    if (serverSide === true) {
      this.setSong(songInj);
    } else {
      var id = parseInt(this.route.snapshot.paramMap.get('id'));
      this.loadSong(id);
    }
  }

  private loadSong(id: number) {
    this.songService.getSong(id, true).then((song) => {
      if (song.lyrics.timeline === null) {
        song.lyrics.timeline = [];
      }
      this.setSong(song);
    }).catch((error) => {
      console.error('Could not fetch song', error);
    });
  }

  private setSong(song: Song) {
    this.song = song;
    if (song != null) {
      this.titleService.setTitle(`${song.title}: Video and lyrics`);
    }
  }

  updateTimeline() {
    this.songService.updateTimeline(this.song).then(() => {
      this.router.navigate(['/song', this.song.id, this.slugifyHelper.slugify(this.song.text)]);
    }).catch((error) => {
      console.error(error);
    });
  }

  song: Song = {
    id: 0,
    title: '',
    released: null,
    artists: [],
    media: [],
    tags: [],
    lyrics: {
      text: '',
      timeline: []
    },
    text: '',
    youtubeId: '',
    dailymotionId: '',
    playerInfo: null,
    description: '',
    dateUpdate: null
  };

  ngOnInit() {
  }

}
