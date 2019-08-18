import { Component, OnInit, Inject } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { MediumType } from '../../../interfaces/medium-type';
import { ePlayerType } from "../../../enums/ePlayerType";
import { MediumTypeService } from '../../../services/medium-type/medium-type.service';

@Component({
  selector: 'app-show',
  templateUrl: './show.component.html',
  styleUrls: ['./show.component.scss']
})
export class ShowComponent implements OnInit {
  constructor(private mediumTypeService: MediumTypeService, @Inject('MEDIUMTYPE') private mediumTypeInj: MediumType, private router: Router, private route: ActivatedRoute, private titleService: Title) {
    var id = parseInt(this.route.snapshot.paramMap.get("id"));
    if (mediumTypeInj === null) {
      this.mediumTypeService.getMediumType(id, true).subscribe(mediumtype => {
        this.setMediumType(mediumtype);
      });
    } else {
      this.setMediumType(mediumTypeInj);
    }
  }

  private setMediumType(mediumtype: MediumType) {
    this.mediumType = mediumtype;
    if (mediumtype !== null) {
      this.titleService.setTitle(`Medium type: ${mediumtype.description}`);
    }
  }

  public deleteMediumType() {
    this.mediumTypeService.deleteMediumType(this.mediumType).subscribe(() => {
      this.router.navigate(["mediumtype"]);
    });
  }

  public playerTypeEnum = ePlayerType;
  public mediumType: MediumType = {
    id: 0,
    description: "",
    playerType: ePlayerType.None
  };

  ngOnInit() {
  }
}
