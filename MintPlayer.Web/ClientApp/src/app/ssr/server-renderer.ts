/**
 * Minimal, dependency-free replacement for the archived `aspnet-prerendering` npm package's
 * `createServerRenderer`. MintPlayer.AspNetCore.SpaServices.Prerendering's bundled prerenderer.js
 * invokes the boot module's default export with the classic JavaScriptServices signature and only
 * calls it directly when the function carries `isServerRenderer = true`; otherwise it falls back to
 * `require('aspnet-prerendering')` at runtime (which would need node_modules in the image).
 *
 * Unlike the original, this does not use Node's deprecated `domain` module or `url.parse`.
 */

/** What the boot function receives — same shape as aspnet-prerendering's BootFuncParams. */
export interface BootFuncParams {
  /** Request path + query, e.g. `/song/313?x=1`. */
  url: string;
  /** `scheme://host` of the request. */
  origin: string;
  /** Path base + `/` (the page's `<base href>`). */
  baseUrl: string;
  absoluteUrl: string;
  /** Everything `ISpaPrerenderingService.OnSupplyData` put in the dictionary (camelCased), plus `originalHtml`. */
  data: Record<string, unknown> & { originalHtml: string };
}

export interface RenderResult {
  html?: string;
  statusCode?: number;
  redirectUrl?: string;
  globals?: Record<string, unknown>;
}

type Callback = (error: unknown, result: RenderResult | null) => void;

const defaultTimeoutMilliseconds = 30_000;

export function createServerRenderer(bootFunc: (params: BootFuncParams) => Promise<RenderResult>) {
  const renderer = (
    callback: Callback,
    _applicationBasePath: string,
    bootModule: { moduleName: string },
    absoluteRequestUrl: string,
    requestPathAndQuery: string,
    customDataParameter: BootFuncParams['data'],
    overrideTimeoutMilliseconds: number,
    requestPathBase: string,
  ) => {
    const absolute = new URL(absoluteRequestUrl);
    const params: BootFuncParams = {
      url: requestPathAndQuery,
      origin: `${absolute.protocol}//${absolute.host}`,
      baseUrl: `${requestPathBase || ''}/`,
      absoluteUrl: absoluteRequestUrl,
      data: customDataParameter,
    };

    let promise: Promise<RenderResult>;
    try {
      promise = bootFunc(params);
    } catch (err) {
      callback(err instanceof Error ? err.stack ?? err.message : err, null);
      return;
    }

    // Report exactly once: whichever of render / timeout settles first wins.
    let settled = false;
    const done = (error: unknown, result: RenderResult | null) => {
      if (settled) return;
      settled = true;
      clearTimeout(timer);
      callback(error, result);
    };

    const timeout = overrideTimeoutMilliseconds || defaultTimeoutMilliseconds; // -1 = never
    const timer = timeout > 0
      ? setTimeout(() => done(`Prerendering timed out after ${timeout}ms (boot module ${bootModule.moduleName}).`, null), timeout)
      : undefined;

    promise.then(
      (result) => done(null, result),
      (err) => done(err instanceof Error ? err.stack ?? err.message : err, null),
    );
  };
  (renderer as unknown as { isServerRenderer: boolean }).isServerRenderer = true;
  return renderer;
}
