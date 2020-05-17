import { Component, OnInit, Inject, OnDestroy } from '@angular/core';
import { SongService } from '../../../services/song/song.service';
import { HtmlLinkHelper } from '../../../helpers/html-link.helper';
import { Song } from '../../../entities/song';
import { PaginationResponse } from '../../../helpers/pagination-response';
import { DatatableSettings } from '../../../controls/datatable/datatable-settings';

@Component({
  selector: 'app-favorite',
  templateUrl: './favorite.component.html',
  styleUrls: ['./favorite.component.scss']
})
export class FavoriteComponent implements OnInit, OnDestroy {

  constructor(@Inject('SERVERSIDE') private serverside: boolean, private songService: SongService, private htmlLink: HtmlLinkHelper) {
    if (!serverside) {
      this.loadFavoriteSongs();
    }
  }

  loadFavoriteSongs() {
    this.songService.pageFavoriteSongs(this.tableSettings.toPaginationRequest()).then((response) => {
      this.setSongData(response);
    }).catch((error) => {
      console.log(error);
    });
  }

  private setSongData(data: PaginationResponse<Song>) {
    this.songData = data;
    this.tableSettings.pages.values = Array.from(Array(data.totalPages).keys()).map((p) => p + 1);
  }

  songData: PaginationResponse<Song> = new PaginationResponse();

  tableSettings: DatatableSettings = new DatatableSettings({
    columns: [{
      name: 'Title',
      data: 'title',
      title: 'Title',
      sortable: true
    }, {
      name: 'Released',
      data: 'released',
      title: 'Released',
      sortable: true
    }],
    perPages: {
      values: [10, 20, 50, 100],
      selected: 20
    },
    pages: {
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
