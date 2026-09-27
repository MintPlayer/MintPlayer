// Calls the server bundle's default export the way SpaServices' prerenderer.js does.
import { readFileSync } from 'node:fs';
import { pathToFileURL } from 'node:url';

const dist = process.argv[2];
const mod = await import(pathToFileURL(`${dist}/server/main.server.mjs`).href);
const fn = mod.default;
console.log('isServerRenderer', fn.isServerRenderer);
const originalHtml = readFileSync(`${dist}/browser/index.csr.html`, 'utf8');
const data = {
  originalHtml,
  song: { id: '313', title: 'The Chain', released: '1977-02-04', artists: [{ id: '311', name: 'Fleetwood Mac', credited: false }], modifiedAt: null },
};
const t0 = performance.now();
fn((err, res) => {
  console.log('ms', Math.round(performance.now() - t0));
  if (err) { console.log('ERR', err); process.exit(1); }
  console.log(res.html);
  process.exit(0);
}, process.cwd(), { moduleName: 'main.server.mjs' }, 'http://localhost:5101/song/313', '/song/313', data, 0, '');
