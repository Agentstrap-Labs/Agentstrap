<#
.SYNOPSIS
    Points Agentstrap's in-app help links at a different documentation site.

.DESCRIPTION
    Agentstrap's settings pages, dialogs and the launch menu link to the Bloxstrap
    wiki for help topics. This script rewrites those links so they point at the
    Agentstrap documentation site instead.

    Only files under the live Agentstrap project are touched. The vendored Bloxstrap
    source and the stale Agentstrap\Agentstrap\ duplicate tree are skipped - neither
    is part of the build.

    The script is idempotent: running it twice makes no further changes.

.PARAMETER BaseUrl
    The root of the new documentation site, with no trailing slash.
    For example: https://agentstrap.github.io

.PARAMETER DryRun
    Show what would change without writing anything.

.EXAMPLE
    .\Scripts\update-docs-links.ps1 -BaseUrl https://agentstrap.github.io

.EXAMPLE
    .\Scripts\update-docs-links.ps1 -BaseUrl https://agentstrap.github.io -DryRun
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$BaseUrl,

    [switch]$DryRun
)

$ErrorActionPreference = 'Stop'

$BaseUrl = $BaseUrl.TrimEnd('/')

$repoRoot = Split-Path -Parent $PSScriptRoot
$projectDir = Join-Path $repoRoot 'Agentstrap'

if (-not (Test-Path $projectDir)) {
    throw "Could not find the Agentstrap project at $projectDir"
}

# Old Bloxstrap wiki path -> path within the new documentation site.
# The new site is flat HTML, so each wiki slug becomes one .html file.
$routeMap = [ordered]@{
    'wiki/'                                          = ''
    'wiki/help/'                                     = 'help/'
    'wiki/info/'                                     = 'info/'
    'wiki/features/'                                 = 'features/'

    'wiki/features/activity-tracking/'               = 'features/activity-tracking.html'
    'wiki/features/custom-integrations/'             = 'features/custom-integrations.html'
    'wiki/features/discord-rich-presence/'           = 'features/discord-rich-presence.html'
    'wiki/features/dont-exit-to-desktop-app/'         = 'features/dont-exit-to-desktop-app.html'
    'wiki/features/engine-settings/'                 = 'features/engine-settings.html'
    'wiki/features/game-history/'                    = 'features/game-history.html'
    'wiki/features/modding/'                         = 'features/modding.html'
    'wiki/features/server-location-hints/'           = 'features/server-location-hints.html'
    'wiki/info/privacy-policy/'                      = 'info/privacy-policy.html'
    'wiki/info/translations/'                        = 'info/translations.html'
    'wiki/info/using-roblox-icons-for-shortcuts/'    = 'info/using-roblox-icons-for-shortcuts.html'
    'wiki/help/discord-rich-presence-not-working/'   = 'help/discord-rich-presence-not-working.html'
    'wiki/help/roblox-crashes-or-does-not-launch/'   = 'help/roblox-crashes.html'

    # No equivalent page exists for these, so they point at the closest thing.
    'wiki/help/bloxstrap-cannot-download-roblox/'    = 'help/troubleshooting.html'
}

# Files to leave alone: the vendored upstream source and the stale duplicate tree.
$excluded = @(
    (Join-Path $projectDir 'Agentstrap'),   # stale duplicate, excluded from the build
    (Join-Path $repoRoot 'Bloxstrap')       # vendored upstream source
)

function Test-Excluded {
    param([string]$Path)

    foreach ($prefix in $excluded) {
        if ($Path.StartsWith($prefix, [System.StringComparison]::OrdinalIgnoreCase)) {
            return $true
        }
    }

    return $false
}

