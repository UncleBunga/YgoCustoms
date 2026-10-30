using System.Collections.Generic;
using YGOSharp.OCGWrapper.Enums;

namespace WindBot.Game.AI.Decks
{
    // IRL Chaos (the user's pet deck). Lines taken from the user's own replays:
    //  - Chaos Space discards a LIGHT/DARK (Eclipse Wyvern, Chaos Witch, Black Luster Soldier) to fetch the boss of the
    //    other Attribute; Wyverburster <-> Collapserpent search each other.
    //  - Chaospawn Bishop pulls a second Bishop > Glitch Clutch Nullgainer > Witness of the Ancient > place Angelechy
    //    Destrier / Bastion / Shatranga (searching Angelechy Problem / Disturbance) > Seneschal > Enlisted > Chaos Angel.
    //  - Core of Chaos <-> Shell of Chaos > Chaos Beast; Chaos Ruler excavates; The Chaos Creator / Bystial Magnamhut extend.
    //  - Yata-Garasu lock: clear their hand and field (pre-errata Chaos Emperor Dragon, Raigeki, Dark Hole, hand
    //    disruption), then keep Yata attacking so they skip their draws.
    // Synchro materials are left to the engine (it only offers legal summons); the executor decides which to make.
    [Deck("IRL_Chaos", "AI_IRL_Chaos")]
    public class IrlChaosExecutor : CustomExecutorBase
    {
        public class CardId
        {
            public const int ThunderKing = 48770333;
            public const int BLSBeginning = 72989439;
            public const int CED = 511000819;
            public const int Levianeer = 55878038;
            public const int DMoC = 511001039;
            public const int ChaosCreator = 90488465;
            public const int Witness = 54577949;
            public const int Magnamhut = 33854624;
            public const int Dragonroar = 29596581;
            public const int Dragonhawk = 83107873;
            public const int Dragondark = 56713174;
            public const int Collapserpent = 61901281;
            public const int Valkyria = 53871273;
            public const int Kycoo = 88240808;
            public const int Wyverburster = 99234526;
            public const int Grepher = 18859369;
            public const int Raiden = 77558536;
            public const int EclipseWyvern = 51858306;
            public const int MirageDragon = 66699781;
            public const int ShellOfChaos = 77312273;
            public const int ChaosWitch = 30327674;
            public const int WitchOfBlackForest = 511003012;
            public const int Bishop = 101402089;
            public const int Defrag = 58582979;
            public const int Sangan = 511002631;
            public const int CyberJar = 34124316;
            public const int SpiritReaper = 23205979;
            public const int FabledRaven = 47217354;
            public const int Yata = 3078576;
            public const int CoreOfChaos = 3806388;
            public const int ChaosSpace = 99266988;
            public const int AngelechyOpposition = 101402094;
            public const int SuperPolymerization = 48130397;
            public const int AngelechyProblem = 17782288;
            public const int AngelechyDisturbance = 54171327;

            public const int StarvingVenom = 41209827;
            public const int ThunderDragonColossus = 15291624;
            public const int DisPater = 27572350;
            public const int ChaosAngel = 22850702;
            public const int KewlTune = 65961304;
            public const int Shatranga = 42410161;
            public const int Seneschal = 101402091;
            public const int ChaosRuler = 3040496;
            public const int Bastion = 28904860;
            public const int ChaosArchfiend = 13076804;
            public const int Nullgainer = 32044675;
            public const int Destrier = 55393975;
            public const int Castellan = 101402090;
            public const int ChaosBeast = 49565413;
            public const int Enlisted = 81797573;
            public const int ZombieVampire = 73082255;
            public const int UnderworldGoddess = 98127546;
            public const int BLSSoldierOfChaos = 49202162;
            public const int VerteAnaconda = 70369116;
            public const int StrikerDragon = 73539069;
            public const int Artemis = 34755994;
            public const int Almiraj = 60303245;
        }

        static readonly int[] ChaosBosses = { CardId.ChaosCreator, CardId.BLSBeginning, CardId.CED, CardId.Levianeer, CardId.Wyverburster, CardId.Collapserpent };
        static readonly int[] AngelechySynchros = { CardId.Destrier, CardId.Bastion, CardId.Shatranga, CardId.Castellan };

