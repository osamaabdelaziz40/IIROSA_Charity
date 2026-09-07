// Extracts epic-9 (periodic/orphan-report) i18n namespaces from ar.json + en.json
// and reports ar/en key parity. Output: c4-i18n-appendix.md next to this script.
const fs = require('fs');
const path = require('path');
const dir = __dirname;
const root = path.resolve(dir, '../../../..');
const load = (p) => JSON.parse(fs.readFileSync(path.join(root, p), 'utf8'));
const ar = load('Frontend/src/assets/i18n/ar.json');
const en = load('Frontend/src/assets/i18n/en.json');

// collect subtrees whose path contains a periodic/orphan-report segment
const MATCH = /periodic|orphanreport/i;
const found = {};
function walk(node, trail, sink) {
  if (node && typeof node === 'object' && !Array.isArray(node)) {
    for (const [k, v] of Object.entries(node)) {
      const t = [...trail, k];
      if (MATCH.test(k) && v && typeof v === 'object') {
        sink[t.join('.')] = v;
      } else {
        walk(v, t, sink);
      }
    }
  }
}
const arHit = {}; walk(ar, [], arHit);
const enHit = {}; walk(en, [], enHit);

function flatKeys(o, prefix, out) {
  for (const [k, v] of Object.entries(o)) {
    const p = prefix ? prefix + '.' + k : k;
    if (v && typeof v === 'object') flatKeys(v, p, out);
    else out.push(p);
  }
  return out;
}

let md = '# C4 appendix — epic-9 i18n namespaces (ar vs en)\n\n';
md += '## Parity summary\n\n| Namespace | ar keys | en keys | ar-only | en-only |\n| --- | --- | --- | --- | --- |\n';
const allNs = [...new Set([...Object.keys(arHit), ...Object.keys(enHit)])].sort();
for (const ns of allNs) {
  const a = arHit[ns] ? flatKeys(arHit[ns], '', []) : [];
  const e = enHit[ns] ? flatKeys(enHit[ns], '', []) : [];
  const as = new Set(a), es = new Set(e);
  const aOnly = a.filter((k) => !es.has(k));
  const eOnly = e.filter((k) => !as.has(k));
  md += `| ${ns} | ${a.length} | ${e.length} | ${aOnly.length} | ${eOnly.length} |\n`;
  if (aOnly.length) md += `  - ar-only: ${aOnly.join(', ')}\n`;
  if (eOnly.length) md += `  - en-only: ${eOnly.join(', ')}\n`;
}
md += '\n## Namespaces (ar.json)\n\n';
for (const ns of allNs) md += `### ${ns}\n\`\`\`json\n${JSON.stringify(arHit[ns] ?? null, null, 1)}\n\`\`\`\n`;
md += '\n## Namespaces (en.json)\n\n';
for (const ns of allNs) md += `### ${ns}\n\`\`\`json\n${JSON.stringify(enHit[ns] ?? null, null, 1)}\n\`\`\`\n`;
fs.writeFileSync(path.join(dir, 'c4-i18n-appendix.md'), md);
console.log('namespaces:', allNs.join(' | '));
console.log('wrote c4-i18n-appendix.md,', md.split('\n').length, 'lines');
