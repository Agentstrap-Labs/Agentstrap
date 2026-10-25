export default [
  {
    path: "info/index.html",
    title: "Info",
    lede: "Background reading: what Agentstrap does with your data, why it can't get you banned, and where things live.",
    body: `
<p>These pages cover the questions that come up when people want to know more about what Agentstrap is doing
underneath.</p>

<div class="card-grid">
  <a class="card" href="privacy-policy.html">
    <span class="card-tag">info</span>
    <h3>Privacy policy</h3>
    <p>Exactly what data Agentstrap sends anywhere, and to whom.</p>
  </a>
  <a class="card" href="bans.html">
    <span class="card-tag">info</span>
    <h3>Agentstrap and bans</h3>
    <p>Why using a third-party bootstrapper doesn't put your account at risk.</p>
  </a>
  <a class="card" href="where-data-is-stored.html">
    <span class="card-tag">info</span>
    <h3>Where data is stored</h3>
    <p>A map of every folder and file Agentstrap writes, and what's in it.</p>
  </a>
  <a class="card" href="translations.html">
    <span class="card-tag">info</span>
    <h3>Translations</h3>
    <p>How the interface is translated, and how to help.</p>
  </a>
  <a class="card" href="using-roblox-icons-for-shortcuts.html">
    <span class="card-tag">info</span>
    <h3>Roblox icons for shortcuts</h3>
    <p>Extract Agentstrap's Roblox icons and use them on your own shortcuts.</p>
  </a>
</div>

<h2>The short version</h2>

<div class="table-wrap">
<table>
  <thead><tr><th>Question</th><th>Answer</th></tr></thead>
  <tbody>
    <tr>
      <td>Does Agentstrap report my activity to Roblox?</td>
      <td>No. Nothing about what you're playing is sent to Roblox or to Bloxstrap. The only Roblox API calls are read-only lookups of public metadata.</td>
    </tr>
    <tr>
      <td>Does it send anything to a Bloxstrap server?</td>
      <td>No. Agentstrap has no analytics backend of its own.</td>
    </tr>
    <tr>
      <td>What leaves my machine?</td>
      <td>Roblox downloads, the GitHub release check, ipinfo.io lookups when you enable server location or the overlay, and per-mod update checks.</td>
    </tr>
    <tr>
      <td>Is it malware?</td>
      <td>No. The source is public and the only official download is the GitHub release.</td>
    </tr>
    <tr>
      <td>Can I get banned for using it?</td>
      <td>No. It doesn't interact with the client in the way exploits do. See <a href="bans.html">the details</a>.</td>
    </tr>
  </tbody>
</table>
</div>

<div class="callout note">
  <strong>Looking for a fix instead?</strong>
  The <a href="../help/index.html">Help section</a> covers the problems people actually run into, step by step.
</div>
`,
  },

  {
    path: "info/privacy-policy.html",
    title: "Privacy policy",
    lede: "What Agentstrap sends over the network, what it stores on your machine, and what you can turn off.",
    body: `
<h2>What leaves your machine</h2>

<div class="table-wrap">
<table>
  <thead><tr><th>When</th><th>Where it goes</th><th>What is sent</th></tr></thead>
  <tbody>
    <tr>
      <td>Installing or updating Roblox</td>
      <td>Roblox's download servers</td>
      <td>Ordinary HTTP requests to fetch the client package.</td>
    </tr>
    <tr>
      <td>Checking for Agentstrap updates</td>
      <td><code>api.github.com</code></td>
      <td>An ordinary release API request. No personal data.</td>
    </tr>
    <tr>
      <td>Looking up a server's location</td>
      <td><code>ipinfo.io</code></td>
      <td>
        The <strong>game server's</strong> IP address, and the path it's looked up under. ipinfo.io necessarily sees
        your own IP as the connecting client, exactly as with any web request.
      </td>
    </tr>
    <tr>
      <td>Showing the server connection overlay</td>
      <td><code>ipinfo.io</code></td>
      <td>The same as above, whenever the overlay is enabled.</td>
    </tr>
    <tr>
      <td>Sending server connection pushes</td>
      <td><code>api.day.app</code> (Bark)</td>
      <td>
        Your Bark device key, notification title and message, and the Agentstrap icon URL, only when server connection
        notifications are enabled and a Bark key is configured. The push key is sent over HTTPS. The Bark app on your
        iPhone retrieves the icon image from GitHub's public raw content host.
      </td>
    </tr>
    <tr>
      <td>Resolving the experience name</td>
      <td><code>games.roblox.com</code></td>
      <td>The game's universe ID.</td>
    </tr>
    <tr>
      <td>Fetching game icons</td>
      <td><code>thumbnails.roblox.com</code></td>
      <td>The game's universe ID.</td>
    </tr>
    <tr>
      <td>Showing your Roblox account on Discord</td>
      <td><code>users.roblox.com</code>, <code>thumbnails.roblox.com</code></td>
      <td>
        Your Roblox user ID and avatar, but only if you turn on <em>Show Roblox account</em> in Discord Rich
        Presence. Off by default.
      </td>
    </tr>
    <tr>
      <td>Checking for mod updates</td>
      <td>Each mod's own <code>releaseApiUrl</code></td>
      <td>Whatever URL the mod author specified. Agentstrap doesn't send anything of yours.</td>
    </tr>
  </tbody>
</table>
</div>

<div class="callout note">
  <strong>Agentstrap has no server of its own.</strong>
  There's no analytics backend, no account system, and no telemetry endpoint. Agentstrap is entirely local.
</div>

<h2>Analytics</h2>

<p>The interface has an <strong>Analytics</strong> toggle in the <em>Agentstrap</em> settings section. It is on by
default, but with the current build it has no endpoint to report to, so nothing is sent either way. The setting is
there so the behaviour is explicit and visible rather than implied.</p>

<h2>What stays on your machine</h2>

<div class="table-wrap">
<table>
  <thead><tr><th>What</th><th>Where</th><th>Notes</th></tr></thead>
  <tbody>
    <tr>
      <td>Your settings</td>
      <td><code>%LOCALAPPDATA%\\Agentstrap\\Settings.json</code></td>
      <td>Every toggle you've changed. Plain text and easy to inspect.</td>
    </tr>
    <tr>
      <td>Launcher state</td>
      <td><code>%LOCALAPPDATA%\\Agentstrap\\State.json</code></td>
      <td>Launch status, the forced-reinstall flag and similar.</td>
    </tr>
    <tr>
      <td>Modifications</td>
      <td><code>%LOCALAPPDATA%\\Agentstrap\\Modifications</code></td>
      <td>Your mods, your Fast Flags, your custom font and skybox.</td>
    </tr>
    <tr>
      <td>Logs</td>
      <td><code>%LOCALAPPDATA%\\Agentstrap\\Logs</code></td>
      <td>
        Up to 15 rotated log files. They record every HTTP request Agentstrap makes, including the full ipinfo.io URL
        with the queried game server IP in it. <strong>If you share logs, those addresses go with them.</strong>
      </td>
    </tr>
    <tr>
      <td>Game history</td>
      <td>Nowhere</td>
      <td>Session only, in memory, never written to disk.</td>
    </tr>
    <tr>
      <td>Location lookups</td>
      <td>Nowhere</td>
      <td>Cached in memory for the session only. Failed lookups are cached too, so they're not retried.</td>
    </tr>
    <tr>
      <td>Notification credentials</td>
      <td><code>%LOCALAPPDATA%\\Agentstrap\\Notifications\\Credentials</code></td>
      <td>The Bark push key and optional IPinfo token are encrypted with Windows DPAPI for your account.</td>
    </tr>
  </tbody>
</table>
</div>

<p>See <a href="where-data-is-stored.html">Where data is stored</a> for the full map.</p>

<h2>Turning things off</h2>

<div class="table-wrap">
<table>
  <thead><tr><th>To stop</th><th>Turn off</th></tr></thead>
  <tbody>
    <tr><td>ipinfo.io lookups for the tray notification and dialog</td><td><em>Query server location</em> on the Integrations page</td></tr>
    <tr><td>ipinfo.io lookups from the overlay</td><td><em>Show server connection notifications</em> on the Integrations page</td></tr>
    <tr><td>Bark server connection pushes and their IPinfo lookups</td><td>Turn off <em>Send server connection notifications</em> on the Server Connection Notifications page</td></tr>
    <tr><td>All server watching</td><td><em>Enable activity tracking</em> on the Integrations page</td></tr>
    <tr><td>Your Roblox account appearing on Discord</td><td><em>Show Roblox account</em> on the Integrations page</td></tr>
    <tr><td>GitHub release checks</td><td><em>Check for updates</em> on the Quality of Life page</td></tr>
    <tr><td>Mod update checks</td><td>Remove the <code>releaseApiUrl</code> from the mod's manifest entry</td></tr>
  </tbody>
</table>
</div>

<div class="callout warning">
  <strong>The overlay has its own lookup.</strong>
  Turning off <em>Query server location</em> does not stop the overlay from contacting ipinfo.io. The overlay performs
  its own lookup whenever it's enabled. If that matters to you, switch the overlay off as well.
</div>

<h2>Roblox's own data</h2>

<p>Agentstrap doesn't control what Roblox itself collects. The Roblox client, running under Agentstrap, talks to
Roblox's services exactly as it would if you'd launched it normally, and Roblox's own privacy policy applies to
that.</p>
`,
  },

  {
    path: "info/bans.html",
    title: "Agentstrap and bans",
    lede: "Why using a third-party bootstrapper doesn't put your Roblox account at risk.",
    body: `
<h2>The short answer</h2>

<p>Using Agentstrap should not get you banned. It doesn't interact with the Roblox client in the way that exploits
do, and it doesn't send anything to Roblox about what you're doing.</p>

<h2>Why</h2>

<p>An exploit works by hooking into the client, injecting code, or sending packets that a normal client never would.
Roblox can detect that, and it does.</p>

<p>Agentstrap works at a completely different layer. It:</p>

<ul>
  <li>Downloads the Roblox client from Roblox's own servers, and verifies the package against the manifest Roblox
      provides.</li>
  <li>Copies a folder of asset replacements into the install directory &mdash; fonts, sounds, cursors, a skybox.
      These are local files that the game reads at startup.</li>
  <li>Launches the resulting executable exactly as Roblox's own launcher would.</li>
</ul>

<p>From Roblox's point of view, you're running the same client binary, talking to the same servers, sending the same
traffic. The only differences are local files on your disk and the fact that a different program pressed the
button.</p>

<h2>What Agentstrap does modify</h2>

<p>Two things, both of which are local configuration:</p>

<ul>
  <li><strong>Fast Flags</strong> &mdash; written into <code>ClientAppSettings.json</code> in the client folder. These
      are Roblox's own supported local configuration mechanism, read by the client at startup.</li>
  <li><strong>Modifications</strong> &mdash; cosmetic asset files.</li>
</ul>

<p>There's also an optional patch related to the desktop app, used by the "don't exit to desktop app" option and by
Discord Rich Presence. It's a local client patch, applied to your own install, and it doesn't change the protocol the
game speaks.</p>

<h2>Things that <em>can</em> get you banned</h2>

<p>None of these are Agentstrap features, but they're worth being clear about:</p>

<ul>
  <li>Third-party executors, script injectors and exploits.</li>
  <li>Macro and automation tools that imitate player behaviour.</li>
  <li>Macro and automation tools that imitate player behaviour.</li>
  <li>Buying or using leaked, stolen or shared game assets.</li>
  <li>Using modified client files to gain an advantage over other players.</li>
</ul>

<div class="callout note">
  <strong>One important caveat.</strong>
  The Fast Flag allowlist Roblox introduced in September 2025 is a sign that Roblox is actively restricting which
  local configuration it honours. Using Fast Flags is a supported mechanism, but a flag that manipulates gameplay
  could still be a problem. Stick to graphics and quality-of-life flags.
</div>

<h2>Mods</h2>

<p>Modding with Agentstrap is purely cosmetic. Replacing a cursor, a sound or a skybox changes nothing about the
game's logic, and nothing about what your account is doing in any server. Other players cannot see it, and servers
cannot detect it.</p>
`,
  },

  {
    path: "info/where-data-is-stored.html",
    title: "Where data is stored",
    lede: "Every folder and file Agentstrap writes, what's in it, and what's safe to delete.",
    body: `
<p>Agentstrap installs itself into <code>%LOCALAPPDATA%\\Agentstrap</code> by default, and you can choose a
different folder when you run the installer. Most of what it writes lives inside that install folder, but two
things deliberately sit elsewhere.</p>

<h2>The default layout</h2>

<pre><code>%LOCALAPPDATA%\\Agentstrap\\
  Agentstrap.exe           the launcher
  Settings.json            your settings
  State.json               launcher state
  Logs\\                    log files (up to 15)
  Downloads\\              downloaded Roblox packages
  Versions\\                extracted Roblox installs
    &lt;version-guid&gt;\\
      RobloxPlayerBeta.exe
  Modifications\\           your mods, Fast Flags and custom assets
    ClientSettings\\
      ClientAppSettings.json
    content\\
    PlatformContent\\
    ExtraContent\\
  CustomThemes\\            your custom themes
  Integrations\\            custom integration definitions</code></pre>

<div class="callout note">
  <strong>Two folders are always here.</strong>
  <code>Modifications</code> and <code>CustomThemes</code> live under
  <code>%LOCALAPPDATA%\\Agentstrap</code> regardless of where you install the app, so your mods survive moving
  the install. Everything else &mdash; settings, logs, downloads and the Roblox installs &mdash; follows the
  install folder.
</div>

<h2>What each thing is</h2>

<div class="table-wrap">
<table>
  <thead><tr><th>Path</th><th>Contains</th><th>Safe to delete?</th></tr></thead>
  <tbody>
    <tr>
      <td><code>Settings.json</code></td>
      <td>Every preference you've set.</td>
      <td>Yes, but Agentstrap will reset to defaults on next launch.</td>
    </tr>
    <tr>
      <td><code>State.json</code></td>
      <td>Launch history, the forced-reinstall flag.</td>
      <td>Yes, same caveat.</td>
    </tr>
    <tr>
      <td><code>Logs\\</code></td>
      <td>Rotated log files. Contains every HTTP request Agentstrap made.</td>
      <td>Yes. The Advanced page has a <strong>Clear logs</strong> button.</td>
    </tr>
    <tr>
      <td><code>Downloads\\</code></td>
      <td>Cached Roblox package archives.</td>
      <td>Yes, they're re-downloaded when needed.</td>
    </tr>
    <tr>
      <td><code>Versions\\</code></td>
      <td>The extracted Roblox installs. The player executable lives here.</td>
      <td>Not recommended &mdash; Roblox will be reinstalled from scratch.</td>
    </tr>
    <tr>
      <td><code>Modifications\\</code></td>
      <td>Your mods, Fast Flags, custom font and skybox.</td>
      <td>Yes, but you'll lose all your mods.</td>
    </tr>
    <tr>
      <td><code>CustomThemes\\</code></td>
      <td>Custom themes you've written.</td>
      <td>Yes.</td>
    </tr>
  </tbody>
</table>
</div>

<h2>Elsewhere on disk</h2>

<div class="table-wrap">
<table>
  <thead><tr><th>Path</th><th>What</th></tr></thead>
  <tbody>
    <tr><td><code>%TEMP%\\Agentstrap</code></td><td>Temporary downloads, in-progress updates and temporary logs.</td></tr>
    <tr><td><code>%LOCALAPPDATA%\\Roblox</code></td><td>Roblox's own data, written by the game client.</td></tr>
    <tr><td><code>%LOCALAPPDATA%\\Roblox\\logs</code></td><td>Roblox's own logs. The <strong>Open Roblox logs</strong> button on the Deployment page opens this.</td></tr>
    <tr><td><code>HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Agentstrap</code></td><td>The uninstall entry, used by <em>Settings &rarr; Apps</em>.</td></tr>
    <tr><td><code>HKCU\\Software\\Classes\\roblox</code></td><td>The protocol handler that makes Roblox links open Agentstrap.</td></tr>
  </tbody>
</table>
</div>

<h2>Changing the install location</h2>

<p>The install location can be moved. The installer asks where you want it, and
<a href="../help/change-install-location.html">there's a walkthrough</a> for changing it later.</p>
`,
  },

  {
    path: "info/translations.html",
    title: "Translations",
    lede: "Agentstrap's interface is available in dozens of languages, and you can help translate it.",
    body: `
<h2>Supported languages</h2>

<p>The interface is translated into more than thirty languages, including:</p>

<p>Arabic, Basque, Bulgarian, Catalan, Chinese (Simplified and Traditional), Croatian, Czech, Danish, Dutch,
English, Estonian, Filipino, Finnish, French, German, Greek, Hebrew, Hindi, Hungarian, Indonesian, Italian,
Japanese, Korean, Latvian, Lithuanian, Norwegian, Persian, Polish, Portuguese (Brazil), Romanian, Russian, Serbian,
Slovak, Spanish, Swedish, Thai, Turkish, Ukrainian, Vietnamese.</p>

<p>You can change the language from the <strong>Appearance</strong> settings page, or from the language selector in
the launch menu.</p>

<h2>How it works</h2>

<p>Translations live in resource files (<code>Strings.resx</code> and one per language), and the locale is applied
when the interface loads. The region format is also switched to match, so dates and numbers appear the way your
language expects.</p>

<div class="callout note">
  <strong>Fallback behaviour.</strong>
  If a string is missing in your language, Agentstrap falls back to English for that string rather than showing
  nothing.
</div>

<h2>Translating</h2>

<p>Translation files are plain XML, so any text editor works. The workflow is:</p>

<ol>
  <li>Find the language you want to contribute to in the <code>Resources</code> folder.</li>
  <li>Duplicate a <code>Strings.resx</code> file as <code>Strings.&lt;code&gt;.resx</code> if it doesn't exist yet.</li>
  <li>Translate the values. Leave the resource <em>names</em> alone &mdash; they're the keys the code looks up.</li>
  <li>Save and rebuild.</li>
</ol>

<p>Two things to be careful about:</p>

<ul>
  <li><strong>Don't rename or remove keys.</strong> A missing key falls back to English, but a renamed one is a
      silent break.</li>
  <li><strong>Watch the formatting placeholders.</strong> Strings like <code>{0}</code> and <code>{1}</code> are
      substituted at runtime. They must survive translation, in the same order.</li>
</ul>

<div class="callout tip">
  <strong>There's a script for finding unused strings.</strong>
  <code>Scripts\\Translations\\find-unused.py</code> reports resource keys that nothing in the code references any
  more, which is a good first thing to clean up when starting a new language.
</div>
`,
  },
{
    path: "info/using-roblox-icons-for-shortcuts.html",
    title: "Using Roblox icons for shortcuts",
    lede: "Extract Agentstrap's collection of Roblox icons and use them for your own shortcuts.",
    body: `
<p>Agentstrap ships a set of Roblox icons covering most of the eras of the platform's logo. You can extract them
to a folder and use them anywhere you like &mdash; for your own desktop shortcuts, for a launcher, or for anything
else that accepts an <code>.ico</code> file.</p>

<div class="callout note">
  <strong>Where it lives.</strong>
  Settings &rarr; <strong>Shortcuts</strong> &rarr; <em>Extract Roblox icons to folder</em>.
</div>

<h2>Extracting the icons</h2>

<ol class="steps">
  <li>
    <strong>Turn on <em>Extract Roblox icons to folder</em>.</strong>
  </li>
  <li>
    <strong>Press Save.</strong>
  </li>
  <li>
    <strong>Open the folder.</strong>
    It's called <code>Icons</code>, and it sits inside your Agentstrap install folder.
  </li>
</ol>

<p>Because the folder lives inside the install folder, it follows Agentstrap if you move the app. If you installed
somewhere else, the path is the one you chose during installation, plus <code>\\Icons</code>.</p>

<h2>What you get</h2>

<div class="table-wrap">
<table>
  <thead><tr><th>File</th><th>Icon</th></tr></thead>
  <tbody>
    <tr><td><code>IconBloxstrap.ico</code></td><td>The Agentstrap icon itself.</td></tr>
    <tr><td><code>Icon2008.ico</code></td><td>The 2008-era Roblox icon.</td></tr>
    <tr><td><code>Icon2011.ico</code></td><td>The 2011-era Roblox icon.</td></tr>
    <tr><td><code>IconEarly2015.ico</code></td><td>The early 2015 icon.</td></tr>
    <tr><td><code>IconLate2015.ico</code></td><td>The late 2015 icon.</td></tr>
    <tr><td><code>Icon2017.ico</code></td><td>The 2017-era Roblox icon.</td></tr>
    <tr><td><code>Icon2019.ico</code></td><td>The 2019-era Roblox icon.</td></tr>
    <tr><td><code>Icon2022.ico</code></td><td>The 2022-era Roblox icon.</td></tr>
    <tr><td><code>IconBloxstrapClassic.ico</code></td><td>The classic Bloxstrap icon.</td></tr>
  </tbody>
</table>
</div>

<h2>Using them</h2>

<h3>For an Agentstrap shortcut</h3>

<p>The simplest way is to point Agentstrap at one of them:</p>

<ol>
  <li>Go to Settings &rarr; <strong>Appearance</strong>.</li>
  <li>Open the <strong>Customisation</strong> expander.</li>
  <li>Set <strong>Custom icon</strong> to one of the extracted files, using <strong>Browse</strong>.</li>
  <li>Press <strong>Save</strong>.</li>
</ol>

<p>The same page also has an <strong>Icon</strong> dropdown, which switches the icon Agentstrap uses for its own
window and taskbar button. That dropdown already includes every one of the Roblox icons, so you don't need to
extract anything just to change it.</p>

<h3>For a shortcut you created yourself</h3>

<p>Any Windows shortcut can use them:</p>

<ol>
  <li>Right-click the shortcut &rarr; <strong>Properties</strong>.</li>
  <li>Go to the <strong>Shortcut</strong> tab.</li>
  <li>Click <strong>Change Icon</strong>, then <strong>Browse</strong> to the extracted folder and pick the
      <code>.ico</code> you want.</li>
</ol>

<h3>For anything else</h3>

<p>They're ordinary <code>.ico</code> files. Use them anywhere that accepts one.</p>

<div class="callout note">
  <strong>Turning it back off deletes the folder.</strong>
  Switching the toggle off and saving removes the whole <code>Icons</code> folder and its contents. Copy anything
  you want to keep somewhere else first.
</div>
`,
  },
];
