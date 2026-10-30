using YGOSharp.OCGWrapper.Enums;

namespace WindBot.Game.AI.Decks
{
    // IRL Exodia: wins with The Unstoppable Exodia Incarnate, not the hand win.
    //  - Wedju Temple / the Millennium monsters' own effects place them in the Spell & Trap Zone; from there they Special
    //    Summon themselves (revealing Millennium Ankh, or paying 2000 LP) and search: Shield > Ankh, Golem > Wedju Temple,
    //    Sengenjin > a Millennium monster.
    //  - Millennium Ankh shows the 5 pieces (hand / Deck / field, so pieces must stay out of the GY) and summons
    //    The Unstoppable Exodia Incarnate. It locks Summons for the turn, so it comes after the Millennium summons.
    //  - Exxod Fires of Rage then clears their field; Exodia negates a Spell/Trap per turn and adds our LP to its ATK.
    [Deck("IRL_Exodia", "AI_IRL_Exodia")]
    public class IrlExodiaExecutor : CustomExecutorBase
    {
        public class CardId
        {
            public const int LeftArm = 7902349;
            public const int RightLeg = 8124921;
            public const int LeftLeg = 44519536;
            public const int RightArm = 70903634;
            public const int ExodiaTheForbiddenOne = 33396948;
            public const int LegendaryIncarnate = 58604027;
            public const int HeartOfTheBlueEyes = 54475145;
            public const int Sengenjin = 38775407;
            public const int SoulOfGaia = 73129314;
            public const int Golem = 74169516;
            public const int Shield = 1164211;
            public const int ExodiaNecross = 12600382;
            public const int Maiden = 37552929;
            public const int FiendReflection = 63947968;
            public const int ContractWithExodia = 33244944;
            public const int MillenniumAnkh = 37613663;
            public const int ObliterateBlaze = 402416;
            public const int WedjuTemple = 63017368;
            public const int ExxodFiresOfRage = 23617756;
            public const int Obliterate = 64043465;
            public const int UnstoppableExodia = 83257450;
        }

        static readonly int[] Pieces = { CardId.ExodiaTheForbiddenOne, CardId.LeftArm, CardId.RightArm, CardId.LeftLeg, CardId.RightLeg };
        // Millennium monsters that Special Summon themselves from the Spell & Trap Zone and search
        static readonly int[] Searchers = { CardId.Shield, CardId.Golem, CardId.Sengenjin };
        static readonly int[] Millennium = { CardId.Shield, CardId.Golem, CardId.Sengenjin, CardId.Maiden, CardId.FiendReflection, CardId.SoulOfGaia, CardId.HeartOfTheBlueEyes };

        public IrlExodiaExecutor(GameAI ai, Duel duel)
            : base(ai, duel)
        {
            // Exodia on the field
            AddExecutor(ExecutorType.Activate, CardId.UnstoppableExodia, UnstoppableEffect);
            AddExecutor(ExecutorType.Activate, CardId.SoulOfGaia, SoulOfGaiaEffect);
            AddExecutor(ExecutorType.Activate, CardId.Maiden, () => Card.Location != CardLocation.Hand || PlaceFromHand());
            AddExecutor(ExecutorType.Activate, CardId.FiendReflection, () => Card.Location != CardLocation.Hand || PlaceFromHand());
            AddExecutor(ExecutorType.Activate, CardId.ExxodFiresOfRage, ExxodEffect);
            AddStaples();

            // Searchers and placement
            AddExecutor(ExecutorType.Activate, CardId.HeartOfTheBlueEyes, () => Card.Location == CardLocation.Grave || !Bot.HasInHand(CardId.MillenniumAnkh));
            AddExecutor(ExecutorType.Activate, CardId.WedjuTemple, WedjuEffect);
            AddExecutor(ExecutorType.Activate, CardId.Shield, MillenniumSearcherEffect);
            AddExecutor(ExecutorType.Activate, CardId.Golem, MillenniumSearcherEffect);
            AddExecutor(ExecutorType.Activate, CardId.Sengenjin, MillenniumSearcherEffect);

            // Normal Summons before the Ankh (it stops Summons for the rest of the turn)
            AddExecutor(ExecutorType.Summon, CardId.SoulOfGaia, SoulOfGaiaSummon);
            AddExecutor(ExecutorType.Activate, CardId.MillenniumAnkh, AnkhEffect);
            AddExecutor(ExecutorType.Activate, CardId.Obliterate, ObliterateEffect);
            AddExecutor(ExecutorType.Activate, CardId.ObliterateBlaze, BlazeEffect);
            AddExecutor(ExecutorType.SpSummon, CardId.LegendaryIncarnate, () => false); // it would tribute a piece into the GY
            AddExecutor(ExecutorType.Activate, CardId.ContractWithExodia, () => true);

            AddExecutor(ExecutorType.SummonOrSet, CardId.Maiden, () => !Bot.HasInHand(CardId.WedjuTemple) && Bot.GetMonsterCount() == 0);
            AddExecutor(ExecutorType.SummonOrSet, CardId.FiendReflection, () => !Bot.HasInHand(CardId.WedjuTemple) && Bot.GetMonsterCount() == 0);
            AddExecutor(ExecutorType.SpellSet, SetTrapOrQuickPlay);
            AddExecutor(ExecutorType.Repos, CardId.UnstoppableExodia, () => Card.IsDefense());
            // WindBot skips the Battle Phase when only 0 ATK monsters could attack; Exodia's ATK comes at damage calculation
            AddExecutor(ExecutorType.GoToBattlePhase, () => Bot.GetMonsters().Any(m => m.IsCode(CardId.UnstoppableExodia) && m.IsAttack() && !m.Attacked));
            AddExecutor(ExecutorType.Repos, DefaultMonsterRepos);
            AddExecutor(ExecutorType.Activate, GenericActivate);
        }

