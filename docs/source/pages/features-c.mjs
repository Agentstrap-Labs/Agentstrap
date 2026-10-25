export default [
  {
    path: "features/accounts.html",
    title: "Roblox accounts",
    lede: "Save, switch and launch Roblox with a selected account.",
    body: `
<p>Agentstrap can save multiple Roblox accounts locally and use the selected one when launching Roblox.</p>

<h2>Adding an account</h2>
<p>Open Settings &rarr; Accounts and use Quick Sign In. Agentstrap validates the session against Roblox and stores only
the account profile locally. Credentials are protected with Windows DPAPI and are never written to logs.</p>

<h2>Logout</h2>
<p>Use the Logout button to sign out of the active account. The saved account list remains available for switching.</p>

<h2>Switching</h2>
<p>Select an account and press Switch. Only one account is active at a time. Roblox launches use the active account.</p>

<div class="callout note">
  <strong>Security.</strong> Credentials are stored locally with DPAPI and are never sent anywhere except Roblox during
  validation.
</div>
`,
  },
  {
    path: "features/profiles.html",
    title: "Profiles",
    lede: "Save, switch, compare and restore whole configuration profiles.",
    body: `
<p>Profiles capture a complete Agentstrap configuration so you can switch between setups (for example a performance
profile and a visual profile) without losing either.</p>

<h2>What a profile stores</h2>
<ul>
  <li>Settings (launch, window, theme, integrations, mod selections where supported).</li>
  <li>Fast Flag configuration.</li>
  <li>Mod selections where the platform supports them.</li>
</ul>

<h2>Operations</h2>
<ul>
  <li>Create from current configuration.</li>
  <li>Create from a default template.</li>
  <li>Duplicate, rename, activate, export, import.</li>
  <li>Compare two profiles or current settings to a profile.</li>
  <li>Restore previous settings from a backup.</li>
</ul>

<div class="callout note">
  <strong>Safety.</strong> Profile activation creates a backup of the current configuration first. Imported profile
  data is validated; arbitrary imported content is never executed.
</div>
`,
  },
  {
    path: "features/backups.html",
    title: "Automatic backups",
    lede: "Automatic, integrity-checked snapshots of your Agentstrap configuration.",
    body: `
<p>Agentstrap can snapshot your configuration before risky operations and on a configurable schedule.</p>

<h2>Triggers</h2>
<ul>
  <li>Before profile activation.</li>
  <li>Before importing configuration.</li>
  <li>Before significant configuration changes.</li>
  <li>Before supported mod operations.</li>
  <li>Before application updates.</li>
  <li>On a configurable schedule.</li>
</ul>

<h2>Restore process</h2>
<ol>
  <li>Validate format and integrity.</li>
  <li>Show what will be restored.</li>
  <li>Create a safety backup of the current configuration.</li>
  <li>Apply restoration safely.</li>
  <li>Validate restored configuration.</li>
  <li>Report success or failure.</li>
</ol>

<p>Backups are stored in a user-accessible location and never include authentication cookies, passwords or tokens.</p>
`,
  },
  {
    path: "features/extensions.html",
    title: "Extensions",
    lede: "Optional Agentstrap extensions with a versioned manifest and least-privilege permissions.",
    body: `
<p>Extensions are optional add-ons. They are validated before installation and never run with administrator
privileges by default.</p>

<h2>Manifest</h2>
<p>Every extension ships a <code>manifest.json</code> with a unique id, display name, version, author, minimum
compatible Agentstrap version, supported operating systems, declared permissions, entry type and optional
dependencies. Malformed manifests are rejected.</p>

<h2>Security</h2>
<ul>
  <li>Path traversal in archive entries is blocked.</li>
  <li>Package integrity is verified where possible.</li>
  <li>Sensitive permissions require explicit consent.</li>
  <li>Authentication tokens and private configuration are not exposed.</li>
</ul>
`,
  },
  {
    path: "features/macos-support.html",
    title: "macOS support",
    lede: "Platform-aware architecture and honest capability reporting for macOS.",
    body: `
<p>Agentstrap keeps its Windows application intact and introduces abstractions for platform detection, capabilities,
paths, configuration storage, process launching, filesystem, logging, diagnostics, updates and backups.</p>

<h2>Capability status</h2>
<p>Every feature is classified as Supported, Experimental, Unsupported or Temporarily unavailable. The UI reflects
the actual capability of the current platform instead of scattering ad-hoc OS checks.</p>

<h2>Roblox on macOS</h2>
<p>Roblox has a native macOS client. Agentstrap detects it where possible and reports unsupported configurations
clearly. Windows-specific Fast Flags, registry settings, launch arguments, mods and integrations are not assumed to
work on macOS. Agentstrap never bypasses anti-cheat, authentication or client integrity checks.</p>

<div class="callout note">
  <strong>Limitation.</strong> macOS compilation and runtime verification require a Mac and the appropriate SDK,
  which may not be available in the current build environment.
</div>
`,
  },
];