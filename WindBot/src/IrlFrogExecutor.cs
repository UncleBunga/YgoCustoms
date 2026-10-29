using System.Collections.Generic;
using System.Linq;
using YGOSharp.OCGWrapper.Enums;

namespace WindBot.Game.AI.Decks
{
    // IRL Frog: Wetlands / Gravity Bind / Aquarium Stage lock with a Level 2 Frog swarm into Toadally Awesome.
    // One-card line: Rescue Rabbit > 2x Slime Toad > Toadally Awesome. Swap Frog fills the GY for Ronintoadin and
    // Treeborn Frog; Dupe Frog searches when it leaves; Smalls brings back banished Frogs.
    [Deck("IRL_Frog", "AI_IRL_Frog")]
    public class IrlFrogExecutor : CustomExecutorBase
    {
        public class CardId
        {
            public const int SlimeToad = 68638985;
            public const int SmallsGuardian = 91900002;
            public const int RescueRabbit = 85138716;
            public const int Cryomancer = 23950192;
            public const int Dewdark = 90311614;
            public const int SubmarineFrog = 63948258;
            public const int SwapFrog = 9126351;
            public const int Prior = 50088247;
            public const int Smalls = 91900001;
            public const int FlipFlopFrog = 81278754;
            public const int Unifrog = 56052205;
            public const int Ronintoadin = 1357146;
            public const int DupeFrog = 46239604;
            public const int Substitoad = 20663556;
            public const int Tradetoad = 23408872;
            public const int TreebornFrog = 12538374;
            public const int Surface = 33057951;
            public const int Medallion = 84206435;
            public const int FieldBarrier = 7153114;
            public const int AquariumStage = 29047353;
            public const int Wetlands = 2084239;
            public const int FroggyForcefield = 34351849;
            public const int RainOfFrogs = 94467314;
            public const int ToadallyAwesome = 90809975;
            public const int CatShark = 84224627;
            public const int Lambchop = 91900013;
        }

        const int SetFrog = 0x12;

        public IrlFrogExecutor(GameAI ai, Duel duel)
            : base(ai, duel)
        {
            // Responses
            AddExecutor(ExecutorType.Activate, CardId.ToadallyAwesome, ToadallyAwesomeEffect);
            AddExecutor(ExecutorType.Activate, CardId.FroggyForcefield, () => Enemy.GetMonsters().Any(m => m.IsAttack()));
            AddExecutor(ExecutorType.Activate, CardId.SmallsGuardian, SmallsGuardianEffect);
            AddExecutor(ExecutorType.Activate, CardId.RainOfFrogs, RainOfFrogsEffect);
            // Gravity Bind locks out Level 4+ attackers (Toadally Awesome is Rank 2, unaffected): flip it up on their turn
            AddExecutor(ExecutorType.Activate, Staple.GravityBind, () => !MyTurn && !Bot.HasInSpellZone(Staple.GravityBind, true));

            // Field first: Wetlands, then its protection
            AddExecutor(ExecutorType.Activate, CardId.Wetlands, FieldSpell);
            AddExecutor(ExecutorType.Activate, CardId.FieldBarrier, () => Bot.SpellZone[5] != null && !Bot.HasInSpellZone(CardId.FieldBarrier, true));
            AddExecutor(ExecutorType.Activate, CardId.AquariumStage, AquariumStageEffect);
            AddExecutor(ExecutorType.Activate, CardId.Medallion, MedallionEffect);
            AddExecutor(ExecutorType.Activate, CardId.Lambchop, LambchopEffect);
            AddStaples();

            // Extenders
            AddExecutor(ExecutorType.Summon, CardId.RescueRabbit, RescueRabbitSummon);
            AddExecutor(ExecutorType.Activate, CardId.RescueRabbit, () => { Pick(CardId.SlimeToad); Pick(CardId.SlimeToad); return true; });
            AddExecutor(ExecutorType.SpSummon, CardId.SwapFrog, SwapFrogSpSummon);
            AddExecutor(ExecutorType.Activate, CardId.SwapFrog, SwapFrogEffect);
            AddExecutor(ExecutorType.Activate, CardId.Ronintoadin, () => { AI.SelectPosition(CardPosition.FaceUpDefence); return true; });
            AddExecutor(ExecutorType.Activate, CardId.TreebornFrog);
            AddExecutor(ExecutorType.Activate, CardId.DupeFrog, () => { Pick(AddToHandPriority); return true; });
            AddExecutor(ExecutorType.Activate, CardId.Smalls, SmallsEffect);
            AddExecutor(ExecutorType.SpSummon, CardId.SmallsGuardian);
            AddExecutor(ExecutorType.Activate, CardId.Tradetoad, () => { Pick(CardId.SwapFrog, CardId.DupeFrog, CardId.Ronintoadin); return true; });
            AddExecutor(ExecutorType.Activate, CardId.Substitoad, SubstitoadEffect);
            AddExecutor(ExecutorType.Summon, CardId.Substitoad, () => !Bot.HasInMonstersZone(CardId.Substitoad));
            AddExecutor(ExecutorType.SpSummon, CardId.Prior);
            AddExecutor(ExecutorType.Activate, CardId.Prior);
            AddExecutor(ExecutorType.Activate, CardId.Surface, () => ReviveBest(false));
            AddExecutor(ExecutorType.Activate, CardId.FlipFlopFrog, () => Enemy.GetMonsterCount() > 0);
            AddExecutor(ExecutorType.Activate, CardId.Unifrog, () => TargetEnemySpell());

            // Xyz
            AddExecutor(ExecutorType.SpSummon, CardId.ToadallyAwesome, ToadallySummon);
            AddExecutor(ExecutorType.SpSummon, CardId.CatShark, CatSharkSummon);
            AddExecutor(ExecutorType.Activate, CardId.CatShark, () => Bot.HasInMonstersZone(CardId.ToadallyAwesome, true));

            // Normal Summons: whatever keeps the Frog engine going, then attackers under Wetlands
            AddExecutor(ExecutorType.Summon, CardId.DupeFrog);
            AddExecutor(ExecutorType.Summon, CardId.SwapFrog);
            AddExecutor(ExecutorType.Summon, CardId.Ronintoadin, LevelTwoPartner);
            AddExecutor(ExecutorType.Summon, CardId.Unifrog);
            AddExecutor(ExecutorType.Summon, CardId.SubmarineFrog);
            AddExecutor(ExecutorType.Summon, CardId.Dewdark, LevelTwoPartner);
            AddExecutor(ExecutorType.Summon, CardId.Cryomancer, LevelTwoPartner);
            AddExecutor(ExecutorType.Summon, CardId.Tradetoad);
            AddExecutor(ExecutorType.Summon, CardId.Substitoad);
            AddExecutor(ExecutorType.MonsterSet, CardId.FlipFlopFrog);
            AddExecutor(ExecutorType.SummonOrSet, DefaultMonsterSummon);

            AddExecutor(ExecutorType.SpellSet, () => SetTrapOrQuickPlay() || Card.IsCode(Staple.GravityBind));
            AddExecutor(ExecutorType.Repos, FrogRepos);
            AddExecutor(ExecutorType.Activate, GenericActivate);
        }

