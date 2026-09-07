// Lists translate keys USED by epic-9 module files but MISSING from ar.json/en.json.
const fs = require('fs');
const path = require('path');
const root = path.resolve(__dirname, '../../../..');
const modDir = path.join(root, 'Frontend/src/app/modules/periodic-orphan-reports');

const files = [];
(function collect(d) {
  for (const e of fs.readdirSync(d, { withFileTypes: true })) {
    const p = path.join(d, e.name);
    if (e.isDirectory()) collect(p);
    else if (/\.(html|ts)$/.test(e.name) && !e.name.endsWith('.spec.ts')) files.push(p);
  }
})(modDir);

const keyRe = [
  /'([a-zA-Z][a-zA-Z0-9_.]+)'\s*\|\s*translate/g,          // pipe
  /\.(?:instant|get|stream)\(\s*'([a-zA-Z][a-zA-Z0-9_.]+)'/g, // service
  /\.(?:instant|get|stream)<[^>]+>\(\s*'([a-zA-Z][a-zA-Z0-9_.]+)'/g,
  /pageTitle:\s*'([a-zA-Z][a-zA-Z0-9_.]+)'/g,
  /breadcrumb:\s*'([a-zA-Z][a-zA-Z0-9_.]+)'/g,
];

const used = new Map(); // key -> [files]
for (const f of files) {
  const txt = fs.readFileSync(f, 'utf8');
  for (const re of keyRe) {
    re.lastIndex = 0;
    let m;
    while ((m = re.exec(txt))) {
      const k = m[1];
      if (!used.has(k)) used.set(k, []);
      used.get(k).push(path.basename(f));
    }
  }
}

const load = (p) => JSON.parse(fs.readFileSync(path.join(root, p), 'utf8'));
function flat(o, prefix, out) {
  for (const [k, v] of Object.entries(o)) {
    const p = prefix ? prefix + '.' + k : k;
    if (v && typeof v === 'object') flat(v, p, out);
    else out.add(p);
  }
  return out;
}
const arKeys = flat(load('Frontend/src/assets/i18n/ar.json'), '', new Set());
const enKeys = flat(load('Frontend/src/assets/i18n/en.json'), '', new Set());

const missing = [...used.keys()].filter((k) => !arKeys.has(k) || !enKeys.has(k)).sort();
for (const k of missing) {
  const where = !arKeys.has(k) && !enKeys.has(k) ? 'BOTH' : !arKeys.has(k) ? 'ar-only' : 'en-only';
  console.log(`${where}\t${k}\t${[...new Set(used.get(k))].join(',')}`);
}
console.log(`--- missing count: ${missing.length}`);
