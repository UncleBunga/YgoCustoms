using System.Collections.Generic;
using YGOSharp.OCGWrapper.Enums;

namespace WindBot.Game.AI.Decks
{
    // IRL HERO. Bread and butter (the user's line):
    //   Vision HERO Faris (discard Neos) > place Vision HERO Increase > Increase tributes Faris > Vision HERO Vyon >
    //   Vyon sends Neos to the GY and banishes a HERO for Polymerization > Polymerization: Flame Wingman - Infernal Rage >
    //   Infernal Rage adds Favorite Contact > Neos + Infernal Rage into Shining Neos Wingman > equip Favorite Hero.
    // Side route: Special Summon Shadow Mist (A Hero Lives, Solid Soldier) > Mask Change > Masked HERO Dark Law.
    [Deck("IRL_Hero", "AI_IRL_Hero")]
    public class IrlHeroExecutor : CustomExecutorBase
    {
        public class CardId
        {
            public const int Neos = 89943723;
            public const int Sparkman = 20721928;
            public const int Clayman = 84327329;
            public const int Burstinatrix = 58932615;
            public const int Avian = 21844576;
            public const int Sabatiel = 40237839;
            public const int Bladedge = 59793705;
            public const int SpiritOfNeos = 13256226;
            public const int HonestNeos = 14124483;
            public const int WingedKuribohLV6 = 48486809;
            public const int Faris = 18094166;
            public const int Necroshade = 89252153;
            public const int NeosAlius = 69884162;
            public const int Stratos = 40044918;
            public const int Heat = 98266377;
            public const int Wildheart = 86188410;
            public const int Ocean = 37195861;
            public const int LiquidSoldier = 59392529;
            public const int SolidSoldier = 45195443;
            public const int Woodsman = 75434695;
            public const int Voltic = 9327502;
            public const int ShadowMist = 50720316;
            public const int Vyon = 27780618;
            public const int Bubbleman = 79979666;
            public const int Increase = 22865492;
            public const int EmergencyCall = 213326;
            public const int AHeroLives = 8949584;
            public const int Polymerization = 24094653;
            public const int MiracleFusion = 45906428;
            public const int ParallelWorldFusion = 54283059;
            public const int OOversoul = 63703130;
            public const int HeatedHeart = 74825788;
            public const int HeroMask = 75141056;
            public const int MaskCharge = 87819421;
            public const int MaskChange = 21143940;
            public const int FormChange = 84536654;
            public const int MaskChangeII = 93600443;
            public const int FavoriteHero = 11881272;
            public const int FusionGate = 33550694;
            public const int Skyscraper = 63035430;
            public const int HeroBlast = 37412656;
            public const int FavoriteContact = 75047173;

            public const int RainbowNeos = 86346643;
            public const int ContrastChaos = 23204029;
            public const int NeosKluger = 90307498;
            public const int ShiningNeosWingman = 56733747;
            public const int Dian = 62624486;
            public const int Tempest = 83121692;
            public const int GreatTornado = 3642509;
            public const int Anki = 59642500;
            public const int DivineWind = 22093873;
            public const int Wildedge = 10526791;
            public const int PlasmaVice = 60493189;
            public const int NovaMaster = 1945387;
            public const int TheShining = 22061412;
            public const int Acid = 29095552;
            public const int ShiningFlareWingman = 25366484;
            public const int FavoriteShiningFlare = 87758525;
            public const int Escuridao = 33574806;
            public const int TerraFirma = 74711057;
            public const int Koga = 50608164;
            public const int InfernalRage = 93347961;
            public const int WildWingman = 55615891;
            public const int Vapor = 10920352;
            public const int DarkLaw = 58481572;
            public const int ThunderGiant = 61204971;
            public const int Gaia = 16304628;
            public const int Goka = 58147549;
            public const int Blast = 89870349;
            public const int FavoriteFlameWingman = 13243124;
            public const int FlameWingman = 35809262;
            public const int RampartBlaster = 47737087;
            public const int Darkbright = 41517968;
            public const int Mudballman = 52031567;
            public const int NecroidShaman = 81003500;
            public const int SteamHealer = 81197327;
            public const int Mariner = 14225239;
            public const int WonderDriver = 1948619;
        }

        const int SetHero = 0x8, SetElementalHero = 0x3008, SetVisionHero = 0x5008, SetMaskedHero = 0xa008;
        static readonly int[] WingmanFusions = { CardId.InfernalRage, CardId.FlameWingman, CardId.WildWingman, CardId.FavoriteFlameWingman, CardId.ShiningFlareWingman };

