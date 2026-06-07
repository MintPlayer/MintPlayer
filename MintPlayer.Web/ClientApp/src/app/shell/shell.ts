import { Component, ChangeDetectionStrategy, inject, signal, effect, afterNextRender, PLATFORM_ID, DestroyRef } from '@angular/core';
import { CommonModule, isPlatformBrowser, KeyValuePipe } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { BsShellComponent, BsShellSidebarDirective, BsShellState } from '@mintplayer/ng-bootstrap/shell';
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

/**
 * Application shell on ng-bootstrap 22's <bs-shell> (a lit <mp-shell> web component with named
 * slots): topbar (sidebar toggle, language picker, auth bar) + a collapsible sidebar driven by
 * Spark's program-unit metadata, with routed content in the main area. Ported from the Spark
 * Fleet demo shell.
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
  shellState = signal<BsShellState>('auto');
  isSidebarVisible = signal<boolean>(false);

  constructor() {
    afterNextRender(() => {
      this.setupResizeListener();
      this.updateSidebarVisibility();
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

  toggleSidebar(open: boolean) {
    this.shellState.set(open ? 'show' : 'hide');
    this.updateSidebarVisibility();
  }

  // Mirror the shell's actual open/closed state back to the toggler without forcing show/hide,
  // so responsive 'auto' mode is preserved. Explicit toggles go through the toggler.
  onShellToggle(detail: ShellStateChangeEventDetail) {
    this.isSidebarVisible.set(detail.open);
  }

  onMenuItemClick() {
    if (this.shellState() !== 'auto') {
      this.shellState.set('hide');
      this.updateSidebarVisibility();
    }
  }

  private setupResizeListener(): void {
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }

    const onResize = () => this.updateSidebarVisibility();
    window.addEventListener('resize', onResize);
    this.destroyRef.onDestroy(() => window.removeEventListener('resize', onResize));
  }

  private updateSidebarVisibility(): void {
    const state = this.shellState();
    if (state === 'show') {
      this.isSidebarVisible.set(true);
    } else if (state === 'hide') {
      this.isSidebarVisible.set(false);
    } else {
      this.isSidebarVisible.set(this.isAboveBreakpoint());
    }
  }

  private isAboveBreakpoint(): boolean {
    if (!isPlatformBrowser(this.platformId)) {
      return false;
    }
    // Bootstrap 'md' breakpoint is 768px
    return window.innerWidth >= 768;
  }
}
