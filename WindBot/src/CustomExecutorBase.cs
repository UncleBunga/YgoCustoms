using System.Collections.Generic;
using System.Linq;
using YGOSharp.OCGWrapper.Enums;

namespace WindBot.Game.AI.Decks
{
    // Shared behaviour for the custom deck executors:
    //  - an explicit pick queue (Pick) consumed before the hint-based ordering in OnSelectCard
    //  - hint-aware default selection (removal hits the opponent, costs use our least valuable cards)
    //  - handling of common staples, registered with AddStaples()
    //  - only uses Executor hooks present in every ExecutorBase build (desktop WindBot.exe and Android libWindbot)
    public abstract class CustomExecutorBase : DefaultExecutor
    {
        public static class Staple
        {
            public const int PotOfGreed = 55144522;
            public const int GracefulCharity = 79571449;
            public const int PotOfDuality = 98645731;
            public const int Raigeki = 12580477;
            public const int DarkHole = 53129443;
            public const int HarpiesFeatherDuster = 18144506;
            public const int TwinTwisters = 43898403;
            public const int MonsterReborn = 83764719;
            public const int PrematureBurial = 70828912;
            public const int CallOfTheHaunted = 97077563;
            public const int ChangeOfHeart = 4031928;
            public const int SnatchSteal = 45986603;
            public const int Confiscation = 17375316;
            public const int DelinquentDuo = 44763025;
            public const int ForcefulSentry = 42829885;
            public const int PainfulChoice = 74191942;
            public const int DimensionFusion = 23557835;
            public const int Terraforming = 73628505;
            public const int ReinforcementOfTheArmy = 32807846;
            public const int InfiniteImpermanence = 10045474;
            public const int BottomlessTrapHole = 29401950;
            public const int CompulsoryEvacuationDevice = 94192409;
            public const int Waboku = 12607053;
            public const int GravityBind = 85742772;
            public const int LimiterRemoval = 23171610;
            public const int Salvage = 96947648;
            public const int MysticalSpaceTyphoon = 5318639;
        }

        protected CustomExecutorBase(GameAI ai, Duel duel)
            : base(ai, duel)
        {
        }

        // ---------- helpers ----------

        protected bool MyTurn => Duel.Player == 0;
        protected bool Desc(int id, int n) => ActivateDescription == Util.GetStringId(id, n);

        // Single optional triggers arrive as a generic "activate trigger?" prompt; the current engine sends 221 in the
        // top 16 bits plus flags, which this WindBot passes through, so Desc() can't identify them.
        protected bool GenericTrigger => ActivateDescription == -1 || ActivateDescription == 0 || ActivateDescription == 221
            || (ActivateDescription >> 48) == 221;

        protected int Seen(int id) => Bot.Hand.Count(c => c.IsCode(id)) + Bot.GetMonsters().Count(c => c.IsCode(id))
            + Bot.GetSpells().Count(c => c.IsCode(id)) + Bot.Graveyard.Count(c => c.IsCode(id)) + Bot.Banished.Count(c => c.IsCode(id));
        protected bool MaybeInDeck(int id, int copies) => Seen(id) < copies;

        protected ClientCard BestEnemyMonster(bool canBeTarget = true)
        {
            return Util.GetProblematicEnemyMonster(0, canBeTarget) ?? Enemy.GetMonsters().Where(m => m.IsFaceup())
                .OrderByDescending(m => m.Attack).FirstOrDefault() ?? Enemy.GetMonsters().FirstOrDefault();
        }

        protected ClientCard BestEnemyCard(bool canBeTarget = true)
        {
            return Util.GetProblematicEnemyCard(0, canBeTarget) ?? BestEnemyMonster(canBeTarget) ?? Util.GetBestEnemySpell();
        }

        protected bool TargetEnemyCard()
        {
            var t = BestEnemyCard();
            if (t == null) return false;
            Pick(t);
            return true;
        }

        protected bool TargetEnemyMonster()
        {
            var t = BestEnemyMonster();
            if (t == null) return false;
            Pick(t);
            return true;
        }

        protected static int Rank(int[] order, ClientCard c)
        {
            int i = System.Array.IndexOf(order, c.Id);
            return i < 0 ? order.Length : i;
        }

        protected virtual int CardValue(ClientCard c)
        {
            if (c.IsMonster()) return c.IsFaceup() || c.Location != CardLocation.MonsterZone ? System.Math.Max(c.Attack, c.Defense) : 1500;
            return c.IsFaceup() ? 1000 : 1200;
        }