        protected override int[] AddToHandPriority => new[] {
            CardId.MillenniumAnkh, CardId.WedjuTemple, CardId.Shield, CardId.Golem, CardId.Sengenjin, CardId.ExxodFiresOfRage
        };
        protected override int[] SpSummonPriority => new[] { CardId.UnstoppableExodia, CardId.Shield, CardId.Golem, CardId.Sengenjin };
        protected override int[] SetPriority => new[] { CardId.ExxodFiresOfRage, CardId.Obliterate, CardId.ObliterateBlaze };
        protected override int[] ExpendablePriority => new[] {
            CardId.ContractWithExodia, CardId.ExodiaNecross, CardId.LegendaryIncarnate, CardId.FiendReflection, CardId.Maiden,
            CardId.HeartOfTheBlueEyes
        };

        bool HasExodia => Bot.GetMonsters().Any(m => m.IsFaceup() && m.IsCode(CardId.UnstoppableExodia, CardId.LegendaryIncarnate));
        int PiecesOutsideGY => Pieces.Count(id => !Bot.Graveyard.Any(c => c.IsCode(id)) && !Bot.Banished.Any(c => c.IsCode(id)));

        // ---------- Millennium engine ----------

        bool PlaceFromHand()
        {
            // the hand effect of a Millennium monster: place it as a Continuous Spell (main phase only)
            return MyTurn && Bot.GetSpellCountWithoutField() < 4;
        }

        bool WedjuEffect()
        {
            if (Card.Location == CardLocation.Hand) return true; // activate the card
            if (!MyTurn || Bot.GetSpellCountWithoutField() >= 4) return false;
            // hand: a searcher, else any Millennium body; Deck: Shield if we still need the Ankh, else Golem / Sengenjin
            var fromHand = Bot.Hand.Where(c => c.IsMonster() && !c.IsCode(Pieces)).OrderBy(c => c.IsCode(Searchers) ? 0 : c.IsCode(Millennium) ? 1 : 2)
                .ThenBy(c => c.IsCode(CardId.Shield) && Bot.HasInHand(CardId.MillenniumAnkh) ? 1 : 0).FirstOrDefault();
            if (fromHand == null) return false;
            Pick(fromHand);
            if (!Bot.HasInHand(CardId.MillenniumAnkh)) Pick(CardId.Shield, CardId.Golem, CardId.Sengenjin);
            else Pick(CardId.Sengenjin, CardId.Golem, CardId.Shield);
            return true;
        }

        bool MillenniumSearcherEffect()
        {
            if (Card.Location == CardLocation.Hand) return PlaceFromHand() && !Bot.HasInHand(CardId.WedjuTemple);
            if (Card.Location != CardLocation.SpellZone) return true;
            // Special Summon from the Spell & Trap Zone: reveal the Ankh, or pay 2000 LP while we can afford it
            if (!Bot.HasInHand(CardId.MillenniumAnkh) && Bot.LifePoints <= 4000) return false;
            if (Card.IsCode(CardId.Shield)) Pick(CardId.MillenniumAnkh);
            else if (Card.IsCode(CardId.Golem)) Pick(CardId.WedjuTemple);
            else Pick(CardId.Shield, CardId.Golem, CardId.Sengenjin, CardId.SoulOfGaia);
            return true;
        }

        public override int OnSelectOption(System.Collections.Generic.IList<long> options)
        {
            // Millennium summons: reveal Millennium Ankh rather than paying 2000 LP
            if (Bot.HasInHand(CardId.MillenniumAnkh))
            {
                for (int i = 0; i < options.Count; i++)
                    foreach (var id in Searchers)
                        if (options[i] == Util.GetStringId(id, 3)) return i;
            }
            return base.OnSelectOption(options);
        }

        bool AnkhEffect()
        {
            if (!MyTurn || HasExodia || !Bot.HasInExtra(CardId.UnstoppableExodia)) return false;
            if (PiecesOutsideGY < 5) return false;
            // the Millennium summons come first through executor order; the Ankh goes as soon as nothing else is offered
            return true;
        }

