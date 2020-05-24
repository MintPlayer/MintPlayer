import { Component, OnInit, Inject } from '@angular/core';
import { SlugifyPipe } from '../../../pipes/slugify/slugify.pipe';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute, Router } from '@angular/router';
import { PlaylistService } from '../../../services/playlist/playlist.service';
import { Playlist } from '../../../entities/playlist';
import { PaginationResponse } from '../../../helpers/pagination-response';
import { DatatableSettings } from '../../../controls/datatable/datatable-settings';
import { ExtendedRouter } from '../../../helpers/extended-router';

@Component({
  selector: 'app-list',
  templateUrl: './list.component.html',
  styleUrls: ['./list.component.scss']
})
export class PlaylistListComponent implements OnInit {

  constructor(@Inject('SERVERSIDE') serverSide: boolean, @Inject('PLAYLISTS') playlistsInj: PaginationResponse<Playlist>, private playlistService: PlaylistService, private router: ExtendedRouter, private route: ActivatedRoute, private titleService: Title, private slugifyPipe: SlugifyPipe) {
    this.titleService.setTitle('My playlists');
    if (serverSide === true) {
      if (playlistsInj !== null) {
        this.setPlaylistData(playlistsInj);
      }
    } else {
      this.loadPlaylists();
    }
  }

  loadPlaylists() {
    this.playlistService.pagePlaylists(this.tableSettings.toPaginationRequest()).then((playlists) => {
      this.setPlaylistData(playlists);
    }).catch((error) => {
      console.log(error);
    });
  }

  private setPlaylistData(data: PaginationResponse<Playlist>) {
    this.playlistData = data;
    this.tableSettings.pages.values = Array.from(Array(data.totalPages).keys()).map((p) => p + 1);
  }

  playlistData: PaginationResponse<Playlist> = new PaginationResponse();

  tableSettings: DatatableSettings = new DatatableSettings({
    columns: [{
      name: 'Description',
      data: 'description',
      title: 'Description',
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
    sortProperty: 'Description',
    sortDirection: 'ascending'
  });

  ngOnInit() {
  }

}