        protected override int[] AddToHandPriority => new[] {
            CardId.Wetlands, CardId.SwapFrog, CardId.RescueRabbit, CardId.DupeFrog, CardId.Smalls, CardId.Unifrog,
            CardId.Ronintoadin, CardId.FlipFlopFrog, CardId.Substitoad, CardId.TreebornFrog, CardId.Prior, CardId.AquariumStage,
            Staple.GravityBind, CardId.FieldBarrier
        };
        protected override int[] SpSummonPriority => new[] {
            CardId.ToadallyAwesome, CardId.SmallsGuardian, CardId.SwapFrog, CardId.DupeFrog, CardId.Unifrog, CardId.Ronintoadin,
            CardId.Smalls, CardId.SlimeToad, CardId.SubmarineFrog, CardId.FlipFlopFrog
        };
        protected override int[] ExpendablePriority => new[] {
            CardId.TreebornFrog, CardId.Ronintoadin, CardId.DupeFrog, CardId.SlimeToad, CardId.Tradetoad, CardId.Substitoad, CardId.Cryomancer,
            CardId.Dewdark, CardId.SubmarineFrog, CardId.FlipFlopFrog
        };

        bool IsFrog(ClientCard c) => c != null && c.HasSetcode(SetFrog);
        bool IsAquaLv2(ClientCard c) => c.IsFaceup() && c.Level == 2 && c.HasRace(CardRace.Aqua) && !c.HasType(CardType.Xyz | CardType.Link);

        bool FieldSpell()
        {
            if (Card.Location == CardLocation.SpellZone && Card.IsFaceup()) return false;
            return Bot.SpellZone[5] == null || !Bot.SpellZone[5].IsCode(CardId.Wetlands);
        }

        bool AquariumStageEffect()
        {
            if (Card.Location == CardLocation.Grave)
            {
                AI.SelectPosition(CardPosition.FaceUpDefence);
                Pick(SpSummonPriority);
                return true;
            }
            return !Bot.HasInSpellZone(CardId.AquariumStage, true);
        }

