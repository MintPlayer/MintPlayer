import { Component, OnInit, Inject, OnDestroy } from '@angular/core';
import { MediumTypeService } from '../../../services/medium-type/medium-type.service';
import { Router, ActivatedRoute } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { MediumType } from '../../../entities/medium-type';
import { ePlayerType } from '../../../enums/ePlayerType';
import { HtmlLinkHelper } from '../../../helpers/html-link.helper';
import { SlugifyPipe } from '../../../pipes/slugify/slugify.pipe';

@Component({
  selector: 'app-show',
  templateUrl: './show.component.html',
  styleUrls: ['./show.component.scss']
})
export class ShowComponent implements OnInit, OnDestroy {
  constructor(@Inject('SERVERSIDE') serverSide: boolean, @Inject('MEDIUMTYPE') private mediumTypeInj: MediumType, private mediumTypeService: MediumTypeService, private router: Router, private route: ActivatedRoute, private titleService: Title, private htmlLink: HtmlLinkHelper) {
    if (serverSide === true) {
      this.setMediumType(mediumTypeInj);
    } else {
      var id = parseInt(this.route.snapshot.paramMap.get('id'));
      this.loadMediumType(id);
    }
  }

  private loadMediumType(id: number) {
    this.mediumTypeService.getMediumType(id, true).then((mediumtype) => {
      this.setMediumType(mediumtype);
    }).catch((error) => {
      console.error('Could not fetch medium type', error);
    });
  }

  private setMediumType(mediumType: MediumType) {
    this.mediumType = mediumType;
    this.titleService.setTitle(`Medium type: ${mediumType.description}`);
  }

  public deleteMediumType() {
    this.mediumTypeService.deleteMediumType(this.mediumType).then(() => {
      this.router.navigate(['mediumtype']);
    }).catch((error) => {
      console.error('Could not delete medium type', error);
    });
  }

  public playerTypeEnum = ePlayerType;
  public mediumType: MediumType = {
    id: 0,
    description: '',
    playerType: ePlayerType.None
  };

  ngOnInit() {
    this.htmlLink.setCanonicalWithoutQuery();
  }

  ngOnDestroy() {
    this.htmlLink.unset('canonical');
  }
}
