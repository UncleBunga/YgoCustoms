using System.Linq;
using YGOSharp.OCGWrapper.Enums;

namespace WindBot.Game.AI.Decks
{
    // IRL Kaiba: Blue-Eyes extenders, with the XYZ Union engine there only to turbo Obelisk (the user's replay line):
    //   Union Hangar (Y-Dragon Head) > Dictator of D. > Y-Dragon Head (Hangar equips Z-Metal Tank) > Spirit with Eyes of
    //   Blue (Mausoleum) > Union Carrier > Union Controller (Union Activation) > Union Activation sends X-Head Cannon for
    //   X-Cross Cannon > X-Cross equips Z-Zillion Tank > XYZ-Hyper Dragon Cannon.
    //   Hyper Dragon Cannon then banishes itself for X-Head / Y-Dragon / Z-Metal; three LIGHT Machines at once wake
    //   Union Activation in the GY, which adds Obelisk and Normal Summons it off those three. Soul Energy MAX!!! finishes.
    [Deck("IRL_Kaiba", "AI_IRL_Kaiba")]
    public class IrlKaibaExecutor : CustomExecutorBase
    {
        public class CardId
        {
            public const int BlueEyes = 89631139;
            public const int XHeadCannon = 62651957;
            public const int Obelisk = 10000000;
            public const int Alternative = 38517737;
            public const int JetDragon = 30576089;
            public const int AbyssDragon = 64202399;
            public const int DragonSpiritOfWhite = 45467446;
            public const int SolidDragon = 57043986;
            public const int NeoKaiserGlider = 45885288;
            public const int KaiserVorseRaider = 93927067;
            public const int XCrossCannon = 70860415;
            public const int NeoKaiserSeaHorse = 54332792;
            public const int YYareHead = 6355563;
            public const int YDragonHead = 65622692;
            public const int ZZillionTank = 33744268;
            public const int ZMetalTank = 64500000;
            public const int DictatorOfD = 66961194;
            public const int MysticalElf = 15613529;
            public const int Peten = 52624755;
            public const int Protector = 72855441;
            public const int WhiteStoneOfAncients = 71039903;
            public const int Master = 45644898;
            public const int WhiteStoneOfLegend = 79814787;
            public const int Sage = 8240199;
            public const int MaidenOfWhite = 17947697;
            public const int MaidenWithEyesOfBlue = 88241506;
            public const int ChaosMax = 55410871;
            public const int ChaosDragon = 20654247;
            public const int BurstStream = 17655904;
            public const int Roar = 17725109;
            public const int TradeIn = 38120068;
            public const int UnionActivation = 38527680;
            public const int Melody = 48800175;
            public const int Wishes = 80326401;
            public const int Bingo = 93437091;
            public const int ChaosForm = 21082832;
            public const int MajestyWithEyes = 2783661;
            public const int UltimateFusion = 71143015;
            public const int FistOfFate = 79868386;
            public const int BreakingRuinGod = 85182315;
            public const int EnemyController = 98045062;
            public const int VisionWithEyes = 35659410;
            public const int Mausoleum = 24382602;
            public const int UnionHangar = 66399653;
            public const int DestinedRivals = 22634473;
            public const int MajestyOfTheWhiteDragons = 43219114;
            public const int UltimateCreature = 56920308;
            public const int SoulEnergyMax = 79339613;
            public const int TrueLight = 62089826;

            public const int UltimateDragon = 23995346;
            public const int NeoUltimateDragon = 56532353;
            public const int TwinBurst = 2129638;
            public const int HyperDragonCannon = 75748977;
            public const int TyrantDragon = 11443677;
            public const int XYZDragonCannon = 91998119;
            public const int UnionController = 2133971;
            public const int YZTankDragon = 25119460;
            public const int UltimateSpirit = 89604813;
            public const int Azure = 40908371;
            public const int SpiritDragon = 59822133;
            public const int Indigo = 16699558;
            public const int UnionCarrier = 83152482;
            public const int DivineDragonOfObelisk = 91900009;
            public const int Hieratic = 24361622;
            public const int SpiritWithEyesOfBlue = 42097666;
        }