function Convert-DocsLink {
    param([string]$Text)

    # Only touch wiki links. services.bloxstraplabs.com is an API host and stays put.
    $pattern = 'https://bloxstraplabs\.com/(wiki/[A-Za-z0-9\-_/]*)'

    return [regex]::Replace($Text, $pattern, {
        param($match)

        $oldPath = $match.Groups[1].Value
        $anchor = ''

        # Split the anchor off so it can be reattached to the new target.
        $hashIndex = $oldPath.IndexOf('#')
        if ($hashIndex -ge 0) {
            $anchor = $oldPath.Substring($hashIndex)
            $oldPath = $oldPath.Substring(0, $hashIndex)
        }

        # Longest match first, so wiki/features/modding/ wins over wiki/features/.
        $newPath = $null
        foreach ($key in ($routeMap.Keys | Sort-Object -Property Length -Descending)) {
            if ($oldPath -eq $key.TrimEnd('/') -or $oldPath.StartsWith($key)) {
                $newPath = $routeMap[$key] + $oldPath.Substring($key.Length)
                break
            }
        }

        if ($null -eq $newPath) {
            return $match.Value   # unmapped, leave it alone
        }

        $newPath = $newPath -replace '/\.html$', '.html'
        $newPath = $newPath -replace '(?<!\.html)/$', '/'

        if ($newPath -match '\.html$') {
            return "$BaseUrl/$newPath$anchor"
        }

        return "$BaseUrl/$newPath$anchor"
    })
}

$files = Get-ChildItem -Path $projectDir -Recurse -File -Include *.cs, *.xaml |
    Where-Object { -not (Test-Excluded $_.FullName) }

# Reads a file as text, keeping track of the byte order mark it used.
function Read-Text {
    param([string]$Path)

    $bytes = [System.IO.File]::ReadAllBytes($Path)

    if ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF) {
        return [PSCustomObject]@{
            Text = [System.Text.Encoding]::UTF8.GetString($bytes, 3, $bytes.Length - 3)
            Bom  = 'utf8'
        }
    }

    if ($bytes.Length -ge 2 -and $bytes[0] -eq 0xFF -and $bytes[1] -eq 0xFE) {
        return [PSCustomObject]@{
            Text = [System.Text.Encoding]::Unicode.GetString($bytes, 2, $bytes.Length - 2)
            Bom  = 'utf16'
        }
    }

    return [PSCustomObject]@{
        Text = [System.Text.Encoding]::UTF8.GetString($bytes)
        Bom  = 'none'
    }
}

# Writes text back with the same byte order mark it was read with, so that
# rewriting a file never changes its encoding.
function Write-Text {
    param([string]$Path, [string]$Text, [string]$Bom)

    switch ($Bom) {
        'utf8' {
            $preamble = [byte[]](0xEF, 0xBB, 0xBF)
            $body = [System.Text.Encoding]::UTF8.GetBytes($Text)
        }
        'utf16' {
            $preamble = [byte[]](0xFF, 0xFE)
            $body = [System.Text.Encoding]::Unicode.GetBytes($Text)
        }
        default {
            $preamble = [byte[]]@()
            $body = [System.Text.Encoding]::UTF8.GetBytes($Text)
        }
    }

    $out = New-Object byte[] ($preamble.Length + $body.Length)
    [System.Array]::Copy($preamble, 0, $out, 0, $preamble.Length)
    [System.Array]::Copy($body, 0, $out, $preamble.Length, $body.Length)

    [System.IO.File]::WriteAllBytes($Path, $out)
}

$changed = 0
$linkCount = 0

foreach ($file in $files) {
    $fileData = Read-Text $file.FullName
    $original = $fileData.Text

    if ([string]::IsNullOrEmpty($original)) { continue }
    if ($original -notmatch 'bloxstraplabs\.com/wiki') { continue }

    $updated = Convert-DocsLink $original
    if ($updated -eq $original) { continue }

    $before = ([regex]::Matches($original, 'https://bloxstraplabs\.com/wiki')).Count
    $linkCount += $before

    if (-not $DryRun) {
        Write-Text -Path $file.FullName -Text $updated -Bom $fileData.Bom
    }

    Write-Host ("  {0} ({1} link(s))" -f $file.FullName.Substring($repoRoot.Length + 1), $before)
    $changed++
}

Write-Host ""
if ($DryRun) {
    Write-Host "Dry run: $changed file(s), $linkCount link(s) would point at $BaseUrl"
} else {
    Write-Host "Updated $changed file(s), $linkCount link(s) now point at $BaseUrl"
}
