export default [
  {
    path: "help/index.html",
    title: "Help",
    lede: "Step-by-step fixes for the problems people actually hit.",
    body: `
<p>Most problems with Agentstrap come down to one of a handful of causes. Start here and work down the list.</p>

<div class="card-grid">
  <a class="card" href="troubleshooting.html">
    <span class="card-tag">help</span>
    <h3>Agentstrap isn't working</h3>
    <p>Nothing happens, or launches fail. The most common causes, in order.</p>
  </a>
  <a class="card" href="roblox-crashes.html">
    <span class="card-tag">help</span>
    <h3>Roblox crashes or won't launch</h3>
    <p>The client starts and immediately closes, or never opens at all.</p>
  </a>
  <a class="card" href="repairing-roblox-install.html">
    <span class="card-tag">help</span>
    <h3>Repairing a broken Roblox install</h3>
    <p>Force a clean reinstall of the Roblox client through Agentstrap.</p>
  </a>
  <a class="card" href="server-location-unavailable.html">
    <span class="card-tag">help</span>
    <h3>Server location unavailable</h3>
    <p>Why the location lookup fails, and what you can do about it.</p>
  </a>
  <a class="card" href="discord-rich-presence-not-working.html">
    <span class="card-tag">help</span>
    <h3>Discord Rich Presence not working</h3>
    <p>Your game isn't showing up on your Discord profile.</p>
  </a>
  <a class="card" href="change-install-location.html">
    <span class="card-tag">help</span>
    <h3>Change the install location</h3>
    <p>Move Agentstrap to a different drive or folder.</p>
  </a>
  <a class="card" href="uninstalling.html">
    <span class="card-tag">help</span>
    <h3>Uninstalling Agentstrap</h3>
    <p>Remove it cleanly, including the parts outside the install folder.</p>
  </a>
</div>

<h2>The fastest first step</h2>

<div class="callout tip">
  <strong>Run diagnostics.</strong>
  Settings &rarr; <strong>Diagnostics</strong> &rarr; <strong>Run diagnostics</strong>. It checks eight things at once and
  tells you which one is wrong. Press <strong>Copy report</strong> and paste it into a bug report if you need help.
</div>

<h2>Before you file a bug</h2>

<ol class="steps">
  <li>Run <a href="../features/diagnostics.html">Diagnostics</a> and copy the report.</li>
  <li>Open the log folder from the same page and attach the most recent log file.</li>
  <li>Check the <a href="../features/updates.html">Updates</a> page for pending configuration migrations and apply them.</li>
  <li>Confirm you're on the latest release &mdash; the <strong>Releases</strong> button opens the download page.</li>
</ol>

<div class="callout note">
  <strong>Logs contain server addresses.</strong>
  Agentstrap's logs record every HTTP request, including the ipinfo.io URLs with the game server's IP address in them.
  Read a log before you attach it to a public issue.
</div>
`,
  },

  {
    path: "help/troubleshooting.html",
    title: "Agentstrap isn't working",
    lede: "Nothing happens, launches fail, or features do nothing. Work through these in order.",
    body: `
<p>These are the failures people report most often, ordered by how often they turn out to be the cause.</p>

<h2>1. Nothing happens when you click Launch</h2>

<div class="table-wrap">
<table>
  <thead><tr><th>Check</th><th>How</th></tr></thead>
  <tbody>
    <tr>
      <td><strong>Is Windows asking for administrator rights and failing?</strong></td>
      <td>Accept the UAC prompt. If it can't be shown, Agentstrap can't install Roblox.</td>
    </tr>
    <tr>
      <td><strong>Is the Media Feature Pack installed?</strong></td>
      <td>
        Roblox needs <code>mfplat.dll</code>. Without it the launch fails with
        <em>"Launch failed &mdash; Windows Media Feature Pack (mfplat.dll) is missing."</em> Install
        <em>Media Feature Pack for Windows</em> from Windows Optional Features.
      </td>
    </tr>
    <tr>
      <td><strong>Is the protocol handler registered?</strong></td>
      <td>
        Command Center &rarr; <strong>Repair Roblox links</strong>. This re-registers <code>roblox://</code> and
        <code>roblox-player://</code> without needing administrator rights.
      </td>
    </tr>
    <tr>
      <td><strong>Is Roblox actually installed?</strong></td>
      <td>
        The Command Center's <strong>Roblox Player</strong> card says <em>Installed</em> or <em>Not detected</em>.
        If it isn't installed, the next launch installs it &mdash; which needs internet access.
      </td>
    </tr>
  </tbody>
</table>
</div>

<h2>2. The launcher opens but Roblox never appears</h2>

<ul>
  <li>Wait for the download. Roblox can be a large download on a slow connection.</li>
  <li>Check the Command Center's <strong>Last launch</strong> card. It distinguishes <em>Launch cancelled</em>,
      <em>Launch failed</em> and <em>Launch did not complete</em>.</li>
  <li>Check the Roblox install folder for <code>RobloxPlayerBeta.exe</code>. If it's missing, see
      <a href="repairing-roblox-install.html">Repairing a broken Roblox install</a>.</li>
  <li>If antivirus or a corporate policy is blocking the launch, Agentstrap's log will record it.</li>
</ul>

<h2>3. A feature does nothing</h2>

<div class="table-wrap">
<table>
  <thead><tr><th>Symptom</th><th>Likely cause</th></tr></thead>
  <tbody>
    <tr>
      <td>Server location, overlay, game history or Discord presence all dead</td>
      <td>
        <a href="../features/activity-tracking.html">Activity tracking</a> is off. It's the master switch for all of
        them.
      </td>
    </tr>
    <tr>
      <td>Features work, then stop after you toggle activity tracking</td>
      <td>
        Turning activity tracking off switches the dependent features off. Turning it back on does <strong>not</strong>
        turn them back on &mdash; re-enable each one.
      </td>
    </tr>
    <tr>
      <td>Fast Flags have no effect</td>
      <td>
        The client was already running. Flags are applied at launch, so restart Roblox. Also check that
        <em>Allow Agentstrap to manage Fast Flags</em> is on.
      </td>
    </tr>
    <tr>
      <td>Fast Flags have no effect at all, in any version</td>
      <td>
        Roblox's September 2025 allowlist. Most custom flags are simply ignored by the player now. See
        <a href="../features/engine-settings.html">Engine settings</a>.
      </td>
    </tr>
    <tr>
      <td>Mods disappeared</td>
      <td>
        The Modifications folder was cleared, or Roblox was reinstalled in a way that bypassed the copy step. See
        <a href="../features/modding.html">Modding</a>.
      </td>
    </tr>
    <tr>
      <td>The overlay never appears</td>
      <td>
        Activity tracking off, or the game is in exclusive fullscreen, or Roblox's logging changed. See
        <a href="../features/server-connection-overlay.html">Server connection overlay</a>.
      </td>
    </tr>
    <tr>
      <td>Matchmaker does nothing</td>
      <td>
        The matchmaker is off, or no place ID is set, or no public server met your minimum free slots. See
        <a href="../features/matchmaker.html">Matchmaker</a>.
      </td>
    </tr>
  </tbody>
</table>
</div>

<h2>4. Settings won't save</h2>

<ul>
  <li>You have to press <strong>Save</strong> in the bottom-right of the settings window. Nothing is written to disk
      until you do.</li>
  <li>If the window is closing without a prompt, the save already succeeded.</li>
  <li>If <code>Settings.json</code> is corrupt, Agentstrap falls back to defaults and says so. The
      <a href="../features/diagnostics.html">Diagnostics</a> page flags this.</li>
</ul>

<h2>5. The interface looks broken</h2>

<ul>
  <li><strong>Try the plain theme.</strong> A custom theme file that no longer exists is a known issue &mdash;
      Diagnostics reports it as an invalid setting. Remove the custom theme, or use
      <em>Settings &rarr; Appearance &rarr; Delete</em>.</li>
  <li><strong>Reset to defaults.</strong> The Advanced page has a reset for NEXUS, but for a full reset, delete
      <code>Settings.json</code> from the install folder and restart Agentstrap. You will lose your preferences.</li>
  <li><strong>Try English.</strong> Several newly added pages still use hardcoded English. If the interface looks
      half-translated, that's why.</li>
</ul>

<h2>6. Collecting more information</h2>

<p>Agentstrap writes a log for every session. The Advanced page has <strong>Open logs</strong> and
<strong>Clear logs</strong> &mdash; clearing them first gives you a clean log with only the failing session in it.</p>

<div class="callout tip">
  <strong>One request before you post.</strong>
  Copy the diagnostics report and check it yourself first. Nine times out of ten it names the exact check that failed.
</div>
`,
  },

  {
    path: "help/roblox-crashes.html",
    title: "Roblox crashes or won't launch",
    lede: "The client starts and immediately closes, or never opens at all.",
    body: `
<h2>The client opens and closes straight away</h2>

<p>This is almost always a bad Fast Flag. A flag that Roblox's client can't handle will take the process down
immediately.</p>

<ol class="steps">
  <li>
    <strong>Turn off Fast Flag management.</strong>
    Settings &rarr; <strong>Engine Settings</strong> &rarr; uncheck <em>Allow Agentstrap to manage Fast Flags</em>,
    then press <strong>Save</strong>.
  </li>
  <li>
    <strong>Restart Roblox.</strong> If it launches cleanly, one of your flags is the culprit.
  </li>
  <li>
    <strong>Add your flags back one at a time.</strong> Use the Fast Flag Editor's <strong>Validate</strong> action to
    catch malformed ones.
  </li>
</ol>

<div class="callout note">
  <strong>Your flags aren't deleted.</strong> Turning off management only stops Agentstrap copying them into the Roblox
  install. Your configuration is still there, and you can turn management back on once you've found the problem.
</div>

<h2>The client never opens at all</h2>

<ul>
  <li><strong>Check the launch status.</strong> Command Center &rarr; <strong>Last launch</strong> tells you whether
      the launch was cancelled, failed, or never completed.</li>
  <li><strong>Check the Roblox install.</strong> If <code>RobloxPlayerBeta.exe</code> is missing from the version
      folder, see <a href="repairing-roblox-install.html">Repairing a broken Roblox install</a>.</li>
  <li><strong>Check for the Media Feature Pack.</strong> Roblox needs <code>mfplat.dll</code>.</li>
  <li><strong>Check for a pending configuration migration.</strong> A stale configuration can stop the client from
      starting. <a href="../features/updates.html">Updates</a> &rarr; <strong>Configuration</strong> &rarr;
      <strong>Check</strong>.</li>
</ul>

<h2>The client crashes after a while</h2>

<ul>
  <li><strong>Turn off exclusive fullscreen.</strong> Some graphics drivers handle it badly. Don't use
      <kbd>Alt</kbd>+<kbd>Enter</kbd> to go fullscreen.</li>
  <li><strong>Turn off custom window attributes.</strong> A fixed window size can exceed your display's usable
      area.</li>
  <li><strong>Disable mods.</strong> Set every preset back to <em>Default</em> on the Mods page and confirm the game
      is stable before re-adding them.</li>
  <li><strong>Update your graphics driver.</strong> Roblox is far more stable on current drivers.</li>
</ul>

<h2>The client crashes on a specific game only</h2>

<p>That's usually the game's own content, not Agentstrap. But if the crash happens before the game even loads its
assets, check the same list as above &mdash; especially Fast Flags.</p>

<h2>Getting the actual error</h2>

<p>Agentstrap's log records the exit and the reason. On the Advanced page, use <strong>Clear logs</strong> and then
reproduce the crash, so the log contains only that attempt.</p>

<p>Roblox keeps its own logs too, in <code>%LOCALAPPDATA%\\Roblox\\logs</code>. The Deployment page has an
<strong>Open Roblox logs</strong> shortcut.</p>

<div class="callout warning">
  <strong>Check the Roblox server status.</strong>
  If nothing you change makes a difference, Roblox's own status page may be the answer. A client that can't reach
  Roblox's servers looks exactly like a crash.
</div>
`,
  },

  {
    path: "help/repairing-roblox-install.html",
    title: "Repairing a broken Roblox install",
    lede: "Force a clean reinstall of the Roblox client through Agentstrap.",
    body: `
<p>Sometimes the Roblox install gets into a state where the client is missing, half-extracted, or fails to start no
matter what you change. Agentstrap can force a clean reinstall.</p>

<h2>Force a reinstall</h2>

<ol class="steps">
  <li>
    <strong>Turn on forced reinstall.</strong>
    Settings &rarr; <strong>Bootstrapper</strong> &rarr; <em>Force reinstall</em>, then press <strong>Save</strong>.
  </li>
  <li>
    <strong>Launch Roblox.</strong>
    Use Command Center &rarr; <strong>Launch Roblox</strong>.
  </li>
  <li>
    <strong>Wait for it to finish.</strong>
    The Roblox package is downloaded again and extracted over the existing files.
  </li>
  <li>
    <strong>Check the result.</strong>
    Command Center &rarr; <strong>Refresh status</strong> re-checks whether the player executable is present.
  </li>
</ol>

<div class="callout note">
  <strong>The flag is one-shot.</strong>
  Forced reinstall applies to the next launch and then resets itself, so you don't have to remember to turn it back off.
</div>

<h2>When that's not enough</h2>

<p>If a forced reinstall doesn't fix it, the install folder itself is probably damaged. Delete the version folder and let
Agentstrap download Roblox again from scratch.</p>

<pre><code>%LOCALAPPDATA%\\Agentstrap\\Versions\\</code></pre>

<p>Close Roblox and Agentstrap first, then delete the contents of that folder. The next launch downloads and
extracts a fresh copy. This does not touch your settings or your mods, because those live outside the Versions
folder.</p>

<div class="callout warning">
  <strong>This needs a working internet connection and some disk space.</strong>
  A full Roblox download is a few hundred megabytes.
</div>

<h2>Deleting the whole install</h2>

<p>If you want a completely clean slate, delete the whole <code>%LOCALAPPDATA%\\Agentstrap</code> folder and run the
installer again. You will lose your settings and mods. See <a href="uninstalling.html">Uninstalling Agentstrap</a>.</p>

<h2>Confirming the repair worked</h2>

<p>Run <a href="../features/diagnostics.html">Diagnostics</a> afterwards. The <em>Roblox installation</em> and
<em>Roblox version state</em> checks should both come back clean.</p>
`,
  },

  {
    path: "help/server-location-unavailable.html",
    title: "Server location unavailable",
    lede: "Why the location lookup fails, and what you can do about it.",
    body: `
<p>When the server location shows <em>"Not available"</em>, one of three things is true.</p>

<h2>1. The server's address can't be looked up</h2>

<p>Not every Roblox server has a publicly resolvable address. Private servers in particular are never queried at all,
because the addresses aren't publicly routable. There's nothing to fix &mdash; the location simply isn't knowable.</p>

<p>The same applies to any address in a private range. Agentstrap skips the request entirely rather than sending
something pointless.</p>

<h2>2. ipinfo.io was rate-limited</h2>

<p>This is the most common cause. ipinfo.io's free tier limits how many lookups you can do per month, and joining
games quickly burns through it.</p>

<p>The error Agentstrap shows says it directly: <em>"The server location could not be queried. You may be joining games
too quickly."</em></p>

<p>Things that help:</p>

<ul>
  <li>Turn off <em>Show server connection notifications</em> on the Integrations page. The overlay does its own lookup
      for every server you join, and it's the biggest source of requests.</li>
  <li>Wait. The limit resets over time.</li>
  <li>Don't join games as fast as you can.</li>
</ul>

<div class="callout note">
  <strong>Failed lookups are cached.</strong>
  Once an address fails to resolve, Agentstrap remembers that and doesn't keep retrying it. So a server that failed
  won't suddenly start working later in the session.
</div>

<h2>3. ipinfo.io only knew the region</h2>

<p>ipinfo.io sometimes doesn't have a city for an address &mdash; only a country or region. Agentstrap would rather show
nothing than claim <em>"Illinois, Illinois"</em>, so it shows <em>"Not available"</em> instead.</p>

<p>This one is outside your control.</p>

<h2>Nothing is wrong</h2>

<p>It's worth checking that the lookup is actually enabled. Settings &rarr; <strong>Integrations</strong> &rarr;
<em>Query server location</em>. It also requires
<a href="../features/activity-tracking.html">activity tracking</a> to be on.</p>

<div class="callout warning">
  <strong>The overlay has its own lookup.</strong>
  If the tray notification says "Not available" but the overlay shows a city, that's expected &mdash; the overlay
  resolves locations independently.
</div>

<h2>Where to look next</h2>

<p>Agentstrap's log records the exact request and the response it got back, including the error message from
ipinfo.io. Open the log folder from the Command Center or the Advanced page and look for the <code>ipinfo</code>
entries around the time you joined the server.</p>
`,
  },

  {
    path: "help/discord-rich-presence-not-working.html",
    title: "Discord Rich Presence not working",
    lede: "Your game isn't showing up on your Discord profile.",
    body: `
<p>Work through these in order &mdash; the first two cause most of the failures.</p>

<h2>1. Is the Discord desktop app running?</h2>

<p>Rich Presence needs the Discord <strong>desktop app</strong>, not the mobile app, not the browser version, and not
the game console. If Discord isn't running when you launch Roblox, nothing appears.</p>

<p>If Discord is running but was started <em>after</em> Roblox, it usually still works &mdash; but restarting Discord
is a quick thing to try.</p>

<h2>2. Is activity tracking enabled?</h2>

<p>Discord Rich Presence is built on top of
<a href="../features/activity-tracking.html">activity tracking</a>. If that's off, the presence never updates.</p>

<p>Settings &rarr; <strong>Integrations</strong> &rarr; <em>Enable activity tracking</em>.</p>

<div class="callout danger">
  <strong>Watch out for this one.</strong>
  Turning activity tracking off also switches off Discord Rich Presence. Turning it back on does <strong>not</strong>
  switch Rich Presence back on &mdash; you have to re-enable <em>Show game activity</em> by hand.
</div>

<h2>3. Is "Show game activity" on?</h2>

<p>It's a separate switch, on by default, but it gets turned off along with everything else when activity tracking is
disabled.</p>

<h2>4. Check Discord's own settings</h2>

<ul>
  <li>Open Discord &rarr; <strong>User Settings</strong> &rarr; <strong>Activities</strong> &rarr; <strong>Roblox</strong>.
      Make sure the toggle is on and the visibility isn't set to <em>Nobody</em>.</li>
  <li>Some Discord privacy settings block Rich Presence entirely. Try setting your status to <em>Online</em> rather
      than <em>Invisible</em>.</li>
  <li>Try running Discord as administrator. On some systems, a non-elevated Discord can't see an elevated
      Roblox process.</li>
</ul>

<h2>5. Is the presence stale?</h2>

<p>Roblox's own Rich Presence integration can conflict with Agentstrap's. If you have Roblox's built-in Discord
connection enabled in Roblox's settings, turn it off &mdash; otherwise both try to publish the same presence and
Discord picks one arbitrarily.</p>

<h2>6. The join button is missing</h2>

<p>Join buttons come from <em>Allow activity joining</em> on the Integrations page, and they're only offered for
<strong>public</strong> servers. Private and reserved servers can't be joined through a deeplink, so the button is
hidden.</p>

<h2>Nothing worked</h2>

<p>Discord's own logs are in <code>%APPDATA%\\discord\\logs</code>. They say whether a connection was attempted and
what Discord thought of it. Agentstrap's own log says whether it tried to publish at all &mdash; if there's no
corresponding entry, the problem is upstream in activity tracking.</p>
`,
  },

  {
    path: "help/change-install-location.html",
    title: "Change the install location",
    lede: "Move Agentstrap to a different drive or folder.",
    body: `
<h2>Moving the install</h2>

<p>Agentstrap's install location is set when you first run the installer, and it can be changed afterwards.</p>

<ol class="steps">
  <li>
    <strong>Close Agentstrap completely.</strong>
    Check the system tray &mdash; right-click the Agentstrap icon and choose <em>Exit</em>. If a Roblox process is
    running, close it too.
  </li>
  <li>
    <strong>Move the folder.</strong>
    Cut <code>%LOCALAPPDATA%\\Agentstrap</code> and paste it to its new home, for example
    <code>D:\\Apps\\Agentstrap</code>.
  </li>
  <li>
    <strong>Run Agentstrap from the new location.</strong>
    The first launch from the new path updates the shortcuts, the registry entries and the file paths Agentstrap
    stores.
  </li>
</ol>

<div class="callout warning">
  <strong>Copy, don't move, the first time.</strong>
  Run Agentstrap from the new location and confirm everything works before you delete the old folder. If something's
  broken, you still have a working install.
</div>

<h2>What has to be updated</h2>

<p>Moving the folder breaks a few absolute paths. Agentstrap repairs most of them on first launch, but these need
checking:</p>

<div class="table-wrap">
<table>
  <thead><tr><th>What</th><th>How to fix it</th></tr></thead>
  <tbody>
    <tr>
      <td>Start Menu and desktop shortcuts</td>
      <td>Settings &rarr; <strong>Shortcuts</strong>. Toggle each one off and on again to rewrite them.</td>
    </tr>
    <tr>
      <td>The <code>roblox://</code> protocol handler</td>
      <td>Command Center &rarr; <strong>Repair Roblox links</strong>.</td>
    </tr>
    <tr>
      <td>The uninstall entry in Windows</td>
      <td>Re-run the installer from the new location, or remove the old entry from <em>Settings &rarr; Apps</em>.</td>
    </tr>
    <tr>
      <td>Custom mods and Fast Flags</td>
      <td>They live in <code>Modifications</code> inside the folder, so they move with it.</td>
    </tr>
  </tbody>
</table>
</div>

<h2>A cleaner option</h2>

<p>If you'd rather start fresh than move files around, delete <code>%LOCALAPPDATA%\\Agentstrap</code> entirely and
run the installer again. The installer asks where you want it to go. You lose your settings and mods, but everything
ends up consistent.</p>

<p>See <a href="uninstalling.html">Uninstalling Agentstrap</a> for the full removal procedure.</p>
`,
  },

  {
    path: "help/uninstalling.html",
    title: "Uninstalling Agentstrap",
    lede: "Remove it cleanly, including the parts that live outside the install folder.",
    body: `
<h2>Uninstall</h2>

<ol class="steps">
  <li>
    <strong>Exit Agentstrap.</strong>
    Right-click the tray icon and choose <em>Exit</em>. Close any running Roblox processes too.
  </li>
  <li>
    <strong>Remove it from Windows.</strong>
    Go to <em>Settings &rarr; Apps &rarr; Installed apps</em>, find <strong>Agentstrap</strong>, and uninstall it.
  </li>
  <li>
    <strong>Delete the leftovers.</strong>
    The uninstaller doesn't remove everything. Delete <code>%LOCALAPPDATA%\\Agentstrap</code> if it's still there.
  </li>
</ol>

<h2>What's left behind</h2>

<p>Some things live outside the install folder, and the uninstaller won't touch them.</p>

<div class="table-wrap">
<table>
  <thead><tr><th>Path or key</th><th>What it is</th></tr></thead>
  <tbody>
    <tr><td><code>%LOCALAPPDATA%\\Agentstrap</code></td><td>The whole install: settings, mods, logs, Roblox installs.</td></tr>
    <tr><td><code>%TEMP%\\Agentstrap</code></td><td>Temporary downloads and in-progress updates.</td></tr>
    <tr><td><code>%LOCALAPPDATA%\\Roblox</code></td><td>Roblox's own data folder. Safe to delete &mdash; Roblox recreates it.</td></tr>
    <tr><td><code>HKCU\\Software\\Classes\\roblox</code></td><td>The protocol handler that makes Roblox links open Agentstrap.</td></tr>
    <tr><td><code>HKCU\\Software\\Classes\\roblox-player</code></td><td>The same, for the player-specific scheme.</td></tr>
    <tr><td><code>HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Agentstrap</code></td><td>The uninstall entry, if it's still there.</td></tr>
    <tr><td><code>HKCU\\SOFTWARE\\ROBLOX Corporation\\Environments\\RobloxPlayer\\Channel\\www.roblox.com</code></td><td>The deployment channel value.</td></tr>
    <tr><td>Start Menu and desktop shortcuts</td><td>Point at the old executable path.</td></tr>
  </tbody>
</table>
</div>

<div class="callout tip">
  <strong>Registry cleanup in one command.</strong>
  Run PowerShell as administrator and paste:
  <code>Remove-Item -Path "HKCU:\Software\Classes\roblox", "HKCU:\Software\Classes\roblox-player" -Recurse -ErrorAction SilentlyContinue</code>
</div>

<h2>Getting back to plain Roblox</h2>

<p>After removing Agentstrap, the <code>roblox://</code> links stop working until Roblox's own installer registers
them again. Run Roblox's official installer once, and it will restore the standard handler.</p>

<p>Roblox itself is not removed by uninstalling Agentstrap &mdash; the client files live in
<code>%LOCALAPPDATA%\\Agentstrap\\Versions</code>, so deleting that folder does remove them. Roblox will be
reinstalled the next time you launch it through the official installer.</p>

<div class="callout note">
  <strong>Keep the folder if you're not sure.</strong>
  Renaming <code>%LOCALAPPDATA%\\Agentstrap</code> to <code>Agentstrap.backup</code> is reversible. Delete it for
  good once you're satisfied.
</div>
`,
  },
];