        public IrlChaosExecutor(GameAI ai, Duel duel)
            : base(ai, duel)
        {
            // Responses
            AddExecutor(ExecutorType.Activate, CardId.DisPater, DisPaterEffect);
            AddExecutor(ExecutorType.Activate, CardId.Magnamhut, MagnamhutEffect);
            AddExecutor(ExecutorType.Activate, CardId.KewlTune, () => Duel.LastChainPlayer == 1);
            // Thunder Dragons (as the official Thunder Dragon bot plays them): banished or sent to the GY they fetch the next
            // piece; Dragondark discards itself for another Dragondark on their turn
            AddExecutor(ExecutorType.Activate, CardId.Dragondark, DragondarkEffect);
            AddExecutor(ExecutorType.Activate, CardId.Dragonroar, () => Card.Location == CardLocation.Hand
                ? PickThen(CardId.Dragonhawk, CardId.Dragondark) : PickThen(CardId.Dragondark, CardId.Dragonhawk));
            AddExecutor(ExecutorType.Activate, CardId.Dragonhawk, () => Card.Location == CardLocation.Hand
                ? PickThen(CardId.Dragonroar, CardId.Dragondark) : true);
            AddExecutor(ExecutorType.Activate, CardId.AngelechyDisturbance, DisturbanceEffect);

            // Openers and hand disruption (the Yata plan wants their hand empty)
            AddStaples();
            AddExecutor(ExecutorType.Activate, CardId.ChaosSpace, ChaosSpaceEffect);
            AddExecutor(ExecutorType.Activate, CardId.CED, CEDEffect);
            AddExecutor(ExecutorType.SpSummon, CardId.CED, CEDSummon);

            // Bishop > Nullgainer > Witness > Angelechy
            AddExecutor(ExecutorType.Summon, CardId.Bishop, () => (Bot.GetMonsterCount() < 4) && NormalSummon());
            AddExecutor(ExecutorType.Activate, CardId.Bishop, () => PickThen(CardId.Bishop));
            AddExecutor(ExecutorType.SpSummon, CardId.Nullgainer, NullgainerSummon);
            AddExecutor(ExecutorType.Activate, CardId.Nullgainer, () => PickThen(CardId.Witness, CardId.Witness));
            AddExecutor(ExecutorType.SpSummon, CardId.Witness);
            AddExecutor(ExecutorType.Activate, CardId.Witness, WitnessEffect);
            AddExecutor(ExecutorType.Activate, CardId.Destrier, AngelechyEffect);
            AddExecutor(ExecutorType.Activate, CardId.Bastion, AngelechyEffect);
            AddExecutor(ExecutorType.Activate, CardId.Shatranga, AngelechyEffect);
            AddExecutor(ExecutorType.Activate, CardId.Castellan, AngelechyEffect);
            AddExecutor(ExecutorType.Activate, CardId.AngelechyProblem, ProblemEffect);
            AddExecutor(ExecutorType.Activate, CardId.AngelechyOpposition, () => MyTurn);
            AddExecutor(ExecutorType.Activate, CardId.Seneschal, () => PickThen(CardId.Enlisted));
            AddExecutor(ExecutorType.Activate, CardId.Enlisted, () => Card.Location != CardLocation.MonsterZone || TargetEnemyMonster());

            // Core / Shell, the dragon loop, Raiden and searchers
            AddExecutor(ExecutorType.Activate, CardId.CoreOfChaos, () => PickThen(CardId.ShellOfChaos));
            AddExecutor(ExecutorType.Activate, CardId.ShellOfChaos, () => PickThen(CardId.CoreOfChaos));
            AddExecutor(ExecutorType.SpSummon, CardId.Wyverburster, () => BanishCost(CardAttribute.Dark));
            AddExecutor(ExecutorType.SpSummon, CardId.Collapserpent, () => BanishCost(CardAttribute.Light));
            AddExecutor(ExecutorType.Activate, CardId.Wyverburster);
            AddExecutor(ExecutorType.Activate, CardId.Collapserpent);
            AddExecutor(ExecutorType.SpSummon, CardId.ChaosCreator, () => BanishLightAndDark());
            AddExecutor(ExecutorType.Activate, CardId.ChaosCreator, () => PickThen(CardId.Bishop, CardId.CoreOfChaos, CardId.ShellOfChaos, CardId.Wyverburster));
            AddExecutor(ExecutorType.SpSummon, CardId.BLSBeginning, () => BanishLightAndDark());
            AddExecutor(ExecutorType.Activate, CardId.BLSBeginning, BLSEffect);
            AddExecutor(ExecutorType.SpSummon, CardId.Levianeer, () => Bot.Graveyard.Count(IsLightOrDark) >= 4);
            AddExecutor(ExecutorType.Activate, CardId.Levianeer);
            AddExecutor(ExecutorType.Activate, CardId.DMoC, () => PickThen(Staple.Raigeki, Staple.DarkHole, Staple.MonsterReborn, Staple.PotOfGreed, CardId.ChaosSpace));
            AddExecutor(ExecutorType.Activate, CardId.Raiden);
            AddExecutor(ExecutorType.Activate, CardId.EclipseWyvern, () => PickThen(CardId.ChaosCreator, CardId.Levianeer, CardId.BLSBeginning, CardId.CED));
            AddExecutor(ExecutorType.Activate, CardId.WitchOfBlackForest, () => PickThen(CardId.ShellOfChaos, CardId.CoreOfChaos, CardId.Bishop, CardId.Yata, CardId.SpiritReaper));
            AddExecutor(ExecutorType.Activate, CardId.Sangan, () => PickThen(CardId.Bishop, CardId.Raiden, CardId.Yata, CardId.CoreOfChaos, CardId.ShellOfChaos));
            AddExecutor(ExecutorType.Activate, CardId.ChaosWitch);
            AddExecutor(ExecutorType.Activate, CardId.ChaosBeast, () => Card.Location == CardLocation.Grave || PickThen(ChaosBosses));
            AddExecutor(ExecutorType.Activate, CardId.ChaosRuler, () => PickThen(ChaosBosses));
            AddExecutor(ExecutorType.Activate, CardId.ChaosAngel, () => TargetEnemyCard() || Card.Location != CardLocation.MonsterZone);
            AddExecutor(ExecutorType.Activate, CardId.MirageDragon, () => Card.Location == CardLocation.Grave ? TargetEnemyCard() : PickThen(ChaosBosses));
            // the 2000 LP Fusion copy only with LP to spare; the Attribute change only on our turn
            AddExecutor(ExecutorType.Activate, CardId.VerteAnaconda, () => MyTurn && (!Desc(CardId.VerteAnaconda, 1) || Bot.LifePoints > 4000));
            AddExecutor(ExecutorType.Activate, CardId.ThunderKing, () => Duel.Phase == DuelPhase.Main1 && MyTurn);
            AddExecutor(ExecutorType.SpSummon, CardId.ThunderKing, () => Util.GetProblematicEnemyMonster(2500) != null && TargetEnemyMonster());

            // Synchro priority (the engine only offers summons it can complete)
            AddExecutor(ExecutorType.SpSummon, CardId.Seneschal, () => SynchroWorth(10));
            AddExecutor(ExecutorType.SpSummon, CardId.ChaosAngel, () => SynchroWorth(10));
            AddExecutor(ExecutorType.SpSummon, CardId.ChaosRuler, () => SynchroWorth(8));
            AddExecutor(ExecutorType.SpSummon, CardId.ChaosBeast, () => SynchroWorth(6));
            AddExecutor(ExecutorType.SpSummon, CardId.DisPater, () => SynchroWorth(10));
            AddExecutor(ExecutorType.SpSummon, CardId.ChaosArchfiend, () => SynchroWorth(8));

            // Yata lock and normal summons
            AddExecutor(ExecutorType.Summon, CardId.Yata, YataSummon);
            AddExecutor(ExecutorType.Summon, CardId.Raiden, NormalSummon);
            AddExecutor(ExecutorType.Summon, CardId.WitchOfBlackForest, NormalSummon);
            AddExecutor(ExecutorType.Summon, CardId.SpiritReaper, () => Enemy.GetHandCount() > 0 && NormalSummon());
            AddExecutor(ExecutorType.Summon, CardId.MirageDragon, () => Bot.Banished.Any(IsLightOrDark) && NormalSummon());
            AddExecutor(ExecutorType.SummonOrSet, () => BestFallbackSummon() && NormalSummon());
            AddExecutor(ExecutorType.MonsterSet, CardId.CyberJar, () => Util.IsOneEnemyBetter() && NormalSummon());

            AddExecutor(ExecutorType.SpellSet, SetTrapOrQuickPlay);
            AddExecutor(ExecutorType.Repos, DefaultMonsterRepos);
            AddExecutor(ExecutorType.Activate, GenericActivate);
        }

