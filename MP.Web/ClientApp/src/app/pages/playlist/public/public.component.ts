import { Component, Inject, OnInit } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute } from '@angular/router';
import { DatatableSettings } from '../../../controls/datatable/datatable-settings';
import { Playlist } from '../../../entities/playlist';
import { ePlaylistScope } from '../../../enums/ePlaylistScope';
import { NavigationHelper } from '../../../helpers/navigation.helper';
import { PaginationResponse } from '../../../helpers/pagination-response';
import { PlaylistService } from '../../../services/playlist/playlist.service';

@Component({
  selector: 'app-public',
  templateUrl: './public.component.html',
  styleUrls: ['./public.component.scss']
})
export class PlaylistPublicComponent implements OnInit {

  constructor(
    @Inject('SERVERSIDE') serverSide: boolean,
    @Inject('PLAYLISTS') playlistsInj: PaginationResponse<Playlist>,
    private playlistService: PlaylistService,
    private navigation: NavigationHelper,
    private route: ActivatedRoute,
    private titleService: Title
  ) {
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
    this.playlistService.pagePlaylists(this.tableSettings.toPaginationRequest(), ePlaylistScope.Public).then((playlists) => {
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