        static readonly int[] Tuners = { CardId.MaidenWithEyesOfBlue, CardId.Protector, CardId.Master, CardId.Sage, CardId.WhiteStoneOfAncients, CardId.WhiteStoneOfLegend, CardId.MaidenOfWhite };
        static readonly int[] BigBlueEyes = { CardId.BlueEyes, CardId.Alternative, CardId.JetDragon, CardId.AbyssDragon, CardId.SolidDragon, CardId.DragonSpiritOfWhite };

        public IrlKaibaExecutor(GameAI ai, Duel duel)
            : base(ai, duel)
        {
            // Quick responses
            AddExecutor(ExecutorType.Activate, CardId.HyperDragonCannon, HyperCannonEffect);
            AddExecutor(ExecutorType.Activate, CardId.SolidDragon, SolidDragonEffect);
            AddExecutor(ExecutorType.Activate, CardId.MaidenWithEyesOfBlue, () => { Pick(CardId.BlueEyes); return true; });
            AddExecutor(ExecutorType.Activate, CardId.MysticalElf, () => Duel.LastChainPlayer == 1 || Card.Location != CardLocation.Hand);
            AddExecutor(ExecutorType.Activate, CardId.UltimateSpirit, () => Duel.LastChainPlayer == 1 || Card.Location != CardLocation.MonsterZone);
            AddExecutor(ExecutorType.Activate, CardId.SpiritDragon, () => Duel.LastChainPlayer == 1 || Card.Location != CardLocation.MonsterZone);
            AddExecutor(ExecutorType.Activate, CardId.JetDragon, JetDragonEffect);
            AddExecutor(ExecutorType.Activate, CardId.XYZDragonCannon, () => TargetEnemyCard() && Bot.Hand.Count > 0);
            AddStaples();

            // Obelisk payoffs
            AddExecutor(ExecutorType.Activate, CardId.SoulEnergyMax, SoulEnergyEffect);
            AddExecutor(ExecutorType.Activate, CardId.FistOfFate, () => HasObelisk && Enemy.GetMonsters().Any(m => m.IsFaceup() && m.HasType(CardType.Effect)) && TargetEnemyMonster());
            AddExecutor(ExecutorType.Activate, CardId.Obelisk, ObeliskEffect);
            AddExecutor(ExecutorType.SpSummon, CardId.DivineDragonOfObelisk, () => HasObelisk && Bot.HasInMonstersZone(CardId.BlueEyes));
            AddExecutor(ExecutorType.Activate, CardId.DivineDragonOfObelisk, DivineDragonEffect);
            AddExecutor(ExecutorType.Activate, CardId.BreakingRuinGod, () => Card.Location == CardLocation.Grave || (!HasObelisk && (Bot.HasInHand(CardId.Obelisk) || Bot.HasInGraveyard(CardId.Obelisk))));
            AddExecutor(ExecutorType.Activate, CardId.UnionActivation, UnionActivationEffect);

            // Union line into XYZ-Hyper Dragon Cannon
            AddExecutor(ExecutorType.Activate, CardId.UnionHangar, UnionHangarEffect);
            AddExecutor(ExecutorType.SpSummon, CardId.DictatorOfD, () => { Pick(CardId.BlueEyes); return true; });
            AddExecutor(ExecutorType.Activate, CardId.DictatorOfD, DictatorEffect);
            AddExecutor(ExecutorType.Summon, CardId.YDragonHead, () => Bot.HasInSpellZone(CardId.UnionHangar) || Bot.HasInHand(CardId.UnionActivation));
            AddExecutor(ExecutorType.SpSummon, CardId.SpiritWithEyesOfBlue, SpiritWithEyesSummon);
            AddExecutor(ExecutorType.Activate, CardId.SpiritWithEyesOfBlue, SpiritWithEyesEffect);
            AddExecutor(ExecutorType.SpSummon, CardId.UnionCarrier, () => UnionLineActive);
            AddExecutor(ExecutorType.Activate, CardId.UnionCarrier, () => { Pick(CardId.ZZillionTank, CardId.YYareHead, CardId.ZMetalTank); return true; });
            AddExecutor(ExecutorType.SpSummon, CardId.UnionController, () => UnionLineActive);
            AddExecutor(ExecutorType.Activate, CardId.UnionController, () => { Pick(CardId.UnionActivation, CardId.UnionHangar); Pick(CardId.XCrossCannon, CardId.YYareHead, CardId.ZZillionTank); return true; });
            AddExecutor(ExecutorType.SpSummon, CardId.XCrossCannon);
            AddExecutor(ExecutorType.Activate, CardId.XCrossCannon, () => { Pick(CardId.ZZillionTank, CardId.YYareHead); return true; });
            AddExecutor(ExecutorType.Activate, CardId.YYareHead, UnionMonsterEffect);
            AddExecutor(ExecutorType.Activate, CardId.ZZillionTank, UnionMonsterEffect);
            AddExecutor(ExecutorType.Activate, CardId.YDragonHead, UnionMonsterEffect);
            AddExecutor(ExecutorType.Activate, CardId.ZMetalTank, UnionMonsterEffect);
            AddExecutor(ExecutorType.SpSummon, CardId.HyperDragonCannon);
            AddExecutor(ExecutorType.SpSummon, CardId.XYZDragonCannon);

            // Blue-Eyes engine
            AddExecutor(ExecutorType.Activate, CardId.Melody, MelodyEffect);
            AddExecutor(ExecutorType.Activate, CardId.Wishes, WishesEffect);
            AddExecutor(ExecutorType.Activate, CardId.Bingo, () => { Pick(CardId.BlueEyes, CardId.Alternative, CardId.TrueLight, CardId.Roar); return true; });
            AddExecutor(ExecutorType.Activate, CardId.TradeIn, () => DiscardLevel8());
            AddExecutor(ExecutorType.SpSummon, CardId.Alternative);
            AddExecutor(ExecutorType.Activate, CardId.Alternative, () => TargetEnemyMonster());
            AddExecutor(ExecutorType.Summon, CardId.Sage, () => true);
            AddExecutor(ExecutorType.Activate, CardId.Sage, SageEffect);
            AddExecutor(ExecutorType.Summon, CardId.Protector, () => true);
            AddExecutor(ExecutorType.Activate, CardId.Protector, SageEffect);
            AddExecutor(ExecutorType.Activate, CardId.Master, SageEffect);
            AddExecutor(ExecutorType.Activate, CardId.WhiteStoneOfAncients, () => { Pick(CardId.BlueEyes, CardId.JetDragon, CardId.AbyssDragon); return true; });
            AddExecutor(ExecutorType.Activate, CardId.AbyssDragon, () => { Pick(CardId.ChaosForm, CardId.UltimateFusion); Pick(CardId.BlueEyes, CardId.Alternative, CardId.Obelisk); return true; });
            AddExecutor(ExecutorType.Activate, CardId.TrueLight, TrueLightEffect);
            AddExecutor(ExecutorType.Activate, CardId.Roar, () => { Pick(CardId.BlueEyes, CardId.JetDragon, CardId.AbyssDragon); return true; });
            AddExecutor(ExecutorType.Activate, CardId.VisionWithEyes, () => Bot.HasInHand(CardId.BlueEyes));
            AddExecutor(ExecutorType.Activate, CardId.ChaosForm, () => { Pick(CardId.ChaosMax, CardId.ChaosDragon); return true; });
            AddExecutor(ExecutorType.Activate, CardId.MajestyOfTheWhiteDragons, () => Enemy.GetFieldCount() > 0);
            AddExecutor(ExecutorType.Activate, CardId.BurstStream, () => Enemy.GetMonsterCount() > 0 && Bot.HasInMonstersZone(CardId.BlueEyes));
            AddExecutor(ExecutorType.Activate, CardId.DestinedRivals, () => Duel.LastChainPlayer == 1 || Enemy.GetMonsters().Count(m => m.IsFaceup() && m.HasType(CardType.Effect)) >= 2);
            AddExecutor(ExecutorType.Activate, CardId.UltimateFusion, () => { Pick(CardId.NeoUltimateDragon, CardId.UltimateDragon, CardId.TwinBurst, CardId.TyrantDragon); return true; });
            AddExecutor(ExecutorType.SpSummon, CardId.TwinBurst, () => Bot.GetMonsters().Count(m => m.IsCode(CardId.BlueEyes)) >= 2 && !Bot.HasInHand(CardId.UltimateFusion));
            AddExecutor(ExecutorType.SpSummon, CardId.UltimateSpirit);
            AddExecutor(ExecutorType.SpSummon, CardId.SpiritDragon);
            AddExecutor(ExecutorType.SpSummon, CardId.Azure);
            // Indigo-Eyes breaks full boards: its Xyz Summon negates every face-up card they control
            AddExecutor(ExecutorType.SpSummon, CardId.Indigo, () => Enemy.GetMonsters().Count(m => m.IsFaceup()) + Enemy.GetSpells().Count(c => c.IsFaceup()) >= 3
                || Bot.GetMonsters().Count(m => m.Level == 8 && m.IsFaceup()) >= 2 && !Bot.HasInExtra(CardId.SpiritDragon));
            AddExecutor(ExecutorType.Activate, CardId.Indigo, () => true);
            AddExecutor(ExecutorType.Activate, CardId.Mausoleum, MausoleumEffect);
            AddExecutor(ExecutorType.SpSummon, CardId.NeoKaiserSeaHorse);
            AddExecutor(ExecutorType.SpSummon, CardId.KaiserVorseRaider);

            AddExecutor(ExecutorType.Summon, CardId.Obelisk, () => true);
            // no starter: Peten (Level 3 Spellcaster) links into Spirit with Eyes of Blue > Mausoleum > Blue-Eyes
            AddExecutor(ExecutorType.Summon, CardId.Peten, () => Bot.HasInExtra(CardId.SpiritWithEyesOfBlue));
            AddExecutor(ExecutorType.Summon, CardId.MaidenOfWhite, () => !TrueLightUp);
            AddExecutor(ExecutorType.Activate, CardId.MaidenOfWhite, MaidenOfWhiteEffect);
            AddExecutor(ExecutorType.Summon, CardId.Master, () => Bot.Graveyard.Any(c => c.IsCode(Tuners)));
            AddExecutor(ExecutorType.SummonOrSet, () => BestFallbackSummon());
            AddExecutor(ExecutorType.SpellSet, SetTrapOrQuickPlay);
            AddExecutor(ExecutorType.Repos, DefaultMonsterRepos);
            AddExecutor(ExecutorType.Activate, GenericActivate);
        }

