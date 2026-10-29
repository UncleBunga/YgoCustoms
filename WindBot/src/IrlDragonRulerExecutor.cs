using System.Collections.Generic;
using System.Linq;
using YGOSharp.OCGWrapper.Enums;

namespace WindBot.Game.AI.Decks
{
    // IRL Dragon Ruler (the user's plan):
    //  - Babies (Burner/Lightning/Stream/Reactan) and Spectral/Nebulus put the Level 7 Rulers on the field; two Level 7
    //    Dragons become Rank 7 Rulers - Eclipse (DARK) and Chasma (LIGHT) first.
    //  - Only make Disaster, Dragon Ruler of All Apocalypses when Chasma and Eclipse are both on the field and the other
    //    four Rulers (Blaster, Tidal, Tempest, Redox) are in the GY, banished, or attached to them: all six Attributes
    //    give Disaster +4600 and immunity and banish matching monsters from the opponent's field and GY.
    //  - Then World Legacy Guardragon / Guardragon Elpy / Agarpain fill the field; Agarpain brings Galaxy-Eyes Tachyon
    //    Dragon, Dark Matter Dragon is Xyz Summoned on it to send Arkbrave and Amorphage Goliath to the GY, and Arkbrave
    //    revives Goliath in the next Standby Phase to lock the opponent's Extra Deck.
    [Deck("IRL_DragonRuler", "AI_IRL_DragonRuler")]
    public class IrlDragonRulerExecutor : CustomExecutorBase
    {
        public class CardId
        {
            public const int REDMD = 88264988;
            public const int DarkHoleDragon = 72880377;
            public const int AmorphageGoliath = 69072185;
            public const int LightEndSublimation = 35614780;
            public const int DarkEndEvaporation = 62002838;
            public const int MythicWater = 92841002;
            public const int Blaster = 53804307;
            public const int PileArmed = 19153590;
            public const int Tidal = 26400609;
            public const int Tempest = 89399912;
            public const int Arkbrave = 33282498;
            public const int Redox = 90411554;
            public const int Noctovision = 70333910;
            public const int FistArmed = 49181828;
            public const int Reactan = 91020571;
            public const int Seyfert = 15381421;
            public const int Stream = 27415516;
            public const int EclipseWyvern = 51858306;
            public const int MythicTree = 50457953;
            public const int KnightArmed = 98007437;
            public const int Defrag = 58582979;
            public const int Burner = 53797637;
            public const int Lightning = 89185742;
            public const int Nebulus = 94655777;
            public const int Spectral = 58201062;
            public const int LaundryDragonmaid = 13171876;
            public const int Brotaur = 8972398;
            public const int ReturnOfDragonLords = 6853254;
            public const int GrassLooksGreener = 11110587;
            public const int HereThereBeDragons = 4575541;
            public const int BurialDifferentDimension = 48976825;
            public const int WorldLegacyGuardragon = 87571563;
            public const int DragonRavine = 62265044;
            public const int StormDragonsReturn = 83195035;

            public const int FiveHeadedDragon = 99267150;
            public const int LightDarknessDragonlord = 19652159;
            public const int LightEndDragon = 25132288;
            public const int DarkMatter = 58820923;
            public const int Tachyon = 88177324;
            public const int OddEyesRebellion = 45627618;
            public const int RebellionOverlord = 98452268;
            public const int Chasma = 4965193;
            public const int Blaze = 47132793;
            public const int Eclipse = 30350202;
            public const int Voloferniges = 38694052;
            public const int Disaster = 67359907;
            public const int FiveHeadedLink = 10669138;
            public const int Borrelend = 98630720;
            public const int Borrelsword = 85289965;
            public const int Saryuja = 74997493;
            public const int Agarpain = 59537380;
            public const int HieraticSeal = 24361622;
            public const int Pisty = 13143275;
            public const int StrikerDragon = 73539069;
            public const int Elpy = 86148577;
        }

        static readonly int[] ElementRulers = { CardId.Blaster, CardId.Tidal, CardId.Tempest, CardId.Redox };
        static readonly int[] BigRulers = { CardId.Blaster, CardId.Tidal, CardId.Tempest, CardId.Redox };
        static readonly int[] BabyRulers = { CardId.Burner, CardId.Lightning, CardId.Stream, CardId.Reactan };

