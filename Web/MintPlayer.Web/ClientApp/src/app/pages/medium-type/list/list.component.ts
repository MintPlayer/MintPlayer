import { Component, OnInit, Inject } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { MediumType } from '../../../interfaces/medium-type';
import { ePlayerType } from "../../../enums/ePlayerType";
import { MediumTypeService } from '../../../services/medium-type/medium-type.service';

@Component({
  selector: 'app-list',
  templateUrl: './list.component.html',
  styleUrls: ['./list.component.scss']
})
export class ListComponent implements OnInit {
  constructor(private mediumTypeService: MediumTypeService, @Inject('MEDIUMTYPES') private mediumTypesInj: MediumType[], private titleService: Title) {
    this.titleService.setTitle('Medium types');
    if (mediumTypesInj === null) {
      this.loadMediumTypes();
    } else {
      this.mediumTypes = mediumTypesInj;
    }
  }

  private loadMediumTypes() {
    this.mediumTypeService.getMediumTypes(false).subscribe(mediumtypes => {
      this.mediumTypes = mediumtypes;
    });
  }

  ngOnInit() {
  }

  public mediumTypes: MediumType[] = [];
  public playerTypes = ePlayerType;
}