        // ---------- opponent's Spell/Trap negation ----------

        // Cards that negate our Spell/Trap activations once per turn. Spells/Traps that wreck our own board when they are
        // negated and destroyed (True Light, Eternal Soul) wait until the negation is used.
        static readonly int[] SpellTrapNegators = { 90809975 }; // Toadally Awesome
        bool enemyNegateUsed;

        protected bool EnemySpellTrapNegateReady => !enemyNegateUsed
            && Enemy.GetMonsters().Any(m => m.IsFaceup() && m.IsCode(SpellTrapNegators) && m.Overlays.Count > 0 && !m.IsDisabled());

        public override void OnChaining(int player, ClientCard card)
        {
            if (player == 1 && card != null && card.IsCode(SpellTrapNegators)) enemyNegateUsed = true;
            base.OnChaining(player, card);
        }

        public override void OnNewTurn()
        {
            enemyNegateUsed = false;
            base.OnNewTurn();
        }

        // ---------- explicit picks ----------

        readonly List<System.Func<IList<ClientCard>, IList<ClientCard>>> picks = new List<System.Func<IList<ClientCard>, IList<ClientCard>>>();

        protected void Pick(params int[] ids)
        {
            picks.Add(cards => ids.SelectMany(id => cards.Where(c => c.IsCode(id))).Distinct().ToList());
        }

        protected void Pick(ClientCard card)
        {
            picks.Add(cards => cards.Where(c => c == card).ToList());
        }

        protected void Pick(IEnumerable<ClientCard> preferred)
        {
            var list = preferred.ToList();
            picks.Add(cards => list.Where(cards.Contains).ToList());
        }

        // orders whatever the prompt offers (lowest rank first)
        protected void PickRanked(System.Func<ClientCard, int> rank)
        {
            picks.Add(cards => cards.OrderBy(rank).ToList());
        }

        // evaluated when the prompt arrives (e.g. discards after a draw)
        protected void PickLater(System.Func<IEnumerable<ClientCard>> preferred)
        {
            picks.Add(cards => preferred().Where(cards.Contains).ToList());
        }

        public override void OnChainEnd()
        {
            picks.Clear();
            base.OnChainEnd();
        }

        public override void OnNewPhase()
        {
            picks.Clear();
            base.OnNewPhase();
        }

        // ---------- per-deck preferences ----------

        protected virtual int[] AddToHandPriority => new int[0];
        protected virtual int[] SpSummonPriority => new int[0];
        protected virtual int[] SetPriority => new int[0];
        // cards we are happy to send / discard / tribute (first = most expendable)
        protected virtual int[] ExpendablePriority => new int[0];

        public override IList<ClientCard> OnSelectCard(IList<ClientCard> cards, int min, int max, long hint, bool cancelable)
        {
            if (cards.Count == 0) return null;
            // optional tributes (e.g. "can be Normal Summoned without Tributing"): tribute nothing
            if (min == 0 && hint == HintMsg.Release) return new List<ClientCard>();
            if (hint == HintMsg.FusionMaterial || hint == HintMsg.SynchroMaterial || hint == HintMsg.XyzMaterial
                || hint == HintMsg.LinkMaterial)
                return null;
            var chosen = new List<ClientCard>();
            if (picks.Count > 0)
            {
                var pick = picks[0];
                picks.RemoveAt(0);
                chosen = pick(cards).Take(System.Math.Max(max, 1)).ToList();
                if (chosen.Count >= min && chosen.Count > 0) return chosen;
                // an empty pick on an optional prompt ends a one-at-a-time selection (SelectUnselect "finish")
                if (min == 0) return chosen;
            }
            IEnumerable<ClientCard> ordered;
            bool enemyFirst = hint == HintMsg.Destroy || hint == HintMsg.Remove || hint == HintMsg.Control
                || hint == HintMsg.PosChange || hint == HintMsg.ReturnToHand || hint == HintMsg.Equip
                || hint == HintMsg.Facedown || hint == HintMsg.Target;
            if (enemyFirst)
                ordered = cards.OrderBy(c => c.Controller == 0 ? 1 : 0).ThenByDescending(CardValue);
            else if (hint == HintMsg.AddToHand)
                ordered = cards.OrderBy(c => Rank(AddToHandPriority, c)).ThenByDescending(CardValue);
            else if (hint == HintMsg.SpSummon)
                ordered = cards.OrderBy(c => Rank(SpSummonPriority, c)).ThenByDescending(c => c.Attack);
            else if (hint == HintMsg.Set)
                ordered = cards.OrderBy(c => Rank(SetPriority, c));
            else if (hint == HintMsg.Release || hint == HintMsg.Tribute || hint == HintMsg.Discard || hint == HintMsg.ToDeck
                     || hint == HintMsg.ToGrave)
                ordered = cards.OrderBy(c => c.Controller == 0 ? 0 : 1).ThenBy(c => Rank(ExpendablePriority, c)).ThenBy(CardValue);
            else
                return null;
            var picked = chosen.Concat(ordered.Where(c => !chosen.Contains(c))).Take(System.Math.Max(min, 1)).ToList();
            return picked.Count >= min ? picked : null;
        }