        public IrlDragonRulerExecutor(GameAI ai, Duel duel)
            : base(ai, duel)
        {
            // Responses / quick effects
            AddExecutor(ExecutorType.Activate, CardId.Eclipse, EclipseEffect);
            AddExecutor(ExecutorType.Activate, CardId.LightDarknessDragonlord, () => Duel.LastChainPlayer == 1 || Card.Location != CardLocation.MonsterZone);
            AddExecutor(ExecutorType.Activate, CardId.Chasma, ChasmaEffect);
            AddExecutor(ExecutorType.Activate, CardId.Noctovision, () => Duel.LastChainPlayer == 1 || Card.Location == CardLocation.Hand);
            AddExecutor(ExecutorType.Activate, CardId.Disaster, DisasterEffect);
            AddExecutor(ExecutorType.Activate, CardId.Arkbrave, () => PickThen(CardId.AmorphageGoliath, CardId.DarkHoleDragon, CardId.LightEndSublimation));
            AddExecutor(ExecutorType.Activate, CardId.DarkMatter, DarkMatterEffect);
            AddExecutor(ExecutorType.Activate, CardId.Brotaur, () => PickThen(CardId.Spectral, CardId.Tempest));
            AddExecutor(ExecutorType.Activate, CardId.DarkHoleDragon);
            AddStaples();

            // Searches and mill
            AddExecutor(ExecutorType.Activate, CardId.GrassLooksGreener);
            AddExecutor(ExecutorType.Activate, CardId.Spectral, SpectralEffect);
            AddExecutor(ExecutorType.Activate, CardId.Seyfert, () => PickThen(CardId.Tempest, CardId.Blaster));
            AddExecutor(ExecutorType.Activate, CardId.LaundryDragonmaid);
            AddExecutor(ExecutorType.Activate, CardId.DragonRavine, () => { Pick(Bot.Hand.OrderBy(c => Rank(ExpendablePriority, c)).Take(1)); Pick(CardId.Tidal, CardId.Blaster, CardId.Redox); return true; });

            // Babies summon the Level 7 Rulers
            AddExecutor(ExecutorType.Activate, CardId.Burner, () => BabyEffect(CardId.Blaster));
            AddExecutor(ExecutorType.Activate, CardId.Lightning, () => BabyEffect(CardId.Tempest));
            AddExecutor(ExecutorType.Activate, CardId.Stream, () => BabyEffect(CardId.Tidal));
            AddExecutor(ExecutorType.Activate, CardId.Reactan, () => BabyEffect(CardId.Redox));
            AddExecutor(ExecutorType.Activate, CardId.Nebulus, NebulusEffect);
            AddExecutor(ExecutorType.Activate, CardId.HereThereBeDragons, HereThereBeDragonsEffect);
            AddExecutor(ExecutorType.Activate, CardId.Blaster, () => BigRulerEffect(CardAttribute.Fire));
            AddExecutor(ExecutorType.Activate, CardId.Tidal, () => BigRulerEffect(CardAttribute.Water));
            AddExecutor(ExecutorType.Activate, CardId.Tempest, () => BigRulerEffect(CardAttribute.Wind));
            AddExecutor(ExecutorType.Activate, CardId.Redox, () => BigRulerEffect(CardAttribute.Earth));
            AddExecutor(ExecutorType.SpSummon, CardId.REDMD, REDMDSummon);
            AddExecutor(ExecutorType.Activate, CardId.REDMD, () => PickThen(BigRulers.Concat(new[] { CardId.DarkHoleDragon, CardId.MythicWater }).ToArray()));
            AddExecutor(ExecutorType.SpSummon, CardId.MythicWater);
            AddExecutor(ExecutorType.Activate, CardId.ReturnOfDragonLords, () => PickThen(BigRulers.Concat(new[] { CardId.DarkHoleDragon }).ToArray()));
            AddExecutor(ExecutorType.Activate, CardId.BurialDifferentDimension, BurialEffect);
            AddExecutor(ExecutorType.Activate, CardId.StormDragonsReturn, () => MyTurn && PickThen(BigRulers));

            // Rank 7 and the calamity
            AddExecutor(ExecutorType.SpSummon, CardId.Disaster, DisasterSummon);
            AddExecutor(ExecutorType.SpSummon, CardId.Eclipse, () => Rank7Summon(CardId.Eclipse));
            AddExecutor(ExecutorType.SpSummon, CardId.Chasma, () => Rank7Summon(CardId.Chasma));
            AddExecutor(ExecutorType.SpSummon, CardId.Blaze, () => Rank7Summon(CardId.Blaze));
            AddExecutor(ExecutorType.SpSummon, CardId.Voloferniges, () => Rank7Summon(CardId.Voloferniges));
            AddExecutor(ExecutorType.Activate, CardId.Blaze, () => Card.Location == CardLocation.Grave || TargetEnemyCard());
            AddExecutor(ExecutorType.Activate, CardId.Voloferniges, () => TargetEnemyCard());

            // Guardragon package: fill the field, Tachyon into Dark Matter
            AddExecutor(ExecutorType.Activate, CardId.WorldLegacyGuardragon, WorldLegacyEffect);
            AddExecutor(ExecutorType.SpSummon, CardId.Elpy, () => LinkOneSummon());
            AddExecutor(ExecutorType.SpSummon, CardId.Pisty, () => Bot.HasInMonstersZone(CardId.Elpy) && LinkOneSummon());
            AddExecutor(ExecutorType.SpSummon, CardId.Agarpain, AgarpainSummon);
            AddExecutor(ExecutorType.Activate, CardId.Elpy, () => PickThen(BigRulers.Concat(new[] { CardId.DarkHoleDragon, CardId.REDMD }).ToArray()));
            AddExecutor(ExecutorType.Activate, CardId.Pisty, () => PickThen(BigRulers.Concat(new[] { CardId.REDMD, CardId.DarkHoleDragon }).ToArray()));
            AddExecutor(ExecutorType.Activate, CardId.Agarpain, () => PickThen(CardId.Tachyon, CardId.Chasma, CardId.Eclipse));
            AddExecutor(ExecutorType.SpSummon, CardId.DarkMatter, () => Bot.HasInMonstersZone(CardId.Tachyon));
            AddExecutor(ExecutorType.Activate, CardId.Tachyon, () => MyTurn && Duel.Phase == DuelPhase.BattleStart);
            AddExecutor(ExecutorType.SpSummon, CardId.Tachyon, () => Bot.GetMonsters().Count(m => m.IsFaceup() && m.Level == 8) >= 2);

            // End dragons and fusion
            AddExecutor(ExecutorType.SpSummon, CardId.DarkEndEvaporation);
            AddExecutor(ExecutorType.Activate, CardId.DarkEndEvaporation, () => TargetEnemyMonster());
            AddExecutor(ExecutorType.SpSummon, CardId.LightEndSublimation);
            AddExecutor(ExecutorType.Activate, CardId.LightEndSublimation, () => !MyTurn);

            AddExecutor(ExecutorType.Summon, CardId.Seyfert, NormalSummon);
            AddExecutor(ExecutorType.Summon, CardId.LaundryDragonmaid, NormalSummon);
            AddExecutor(ExecutorType.Summon, CardId.EclipseWyvern, NormalSummon);
            AddExecutor(ExecutorType.SummonOrSet, () => BestFallbackSummon() && NormalSummon());
            AddExecutor(ExecutorType.SpellSet, SetTrapOrQuickPlay);
            AddExecutor(ExecutorType.Repos, DefaultMonsterRepos);
            AddExecutor(ExecutorType.Activate, GenericActivate);
        }