        protected override int[] AddToHandPriority => new[] {
            CardId.Bishop, CardId.Witness, CardId.Wyverburster, CardId.Collapserpent, CardId.ChaosCreator, CardId.CoreOfChaos,
            CardId.ShellOfChaos, CardId.AngelechyProblem, CardId.AngelechyDisturbance, CardId.Yata, CardId.Raiden, CardId.ChaosSpace
        };
        protected override int[] SpSummonPriority => new[] {
            CardId.ChaosAngel, CardId.Seneschal, CardId.ChaosCreator, CardId.BLSBeginning, CardId.Bishop, CardId.Wyverburster,
            CardId.Collapserpent, CardId.CoreOfChaos, CardId.ShellOfChaos
        };
        protected override int[] ExpendablePriority => new[] {
            CardId.EclipseWyvern, CardId.ChaosWitch, CardId.Sangan, CardId.WitchOfBlackForest, CardId.Dragondark, CardId.Dragonroar,
            CardId.Dragonhawk, CardId.Kycoo, CardId.FabledRaven, CardId.Defrag
        };

        bool summonedThisTurn;
        public override void OnNewTurn()
        {
            summonedThisTurn = false;
            base.OnNewTurn();
        }

        bool NormalSummon()
        {
            summonedThisTurn = true;
            return true;
        }

