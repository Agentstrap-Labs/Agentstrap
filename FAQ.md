**What OS does it run on?**
Windows only.

**Is this just Bloxstrap?**
It's a fork. Does everything Bloxstrap did (Bloxstrap dev has ended).

**Where's my data?**
Under `%LOCALAPPDATA%\Agentstrap`. See `docs/info/where-data-is-stored.html`.

**Why are server location / overlay / game history / Discord presence all dead?**
Activity tracking is the master switch. If it's off, all of them are off. Settings → Integrations → Enable activity tracking.

**I turned activity tracking back on but stuff is still off.**
Turning it off switches the dependents off too, and turning it back on doesn't bring them back. Re-enable each one by hand.

**Fast Flags do nothing.**
Flags apply at launch, so restart Roblox. Also make sure "Allow Agentstrap to manage Fast Flags" is on. Roblox's Sept 2025 allowlist ignores most custom flags now anyway.

**My mods vanished.**
Modifications folder got cleared, or Roblox got reinstalled in a way that skipped the copy step. See `docs/features/modding.html`.

**Overlay never shows up.**
Activity tracking off, exclusive fullscreen, or Roblox changed its logging. See `docs/features/server-connection-overlay.html`.

**Matchmaker does nothing.**
It's off, no place ID set, or no public server met your min free slots. See `docs/features/matchmaker.html`.

**Settings won't save.**
You have to hit Save in the bottom-right. Nothing writes until you do. Corrupt `Settings.json` makes it fall back to defaults (Diagnostics flags it).

**UI looks broken / half-translated.**
Try the plain theme — a missing custom theme file is a known issue. Or delete `Settings.json` and restart for a full reset. Some new pages are still hardcoded English.

**How do I move it to another drive?**
Close Agentstrap (and Roblox), move `%LOCALAPPDATA%\Agentstrap`, then launch from the new spot. See `docs/help/change-install-location.html`.

**How do I uninstall?**
Exit Agentstrap, remove it from Settings → Apps, delete `%LOCALAPPDATA%\Agentstrap`. Some registry stuff may linger — see `docs/help/uninstalling.html`.

**How do I go back to plain Roblox?**
Run Roblox's official installer once after removing Agentstrap to restore the `roblox://` handler. Roblox itself isn't removed.

## Errors

**"Launch failed — Windows Media Feature Pack (mfplat.dll) is missing."**
Roblox needs `mfplat.dll`. Install Media Feature Pack from Windows Optional Features.

**`roblox://` / `roblox-player://` links don't open Agentstrap.**
Command Center → Repair Roblox links. No admin needed.

**Nothing happens when you click Launch.**
Accept the UAC prompt. Check Roblox is actually installed (Command Center → Roblox Player card). If not, next launch installs it (needs internet).

