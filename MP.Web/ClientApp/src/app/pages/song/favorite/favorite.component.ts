import { Component, OnInit, Inject, OnDestroy } from '@angular/core';
import { DatatableSettings } from '@mintplayer/ng-datatables';
import { SERVER_SIDE } from '@mintplayer/ng-server-side';
import { PaginationResponse } from '@mintplayer/ng-pagination';
import { Song, SongService } from '@mintplayer/ng-client';
import { HtmlLinkHelper } from '../../../helpers/html-link.helper';

@Component({
  selector: 'app-favorite',
  templateUrl: './favorite.component.html',
  styleUrls: ['./favorite.component.scss']
})
export class FavoriteComponent implements OnInit, OnDestroy {

  constructor(
    @Inject(SERVER_SIDE) private serverside: boolean,
    private songService: SongService,
    private htmlLink: HtmlLinkHelper
  ) {
    if (serverside === false) {
      this.loadFavoriteSongs();
    }
  }

  loadFavoriteSongs() {
    this.songService.pageFavoriteSongs(this.tableSettings.toPagination()).then((response) => {
      this.setSongData(response);
    }).catch((error) => {
      console.log(error);
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

  ngOnDestroy() {
    this.htmlLink.unset('canonical');
  }

}
