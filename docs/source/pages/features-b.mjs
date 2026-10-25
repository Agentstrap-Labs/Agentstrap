export default [
  {
    path: "features/discord-rich-presence.html",
    title: "Discord Rich Presence",
    lede: "Show the game you're playing on your Discord profile, and let friends join it from there.",
    body: `
<p>Agentstrap publishes your current Roblox game to Discord, so anyone looking at your profile can see what you're
playing and jump into it.</p>

<div class="callout note">
  <strong>Where it lives.</strong>
  Settings &rarr; <strong>Integrations</strong> &rarr; <strong>Discord Rich Presence</strong>.
</div>

<h2>Requirements</h2>

<ul>
  <li>Activity tracking must be enabled. See <a href="activity-tracking.html">Activity tracking</a>.</li>
  <li>The Discord desktop app must be installed and running. The mobile app doesn't work.</li>
</ul>

<h2>Options</h2>

<div class="table-wrap">
<table>
  <thead><tr><th>Option</th><th>Default</th><th>What it does</th></tr></thead>
  <tbody>
    <tr>
      <td><strong>Show game activity</strong></td>
      <td><span class="badge on">On</span></td>
      <td>Publishes the Roblox game you're playing to your Discord profile. Turning this off also turns off the two
          options below.</td>
    </tr>
    <tr>
      <td><strong>Allow activity joining</strong></td>
      <td><span class="badge on">On</span></td>
      <td>Shows the join buttons on your Discord profile so friends can enter the game you're in.</td>
    </tr>
    <tr>
      <td><strong>Show Roblox account</strong></td>
      <td><span class="badge off">Off</span></td>
      <td>
        Also shows which Roblox account you're playing with, along with its avatar. This fetches your Roblox user ID
        and profile picture from Roblox's public API.
      </td>
    </tr>
  </tbody>
</table>
</div>

<h2>Extra details on your profile</h2>

<p>Depending on what you're playing, your Discord presence can show more than just the game name:</p>

<ul>
  <li>The number of players currently in the server.</li>
  <li>The server's location, when server location querying is enabled.</li>
  <li>Join buttons, when activity joining is on.</li>
</ul>

<p>The "Copy invite deeplink" item in the Agentstrap tray menu gives you a link to the server you're in, but only
while you're in a <strong>public</strong> server.</p>

<h2>If it isn't working</h2>

<p>See <a href="../help/discord-rich-presence-not-working.html">Discord Rich Presence not working</a> for a
step-by-step fix.</p>
`,
  },

  {
    path: "features/custom-integrations.html",
    title: "Custom integrations",
    lede: "Launch other programs automatically when Roblox starts, and close them when it stops.",
    body: `
<p>Custom integrations let you hook other programs onto Roblox launches. A common use is running a companion app, a
voice tool or a chat client alongside the game, without having to start it yourself every time.</p>

<div class="callout note">
  <strong>Where it lives.</strong>
  Settings &rarr; <strong>Integrations</strong> &rarr; <strong>Custom Integrations</strong>.
</div>

<h2>Fields</h2>

<div class="table-wrap">
<table>
  <thead><tr><th>Field</th><th>Meaning</th></tr></thead>
  <tbody>
    <tr>
      <td><strong>Name</strong></td>
      <td>A label for the entry, shown in the list. New entries start as "New Integration".</td>
    </tr>
    <tr>
      <td><strong>Application Location</strong></td>
      <td>
        The full path to the program to launch, for example
        <code>C:\\Windows\\System32\\cmd.exe</code>. There's a <strong>Browse</strong> button next to it so you don't
        have to type paths.
      </td>
    </tr>
    <tr>
      <td><strong>Launch Arguments</strong></td>
      <td>
        Any command line arguments to pass. The placeholder is <em>"Roblox is running!"</em>. Arguments can span
        multiple lines.
      </td>
    </tr>
    <tr>
      <td><strong>Auto close when Roblox closes</strong></td>
      <td>
        When checked, Agentstrap closes the launched process when Roblox exits. Leave it off for programs that are
        meant to keep running.
      </td>
    </tr>
  </tbody>
</table>
</div>

<h2>Adding and removing</h2>

<ul>
  <li><strong>New</strong> adds an entry named "New Integration" and selects it for editing.</li>
  <li><strong>Delete</strong> removes the selected entry. It's disabled when nothing is selected.</li>
  <li>Select an entry in the list to edit its fields. The list shows the name of each integration.</li>
</ul>

<div class="callout note">
  <strong>Integrations launch alongside Roblox.</strong>
  They start when Agentstrap launches Roblox, not when you open the settings window.
</div>
`,
  },

  {
    path: "features/window-attributes.html",
    title: "Custom window attributes",
    lede: "Force a fixed size, screen position and window title on the Roblox window every time it opens.",
    body: `
<p>By default the Roblox window uses whatever size and position it last had. With custom window attributes enabled,
Agentstrap applies a fixed size, position and title to the window as soon as it appears.</p>

<div class="callout note">
  <strong>Where it lives.</strong>
  Settings &rarr; <strong>Quality of Life</strong> &rarr; <strong>Custom window attributes</strong>.
</div>

<h2>Options</h2>

<div class="table-wrap">
<table>
  <thead><tr><th>Option</th><th>Default</th><th>Detail</th></tr></thead>
  <tbody>
    <tr>
      <td><strong>Enable custom window attributes</strong></td>
      <td><span class="badge off">Off</span></td>
      <td>Applies the values below to the Roblox player window once it opens. You can fill the fields in ahead of time
          even while this is off.</td>
    </tr>
    <tr>
      <td><strong>Window size</strong></td>
      <td>Empty</td>
      <td>
        Width and height in pixels, as <code>width</code> &times; <code>height</code>. Leave a box empty to keep
        Roblox's own value.
      </td>
    </tr>
    <tr>
      <td><strong>Window position</strong></td>
      <td>Empty</td>
      <td>
        The top-left corner in screen pixels, as <code>x</code>, <code>y</code>. Negative values are allowed, which is
        how you place the window on a monitor to the left of or above the primary one.
      </td>
    </tr>
    <tr>
      <td><strong>Window title</strong></td>
      <td>Empty</td>
      <td>Replaces the title on the game window. Leave it empty to keep Roblox's own title.</td>
    </tr>
  </tbody>
</table>
</div>

<h2>Resetting</h2>

<p><strong>Reset window attributes</strong> clears the size, position and title back to empty. It doesn't turn the
master switch off, and it doesn't save &mdash; you still need to press <strong>Save</strong>.</p>

<h2>How it works</h2>

<p>The values are applied by Agentstrap's background watcher process, not by the settings window. After the Roblox
process starts, the watcher:</p>

<ol>
  <li>Waits for the Roblox window handle to appear (up to 60 seconds).</li>
  <li>Waits another 1.5 seconds for Roblox to finish sizing itself.</li>
  <li>Applies the size, position and title with the Windows API.</li>
</ol>

<div class="callout note">
  <strong>A watcher process stays running while Roblox does.</strong>
  That's how the attributes get applied to a window that didn't exist when you pressed Save.
</div>

<div class="callout warning">
  <strong>Invalid values are dropped silently.</strong>
  A size of zero or less, or anything that isn't a number, is discarded and treated as "leave it to Roblox". The same
  goes for a title that's only whitespace.
</div>

<div class="callout tip">
  <strong>Multi-monitor tip.</strong>
  Negative X and Y values are the easiest way to place the window on a secondary display. If your secondary monitor
  is to the left of the primary one, try <code>-1920</code> for the X position.
</div>
`,
  },

  {
    path: "features/deployment-channels.html",
    title: "Deployment channels",
    lede: "Pin Agentstrap to a specific Roblox release channel instead of always following production.",
    body: `
<p>Roblox ships several release channels. By default Agentstrap follows whatever channel Roblox itself would use.
Setting a deployment channel pins Agentstrap to one specific channel, which is how you test against a pre-release
build or deliberately stay on an older one.</p>

<div class="callout note">
  <strong>Where it lives.</strong>
  Settings &rarr; <strong>Deployment</strong> &rarr; <strong>Roblox channel</strong>.
</div>

<h2>Options</h2>

<div class="table-wrap">
<table>
  <thead><tr><th>Option</th><th>Default</th><th>Detail</th></tr></thead>
  <tbody>
    <tr>
      <td><strong>Deployment channel</strong></td>
      <td>Empty</td>
      <td>
        The channel name to pin to. Leave it empty to follow Roblox's own channel selection. Channel names may contain
        letters, digits, hyphens and underscores, up to 64 characters.
      </td>
    </tr>
  </tbody>
</table>
</div>

<p>There's no fixed list of channels in the interface &mdash; you type the name you want. The only channel Agentstrap
knows about by name is <code>production</code>, which is what it falls back to when the field is empty.</p>

<h2>Buttons</h2>

<div class="table-wrap">
<table>
  <thead><tr><th>Button</th><th>What it does</th></tr></thead>
  <tbody>
    <tr>
      <td><strong>Check</strong></td>
      <td>
        Resolves the channel against Roblox's client settings service and tells you the build it points at. With an
        empty field it checks <code>production</code>. A successful check reports the Roblox build version and GUID.
      </td>
    </tr>
    <tr>
      <td><strong>Apply channel</strong></td>
      <td>Validates the name and saves it. This one saves immediately &mdash; you don't need to press Save afterwards.</td>
    </tr>
    <tr>
      <td><strong>Clear</strong></td>
      <td>Empties the field and applies the empty value, so Agentstrap goes back to following Roblox's own selection.</td>
    </tr>
  </tbody>
</table>
</div>

<p>An invalid channel name shows
<em>"Channel names can contain only letters, digits, hyphens, and underscores (up to 64 characters)."</em> A channel
that Roblox doesn't recognise reports <em>"Could not resolve channel '{channel}'"</em>.</p>

<h2>Which channel wins</h2>

<p>Several things can request a channel. This is the order they take effect:</p>

<ol>
  <li>The <code>--channel</code> command line argument.</li>
  <li>A <code>channel:</code> parameter inside a launch URI.</li>
  <li><strong>Your saved deployment channel setting.</strong></li>
  <li>The registry value
      <code>HKCU\\SOFTWARE\\ROBLOX Corporation\\Environments\\RobloxPlayer\\Channel\\www.roblox.com</code>.</li>
  <li><code>production</code>.</li>
</ol>

<div class="callout tip">
  <strong>Explicit launch links still take precedence.</strong>
  If you launch with a <code>roblox://</code> link that names a channel, that wins over your saved preference. That's
  deliberate &mdash; a link should do what it says.
</div>

<p>Agentstrap writes the resolved channel back to the registry value so that Roblox's own tooling agrees with
it.</p>

<h2>What actually changes on disk</h2>

<p>Applying a channel only writes one key to <code>Settings.json</code>. The Roblox package itself is then downloaded
from that channel's setup mirror and extracted into the usual version folder, exactly as a normal update would.</p>

<div class="callout note">
  <strong>Stick to channels you know exist.</strong>
  Pre-release channels can contain builds that are unstable or mid-development. If a channel starts misbehaving,
  clear the setting and follow production again.
</div>
`,
  },

  {
    path: "features/diagnostics.html",
    title: "Diagnostics",
    lede: "Eight health checks that find broken installs, bad permissions, conflicting mods and pending migrations.",
    body: `
<p>Diagnostics runs every Agentstrap health check in a single pass and groups the results by what they cover. It's
the fastest way to work out why something isn't working, and the easiest way to give someone enough information to
help you.</p>

<div class="callout note">
  <strong>Where it lives.</strong>
  Settings &rarr; <strong>Diagnostics</strong>, or the <strong>Run diagnostics</strong> button on the
  <a href="command-center.html">Command Center</a>.
</div>

<h2>Running a check</h2>

<p>Press <strong>Run diagnostics</strong>. The checks run concurrently and a spinner appears while they work. The page
doesn't run them automatically when you open it &mdash; you always trigger it yourself, so it never surprises you with
network traffic.</p>

<p>When it finishes you get an overall verdict &mdash; <em>Healthy</em>, <em>Warnings found</em> or
<em>Problems found</em> &mdash; plus a counter for each severity.</p>

<h2>The eight checks</h2>

<div class="table-wrap">
<table>
  <thead><tr><th>Check</th><th>Category</th><th>What it looks at</th></tr></thead>
  <tbody>
    <tr>
      <td><strong>Roblox installation</strong></td><td>Roblox</td>
      <td>
        Whether the Roblox Player executable exists in the current version folder. A missing install is a warning,
        not a problem &mdash; it gets installed the next time you launch.
      </td>
    </tr>
    <tr>
      <td><strong>Roblox version state</strong></td><td>Roblox</td>
      <td>
        Whether Agentstrap knows which version of Roblox is installed. If the version state can't be read, it reports
        it as a warning.
      </td>
    </tr>
    <tr>
      <td><strong>Agentstrap configuration</strong></td><td>Configuration</td>
      <td>
        Whether <code>Settings.json</code> loaded correctly. Failing to load is a problem, because Agentstrap falls
        back to defaults. It also flags non-default settings such as disabled update checking, disabled analytics, a
        changed web environment or enabled developer mode.
      </td>
    </tr>
    <tr>
      <td><strong>Missing files</strong></td><td>Filesystem</td>
      <td>
        The files and folders Agentstrap expects: <code>Settings.json</code>, <code>State.json</code>, the
        <code>Logs</code>, <code>Versions</code> and <code>Modifications</code> folders. Missing folders that get
        recreated on demand are a warning; missing core files are a problem.
      </td>
    </tr>
    <tr>
      <td><strong>Invalid settings</strong></td><td>Configuration</td>
      <td>
        Individually invalid values: a process priority Windows won't accept, an unsupported locale, a custom theme
        file that no longer exists, a custom icon path that's gone, a matchmaker that's enabled with no place ID, or
        a negative minimum slot count.
      </td>
    </tr>
    <tr>
      <td><strong>Mod conflicts</strong></td><td>Mods</td>
      <td>
        A recorded custom skybox with no images in the sky folder, an empty custom font file, and mod changes that
        have been set but not yet applied to Roblox.
      </td>
    </tr>
    <tr>
      <td><strong>Permissions</strong></td><td>Filesystem</td>
      <td>
        Whether Agentstrap can write to its install folder, to <code>%LOCALAPPDATA%\\Roblox</code>, and to its temp
        directory. It does this by creating and deleting a small probe file in each.
      </td>
    </tr>
    <tr>
      <td><strong>Update state</strong></td><td>Updates</td>
      <td>
        Whether there are pending configuration migrations, whether update checking is enabled, and whether a newer
        Agentstrap release exists. This is the only check that makes a network request.
      </td>
    </tr>
  </tbody>
</table>
</div>

<h2>Copying the report</h2>

<p><strong>Copy report</strong> puts a plain-text report on your clipboard, one line per check, with the details
underneath where there are any. Paste it straight into a bug report.</p>

<p>There's also <strong>Open logs</strong>, which opens Agentstrap's log folder so you can attach the actual log
files if the report isn't enough.</p>

<div class="callout note">
  <strong>No file export.</strong>
  The report goes to the clipboard, not to a file. Paste it into a text editor if you want to keep it.
</div>

<div class="callout tip">
  <strong>Diagnostics doesn't change your configuration.</strong>
  It's a read-only tool. If a check points at a fix &mdash; like pending migrations, which it tells you to apply from
  the Updates tab &mdash; it says so rather than doing it for you.
</div>
`,
  },

  {
    path: "features/updates.html",
    title: "Updates",
    lede: "Check Agentstrap, your installed mods, and your configuration independently.",
    body: `
<p>The Updates page has four separate sections. Each one is checked on its own, so you can check just the thing you
care about instead of waiting for everything.</p>

<div class="callout note">
  <strong>Where it lives.</strong>
  Settings &rarr; <strong>Updates</strong>. <strong>Check everything</strong> runs all four sections at once, and
  opening the page runs it automatically.
</div>

<h2>Agentstrap</h2>

<p>Shows your installed version next to the latest published version, and a status line.</p>

<ul>
  <li><strong>Check</strong> queries the GitHub releases API for the newest published release.</li>
  <li><strong>Releases</strong> opens the releases page in your browser.</li>
</ul>

<div class="callout warning">
  <strong>Agentstrap doesn't update itself.</strong>
  There's no download-and-install button. The Releases link opens GitHub in your browser and you take it from there.
  The "Check for updates" setting controls whether Agentstrap queries GitHub at all.
</div>

<h2>Mods</h2>

<p>Agentstrap can check whether your installed mods are out of date, using the manifest in
<code>%LOCALAPPDATA%\\Agentstrap\\Modifications\\Mods\\manifest.json</code>.</p>

<ul>
  <li><strong>Check</strong> scans the manifest and reports anything outdated.</li>
  <li><strong>Mods folder</strong> opens the Modifications folder.</li>
</ul>

<p>Each mod entry can carry a <code>releaseApiUrl</code>. Agentstrap fetches that URL and looks for
<code>tag_name</code>, <code>version</code> or <code>latest_version</code> in the response, then compares it with
the version recorded on disk. Mods without a <code>releaseApiUrl</code> are skipped, so the check never reports a
false positive.</p>

<div class="callout note">
  <strong>It reports, it doesn't install.</strong>
  The Mods section tells you which mods are outdated. It never downloads or applies an update for you.
</div>

<h2>Configuration</h2>

<p>Agentstrap keeps a schema revision for your configuration. When a future version changes the shape of
<code>Settings.json</code>, a migration cleans up the old data.</p>

<ul>
  <li><strong>Check</strong> validates the stored configuration against the current schema revision and reports
      what's pending. It never writes anything.</li>
  <li><strong>Apply</strong> runs the pending migrations and saves your settings, state and Fast Flags.</li>
</ul>

<p>There are two migrations defined so far:</p>

<div class="table-wrap">
<table>
  <thead><tr><th>Migration</th><th>When it applies</th><th>What it does</th></tr></thead>
  <tbody>
    <tr>
      <td><strong>Drop matchmaker place ID</strong></td>
      <td>The matchmaker is enabled but no place ID is set.</td>
      <td>Turns the matchmaker off, rather than leaving it enabled with nothing to join.</td>
    </tr>
    <tr>
      <td><strong>Normalise process priority</strong></td>
      <td>The stored process priority isn't a value Windows accepts.</td>
      <td>Resets it to <em>Normal</em>.</td>
    </tr>
  </tbody>
</table>
</div>

<div class="callout tip">
  <strong>Check before you apply.</strong>
  Because Check is read-only, you can see exactly what a migration would do before you commit to it.
</div>

<h2>NEXUS</h2>

<p>Reports the local state of the experimental feature channel &mdash; whether it's on and which channel it's tracking.
It's a local check with no network traffic. See <a href="nexus.html">NEXUS mode</a>.</p>

<h2>Update checking</h2>

<p>Whether Agentstrap queries GitHub at all is controlled by <strong>Check for updates</strong> on the
<a href="index.html">Quality of Life</a> page, and it's on by default. When it's off, the Agentstrap section reports
<em>"Update checking is disabled"</em> instead of querying.</p>
`,
  },

  {
    path: "features/modding.html",
    title: "Modding",
    lede: "Customise Roblox's own assets &mdash; cursors, sounds, fonts, the avatar background and the skybox.",
    body: `
<p>Roblox stores some of its assets locally, and Agentstrap can replace them. Because the replacements live outside
the Roblox install, they survive every Roblox update.</p>

<div class="callout note">
  <strong>Where it lives.</strong>
  Settings &rarr; <strong>Mods</strong>.
</div>

<h2>How modding works</h2>

<p>Your <strong>Modifications</strong> folder is an overlay on top of the Roblox client folder. Every time Roblox
launches, the contents of Modifications are copied into the client install, replacing files with the same path.</p>

<pre><code>Agentstrap\\
  Modifications\\
    content\\
      sounds\\
        ouch.ogg        &lt;-- your version
  Versions\\&lt;version-guid&gt;\\
    content\\
      sounds\\
        ouch.ogg        &lt;-- Roblox's original, kept intact</code></pre>

<div class="callout tip">
  <strong>Use the Mods folder button.</strong>
  The Mods page has an <strong>Open mods folder</strong> shortcut that takes you straight to the right place, so you
  don't have to hunt for it.
</div>

<h2>Presets</h2>

<p>These are the ready-made modifications Agentstrap provides.</p>

<div class="table-wrap">
<table>
  <thead><tr><th>Preset</th><th>What it changes</th></tr></thead>
  <tbody>
    <tr>
      <td><strong>Mouse cursor</strong></td>
      <td>
        The in-game cursor. You can pick between the <em>Default</em> cursor and the ones from Roblox's
        <em>2013</em> and <em>2006</em> eras.
      </td>
    </tr>
    <tr>
      <td><strong>Old character sounds</strong></td>
      <td>Restores the older walking, jumping and getting-up sounds.</td>
    </tr>
    <tr>
      <td><strong>Old avatar editor background</strong></td>
      <td>Restores the original mobile avatar editor background image.</td>
    </tr>
    <tr>
      <td><strong>Emoji font</strong></td>
      <td>
        Replaces the in-game emoji font. Options are <em>Default</em>, <em>Catmoji</em>, <em>Windows 11</em>,
        <em>Windows 10</em> and <em>Windows 8</em>. The fonts are downloaded from GitHub the first time you pick one,
        so this option needs an internet connection. Pick <em>Default</em> to remove the replacement entirely.
      </td>
    </tr>
  </tbody>
</table>
</div>

<div class="callout note">
  <strong>Presets apply on the next launch.</strong>
  Changing one queues the change; it's copied into the Roblox install the next time Roblox starts.
</div>

<h2>Roblox Apple Emoji Mode</h2>

<p>Roblox Apple Emoji Mode is a separate settings page. It attempts to make supported Roblox text elements render
emojis in Apple's iPhone emoji style by replacing Roblox's emoji font through the same Modifications overlay used
by the emoji font preset. It never changes Windows system fonts.</p>

<div class="callout note">
  <strong>Where it lives.</strong>
  Settings &rarr; <strong>Roblox Apple Emoji Mode</strong>.
</div>

<p>Because Apple's original emoji designs are proprietary, Agentstrap does not ship or download an Apple emoji
font. You must select a compatible, legally usable Apple style colour emoji font yourself. Agentstrap validates
the selected file, copies it to <code>content\\fonts\\TwemojiMozilla.ttf</code>, and keeps a backup so the
previous font can be restored.</p>

<p>Agentstrap cannot observe Roblox's renderer, so the page reports <strong>Rendering unverified</strong> after a
successful application. It never reports a verified Apple rendering result.</p>

<div class="callout warning">
  <strong>Supported text elements are unverified.</strong>
  Roblox may use different text rendering paths in different parts of the client. A font replacement can only
  affect the paths that actually load <code>TwemojiMozilla.ttf</code>.
</div>

<h2>Custom font</h2>

<p>You can replace the game's own font with any font file on your machine. Use <strong>Choose</strong> to pick a
<code>.ttf</code> file, and <strong>Remove</strong> to go back to the default. The file is copied to
<code>Modifications\\content\\fonts\\CustomFont.ttf</code>.</p>

<h2>Game skybox</h2>

<p>The skybox is the panorama you see in games that use one. Agentstrap can replace it with an image of your
choosing.</p>

<ul>
  <li>Pick a <code>.png</code>, <code>.jpg</code>, <code>.bmp</code>, <code>.gif</code> or <code>.tiff</code>.</li>
  <li>The image is squared up and written into the Roblox install on the next launch.</li>
  <li>The Mods page shows a live preview of the current skybox, and lets you remove it again.</li>
</ul>

<p>Agentstrap converts a single image into the six faces Roblox expects
(<code>sky512_up</code>, <code>sky512_down</code>, and the four sides).</p>

<div class="callout tip">
  <strong>Use a square image.</strong>
  A square source gives the best result, since it's projected across a panorama.
</div>

<h2>Making your own mods</h2>

<p>Anything not covered by a preset can be done by hand. Drop the file into the Modifications folder at the same
relative path Roblox uses.</p>

<div class="table-wrap">
<table>
  <thead><tr><th>What</th><th>Path inside Modifications</th></tr></thead>
  <tbody>
    <tr><td>Mouse cursors</td><td><code>content\\textures\\KeyboardMouse\\Cursors\\</code></td></tr>
    <tr><td>Character sounds</td><td><code>content\\sounds\\</code></td></tr>
    <tr><td>Fonts</td><td><code>content\\fonts\\</code></td></tr>
    <tr><td>Emoji font</td><td><code>content\\fonts\\TwemojiMozilla.ttf</code></td></tr>
    <tr><td>Avatar editor background</td><td><code>ExtraContent\\places\\Mobile.rbxl</code></td></tr>
    <tr><td>Surface textures</td><td><code>PlatformContent\\pc\\textures\\</code></td></tr>
    <tr><td>Skybox</td><td><code>PlatformContent\\pc\\textures\\sky\\</code></td></tr>
    <tr><td>Fast Flags</td><td><code>ClientSettings\\ClientAppSettings.json</code></td></tr>
  </tbody>
</table>
</div>

<div class="callout warning">
  <strong>You can't replace individual icons.</strong>
  Roblox stores them in sprite sheets whose layout changes frequently, so replacing one reliably isn't practical.
</div>

<h2>Distributing a mod</h2>

<p>Zip up the contents of the Modifications folder, keeping the folder structure intact, so the files sit at the
root of the archive. A user then extracts it straight into their own Modifications folder.</p>

<div class="callout note">
  <strong>Mods are purely cosmetic and local.</strong>
  They never touch the game server, and nobody else can see them.
</div>
`,
  },

  {
    path: "features/nexus.html",
    title: "NEXUS mode",
    lede: "An opt-in experimental feature channel for people who want changes before everyone else gets them.",
    body: `
<p>NEXUS is Agentstrap's optional feature channel. It's a way for experimental behaviour to be enabled without
shipping it to everyone, and it's off unless you explicitly turn it on.</p>

<div class="callout note">
  <strong>Where it lives.</strong>
  Settings &rarr; <strong>Advanced</strong> &rarr; <strong>NEXUS</strong>.
</div>

<h2>Options</h2>

<div class="table-wrap">
<table>
  <thead><tr><th>Option</th><th>Default</th><th>What it does</th></tr></thead>
  <tbody>
    <tr>
      <td><strong>Enable NEXUS mode</strong></td>
      <td><span class="badge off">Off</span></td>
      <td>
        Pulls feature flags and configuration from the NEXUS channel instead of the defaults. Turning it on shows a
        warning first, because it can change how Agentstrap behaves during a launch.
      </td>
    </tr>
    <tr>
      <td><strong>Channel</strong></td>
      <td><em>stable</em></td>
      <td>
        Which NEXUS channel to track. <em>stable</em> is the tested one, <em>preview</em> gets changes first, and
        <em>nightly</em> is the most experimental.
      </td>
    </tr>
    <tr>
      <td><strong>Verbose logging</strong></td>
      <td><span class="badge off">Off</span></td>
      <td>Writes an extra NEXUS log entry for every flag it resolves, which makes flag problems much easier to
          trace.</td>
    </tr>
    <tr>
      <td><strong>Allow mod overrides</strong></td>
      <td><span class="badge off">Off</span></td>
      <td>Lets a mod replace NEXUS-provided configuration. Useful when testing, but it can conflict with NEXUS
          itself.</td>
    </tr>
  </tbody>
</table>
</div>

<p>A status card under the options shows whether NEXUS is enabled and which channel it's on, with a
<strong>Refresh</strong> button to re-read the state.</p>

<h2>Resetting</h2>

<p><strong>Reset to defaults</strong> turns NEXUS off, puts the channel back to <em>stable</em>, and switches off
both verbose logging and mod overrides. It saves immediately and asks for confirmation first, because it clears
settings outright.</p>

<h2>Current status</h2>

<div class="callout warning">
  <strong>NEXUS is a local opt-in in this build.</strong>
  The channel selection, verbose logging and mod override settings are stored and reported, but Agentstrap doesn't
  currently fetch anything from a NEXUS channel &mdash; there's no network request behind it yet. Enabling it today
  mainly acts as a flag for the feature set, and it's what unlocks the developer tools described below.
</div>

<p>Because it's experimental and unfinished, treat NEXUS as a preview of what's coming rather than a feature to
depend on. If something behaves oddly with NEXUS on, that's likely why.</p>

<h2>Developer tools</h2>

<p>The <strong>Advanced</strong> page also has a set of developer tools. They're hidden behind a lock, and unlock when
either developer mode or NEXUS mode is enabled.</p>

<div class="table-wrap">
<table>
  <thead><tr><th>Tool</th><th>What it does</th></tr></thead>
  <tbody>
    <tr><td><strong>Dump runtime environment</strong></td><td>Writes OS, runtime and install details to the log.</td></tr>
    <tr><td><strong>Open log directory</strong></td><td>Opens the folder containing Agentstrap's logs.</td></tr>
    <tr><td><strong>Clear logs</strong></td><td>Deletes every log file Agentstrap has written.</td></tr>
    <tr><td><strong>Copy environment summary</strong></td><td>Copies the version, Windows build and install paths to the clipboard.</td></tr>
    <tr><td><strong>Restart Agentstrap</strong></td><td>Relaunches Agentstrap with the current settings.</td></tr>
  </tbody>
</table>
</div>

<div class="callout warning">
  <strong>Clear logs deletes everything.</strong>
  It removes every file in the log folder, including the one currently being written to, and it can't be undone. It
  asks for confirmation first.
</div>

<p>The same <strong>Clear logs</strong> and <strong>Open logs</strong> buttons are always visible on the page, even
while the tool list itself is locked.</p>

<h2>Maintenance</h2>

<p>Under <strong>Maintenance</strong> you'll also find <strong>Dump configuration</strong>, which writes a snapshot of
your environment to the log file, and <strong>Restart Agentstrap</strong>, which saves your settings and relaunches
the app.</p>
`,
  },

  {
    path: "features/dont-exit-to-desktop-app.html",
    title: "Don't exit to desktop app",
    lede: "Make Roblox fully close when you leave a game, instead of dropping back to the desktop app.",
    body: `
<p>By default, leaving a Roblox game returns you to the Roblox desktop app rather than closing the client
completely. Turning this on makes the client fully exit.</p>

<div class="callout note">
  <strong>Where it lives.</strong>
  Settings &rarr; <strong>Integrations</strong> &rarr; <strong>Don't exit to desktop app</strong>. Off by default.
  Requires <a href="activity-tracking.html">activity tracking</a>.
</div>

<h2>What it affects</h2>

<p>With the option enabled, quitting a game closes the Roblox client instead of showing the app chooser. The next
launch therefore starts from a completely clean client.</p>

<p>It also disables the <strong>Game history</strong> item in the tray menu, because the history is tied to a running
session.</p>

<div class="callout warning">
  <strong>This will break some things.</strong>
  The setting is labelled with a warning in the interface for a reason. Features that rely on the Roblox app
  surviving &mdash; including Agentstrap's own game history &mdash; stop working.
</div>
`,
  },
];