        protected override int[] AddToHandPriority => new[] {
            CardId.Obelisk, CardId.MaidenOfWhite, CardId.TrueLight, CardId.BlueEyes, CardId.Alternative, CardId.YDragonHead, CardId.UnionActivation, CardId.XCrossCannon,
            CardId.MaidenWithEyesOfBlue, CardId.Sage, CardId.Mausoleum, CardId.JetDragon, CardId.ChaosForm
        };
        protected override int[] SpSummonPriority => new[] {
            CardId.XHeadCannon, CardId.YDragonHead, CardId.ZMetalTank, CardId.BlueEyes, CardId.Alternative, CardId.JetDragon,
            CardId.SolidDragon, CardId.AbyssDragon, CardId.Obelisk
        };
        protected override int[] ExpendablePriority => new[] {
            CardId.Peten, CardId.WhiteStoneOfAncients, CardId.WhiteStoneOfLegend, CardId.DictatorOfD,
            CardId.NeoKaiserGlider, CardId.KaiserVorseRaider, CardId.SpiritWithEyesOfBlue, CardId.UnionCarrier
        };

        bool HasObelisk => Bot.GetMonsters().Any(m => m.IsFaceup() && m.IsCode(CardId.Obelisk));
        // the Union line is on while Obelisk is still to come
        bool UnionLineActive => !HasObelisk && (MaybeInDeck(CardId.Obelisk, 1) || Bot.HasInHand(CardId.Obelisk))
            && (Bot.HasInSpellZone(CardId.UnionHangar) || Bot.HasInHand(CardId.UnionActivation) || Bot.GetMonsters().Any(m => m.IsCode(CardId.YDragonHead, CardId.ZMetalTank, CardId.UnionController)));

