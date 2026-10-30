using System.Collections.Generic;
using YGOSharp.OCGWrapper.Enums;

namespace WindBot.Game.AI.Decks
{
    // Plays the custom "Slasher" deck (cards 92900000-92900036).
    // Game plan: get Hunting Grounds and Tow Truck down to build Kill Counters, extend with Plastic Partner /
    // Cannibal Brute / Watcher, set Slasher Traps (Bear Trap, Barbed Wire, Doll Store), revive the Level 8 bosses
    // with the Quick-Play Spells, and spend Kill Counters on removal (Drowned Revenant, Preordained Design, Puppet).
    [Deck("Slasher", "AI_Slasher")]
    public class SlasherExecutor : DefaultExecutor
    {
        public class CardId
        {
            public const int HuntingGrounds = 92900000;
            public const int Butcher = 92900001;
            public const int PlasticPartner = 92900002;
            public const int PossessedPuppet = 92900003;
            public const int Damballa = 92900004;
            public const int Shape = 92900005;
            public const int Entity = 92900006;
            public const int Caller = 92900007;
            public const int DreamDemon = 92900008;
            public const int DrownedRevenant = 92900009;
            public const int DollHeir = 92900010;
            public const int Messenger = 92900011;
            public const int Miner = 92900012;
            public const int JumpScare = 92900013;
            public const int BarbedWire = 92900014;
            public const int BearTrap = 92900015;
            public const int LightningRevival = 92900016;
            public const int SchismBox = 92900017;
            public const int TowTruck = 92900018;
            public const int WrongTurn = 92900019;
            public const int Cackler = 92900020;
            public const int Watcher = 92900021;
            public const int Brute = 92900022;
            public const int Patriarch = 92900023;
            public const int Showdown = 92900024;
            public const int CurseOfPazuzu = 92900025;
            public const int ChestOfSouls = 92900026;
            public const int DollStore = 92900027;
            public const int BrittleBlessing = 92900028;
            public const int CovetousCreature = 92900029;
            public const int HallowsGuard = 92900030;
            public const int PreordainedDesign = 92900031;
            public const int AncientStalker = 92900032;
            public const int CosmicEntity = 92900033;
            public const int CurseOfThorn = 92900034;
            public const int CursedVideotape = 92900035;
            public const int WellDweller = 92900036;
        }

        const int SetSlasher = 0x991;
        const int SetCannibal = 0x1991;

        // Preference orders used when the engine asks us to pick cards
        static readonly int[] SearchPriority = {
            CardId.HuntingGrounds, CardId.TowTruck, CardId.Damballa, CardId.WellDweller, CardId.LightningRevival,
            CardId.ChestOfSouls, CardId.CurseOfThorn, CardId.Brute, CardId.Patriarch, CardId.Cackler,
            CardId.Watcher, CardId.Butcher, CardId.HallowsGuard, CardId.Messenger, CardId.Miner, CardId.PlasticPartner,
            CardId.PossessedPuppet, CardId.SchismBox, CardId.Showdown, CardId.CursedVideotape, CardId.BrittleBlessing,
            CardId.CovetousCreature, CardId.Caller
        };
        static readonly int[] RevivePriority = {
            CardId.DrownedRevenant, CardId.Shape, CardId.Patriarch, CardId.DreamDemon, CardId.WellDweller,
            CardId.Butcher, CardId.Brute, CardId.Watcher, CardId.Caller, CardId.Miner, CardId.Cackler
        };
        static readonly int[] TrapSetPriority = {
            CardId.BearTrap, CardId.DollStore, CardId.BarbedWire, CardId.CurseOfPazuzu, CardId.JumpScare, CardId.WrongTurn
        };
        // Level 8s for Slasher Showdown: Dream Demon works from the GY, Drowned Revenant can climb back out
        static readonly int[] ShowdownSendPriority = { CardId.DreamDemon, CardId.DrownedRevenant, CardId.Shape };

