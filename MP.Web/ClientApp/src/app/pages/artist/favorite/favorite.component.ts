import { Component, OnInit, Inject, OnDestroy } from '@angular/core';
import { DatatableSettings } from '@mintplayer/ng-datatables';
import { ArtistService } from '../../../services/artist/artist.service';
import { PaginationResponse } from '../../../helpers/pagination-response';
import { Artist } from '../../../entities/artist';
import { HtmlLinkHelper } from '../../../helpers/html-link.helper';

@Component({
  selector: 'app-favorite',
  templateUrl: './favorite.component.html',
  styleUrls: ['./favorite.component.scss']
})
export class FavoriteComponent implements OnInit, OnDestroy {

  constructor(
    @Inject('SERVERSIDE') private serverside: boolean,
    private artistService: ArtistService,
    private htmlLink: HtmlLinkHelper
  ) {
    if (serverside === false) {
      this.loadFavoriteArtists();
    }
  }

  loadFavoriteArtists() {
    this.artistService.pageFavoriteArtists(this.tableSettings.toPagination()).then((response) => {
      this.setArtistData(response);
    }).catch((error) => {
      console.log(error);
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

  ngOnInit() {
    this.htmlLink.setCanonicalWithoutQuery();
  }

  ngOnDestroy() {
    this.htmlLink.unset('canonical');
  }

}
