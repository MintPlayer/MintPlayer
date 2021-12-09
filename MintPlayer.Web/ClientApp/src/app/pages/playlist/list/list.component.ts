import { Component, OnInit, Inject, OnDestroy } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute } from '@angular/router';
import { AdvancedRouter } from '@mintplayer/ng-router';
import { DatatableSettings } from '@mintplayer/ng-datatables';
import { SERVER_SIDE } from '@mintplayer/ng-server-side';
import { PaginationResponse } from '@mintplayer/ng-pagination';
import { Playlist, PlaylistScope, PlaylistService } from '@mintplayer/ng-client';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

@Component({
  selector: 'app-list',
  templateUrl: './list.component.html',
  styleUrls: ['./list.component.scss']
})
export class PlaylistListComponent implements OnInit, OnDestroy {

  constructor(
    @Inject(SERVER_SIDE) serverSide: boolean,
    @Inject('PLAYLISTS') playlistsInj: PaginationResponse<Playlist>,
    private playlistService: PlaylistService,
    private router: AdvancedRouter,
    private route: ActivatedRoute,
    private titleService: Title,
  ) {
    this.titleService.setTitle('My playlists');
    if (serverSide === true) {
      if (playlistsInj !== null) {
        this.setPlaylistData(playlistsInj);
      }
    } else {
      //this.loadPlaylists();
      this.route.queryParams
        .pipe(takeUntil(this.destroyed$))
        .subscribe((queryParams) => {
          this.tableSettings.perPage.selected = parseInt(queryParams['perpage'] ?? 20);
          this.tableSettings.page.selected = parseInt(queryParams['page'] ?? 1);
          this.tableSettings.sortProperty = queryParams['sortproperty'] ?? 'Description';
          this.tableSettings.sortDirection = queryParams['sortdirection'] ?? 'ascending';

          this.playlistService.pagePlaylists(this.tableSettings.toPagination(), PlaylistScope.my).then((playlists) => {
            this.setPlaylistData(playlists);
          }).catch((error) => {
            console.error(error);
          });
        });
    }
  }

  loadPlaylists() {
    this.router.navigate([], {
      queryParams: {
        perpage: this.tableSettings.perPage.selected,
        page: this.tableSettings.page.selected,
        sortproperty: this.tableSettings.sortProperty,
        sortdirection: this.tableSettings.sortDirection,
      }
    });
  }

  private setPlaylistData(data: PaginationResponse<Playlist>) {
    this.playlistData = data;
    this.tableSettings.page.values = Array.from(Array(data.totalPages).keys()).map((p) => p + 1);
  }

  playlistData: PaginationResponse<Playlist> = new PaginationResponse();

  tableSettings: DatatableSettings = new DatatableSettings({
    perPage: {
      values: [10, 20, 50, 100],
      selected: 20
    },
    page: {
      values: [],
      selected: 1
    },
    sortProperty: 'Description',
    sortDirection: 'ascending'
  });

  ngOnInit() {
  }

  private destroyed$ = new Subject();
  ngOnDestroy() {
    this.destroyed$.next(true);
  }

}
