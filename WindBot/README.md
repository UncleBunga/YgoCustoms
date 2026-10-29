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

Inside the EDOPro folder on the device (the one containing `deck`, `repositories` and `WindBot`):

1. Copy `WindBot/Executors/CustomBots.dll` from this repo to `EDOPro/WindBot/Executors/` (create the folder if needed).
2. Copy every `.ydk` in `WindBot/Decks/` to `EDOPro/WindBot/Decks/`.
3. Open `EDOPro/WindBot/bots.json` and add the entries from `bots-custom.json` inside the outer `[ ]`
   (put a comma after the previous entry).

## Using it

Host a game, click "Add AI" and pick one of the bots above from the deck list.

## Rebuilding the bot

Source is in `src/`. Build with the .NET SDK (`dotnet build -c Release`) and copy `bin/Release/net40/CustomBots.dll`
into `Executors/`. `CustomBots.csproj` references `ExecutorBase.dll` from an EDOPro install; adjust the HintPath if
EDOPro lives somewhere other than `D:\Games\ProjectIgnis`.
