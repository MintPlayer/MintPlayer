import { Component, OnInit, Inject, OnDestroy, HostListener, DoCheck, KeyValueDiffers, KeyValueDiffer } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { HttpHeaders } from '@angular/common/http';
import { Title } from '@angular/platform-browser';
import { AdvancedRouter } from '@mintplayer/ng-router';
import { SERVER_SIDE } from '@mintplayer/ng-server-side';
import { MINTPLAYER_API_VERSION, Artist, MediumType, MediumTypeService, Song, SongService, SubjectService, ESubjectType, Tag, TagService } from '@mintplayer/ng-client';
import { HtmlLinkHelper } from '../../../helpers/html-link.helper';
import { SlugifyHelper } from '../../../helpers/slugify.helper';
import { HasChanges } from '../../../interfaces/has-changes';
import { IBeforeUnloadEvent } from '../../../events/my-before-unload.event';

@Component({
  selector: 'app-create',
  templateUrl: './create.component.html',
  styleUrls: ['./create.component.scss']
})
export class CreateComponent implements OnInit, OnDestroy, DoCheck, HasChanges {

  constructor(
    @Inject(SERVER_SIDE) private serverSide: boolean,
    @Inject(MINTPLAYER_API_VERSION) apiVersion: string,
    @Inject('MEDIUMTYPES') private mediumTypesInj: MediumType[],
    private songService: SongService,
    private subjectService: SubjectService,
    private mediumTypeService: MediumTypeService,
    private tagService: TagService,
    private router: AdvancedRouter,
    private route: ActivatedRoute,
    private titleService: Title,
    private htmlLink: HtmlLinkHelper,
    private slugifyHelper: SlugifyHelper,
    private differs: KeyValueDiffers,
  ) {
    this.apiVersion = apiVersion;
    this.titleService.setTitle('Add new song');
    if (serverSide === false) {
      this.mediumTypeService.getMediumTypes(false).subscribe({
        next: (mediumTypes) => {
          this.mediumTypes = mediumTypes;
        }, error: (error) => {
          console.error('Could not fetch medium types', error);
        }
      });
    } else {
      this.mediumTypes = mediumTypesInj;
    }
  }

  updateReleaseDate(date: Date) {
    this.song.released = new Date(date);
  }

  //fetchUrl: string = '';
  //fetchDialogVisible: string = 'out';
  apiVersion: string = '';
  mediumTypes: MediumType[] = [];
  oldSongTitle: string = '';
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

  artistSuggestions: Artist[] = [];
  onProvideArtistSuggestions(searchText: string) {
    this.subjectService.suggest(searchText, [ESubjectType.artist]).subscribe({
      next: (artists) => {
        this.artistSuggestions = <Artist[]>artists;
      }
    });
  }
  uncreditedArtistSuggestions: Artist[] = [];
  onProvideUncreditedArtistSuggestions(searchText: string) {
    this.subjectService.suggest(searchText, [ESubjectType.artist]).subscribe({
      next: (artists) => {
        this.uncreditedArtistSuggestions = <Artist[]>artists;
      }
    });
  }
  tagSuggestions: Tag[] = [];
  onProvideTagSuggestions(searchText: string) {
    this.tagService.suggestTags(searchText, true).subscribe({
      next: (tags) => {
        this.tagSuggestions = tags;
      }
    });
  }

  removeBrackets() {
    var rgx = /\[(.*?)\]\n/gm;
    this.song.lyrics.text = this.song.lyrics.text.replace(rgx, '');
  }

  saveSong() {
    this.songService.createSong(this.song).subscribe({
      next: (song) => {
        this.hasChanges = false;
        this.router.navigate(['song', song.id, this.slugifyHelper.slugify(song.title)]);
      }, error: (error) => {
        console.error('Could not create song', error);
      }
    });
  }

  //#region Prevent loss of changes
  hasChanges: boolean = false;
  private songDiffer: KeyValueDiffer<string, any> = null;
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
    if (this.songDiffer !== null) {
      const changes = this.songDiffer.diff(this.song);
      if (changes) {
        this.hasChanges = true;
      }
    }
  }
  //#endregion

  ngOnInit() {
    this.htmlLink.setCanonicalWithoutQuery();
    this.songDiffer = this.differs.find(this.song).create();
    setTimeout(() => this.hasChanges = false);
  }

  ngOnDestroy() {
    this.htmlLink.unset('canonical');
  }
}
