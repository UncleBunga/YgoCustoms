# Custom WindBot decks

AI opponents for the custom decks in this repo and for the owner's irl decks: **Slasher**, **IRL Chaos**, **IRL Frog**,
**IRL Dragon Ruler**, **IRL HERO**, **IRL Cyber Dragon**, **IRL Yugi**, **IRL Kaiba**, **IRL Yubel** and
**IRL Exodia**.

EDOPro's repo sync only brings cards, scripts and pictures, so the bot needs a one-time install on each device
(and again after pulling a bot update or updating EDOPro, since EDOPro updates replace `WindBot/bots.json`).

## Windows

Close EDOPro, then run `install-windbot.ps1` from this folder (right-click > Run with PowerShell), or:

```
powershell -ExecutionPolicy Bypass -File install-windbot.ps1 -EdoPath "D:\Games\ProjectIgnis"
```

If the repo is cloned inside EDOPro (`<EDOPro>/repositories/<name>`), `-EdoPath` can be left out.

## Android

EDOPro only lists bots that are in `WindBot/bots.json` (decks sitting in the folder are not enough). This folder is a
complete drop-in: `bots.json` lists every official bot plus the custom ones, and `Decks/` and `Dialogs/` hold all of
their files. The official part is copied from Project Ignis's current WindBot (github.com/ProjectIgnis/windbot), which is
what the Android app runs; if an EDOPro update adds new official bots, refresh it from there.

Inside the EDOPro folder on the device (the one containing `deck`, `repositories` and `WindBot`), copy these from this
repo's `WindBot` folder into `EDOPro/WindBot/`, overwriting:

1. `bots.json`
2. `Executors/CustomBots.dll` (create the `Executors` folder if needed)
3. everything in `Decks/` and `Dialogs/`

Restart EDOPro afterwards. Redo this after pulling a bot update or updating EDOPro (updates replace `bots.json`).

## Using it

Host a game, click "Add AI" and pick one of the bots above from the deck list.

## Rebuilding the bot

Source is in `src/`. Build with the .NET SDK (`dotnet build -c Release`) and copy `bin/Release/net40/CustomBots.dll`
into `Executors/`. `CustomBots.csproj` references `ExecutorBase.dll` from an EDOPro install; adjust the HintPath if
EDOPro lives somewhere other than `D:\Games\ProjectIgnis`.