        // Ritual / sum tributes: use only what is needed (the engine rejects extra materials), cheapest first
        public override IList<ClientCard> OnSelectSum(IList<ClientCard> cards, int sum, int min, int max, long hint, bool mode)
        {
            if (hint != HintMsg.Release) return null;
            var order = cards.OrderBy(c => c.Location == CardLocation.Grave ? 0 : c.Location == CardLocation.Hand ? 1 : 2)
                .ThenBy(c => Rank(ExpendablePriority, c)).ThenBy(CardValue).ToList();
            var chosen = new List<ClientCard>();
            foreach (var c in order)
            {
                if (chosen.Sum(x => x.Level) >= sum) break;
                chosen.Add(c);
            }
            if (chosen.Sum(x => x.Level) < sum) return null;
            foreach (var c in chosen.OrderBy(x => x.Level).ToList())
                if (chosen.Sum(x => x.Level) - c.Level >= sum) chosen.Remove(c);
            if (mode && chosen.Sum(x => x.Level) != sum) return null;
            return chosen;
        }

        // Fallback Normal Summon: only the strongest Level 4 or lower monster in hand, not whichever comes first.
        protected bool BestFallbackSummon()
        {
            if (Card.Level > 4) return DefaultMonsterSummon();
            if (Bot.Hand.Any(c => c.IsMonster() && c.Level <= 4 && c.Attack > Card.Attack
                    && !Executors.Any(e => e.Type == ExecutorType.Summon && e.CardId == c.Id)))
                return false;
            return DefaultMonsterSummon();
        }

        // ---------- generic activation ----------

        // Optional effects without a specific rule: use them, unless a specific executor for the card exists
        // (its "not now" must win) or it would chain onto our own chain link from the hand.
        protected bool GenericActivate()
        {
            if (Executors.Any(e => e.Type == ExecutorType.Activate && e.CardId == Card.Id)) return false;
            if (Duel.LastChainPlayer == 0 && Duel.CurrentChain.Count > 0 && Card.Location == CardLocation.Hand) return false;
            return true;
        }

        protected bool SetTrapOrQuickPlay()
        {
            if (!Card.IsTrap() && !Card.HasType(CardType.QuickPlay)) return false;
            return Bot.GetSpellCountWithoutField() < 4;
        }

        // ---------- staples ----------

