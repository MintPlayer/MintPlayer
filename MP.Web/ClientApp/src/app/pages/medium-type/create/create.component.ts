import { Component, OnInit, OnDestroy, HostListener, DoCheck, KeyValueDiffers, KeyValueDiffer } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { AdvancedRouter } from '@mintplayer/ng-router';
import { MediumType, MediumTypeService, PlayerType } from '@mintplayer/ng-client';
import { HtmlLinkHelper } from '../../../helpers/html-link.helper';
import { SlugifyHelper } from '../../../helpers/slugify.helper';
import { HasChanges } from '../../../interfaces/has-changes';
import { IBeforeUnloadEvent } from '../../../events/my-before-unload.event';
import { EnumHelper } from '../../../helpers/enum.helper';
import { EnumItem } from '../../../entities/enum-item';

@Component({
  selector: 'app-create',
  templateUrl: './create.component.html',
  styleUrls: ['./create.component.scss']
})
export class CreateComponent implements OnInit, OnDestroy, DoCheck, HasChanges {
  constructor(
    private mediumTypeService: MediumTypeService,
    private router: AdvancedRouter,
    private titleService: Title,
    private htmlLink: HtmlLinkHelper,
    private slugifyHelper: SlugifyHelper,
    private differs: KeyValueDiffers,
  ) {
    this.titleService.setTitle('Create medium type');
  }

  public mediumType: MediumType = {
    id: 0,
    description: ''
  };

  public saveMediumType() {
    this.mediumTypeService.createMediumType(this.mediumType).then((mediumType) => {
      this.hasChanges = false;
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
