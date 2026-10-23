# Agentstrap Docs

The documentation site for [Agentstrap](https://github.com/agentxco2-dev/Agentstrap).

It's a **dependency-free static site** — plain HTML, one CSS file and one JavaScript file. There's no build
step required to *use* it, no Node dependency, and no framework. Open `index.html` in a browser and it works,
including straight off the filesystem over `file://`.

## Layout

```
Docs Site/
  index.html            Home
  features/             Feature reference (17 pages)
  info/                 Privacy, bans, translations, icons, data storage (6 pages)
  help/                 Troubleshooting guides (8 pages)
  developers/           AgentXCO2 (1 page)
  assets/
    style.css           The whole design system
    site.js             Navigation, theme and search
    Agentstrap.ico     The app icon
  source/               The generator that produces the .html files
```

The `.html` files at the top level are the site. `source/` is only needed if you want to change the content.

## Regenerating the pages

Content lives in `source/pages/*.mjs` as plain HTML strings. To change a page, edit its module and rebuild:

```powershell
node source/build.mjs
```

That writes every page into place. The script has no dependencies — Node 18 or newer is enough.

| Module | Pages |
|---|---|
| `source/pages/home.mjs` | `index.html` |
| `source/pages/features-a.mjs` | Features index + 7 feature pages |
| `source/pages/features-b.mjs` | 9 feature pages |
| `source/pages/info.mjs` | The Info section |
| `source/pages/help.mjs` | The Help section |
| `source/pages/developers.mjs` | The Developers section |

### Adding a page

1. Add an entry to the relevant `source/pages/*.mjs` file with `path`, `title`, `lede` and `body`.
2. Add the matching link to the `NAV` array in `assets/site.js`.
3. Run `node source/build.mjs`.

`path` is relative to the site root, so a page in `features/` uses `"features/my-page.html"`. The generator
works out the relative asset paths and the active-nav highlighting from that.

## How the shared chrome works

`assets/site.js` injects the sidebar, the previous/next links and the search results at runtime. The navigation
structure lives in the `NAV` array at the top of that file — one array, and the sidebar, the prev/next pager and
the search index all derive from it. Adding a link in one place updates all three.

Relative paths are resolved with two attributes that the generator writes onto `<body>`:

- `data-page` — the page's path relative to the site root, used to work out which nav item is active.
- `data-base` — `""` on the home page and `"../"` on every sub-page, used to build relative links.

This is why the site works over `file://`: there's no `fetch`, no absolute root path, and no server-side
routing.

## Search

The search box filters the navigation index by name. It's client-side only and works offline.

## Theme

Light and dark themes are selected from the sun/moon button in the top bar. The choice is stored in
`localStorage` under `agentstrap-docs-theme`, and defaults to whatever the operating system is set to.

## Deploying

The site is live at **[agentstrap-doc.pages.dev](https://agentstrap-doc.pages.dev)** (Cloudflare Pages). The app's
in-app help links point at that URL, so keep the root path stable if you move it.

The site is plain static files, so anything that serves files works:

| Host | What to point at |
|---|---|
| GitHub Pages | Publish the `Docs Site` folder (or its contents) as a Pages site |
| Netlify / Cloudflare Pages | Build command: none. Publish directory: `Docs Site` |
| Any web server | Copy the folder contents into the document root |

One thing to be careful about: **serve the contents of `Docs Site`, not the folder itself.** The home page has to
end up at `/index.html` for the relative links to resolve.

If you change the URL, rerun the link updater so the app keeps pointing at the right place:

```powershell
.\Scripts\update-docs-links.ps1 -BaseUrl https://your-new-url.example
```

## Adding it to the Bloxstrap wiki site

The upstream wiki lives in the [`bloxstraplabs.com`](https://github.com/bloxstraplabs/bloxstraplabs.com)
repository as an Astro Starlight site, with the pages under `src/content/docs/wiki/`. To replace that content with
these docs, each page needs converting from HTML to MDX (or Markdown) and the sidebar configuration in
`astro.config.mjs` needs updating to match the new structure.

The section structure maps across directly:

| This site | Wiki path |
|---|---|
| `features/index.html` | `features/index.mdx` |
| `features/*.html` | `features/*.mdx` |
| `info/*.html` | `info/*.mdx` |
| `help/*.html` | `help/*.mdx` |
| `developers/index.html` | `developers/index.mdx` |

The HTML in `source/pages/*.mjs` is mostly plain markup, so most of it converts to MDX with little more than
replacing the `<div class="callout note">` wrappers with Starlight asides:

```mdx
:::note
Text goes here.
:::
```

| Callout class | Starlight aside |
|---|---|
| `callout note` | `:::note` |
| `callout tip` | `:::tip` |
| `callout warning` | `:::caution` |
| `callout danger` | `:::danger` |

## Versioning

The docs describe Agentstrap **1.1.0**. The version is set in `source/build.mjs` and appears in the footer of
every page.

## Credits

Agentstrap is a fork of [Bloxstrap](https://github.com/bloxstraplabs/bloxstrap), which is licensed under the
[MIT License](https://github.com/agentxco2-dev/Agentstrap/blob/main/LICENSE). The interface uses
[WPF UI](https://github.com/lepoco/wpfui).
