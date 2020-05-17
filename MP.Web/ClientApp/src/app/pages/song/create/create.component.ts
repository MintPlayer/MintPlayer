import { Component, OnInit, Inject, OnDestroy } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { HttpHeaders } from '@angular/common/http';
import { Title } from '@angular/platform-browser';
import { SongService } from '../../../services/song/song.service';
import { Song } from '../../../entities/song';
import { Artist } from '../../../entities/artist';
import { MediumTypeService } from '../../../services/medium-type/medium-type.service';
import { MediumType } from '../../../entities/medium-type';
import { Tag } from '../../../entities/tag';
import { HtmlLinkHelper } from '../../../helpers/html-link.helper';
import { SlugifyHelper } from '../../../helpers/slugify.helper';

@Component({
  selector: 'app-create',
  templateUrl: './create.component.html',
  styleUrls: ['./create.component.scss']
})
export class CreateComponent implements OnInit, OnDestroy {

  constructor(@Inject('MEDIUMTYPES') private mediumTypesInj: MediumType[], private songService: SongService, private mediumTypeService: MediumTypeService, private router: Router, private route: ActivatedRoute, private titleService: Title, private htmlLink: HtmlLinkHelper, private slugifyHelper: SlugifyHelper) {
    this.titleService.setTitle('Add new song');
    if (mediumTypesInj === null) {
      this.mediumTypeService.getMediumTypes(false).then((mediumTypes) => {
        this.mediumTypes = mediumTypes;
      }).catch((error) => {
        console.error('Could not fetch medium types', error);
      });
    } else {
      this.mediumTypes = mediumTypesInj;
    }
  }

  printItem(item: any) {
    console.log(item);
  }

  ngOnInit() {
    this.htmlLink.setCanonicalWithoutQuery();
  }

  ngOnDestroy() {
    this.htmlLink.unset('canonical');
  }

  updateReleaseDate(date: Date) {
    this.song.released = new Date(date);
  }

  //fetchUrl: string = '';
  //fetchDialogVisible: string = 'out';
  mediumTypes: MediumType[] = [];
  oldSongTitle: string = '';
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

  httpHeaders: HttpHeaders = new HttpHeaders({
    'include_relations': String(true)
  });

  removeBrackets() {
    var rgx = /\[(.*?)\]\n/gm;
    this.song.lyrics.text = this.song.lyrics.text.replace(rgx, '');
  }

  saveSong() {
    this.songService.createSong(this.song).then((song) => {
      this.router.navigate(['song', song.id, this.slugifyHelper.slugify(song.title)]);
    }).catch((error) => {
      console.error('Could not create song', error);
    });
  }
}
