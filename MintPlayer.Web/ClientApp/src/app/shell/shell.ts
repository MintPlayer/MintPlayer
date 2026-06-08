import { Component, ChangeDetectionStrategy, inject, signal, effect, afterNextRender, PLATFORM_ID, DestroyRef } from '@angular/core';
import { CommonModule, isPlatformBrowser, KeyValuePipe } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { BsShellComponent, BsShellSidebarDirective } from '@mintplayer/ng-bootstrap/shell';
import type { ShellStateChangeEventDetail } from '@mintplayer/web-components/shell';
import { BsAccordionComponent, BsAccordionTabComponent, BsAccordionTabHeaderComponent } from '@mintplayer/ng-bootstrap/accordion';
import { BsNavbarTogglerComponent } from '@mintplayer/ng-bootstrap/navbar-toggler';
import { BsSelectComponent, BsSelectOption } from '@mintplayer/ng-bootstrap/select';
import { SparkAuthBarComponent } from '@mintplayer/ng-spark-auth/auth-bar';
import { SparkAuthService } from '@mintplayer/ng-spark-auth/core';
import { SparkService, SparkLanguageService } from '@mintplayer/ng-spark/services';
import { ProgramUnitGroup } from '@mintplayer/ng-spark/models';
import { SparkIconComponent } from '@mintplayer/ng-spark/icon';
import { ResolveTranslationPipe, IconNamePipe, RouterLinkPipe } from '@mintplayer/ng-spark/pipes';
import { BsShellTopbarDirective } from './bs-shell-topbar.directive';
import { PlayerCard } from '../player/player-card';
import { PlaylistSidebar } from '../player/playlist-sidebar';

/**
 * Application shell on ng-bootstrap 22's <bs-shell> (a lit <mp-shell> web component with named
 * slots): topbar (sidebar toggle, language picker, auth bar) + a collapsible sidebar driven by
 * Spark's program-unit metadata, with routed content in the main area.
 *
 * Sidebar open/closed is a single writable boolean `sidebarState`: the topbar
 * <bs-navbar-toggler> two-way binds it (`[(state)]`), and <bs-shell> is driven from it
 * (`[state]="sidebarState() ? 'show' : 'hide'"`, mirrored back via (statechange)). We emulate
 * responsive behaviour ourselves — open by default above the breakpoint, closed below, switching
 * only on breakpoint crossings so a manual toggle isn't clobbered by every resize event.
 */
@Component({
  selector: 'app-shell',
  imports: [
    CommonModule, RouterModule, FormsModule, KeyValuePipe,
    BsShellComponent, BsShellSidebarDirective, BsShellTopbarDirective,
    BsAccordionComponent, BsAccordionTabComponent, BsAccordionTabHeaderComponent,
    BsNavbarTogglerComponent, BsSelectComponent, BsSelectOption,
    SparkIconComponent, SparkAuthBarComponent,
    ResolveTranslationPipe, IconNamePipe, RouterLinkPipe,
    PlayerCard, PlaylistSidebar,
  ],
  templateUrl: './shell.html',
  styleUrl: './shell.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Shell {
  private readonly sparkService = inject(SparkService);
  private readonly authService = inject(SparkAuthService);
  private readonly platformId = inject(PLATFORM_ID);
  private readonly destroyRef = inject(DestroyRef);

  readonly lang = inject(SparkLanguageService);
  programUnitGroups = signal<ProgramUnitGroup[]>([]);

  /** Single source of truth for sidebar open/closed; two-way bound to the navbar toggler. */
  sidebarState = signal<boolean>(false);
  private wasAboveBreakpoint = false;

  constructor() {
    afterNextRender(() => {
      this.wasAboveBreakpoint = this.isAboveBreakpoint();
      this.sidebarState.set(this.wasAboveBreakpoint);
      this.setupResizeListener();
    });

    // Re-fetch program units whenever auth state changes (login/logout).
    effect(() => {
      this.authService.user(); // track the signal
      this.loadProgramUnits();
    });
  }

  private async loadProgramUnits(): Promise<void> {
    const config = await this.sparkService.getProgramUnits();
    this.programUnitGroups.set(config.programUnitGroups.sort((a, b) => a.order - b.order));
  }

  /** Reflect the shell's own open/close (e.g. backdrop click) back into the toggler's state. */
  onShellStateChange(detail: ShellStateChangeEventDetail) {
    this.sidebarState.set(detail.open);
  }

  onMenuItemClick() {
    // On narrow viewports the sidebar is an overlay — close it after navigating.
    if (!this.isAboveBreakpoint()) {
      this.sidebarState.set(false);
    }
  }

  private setupResizeListener(): void {
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }

    const onResize = () => {
      const above = this.isAboveBreakpoint();
      if (above !== this.wasAboveBreakpoint) {
        this.wasAboveBreakpoint = above;
        this.sidebarState.set(above); // only flip on a breakpoint crossing
      }
    };
    window.addEventListener('resize', onResize);
    this.destroyRef.onDestroy(() => window.removeEventListener('resize', onResize));
  }

  private isAboveBreakpoint(): boolean {
    if (!isPlatformBrowser(this.platformId)) {
      return false;
    }
    // Bootstrap 'md' breakpoint is 768px
    return window.innerWidth >= 768;
  }
}