        public IrlHeroExecutor(GameAI ai, Duel duel)
            : base(ai, duel)
        {
            // Responses
            AddExecutor(ExecutorType.Activate, CardId.ContrastChaos, () => Duel.LastChainPlayer == 1 && TargetEnemyCard());
            AddExecutor(ExecutorType.Activate, CardId.Koga, KogaEffect);
            AddExecutor(ExecutorType.Activate, CardId.HonestNeos, HonestNeosEffect);
            AddExecutor(ExecutorType.Activate, CardId.SpiritOfNeos, () => { Pick(CardId.Polymerization, CardId.FavoriteContact, CardId.MiracleFusion); return true; });
            AddExecutor(ExecutorType.Activate, CardId.WingedKuribohLV6, () => !MyTurn || Card.Location != CardLocation.MonsterZone);
            AddExecutor(ExecutorType.Activate, CardId.MaskChange, MaskChangeEffect);
            AddExecutor(ExecutorType.Activate, CardId.MaskChangeII, MaskChangeIIEffect);
            AddExecutor(ExecutorType.Activate, CardId.DarkLaw);

            // Field / searches
            AddExecutor(ExecutorType.Activate, CardId.Skyscraper, () => Bot.SpellZone[5] == null);
            AddExecutor(ExecutorType.Activate, CardId.EmergencyCall, EmergencyCallEffect);
            AddExecutor(ExecutorType.Activate, CardId.Sabatiel, () => Card.Location == CardLocation.Hand && Bot.LifePoints > 4000
                && !Bot.HasInHand(CardId.Polymerization) ? PickThen(CardId.Polymerization, CardId.MiracleFusion) : !MyTurn);
            AddStaples();

            // The Vision HERO line
            AddExecutor(ExecutorType.SpSummon, CardId.Faris, FarisSpSummon);
            AddExecutor(ExecutorType.Summon, CardId.Faris, NormalSummon);
            AddExecutor(ExecutorType.Activate, CardId.Faris, () => Card.Location == CardLocation.Hand ? FarisSpSummon() : PickThen(CardId.Increase, CardId.Vyon));
            AddExecutor(ExecutorType.Activate, CardId.Increase, IncreaseEffect);
            AddExecutor(ExecutorType.Activate, CardId.Vyon, VyonEffect);
            AddExecutor(ExecutorType.Activate, CardId.Polymerization, PolymerizationEffect);
            AddExecutor(ExecutorType.Activate, CardId.InfernalRage, InfernalRageEffect);
            AddExecutor(ExecutorType.Activate, CardId.FavoriteContact, FavoriteContactEffect);
            AddExecutor(ExecutorType.Activate, CardId.ShiningNeosWingman, () => { Pick(Enemy.GetMonsters().Concat(Enemy.GetSpells()).OrderByDescending(CardValue)); return true; });
            AddExecutor(ExecutorType.Activate, CardId.FavoriteHero, FavoriteHeroEffect);

            // Mask route
            AddExecutor(ExecutorType.Activate, CardId.AHeroLives, () => Bot.GetMonsters().All(m => m.IsFacedown()) && Bot.LifePoints > 3000
                && PickThen(CardId.ShadowMist, CardId.Stratos, CardId.SolidSoldier));
            AddExecutor(ExecutorType.Activate, CardId.ShadowMist, ShadowMistEffect);
            AddExecutor(ExecutorType.Activate, CardId.Stratos, StratosEffect);
            AddExecutor(ExecutorType.Activate, CardId.SolidSoldier, () => PickThen(CardId.ShadowMist, CardId.Vyon, CardId.Stratos, CardId.Faris));
            AddExecutor(ExecutorType.Activate, CardId.LiquidSoldier, () => PickThen(CardId.ShadowMist, CardId.Vyon, CardId.Stratos, CardId.SolidSoldier));
            AddExecutor(ExecutorType.Activate, CardId.MaskCharge, () => PickThen(CardId.ShadowMist, CardId.MaskChange));
            AddExecutor(ExecutorType.Activate, CardId.FormChange, () => Bot.GetMonsters().Any(m => m.HasType(CardType.Fusion) && m.Attack < 2500));

            // Other fusions and recursion
            AddExecutor(ExecutorType.Activate, CardId.MiracleFusion, MiracleFusionEffect);
            AddExecutor(ExecutorType.Activate, CardId.ParallelWorldFusion, () => PickThen(CardId.ShiningNeosWingman, CardId.TheShining, CardId.Escuridao));
            AddExecutor(ExecutorType.Activate, CardId.FusionGate, () => !Bot.HasInSpellZone(CardId.FusionGate, true));
            AddExecutor(ExecutorType.Activate, CardId.OOversoul, () => PickThen(CardId.Neos, CardId.NeosAlius, CardId.Sparkman, CardId.Bladedge));
            AddExecutor(ExecutorType.Activate, CardId.HeroBlast, () => PickThen(CardId.Neos, CardId.Bladedge, CardId.Sparkman));
            AddExecutor(ExecutorType.Activate, CardId.HeatedHeart, () => MyTurn && Duel.Phase == DuelPhase.BattleStart);
            AddExecutor(ExecutorType.Activate, CardId.ShiningFlareWingman);
            AddExecutor(ExecutorType.Activate, CardId.PlasmaVice, () => TargetEnemyMonster());
            AddExecutor(ExecutorType.Activate, CardId.ThunderGiant, () => TargetEnemyMonster());
            AddExecutor(ExecutorType.Activate, CardId.WildWingman, () => Util.GetBestEnemySpell() != null && PickThen(Util.GetBestEnemySpell()));
            AddExecutor(ExecutorType.Activate, CardId.NecroidShaman, () => TargetEnemyMonster());
            AddExecutor(ExecutorType.Activate, CardId.Gaia, () => TargetEnemyMonster());
            AddExecutor(ExecutorType.Activate, CardId.Blast, () => TargetEnemyMonster());
            AddExecutor(ExecutorType.Activate, CardId.Woodsman, () => !Bot.HasInHand(CardId.Polymerization));

            // Normal Summons
            AddExecutor(ExecutorType.Summon, CardId.Stratos, NormalSummon);
            AddExecutor(ExecutorType.Summon, CardId.SolidSoldier, () => Bot.Hand.Any(c => c.HasSetcode(SetHero) && c.Level <= 4 && !c.IsCode(CardId.SolidSoldier)) && NormalSummon());
            AddExecutor(ExecutorType.Summon, CardId.Vyon, NormalSummon);
            AddExecutor(ExecutorType.Summon, CardId.LiquidSoldier, () => Bot.Graveyard.Any(c => c.HasSetcode(SetHero) && c.Level <= 4) && NormalSummon());
            AddExecutor(ExecutorType.SpSummon, CardId.Bubbleman);
            AddExecutor(ExecutorType.SummonOrSet, () => BestFallbackSummon() && NormalSummon());

            AddExecutor(ExecutorType.SpellSet, SetTrapOrQuickPlay);
            AddExecutor(ExecutorType.Repos, DefaultMonsterRepos);
            AddExecutor(ExecutorType.Activate, GenericActivate);
        }

