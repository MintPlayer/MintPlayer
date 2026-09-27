import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

/** Like state of a subject for the current request (mirror of the server `SubjectLikeResult`, camelCased). */
export interface SubjectLikeResult {
  likes: number;
  dislikes: number;
  /** The caller's own preference: `true` likes, `false` dislikes, `null` neither. */
  like: boolean | null;
  /** Whether the caller is signed in (so the UI knows if {@link like} is actionable). */
  authenticated: boolean;
}

/**
 * Thin client over the existing like API (`/api/subject/likes`). Reading totals is anonymous; setting a
 * preference requires a signed-in user (cookie auth — sent automatically same-origin). Passing `like: null`
 * clears the caller's preference (unlike).
 */
@Injectable({ providedIn: 'root' })
export class SubjectLikeService {
  private readonly http = inject(HttpClient);

  get(subjectId: string): Promise<SubjectLikeResult> {
    return firstValueFrom(this.http.get<SubjectLikeResult>('/api/subject/likes', { params: { id: subjectId } }));
  }

  set(subjectId: string, like: boolean | null): Promise<SubjectLikeResult> {
    return firstValueFrom(this.http.post<SubjectLikeResult>('/api/subject/likes', { subjectId, like }));
  }
}
