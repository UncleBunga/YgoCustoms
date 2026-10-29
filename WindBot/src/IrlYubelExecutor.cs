using System.Collections.Generic;
using System.Linq;
using YGOSharp.OCGWrapper.Enums;

namespace WindBot.Game.AI.Decks
{
    // IRL Yubel: both engines.
    //  - Yubel loop: destroy Yubel (Nightmare Throne from the Deck, Nightmare Pain from hand / field, Samsara D Lotus on
    //    their monster effect) so it brings out Terror Incarnate, which leaves for The Ultimate Nightmare. Geistgrinder Golem
    //    lands on their field and becomes Fusion material for Yubel - The Loving Defender Forever through Eternal Favorite.
    //  - Sacred Beasts: Dark Beckoning Beast searches and grants a second Normal Summon for a 0/0 Summoning Beast, which
    //    tributes itself for Uria / Hamon / Raviel; Fallen Paradise draws, Armityle or Trilojig finish.
    [Deck("IRL_Yubel", "AI_IRL_Yubel")]
    public class IrlYubelExecutor : CustomExecutorBase
    {
        public class CardId
        {
            public const int UltimateNightmare = 31764700;
            public const int TerrorIncarnate = 4779091;
            public const int Hamon = 32491822;
            public const int Raviel = 69890967;
            public const int Uria = 6007213;
            public const int Yubel = 78371393;
            public const int SpiritOfYubel = 90829280;
            public const int Geistgrinder = 26913989;
            public const int PhantasmalSummoningBeast = 53134520;
            public const int DarkSummoningBeast = 87917187;
            public const int ChaosCore = 54040484;
            public const int DarkBeckoningBeast = 81034083;
            public const int GruesomeGraveSquirmer = 24215921;
            public const int ChaosSummoningBeast = 27439792;
            public const int SamsaraDLotus = 62318994;
            public const int OneForOne = 2295440;
            public const int DimensionFusionDestruction = 89190953;
            public const int SetRotation = 73468603;
            public const int CeruleanSkyfire = 54828837;
            public const int NightmarePain = 65261141;
            public const int OpeningOfTheSpiritGates = 80312545;
            public const int MatureChronicle = 92650749;
            public const int MoundOfTheBoundCreator = 269012;
            public const int FallenParadise = 13301895;
            public const int NightmareThrone = 93729896;
            public const int HyperBlaze = 16317140;
            public const int AwakeningOfTheSacredBeasts = 53701259;
            public const int EternalFavorite = 87532344;

            public const int Armityle = 43378048;
            public const int LovingDefender = 47172959;
            public const int Trilojig = 95463814;
        }

        static readonly int[] Beasts = { CardId.Raviel, CardId.Hamon, CardId.Uria };
        static readonly int[] Yubels = { CardId.Yubel, CardId.TerrorIncarnate, CardId.UltimateNightmare, CardId.SpiritOfYubel };