        protected override int[] AddToHandPriority => new[] {
            CardId.Tempest, CardId.Blaster, CardId.Tidal, CardId.Redox, CardId.Burner, CardId.Lightning, CardId.Stream,
            CardId.Reactan, CardId.Spectral, CardId.Nebulus, CardId.HereThereBeDragons, CardId.REDMD
        };
        protected override int[] SpSummonPriority => new[] {
            CardId.Disaster, CardId.Eclipse, CardId.Chasma, CardId.AmorphageGoliath, CardId.Blaster, CardId.Tempest, CardId.Tidal,
            CardId.Redox, CardId.DarkHoleDragon, CardId.REDMD
        };
        protected override int[] ExpendablePriority => new[] {
            CardId.EclipseWyvern, CardId.Defrag, CardId.MythicTree, CardId.LaundryDragonmaid, CardId.KnightArmed, CardId.FistArmed,
            CardId.PileArmed, CardId.Burner, CardId.Lightning, CardId.Stream, CardId.Reactan
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

        IEnumerable<ClientCard> MyDragonsField => Bot.GetMonsters().Where(m => m.IsFaceup() && m.HasRace(CardRace.Dragon));
        bool RulerAvailable(int id) => Bot.Graveyard.Any(c => c.IsCode(id)) || Bot.Banished.Any(c => c.IsCode(id))
            || Bot.GetMonsters().Any(m => m.HasType(CardType.Xyz) && m.Overlays.Contains(id));

        // ---------- engine ----------

        bool SpectralEffect()
        {
            if (Card.Location == CardLocation.Grave) return Bot.HasInGraveyard(CardId.Chasma);
            // discard Spectral + a Dragon/LIGHT: add 2 Rulers - the ones we don't have yet
            Pick(Bot.Hand.Where(c => c != Card && c.IsMonster() && (c.HasRace(CardRace.Dragon) || c.HasAttribute(CardAttribute.Light)))
                .OrderBy(c => Rank(ExpendablePriority, c)).ThenBy(CardValue).Take(1));
            Pick(ElementRulers.Where(id => !Bot.HasInHand(id)).Concat(BabyRulers).ToArray());
            return true;
        }

        bool BabyEffect(int bigRuler)
        {
            if (!MaybeInDeck(bigRuler, 2)) return false;
            // second discard: another baby or a dead Dragon
            Pick(Bot.Hand.Where(c => c != Card && c.IsMonster() && c.HasRace(CardRace.Dragon))
                .OrderBy(c => Rank(ExpendablePriority, c)).ThenBy(CardValue).Take(1));
            return true;
        }

        bool BigRulerEffect(CardAttribute attr)
        {
            if (Card.Location == CardLocation.Removed) return true; // banished: search
            if (Card.Location == CardLocation.Hand || Card.Location == CardLocation.Grave)
            {
                // Special Summon by banishing 2 Dragons/attribute: banish babies first (their banish has no cost to us)
                var pool = Bot.Hand.Concat(Bot.Graveyard).Where(c => c != Card && c.IsMonster() && (c.HasRace(CardRace.Dragon) || c.HasAttribute(attr)))
                    .OrderBy(c => c.IsCode(BabyRulers) ? 0 : c.IsCode(BigRulers) ? 2 : 1).ThenBy(c => Rank(ExpendablePriority, c)).ToList();
                if (pool.Count < 2) return false;
                Pick(pool.Take(2));
                return true;
            }
            return false;
        }

        bool NebulusEffect()
        {
            if (Card.Location == CardLocation.Grave) return Bot.HasInGraveyard(CardId.Eclipse);
            var banished = Bot.Banished.Where(c => c.IsCode(BigRulers)).GroupBy(c => c.Attribute).Select(g => g.First()).Take(2).ToList();
            if (banished.Count < 2) return false;
            Pick(Bot.Hand.Where(c => c != Card && c.IsMonster() && (c.HasRace(CardRace.Dragon) || c.HasAttribute(CardAttribute.Dark)))
                .OrderBy(c => Rank(ExpendablePriority, c)).Take(1));
            Pick(banished);
            return true;
        }

        bool HereThereBeDragonsEffect()
        {
            if (Card.Location == CardLocation.Grave) return Bot.Banished.Count(c => c.IsCode(BigRulers)) >= 2;
            Pick(BigRulers);
            return true;
        }

        bool BurialEffect()
        {
            var t = Bot.Banished.Where(c => c.IsCode(BigRulers.Concat(BabyRulers).ToArray())).Take(3).ToList();
            if (t.Count == 0) return false;
            Pick(t);
            return true;
        }

        bool REDMDSummon()
        {
            var cost = MyDragonsField.Where(m => !m.HasType(CardType.Xyz | CardType.Link)).OrderBy(CardValue).FirstOrDefault();
            if (cost == null) return false;
            Pick(cost);
            return true;
        }

        // ---------- Rank 7 / Disaster ----------

        bool Rank7Summon(int id)
        {
            var lv7 = MyDragonsField.Where(m => m.Level == 7 && !m.HasType(CardType.Xyz | CardType.Link)).OrderBy(m => m.IsCode(BigRulers) ? 0 : 1).ToList();
            if (lv7.Count < 2) return false;
            // Eclipse first (its Summon brings another Ruler), then Chasma, so both Light/Dark Rulers reach the field
            if (id != CardId.Eclipse && !Bot.HasInMonstersZone(CardId.Eclipse) && Bot.HasInExtra(CardId.Eclipse)) return false;
            if (id != CardId.Chasma && id != CardId.Eclipse && !Bot.HasInMonstersZone(CardId.Chasma) && Bot.HasInExtra(CardId.Chasma)) return false;
            AI.SelectMaterials(lv7.Take(2).ToList());
            return true;
        }

        bool DisasterReady =>
            Bot.HasInMonstersZone(CardId.Chasma) && Bot.HasInMonstersZone(CardId.Eclipse) && ElementRulers.All(RulerAvailable);

        bool DisasterSummon()
        {
            if (!DisasterReady) return false;
            var mats = Bot.GetMonsters().Where(m => m.IsCode(CardId.Chasma, CardId.Eclipse)).ToList();
            AI.SelectMaterials(mats);
            return true;
        }

        bool DisasterEffect()
        {
            // attach the four element Rulers
            Pick(Bot.Graveyard.Concat(Bot.Banished).Where(c => c.IsCode(ElementRulers)).GroupBy(c => c.Id).Select(g => g.First()));
            return true;
        }

        bool EclipseEffect()
        {
            if (Duel.CurrentChain.Count > 0) return Duel.LastChainPlayer == 1;
            // on Xyz Summon: a baby in GY/banishment brings out its Level 7 Ruler
            var baby = Bot.Graveyard.Concat(Bot.Banished).FirstOrDefault(c => c.IsCode(BabyRulers));
            if (baby == null) return false;
            Pick(baby);
            return true;
        }

        bool ChasmaEffect()
        {
            if (Card.Location != CardLocation.MonsterZone) return true; // destroyed: bring back a Level 7 Ruler
            // send an element Ruler we still need in the GY and use its discard effect
            var need = ElementRulers.Where(id => !RulerAvailable(id) && MaybeInDeck(id, 2)).ToArray();
            if (need.Length == 0) return false;
            Pick(need);
            return Duel.LastChainPlayer == 1 || MyTurn;
        }

        // ---------- Guardragon package ----------

        bool WorldLegacyEffect()
        {
            if (Card.Location == CardLocation.SpellZone && Card.IsFaceup()) return false;
            var t = Bot.Graveyard.Where(c => c.HasRace(CardRace.Dragon) && c.Level <= 4).OrderBy(c => c.IsCode(BabyRulers) ? 0 : 1).FirstOrDefault();
            if (t == null) return false;
            Pick(t);
            return true;
        }

        bool LinkOneSummon()
        {
            var m = MyDragonsField.Where(x => x.Level > 0 && x.Level <= 4 && !x.HasType(CardType.Link | CardType.Xyz)).OrderBy(CardValue).FirstOrDefault();
            if (m == null) return false;
            AI.SelectMaterials(new List<ClientCard> { m });
            return true;
        }

        bool AgarpainSummon()
        {
            // only once the calamity is up (or impossible), to set up Tachyon > Dark Matter
            if (!Bot.HasInExtra(CardId.Tachyon) && !Bot.HasInExtra(CardId.DarkMatter)) return false;
            var mats = MyDragonsField.Where(m => !m.IsCode(CardId.Disaster, CardId.Chasma, CardId.Eclipse) && !m.HasType(CardType.Xyz))
                .OrderBy(CardValue).Take(2).ToList();
            if (mats.Count < 2) return false;
            AI.SelectMaterials(mats);
            return true;
        }

        bool DarkMatterEffect()
        {
            if (Card.Location != CardLocation.MonsterZone) return false;
            if (GenericTrigger || Desc(CardId.DarkMatter, 0))
            {
                Pick(CardId.Arkbrave, CardId.AmorphageGoliath, CardId.DarkHoleDragon, CardId.LightEndSublimation, CardId.MythicWater);
                return true;
            }
            return MyTurn && Duel.Phase == DuelPhase.BattleStart && Enemy.GetMonsterCount() >= 2;
        }

        public override bool OnSelectHand()
        {
            return true;
        }
    }
}
