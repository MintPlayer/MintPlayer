import { Component, OnInit, Inject } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute } from '@angular/router';
import { AdvancedRouter } from '@mintplayer/ng-router';
import { DatatableSettings } from '@mintplayer/ng-datatables';
import { PlaylistService } from '../../../services/playlist/playlist.service';
import { Playlist } from '../../../entities/playlist';
import { PaginationResponse } from '../../../helpers/pagination-response';
import { ePlaylistScope } from '../../../enums/ePlaylistScope';

@Component({
  selector: 'app-list',
  templateUrl: './list.component.html',
  styleUrls: ['./list.component.scss']
})
export class PlaylistListComponent implements OnInit {

  constructor(
    @Inject('SERVERSIDE') serverSide: boolean,
    @Inject('PLAYLISTS') playlistsInj: PaginationResponse<Playlist>,
    private playlistService: PlaylistService,
    private router: AdvancedRouter,
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
    this.playlistService.pagePlaylists(this.tableSettings.toPagination(), ePlaylistScope.My).then((playlists) => {
      this.setPlaylistData(playlists);
    }).catch((error) => {
      console.log(error);
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

}