        public IrlYubelExecutor(GameAI ai, Duel duel)
            : base(ai, duel)
        {
            // Responses
            AddExecutor(ExecutorType.Activate, CardId.SamsaraDLotus, LotusEffect);
            AddExecutor(ExecutorType.Activate, CardId.ChaosCore, () => { Pick(CardId.Uria, CardId.Hamon, CardId.Raviel); return true; });
            AddExecutor(ExecutorType.Activate, CardId.SpiritOfYubel, SpiritEffect);
            AddExecutor(ExecutorType.Activate, CardId.CeruleanSkyfire, () => Duel.LastChainPlayer == 1 || Card.Location != CardLocation.SpellZone);
            AddExecutor(ExecutorType.Activate, CardId.GruesomeGraveSquirmer, SquirmerEffect);
            AddExecutor(ExecutorType.Activate, CardId.TerrorIncarnate, () => { Pick(CardId.UltimateNightmare); return true; });
            AddExecutor(ExecutorType.Activate, CardId.Yubel, () => { Pick(CardId.TerrorIncarnate); return true; });
            AddStaples();

            // Yubel loop
            AddExecutor(ExecutorType.Activate, CardId.NightmareThrone, ThroneEffect);
            AddExecutor(ExecutorType.Activate, CardId.NightmarePain, PainEffect);
            AddExecutor(ExecutorType.SpSummon, CardId.Geistgrinder, GeistgrinderSummon);
            AddExecutor(ExecutorType.Activate, CardId.Geistgrinder, () => true);
            AddExecutor(ExecutorType.Activate, CardId.EternalFavorite, EternalFavoriteEffect);
            AddExecutor(ExecutorType.Activate, CardId.LovingDefender, () => true);
            AddExecutor(ExecutorType.Activate, CardId.MatureChronicle, MatureChronicleEffect);

            // Sacred Beasts
            AddExecutor(ExecutorType.Activate, CardId.OpeningOfTheSpiritGates, SpiritGatesEffect);
            AddExecutor(ExecutorType.Summon, CardId.DarkBeckoningBeast, () => true);
            AddExecutor(ExecutorType.Activate, CardId.DarkBeckoningBeast, BeckoningSearch);
            AddExecutor(ExecutorType.Summon, CardId.DarkSummoningBeast, () => true);
            AddExecutor(ExecutorType.Summon, CardId.ChaosSummoningBeast, () => Bot.Hand.Any(c => c.IsCode(Beasts)));
            AddExecutor(ExecutorType.Summon, CardId.PhantasmalSummoningBeast, () => true);
            AddExecutor(ExecutorType.Activate, CardId.DarkSummoningBeast, () => SummonBeast(true));
            AddExecutor(ExecutorType.Activate, CardId.ChaosSummoningBeast, () => SummonBeast(false));
            AddExecutor(ExecutorType.Activate, CardId.PhantasmalSummoningBeast, () => SummonBeast(true));
            AddExecutor(ExecutorType.Activate, CardId.OneForOne, OneForOneEffect);
            AddExecutor(ExecutorType.Activate, CardId.HyperBlaze, HyperBlazeEffect);
            AddExecutor(ExecutorType.Activate, CardId.FallenParadise, () => true);
            AddExecutor(ExecutorType.Activate, CardId.Raviel, RavielEffect);
            AddExecutor(ExecutorType.Activate, CardId.Uria, () => TargetEnemySetCard());
            AddExecutor(ExecutorType.SpSummon, CardId.Armityle, ArmityleSummon);
            AddExecutor(ExecutorType.SpSummon, CardId.Trilojig, () => MyTurn && Enemy.GetMonsters().Any(m => m.IsFaceup()));
            AddExecutor(ExecutorType.Activate, CardId.Trilojig, () => TargetEnemyMonster());
            AddExecutor(ExecutorType.Activate, CardId.DimensionFusionDestruction, () => MyTurn && Bot.HasInExtra(CardId.Armityle));

            // Normal Summon Yubel only when nothing better uses the summon
            AddExecutor(ExecutorType.Summon, CardId.Yubel, () => Bot.GetMonsterCount() == 0);
            AddExecutor(ExecutorType.SummonOrSet, () => BestFallbackSummon());
            AddExecutor(ExecutorType.SpellSet, SetTrapOrQuickPlay);
            AddExecutor(ExecutorType.Repos, YubelRepos);
            AddExecutor(ExecutorType.Activate, GenericActivate);
        }

        protected override int[] AddToHandPriority => new[] {
            CardId.DarkSummoningBeast, CardId.Yubel, CardId.NightmareThrone, CardId.Raviel, CardId.Hamon, CardId.OpeningOfTheSpiritGates,
            CardId.FallenParadise, CardId.SamsaraDLotus, CardId.EternalFavorite, CardId.ChaosSummoningBeast, CardId.Uria
        };
        protected override int[] SpSummonPriority => new[] {
            CardId.UltimateNightmare, CardId.TerrorIncarnate, CardId.Raviel, CardId.Hamon, CardId.Uria, CardId.Yubel, CardId.SpiritOfYubel,
            CardId.DarkSummoningBeast, CardId.DarkBeckoningBeast
        };
        protected override int[] ExpendablePriority => new[] {
            CardId.SamsaraDLotus, CardId.GruesomeGraveSquirmer, CardId.ChaosCore, CardId.DarkBeckoningBeast, CardId.ChaosSummoningBeast,
            CardId.DarkSummoningBeast, CardId.PhantasmalSummoningBeast, CardId.SetRotation, CardId.MoundOfTheBoundCreator
        };

        bool destroyYubelFromDeck;

        bool YubelOnField => Bot.GetMonsters().Any(m => m.IsFaceup() && m.IsCode(Yubels));
        bool TerrorReady => MaybeInDeck(CardId.TerrorIncarnate, 1) || Bot.Graveyard.Any(c => c.IsCode(CardId.TerrorIncarnate)) || Bot.HasInHand(CardId.TerrorIncarnate);

        // ---------- Yubel loop ----------

        bool ThroneEffect()
        {
            if (Card.Location == CardLocation.Hand || (Card.Location == CardLocation.SpellZone && Card.IsFacedown()))
            {
                // activation: destroy Yubel in the Deck so it brings Terror Incarnate, else add a 0/0 Fiend
                destroyYubelFromDeck = !YubelOnField && TerrorReady && MaybeInDeck(CardId.Yubel, 2) && !Bot.HasInMonstersZone(CardId.TerrorIncarnate);
                if (destroyYubelFromDeck) Pick(CardId.Yubel);
                else Pick(CardId.DarkSummoningBeast, CardId.SamsaraDLotus, CardId.Yubel, CardId.DarkBeckoningBeast);
                return true;
            }
            // a Yubel left the field by card effect: fetch the next one
            Pick(CardId.UltimateNightmare, CardId.TerrorIncarnate, CardId.Yubel);
            return true;
        }