        // ---------- Union > Obelisk ----------

        bool UnionHangarEffect()
        {
            if (Card.Location == CardLocation.Hand) { Pick(CardId.YDragonHead, CardId.ZMetalTank, CardId.YYareHead); return true; }
            Pick(Bot.GetMonsters().Where(m => m.IsFaceup() && m.HasRace(CardRace.Machine)));
            Pick(CardId.ZMetalTank, CardId.ZZillionTank, CardId.YYareHead);
            return true;
        }

        bool DictatorEffect()
        {
            if (Card.Location == CardLocation.Hand) { Pick(CardId.BlueEyes); return true; }
            // discard a Blue-Eyes card to revive a Blue-Eyes
            if (!Bot.Graveyard.Any(c => c.IsCode(BigBlueEyes))) return false;
            Pick(Bot.Hand.Where(c => c.IsCode(CardId.BlueEyes, CardId.TrueLight, CardId.BurstStream, CardId.Wishes)).OrderBy(c => c.IsCode(CardId.BlueEyes) ? 1 : 0));
            Pick(Bot.Graveyard.Where(c => c.IsCode(BigBlueEyes)).OrderByDescending(c => c.Attack));
            return true;
        }

        bool SpiritWithEyesSummon()
        {
            // link off Dictator / a Level 4 Dragon for Mausoleum
            return Bot.GetMonsters().Any(m => m.IsFaceup() && m.Level <= 4 && (m.HasRace(CardRace.Dragon) || m.HasRace(CardRace.SpellCaster)));
        }

