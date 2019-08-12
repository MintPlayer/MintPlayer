import { Component, OnInit } from '@angular/core';
import { MediumTypeService } from '../../../services/medium-type/medium-type.service';
import { Router } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { ePlayerType } from '../../../enums/ePlayerType';
import { MediumType } from '../../../interfaces/medium-type';
import { PlayerType } from '../../../interfaces/playerType';
import { PlayerTypeHelper } from '../../../helpers/playerTypeHelper';

@Component({
  selector: 'app-create',
  templateUrl: './create.component.html',
  styleUrls: ['./create.component.scss']
})
export class CreateComponent implements OnInit {

  constructor(private mediumTypeService: MediumTypeService, private playerTypeHelper: PlayerTypeHelper, private router: Router, private titleService: Title) {
    this.titleService.setTitle('Create medium type');
    this.playerTypes = this.playerTypeHelper.getPlayerTypes();
  }

  ngOnInit() {
  }

  public mediumType: MediumType = {
    id: 0,
    description: "",
    playerType: ePlayerType.None
  };

  public playerTypes: PlayerType[] = [];
  public playerTypeSelected(playerType: number) {
    this.mediumType.playerType = ePlayerType[ePlayerType[playerType]];
  }

  public saveMediumType() {
    this.mediumTypeService.createMediumType(this.mediumType).subscribe((mediumType) => {
      this.router.navigate(["mediumtype", mediumType.id]);
    });
  }
}