        bool PainEffect()
        {
            if (Card.Location == CardLocation.Hand) return true;
            if (!MyTurn) return false;
            // destroy Yubel (hand first) so Terror Incarnate arrives; Terror on the field becomes The Ultimate Nightmare
            var t = Bot.Hand.FirstOrDefault(c => c.IsCode(CardId.Yubel))
                ?? Bot.GetMonsters().FirstOrDefault(m => m.IsFaceup() && m.IsCode(CardId.Yubel))
                ?? Bot.GetMonsters().FirstOrDefault(m => m.IsFaceup() && m.IsCode(CardId.TerrorIncarnate) && MaybeInDeck(CardId.UltimateNightmare, 1));
            if (t != null && (t.IsCode(CardId.TerrorIncarnate) || TerrorReady))
            {
                Pick(t);
                Pick(CardId.NightmareThrone, CardId.SamsaraDLotus, CardId.EternalFavorite, CardId.SpiritOfYubel, CardId.Geistgrinder);
                return true;
            }
            // otherwise only a spare DARK monster in hand, for the search
            var spare = Bot.Hand.Where(c => c.IsMonster() && c.HasAttribute(CardAttribute.Dark) && !c.IsCode(Beasts) && !c.IsCode(Yubels))
                .OrderBy(c => Rank(ExpendablePriority, c)).FirstOrDefault();
            if (spare == null || Bot.HasInHand(CardId.Yubel)) return false;
            Pick(spare);
            Pick(CardId.Yubel, CardId.NightmareThrone, CardId.SamsaraDLotus);
            return true;
        }

        bool LotusEffect()
        {
            if (Card.Location == CardLocation.Grave) return true; // End Phase: back to hand / field
            if (Card.Location == CardLocation.Hand) return false;
            if (!MyTurn)
            {
                // their monster effect becomes "destroy 1 Yubel": our Yubel moves up the chain
                return Duel.LastChainPlayer == 1 && YubelOnField && Bot.GetMonsters().Any(m => m.IsCode(CardId.Yubel, CardId.TerrorIncarnate));
            }
            // tribute: Yubel from the Deck
            if (YubelOnField) return false;
            Pick(CardId.Yubel, CardId.SpiritOfYubel);
            return true;
        }

        bool SpiritEffect()
        {
            if (Card.Location == CardLocation.Hand) return true;
            Pick(CardId.NightmareThrone, CardId.NightmarePain, CardId.EternalFavorite, CardId.MatureChronicle);
            Pick(CardId.UltimateNightmare, CardId.TerrorIncarnate, CardId.Yubel);
            return true;
        }

        bool SquirmerEffect()
        {
            if (Card.Location == CardLocation.Grave) { Pick(CardId.Yubel, CardId.DarkSummoningBeast, CardId.ChaosSummoningBeast); return true; }
            // from hand: destroy our Yubel when that advances it (never the Ultimate Nightmare)
            return Bot.GetMonsters().Any(m => m.IsFaceup() && m.IsCode(CardId.Yubel, CardId.TerrorIncarnate));
        }

        bool GeistgrinderSummon()
        {
            // put Golem on their field and bring Yubel: Golem then becomes Loving Defender material
            if (!Bot.HasInHand(CardId.Yubel)) return false;
            if (Enemy.GetMonsterCount() >= 5) return false;
            Pick(CardId.Yubel);
            return true;
        }

        bool EternalFavoriteEffect()
        {
            if (!MyTurn) return false;
            if (Bot.HasInExtra(CardId.LovingDefender) && Bot.GetMonsters().Any(m => m.IsFaceup() && m.IsCode(CardId.Yubel)) && Bot.Hand.Count > 0)
            {
                // Fusion with their monsters as material: the burn counts every material
                Pick(Bot.Hand.OrderBy(c => Rank(ExpendablePriority, c)).ThenBy(CardValue).First());
                Pick(CardId.LovingDefender);
                return true;
            }
            // revive a Yubel
            return Bot.Graveyard.Any(c => c.IsCode(Yubels)) || Bot.Banished.Any(c => c.IsCode(Yubels));
        }

        bool MatureChronicleEffect()
        {
            if (!MyTurn) return false;
            return TargetEnemyCard() || !YubelOnField;
        }

        // ---------- Sacred Beasts ----------

        bool BeckoningSearch()
        {
            if (!Bot.Hand.Any(c => c.IsCode(CardId.DarkSummoningBeast, CardId.ChaosSummoningBeast, CardId.PhantasmalSummoningBeast)))
                Pick(CardId.DarkSummoningBeast, CardId.PhantasmalSummoningBeast, CardId.OpeningOfTheSpiritGates, CardId.Raviel);
            else
                Pick(CardId.Raviel, CardId.Hamon, CardId.OpeningOfTheSpiritGates, CardId.FallenParadise);
            return true;
        }

