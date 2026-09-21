# Fireproof

Turns off burning and fire damage for you from everywhere in the game - fire attacks (including Fader and Kall), stepping on camp fires by accident, walking around on LAVA, the lot. Also avoids the smoke cough from standing in fire smoke.

This also removes the slow effect from walking in lava.

## Install

1. Open [r2modman](https://r2modman.com/) (or Thunderstore Mod Manager) → Valheim → your profile.
2. **Online** → search `DevDonkey-Fireproof`, or install from the package page once published.
3. Launch the game through the mod manager.

Manual install: put `Fireproof.dll` in `BepInEx/plugins/` (requires [BepInExPack_Valheim](https://thunderstore.io/c/valheim/p/denikson/BepInExPack_Valheim/)).

## Notes

- Only affects you — friends still burn unless they install it too.
- Doesn't touch cold, wet, poison, and the rest.
- Other mods: [DevDonkey on Thunderstore](https://thunderstore.io/c/valheim/p/DevDonkey/)

## Changelog

See [CHANGELOG.md](CHANGELOG.md).

## Build

Requires a .NET SDK with the net4.8 targeting pack, plus publicized Valheim / BepInEx reference assemblies (see HintPaths in `Fireproof/Fireproof.csproj`).

```powershell
.\scripts\build-deploy.ps1
.\scripts\create-release.ps1
```

`create-release.ps1` writes `release/<version>/Thunderstore.zip` for upload. Bump `VERSION` in `Fireproof/Fireproof.cs` before each Thunderstore publish.
