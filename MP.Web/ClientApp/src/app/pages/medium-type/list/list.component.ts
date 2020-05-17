import { Component, OnInit, Inject, OnDestroy } from '@angular/core';
import { MediumTypeService } from '../../../services/medium-type/medium-type.service';
import { Title } from '@angular/platform-browser';
import { MediumType } from '../../../entities/medium-type';
import { ePlayerType } from '../../../enums/ePlayerType';
import { HtmlLinkHelper } from '../../../helpers/html-link.helper';
import { SlugifyPipe } from '../../../pipes/slugify/slugify.pipe';

@Component({
  selector: 'app-list',
  templateUrl: './list.component.html',
  styleUrls: ['./list.component.scss']
})
export class ListComponent implements OnInit, OnDestroy {
  constructor(@Inject('MEDIUMTYPES') private mediumtypesInj: MediumType[], private mediumTypeService: MediumTypeService, private titleService: Title, private htmlLink: HtmlLinkHelper) {
    this.titleService.setTitle('Medium types');
    if (mediumtypesInj === null) {
      this.mediumTypeService.getMediumTypes(false).then((mediumtypes) => {
        this.mediumTypes = mediumtypes;
      }).catch((error) => {
        console.error('Could not fetch medium types', error);
      });
    } else {
      this.mediumTypes = mediumtypesInj;
    }
  }

  ngOnInit() {
    this.htmlLink.setCanonicalWithoutQuery();
  }

  ngOnDestroy() {
    this.htmlLink.unset('canonical');
  }

  public mediumTypes: MediumType[] = [];
  public playerTypes = ePlayerType;

}
