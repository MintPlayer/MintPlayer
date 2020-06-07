import { Component, OnInit, Inject, HostListener, DoCheck, KeyValueDiffers, KeyValueDiffer } from '@angular/core';
import { Song } from '../../../entities/song';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute, Router } from '@angular/router';
import { SongService } from '../../../services/song/song.service';
import { SlugifyHelper } from '../../../helpers/slugify.helper';
import { HasChanges } from '../../../interfaces/has-changes';
import { IBeforeUnloadEvent } from '../../../events/my-before-unload.event';

@Component({
  selector: 'app-sync',
  templateUrl: './sync.component.html',
  styleUrls: ['./sync.component.scss']
})
export class SyncComponent implements OnInit, DoCheck, HasChanges {

  constructor(@Inject('SERVERSIDE') serverSide: boolean, @Inject('SONG') private songInj: Song, private songService: SongService, private router: Router, private route: ActivatedRoute, private titleService: Title, private slugifyHelper: SlugifyHelper, private differs: KeyValueDiffers) {
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
    this.timelineDiffer = this.differs.find(this.song.lyrics.timeline).create();
    setTimeout(() => this.hasChanges = false);
  }

  updateTimeline() {
    this.songService.updateTimeline(this.song).then(() => {
      this.hasChanges = false;
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
