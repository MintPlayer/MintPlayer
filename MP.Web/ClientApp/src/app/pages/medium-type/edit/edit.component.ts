import { Component, OnInit, Inject, OnDestroy, HostListener, DoCheck, KeyValueDiffers, KeyValueDiffer } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { AdvancedRouter } from '@mintplayer/ng-router';
import { SERVER_SIDE } from '@mintplayer/ng-server-side';
import { MediumType, MediumTypeService, PlayerType } from '@mintplayer/ng-client';
import { HtmlLinkHelper } from '../../../helpers/html-link.helper';
import { SlugifyHelper } from '../../../helpers/slugify.helper';
import { HasChanges } from '../../../interfaces/has-changes';
import { IBeforeUnloadEvent } from '../../../events/my-before-unload.event';
import { EnumHelper } from '../../../helpers/enum.helper';
import { EnumItem } from '../../../entities/enum-item';

@Component({
  selector: 'app-edit',
  templateUrl: './edit.component.html',
  styleUrls: ['./edit.component.scss']
})
export class EditComponent implements OnInit, OnDestroy, DoCheck, HasChanges {
  constructor(
    @Inject(SERVER_SIDE) private serverSide: boolean,
    @Inject('MEDIUMTYPE') private mediumTypeInj: MediumType,
    private mediumTypeService: MediumTypeService,
    private router: AdvancedRouter,
    private route: ActivatedRoute,
    private titleService: Title,
    private htmlLink: HtmlLinkHelper,
    private slugifyHelper: SlugifyHelper,
    private differs: KeyValueDiffers,
  ) {
    if (serverSide) {
      this.setMediumType(mediumTypeInj);
    } else {
      var id = parseInt(this.route.snapshot.paramMap.get('id'));
      this.loadMediumType(id);
    }
  }

  private loadMediumType(id: number) {
    this.mediumTypeService.getMediumType(id, false).then((mediumType) => {
      this.setMediumType(mediumType);
    }).catch((error) => {
      console.error('Could not fetch medium type', error);
    });
  }

  private setMediumType(mediumType: MediumType) {
    this.mediumType = mediumType;
    if (mediumType !== null) {
      this.titleService.setTitle(`Edit medium type: ${mediumType.description}`);
      this.oldMediumTypeDescription = mediumType.description;
    }
    this.MediumTypeDiffer = this.differs.find(this.mediumType).create();
    setTimeout(() => this.hasChanges = false);
  }

  oldMediumTypeDescription: string = '';
  mediumType: MediumType = {
    id: 0,
    description: ''
  };

  public updateMediumType() {
    this.mediumTypeService.updateMediumType(this.mediumType).then((mediumType) => {
      this.hasChanges = false;
      this.router.navigate(['mediumtype', this.mediumType.id, this.slugifyHelper.slugify(mediumType.description)]);
    }).catch((error) => {
      console.error('Could not update medium type', error);
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
  }

  ngOnDestroy() {
    this.htmlLink.unset('canonical');
  }
}
