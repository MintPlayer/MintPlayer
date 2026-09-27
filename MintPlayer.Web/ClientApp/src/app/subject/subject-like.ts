import { ChangeDetectionStrategy, Component, effect, inject, input, signal } from '@angular/core';
import { SubjectLikeResult, SubjectLikeService } from './subject-like.service';

/**
 * Like / dislike widget for a catalog subject (Song / Artist / Person), shown in the detail-page toolbar by
 * {@link AppPoDetail}. Two toggle buttons with live counts (legacy parity: thumbs up/down). Reading counts is
 * anonymous; the buttons are disabled until the user signs in. Clicking the already-active choice clears it.
 */
@Component({
  selector: 'app-subject-like',
  template: `
    @if (state(); as s) {
      <div class="btn-group" role="group" aria-label="Like or dislike">
        <button type="button" class="btn"
                [class.btn-success]="s.like === true" [class.btn-outline-success]="s.like !== true"
                (click)="toggle(true)" [disabled]="busy() || !s.authenticated"
                [title]="s.authenticated ? 'Like' : 'Log in to like'" aria-label="Like">
          <i class="bi bi-hand-thumbs-up"></i> {{ s.likes }}
        </button>
        <button type="button" class="btn"
                [class.btn-danger]="s.like === false" [class.btn-outline-danger]="s.like !== false"
                (click)="toggle(false)" [disabled]="busy() || !s.authenticated"
                [title]="s.authenticated ? 'Dislike' : 'Log in to dislike'" aria-label="Dislike">
          <i class="bi bi-hand-thumbs-down"></i> {{ s.dislikes }}
        </button>
      </div>
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SubjectLike {
  private readonly service = inject(SubjectLikeService);

  /** The subject's id (e.g. `"Songs/6"`). */
  readonly subjectId = input.required<string>();

  protected readonly state = signal<SubjectLikeResult | null>(null);
  protected readonly busy = signal(false);

  constructor() {
    effect(() => {
      const id = this.subjectId();
      this.state.set(null);
      this.service.get(id).then((result) => this.state.set(result)).catch(() => {});
    });
  }

  /** Apply a preference; clicking the active one clears it (unlike). The response carries fresh totals. */
  protected async toggle(like: boolean): Promise<void> {
    const current = this.state();
    if (!current || this.busy()) {
      return;
    }
    const next = current.like === like ? null : like;
    this.busy.set(true);
    try {
      this.state.set(await this.service.set(this.subjectId(), next));
    } catch {
      // Keep the previous state on failure (e.g. session expired).
    } finally {
      this.busy.set(false);
    }
  }
}
