/* ============================================================
   Agentstrap Docs — static site generator
   Usage:  node source/build.mjs
   Output: index.html + <section>/*.html in the Docs Site root.
   No external dependencies.
   ============================================================ */

import { writeFileSync, mkdirSync, existsSync } from "node:fs";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const HERE = dirname(fileURLToPath(import.meta.url));
const ROOT = resolve(HERE, "..");

const SITE = "Agentstrap Docs";
const VERSION = "1.0.0";

/**
 * Gives every h2/h3 a stable id derived from its text, so sections can be
 * deep-linked. Existing ids are left alone.
 */
function addHeadingIds(html) {
  const seen = new Map();

  return html.replace(/<h([23])([^>]*)>([\s\S]*?)<\/h\1>/g, (match, level, attrs, inner) => {
    if (/\sid="/.test(attrs)) return match;

    const text = inner.replace(/<[^>]+>/g, "").trim();
    let slug = text
      .toLowerCase()
      .replace(/[^a-z0-9]+/g, "-")
      .replace(/^-+|-+$/g, "");

    if (!slug) return match;

    const count = seen.get(slug) || 0;
    seen.set(slug, count + 1);
    if (count > 0) slug = `${slug}-${count}`;

    return `<h${level}${attrs} id="${slug}">${inner}</h${level}>`;
  });
}

function render(page) {
  const base = page.path.includes("/") ? "../" : "";

  return `<!DOCTYPE html>
<html lang="en" data-theme="dark">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>${page.title} &middot; ${SITE}</title>
<meta name="description" content="${page.lede}">
<meta property="og:title" content="${page.title} &middot; ${SITE}">
<meta property="og:description" content="${page.lede}">
<meta property="og:type" content="article">
<link rel="stylesheet" href="${base}assets/style.css">
<link rel="icon" href="${base}assets/Agentstrap.ico" type="image/x-icon">
</head>
<body data-page="${page.path}" data-base="${base}">
<header class="topbar">
  <button class="icon-btn menu-btn" id="menu-btn" aria-label="Toggle navigation">&#9776;</button>
  <a class="brand" href="${base}index.html">
    <span class="brand-mark"><img src="${base}assets/Agentstrap.ico" alt="" width="26" height="26"></span>
    Agentstrap <small>Docs</small>
  </a>
  <span class="topbar-spacer"></span>
  <div class="search-wrap">
    <input id="search" type="search" placeholder="Search docs&hellip;" autocomplete="off" aria-label="Search documentation">
    <div id="search-results"></div>
  </div>
  <button class="icon-btn" id="theme-btn" title="Toggle theme" aria-label="Toggle theme">&#9680;</button>
</header>

<div class="shell">
  <aside class="sidebar" id="sidebar"></aside>
  <main class="content">
    <h1>${page.title}</h1>
    <p class="page-lede">${page.lede}</p>
${addHeadingIds(page.body)}
    <nav class="page-nav" id="page-nav"></nav>
    <p class="meta">Agentstrap ${VERSION} &middot; These docs describe Agentstrap ${VERSION}. Features marked
    <span class="badge new">new</span> are not present in upstream Bloxstrap.</p>
  </main>
</div>

<script src="${base}assets/site.js"></script>
</body>
</html>
`;
}

const pages = [];

async function collect() {
  const mods = [
    "./pages/home.mjs",
    "./pages/features-a.mjs",
    "./pages/features-b.mjs",
    "./pages/info.mjs",
    "./pages/help.mjs",
    "./pages/developers.mjs",
  ];
  for (const m of mods) {
    const mod = await import(m);
    pages.push(...mod.default);
  }
}

await collect();

let written = 0;
for (const page of pages) {
  const out = join(ROOT, page.path);
  if (!existsSync(dirname(out))) mkdirSync(dirname(out), { recursive: true });
  writeFileSync(out, render(page), "utf8");
  written++;
  console.log("  " + page.path);
}

console.log(`\n${written} pages written to ${ROOT}`);
