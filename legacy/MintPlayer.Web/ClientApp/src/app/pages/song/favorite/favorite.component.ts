import { Component, OnInit, Inject, OnDestroy } from '@angular/core';
import { DatatableSettings } from '@mintplayer/ng-datatables';
import { SERVER_SIDE } from '@mintplayer/ng-server-side';
import { PaginationResponse } from '@mintplayer/ng-pagination';
import { Song, SongService } from '@mintplayer/ng-client';
import { HtmlLinkHelper } from '../../../helpers/html-link.helper';
import { ActivatedRoute } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { AdvancedRouter } from '@mintplayer/ng-router';

@Component({
  selector: 'app-favorite',
  templateUrl: './favorite.component.html',
  styleUrls: ['./favorite.component.scss']
})
export class FavoriteComponent implements OnInit, OnDestroy {

  constructor(
    @Inject(SERVER_SIDE) private serverside: boolean,
    private songService: SongService,
    private htmlLink: HtmlLinkHelper,
    private router: AdvancedRouter,
    private route: ActivatedRoute,
  ) {
    if (serverside === true) {
    } else {
      //this.loadFavoriteSongs();
      this.route.queryParams
        .pipe(takeUntil(this.destroyed$))
        .subscribe((queryParams) => {
          this.tableSettings.perPage.selected = parseInt(queryParams['perpage'] ?? 20);
          this.tableSettings.page.selected = parseInt(queryParams['page'] ?? 1);
          this.tableSettings.sortProperty = queryParams['sortproperty'] ?? 'Title';
          this.tableSettings.sortDirection = queryParams['sortdirection'] ?? 'ascending';

          this.songService.pageFavoriteSongs(this.tableSettings.toPagination()).subscribe({
            next: (response) => {
              this.setSongData(response);
            }, error: (error) => {
              console.error(error);
            }
          });
        });
    }
  }

  loadFavoriteSongs() {
    this.router.navigate([], {
      queryParams: {
        perpage: this.tableSettings.perPage.selected,
        page: this.tableSettings.page.selected,
        sortproperty: this.tableSettings.sortProperty,
        sortdirection: this.tableSettings.sortDirection,
      }
    });
  }

  private setSongData(data: PaginationResponse<Song>) {
    this.songData = data;
    this.tableSettings.page.values = Array.from(Array(data.totalPages).keys()).map((p) => p + 1);
  }

  songData: PaginationResponse<Song> = new PaginationResponse();

  tableSettings: DatatableSettings = new DatatableSettings({
    perPage: {
      values: [10, 20, 50, 100],
      selected: 20
    },
    page: {
      values: [],
      selected: 1
    },
    sortProperty: 'Title',
    sortDirection: 'ascending'
  });

  ngOnInit() {
    this.htmlLink.setCanonicalWithoutQuery();
  }

  private destroyed$ = new Subject();
  ngOnDestroy() {
    this.htmlLink.unset('canonical');
    this.destroyed$.next(true);
  }

}