        bool PickThen(params int[] ids)
        {
            Pick(ids);
            return true;
        }

        bool IsLightOrDark(ClientCard c) => c != null && c.IsMonster() && (c.HasAttribute(CardAttribute.Light) || c.HasAttribute(CardAttribute.Dark));

        // ---------- Chaos Space and bosses ----------

        bool ChaosSpaceEffect()
        {
            if (Card.Location == CardLocation.Grave) return Bot.Banished.Any(c => c.IsCode(ChaosBosses));
            var discard = Bot.Hand.Where(c => IsLightOrDark(c)).OrderBy(c => Rank(ExpendablePriority, c))
                .ThenBy(c => c.IsCode(CardId.BLSBeginning) ? 0 : 1).ThenBy(CardValue).FirstOrDefault();
            if (discard == null) return false;
            Pick(discard);
            // the boss of the other Attribute; Creator if both Attributes will be in the GY
            bool lightInGY = Bot.Graveyard.Any(c => c.IsMonster() && c.HasAttribute(CardAttribute.Light)) || discard.HasAttribute(CardAttribute.Light);
            bool darkInGY = Bot.Graveyard.Any(c => c.IsMonster() && c.HasAttribute(CardAttribute.Dark)) || discard.HasAttribute(CardAttribute.Dark);
            if (discard.HasAttribute(CardAttribute.Light))
                Pick(darkInGY && lightInGY ? new[] { CardId.ChaosCreator, CardId.Collapserpent } : new[] { CardId.Collapserpent, CardId.ChaosCreator });
            else
                Pick(darkInGY && lightInGY ? new[] { CardId.BLSBeginning, CardId.Wyverburster } : new[] { CardId.Wyverburster, CardId.BLSBeginning });
            return true;
        }

        bool DragondarkEffect()
        {
            // banished / sent to the GY: add another Thunder Dragon
            if (Card.Location != CardLocation.Hand) { Pick(CardId.Dragonhawk, CardId.Dragonroar); return true; }
            // hand (Quick): discard for another Dragondark, which also puts a DARK in the GY for Chaos costs
            if (!MaybeInDeck(CardId.Dragondark, 3)) return false;
            return !MyTurn;
        }

        bool BanishCost(CardAttribute attr)
        {
            var t = Bot.Graveyard.Where(c => c.IsMonster() && c.HasAttribute(attr)).OrderBy(c => c.IsCode(CardId.ChaosWitch, CardId.EclipseWyvern) ? 0 : 1)
                .ThenBy(c => Rank(ExpendablePriority, c)).FirstOrDefault();
            if (t == null) return false;
            Pick(t);
            return true;
        }

        bool BanishLightAndDark()
        {
            var light = Bot.Graveyard.Where(c => c.IsMonster() && c.HasAttribute(CardAttribute.Light)).OrderBy(c => Rank(ExpendablePriority, c)).FirstOrDefault();
            var dark = Bot.Graveyard.Where(c => c.IsMonster() && c.HasAttribute(CardAttribute.Dark)).OrderBy(c => Rank(ExpendablePriority, c)).FirstOrDefault();
            if (light == null || dark == null) return false;
            Pick(new[] { light, dark });
            return true;
        }

        bool BLSEffect()
        {
            // banish their best monster unless attacking twice wins
            var t = BestEnemyMonster();
            if (t == null) return false;
            if (Duel.Phase != DuelPhase.Main1) return false;
            Pick(t);
            return true;
        }

