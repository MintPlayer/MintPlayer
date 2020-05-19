import { Component, OnInit, OnDestroy, HostListener } from '@angular/core';
import { MediumTypeService } from '../../../services/medium-type/medium-type.service';
import { Router } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { ePlayerType } from '../../../enums/ePlayerType';
import { MediumType } from '../../../entities/medium-type';
import { PlayerType } from '../../../entities/player-type';
import { PlayerTypeHelper } from '../../../helpers/player-type.helper';
import { HtmlLinkHelper } from '../../../helpers/html-link.helper';
import { SlugifyHelper } from '../../../helpers/slugify.helper';

@Component({
  selector: 'app-create',
  templateUrl: './create.component.html',
  styleUrls: ['./create.component.scss']
})
export class CreateComponent implements OnInit, OnDestroy {
  constructor(private mediumTypeService: MediumTypeService, private playerTypeHelper: PlayerTypeHelper, private router: Router, private titleService: Title, private htmlLink: HtmlLinkHelper, private slugifyHelper: SlugifyHelper) {
    this.titleService.setTitle('Create medium type');
    this.playerTypes = this.playerTypeHelper.getPlayerTypes();
  }

  ngOnInit() {
    this.htmlLink.setCanonicalWithoutQuery();
  }

  ngOnDestroy() {
    this.htmlLink.unset('canonical');
  }

  public mediumType: MediumType = {
    id: 0,
    description: '',
    playerType: ePlayerType.None
  };

  public playerTypes: PlayerType[] = [];
  public playerTypeSelected(playerType: number) {
    this.mediumType.playerType = ePlayerType[ePlayerType[playerType]];
  }

  public saveMediumType() {
    this.mediumTypeService.createMediumType(this.mediumType).then((mediumType) => {
      this.router.navigate(['mediumtype', mediumType.id, this.slugifyHelper.slugify(mediumType.description)]);
    }).catch((error) => {
      console.error('Could not create medium type', error);
    });
  }

  @HostListener('window:beforeunload', ['$event'])
  beforeUnload($event: BeforeUnloadEvent) {
    $event.returnValue = '';
    let result = confirm("There are unsaved changes. Are you sure you want to quit?");

    if (!result) {
      $event.preventDefault();
    }
  }
}
