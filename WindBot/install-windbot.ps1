# Installs the custom WindBot decks from this repo into EDOPro's WindBot folder (Windows).
# Run from anywhere:  powershell -ExecutionPolicy Bypass -File install-windbot.ps1 [-EdoPath "D:\Games\ProjectIgnis"]
# Re-run after pulling bot updates, and after EDOPro updates (they replace WindBot\bots.json).
param(
    [string]$EdoPath = ""
)
$ErrorActionPreference = "Stop"
$here = Split-Path -Parent $MyInvocation.MyCommand.Path

if (-not $EdoPath) {
    # Repo cloned by EDOPro: <EDOPro>\repositories\<name>\WindBot
    $guess = Resolve-Path (Join-Path $here "..\..\..") -ErrorAction SilentlyContinue
    if ($guess -and (Test-Path (Join-Path $guess "WindBot\WindBot.exe"))) { $EdoPath = $guess.Path }
}
if (-not $EdoPath -or -not (Test-Path (Join-Path $EdoPath "WindBot"))) {
    throw "EDOPro folder not found. Pass it with -EdoPath, e.g. -EdoPath `"D:\Games\ProjectIgnis`""
}
$wb = Join-Path $EdoPath "WindBot"

New-Item -ItemType Directory -Force (Join-Path $wb "Executors") | Out-Null
try {
    Copy-Item (Join-Path $here "Executors\*.dll") (Join-Path $wb "Executors") -Force
} catch {
    throw "Could not replace the bot DLL - close EDOPro (and any running AI duels) and run this again."
}
# only our decks: Decks\ also carries the official AI decks for Android drop-in, which must match the local WindBot
Copy-Item (Join-Path $here "Decks\AI_IRL_*.ydk"), (Join-Path $here "Decks\AI_Slasher.ydk") (Join-Path $wb "Decks") -Force

# Add our entries to bots.json (keeping the official ones), replacing older copies of the same deck
$botsFile = Join-Path $wb "bots.json"
Copy-Item $botsFile "$botsFile.bak" -Force
# piping enumerates the parsed array (Windows PowerShell 5.1 returns JSON arrays as a single object)
$bots = @(Get-Content $botsFile -Raw | ConvertFrom-Json | ForEach-Object { $_ })
$custom = @(Get-Content (Join-Path $here "bots-custom.json") -Raw | ConvertFrom-Json | ForEach-Object { $_ })
$names = $custom | ForEach-Object { $_.deck }
$merged = @($bots | Where-Object { $names -notcontains $_.deck }) + $custom
$json = ConvertTo-Json -InputObject $merged -Depth 5
[System.IO.File]::WriteAllText($botsFile, $json, (New-Object System.Text.UTF8Encoding($false)))

Write-Host "Installed $($custom.Count) custom bot(s) into $wb"
