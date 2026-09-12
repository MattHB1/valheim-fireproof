# Build Fireproof and package Thunderstore + GitHub release artifacts.
# Mirrors ValheimPlus bin/create_releases.sh at a smaller scale.
#
# Usage:
#   .\scripts\create-release.ps1
#   .\scripts\create-release.ps1 -GitHubRelease   # also: gh release create with assets

param(
  [switch]$GitHubRelease
)

$ErrorActionPreference = "Stop"

$RepoRoot = Split-Path $PSScriptRoot -Parent
$Project = Join-Path $RepoRoot "Fireproof\Fireproof.csproj"
$PluginCs = Join-Path $RepoRoot "Fireproof\Fireproof.cs"
$OutDll = Join-Path $RepoRoot "Fireproof\bin\Release\net4.8\Fireproof.dll"
$Icon = Join-Path $RepoRoot "resources\icon.png"
$PackageReadme = Join-Path $RepoRoot "publish\README.md"
$Changelog = Join-Path $RepoRoot "CHANGELOG.md"

if (-not (Test-Path $Icon)) { throw "Missing Thunderstore icon: $Icon (256x256 PNG)" }
if (-not (Test-Path $PackageReadme)) { throw "Missing package README: $PackageReadme" }
if (-not (Test-Path $Changelog)) { throw "Missing changelog: $Changelog" }

$versionLine = Select-String -Path $PluginCs -Pattern 'public const string VERSION = "([^"]+)"' | Select-Object -First 1
if (-not $versionLine) { throw "Could not parse VERSION from Fireproof.cs" }
$Version = $versionLine.Matches[0].Groups[1].Value

$ReleaseDir = Join-Path $RepoRoot "release\$Version"
$TempDir = Join-Path $RepoRoot "release\temp"

Write-Host "Building Fireproof $Version ..."
dotnet build $Project -c Release
if ($LASTEXITCODE -ne 0) { throw "Build failed" }
if (-not (Test-Path $OutDll)) { throw "Missing build output: $OutDll" }

Remove-Item -Recurse -Force $ReleaseDir, $TempDir -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path $ReleaseDir | Out-Null

# Loose DLL for GitHub Releases / manual install
Copy-Item $OutDll (Join-Path $ReleaseDir "Fireproof.dll") -Force

# Thunderstore package layout (same shape as ValheimPlus)
$Ts = Join-Path $TempDir "Thunderstore"
$TsPlugins = Join-Path $Ts "BepInEx\plugins"
New-Item -ItemType Directory -Force -Path $TsPlugins | Out-Null
Copy-Item $Icon (Join-Path $Ts "icon.png") -Force
Copy-Item $PackageReadme (Join-Path $Ts "README.md") -Force
Copy-Item $Changelog (Join-Path $Ts "CHANGELOG.md") -Force
Copy-Item $OutDll (Join-Path $TsPlugins "Fireproof.dll") -Force

$manifest = @{
  name            = "Fireproof"
  version_number  = $Version
  website_url     = "https://github.com/MattHB1/valheim-fireproof"
  description     = "Turns off burning and fire damage for you - hearths, campfires, the lot."
  dependencies    = @("denikson-BepInExPack_Valheim-5.4.2350")
} | ConvertTo-Json -Depth 5
# Thunderstore expects UTF-8 without BOM-ish; .NET often writes BOM - strip if present
[IO.File]::WriteAllText((Join-Path $Ts "manifest.json"), $manifest + "`n")

$TsZip = Join-Path $ReleaseDir "Thunderstore.zip"
if (Test-Path $TsZip) { Remove-Item $TsZip -Force }
Compress-Archive -Path (Join-Path $Ts "*") -DestinationPath $TsZip -Force

Remove-Item -Recurse -Force $TempDir -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "Release artifacts in $ReleaseDir"
Get-ChildItem $ReleaseDir | Format-Table Name, Length
Write-Host "Upload Thunderstore.zip at https://thunderstore.io/c/valheim/create/package/"
Write-Host "(or drag the zip into the Thunderstore / r2modman upload UI)"

if ($GitHubRelease) {
  $tag = "v$Version"
  $dllAsset = Join-Path $ReleaseDir "Fireproof.dll"
  $notes = "Turns off burning and fire damage for you - hearths, campfires, the lot. Install via Thunderstore/r2modman when published, or drop Fireproof.dll into BepInEx/plugins."
  gh release view $tag -R MattHB1/valheim-fireproof 2>$null
  if ($LASTEXITCODE -eq 0) {
    Write-Host "GitHub release $tag already exists; uploading assets..."
    gh release upload $tag $dllAsset $TsZip -R MattHB1/valheim-fireproof --clobber
  } else {
    gh release create $tag $dllAsset $TsZip -R MattHB1/valheim-fireproof --title "Fireproof $Version" --notes $notes
  }
  Write-Host "GitHub release: https://github.com/MattHB1/valheim-fireproof/releases/tag/$tag"
}