        public SlasherExecutor(GameAI ai, Duel duel)
            : base(ai, duel)
        {
            // Hand traps and responses first
            AddExecutor(ExecutorType.Activate, CardId.HallowsGuard, HallowsGuardEffect);
            AddExecutor(ExecutorType.Activate, CardId.HuntingGrounds, HuntingGroundsEffect);
            AddExecutor(ExecutorType.Activate, CardId.CovetousCreature, CovetousEffect);
            AddExecutor(ExecutorType.Activate, CardId.DollHeir, DollHeirEffect);
            AddExecutor(ExecutorType.Activate, CardId.BearTrap, BearTrapEffect);
            AddExecutor(ExecutorType.Activate, CardId.BarbedWire);
            AddExecutor(ExecutorType.Activate, CardId.CurseOfPazuzu, PazuzuEffect);
            AddExecutor(ExecutorType.Activate, CardId.JumpScare, JumpScareEffect);
            AddExecutor(ExecutorType.Activate, CardId.WrongTurn);
            AddExecutor(ExecutorType.Activate, CardId.CursedVideotape, VideotapeEffect);

            // Engine
            AddExecutor(ExecutorType.Activate, CardId.TowTruck, TowTruckActivate);
            AddExecutor(ExecutorType.Activate, CardId.SchismBox);
            AddExecutor(ExecutorType.Activate, CardId.Showdown, ShowdownEffect);
            AddExecutor(ExecutorType.Activate, CardId.DollStore, DollStoreEffect);
            AddExecutor(ExecutorType.SpSummon, CardId.PlasticPartner, () => CountSummon());
            AddExecutor(ExecutorType.Activate, CardId.PlasticPartner, PlasticPartnerEffect);
            AddExecutor(ExecutorType.Activate, CardId.Brute, BruteEffect);
            AddExecutor(ExecutorType.Activate, CardId.Miner, MinerEffect);
            AddExecutor(ExecutorType.Activate, CardId.Cackler, CacklerEffect);
            AddExecutor(ExecutorType.Activate, CardId.Watcher, WatcherEffect);
            AddExecutor(ExecutorType.SpSummon, CardId.Patriarch, PatriarchSummon);
            AddExecutor(ExecutorType.Activate, CardId.Patriarch);
            AddExecutor(ExecutorType.Activate, CardId.HuntingGrounds, FieldActivate);
            AddExecutor(ExecutorType.Activate, CardId.PossessedPuppet, PuppetEffect);
            AddExecutor(ExecutorType.Activate, CardId.WellDweller, WellDwellerEffect);
            AddExecutor(ExecutorType.Activate, CardId.Damballa, DamballaActivate);
            AddExecutor(ExecutorType.Activate, CardId.BrittleBlessing, BrittleBlessingActivate);

            // Synchro branches when the opponent has a board, then the Stalker link
            AddExecutor(ExecutorType.SpSummon, CardId.CosmicEntity, () => EnemyHasBoard && CosmicEntitySummon());
            AddExecutor(ExecutorType.SpSummon, CardId.Entity, EntitySummon);
            // Combo: link Cackler + Plastic Partner into Ancient Stalker (searches Hunting Grounds) before reviving
            AddExecutor(ExecutorType.SpSummon, CardId.AncientStalker, StalkerSummon);
            AddExecutor(ExecutorType.Activate, CardId.AncientStalker, StalkerEffect);

            // Removal and boss revival
            AddExecutor(ExecutorType.Activate, CardId.DrownedRevenant, RevenantEffect);
            AddExecutor(ExecutorType.Activate, CardId.CosmicEntity, CosmicEntityEffect);
            AddExecutor(ExecutorType.Activate, CardId.Entity, EntityEffect);
            AddExecutor(ExecutorType.Activate, CardId.PreordainedDesign, PreordainedEffect);
            AddExecutor(ExecutorType.Activate, CardId.DreamDemon, TargetsEnemyCard);
            AddExecutor(ExecutorType.Activate, CardId.ChestOfSouls, ReviveSpellEffect);
            AddExecutor(ExecutorType.Activate, CardId.LightningRevival, ReviveSpellEffect);
            AddExecutor(ExecutorType.Activate, CardId.CurseOfThorn, ReviveSpellEffect);
            AddExecutor(ExecutorType.Activate, CardId.Caller, CallerEffect);
            AddExecutor(ExecutorType.Activate, CardId.Messenger, MessengerEffect);

            // Extra Deck
            AddExecutor(ExecutorType.SpSummon, CardId.CosmicEntity, CosmicEntitySummon);
            AddExecutor(ExecutorType.SpSummon, CardId.PreordainedDesign, PreordainedSummon);
            AddExecutor(ExecutorType.SpSummon, CardId.Entity, EntitySummon);
            AddExecutor(ExecutorType.SpSummon, CardId.DollHeir, DollHeirSummon);

            // Normal Summons
            AddExecutor(ExecutorType.Summon, CardId.Cackler, NormalSummon);
            AddExecutor(ExecutorType.Summon, CardId.Miner, NormalSummon);
            AddExecutor(ExecutorType.Summon, CardId.Butcher, () => ButcherSummon() && NormalSummon());
            AddExecutor(ExecutorType.Summon, CardId.Caller, NormalSummon);
            AddExecutor(ExecutorType.Summon, CardId.Watcher, NormalSummon);
            AddExecutor(ExecutorType.Summon, CardId.Brute, NormalSummon);
            AddExecutor(ExecutorType.Summon, CardId.PlasticPartner, NormalSummon);
            AddExecutor(ExecutorType.MonsterSet, CardId.Messenger, NormalSummon);
            AddExecutor(ExecutorType.SummonOrSet, () => DefaultMonsterSummon() && NormalSummon());
            AddExecutor(ExecutorType.MonsterSet, CardId.HallowsGuard, () => LowValueSet() && NormalSummon());
            AddExecutor(ExecutorType.MonsterSet, CardId.CovetousCreature, () => LowValueSet() && NormalSummon());

            // Everything else: equips, sets, positions, and any remaining optional effect
            AddExecutor(ExecutorType.SpellSet, SpellSet);
            AddExecutor(ExecutorType.Repos, DefaultMonsterRepos);
            AddExecutor(ExecutorType.Activate, GenericActivate);
        }

