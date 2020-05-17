import { Component, OnInit, Inject, OnDestroy } from '@angular/core';
import { Router, UrlSerializer } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { ArtistService } from '../../../services/artist/artist.service';
import { Artist } from '../../../entities/artist';
import { PaginationResponse } from '../../../helpers/pagination-response';
import { DatatableSettings } from '../../../controls/datatable/datatable-settings';
import { HtmlLinkHelper } from '../../../helpers/html-link.helper';

@Component({
  selector: 'app-list',
  templateUrl: './list.component.html',
  styleUrls: ['./list.component.scss']
})
export class ListComponent implements OnInit, OnDestroy {

  constructor(@Inject('ARTISTS') private artistsInj: PaginationResponse<Artist>, private artistService: ArtistService, private router: Router, private urlSerializer: UrlSerializer, private titleService: Title, private htmlLink: HtmlLinkHelper) {
    this.titleService.setTitle('Artists');
    if (artistsInj === null) {
      this.loadArtists();
    } else {
      this.setArtistData(artistsInj);
    }
  }

  loadArtists() {
    this.artistService.pageArtists(this.tableSettings.toPaginationRequest()).then((response) => {
      this.setArtistData(response);
    }).catch((error) => {
      console.log(error);
    });
  }

  private setArtistData(data: PaginationResponse<Artist>) {
    this.artistData = data;
    this.tableSettings.pages.values = Array.from(Array(data.totalPages).keys()).map((p) => p + 1);
  }

  artistData: PaginationResponse<Artist> = new PaginationResponse();

  tableSettings: DatatableSettings = new DatatableSettings({
    columns: [{
      name: 'Name',
      data: 'name',
      title: 'Name',
      sortable: true
    }, {
      name: 'YearStarted',
      data: 'yearStarted',
      title: 'Year started',
      sortable: true
    }, {
      name: 'YearQuit',
      data: 'yearQuit',
      title: 'Year quit',
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