        bool SpiritWithEyesEffect()
        {
            if (Desc(CardId.SpiritWithEyesOfBlue, 0) || GenericTrigger) { Pick(CardId.Mausoleum); return true; }
            Pick(CardId.BlueEyes, CardId.Alternative, CardId.SolidDragon, CardId.JetDragon);
            return true;
        }

        bool UnionMonsterEffect()
        {
            if (Card.Location == CardLocation.SpellZone) return true; // unequip and Special Summon
            if (Card.Location != CardLocation.MonsterZone) return true;
            // Special Summoned: equip a banished / hand Level 4 LIGHT Machine
            if (GenericTrigger) return true;
            return false;
        }

        bool HyperCannonEffect()
        {
            if (!MyTurn)
            {
                // their turn: discard to destroy
                if (Duel.LastChainPlayer == 0 || Bot.Hand.Count == 0) return false;
                return TargetEnemyCard();
            }
            // our turn: split into X / Y / Z to wake Union Activation for Obelisk
            if (!Bot.HasInGraveyard(CardId.UnionActivation) || HasObelisk) return false;
            if (!MaybeInDeck(CardId.Obelisk, 1) && !Bot.HasInGraveyard(CardId.Obelisk)) return false;
            Pick(CardId.XYZDragonCannon);
            return true;
        }

        bool UnionActivationEffect()
        {
            if (Card.Location == CardLocation.Grave) { Pick(CardId.Obelisk); return true; }
            // send X-Head Cannon for X-Cross Cannon
            Pick(CardId.XHeadCannon, CardId.YDragonHead, CardId.ZMetalTank);
            Pick(CardId.XCrossCannon, CardId.YYareHead, CardId.ZZillionTank);
            return true;
        }

        bool ObeliskEffect()
        {
            if (!MyTurn || Duel.Phase != DuelPhase.Main1) return false;
            var fodder = Bot.GetMonsters().Where(m => m != Card).OrderBy(CardValue).Take(2).ToList();
            if (fodder.Count < 2 || Enemy.GetMonsterCount() < 2) return false;
            AI.SelectMaterials(fodder);
            Pick(fodder);
            return true;
        }

        bool SoulEnergyEffect()
        {
            if (Card.Location == CardLocation.Grave) return !HasObelisk;
            if (!HasObelisk) return false;
            var fodder = Bot.GetMonsters().Where(m => m.IsFaceup() && !m.IsCode(CardId.Obelisk)).OrderBy(CardValue).Take(2).ToList();
            if (fodder.Count < 2) return false;
            Pick(fodder);
            return true;
        }

        bool DivineDragonEffect()
        {
            var fodder = Bot.GetMonsters().Concat(Bot.Hand.Where(c => c.IsMonster())).Where(c => c != Card).OrderBy(CardValue).Take(2).ToList();
            if (fodder.Count < 2 || Enemy.GetFieldCount() < 2) return false;
            Pick(fodder);
            return true;
        }

        // ---------- Blue-Eyes ----------

        // True Light is the goal: Maiden of White places it, it revives / protects Blue-Eyes and Sets their Spells/Traps
        bool TrueLightUp => Bot.GetSpells().Any(c => c.IsFaceup() && c.IsCode(CardId.TrueLight));

        bool MaidenOfWhiteEffect()
        {
            if (Card.Location == CardLocation.Grave) return true; // returns when Blue-Eyes White Dragon is Special Summoned
            if (TrueLightUp) return false;
            Pick(CardId.TrueLight);
            return true;
        }