        bool summonedThisTurn;

        // Explicit choices for upcoming card prompts, in order. WindBot's own AI.SelectCard queue is bypassed by
        // OnSelectCard below, so the executor keeps its own.
        readonly List<System.Func<IList<ClientCard>, IList<ClientCard>>> picks = new List<System.Func<IList<ClientCard>, IList<ClientCard>>>();

        void Pick(params int[] ids)
        {
            picks.Add(cards => ids.SelectMany(id => cards.Where(c => c.IsCode(id))).Distinct().ToList());
        }

        void Pick(ClientCard card)
        {
            picks.Add(cards => cards.Where(c => c == card).ToList());
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

        public override void OnNewTurn()
        {
            summonedThisTurn = false;
            if (!TowTruckUp) killCounters = 0;
            base.OnNewTurn();
        }

        bool NormalSummon()
        {
            summonedThisTurn = true;
            if (TowTruckUp) killCounters++;
            return true;
        }

        // Kill Counters on our side, estimated: Tow Truck gains one per Slasher summon, costs spend them.
        int killCounters;
        bool TowTruckUp => Bot.SpellZone.Any(c => c != null && c.IsFaceup() && c.IsCode(CardId.TowTruck));

        // Called by our own actions that Special Summon a Slasher (only hooks present in every ExecutorBase are used)
        bool CountSummon(bool result = true)
        {
            if (result && TowTruckUp) killCounters++;
            return result;
        }

        static readonly int[] Bosses = {
            CardId.DrownedRevenant, CardId.Shape, CardId.DreamDemon, CardId.Patriarch, CardId.WellDweller,
            CardId.CosmicEntity, CardId.PreordainedDesign, CardId.Entity
        };

        // Slashers we are happy to use as Link/Synchro material
        bool Fodder(ClientCard m) => m.IsFaceup() && IsSlasher(m) && !m.IsCode(Bosses) && m.Attack < 2000;

        // Copies we can see (hand, field, GY, banished); anything else may still be in the Deck
        int Seen(int id) => Bot.Hand.Count(c => c.IsCode(id)) + Bot.GetMonsters().Count(c => c.IsCode(id))
            + Bot.GetSpells().Count(c => c.IsCode(id)) + Bot.Graveyard.Count(c => c.IsCode(id)) + Bot.Banished.Count(c => c.IsCode(id));
        bool MaybeInDeck(int id, int copies) => Seen(id) < copies;

        // The 1-card line: Tow Truck > Cackler > Doll Store > Plastic Partner > Showdown (Revenant, Lightning Revival)
        // > link Cackler + Plastic into Ancient Stalker (Hunting Grounds) > Lightning Revival on Drowned Revenant
        bool ComboAvailable => Bot.HasInExtra(CardId.AncientStalker) && !Bot.HasInMonstersZone(CardId.AncientStalker);

        bool IsSlasher(ClientCard c) => c != null && c.HasSetcode(SetSlasher);
        bool IsCannibal(ClientCard c) => c != null && c.HasSetcode(SetCannibal);
        bool MyTurn => Duel.Player == 0;
        bool Desc(int id, int n) => ActivateDescription == Util.GetStringId(id, n);
        // Single optional triggers arrive as a generic "activate trigger?" prompt. The current engine encodes it as
        // 221 in the top 16 bits (plus flags), which this WindBot build passes through unchanged, so Desc() can't
        // identify the effect - work it out from where the card is instead.
        bool GenericTrigger => ActivateDescription == -1 || ActivateDescription == 0 || ActivateDescription == 221
            || (ActivateDescription >> 48) == 221;
        ClientCard LastChain => Util.GetLastChainCard();
        int SlashersOnField => Bot.GetMonsters().Count(m => m.IsFaceup() && IsSlasher(m));

        ClientCard BestEnemyMonster(bool canBeTarget = true)
        {
            return Util.GetProblematicEnemyMonster(0, canBeTarget) ?? Enemy.GetMonsters().Where(m => m.IsFaceup())
                .OrderByDescending(m => m.Attack).FirstOrDefault() ?? Enemy.GetMonsters().FirstOrDefault();
        }

        ClientCard BestEnemyCard(bool canBeTarget = true)
        {
            return Util.GetProblematicEnemyCard(0, canBeTarget) ?? BestEnemyMonster(canBeTarget) ?? Util.GetBestEnemySpell();
        }

        // ---------- responses ----------

        bool HallowsGuardEffect()
        {
            // Search when it hits the GY; the negate only appears when the opponent responds to our Slasher effect
            return true;
        }

        bool HuntingGroundsEffect()
        {
            // Remove 3 Kill Counters to negate whatever the opponent just activated
            return Desc(CardId.HuntingGrounds, 1) && Duel.LastChainPlayer == 1;
        }

        bool FieldActivate()
        {
            if (Card.Location == CardLocation.SpellZone && Card.IsFaceup()) return false;
            return Bot.SpellZone[5] == null || !Bot.SpellZone[5].IsCode(CardId.HuntingGrounds);
        }

        bool CovetousEffect()
        {
            return true;
        }

        bool DollHeirEffect()
        {
            if (Desc(CardId.DollHeir, 0))
            {
                // attach: take the opponent's best monster out of their GY
                var target = Enemy.Graveyard.Where(c => c.IsMonster()).OrderByDescending(c => c.Attack).FirstOrDefault()
                             ?? Bot.Graveyard.Where(c => c.IsMonster() && !RevivePriority.Contains(c.Id)).FirstOrDefault();
                if (target == null) return false;
                Pick(target);
                return true;
            }
            // Quick Effect: in Attack Position it flips to Defense (protection); in Defense it flips to Attack (destroy)
            return Duel.LastChainPlayer == 1 && Card.HasXyzMaterial();
        }

        bool BearTrapEffect()
        {
            var summoned = Duel.LastSummonedCards?.Where(c => c.Controller == 1).ToList() ?? new List<ClientCard>();
            // save it for a real threat rather than a small searcher
            return summoned.Any(c => c.Attack >= 2400 || c.IsMonsterDangerous() || c.IsFloodgate()
                || (c.Attack >= 2000 && Bot.GetMonsters().All(m => m.Attack < c.Attack)));
        }

        bool PazuzuEffect()
        {
            var target = Enemy.GetMonsters().Where(m => m.IsFaceup() && !m.IsShouldNotBeTarget())
                .OrderByDescending(m => m.Attack).FirstOrDefault();
            if (target == null || target.Attack < 1500) return false;
            Pick(target);
            return true;
        }

        bool JumpScareEffect()
        {
            if (Duel.Phase != DuelPhase.BattleStart && Duel.Phase != DuelPhase.BattleStep) return false;
            return Bot.Hand.Any(c => IsSlasher(c) && c.IsMonster() && c.Level <= 8 && c.Attack >= 1500);
        }

        bool VideotapeEffect()
        {
            if (Card.Location == CardLocation.Hand || (Card.Location == CardLocation.SpellZone && Card.IsFacedown()))
            {
                var target = Enemy.GetMonsters().Where(m => m.IsFaceup()).OrderByDescending(m => m.Attack).FirstOrDefault();
                if (target == null) return false;
                Pick(target);
            }
            return true;
        }

        // ---------- engine ----------

        bool TowTruckActivate()
        {
            if (Card.Location == CardLocation.SpellZone && Card.IsFaceup()) return false;
            if (Bot.SpellZone.Count(c => c != null && c.IsCode(CardId.TowTruck) && c.IsFaceup()) >= 2) return false;
            bool trapInHand = Bot.Hand.Any(c => c.IsTrap() && IsSlasher(c));
            if (!summonedThisTurn && !Bot.HasInHand(CardId.Cackler))
                Pick(CardId.Cackler, CardId.Brute, CardId.Watcher, CardId.Butcher, CardId.Patriarch);
            else if (trapInHand)
                Pick(CardId.Brute, CardId.Watcher, CardId.Patriarch, CardId.Cackler, CardId.Butcher);
            else
                Pick(CardId.Watcher, CardId.Patriarch, CardId.Brute, CardId.Butcher, CardId.Cackler);
            return true;
        }

        bool ShowdownEffect()
        {
            if (Card.Location == CardLocation.Grave) return true;
            bool revivalInHand = Bot.Hand.Any(c => c.IsCode(CardId.LightningRevival, CardId.ChestOfSouls, CardId.CurseOfThorn));
            if (killCounters == 0 && !revivalInHand)
            {
                // no Quick-Play coming: Dream Demon destroys a card every End Phase from the GY
                Pick(CardId.DreamDemon, CardId.DrownedRevenant, CardId.Shape);
                return true;
            }
            if (killCounters == 0 && revivalInHand)
            {
                // match the boss to the revival spell already in hand
                if (Bot.HasInHand(CardId.LightningRevival)) Pick(CardId.DrownedRevenant, CardId.DreamDemon, CardId.Shape);
                else if (Bot.HasInHand(CardId.CurseOfThorn)) Pick(CardId.Shape, CardId.DrownedRevenant, CardId.DreamDemon);
                else Pick(CardId.DreamDemon, CardId.DrownedRevenant, CardId.Shape);
                return true;
            }
            killCounters--;
            if (MaybeInDeck(CardId.DrownedRevenant, 1) && MaybeInDeck(CardId.LightningRevival, 1))
            {
                Pick(CardId.DrownedRevenant);
                Pick(CardId.LightningRevival, CardId.ChestOfSouls, CardId.CurseOfThorn);
            }
            else if (MaybeInDeck(CardId.Shape, 1) && MaybeInDeck(CardId.CurseOfThorn, 1))
            {
                Pick(CardId.Shape);
                Pick(CardId.CurseOfThorn, CardId.LightningRevival, CardId.ChestOfSouls);
            }
            else
            {
                Pick(ShowdownSendPriority);
                Pick(CardId.ChestOfSouls, CardId.LightningRevival, CardId.CurseOfThorn);
            }
            return true;
        }

        bool DollStoreEffect()
        {
            // free search; prefer to resolve it on the opponent's End Phase or our own Main Phase
            if (!MyTurn && Duel.Phase != DuelPhase.End) return false;
            Pick(CardId.PlasticPartner, CardId.HallowsGuard, CardId.Messenger, CardId.PossessedPuppet);
            return CountSummon();
        }

        bool PlasticPartnerEffect()
        {
            if (Desc(CardId.PlasticPartner, 0) || (GenericTrigger && Card.Location == CardLocation.MonsterZone))
            {
                // search: Showdown to set up the revival line (Stalker then fetches the field spell),
                // otherwise field spell, then Tow Truck, then the ritual line
                bool revivalLine = MaybeInDeck(CardId.DrownedRevenant, 1) && MaybeInDeck(CardId.Showdown, 1)
                    && (MaybeInDeck(CardId.LightningRevival, 1) || MaybeInDeck(CardId.ChestOfSouls, 1) || MaybeInDeck(CardId.CurseOfThorn, 1));
                // Showdown only fetches its Quick-Play if a Kill Counter can be removed
                if (revivalLine && killCounters > 0) Pick(CardId.Showdown, CardId.HuntingGrounds, CardId.TowTruck);
                else if (!TowTruckUp && !Bot.HasInHand(CardId.TowTruck) && MaybeInDeck(CardId.TowTruck, 3)) Pick(CardId.TowTruck, CardId.HuntingGrounds);
                else if (Bot.SpellZone[5] == null && !Bot.HasInHand(CardId.HuntingGrounds) && !ComboAvailable) Pick(CardId.HuntingGrounds);
                else if (MaybeInDeck(CardId.Showdown, 1) && MaybeInDeck(CardId.DreamDemon, 1)) Pick(CardId.Showdown, CardId.HuntingGrounds);
                else if (!Bot.HasInHandOrInSpellZone(CardId.TowTruck)) Pick(CardId.TowTruck);
                else Pick(CardId.SchismBox, CardId.ChestOfSouls, CardId.LightningRevival, CardId.CurseOfThorn, CardId.Damballa);
            }
            return true;
        }

        bool BruteEffect()
        {
            Pick(TrapSetPriority.Reverse().ToArray()); // reveal the least valuable trap
            return CountSummon();
        }

        bool MinerEffect()
        {
            return true;
        }

        bool CacklerEffect()
        {
            if (MyTurn && Card.Location == CardLocation.MonsterZone && MaybeInDeck(CardId.PlasticPartner, 3) && MaybeInDeck(CardId.DollStore, 2))
                Pick(CardId.DollStore, CardId.BearTrap, CardId.BarbedWire, CardId.CurseOfPazuzu, CardId.JumpScare, CardId.WrongTurn);
            else
                Pick(TrapSetPriority);
            return true;
        }

        bool WatcherEffect()
        {
            if (Card.Location == CardLocation.MonsterZone)
            {
                var spell = Util.GetBestEnemySpell();
                if (spell == null) return false;
                Pick(spell);
            }
            return true;
        }

        bool PatriarchSummon()
        {
            // Tribute 1 Cannibal: worth it when a field spell is missing or when we need a 2500 body
            var tribute = Bot.GetMonsters().Where(m => IsCannibal(m) && !m.IsCode(CardId.Patriarch)).OrderBy(m => m.Attack).FirstOrDefault();
            if (tribute == null) return false;
            if (tribute.Attack >= 2400 && Bot.SpellZone[5] != null) return false;
            // don't eat a Stalker material while the link is still possible
            if (ComboAvailable && Bot.GetMonsters().Count(Fodder) == 2 && Fodder(tribute)) return false;
            Pick(tribute);
            return CountSummon();
        }

        bool PuppetEffect()
        {
            if (Card.Location == CardLocation.Hand)
                return Bot.Hand.Count(c => IsSlasher(c) && c.IsMonster()) >= 2 && !Bot.HasInHand(CardId.Damballa);
            if (Desc(CardId.PossessedPuppet, 2))
            {
                // zero out the biggest threat before attacking
                if (!MyTurn || Duel.Phase != DuelPhase.Main1) return false;
                var target = Enemy.GetMonsters().Where(m => m.IsFaceup()).OrderByDescending(m => m.Attack).FirstOrDefault();
                if (target == null || target.Attack < 1000) return false;
                Pick(target);
                return true;
            }
            Pick(CardId.DreamDemon, CardId.DrownedRevenant, CardId.Shape);
            return true;
        }

        bool WellDwellerEffect()
        {
            if (Card.Location == CardLocation.Hand)
                return !Bot.HasInHand(CardId.Damballa) && !Bot.HasInHandOrInSpellZone(CardId.CursedVideotape);
            return true;
        }

        bool DamballaActivate()
        {
            if (Bot.HasInHand(CardId.WellDweller)) Pick(CardId.WellDweller);
            else Pick(CardId.PossessedPuppet);
            return true;
        }

        bool BrittleBlessingActivate()
        {
            var target = Enemy.Graveyard.Where(c => c.IsMonster()).OrderByDescending(c => c.Attack).FirstOrDefault();
            if (target == null || target.Attack < 1500) return false;
            Pick(target);
            return true;
        }

        // ---------- removal / bosses ----------

        bool RevenantEffect()
        {
            if (Card.Location == CardLocation.Grave) return true;
            return TargetsEnemyMonster();
        }

        bool CosmicEntityEffect()
        {
            if (Card.Location == CardLocation.MonsterZone && !Desc(CardId.CosmicEntity, 0) && !GenericTrigger)
                return TargetsEnemyCard();
            return Enemy.GetFieldCount() > 0;
        }

        bool EntityEffect()
        {
            if (Card.Location != CardLocation.MonsterZone) return true;
            var target = Enemy.GetMonsters().Where(m => m.IsFaceup() && !m.IsShouldNotBeTarget()).OrderByDescending(m => m.BaseAttack).FirstOrDefault();
            if (target == null || target.BaseAttack < 1000) return false;
            Pick(target);
            return true;
        }

        bool PreordainedEffect()
        {
            return TargetsEnemyCard();
        }

        bool TargetsEnemyCard()
        {
            var target = BestEnemyCard();
            if (target == null) return false;
            Pick(target);
            return true;
        }

        bool TargetsEnemyMonster()
        {
            var target = BestEnemyMonster();
            if (target == null) return false;
            Pick(target);
            return true;
        }

        bool ReviveSpellEffect()
        {
            var target = RevivePriority.Select(id => Bot.Graveyard.FirstOrDefault(c => c.IsCode(id))).FirstOrDefault(c => c != null);
            if (target == null) return false;
            // On our turn use it in Main Phase 1; on theirs, at the End Phase or to block a lethal attack
            if (MyTurn && Duel.Phase != DuelPhase.Main1) return false;
            if (MyTurn && Duel.CurrentChain.Count > 0) return false; // let searches resolve (field spell first)
            if (MyTurn && ComboAvailable && Bot.GetMonsters().Count(Fodder) >= 2) return false; // link first
            if (MyTurn && Bot.HasInHand(CardId.HuntingGrounds) && Bot.SpellZone[5] == null) return false; // field spell first
            if (!MyTurn && Duel.Phase != DuelPhase.End && !(Duel.Phase == DuelPhase.BattleStart && Bot.GetMonsterCount() == 0)) return false;
            Pick(target);
            return CountSummon();
        }

        bool CallerEffect()
        {
            if (Card.Location == CardLocation.Grave) return true;
            return Bot.Hand.Count > 0 && Enemy.GetFieldCount() > 0;
        }

        bool MessengerEffect()
        {
            if (Desc(CardId.Messenger, 3)) return Enemy.GetMonsterCount() > 0 && Card.IsFaceup();
            return true;
        }

        // ---------- extra deck ----------

        bool ExtraDeckSummon()
        {
            return Bot.GetMonsterCount() >= 2;
        }

        // Tuner + non-Tuner face-up monsters whose Levels add up exactly; non-Tuners must be Slashers if slasherOnly
        List<ClientCard> SynchroMaterials(int level, bool slasherOnly, bool allowBosses)
        {
            var mons = Bot.GetMonsters().Where(m => m.IsFaceup() && m.Level > 0 && !m.HasType(CardType.Xyz | CardType.Link)).ToList();
            foreach (var tuner in mons.Where(m => m.IsTuner()).OrderBy(m => m.Attack))
            {
                var others = mons.Where(m => m != tuner && (!slasherOnly || IsSlasher(m)) && (allowBosses || !m.IsCode(Bosses)))
                    .OrderBy(m => m.IsTuner() ? 1 : 0).ThenBy(m => m.Attack).ToList();
                // small subset search (field has at most 7 monsters)
                for (int mask = 1; mask < (1 << others.Count); mask++)
                {
                    var pick = MiniLinq.Range(0, others.Count).Where(i => (mask & (1 << i)) != 0).Select(i => others[i]).ToList();
                    if (pick.Any(m => m.IsTuner()) && pick.All(m => m.IsTuner())) continue;
                    if (tuner.Level + pick.Sum(m => m.Level) == level)
                        return new List<ClientCard> { tuner }.Concat(pick).ToList();
                }
            }
            return null;
        }

        bool EnemyHasBoard => Enemy.GetMonsters().Any(m => m.IsFaceup() && m.Attack >= 2000) || Enemy.GetFieldCount() >= 3;

        bool CosmicEntitySummon()
        {
            // Lv10: negates the opponent's face-up cards; worth spending a boss on only when they have a board
            var mats = SynchroMaterials(10, true, EnemyHasBoard) ?? (EnemyHasBoard ? null : SynchroMaterials(10, true, false));
            if (mats == null) return false;
            if (!EnemyHasBoard && mats.Any(m => m.IsCode(Bosses))) return false;
            AI.SelectMaterials(mats);
            return true;
        }

        bool EntitySummon()
        {
            // Lv6, 0 ATK: steals the ATK of their best monster, so only when they have one worth stealing
            if (!Enemy.GetMonsters().Any(m => m.IsFaceup() && m.BaseAttack >= 2000)) return false;
            var mats = SynchroMaterials(6, false, false);
            if (mats == null) return false;
            AI.SelectMaterials(mats);
            return true;
        }

        bool DollHeirSummon()
        {
            var lv2 = Bot.GetMonsters().Where(m => Fodder(m) && m.Level == 2 && !m.HasType(CardType.Xyz | CardType.Link))
                .OrderBy(m => m.Attack).ToList();
            if (lv2.Count < 2) return false;
            // keep two bodies for Ancient Stalker if it is still in the Extra Deck
            if (ComboAvailable && Bot.GetMonsters().Count(Fodder) < 4) return false;
            AI.SelectMaterials(lv2.Take(2).ToList());
            return true;
        }

        bool PreordainedSummon()
        {
            // 3 Slashers on the field become a protected removal engine; do it when we are behind on board
            return SlashersOnField >= 3 && (Util.IsOneEnemyBetter() || Enemy.GetSpellCount() >= 2);
        }

        bool StalkerSummon()
        {
            var cheap = Bot.GetMonsters().Where(Fodder).OrderBy(m => m.Attack).Take(2).ToList();
            if (cheap.Count < 2) return false;
            if (TowTruckUp) killCounters++;
            AI.SelectMaterials(cheap);
            return true;
        }

        bool StalkerEffect()
        {
            if (Bot.SpellZone[5] == null && !Bot.HasInHand(CardId.HuntingGrounds)) Pick(CardId.HuntingGrounds, CardId.TowTruck);
            else Pick(CardId.TowTruck, CardId.HuntingGrounds);
            return true;
        }

        // ---------- summons / sets ----------

        bool ButcherSummon()
        {
            return Enemy.GetMonsterCount() > Bot.GetMonsterCount() || DefaultMonsterSummon();
        }

        bool LowValueSet()
        {
            return Bot.GetMonsterCount() == 0;
        }

        bool SpellSet()
        {
            if (Card.IsTrap()) return Bot.GetSpellCountWithoutField() < 4;
            if (Card.IsCode(CardId.ChestOfSouls, CardId.LightningRevival, CardId.CurseOfThorn))
                return Bot.GetSpellCountWithoutField() < 4;
            return false;
        }

        bool GenericActivate()
        {
            // Any optional effect we have no specific rule for (searches, floats, counters): use it,
            // but never chain onto our own chain link unless something was activated by the opponent.
            if (Executors.Any(e => e.Type == ExecutorType.Activate && e.CardId == Card.Id)) return false;
            if (Duel.LastChainPlayer == 0 && Duel.CurrentChain.Count > 0 && Card.Location == CardLocation.Hand) return false;
            return true;
        }

        // ---------- selection overrides ----------

        public override IList<ClientCard> OnSelectCard(IList<ClientCard> cards, int min, int max, long hint, bool cancelable)
        {
            if (cards.Count == 0) return null;
            if (hint == HintMsg.FusionMaterial || hint == HintMsg.SynchroMaterial || hint == HintMsg.XyzMaterial
                || hint == HintMsg.LinkMaterial)
                return null;
            List<ClientCard> chosen = new List<ClientCard>();
            if (picks.Count > 0)
            {
                var pick = picks[0];
                picks.RemoveAt(0);
                chosen = pick(cards).Take(System.Math.Max(max, 1)).ToList();
                if (chosen.Count >= min && chosen.Count > 0) return chosen;
            }
            IEnumerable<ClientCard> ordered = null;
            bool enemyFirst = hint == HintMsg.Destroy || hint == HintMsg.Remove || hint == HintMsg.Control
                || hint == HintMsg.PosChange || hint == HintMsg.ReturnToHand || hint == HintMsg.Equip
                || hint == HintMsg.Facedown || hint == HintMsg.Target;
            if (enemyFirst)
                ordered = cards.OrderBy(c => c.Controller == 0 ? 1 : 0).ThenByDescending(CardValue);
            else if (hint == HintMsg.AddToHand)
                ordered = cards.OrderBy(c => Rank(SearchPriority, c));
            else if (hint == HintMsg.SpSummon)
                ordered = cards.OrderBy(c => Rank(RevivePriority, c)).ThenByDescending(c => c.Attack);
            else if (hint == HintMsg.Set)
                ordered = cards.OrderBy(c => Rank(TrapSetPriority, c));
            else if (hint == HintMsg.Release || hint == HintMsg.Tribute || hint == HintMsg.Discard || hint == HintMsg.ToDeck)
                ordered = cards.OrderBy(c => c.Controller == 0 ? 0 : 1).ThenBy(CardValue);
            else if (hint == HintMsg.ToGrave)
                ordered = cards.OrderBy(c => Rank(ShowdownSendPriority, c)).ThenBy(CardValue);
            if (ordered == null) ordered = cards;
            var picked = chosen.Concat(ordered.Where(c => !chosen.Contains(c))).Take(System.Math.Max(min, 1)).ToList();
            return picked.Count >= min ? picked : null;
        }

        static int Rank(int[] order, ClientCard c)
        {
            int i = System.Array.IndexOf(order, c.Id);
            return i < 0 ? order.Length : i;
        }

        static int CardValue(ClientCard c)
        {
            if (c.IsMonster()) return c.IsFaceup() ? System.Math.Max(c.Attack, c.Defense) : 1500;
            return c.IsFaceup() ? 1000 : 1200;
        }

        public override bool OnSelectYesNo(long desc)
        {
            // The opponent's Messenger asks us whether to pay 1000 LP to save 2 cards
            if (desc == Util.GetStringId(CardId.Messenger, 1))
                return Bot.LifePoints > 3000;
            return true;
        }

        public override int OnSelectOption(IList<long> options)
        {
            // Opponent's Brittle Blessing: we choose for them - give LP instead of a monster
            int lp = options.IndexOf(Util.GetStringId(CardId.BrittleBlessing, 1));
            if (lp >= 0) return lp;
            // Doll Store: Special Summon the monster
            int ss = options.IndexOf(Util.GetStringId(CardId.DollStore, 0));
            if (ss >= 0) return ss;
            // Showdown: counters if we have something to put them on, otherwise recycle a banished card
            int ct = options.IndexOf(Util.GetStringId(CardId.Showdown, 2));
            if (ct >= 0) return Bot.SpellZone[5] != null ? ct : options.Count - 1;
            return base.OnSelectOption(options);
        }

        public override IList<ClientCard> OnSelectSum(IList<ClientCard> cards, int sum, int min, int max, long hint, bool mode)
        {
            if (hint != HintMsg.Release) return null;
            // preference: Hallows Guard (search when it hits the GY) > GY (banished by Damballa) > dead hand cards > field
            var order = cards.OrderBy(c => c.IsCode(CardId.HallowsGuard) ? 0 : c.Location == CardLocation.Grave ? 1
                    : c.Location == CardLocation.Hand ? (IsSlasher(c) ? 3 : 2) : 4)
                .ThenBy(c => c.Location == CardLocation.Grave ? -c.Level : CardValue(c)).ToList();
            var chosen = new List<ClientCard>();
            foreach (var c in order)
            {
                if (chosen.Sum(x => x.Level) >= sum) break;
                chosen.Add(c);
            }
            if (chosen.Sum(x => x.Level) < sum) return null;
            // drop anything not needed to reach the total (the engine rejects extra materials)
            foreach (var c in chosen.OrderBy(x => x.Level).ToList())
                if (chosen.Sum(x => x.Level) - c.Level >= sum) chosen.Remove(c);
            return chosen;
        }

        public override bool OnSelectHand()
        {
            // Trap-heavy deck: prefer going second unless the hand is set up to go first
            return false;
        }
    }
}
