/* ============================================================
   Agentstrap Docs — shared behaviour
   Vanilla JS, no build step, no modules (works over file://)
   ============================================================ */

const NAV = [
  {
    title: "GitHub",
    items: [
      { href: "github.html", text: "GitHub" },
    ],
  },
  {
    title: "Overview",
    items: [{ href: "index.html", text: "Home" }],
  },
  {
    title: "Features",
    items: [
      { href: "features/index.html", text: "All features" },
      { href: "features/matchmaker.html", text: "Matchmaker" },
      { href: "features/command-center.html", text: "Command Center" },
      { href: "features/engine-settings.html", text: "Engine settings" },
      { href: "features/server-connection-overlay.html", text: "Server connection overlay" },
      { href: "features/server-connection-push-notifications.html", text: "Server connection push notifications" },
      { href: "features/server-location-hints.html", text: "Server location hints" },
      { href: "features/game-history.html", text: "Game history" },
      { href: "features/activity-tracking.html", text: "Activity tracking" },
      { href: "features/discord-rich-presence.html", text: "Discord Rich Presence" },
      { href: "features/custom-integrations.html", text: "Custom integrations" },
      { href: "features/window-attributes.html", text: "Custom window attributes" },
      { href: "features/deployment-channels.html", text: "Deployment channels" },
      { href: "features/diagnostics.html", text: "Diagnostics" },
      { href: "features/updates.html", text: "Updates" },
      { href: "features/modding.html", text: "Modding" },
      { href: "features/nexus.html", text: "NEXUS mode" },
      { href: "features/dont-exit-to-desktop-app.html", text: "Don't exit to desktop app" },
    ],
  },
  {
    title: "Info",
    items: [
      { href: "info/index.html", text: "Overview" },
      { href: "info/privacy-policy.html", text: "Privacy policy" },
      { href: "info/bans.html", text: "Agentstrap and bans" },
      { href: "info/translations.html", text: "Translations" },
      { href: "info/using-roblox-icons-for-shortcuts.html", text: "Roblox icons for shortcuts" },
      { href: "info/where-data-is-stored.html", text: "Where data is stored" },
    ],
  },
  {
    title: "Help",
    items: [
      { href: "help/index.html", text: "Help index" },
      { href: "help/troubleshooting.html", text: "Agentstrap isn't working" },
      { href: "help/roblox-crashes.html", text: "Roblox crashes or won't launch" },
      { href: "help/repairing-roblox-install.html", text: "Repairing a broken Roblox install" },
      { href: "help/server-location-unavailable.html", text: "Server location unavailable" },
      { href: "help/discord-rich-presence-not-working.html", text: "Discord Rich Presence not working" },
      { href: "help/change-install-location.html", text: "Change the install location" },
      { href: "help/uninstalling.html", text: "Uninstalling Agentstrap" },
    ],
  },
  {
    title: "Developers",
    items: [{ href: "developers/index.html", text: "AgentXCO2" }],
  },
];

// Flat list used by the search box.
const SEARCH_INDEX = NAV.flatMap((g) =>
  g.items.map((i) => ({ ...i, group: g.title }))
);

function currentBase() {
  return (document.body && document.body.dataset.base) || "";
}

function currentPage() {
  return (document.body && document.body.dataset.page) || "";
}

function buildSidebar() {
  const host = document.getElementById("sidebar");
  if (!host) return;

  const base = currentBase();
  const here = currentPage();

  let html = "";
  for (const group of NAV) {
    html += '<div class="nav-group"><h4>' + group.title + "</h4><ul class=\"nav-list\">";
    for (const item of group.items) {
      const active = item.href === here ? ' class="active"' : "";
      const href = item.href.startsWith("http") ? item.href : base + item.href;
      html +=
        '<li><a href="' +
        href +
        '"' +
        active +
        ">" +
        item.text +
        "</a></li>";
    }
    html += "</ul></div>";
  }
  host.innerHTML = html;
}

function buildPrevNext() {
  const host = document.getElementById("page-nav");
  if (!host) return;

  const flat = SEARCH_INDEX;
  const here = currentPage();

  const idx = flat.findIndex((i) => i.href === here);
  if (idx === -1) {
    host.innerHTML = "";
    return;
  }

  const base = currentBase();
  const prev = flat[idx - 1];
  const next = flat[idx + 1];

  let html = "";
  if (prev) {
    html +=
      '<a class="prev" href="' +
      base +
      prev.href +
      '"><span>Previous</span>' +
      prev.text +
      "</a>";
  } else {
    html += "<span></span>";
  }
  if (next) {
    html +=
      '<a class="next" href="' +
      base +
      next.href +
      '"><span>Next</span>' +
      next.text +
      "</a>";
  }
  host.innerHTML = html;
}

function setupTheme() {
  const btn = document.getElementById("theme-btn");
  const saved = localStorage.getItem("agentstrap-docs-theme");
  const prefersLight = window.matchMedia("(prefers-color-scheme: light)").matches;
  document.documentElement.setAttribute("data-theme", saved || (prefersLight ? "light" : "dark"));

  if (!btn) return;
  const sync = () => {
    const dark = document.documentElement.getAttribute("data-theme") === "dark";
    btn.textContent = dark ? "\u25D1" : "\u25D0";
    btn.title = dark ? "Switch to light theme" : "Switch to dark theme";
  };
  sync();
  btn.addEventListener("click", () => {
    const cur = document.documentElement.getAttribute("data-theme");
    const next = cur === "dark" ? "light" : "dark";
    document.documentElement.setAttribute("data-theme", next);
    localStorage.setItem("agentstrap-docs-theme", next);
    sync();
  });
}

function setupSearch() {
  const input = document.getElementById("search");
  const results = document.getElementById("search-results");
  if (!input || !results) return;

  const base = currentBase();

  const run = () => {
    const q = input.value.trim().toLowerCase();
    if (q.length < 2) {
      results.classList.remove("open");
      results.innerHTML = "";
      return;
    }

    const hits = SEARCH_INDEX.filter(
      (i) => i.text.toLowerCase().includes(q) || i.group.toLowerCase().includes(q)
    );

    if (!hits.length) {
      results.innerHTML = '<div class="empty">No pages match &ldquo;' + q + '&rdquo;.</div>';
    } else {
      results.innerHTML = hits
        .map(
          (h) =>
            '<a href="' +
            base +
            h.href +
            '">' +
            h.text +
            "<small>" +
            h.group +
            "</small></a>"
        )
        .join("");
    }
    results.classList.add("open");
  };

  input.addEventListener("input", run);
  input.addEventListener("keydown", (e) => {
    if (e.key === "Escape") {
      input.value = "";
      results.classList.remove("open");
      input.blur();
    }
  });
  document.addEventListener("click", (e) => {
    if (!results.contains(e.target) && e.target !== input) {
      results.classList.remove("open");
    }
  });
}

function setupMenu() {
  const btn = document.getElementById("menu-btn");
  const sidebar = document.getElementById("sidebar");
  if (!btn || !sidebar) return;
  btn.addEventListener("click", () => sidebar.classList.toggle("open"));
}

document.addEventListener("DOMContentLoaded", () => {
  buildSidebar();
  buildPrevNext();
  setupTheme();
  setupSearch();
  setupMenu();
});