        protected override int[] AddToHandPriority => new[] {
            CardId.Faris, CardId.Polymerization, CardId.FavoriteContact, CardId.MaskChange, CardId.ShadowMist, CardId.Vyon,
            CardId.Stratos, CardId.FavoriteHero, CardId.Skyscraper, CardId.SolidSoldier, CardId.Neos, CardId.MiracleFusion
        };
        protected override int[] SpSummonPriority => new[] {
            CardId.ShiningNeosWingman, CardId.DarkLaw, CardId.InfernalRage, CardId.Neos, CardId.Vyon, CardId.ShadowMist,
            CardId.Stratos, CardId.Faris
        };
        protected override int[] ExpendablePriority => new[] {
            CardId.Neos, CardId.NeosAlius, CardId.Necroshade, CardId.Clayman, CardId.Burstinatrix, CardId.Avian, CardId.Woodsman,
            CardId.Ocean, CardId.Heat, CardId.Bubbleman, CardId.Voltic
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

        bool PickThen(ClientCard c)
        {
            if (c == null) return false;
            Pick(c);
            return true;
        }

        bool IsHero(ClientCard c) => c != null && c.HasSetcode(SetHero);
        bool IsElementalHero(ClientCard c) => c != null && c.HasSetcode(SetElementalHero);

        // ---------- Vision HERO line ----------

        bool EmergencyCallEffect()
        {
            // Shadow Mist when A Hero Lives / Solid Soldier can summon it; otherwise Stratos (searches again)
            if (Bot.HasInHand(CardId.SolidSoldier) || Bot.HasInHand(CardId.AHeroLives)) Pick(CardId.ShadowMist, CardId.Stratos);
            else Pick(CardId.Stratos, CardId.ShadowMist, CardId.SolidSoldier, CardId.Sparkman);
            return true;
        }

        bool FarisSpSummon()
        {
            // discard a HERO, ideally Neos (Favorite Contact material)
            var discard = Bot.Hand.Where(c => IsHero(c) && c.IsMonster() && !c.IsCode(CardId.Faris))
                .OrderBy(c => c.IsCode(CardId.Neos) ? 0 : 1).ThenBy(c => Rank(ExpendablePriority, c)).ThenBy(CardValue).FirstOrDefault();
            if (discard == null) return false;
            Pick(discard);
            return true;
        }

        bool IncreaseEffect()
        {
            if (Card.Location == CardLocation.SpellZone)
            {
                // tribute Faris (or the least useful HERO) to bring out Increase
                var t = Bot.GetMonsters().Where(m => IsHero(m) && !m.HasType(CardType.Fusion)).OrderBy(m => m.IsCode(CardId.Faris) ? 0 : 1).ThenBy(CardValue).FirstOrDefault();
                if (t == null) return false;
                Pick(t);
                return true;
            }
            // Special Summoned from the S/T zone: Vyon from the Deck
            Pick(CardId.Vyon);
            return true;
        }

        bool VyonEffect()
        {
            if (Card.Location != CardLocation.MonsterZone) return false;
            if (GenericTrigger || Desc(CardId.Vyon, 0))
            {
                // send Neos for Favorite Contact; Shadow Mist (searches when sent) once Neos is there
                if (!Bot.HasInGraveyard(CardId.Neos) && MaybeInDeck(CardId.Neos, 2)) Pick(CardId.Neos);
                else Pick(CardId.ShadowMist, CardId.Neos, CardId.NeosAlius);
                return true;
            }
            // banish a HERO from the GY (never Neos) to add Polymerization
            if (Bot.HasInHand(CardId.Polymerization)) return false;
            var cost = Bot.Graveyard.Where(c => IsHero(c) && !c.IsCode(CardId.Neos)).OrderBy(CardValue).FirstOrDefault();
            if (cost == null) return false;
            Pick(cost);
            Pick(CardId.Polymerization);
            return true;
        }

        // Two Elemental HEROes with different Attributes for Infernal Rage, from hand/field
        List<ClientCard> InfernalRageMaterials()
        {
            var pool = Bot.Hand.Where(c => IsElementalHero(c) && c.IsMonster()).Concat(Bot.GetMonsters().Where(m => m.IsFaceup() && IsElementalHero(m)))
                .OrderBy(c => c.Location == CardLocation.Hand ? 0 : 1).ThenBy(c => c.HasType(CardType.Normal) ? 0 : 1).ThenBy(CardValue).ToList();
            foreach (var a in pool)
                foreach (var b in pool)
                    if (a != b && a.Attribute != b.Attribute)
                        return new List<ClientCard> { a, b };
            return null;
        }

        bool PolymerizationEffect()
        {
            if (!MyTurn) return false;
            var mats = InfernalRageMaterials();
            if (mats != null && Bot.HasInExtra(CardId.InfernalRage))
            {
                Pick(CardId.InfernalRage);
                AI.SelectMaterials(mats);
                return true;
            }
            // otherwise the best Fusion the engine can make
            Pick(CardId.ShiningFlareWingman, CardId.Escuridao, CardId.TheShining, CardId.GreatTornado, CardId.NovaMaster, CardId.Gaia,
                CardId.ThunderGiant, CardId.PlasmaVice, CardId.WildWingman, CardId.FlameWingman);
            return Bot.Hand.Count(c => IsHero(c) && c.IsMonster()) + Bot.GetMonsters().Count(IsHero) >= 2;
        }

        bool InfernalRageEffect()
        {
            if (GenericTrigger || Desc(CardId.InfernalRage, 0))
            {
                // Favorite Contact when Neos is available for Shining Neos Wingman, else Favorite Hero
                bool neos = Bot.HasInGraveyard(CardId.Neos) || Bot.HasInHand(CardId.Neos) || Bot.HasInBanished(CardId.Neos);
                if (neos && !Bot.HasInHand(CardId.FavoriteContact)) Pick(CardId.FavoriteContact, CardId.FavoriteHero);
                else Pick(CardId.FavoriteHero, CardId.FavoriteContact);
                return true;
            }
            return false; // keep Infernal Rage on the field as Shining Neos Wingman material
        }

        bool FavoriteContactEffect()
        {
            bool neos = Bot.HasInGraveyard(CardId.Neos) || Bot.HasInHand(CardId.Neos) || Bot.HasInBanished(CardId.Neos) || Bot.HasInMonstersZone(CardId.Neos);
            bool wingman = WingmanFusions.Any(id => Bot.HasInMonstersZone(id) || Bot.HasInGraveyard(id));
            if (neos && wingman && Bot.HasInExtra(CardId.ShiningNeosWingman))
            {
                Pick(CardId.ShiningNeosWingman);
                return true;
            }
            Pick(CardId.ShiningFlareWingman, CardId.Escuridao, CardId.TheShining);
            return MyTurn;
        }

        bool FavoriteHeroEffect()
        {
            if (Card.Location == CardLocation.SpellZone && Card.IsFaceup())
            {
                // start of Battle Phase: activate a Field Spell from hand/Deck (Skyscraper)
                Pick(CardId.Skyscraper);
                return true;
            }
            var t = Bot.GetMonsters().Where(m => m.IsFaceup() && IsHero(m) && m.Level >= 5).OrderByDescending(m => m.IsCode(CardId.ShiningNeosWingman) ? 1 : 0)
                .ThenByDescending(m => m.Attack).FirstOrDefault();
            if (t == null) return false;
            Pick(t);
            return true;
        }

        // ---------- Mask route ----------

        bool ShadowMistEffect()
        {
            if (Card.Location == CardLocation.MonsterZone) { Pick(CardId.MaskChange, CardId.MaskChangeII, CardId.FormChange); return true; }
            Pick(CardId.Faris, CardId.Stratos, CardId.Vyon, CardId.ShadowMist);
            return true;
        }

        bool StratosEffect()
        {
            // 0: destroy Spells/Traps (needs other HEROes), 1: search a HERO
            int heroes = Bot.GetMonsters().Count(IsHero) - 1;
            preferStratosDestroy = heroes >= 1 && Enemy.GetSpellCount() >= heroes;
            if (!preferStratosDestroy) Pick(CardId.Faris, CardId.ShadowMist, CardId.Vyon, CardId.SolidSoldier);
            return true;
        }

        bool preferStratosDestroy;

        public override int OnSelectOption(IList<long> options)
        {
            // Stratos: search (option 2) unless we chose to clear backrow
            int destroy = options.IndexOf(Util.GetStringId(CardId.Stratos, 1));
            if (destroy >= 0)
                return preferStratosDestroy ? destroy : (destroy == 0 && options.Count > 1 ? 1 : 0);
            return base.OnSelectOption(options);
        }

        bool MaskChangeEffect()
        {
            // DARK HERO into Dark Law (control), others into the matching Masked HERO; quick-play, also a dodge
            var t = Bot.GetMonsters().Where(m => m.IsFaceup() && IsHero(m) && !m.HasSetcode(SetMaskedHero))
                .OrderBy(m => m.HasAttribute(CardAttribute.Dark) ? 0 : 1).ThenBy(CardValue).FirstOrDefault();
            if (t == null) return false;
            if (MyTurn && Duel.Phase != DuelPhase.Main1 && Duel.Phase != DuelPhase.Main2) return false;
            Pick(t);
            return true;
        }

        bool MaskChangeIIEffect()
        {
            var t = Bot.GetMonsters().Where(m => m.IsFaceup() && m.Level > 0 && !m.HasSetcode(SetMaskedHero)).OrderBy(CardValue).FirstOrDefault();
            if (t == null || Bot.Hand.Count < 2) return false;
            Pick(Bot.Hand.Where(c => c != Card).OrderBy(c => Rank(ExpendablePriority, c)).ThenBy(CardValue).Take(1));
            Pick(t);
            return true;
        }

        bool MiracleFusionEffect()
        {
            if (!MyTurn) return false;
            Pick(CardId.ShiningNeosWingman, CardId.InfernalRage, CardId.Escuridao, CardId.TheShining, CardId.ShiningFlareWingman,
                CardId.GreatTornado, CardId.NovaMaster, CardId.Gaia);
            return Bot.Graveyard.Count(IsHero) + Bot.GetMonsters().Count(IsHero) >= 2;
        }

        bool KogaEffect()
        {
            if (Card.Location != CardLocation.MonsterZone || !Bot.Graveyard.Any(IsHero)) return false;
            var t = Enemy.GetMonsters().Where(m => m.IsFaceup()).OrderByDescending(m => m.Attack).FirstOrDefault();
            if (t == null) return false;
            Pick(Bot.Graveyard.Where(IsHero).OrderByDescending(c => c.Attack).Take(1));
            Pick(t);
            return true;
        }

        bool HonestNeosEffect()
        {
            // +2500 on an attacking HERO during battle
            if (Duel.Phase != DuelPhase.DamageCal && Duel.Phase != DuelPhase.BattleStep && Duel.Phase != DuelPhase.Damage) return false;
            var attacker = Bot.GetMonsters().FirstOrDefault(m => m.IsAttack() && IsHero(m));
            if (attacker == null) return false;
            Pick(attacker);
            return true;
        }

        public override bool OnSelectHand()
        {
            return true;
        }
    }
}
