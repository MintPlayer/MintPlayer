import { defineConfig } from 'vitest/config';

/**
 * Vitest config for the Angular `@angular/build:unit-test` builder (wired via `test.runnerConfig`).
 *
 * The `@mintplayer/*` player libraries ship ESM with extensionless / directory-style internal
 * imports (e.g. `import … from '../../enums'`). The application build (esbuild) resolves those
 * fine, but Vitest *externalizes* node_modules to Node's strict ESM resolver, which rejects them
 * (ERR_UNSUPPORTED_DIR_IMPORT). Inlining them forces Vitest to transform them through Vite's
 * resolver instead, matching the app build. Needed for any spec touching the player/playlist stack.
 */
export default defineConfig({
  test: {
    server: {
      deps: {
        inline: [/@mintplayer\//],
      },
    },
  },
});