        bool MausoleumEffect()
        {
            if (Card.Location == CardLocation.Grave) { Pick(CardId.BurstStream); return true; }
            if (Card.Location == CardLocation.Hand) return true;
            // send Blue-Eyes White Dragon from the Deck so Spirit with Eyes of Blue / True Light can bring it back
            var t = Bot.GetMonsters().Where(m => m.IsFaceup()).OrderByDescending(m => m.Attack).FirstOrDefault();
            if (t == null) return false;
            Pick(t);
            Pick(CardId.BlueEyes);
            return true;
        }

        bool DiscardLevel8()
        {
            var t = Bot.Hand.Where(c => c.Level == 8 && c.IsMonster()).OrderBy(c => c.IsCode(CardId.BlueEyes) && Bot.Hand.Count(x => x.IsCode(CardId.BlueEyes)) == 1 ? 1 : 0).FirstOrDefault();
            if (t == null) return false;
            Pick(t);
            return true;
        }

        bool MelodyEffect()
        {
            var discard = Bot.Hand.Where(c => c != Card).OrderBy(c => c.IsCode(CardId.WhiteStoneOfLegend, CardId.WhiteStoneOfAncients) ? 0 : 1)
                .ThenBy(c => Rank(ExpendablePriority, c)).ThenBy(CardValue).FirstOrDefault();
            if (discard == null) return false;
            Pick(discard);
            Pick(CardId.BlueEyes, CardId.Alternative, CardId.JetDragon);
            return true;
        }

        bool WishesEffect()
        {
            if (Card.Location == CardLocation.Grave) return true;
            var discard = Bot.Hand.Where(c => c != Card).OrderBy(c => c.IsCode(CardId.WhiteStoneOfLegend, CardId.WhiteStoneOfAncients) ? 0 : 1)
                .ThenBy(c => Rank(ExpendablePriority, c)).ThenBy(CardValue).FirstOrDefault();
            if (discard == null) return false;
            Pick(discard);
            Pick(TrueLightUp ? new[] { CardId.MaidenWithEyesOfBlue, CardId.Sage } : new[] { CardId.MaidenOfWhite, CardId.MaidenWithEyesOfBlue, CardId.Sage });
            Pick(CardId.TrueLight, CardId.Roar, CardId.ChaosForm, CardId.BurstStream);
            return true;
        }

        bool SageEffect()
        {
            if (Card.Location == CardLocation.MonsterZone && (GenericTrigger || Desc(Card.Id, 0)) && !Card.IsCode(CardId.Protector))
            {
                if (!TrueLightUp && MaybeInDeck(CardId.MaidenOfWhite, 1)) Pick(CardId.MaidenOfWhite, CardId.MaidenWithEyesOfBlue, CardId.WhiteStoneOfLegend);
                else Pick(CardId.MaidenWithEyesOfBlue, CardId.WhiteStoneOfLegend, CardId.WhiteStoneOfAncients, CardId.Protector);
                return true;
            }
            // send an Effect Monster we control to the GY for a Blue-Eyes from hand
            if (!Bot.Hand.Any(c => c.IsCode(BigBlueEyes))) return false;
            var fodder = Bot.GetMonsters().Where(m => m.IsFaceup() && m.HasType(CardType.Effect) && m.Attack < 2500).OrderBy(CardValue).FirstOrDefault();
            if (fodder == null) return false;
            Pick(fodder);
            Pick(CardId.BlueEyes, CardId.Alternative, CardId.JetDragon, CardId.AbyssDragon);
            return true;
        }

        bool SolidDragonEffect()
        {
            if (Duel.CurrentChain.Count > 0) return Duel.LastChainPlayer == 1;
            return TargetEnemyMonster();
        }

        bool JetDragonEffect()
        {
            if (Card.Location == CardLocation.MonsterZone) return TargetEnemyCard();
            return true;
        }

        bool TrueLightEffect()
        {
            // negated True Light is destroyed and destroys our whole board
            if (EnemySpellTrapNegateReady) return false;
            if (Card.Location == CardLocation.Hand) return true;
            if (Bot.HasInHand(CardId.BlueEyes) || Bot.HasInGraveyard(CardId.BlueEyes)) { Pick(CardId.BlueEyes); return true; }
            Pick(CardId.BurstStream, CardId.MajestyOfTheWhiteDragons, CardId.Roar, CardId.UltimateCreature);
            return true;
        }

        public override bool OnSelectHand()
        {
            return true;
        }
    }
}