        bool CEDSummon() => BanishLightAndDark();

        bool CEDEffect()
        {
            // pre-errata: pay 1000, send all hands and fields to the GY, 300 per card. Worth it when they lose more,
            // and it sets up the Yata lock.
            if (Bot.LifePoints <= 1000) return false;
            int theirs = Enemy.GetFieldCount() + Enemy.GetHandCount();
            int ours = Bot.GetFieldCount() + Bot.GetHandCount() - 1;
            if (theirs * 300 >= Enemy.LifePoints) return true;
            return theirs >= ours + 2 || (theirs >= 3 && Bot.HasInHand(CardId.Yata) == false && ours <= 2);
        }

        bool MagnamhutEffect()
        {
            if (Card.Location == CardLocation.Hand)
            {
                // banish a LIGHT/DARK from either GY: theirs first
                var t = Enemy.Graveyard.Where(IsLightOrDark).OrderByDescending(c => c.Attack).FirstOrDefault()
                        ?? Bot.Graveyard.Where(IsLightOrDark).OrderBy(c => Rank(ExpendablePriority, c)).FirstOrDefault();
                if (t == null) return false;
                Pick(t);
                return true;
            }
            return true;
        }

        bool DisPaterEffect()
        {
            if (Duel.CurrentChain.Count > 0) return Duel.LastChainPlayer == 1;
            Pick(Bot.Banished.Where(c => c.IsCode(ChaosBosses)).OrderByDescending(c => c.Attack));
            return Bot.Banished.Any(c => c.IsCode(ChaosBosses));
        }

        // ---------- Bishop / Witness / Angelechy ----------

        bool NullgainerSummon()
        {
            // the Bishop line: Nullgainer searches Witness of the Ancient
            if (!MaybeInDeck(CardId.Witness, 2) || Bot.HasInHand(CardId.Witness)) return SynchroWorth(8);
            return true;
        }

        bool WitnessEffect()
        {
            if (Card.Location == CardLocation.Hand) return true;
            // place exactly 2 (Destrier: Angelechy Spell, Bastion: places Shatranga as the third) so the Arc Token is Level 2
            // and Witness (Level 8 Tuner) + the token make Angelechy Seneschal, which brings Enlisted for Chaos Angel
            // (the placement is chosen one card at a time, then finished)
            Pick(CardId.Destrier);
            Pick(CardId.Bastion);
            Pick(new int[0]);
            return true;
        }

        bool AngelechyEffect()
        {
            if (Card.Location == CardLocation.SpellZone)
            {
                if (Card.IsCode(CardId.Destrier)) Pick(CardId.AngelechyProblem, CardId.AngelechyOpposition);
                else if (Card.IsCode(CardId.Shatranga)) Pick(CardId.AngelechyDisturbance);
                else if (Card.IsCode(CardId.Bastion)) Pick(CardId.Shatranga);
                return true;
            }
            // as monsters: banish
            return TargetEnemyCard();
        }

        bool ProblemEffect()
        {
            if (Card.Location != CardLocation.SpellZone || !Card.IsFaceup()) return true;
            var discard = Bot.Hand.Where(c => !c.IsMonster()).OrderBy(c => c.IsCode(CardId.AngelechyDisturbance) ? 0 : 1).ThenBy(CardValue).FirstOrDefault();
            if (discard == null) return false;
            Pick(discard);
            Pick(CardId.Enlisted);
            return true;
        }

        bool DisturbanceEffect()
        {
            if (Card.Location == CardLocation.Grave) return true;
            return !MyTurn && Duel.LastChainPlayer == 1;
        }

        bool SynchroWorth(int level)
        {
            // don't burn a big boss as material for a smaller Synchro
            var bosses = Bot.GetMonsters().Count(m => m.IsCode(CardId.ChaosAngel, CardId.Seneschal, CardId.DisPater) || m.Attack >= 3000);
            if (level < 10 && bosses > 0 && Bot.GetMonsterCount() <= 2) return false;
            return MyTurn;
        }

        // ---------- Yata lock ----------

        bool YataSummon()
        {
            // keep the lock: Yata only when it can hit them (never on the first turn, it would just return to the hand)
            if (Enemy.GetMonsterCount() > 0 || Duel.Turn == 1) return false;
            return NormalSummon();
        }

        public override bool OnSelectHand()
        {
            return true;
        }
    }
}
