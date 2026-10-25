export default [
  {
    path: "features/index.html",
    title: "Features",
    lede: "Every capability Agentstrap ships, grouped by what it does for you.",
    body: `
<p>Agentstrap is a fork of Bloxstrap, so it inherits the full Bloxstrap feature set and adds a substantial set of
new capabilities on top. Pages marked <span class="badge new">new</span> describe features that do not exist upstream.</p>

<div class="card-grid">
  <a class="card" href="matchmaker.html"><span class="card-tag">new</span><h3>Matchmaker</h3><p>Auto-join a place on launch, preferring quieter servers.</p></a>
  <a class="card" href="command-center.html"><span class="card-tag">new</span><h3>Command Center</h3><p>Launch, update and repair from one screen.</p></a>
  <a class="card" href="engine-settings.html"><h3>Engine settings</h3><p>Fast Flag presets, bundles and validation.</p></a>
  <a class="card" href="server-connection-overlay.html"><span class="card-tag">new</span><h3>Server connection overlay</h3><p>In-game toast showing server and location.</p></a>
  <a class="card" href="server-connection-push-notifications.html"><span class="card-tag">new</span><h3>Server connection push notifications</h3><p>Send server location alerts to your iPhone with Bark.</p></a>
  <a class="card" href="server-location-hints.html"><h3>Server location hints</h3><p>Find out where a server's data centre is.</p></a>
  <a class="card" href="game-history.html"><h3>Game history</h3><p>Rejoin a server you just left.</p></a>
  <a class="card" href="activity-tracking.html"><h3>Activity tracking</h3><p>The master switch for everything that watches your games.</p></a>
  <a class="card" href="discord-rich-presence.html"><h3>Discord Rich Presence</h3><p>Show what you're playing on Discord.</p></a>
  <a class="card" href="custom-integrations.html"><h3>Custom integrations</h3><p>Launch other programs alongside Roblox.</p></a>
  <a class="card" href="window-attributes.html"><span class="card-tag">new</span><h3>Custom window attributes</h3><p>Fixed size, position and title for the game window.</p></a>
  <a class="card" href="deployment-channels.html"><span class="card-tag">new</span><h3>Deployment channels</h3><p>Pin to a specific Roblox release channel.</p></a>
  <a class="card" href="diagnostics.html"><span class="card-tag">new</span><h3>Diagnostics</h3><p>Eight health checks, one report.</p></a>
  <a class="card" href="updates.html"><span class="card-tag">new</span><h3>Updates</h3><p>Agentstrap, mods and config migrations.</p></a>
  <a class="card" href="modding.html"><h3>Modding</h3><p>Customise assets without losing them on update.</p></a>
  <a class="card" href="nexus.html"><span class="card-tag">new</span><h3>NEXUS mode</h3><p>Opt in to experimental behaviour.</p></a>
  <a class="card" href="profiles.html"><span class="card-tag">new</span><h3>Profiles</h3><p>Save, switch, compare and restore configurations.</p></a>
  <a class="card" href="backups.html"><span class="card-tag">new</span><h3>Automatic backups</h3><p>Integrity-checked configuration snapshots.</p></a>
  <a class="card" href="extensions.html"><span class="card-tag">new</span><h3>Extensions</h3><p>Optional add-ons with validated manifests.</p></a>
  <a class="card" href="macos-support.html"><span class="card-tag">new</span><h3>macOS support</h3><p>Platform-aware capabilities and honest reporting.</p></a>
  <a class="card" href="dont-exit-to-desktop-app.html"><h3>Don't exit to desktop app</h3><p>Make the game fully close instead of returning to the app.</p></a>
</div>

<h2>Where each setting lives</h2>

<p>Agentstrap's settings window has a sidebar with fourteen pages. This is the map you need to find any option:</p>

<div class="table-wrap">
<table>
  <thead><tr><th>Settings page</th><th>What you'll configure there</th></tr></thead>
  <tbody>
    <tr><td><strong>Command Center</strong></td><td>Launch buttons, live status, repair shortcuts</td></tr>
    <tr><td><strong>Integrations</strong></td><td>Activity tracking, server location, overlay, Discord, custom integrations</td></tr>
    <tr><td><strong>Server Connection Notifications</strong></td><td>Bark push notifications and IPinfo location lookup</td></tr>
    <tr><td><strong>Bootstrapper</strong></td><td>Launch confirmation, background updates, forced reinstall</td></tr>
    <tr><td><strong>Mods</strong></td><td>Cursors, sounds, emoji font, custom font, skybox</td></tr>
    <tr><td><strong>Engine Settings</strong></td><td>Fast Flag presets, preset bundles, validator, flag editor</td></tr>
    <tr><td><strong>Quality of Life</strong></td><td>Update checking, background updates, custom window attributes</td></tr>
    <tr><td><strong>Appearance</strong></td><td>Theme, accent colour, custom themes, software rendering</td></tr>
    <tr><td><strong>Shortcuts</strong></td><td>Start Menu and desktop shortcuts, Roblox icons</td></tr>
    <tr><td><strong>Updates</strong></td><td>Agentstrap releases, mod updates, config migrations, NEXUS status</td></tr>
    <tr><td><strong>Diagnostics</strong></td><td>Health checks and the report</td></tr>
    <tr><td><strong>Deployment</strong></td><td>Channel pinning, launch behaviour, matchmaker</td></tr>
    <tr><td><strong>Advanced</strong></td><td>NEXUS mode, developer tools, maintenance</td></tr>
    <tr><td><strong>Agentstrap</strong></td><td>Update checking, analytics, web environment, export data</td></tr>
  </tbody>
</table>
</div>

<div class="callout note">
  <strong>Save button.</strong>
  Every page writes into an in-memory copy of your configuration. Nothing touches disk until you press
  <strong>Save</strong> in the bottom-right of the settings window. Three things save immediately: changing the
  process priority, resetting NEXUS to defaults, and restarting Agentstrap.
</div>
`,
  },

  {
    path: "features/matchmaker.html",
    title: "Matchmaker",
    lede: "Join a place automatically the moment Roblox starts, optionally picking a server that isn't already full.",
    body: `
<p>The matchmaker is Agentstrap's most automation-heavy feature. Once it's enabled and you give it a place, every
time Roblox launches it immediately joins that place &mdash; and if you'd rather not land in a packed server, it can
look for one with room first.</p>

<div class="callout tip">
  <strong>Where it lives.</strong>
  Settings &rarr; <strong>Deployment</strong> &rarr; <strong>Matchmaker</strong>. It is off by default.
</div>

<h2>Options</h2>

<div class="table-wrap">
<table>
  <thead><tr><th>Option</th><th>Default</th><th>What it does</th></tr></thead>
  <tbody>
    <tr>
      <td><strong>Enable matchmaker</strong></td>
      <td><span class="badge off">Off</span></td>
      <td>Join the place below as soon as Roblox starts. Leaving this off means the place ID is ignored.</td>
    </tr>
    <tr>
      <td><strong>Place ID</strong></td>
      <td>Empty</td>
      <td>
        The experience to join. Accepts a raw numeric place ID, or a full link such as
        <code>https://www.roblox.com/games/123456789/Whatever</code> &mdash; the ID is pulled out of the link for you.
        Leave it empty to pick a place at launch time instead.
      </td>
    </tr>
    <tr>
      <td><strong>Prefer low population servers</strong></td>
      <td><span class="badge on">On</span></td>
      <td>
        Skip full servers even when the list still has slots. This avoids long queue times on popular games.
        When it's on, Agentstrap picks the server with the fewest players; when it's off, it picks the busiest one
        that still satisfies your minimum.
      </td>
    </tr>
    <tr>
      <td><strong>Minimum free slots</strong></td>
      <td>1</td>
      <td>
        How many free slots a server needs before the matchmaker will join it. Offered values are
        <code>1</code>, <code>2</code>, <code>3</code>, <code>5</code> and <code>10</code>. Values outside
        <code>1&ndash;50</code> are clamped.
      </td>
    </tr>
  </tbody>
</table>
</div>

<h2>Finding a server right now</h2>

<p>The <strong>Find a server now</strong> button does the whole thing immediately, without waiting for the next
launch. It:</p>

<ol>
  <li>Saves your settings first, so the search uses exactly what you see on screen.</li>
  <li>Queries Roblox's public server list for the place, following at most three pages of results. The search is
      deliberately bounded so a single request can never churn through the entire server list.</li>
  <li>Keeps only servers where <code>MaxPlayers - Playing &gt;= Minimum free slots</code>.</li>
  <li>Sorts by your preference: fewest players first when <strong>Prefer low population servers</strong> is on
      (ties broken by the larger server), most players first when it's off (ties broken by the most free slots).</li>
  <li>Launches Roblox with a deep link that joins that exact server.</li>
</ol>

<div class="callout note">
  <strong>Roblox doesn't report back.</strong>
  After the matchmaker asks Roblox to join a server, Roblox doesn't tell Agentstrap whether the join actually
  worked. The status line tells you what was requested, not what happened.
</div>

<h2>Status messages</h2>

<p>The card under the matchmaker options tells you what the last action did:</p>

<div class="table-wrap">
<table>
  <thead><tr><th>Message</th><th>Meaning</th></tr></thead>
  <tbody>
    <tr><td><em>Enter an experience ID or link, then find a public server.</em></td><td>Nothing has been attempted yet.</td></tr>
    <tr><td><em>Enable Agentstrap Matchmaker before searching for servers.</em></td><td>You pressed search with the feature off.</td></tr>
    <tr><td><em>Enter a valid Roblox place ID or a roblox.com/games/{id} link.</em></td><td>The place field couldn't be parsed.</td></tr>
    <tr><td><em>No public server found with at least {n} open slot(s). Try a lower minimum or search again later.</em></td><td>The search ran but nothing qualified.</td></tr>
    <tr><td><em>Launch requested for a server with {playing}/{max} players.</em></td><td>Roblox was asked to join a specific server.</td></tr>
    <tr><td><em>Server search failed: {message}</em></td><td>Roblox's API returned an error or was unreachable.</td></tr>
  </tbody>
</table>
</div>

<h2>Configuration migration</h2>

<p>If you enabled the matchmaker in an older build but never set a place ID, Agentstrap's
<a href="updates.html">configuration migration</a> turns the matchmaker off for you rather than leaving it in a state
where every launch does nothing. The <strong>Updates</strong> page will show this as a pending migration you can
apply.</p>

<div class="callout warning">
  <strong>Old key names.</strong>
  The matchmaker settings are stored in <code>Settings.json</code> under the keys
  <code>VoidstrapMatchmakerEnabled</code>, <code>VoidstrapMatchmakerPlaceId</code>,
  <code>VoidstrapMatchmakerPreferLowPopulation</code> and <code>VoidstrapMatchmakerMinimumFreeSlots</code>.
  The names are legacy but are kept for compatibility with existing installations.
</div>
`,
  },

  {
    path: "features/command-center.html",
    title: "Command Center",
    lede: "The default settings page: launch Roblox, see what's going on, and jump to the tool you need.",
    body: `
<p>The Command Center is the first item in the settings sidebar and the page Agentstrap opens on. It exists to answer
three questions quickly: <em>can I launch right now?</em>, <em>is everything healthy?</em> and <em>where do I go
next?</em></p>

<div class="callout note">
  <strong>Where it lives.</strong>
  Settings &rarr; <strong>Command Center</strong>. It is the default landing page.
</div>

<h2>Launch</h2>

<div class="table-wrap">
<table>
  <thead><tr><th>Button</th><th>What it does</th></tr></thead>
  <tbody>
    <tr>
      <td><strong>Launch Roblox</strong></td>
      <td>
        Starts the Roblox Player through Agentstrap, showing the normal bootstrapper progress window. It
        re-registers the <code>roblox://</code> and <code>roblox-player://</code> protocol handlers, records the
        launch status, and may prompt for administrator rights.
      </td>
    </tr>
    <tr>
      <td><strong>Launch Roblox Studio</strong></td>
      <td>
        The same, for Roblox Studio. Studio launches skip the "confirm launches" prompt.
      </td>
    </tr>
  </tbody>
</table>
</div>

<div class="callout warning">
  <strong>Media Feature Pack.</strong>
  Launching Roblox needs the Windows Media Feature Pack. If <code>mfplat.dll</code> is missing, the launch fails
  with <em>"Launch failed &mdash; Windows Media Feature Pack (mfplat.dll) is missing."</em> Install
  <em>Media Feature Pack for Windows</em> from Windows Optional Features and try again.
</div>

<h2>Status</h2>

<p>Six read-only cards. They tell you the state of the install at a glance.</p>

<div class="table-wrap">
<table>
  <thead><tr><th>Card</th><th>Shows</th></tr></thead>
  <tbody>
    <tr>
      <td><strong>Roblox Player</strong></td>
      <td>
        <em>Installed</em> or <em>Not detected</em>, based on whether the player executable exists inside the current
        version folder. If it isn't installed yet, it gets installed the next time you launch.
      </td>
    </tr>
    <tr>
      <td><strong>Last launch</strong></td>
      <td>
        The outcome of the most recent launch &mdash; for example <em>Roblox process started</em>,
        <em>Client startup detected</em>, <em>Launch cancelled</em>, <em>Launch failed</em> or
        <em>Launch did not complete</em> &mdash; with the local time it happened. Defaults to
        <em>No launch recorded</em>.
      </td>
    </tr>
    <tr><td><strong>Agentstrap</strong></td><td>The running build version.</td></tr>
    <tr>
      <td><strong>NEXUS</strong></td>
      <td><em>Off</em>, or <em>On (channel)</em> when the experimental feature channel is enabled. See
          <a href="nexus.html">NEXUS mode</a>.</td>
    </tr>
    <tr>
      <td><strong>Process priority</strong></td>
      <td>The priority applied to the Roblox process &mdash; <em>Realtime</em>, <em>High</em>,
          <em>Above normal</em>, <em>Normal</em> or <em>Below normal</em>. Defaults to <em>Normal</em>.</td>
    </tr>
    <tr>
      <td><strong>Test mode</strong></td>
      <td>
        Whether test mode is on. Test mode is toggled from the status bar of the settings window, not from this page.
        It makes Agentstrap relaunch Roblox after settings close, and reopen settings after Roblox closes, in a loop.
      </td>
    </tr>
  </tbody>
</table>
</div>

<h2>Tools</h2>

<div class="table-wrap">
<table>
  <thead><tr><th>Button</th><th>What it does</th></tr></thead>
  <tbody>
    <tr><td><strong>Check for updates</strong></td><td>Jumps to the <a href="updates.html">Updates</a> page.</td></tr>
    <tr><td><strong>Run diagnostics</strong></td><td>Jumps to the <a href="diagnostics.html">Diagnostics</a> page.</td></tr>
    <tr><td><strong>Open logs</strong></td><td>Opens Agentstrap's log folder in Explorer.</td></tr>
    <tr><td><strong>Advanced</strong></td><td>Jumps to the <a href="nexus.html">Advanced</a> page.</td></tr>
    <tr>
      <td><strong>Refresh status</strong></td>
      <td>
        Re-checks whether the Roblox Player is installed. It deliberately only refreshes that one card &mdash; the
        launch, version, NEXUS, priority and test mode cards don't change.
      </td>
    </tr>
    <tr>
      <td><strong>Repair Roblox links</strong></td>
      <td>
        Re-registers the <code>roblox://</code> and <code>roblox-player://</code> protocol handlers in
        <code>HKCU\\Software\\Classes</code> so they point at Agentstrap with <code>-player "%1"</code>. Useful if
        clicking a Roblox link opens the wrong thing. No administrator rights needed.
      </td>
    </tr>
  </tbody>
</table>
</div>

<div class="callout note">
  <strong>Some of these are shortcuts, not actions.</strong>
  <em>Check for updates</em>, <em>Run diagnostics</em> and <em>Advanced</em> only navigate to another page &mdash; they
  don't start a check. Use <strong>Refresh status</strong> if you want the install card re-evaluated without
  leaving the page.
</div>
`,
  },

  {
    path: "features/engine-settings.html",
    title: "Engine settings",
    lede: "Tweak how Roblox renders, manage Fast Flags by hand, and catch mistakes before you launch.",
    body: `
<p>Roblox exposes a set of internal switches called <strong>Fast Flags</strong>. Roblox engineers use them to roll
out features without shipping a full update, but they can also be set locally, and a few of them control things
players genuinely want to change &mdash; anti-aliasing, texture quality, and whether display scaling degrades the
image.</p>

<div class="callout note">
  <strong>Where it lives.</strong>
  Settings &rarr; <strong>Engine Settings</strong>. The page is sometimes called <em>Fast Flags</em> in older builds.
</div>

<h2>Master switch</h2>

<p><strong>Allow Agentstrap to manage Fast Flags</strong> is on by default. Turning it off means nothing configured on
this page is applied to Roblox &mdash; the file is still written to your Modifications folder, it's just never copied
into the Roblox install.</p>

<h2>Rendering presets</h2>

<h3>Anti-aliasing quality (MSAA)</h3>

<div class="table-wrap">
<table>
  <thead><tr><th>Option</th><th>Effect</th></tr></thead>
  <tbody>
    <tr><td><strong>Automatic</strong></td><td>Roblox picks its own anti-aliasing level. This is the default.</td></tr>
    <tr><td><strong>1x</strong></td><td>No multi-sample anti-aliasing. Fastest, but edges look jagged.</td></tr>
    <tr><td><strong>2x</strong></td><td>Moderate smoothing.</td></tr>
    <tr><td><strong>4x</strong></td><td>Best edge quality, highest GPU cost.</td></tr>
  </tbody>
</table>
</div>

<h3>Texture quality</h3>

<div class="table-wrap">
<table>
  <thead><tr><th>Option</th><th>Effect</th></tr></thead>
  <tbody>
    <tr><td><strong>Automatic</strong></td><td>Roblox picks its own texture quality.</td></tr>
    <tr><td><strong>Level 0 (Lowest)</strong></td><td>Lowest memory usage and fastest texture uploads. Blurry textures.</td></tr>
    <tr><td><strong>Level 1</strong></td><td>Low.</td></tr>
    <tr><td><strong>Level 2</strong></td><td>High.</td></tr>
    <tr><td><strong>Level 3 (Highest)</strong></td><td>Best looking, most memory.</td></tr>
  </tbody>
</table>
</div>

<h3 id="preserving-rendering-quality">Preserve rendering quality with display scaling</h3>

<p>When your Windows display scaling is above 100%, Roblox reduces its rendering resolution to match. Enabling this
prevents that, so the game stays sharp on a high-DPI display.</p>

<div class="callout tip" id="exclusive-fullscreen">
  <strong>Exclusive fullscreen is always on.</strong>
  Roblox normally fullscreens in borderless windowed mode. Agentstrap always enables Direct3D exclusive fullscreen,
  so <kbd>Alt</kbd>+<kbd>Enter</kbd> gives you true fullscreen &mdash; which can be a small performance win, because
  the game gets your whole screen instead of a window.
</div>

<div class="callout warning">
  <strong>Exclusive fullscreen has trade-offs.</strong>
  You won't see server location notifications from Agentstrap, popout windows in the Roblox app (server browser,
  badges) stop working, and you may hit GPU quirks such as odd resolutions, VSync behaviour or a missing cursor.
  If any of that bites, fullscreen normally without <kbd>Alt</kbd>+<kbd>Enter</kbd>.
</div>

<h2>Preset bundles</h2>

<p>A preset bundle is one click that applies a whole set of flags at once. Pick one from the dropdown and press
<strong>Apply preset</strong>.</p>

<div class="table-wrap">
<table>
  <thead><tr><th>Bundle</th><th>What it sets</th></tr></thead>
  <tbody>
    <tr>
      <td><strong>Default</strong></td>
      <td>Clears the rendering preset flags and returns them to Roblox's own defaults.</td>
    </tr>
    <tr>
      <td><strong>Performance</strong></td>
      <td>Disables display scaling, forces 1x MSAA and the lowest texture quality. Favours frame rate.</td>
    </tr>
    <tr>
      <td><strong>Balanced</strong></td>
      <td>2x MSAA and one step below the highest texture quality. A middle ground.</td>
    </tr>
    <tr>
      <td><strong>Quality</strong></td>
      <td>4x MSAA and the highest texture quality. Favours visuals.</td>
    </tr>
  </tbody>
</table>
</div>

<p>Applying a bundle only touches the flags that bundle owns &mdash; your own custom flags are left alone. The
<strong>Default</strong> bundle is the exception: it clears the five preset flags as well.</p>

<p>The MSAA and texture quality dropdowns update immediately after you apply a bundle, so you can see exactly what
you ended up with.</p>

<h2>Validator</h2>

<p><strong>Validate fast flags</strong> checks every flag name and value against the format Roblox expects, and
reports anything wrong. It is read-only: it never changes or removes your flags.</p>

<p>Results appear in a monospace card:</p>

<div class="callout tip">
  <strong>All 24 fast flags are valid.</strong>
</div>

<div class="callout warning">
  <strong>2 of 24 fast flags look invalid:</strong>
  <br><code>  FFlagFoo = maybe (InvalidBoolValue)</code>
  <br><code>  DFFlagBar = 3 (InvalidNumberValue)</code>
</div>

<p>Only the first fifteen problems are listed; the rest are summarised as <em>"and N more"</em>.</p>

<h3>What counts as valid</h3>

<div class="table-wrap">
<table>
  <thead><tr><th>Rule</th><th>Detail</th></tr></thead>
  <tbody>
    <tr><td>Name prefix</td><td>Must start with <code>FFlag</code>, <code>DFFlag</code>, <code>SFFlag</code>, <code>FInt</code>, <code>DFInt</code>, <code>FString</code>, <code>DFString</code>, <code>FLog</code> or <code>DFLog</code>.</td></tr>
    <tr><td>Name characters</td><td>Letters, digits and underscores only.</td></tr>
    <tr><td>Place / data centre filters</td><td>
      If the name ends in <code>_PlaceFilter</code> or <code>_DataCenterFilter</code>, the value must be a
      semicolon-separated list in the format that flag type expects.
    </td></tr>
    <tr><td>Numeric values</td><td>Names starting <code>FInt</code> or <code>DFInt</code> must have an integer value.</td></tr>
    <tr><td>Boolean values</td><td>Names starting <code>FFlag</code> or <code>DFFlag</code> must have <code>True</code> or <code>False</code>.</td></tr>
  </tbody>
</table>
</div>

<h2>Fast Flag Editor</h2>

<p>At the bottom of the page is a link to the <strong>Fast Flag Editor</strong>, where you can add, edit, import and
export individual flags. It's only shown when Roblox Studio is installed, because custom flags mostly matter for
Studio.</p>

<div class="callout warning">
  <strong>Roblox's allowlist.</strong>
  Since September 30th 2025, Roblox only applies a whitelisted set of Fast Flags to the client. Most custom flags
  now have no effect on the Roblox <em>player</em>, even though the editor will happily accept them. Treat the editor
  as best-effort and don't expect every flag to do something.
</div>

<h3>Editor basics</h3>

<ul>
  <li><strong>Search</strong> filters by flag name only, case-insensitively. It does not match values.</li>
  <li><strong>Show preset flags</strong> reveals the flags Agentstrap manages for you. They're hidden by default.</li>
  <li><strong>Add new</strong> opens a dialog with two tabs: add a single flag by name and value, or import a block of
      JSON (pasted directly or loaded from a file).</li>
  <li><strong>Export JSON</strong> copies every flag to your clipboard as formatted JSON.</li>
  <li><strong>Delete selected</strong> removes the highlighted rows. Multi-select works.</li>
  <li>Double-click a cell to edit it. Editing the <strong>Value</strong> is validated; editing the <strong>Name</strong>
      is not, and only checks for duplicates.</li>
</ul>

<h3>Importing JSON</h3>

<p>Imports are handled carefully, because a bad config can make Roblox unstable:</p>

<ul>
  <li>More than 16 flags triggers a warning asking you to confirm.</li>
  <li>Flags that already have a value are reported, and you choose whether to overwrite them or keep the existing
      values.</li>
  <li>Invalid entries are skipped, and each one shows a message explaining why.</li>
  <li>Entries with a <code>null</code> value are ignored.</li>
  <li>Trailing commas and comments in the JSON are tolerated.</li>
</ul>

<h3>Resetting</h3>

<p><strong>Reset everything to defaults</strong> clears all flags. Flipping it back on restores what was there before
&mdash; but only if you haven't left the page in the meantime. Navigate away, come back, or apply a preset bundle, and
the undo is gone.</p>

<h2>When changes take effect</h2>

<p>Fast Flags are written to
<code>%LOCALAPPDATA%\\Agentstrap\\Modifications\\ClientSettings\\ClientAppSettings.json</code> and copied into the
Roblox install at launch. A running Roblox client will not pick them up, so <strong>restart Roblox</strong> after
saving.</p>

<div class="callout note">
  <strong>Your configuration survives Roblox updates.</strong>
  Because the flags live in the Modifications folder rather than inside the Roblox install, upgrading Roblox doesn't
  wipe them. If Roblox renames or removes a flag, it simply stops having an effect &mdash; Agentstrap doesn't migrate
  flags individually.
</div>

<h2>Troubleshooting</h2>

<div class="table-wrap">
<table>
  <thead><tr><th>Symptom</th><th>Likely cause</th></tr></thead>
  <tbody>
    <tr><td>My flag isn't in the editor list</td><td>Preset flags are hidden by default. Turn on <strong>Show preset flags</strong>.</td></tr>
    <tr><td>The flag is set but nothing changes</td><td>Roblox's Fast Flag allowlist. Most custom flags are ignored by the player.</td></tr>
    <tr><td>Editing a cell does nothing</td><td>Your UI language is translated, which breaks the editor's column matching. Use English to edit flags, or report it.</td></tr>
    <tr><td>"Your Fast Flags could not be loaded"</td><td>The stored JSON was corrupt. Agentstrap backs it up and starts fresh.</td></tr>
  </tbody>
</table>
</div>
`,
  },

  {
    path: "features/server-connection-overlay.html",
    title: "Server connection overlay",
    lede: "A toast on the Roblox window that tells you which server you joined and where its data centre is.",
    body: `
<p>When you connect to a Roblox server, Agentstrap can pop a small dark toast just inside the top of the game window
showing where you ended up. It's designed to be read at a glance and then get out of the way.</p>

<div class="callout tip">
  <strong>Where it lives.</strong>
  Settings &rarr; <strong>Integrations</strong> &rarr; <strong>Activity tracking</strong> section.
</div>

<h2>What it looks like</h2>

<pre><code>Dallas, Texas: Brookhaven RP
Chicago, Illinois: Adopt Me!</code></pre>

<p>The format is always <code>{City}, {State}: {Experience name}</code>. When information isn't available, the
overlay falls back gracefully:</p>

<div class="table-wrap">
<table>
  <thead><tr><th>Situation</th><th>Text shown</th></tr></thead>
  <tbody>
    <tr><td>Everything resolved</td><td><code>Dallas, Texas: Brookhaven RP</code></td></tr>
    <tr><td>Location unknown, name known</td><td><code>Unknown location: Adopt Me!</code></td></tr>
    <tr><td>Location unknown, name unknown</td><td><code>Unknown location: Roblox</code></td></tr>
  </tbody>
</table>
</div>

<div class="callout note">
  <strong>What it doesn't show.</strong>
  There's no player count, no country, no region code and no server ID. For those, open the
  <strong>Server information</strong> dialog from the tray menu &mdash; see
  <a href="server-location-hints.html">Server location hints</a>.
</div>

<h2>Options</h2>

<div class="table-wrap">
<table>
  <thead><tr><th>Option</th><th>Default</th><th>Detail</th></tr></thead>
  <tbody>
    <tr>
      <td><strong>Show server connection notifications</strong></td>
      <td><span class="badge on">On</span></td>
      <td>
        Needs <a href="activity-tracking.html">activity tracking</a> to be enabled. When it's off, the row is
        greyed out and the overlay doesn't exist at all.
      </td>
    </tr>
    <tr>
      <td><strong>Notification duration</strong></td>
      <td>5 seconds</td>
      <td>How long the toast stays on screen. You can set anything from 1 to 30 seconds.</td>
    </tr>
    <tr>
      <td><strong>Test notification</strong></td>
      <td>&mdash;</td>
      <td>
        Shows a preview of the toast over the Agentstrap settings window. It always uses the fallback text
        <em>"Unknown location: Roblox"</em> and makes no network requests, so it won't match a real notification
        exactly.
      </td>
    </tr>
  </tbody>
</table>
</div>

<h2>How it behaves</h2>

<ul>
  <li>
    <strong>It appears immediately.</strong> The toast is shown right away using whatever is already cached, then the
    text is patched in place once the lookups come back. Waiting for the network first could stall it for the full
    request timeout and make it look broken.
  </li>
  <li>
    <strong>It never steals focus.</strong> The overlay is a click-through, always-on-top window that doesn't take
    input, doesn't appear in the Alt+Tab list, and doesn't affect the game.
  </li>
  <li>
    <strong>It follows the game window.</strong> The position is re-tracked every quarter-second, so the toast stays
    centred if you move or resize the Roblox window.
  </li>
  <li>
    <strong>It's always dark.</strong> The toast ignores the Agentstrap theme on purpose, so it stays readable over
    any Roblox experience.
  </li>
  <li>
    <strong>Stale results are discarded.</strong> If you leave the server or join another one while lookups are still
    running, the late result is dropped rather than overwriting the newer one. A toast that has already faded out is
    not brought back.
  </li>
  <li>
    <strong>Repeats are suppressed.</strong> Joining the same server twice in a row doesn't produce a second toast.
  </li>
</ul>

<h2>Where the data comes from</h2>

<div class="table-wrap">
<table>
  <thead><tr><th>What</th><th>Source</th></tr></thead>
  <tbody>
    <tr><td>Experience name</td><td><code>games.roblox.com/v1/games</code></td></tr>
    <tr><td>City and state</td><td><code>ipinfo.io/{server-ip}/json</code></td></tr>
  </tbody>
</table>
</div>

<div class="callout warning">
  <strong>The location is the data centre, not you.</strong>
  ipinfo.io geolocates the game server's IP address, which for Roblox is a Google Cloud region. The city and state
  shown describe the server, not where you are sitting.
</div>

<div class="callout note">
  <strong>Private servers are never looked up.</strong>
  Addresses in private ranges aren't publicly queryable, so no request is made and the location stays unknown.
</div>

<h2>Requirements and caveats</h2>

<ul>
  <li>
    <strong>Windows only.</strong> The overlay uses Win32 window styles and DPI APIs.
  </li>
  <li>
    <strong>Activity tracking must be on.</strong> The overlay is built on the same watcher as activity tracking.
  </li>
  <li>
    <strong>No client patch required.</strong> Unlike Discord Rich Presence, the overlay reads Roblox's standard log
    output, so it works with an unmodified client.
  </li>
  <li>
    <strong>It depends on Roblox's logging.</strong> The code that detects a server join reads specific FLog lines
    from the Roblox log. Roblox can change its log verbosity at any time, and if it does, the overlay can silently
    stop appearing.
  </li>
  <li>
    <strong>Fullscreen games can hide it.</strong> In some exclusive fullscreen configurations, a separate top-level
    window can't composite over the game. Windowed and borderless modes are unaffected.
  </li>
  <li>
    <strong>Minimising Roblox hides it.</strong> The toast reappears when you restore the window.
  </li>
</ul>

<h2>Privacy</h2>

<p>Enabling the overlay means the game server's IP address is sent to ipinfo.io. Only the <em>server's</em> address is
queried &mdash; your own IP is never the subject of a lookup, though ipinfo.io naturally sees it as the connecting
client, exactly as with any web request.</p>

<div class="callout warning">
  <strong>Turning off "Query server location" does not stop the overlay's lookups.</strong>
  That switch only controls the tray notification, the prefetch and the Location row in the Server information
  dialog. The overlay does its own ipinfo.io lookup whenever it's enabled. If you need ipinfo.io to be contacted
  as little as possible, turn the overlay off too.
</div>

<p>Agentstrap's own log files record every HTTP request, including the full ipinfo.io URL with the server IP in it.
If you share logs when reporting a problem, those addresses go with them.</p>
`,
  },

  {
    path: "features/server-location-hints.html",
    title: "Server location hints",
    lede: "Work out roughly where a Roblox server's data centre is, and get the details on demand.",
    body: `
<p>When you join a server, Agentstrap can work out approximately where that server is hosted. The provider is
<a href="https://ipinfo.io" target="_blank" rel="noopener">ipinfo.io</a>, and the result is cached for the duration of
the session.</p>

<div class="callout note">
  <strong>Where it lives.</strong>
  Settings &rarr; <strong>Integrations</strong> &rarr; <strong>Query server location</strong>. Off by default.
  Requires <a href="activity-tracking.html">activity tracking</a>.
</div>

<h2>How you see it</h2>

<h3>The tray notification</h3>

<p>When you connect, Windows shows a balloon notification:</p>

<pre><code>Connected to public server
Location: Chicago, Illinois, US
Click for more information</code></pre>

<p>The title reflects the server type &mdash; <em>Connected to public server</em>,
<em>Connected to private server</em> or <em>Connected to reserved server</em>. The notification stays for ten
seconds, and clicking it opens the Server information dialog.</p>

<h3>The Server information dialog</h3>

<p>Open it from the Agentstrap tray icon's right-click menu while you're in a game.</p>

<div class="table-wrap">
<table>
  <thead><tr><th>Field</th><th>Shows</th></tr></thead>
  <tbody>
    <tr><td><strong>Type</strong></td><td>Public server, private server or reserved server.</td></tr>
    <tr><td><strong>Instance ID</strong></td><td>The Roblox job ID for the server you're in.</td></tr>
    <tr>
      <td><strong>Location</strong></td>
      <td>
        Only shown when server location querying is enabled. It starts at
        <em>"Loading, please wait..."</em> and becomes <code>City, Region, Country</code>, or
        <em>"Not available"</em> if the lookup failed.
      </td>
    </tr>
  </tbody>
</table>
</div>

<p>There's a <strong>Copy Instance ID</strong> button for when you need to share the server with someone. The dialog
closes itself when you leave the server.</p>

<h2>Why the location can be "Not available"</h2>

<ul>
  <li>The server's address isn't publicly queryable &mdash; private servers in particular are never looked up.</li>
  <li>ipinfo.io was unreachable or rate-limited. Agentstrap's free-tier limit is easy to hit if you join games very
      quickly; the error message says <em>"The server location could not be queried. You may be joining games too
      quickly."</em></li>
  <li>ipinfo.io didn't have a more precise answer than the region itself. Agentstrap would rather show nothing than
      claim <em>"Illinois, Illinois"</em>.</li>
</ul>

<div class="callout note">
  <strong>Failed lookups are cached too.</strong> Once a server's address fails to resolve, Agentstrap remembers
  that and doesn't keep retrying it.
</div>

<h2>Accuracy</h2>

<p>The location describes the <strong>server's data centre</strong>, not your physical position. Roblox servers run
in Google Cloud regions, so the city name tells you roughly which continent and region the game is running in. For
serverhopping purposes &mdash; finding a low-ping server &mdash; it's genuinely useful. For "where am I", it isn't.</p>

<h3>Related</h3>

<ul>
  <li><a href="server-connection-overlay.html">Server connection overlay</a> &mdash; a newer, in-game way to see the same
      information, on by default.</li>
  <li><a href="game-history.html">Game history</a> &mdash; rejoin a server you've left.</li>
</ul>
`,
  },

  {
    path: "features/game-history.html",
    title: "Game history",
    lede: "A session-only list of every game you've been in, with one-click rejoining.",
    body: `
<p>Need to go back to a server you just left? Game history keeps a record of the servers you've joined during the
current Roblox session and lets you rejoin them directly.</p>

<div class="callout note">
  <strong>Where it lives.</strong>
  Right-click the Agentstrap tray icon &rarr; <strong>Game history</strong>. The menu item is only present when
  activity tracking is enabled and <em>Don't exit to desktop app</em> is <strong>off</strong>.
</div>

<h2>What the list shows</h2>

<p>Each entry has a game icon, the game name, and a line describing it:</p>

<pre><code>Brookhaven RP
Builderman • 14:32 - 15:04</code></pre>

<p>For non-public servers, the server type is appended:</p>

<pre><code>Brookhaven RP
Builderman • 14:32 - 15:04 • Private server</code></pre>

<p>Each entry also has a green <strong>Rejoin</strong> button that relaunches Roblox and joins that exact
server.</p>

<h2>Session-only, on purpose</h2>

<div class="callout warning">
  <strong>Nothing is written to disk.</strong>
  Game history exists only in memory for the current Roblox session. Close Roblox and it's gone. If you're
  switching between games by closing Roblox each time, it won't help you.
</div>

<p>Entries appear as you leave a game or teleport within one. Teleports inside the same experience are folded into
the original entry rather than filling the list with near-duplicates, so a game that teleports you around a dozen
times still shows as one row with an accurate end time.</p>

<div class="callout note">
  <strong>Not every server is rejoinable.</strong>
  Reserved servers and some private servers can't be rejoined by deep link. The Rejoin button simply doesn't do
  anything for those.
</div>
`,
  },

  {
    path: "features/activity-tracking.html",
    title: "Activity tracking",
    lede: "The master switch that lets Agentstrap see what you're playing &mdash; and everything built on top of it.",
    body: `
<p>Activity tracking is how Agentstrap works out which game you're in and which server you joined. Several other
features are built on it, and it has to be on for any of them to work.</p>

<div class="callout note">
  <strong>Where it lives.</strong>
  Settings &rarr; <strong>Integrations</strong> &rarr; <strong>Activity tracking</strong>. On by default.
</div>

<h2>What it enables</h2>

<div class="table-wrap">
<table>
  <thead><tr><th>Feature</th><th>Needs activity tracking?</th></tr></thead>
  <tbody>
    <tr><td><a href="server-location-hints.html">Server location hints</a></td><td>Yes</td></tr>
    <tr><td><a href="server-connection-overlay.html">Server connection overlay</a></td><td>Yes</td></tr>
    <tr><td><a href="server-connection-push-notifications.html">Server connection push notifications</a></td><td>Yes</td></tr>
    <tr><td><a href="game-history.html">Game history</a></td><td>Yes</td></tr>
    <tr><td><a href="discord-rich-presence.html">Discord Rich Presence</a></td><td>Yes</td></tr>
    <tr><td><a href="dont-exit-to-desktop-app.html">Don't exit to desktop app</a></td><td>Yes</td></tr>
  </tbody>
</table>
</div>

<div class="callout warning">
  <strong>Turning it off turns other things off too.</strong>
  Disabling activity tracking immediately switches off server location, the connection overlay, server connection
  push notifications, the desktop-app patch and Discord Rich Presence, and greys out their rows.
</div>

<div class="callout danger">
  <strong>They don't come back automatically.</strong>
  Turn activity tracking back on and the other switches are still off. You have to re-enable each one by hand.
  This catches people out regularly, so check the Integrations page after switching tracking back on.
</div>

<h2>How it works</h2>

<p>Agentstrap starts a background <em>watcher</em> process while Roblox is running. The watcher tails Roblox's own log
file and reacts to specific entries, such as:</p>

<pre><code>[FLog::Output] ! Joining game
[FLog::Network] UDMUX Address = ...
[FLog::Network] Replicator created: ...
[FLog::SingleSurfaceApp] leaveUGCGameInternal</code></pre>

<p>From those it works out the game you're in, the server address, the server type and the session timings.</p>

<div class="callout warning">
  <strong>This is the fragile part of Agentstrap.</strong>
  The log entries Agentstrap depends on are not a supported interface &mdash; the code itself calls them
  "technically volatile". Roblox can change its logging behaviour at any time, and if it does, activity tracking
  stops working silently. If features that depend on it suddenly do nothing, check the Agentstrap log first.
</div>

<h2>Privacy</h2>

<p>Activity tracking is local: Agentstrap reads Roblox's log file on your machine. It does not report what you're
playing to anyone, with one exception &mdash; if you enable <em>Show Roblox account</em> in Discord Rich Presence, your
Roblox user ID and avatar are fetched from Roblox's public API so they can be displayed on your Discord profile.</p>

<p>Roblox's own API calls that Agentstrap makes are read-only lookups of public metadata.</p>
`,
  },

  {
    path: "features/server-connection-push-notifications.html",
    title: "Server connection push notifications",
    lede: "Get a Bark push on your iPhone when Agentstrap detects that you joined a Roblox server.",
    body: `
<p>Agentstrap can send a push notification with the server's approximate location each time you join a game server.
Notifications are delivered through <a href="https://bark.day.app/" target="_blank" rel="noopener">Bark</a> to
the Bark app on your iPhone. This is separate from the in-game server connection overlay and the Windows tray hint.</p>

<div class="callout tip">
  <strong>Where it lives.</strong>
  Open Settings and select <strong>Server Connection Notifications</strong>. Activity tracking must be enabled.
</div>

<h2>Set it up</h2>

<ol>
  <li>Install Bark on your iPhone, open it, and copy your device push key.</li>
  <li>In Agentstrap, paste the key into <strong>Bark push key</strong> and press <strong>Save credentials</strong>.</li>
  <li>Optionally enter an <a href="https://ipinfo.io/account/token" target="_blank" rel="noopener">IPinfo token</a>
      for more reliable city and region lookups. Without one, Agentstrap uses IPinfo's public quota.</li>
  <li>Turn on <strong>Send server connection notifications</strong> and press <strong>Save</strong> in Settings.</li>
  <li>Use <strong>Test notification</strong> to check Bark delivery. Use <strong>Test IPinfo connection</strong> to
      check the optional location token.</li>
</ol>

<p>Allow notifications for Bark in iOS. A successful test means Bark accepted the push request; the final alert is
delivered by Bark and iOS.</p>

<h2>Options</h2>

<div class="table-wrap">
<table>
  <thead><tr><th>Option</th><th>Default</th><th>What it does</th></tr></thead>
  <tbody>
    <tr><td><strong>Send server connection notifications</strong></td><td><span class="badge off">Off</span></td><td>Enables Bark pushes when a Roblox server join is detected.</td></tr>
    <tr><td><strong>Include the server's IP address</strong></td><td><span class="badge off">Off</span></td><td>Adds the remote game server IP to the notification body.</td></tr>
    <tr><td><strong>Duplicate suppression window</strong></td><td>30 minutes</td><td>Suppresses another push for the same server during this interval. Set it to 0 to disable suppression.</td></tr>
    <tr><td><strong>Detailed diagnostic logging</strong></td><td><span class="badge off">Off</span></td><td>Allows full remote server IP addresses in Agentstrap logs to help troubleshoot notifications.</td></tr>
  </tbody>
</table>
</div>

<h2>Location and notification contents</h2>

<p>When enabled, Agentstrap sends the <strong>game server's public IP address</strong> to ipinfo.io for a location
lookup. The lookup uses the game server's IP, not your own IP. If the lookup fails, the Bark push still goes out
without location details. If <strong>Include the server's IP address</strong> is on, that server IP is also included
in the push text.</p>

<p>Agentstrap sends the notification title and body and your Bark device key to Bark's API over HTTPS. The device key
and optional IPinfo token are stored encrypted for your Windows account with DPAPI; they aren't written to
<code>Settings.json</code> or included in Agentstrap logs. The notification uses the Agentstrap PNG as its Bark icon.
Bark's custom icon is supported on iOS 15 and later.</p>

<h2>When nothing arrives</h2>

<ul>
  <li>Make sure activity tracking and <strong>Send server connection notifications</strong> are both on.</li>
  <li>Check that the saved Bark key is current and that Bark is allowed to show notifications on your iPhone.</li>
  <li>Try <strong>Test notification</strong>. The status below the button reports whether Bark accepted the request.</li>
  <li>If notifications arrive without a city or region, test the IPinfo token and check whether its plan includes
      location fields.</li>
</ul>

<p>For details on network requests and stored credentials, see the <a href="../info/privacy-policy.html">Privacy
policy</a>.</p>
`,
  },
];
