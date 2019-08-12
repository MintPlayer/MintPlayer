import { Component, OnInit } from '@angular/core';
import { MediumTypeService } from '../../../services/medium-type/medium-type.service';
import { PlayerTypeHelper } from '../../../helpers/playerTypeHelper';
import { Router, ActivatedRoute } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { MediumType } from '../../../interfaces/medium-type';
import { ePlayerType } from '../../../enums/ePlayerType';
import { PlayerType } from '../../../interfaces/playerType';

@Component({
  selector: 'app-edit',
  templateUrl: './edit.component.html',
  styleUrls: ['./edit.component.scss']
})
export class EditComponent implements OnInit {
  constructor(private mediumTypeService: MediumTypeService, private playerTypeHelper: PlayerTypeHelper, private router: Router, private route: ActivatedRoute, private titleService: Title) {
    var id = parseInt(this.route.snapshot.paramMap.get("id"));
    this.playerTypes = this.playerTypeHelper.getPlayerTypes();

    this.mediumTypeService.getMediumType(id, false).subscribe(mediumtype => {
      this.mediumType = mediumtype;
      this.titleService.setTitle(`Edit medium type: ${mediumtype.description}`);
      this.oldMediumTypeDescription = mediumtype.description;
    });
  }

  public oldMediumTypeDescription: string = "";
  public mediumType: MediumType = {
    id: 0,
    description: "",
    playerType: ePlayerType.None
  };

  public playerTypes: PlayerType[] = [];
  public playerTypeSelected(playerType: number) {
    this.mediumType.playerType = ePlayerType[ePlayerType[playerType]];
  }

  public updateMediumType() {
    this.mediumTypeService.updateMediumType(this.mediumType).subscribe(() => {
      this.router.navigate(["mediumtype", this.mediumType.id]);
    });
  }

  ngOnInit() {
  }
}
