import { Component, OnInit, Inject, OnDestroy } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { MediumTypeService } from '../../../services/medium-type/medium-type.service';
import { PlayerTypeHelper } from '../../../helpers/player-type.helper';
import { MediumType } from '../../../entities/medium-type';
import { PlayerType } from '../../../entities/player-type';
import { ePlayerType } from '../../../enums/ePlayerType';
import { HtmlLinkHelper } from '../../../helpers/html-link.helper';
import { SlugifyHelper } from '../../../helpers/slugify.helper';

@Component({
  selector: 'app-edit',
  templateUrl: './edit.component.html',
  styleUrls: ['./edit.component.scss']
})
export class EditComponent implements OnInit, OnDestroy {
  constructor(@Inject('MEDIUMTYPE') private mediumTypeInj: MediumType, private mediumTypeService: MediumTypeService, private playerTypeHelper: PlayerTypeHelper, private router: Router, private route: ActivatedRoute, private titleService: Title, private htmlLink: HtmlLinkHelper, private slugifyHelper: SlugifyHelper) {
    this.playerTypes = this.playerTypeHelper.getPlayerTypes();

    if (mediumTypeInj === null) {
      var id = parseInt(this.route.snapshot.paramMap.get('id'));
      this.mediumTypeService.getMediumType(id, false).then((mediumtype) => {
        this.mediumType = mediumtype;
        this.titleService.setTitle(`Edit medium type: ${mediumtype.description}`);
        this.oldMediumTypeDescription = mediumtype.description;
      }).catch((error) => {
        console.error('Could not fetch medium type', error);
      });
    } else {
      this.mediumType = mediumTypeInj;
      this.titleService.setTitle(`Edit medium type: ${mediumTypeInj.description}`);
      this.oldMediumTypeDescription = mediumTypeInj.description;
    }
  }

  public oldMediumTypeDescription: string = '';
  public mediumType: MediumType = {
    id: 0,
    description: '',
    playerType: ePlayerType.None
  };

  public playerTypes: PlayerType[] = [];
  public playerTypeSelected(playerType: number) {
    this.mediumType.playerType = ePlayerType[ePlayerType[playerType]];
  }

  public updateMediumType() {
    this.mediumTypeService.updateMediumType(this.mediumType).then((mediumType) => {
      this.router.navigate(['mediumtype', this.mediumType.id, this.slugifyHelper.slugify(mediumType.description)]);
    }).catch((error) => {
      console.error('Could not update medium type', error);
    });
  }

  ngOnInit() {
    this.htmlLink.setCanonicalWithoutQuery();
  }

  ngOnDestroy() {
    this.htmlLink.unset('canonical');
  }
}