        bool SummonBeast(bool fromDeck)
        {
            if (!MyTurn) return false;
            if (Card.Location == CardLocation.Grave)
            {
                Pick(CardId.FallenParadise, CardId.DimensionFusionDestruction, CardId.Raviel, CardId.Hamon, CardId.Uria);
                return true;
            }
            // the one we don't control yet: Raviel first (biggest), Hamon, Uria
            var want = Beasts.Where(id => !Bot.HasInMonstersZone(id)).ToArray();
            if (want.Length == 0) return false;
            if (!fromDeck && !Bot.Hand.Any(c => c.IsCode(want))) return false;
            if (Card.IsCode(CardId.PhantasmalSummoningBeast)) Pick(want);
            Pick(want);
            return true;
        }

        bool SpiritGatesEffect()
        {
            if (Card.Location == CardLocation.Hand)
            {
                Pick(CardId.DarkSummoningBeast, CardId.Raviel, CardId.Hamon, CardId.PhantasmalSummoningBeast);
                return true;
            }
            // discard to revive a 0/0 Fiend (a Summoning Beast or Yubel), or recover a Continuous Spell
            var discard = Bot.Hand.Where(c => !c.IsCode(CardId.Yubel)).OrderBy(c => Rank(ExpendablePriority, c)).ThenBy(CardValue).FirstOrDefault();
            if (discard == null) return false;
            if (!Bot.Graveyard.Any(c => c.IsCode(CardId.DarkSummoningBeast, CardId.ChaosSummoningBeast, CardId.Yubel, CardId.PhantasmalSummoningBeast))) return true;
            Pick(discard);
            Pick(CardId.Yubel, CardId.DarkSummoningBeast, CardId.ChaosSummoningBeast, CardId.PhantasmalSummoningBeast);
            return true;
        }

        bool OneForOneEffect()
        {
            var cost = Bot.Hand.Where(c => c.IsMonster() && !c.IsCode(CardId.Yubel)).OrderBy(c => c.IsCode(Beasts) ? 0 : 1).ThenBy(c => Rank(ExpendablePriority, c)).FirstOrDefault();
            if (cost == null) return false;
            Pick(cost);
            Pick(CardId.ChaosSummoningBeast, CardId.SamsaraDLotus, CardId.GruesomeGraveSquirmer);
            return true;
        }

        bool HyperBlazeEffect()
        {
            if (Card.Location == CardLocation.Hand) return true;
            if (!Bot.Graveyard.Any(c => c.IsCode(Beasts))) return Duel.Phase == DuelPhase.BattleStep;
            var discard = Bot.Hand.OrderBy(c => Rank(ExpendablePriority, c)).ThenBy(CardValue).FirstOrDefault();
            if (discard == null) return false;
            Pick(discard);
            Pick(Bot.Graveyard.Where(c => c.IsCode(Beasts)).OrderByDescending(c => c.Attack));
            return true;
        }

        bool RavielEffect()
        {
            // tribute a spare monster for ATK only when it turns a battle
            if (!MyTurn || Duel.Phase != DuelPhase.Main1) return false;
            var spare = Bot.GetMonsters().Where(m => m != Card && !m.IsCode(Beasts) && !m.IsCode(Yubels) && m.Attack > 0).OrderBy(CardValue).FirstOrDefault();
            if (spare == null) return false;
            var best = BestEnemyMonster();
            if (best == null || Card.Attack > best.Attack) return false;
            Pick(spare);
            return true;
        }

        bool TargetEnemySetCard()
        {
            var t = Enemy.GetSpells().FirstOrDefault(c => c.IsFacedown());
            if (t == null) return false;
            Pick(t);
            return true;
        }

        bool ArmityleSummon()
        {
            // banishes all three Lords for a 10000+ ATK attacker on our turn
            return MyTurn && Duel.Phase == DuelPhase.Main1 && Duel.Turn > 1;
        }

        bool YubelRepos()
        {
            // Yubel monsters burn from Attack Position
            if (Card.IsCode(Yubels)) return Card.IsDefense();
            return DefaultMonsterRepos();
        }

        public override int OnSelectOption(IList<long> options)
        {
            // Nightmare Throne: destroy Yubel in the Deck instead of adding it
            if (destroyYubelFromDeck)
            {
                int i = options.IndexOf(Util.GetStringId(CardId.NightmareThrone, 3));
                if (i >= 0) { destroyYubelFromDeck = false; return i; }
            }
            return base.OnSelectOption(options);
        }

        public override bool OnSelectHand()
        {
            return true;
        }
    }
}