**Launcher opens but Roblox never appears.**
Wait on slow connections. Check Command Center → Last launch (cancelled / failed / didn't complete). Check for `RobloxPlayerBeta.exe` in the version folder — if missing, see `docs/help/repairing-roblox-install.html`. Antivirus or policy might be blocking it (check the log).

**Roblox opens then closes immediately.**
Almost always a bad Fast Flag. Turn off Fast Flag management (Settings → Engine Settings, uncheck it, Save), restart Roblox. If it's fine, re-add flags one at a time and use Validate.

**Roblox never opens at all.**
Check Last launch status. Check for `RobloxPlayerBeta.exe`. Check Media Feature Pack. Check for pending config migration (Updates → Configuration → Check).

**Roblox crashes after a while.**
Turn off exclusive fullscreen (don't Alt+Enter). Turn off custom window attributes. Disable mods (set presets to Default). Update graphics drivers.

**Crashes on one specific game.**
Usually the game's own content. If it dies before assets load, check Fast Flags.

**Fast Flags have no effect.**
Roblox already running (restart), management toggle off, or the Sept 2025 allowlist is ignoring them.

**Rendering distance won't change.**
It writes `DFIntDebugFRMQualityLevelOverride`. Restart Roblox after changing. Roblox may ignore it under the allowlist, or the graphics slider may override.

**Texture or MSAA looks wrong.**
Set back to Automatic, save, restart. Check Fast Flag Editor with "Show preset flags" on.

**Discord Rich Presence doesn't show.**
Need the desktop app (not mobile/browser). Activity tracking on. "Show game activity" on. Check Discord settings (Activities → Roblox, visibility not Nobody, status Online). Try running Discord as admin. Turn off Roblox's built-in Discord connection to avoid conflicts.

**Join button missing in Discord.**
"Allow activity joining" on (Integrations), and only for public servers.

**Server location says "Not available".**
Address may not be publicly resolvable (private servers never queried). ipinfo rate-limited. ipinfo only knew the region. Also check "Query server location" is on and activity tracking is on.

**ipinfo rate limited.**
Turn off "Show server connection notifications" (overlay does its own lookup per server). Wait. Don't join games as fast as possible. Failed lookups are cached for the session.

**Bark test fails / notifications not sent.**
Activity tracking + "Send server connection notifications" on. Bark push key saved (Save credentials). Server address must be public IP. Repeats to same server suppressed for the configured window. For Bark test, check push key is current and Bark can show notifications on your phone.

**IPinfo test fails or no city.**
Test with "Test IPinfo connection". Need a plan with city/region data. No token = public quota = may be rate-limited.

**Credentials won't save.**
Stored with Windows DPAPI for the current user, never in Settings.json. If it fails, check the log and run Agentstrap normally (not as another user).

**Mods disappeared.**
Same as above — Modifications cleared or reinstall skipped copy. See `docs/features/modding.html`.

**Overlay never appears.**
Same as above — tracking off, exclusive fullscreen, or logging changed.

**Matchmaker does nothing.**
Same as above — off, no place ID, or no server met min slots.

**Settings won't save.**
Hit Save bottom-right. Corrupt Settings.json falls back to defaults (Diagnostics flags it).

**Interface looks broken.**
Try plain theme (missing custom theme file = invalid setting per Diagnostics). Delete Settings.json + restart for full reset. Try English for half-translated pages.

**Auto-update keeps asking to upgrade.**
Launched copy differs from installed one — Agentstrap asks before replacing. No keeps installed, yes copies running over.

**Newer release available but nothing happens.**
Update checking off (Settings → Quality of Life). You're newer than latest (no downgrades). Only stable releases count. Asset must be an .exe.

**Auto-update failed.**
Check log for `Bootstrapper::CheckForUpdates`. Blocked download, no internet, or another instance holding the updater mutex. Falls back to opening releases page.

**Permission errors.**
Diagnostics → Permissions checks write access to install folder, `%LOCALAPPDATA%\Roblox`, and temp dir via probe files.

**Diagnostics: missing files.**
Core files missing (Settings.json, State.json, Logs/Versions/Modifications folders). Missing folders recreated on demand = warning; missing core files = problem.

**Diagnostics: invalid settings.**
Bad process priority, unsupported locale, missing custom theme file, missing custom icon path, matchmaker on with no place ID, negative min slot count.

**Diagnostics: mod conflicts.**
Custom skybox with no images in sky folder, empty custom font file, or mod changes set but not applied.

**Diagnostics: pending migrations.**
Apply from Updates tab (Updates → Configuration → Check).

**Version mismatch / keeps asking to upgrade.**
`HandleUpgrade` compares versions and won't overwrite a newer install without asking. Run installer for the version you want from Releases.

## Troubleshooting

1. Run Diagnostics (Settings → Diagnostics → Run diagnostics). Eight checks at once, tells you which failed. Copy report → paste into bug report.
2. Open log folder (Advanced page or Command Center). Clear logs first for a clean single-session log.
3. Check Updates page for pending migrations and apply them.
4. Confirm latest release (Releases button).
5. Logs contain server addresses (ipinfo URLs with game server IPs). Read before posting publicly.

Agentstrap logs every session. Advanced page has Open logs / Clear logs. Roblox keeps its own logs in `%LOCALAPPDATA%\Roblox\logs` (Deployment page → Open Roblox logs).

If nothing you change helps, check Roblox's server status — a client that can't reach Roblox looks like a crash.

Still stuck? Open an issue with your diagnostics report and latest log attached.
