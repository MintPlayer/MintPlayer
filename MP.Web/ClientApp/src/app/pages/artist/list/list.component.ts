import { Component, OnInit, Inject, OnDestroy } from '@angular/core';
import { ActivatedRoute, UrlSerializer } from '@angular/router';
import { Title, Meta } from '@angular/platform-browser';
import { AdvancedRouter } from '@mintplayer/ng-router';
import { DatatableSettings } from '@mintplayer/ng-datatables';
import { SERVER_SIDE } from '@mintplayer/ng-server-side';
import { PaginationResponse } from '@mintplayer/ng-pagination';
import { Artist, ArtistService } from '@mintplayer/ng-client';
import { HtmlLinkHelper } from '../../../helpers/html-link.helper';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

@Component({
  selector: 'app-list',
  templateUrl: './list.component.html',
  styleUrls: ['./list.component.scss']
})
export class ListComponent implements OnInit, OnDestroy {

  constructor(
    @Inject(SERVER_SIDE) private serverSide: boolean,
    @Inject('ARTISTS') private artistsInj: PaginationResponse<Artist>,
    private artistService: ArtistService,
    private router: AdvancedRouter,
    private route: ActivatedRoute,
    private urlSerializer: UrlSerializer,
    private titleService: Title,
    private htmlLink: HtmlLinkHelper,
    private metaService: Meta
  ) {
    this.titleService.setTitle('Artists');
    if (serverSide === true) {
      this.setArtistData(artistsInj);
    } else {
      this.loadArtists();
    }

    this.route.queryParams
      .pipe(takeUntil(this.destroyed$))
      .subscribe((queryParams) => {
        this.tableSettings.perPage.selected = parseInt(queryParams['perpage'] ?? 20);
        this.tableSettings.page.selected = parseInt(queryParams['page'] ?? 1);
        this.tableSettings.sortProperty = queryParams['sortproperty'] ?? 'name';
        this.tableSettings.sortDirection = queryParams['sortdirection'] ?? 'ascending';

        this.artistService.pageArtists(this.tableSettings.toPagination()).then((response) => {
          this.setArtistData(response);
        }).catch((error) => {
          console.error(error);
        });
      })
  }

  ngOnInit() {
    this.htmlLink.setCanonicalWithoutQuery();
    this.addMetaTags();
  }

  private destroyed$ = new Subject();
  ngOnDestroy() {
    this.htmlLink.unset('canonical');
    this.removeMetaTags();
    this.destroyed$.next(true);
  }

  //#region Add meta-tags
  private basicMetaTags: HTMLMetaElement[] = [];
  private ogMetaTags: HTMLMetaElement[] = [];
  private twitterMetaTags: HTMLMetaElement[] = [];
  private addMetaTags() {
    this.addBasicMetaTags();
    this.addOpenGraphTags();
    this.addTwitterCard();
  }
  private addBasicMetaTags() {
    this.basicMetaTags = this.metaService.addTags([{
      name: 'description',
      content: 'Here you can find a list of all the artists in our database.'
    }]);
  }
  private addOpenGraphTags() {

  }
  private addTwitterCard() {

  }
  private removeMetaTags() {
    if (this.ogMetaTags !== null) {
      this.ogMetaTags.forEach((tag) => {
        this.metaService.removeTagElement(tag);
      });
    }
    if (this.basicMetaTags !== null) {
      this.basicMetaTags.forEach((tag) => {
        this.metaService.removeTagElement(tag);
      });
    }
    if (this.twitterMetaTags !== null) {
      this.twitterMetaTags.forEach((tag) => {
        this.metaService.removeTagElement(tag);
      });
    }
  }
  //#endregion

  loadArtists() {
    this.router.navigate([], {
      queryParams: {
        perpage: this.tableSettings.perPage.selected,
        page: this.tableSettings.page.selected,
        sortproperty: this.tableSettings.sortProperty,
        sortdirection: this.tableSettings.sortDirection,
      }
    });
  }

  private setArtistData(data: PaginationResponse<Artist>) {
    this.artistData = data;
    this.tableSettings.page.values = Array.from(Array(data.totalPages).keys()).map((p) => p + 1);
  }

  artistData: PaginationResponse<Artist> = new PaginationResponse();

  tableSettings: DatatableSettings = new DatatableSettings({
    perPage: {
      values: [10, 20, 50, 100],
      selected: 20
    },
    page: {
      values: [],
      selected: 1
    },
    sortProperty: 'Name',
    sortDirection: 'ascending'
  });

}