        protected void AddStaples()
        {
            AddExecutor(ExecutorType.Activate, Staple.PotOfGreed);
            AddExecutor(ExecutorType.Activate, Staple.GracefulCharity);
            AddExecutor(ExecutorType.Activate, Staple.PotOfDuality, () => { Pick(AddToHandPriority); return true; });
            AddExecutor(ExecutorType.Activate, Staple.Terraforming, () => { Pick(AddToHandPriority); return true; });
            AddExecutor(ExecutorType.Activate, Staple.ReinforcementOfTheArmy, () => { Pick(AddToHandPriority); return true; });
            AddExecutor(ExecutorType.Activate, Staple.Raigeki, DefaultRaigeki);
            AddExecutor(ExecutorType.Activate, Staple.DarkHole, DefaultDarkHole);
            AddExecutor(ExecutorType.Activate, Staple.HarpiesFeatherDuster, DefaultHarpiesFeatherDusterFirst);
            AddExecutor(ExecutorType.Activate, Staple.TwinTwisters, TwinTwistersEffect);
            AddExecutor(ExecutorType.Activate, Staple.MysticalSpaceTyphoon, DefaultMysticalSpaceTyphoon);
            AddExecutor(ExecutorType.Activate, Staple.ChangeOfHeart, StealEffect);
            AddExecutor(ExecutorType.Activate, Staple.SnatchSteal, StealEffect);
            AddExecutor(ExecutorType.Activate, Staple.MonsterReborn, () => ReviveBest(true));
            AddExecutor(ExecutorType.Activate, Staple.PrematureBurial, () => ReviveBest(false));
            AddExecutor(ExecutorType.Activate, Staple.CallOfTheHaunted, () => ReviveBest(false));
            AddExecutor(ExecutorType.Activate, Staple.Confiscation, () => Bot.LifePoints > 2000 && Enemy.GetHandCount() > 0);
            AddExecutor(ExecutorType.Activate, Staple.DelinquentDuo, () => Bot.LifePoints > 2000 && Enemy.GetHandCount() >= 2);
            AddExecutor(ExecutorType.Activate, Staple.ForcefulSentry, () => Enemy.GetHandCount() > 0);
            AddExecutor(ExecutorType.Activate, Staple.DimensionFusion, () => Bot.LifePoints > 3000 && Bot.Banished.Any(c => c.IsMonster()));
            AddExecutor(ExecutorType.Activate, Staple.InfiniteImpermanence, DefaultInfiniteImpermanence);
            AddExecutor(ExecutorType.Activate, Staple.CompulsoryEvacuationDevice, DefaultCompulsoryEvacuationDevice);
            AddExecutor(ExecutorType.Activate, Staple.BottomlessTrapHole, () => Duel.LastSummonPlayer == 1
                && Duel.LastSummonedCards.Any(c => c.Attack >= 1500 || c.IsMonsterDangerous()));
            AddExecutor(ExecutorType.Activate, Staple.Waboku, WabokuEffect);
            AddExecutor(ExecutorType.Activate, Staple.GravityBind, () => Enemy.GetMonsters().Any(m => m.Level >= 4 || m.Rank >= 4 || m.LinkCount > 0)
                && !Bot.HasInSpellZone(Staple.GravityBind, true));
            AddExecutor(ExecutorType.Activate, Staple.LimiterRemoval, LimiterRemovalEffect);
            AddExecutor(ExecutorType.Activate, Staple.Salvage);
        }

        protected bool TwinTwistersEffect()
        {
            if (Enemy.GetSpellCount() == 0) return false;
            Pick(Bot.Hand.OrderBy(c => Rank(ExpendablePriority, c)).ThenBy(CardValue).Take(1));
            Pick(Enemy.GetSpells().OrderByDescending(c => c.IsFacedown()).Take(2));
            return true;
        }

        protected bool StealEffect()
        {
            var t = Enemy.GetMonsters().Where(m => m.IsFaceup() && !m.IsShouldNotBeTarget()).OrderByDescending(m => m.Attack).FirstOrDefault()
                    ?? Enemy.GetMonsters().FirstOrDefault();
            if (t == null) return false;
            Pick(t);
            return true;
        }

        protected virtual bool ReviveBest(bool eitherGY)
        {
            var pool = Bot.Graveyard.Where(c => c.IsMonster()).ToList();
            if (eitherGY) pool.AddRange(Enemy.Graveyard.Where(c => c.IsMonster()));
            var t = pool.OrderBy(c => Rank(SpSummonPriority, c)).ThenByDescending(c => c.Attack).FirstOrDefault();
            if (t == null || (t.Attack < 1500 && Rank(SpSummonPriority, t) == SpSummonPriority.Length)) return false;
            if (MyTurn && Duel.Phase != DuelPhase.Main1 && Duel.Phase != DuelPhase.Main2) return false;
            if (!MyTurn && Duel.Phase != DuelPhase.End && Duel.Phase != DuelPhase.BattleStart) return false;
            Pick(t);
            return true;
        }

        protected bool WabokuEffect()
        {
            if (MyTurn) return false;
            if (Duel.Phase != DuelPhase.BattleStart && Duel.Phase != DuelPhase.BattleStep) return false;
            int incoming = Enemy.GetMonsters().Where(m => m.IsAttack()).Sum(m => m.Attack);
            return incoming >= Bot.LifePoints || Enemy.GetMonsters().Any(m => m.IsAttack() && m.Attack > Util.GetBestPower(Bot));
        }

        protected bool LimiterRemovalEffect()
        {
            if (!MyTurn || Duel.Phase != DuelPhase.BattleStart) return false;
            int machines = Bot.GetMonsters().Where(m => m.IsFaceup() && m.HasRace(CardRace.Machine) && m.IsAttack()).Sum(m => m.Attack);
            return machines * 2 >= Enemy.LifePoints || Enemy.GetMonsters().Any(m => m.IsFaceup() && m.Attack > Util.GetBestAttack(Bot));
        }
    }
}
