import { Component, OnInit, Inject, OnDestroy, HostListener } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { HttpHeaders } from '@angular/common/http';
import { Title } from '@angular/platform-browser';
import { SongService } from '../../../services/song/song.service';
import { Song } from '../../../entities/song';
import { Artist } from '../../../entities/artist';
import { MediumType } from '../../../entities/medium-type';
import { MediumTypeService } from '../../../services/medium-type/medium-type.service';
import { Tag } from '../../../entities/tag';
import { HtmlLinkHelper } from '../../../helpers/html-link.helper';
import { SlugifyHelper } from '../../../helpers/slugify.helper';

@Component({
  selector: 'app-edit',
  templateUrl: './edit.component.html',
  styleUrls: ['./edit.component.scss']
})
export class EditComponent implements OnInit, OnDestroy {
  constructor(@Inject('SERVERSIDE') private serverSide: boolean, private songService: SongService, private mediumTypeService: MediumTypeService, private router: Router, private route: ActivatedRoute, private titleService: Title, private htmlLink: HtmlLinkHelper, private slugifyHelper: SlugifyHelper) {
    if (serverSide === false) {
      // Get song
      var id = parseInt(this.route.snapshot.paramMap.get('id'));
      this.songService.getSong(id, true).then(song => {
        this.song = song;
        this.titleService.setTitle(`Edit song: ${song.title}`);
        this.oldSongTitle = song.title;
      }).catch((error) => {
        console.error('Could not fetch song', error);
      });

      // Get mediumtypes
      this.mediumTypeService.getMediumTypes(false).then((mediumTypes) => {
        this.mediumTypes = mediumTypes;
      }).catch((error) => {
        console.error('Could not fetch medium types', error);
      });
    }
  }

  ngOnInit() {
    this.htmlLink.setCanonicalWithoutQuery();
  }

  ngOnDestroy() {
    this.htmlLink.unset('canonical');
  }

  mediumTypes: MediumType[] = [];
  oldSongTitle: string = '';
  public song: Song = {
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

  public httpHeaders: HttpHeaders = new HttpHeaders({
    'include_relations': String(true)
  });

  removeBrackets() {
    var rgx = /\[(.*?)\]\n/gm;
    this.song.lyrics.text = this.song.lyrics.text.replace(rgx, '');
  }

  public updateSong() {
    this.songService.updateSong(this.song).then((song) => {
      this.router.navigate(['song', this.song.id, this.slugifyHelper.slugify(song.title)]);
    }).catch((error) => {
      console.error('Could not update song', error);
    });
  }

  @HostListener('window:beforeunload', ['$event'])
  beforeUnload($event: BeforeUnloadEvent) {
    $event.returnValue = '';
    let result = confirm("There are unsaved changes. Are you sure you want to quit?");

    if (!result) {
      $event.preventDefault();
    }
  }
}