        bool MedallionEffect()
        {
            if (Bot.HasInHandOrHasInMonstersZone(new[] { CardId.Cryomancer, CardId.Dewdark })) Pick(CardId.Prior, CardId.Dewdark);
            else Pick(CardId.Dewdark, CardId.Cryomancer, CardId.Prior);
            return true;
        }

        bool RescueRabbitSummon()
        {
            return MaybeInDeck(CardId.SlimeToad, 3) && Seen(CardId.SlimeToad) <= 1;
        }

        bool SwapFrogSpSummon()
        {
            // discard a WATER monster that wants to be in the GY (never Smalls - it works from the hand)
            var discard = Bot.Hand.Where(c => c.IsMonster() && c.HasAttribute(CardAttribute.Water) && !c.IsCode(CardId.SwapFrog))
                .OrderBy(c => Rank(new[] { CardId.TreebornFrog, CardId.Ronintoadin, CardId.SlimeToad, CardId.Tradetoad, CardId.DupeFrog,
                    CardId.FlipFlopFrog, CardId.SubmarineFrog, CardId.Cryomancer, CardId.Dewdark, CardId.Unifrog, CardId.Substitoad }, c)).FirstOrDefault();
            if (discard == null) return false;
            Pick(discard);
            return true;
        }

        bool SwapFrogEffect()
        {
            if (Card.Location == CardLocation.MonsterZone && (GenericTrigger || ActivateDescription == Util.GetStringId(CardId.SwapFrog, 0)))
            {
                // On Summon: always mill Ronintoadin while one is left, it climbs back out of the GY
                Pick(CardId.Ronintoadin, CardId.TreebornFrog, CardId.DupeFrog, CardId.SlimeToad);
                return true;
            }
            // Bounce Swap Frog to hand: re-Summon it (discarding another WATER) to mill again, and gain an extra Frog Normal Summon
            bool canResummon = Bot.Hand.Count(c => c.IsMonster() && c.HasAttribute(CardAttribute.Water) && !c.IsCode(CardId.SwapFrog)) >= 1
                && MaybeInDeck(CardId.Ronintoadin, 3);
            bool extraFrog = Bot.Hand.Any(c => IsFrog(c) && c.Level <= 2 && !c.IsCode(CardId.SwapFrog));
            if (!canResummon && !extraFrog) return false;
            Pick(Bot.GetMonsters().Where(m => m.IsCode(CardId.SwapFrog)));
            return true;
        }

        bool SmallsEffect()
        {
            // discard: Special Summon a banished Frog/Toad
            var target = Bot.Banished.Where(c => c.IsMonster() && (IsFrog(c) || c.Name != null && c.Name.Contains("Toad")))
                .OrderBy(c => Rank(SpSummonPriority, c)).FirstOrDefault();
            if (target == null) return false;
            Pick(target);
            return true;
        }

        // Frogs left in the Deck, by what we have already seen (deck list counts)
        static readonly Dictionary<int, int> FrogCopies = new Dictionary<int, int> {
            { CardId.DupeFrog, 3 }, { CardId.SwapFrog, 3 }, { CardId.Ronintoadin, 3 }, { CardId.TreebornFrog, 2 },
            { CardId.Unifrog, 2 }, { CardId.FlipFlopFrog, 2 }, { CardId.SubmarineFrog, 1 }, { CardId.Tradetoad, 2 }
        };
        bool AnyFrogInDeck => FrogCopies.Any(kv => MaybeInDeck(kv.Key, kv.Value));

        bool SubstitoadEffect()
        {
            // Loop: tribute anything except Xyz / Smalls / Substitoad, Special Summon a Frog from the Deck. Dupe Frog
            // searches when it is tributed; everything else fuels Ronintoadin and Toadally Awesome's recursion.
            if (!AnyFrogInDeck) return false;
            var fodder = Bot.GetMonsters().Where(m => !m.IsCode(CardId.Substitoad, CardId.Smalls, CardId.SmallsGuardian)
                    && !m.HasType(CardType.Xyz | CardType.Link) && m.Attack < 2000)
                .OrderBy(m => m.IsCode(CardId.DupeFrog) ? 0 : 1).ThenBy(m => Rank(ExpendablePriority, m)).ThenBy(CardValue).FirstOrDefault();
            if (fodder == null)
            {
                // nothing else to tribute: Substitoad can tribute itself into Swap Frog, which mills Ronintoadin
                var self = Bot.GetMonsters().FirstOrDefault(m => m.IsCode(CardId.Substitoad));
                if (self == null || !MaybeInDeck(CardId.SwapFrog, 3)) return false;
                Pick(self);
                Pick(CardId.SwapFrog, CardId.DupeFrog, CardId.Ronintoadin);
                return true;
            }
            Pick(fodder);
            Pick(CardId.DupeFrog, CardId.SwapFrog, CardId.Ronintoadin, CardId.TreebornFrog, CardId.Unifrog, CardId.FlipFlopFrog,
                CardId.SubmarineFrog, CardId.Tradetoad);
            return true;
        }

