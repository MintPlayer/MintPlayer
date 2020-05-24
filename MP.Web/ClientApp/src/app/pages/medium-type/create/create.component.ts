import { Component, OnInit, OnDestroy, HostListener, DoCheck, KeyValueDiffers, KeyValueDiffer } from '@angular/core';
import { MediumTypeService } from '../../../services/medium-type/medium-type.service';
import { Router } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { ePlayerType } from '../../../enums/ePlayerType';
import { MediumType } from '../../../entities/medium-type';
import { PlayerType } from '../../../entities/player-type';
import { PlayerTypeHelper } from '../../../helpers/player-type.helper';
import { HtmlLinkHelper } from '../../../helpers/html-link.helper';
import { SlugifyHelper } from '../../../helpers/slugify.helper';
import { HasChanges } from '../../../interfaces/has-changes';
import { IBeforeUnloadEvent } from '../../../events/my-before-unload.event';
import { ExtendedRouter } from '../../../helpers/extended-router';

@Component({
  selector: 'app-create',
  templateUrl: './create.component.html',
  styleUrls: ['./create.component.scss']
})
export class CreateComponent implements OnInit, OnDestroy, DoCheck, HasChanges {
  constructor(private mediumTypeService: MediumTypeService, private playerTypeHelper: PlayerTypeHelper, private router: ExtendedRouter, private titleService: Title, private htmlLink: HtmlLinkHelper, private slugifyHelper: SlugifyHelper, private differs: KeyValueDiffers) {
    this.titleService.setTitle('Create medium type');
    this.playerTypes = this.playerTypeHelper.getPlayerTypes();
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

  //#region Prevent loss of changes
  hasChanges: boolean = false;
  private MediumTypeDiffer: KeyValueDiffer<string, any> = null;
  @HostListener('window:beforeunload', ['$event'])
  beforeUnload($event: IBeforeUnloadEvent) {
    if (this.hasChanges) {
      $event.returnValue = '';
      if (!confirm("There are unsaved changes. Are you sure you want to quit?")) {
        $event.preventDefault();
      }
    }
  }

  ngDoCheck() {
    if (this.MediumTypeDiffer !== null) {
      const changes = this.MediumTypeDiffer.diff(this.mediumType);
      if (changes) {
        this.hasChanges = true;
      }
    }
  }
  //#endregion

  ngOnInit() {
    this.htmlLink.setCanonicalWithoutQuery();
    this.MediumTypeDiffer = this.differs.find(this.mediumType).create();
    setTimeout(() => this.hasChanges = false);
  }

  ngOnDestroy() {
    this.htmlLink.unset('canonical');
  }
}
