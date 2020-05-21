import { Component, OnInit, OnDestroy, Inject, HostListener, KeyValueDiffers, KeyValueDiffer, DoCheck } from '@angular/core';
import { TagService } from '../../../../services/tag/tag.service';
import { Router, ActivatedRoute } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { Tag } from '../../../../entities/tag';
import { HtmlLinkHelper } from '../../../../helpers/html-link.helper';
import { HasChanges } from '../../../../interfaces/has-changes';
import { IBeforeUnloadEvent } from '../../../../events/my-before-unload.event';

@Component({
  selector: 'app-edit',
  templateUrl: './edit.component.html',
  styleUrls: ['./edit.component.scss']
})
export class EditComponent implements OnInit, OnDestroy, DoCheck, HasChanges {

  constructor(@Inject('SERVERSIDE') private serverSide: boolean, @Inject('TAG') private tagInj: Tag, private tagService: TagService, private router: Router, private route: ActivatedRoute, private titleService: Title, private htmlLink: HtmlLinkHelper, private differs: KeyValueDiffers) {
    if (serverSide === true) {
      this.setTag(tagInj);
    } else {
      var id = parseInt(this.route.snapshot.paramMap.get('id'));
      this.loadTag(id);
    }
  }

  private loadTag(id: number) {
    this.tagService.getTag(id, true).then((tag) => {
      this.setTag(tag);
    }).catch((error) => {
      console.log('Could not fetch tag category', error);
    });
  }

  private setTag(tag: Tag) {
    console.log('set tag');
    this.tag = tag;
    this.titleService.setTitle(`Edit tag: ${tag.description}`);
    this.oldDescription = tag.description;

    this.tagDiffer = this.differs.find(this.tag).create();
    setTimeout(() => this.hasChanges = false);
  }

  oldDescription: string = '';
  tag: Tag = {
    id: 0,
    subjects: [],
    description: '',
    category: {
      id: 0,
      description: '',
      color: null,
      tags: []
    },
    parent: null,
    children: []
  };

  public updateTag() {
    this.tagService.updateTag(this.tag).then((tag) => {
      this.router.navigate(['tag','category',tag.category.id,'tags',tag.id]);
    }).catch((error) => {
      console.log('Could not update tag', error);
    });
  }

  //#region Prevent loss of changes
  hasChanges: boolean = false;
  private tagDiffer: KeyValueDiffer<string, any> = null;
  @HostListener('window:beforeunload', ['$event'])
  beforeUnload($event: IBeforeUnloadEvent) {
    if (this.hasChanges) {
      $event.returnValue = '';
      if (!confirm("There are unsaved changes. Are you sure you want to quit?")) {
        $event.preventDefault();
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

  ngDoCheck() {
    if (this.tagDiffer !== null) {
      const changes = this.tagDiffer.diff(this.tag);
      if (changes) {
        this.hasChanges = true;
      }
    }
  }
}