        // ---------- Exodia ----------

        bool UnstoppableEffect()
        {
            if (Card.Location != CardLocation.MonsterZone) return true;
            if (Duel.Phase == DuelPhase.End) { Pick(CardId.ExxodFiresOfRage, CardId.Obliterate); return true; }
            bool battle = Duel.Phase == DuelPhase.BattleStart || Duel.Phase == DuelPhase.BattleStep || Duel.Phase == DuelPhase.Damage
                || Duel.Phase == DuelPhase.DamageCal || Duel.Phase == DuelPhase.Battle;
            if (Desc(CardId.UnstoppableExodia, 0) || (battle && (GenericTrigger || Duel.CurrentChain.Count == 0))) return true; // gain ATK equal to our LP
            // negate a Spell/Trap of theirs
            return Duel.LastChainPlayer == 1;
        }

        bool ExxodEffect()
        {
            if (Card.Location == CardLocation.Grave)
            {
                // shuffle pieces back for the Ankh, or add one to hand
                if (PiecesOutsideGY < 5) return true;
                Pick(Pieces);
                return true;
            }
            return HasExodia && Enemy.GetFieldCount() > 0;
        }

        bool ObliterateEffect()
        {
            if (Card.Location == CardLocation.Grave) { Pick(CardId.ContractWithExodia, CardId.ExodiaNecross); Pick(Pieces); return true; }
            var t = BestEnemyMonster();
            if (t == null) return false;
            if (t.Attack < 2000 && !t.IsMonsterDangerous() && HasExodia) return false;
            // keep the pieces out of the GY (the Ankh shows them from hand / Deck / field)
            Pick(t);
            Pick(CardId.ContractWithExodia, CardId.ExodiaNecross, CardId.LegendaryIncarnate);
            return true;
        }

        bool BlazeEffect()
        {
            if (Card.Location != CardLocation.MonsterZone) return true; // the Spell itself grants the effects
            // pay half our LP: only when the equips (2000 each) win this turn
            if (!MyTurn || Duel.Phase != DuelPhase.Main1 || Bot.LifePoints < 2000) return false;
            int atk = Card.Attack + 10000 + Bot.LifePoints / 2;
            return Enemy.GetMonsterCount() == 0 || atk >= Enemy.LifePoints + Enemy.GetMonsters().Max(m => m.IsAttack() ? m.Attack : 0);
        }

        bool SoulOfGaiaSummon()
        {
            // without Tributing while a Level 5+ Monster Card is on the field (Millennium monsters in the Spell & Trap Zone
            // count): tribute nothing
            bool free = Bot.GetMonsters().Concat(Bot.GetSpells()).Concat(Enemy.GetMonsters()).Any(c => c.IsFaceup() && c.Level >= 5 && c.HasType(CardType.Monster));
            // the engine still offers a Tribute when we control a monster, and WindBot always takes it
            return free && Bot.GetMonsterCount() == 0;
        }

        bool SoulOfGaiaEffect()
        {
            if (Duel.LastChainPlayer != 1) return false;
            // send a Level 5+ Monster Card we can spare: never Exodia
            var cost = Bot.GetMonsters().Concat(Bot.GetSpells()).Where(c => c.IsFaceup() && c != Card && c.Level >= 5 && !c.IsCode(CardId.UnstoppableExodia))
                .OrderBy(c => c.IsCode(CardId.Shield) ? 1 : 0).ThenBy(CardValue).FirstOrDefault();
            if (cost == null) return false;
            Pick(cost);
            return true;
        }

        // The Unstoppable Exodia Incarnate gains ATK equal to our LP at damage calculation (direct attacks too)
        public override CardPosition OnSelectPosition(int cardId, System.Collections.Generic.IList<CardPosition> positions)
        {
            if (cardId == CardId.UnstoppableExodia && positions.Contains(CardPosition.FaceUpAttack)) return CardPosition.FaceUpAttack;
            return base.OnSelectPosition(cardId, positions);
        }

        public override BattlePhaseAction OnBattle(System.Collections.Generic.IList<ClientCard> attackers, System.Collections.Generic.IList<ClientCard> defenders)
        {
            // the default battle logic skips 0 ATK attackers for direct attacks
            var exodia = attackers.FirstOrDefault(c => c.IsCode(CardId.UnstoppableExodia));
            if (exodia != null && defenders.Count == 0) return AI.Attack(exodia, null);
            return base.OnBattle(attackers, defenders);
        }

        public override bool OnPreBattleBetween(ClientCard attacker, ClientCard defender)
        {
            bool ok = base.OnPreBattleBetween(attacker, defender); // resets RealPower
            if (attacker.IsCode(CardId.UnstoppableExodia)) { attacker.RealPower = attacker.Attack + Bot.LifePoints; return true; }
            return ok;
        }

        public override bool OnSelectHand()
        {
            return true;
        }
    }
}