        bool LambchopEffect()
        {
            if (Card.Location == CardLocation.Grave)
                return Bot.Hand.Count(c => CardValue(c) < 1000 || c.IsCode(CardId.SlimeToad)) >= 2;
            if (!MaybeInDeck(CardId.Smalls, 2) && !MaybeInDeck(CardId.SmallsGuardian, 1)) return false;
            // the banished Aqua becomes a target for Smalls
            var cost = Bot.Hand.Concat(Bot.GetMonsters().Where(m => m.IsFaceup()))
                .Where(c => c.HasRace(CardRace.Aqua) && !c.HasType(CardType.Xyz))
                .OrderBy(c => Rank(ExpendablePriority, c)).ThenBy(CardValue).FirstOrDefault();
            if (cost == null) return false;
            Pick(cost);
            Pick(Bot.HasInHand(CardId.Smalls) ? new[] { CardId.SmallsGuardian, CardId.Smalls } : new[] { CardId.Smalls, CardId.SmallsGuardian });
            return true;
        }

        bool SmallsGuardianEffect()
        {
            // make one of our monsters Level 2 Aqua when that completes Toadally Awesome
            if (!Bot.HasInExtra(CardId.ToadallyAwesome)) return false;
            int ready = Bot.GetMonsters().Count(IsAquaLv2);
            if (ready != 1) return false;
            var t = Bot.GetMonsters().Where(m => m.IsFaceup() && !IsAquaLv2(m) && !m.HasType(CardType.Xyz | CardType.Link))
                .OrderBy(CardValue).FirstOrDefault();
            if (t == null) return false;
            Pick(t);
            return true;
        }

        bool RainOfFrogsEffect()
        {
            // send every non-Aqua monster to the GY: worth it when they lose more than we do
            int theirs = Enemy.GetMonsters().Where(m => !m.HasRace(CardRace.Aqua)).Sum(m => CardValue(m) + 500);
            int ours = Bot.GetMonsters().Where(m => !m.HasRace(CardRace.Aqua)).Sum(m => CardValue(m) + 500);
            return theirs > ours && theirs >= 2000;
        }

        bool TargetEnemySpell()
        {
            var s = Util.GetBestEnemySpell();
            if (s != null) Pick(s);
            return true;
        }

        bool LevelTwoPartner()
        {
            return Bot.GetMonsters().Any(IsAquaLv2);
        }

        bool ToadallySummon()
        {
            var mats = Bot.GetMonsters().Where(IsAquaLv2).OrderBy(m => m.IsCode(CardId.Unifrog) ? 1 : 0).ThenBy(CardValue).Take(2).ToList();
            if (mats.Count < 2) return false;
            AI.SelectMaterials(mats);
            return true;
        }

        bool CatSharkSummon()
        {
            var mats = Bot.GetMonsters().Where(m => m.IsFaceup() && m.Level == 2 && !m.HasType(CardType.Xyz | CardType.Link))
                .OrderBy(CardValue).Take(2).ToList();
            if (mats.Count < 2 || !Bot.HasInMonstersZone(CardId.ToadallyAwesome)) return false;
            AI.SelectMaterials(mats);
            return true;
        }

        bool ToadallyAwesomeEffect()
        {
            if (Duel.CurrentChain.Count > 0)
            {
                // negate: pay with the least useful Aqua
                if (Duel.LastChainPlayer != 1) return false;
                var cost = Bot.Hand.Concat(Bot.GetMonsters()).Where(c => c.HasRace(CardRace.Aqua) && !c.IsCode(CardId.ToadallyAwesome))
                    .OrderBy(c => Rank(ExpendablePriority, c)).ThenBy(CardValue).FirstOrDefault();
                if (cost == null) return false;
                Pick(cost);
                return true;
            }
            // Standby send / revive when destroyed
            Pick(CardId.SwapFrog, CardId.Ronintoadin, CardId.TreebornFrog, CardId.DupeFrog, CardId.ToadallyAwesome);
            return true;
        }

        bool FrogRepos()
        {
            // Gravity Bind keeps big monsters off us; Level 2 Frogs attack under Wetlands
            if (Card.IsFacedown()) return Card.IsCode(CardId.FlipFlopFrog) && Enemy.GetMonsterCount() > 0;
            return DefaultMonsterRepos();
        }

        public override bool OnSelectHand()
        {
            return true; // go first and set up the lock
        }
    }
}
