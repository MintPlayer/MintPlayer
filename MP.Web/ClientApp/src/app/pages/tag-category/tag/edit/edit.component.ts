import { Component, OnInit, OnDestroy, Inject } from '@angular/core';
import { TagService } from '../../../../services/tag/tag.service';
import { Router, ActivatedRoute } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { Tag } from '../../../../entities/tag';
import { HtmlLinkHelper } from '../../../../helpers/html-link.helper';

@Component({
  selector: 'app-edit',
  templateUrl: './edit.component.html',
  styleUrls: ['./edit.component.scss']
})
export class EditComponent implements OnInit, OnDestroy {

  constructor(@Inject('SERVERSIDE') private serverSide: boolean, @Inject('TAG') private tagInj: Tag, private tagService: TagService, private router: Router, private route: ActivatedRoute, private titleService: Title, private htmlLink: HtmlLinkHelper) {
    if (serverSide === true) {
      this.setTag(tagInj);
    } else {
      // Get tag
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
    this.tag = tag;
    this.titleService.setTitle(`Edit tag: ${tag.description}`);
    this.oldDescription = tag.description;
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

  ngOnInit() {
    this.htmlLink.setCanonicalWithoutQuery();
  }

  ngOnDestroy() {
    this.htmlLink.unset('canonical');
  }
}
