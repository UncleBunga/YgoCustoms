using System.Collections.Generic;
using System.Linq;
using YGOSharp.OCGWrapper.Enums;

namespace WindBot.Game.AI.Decks
{
    // IRL Yugi, as the user plays it:
    //  - Light and Darkness Ritual is the main force. Mind Shuffle searches Skull Archfiend of Chaos and discards it; Skull
    //    sends the Ritual Spell and adds Black Chaos / BLS. Celtic Mystic draws and tributes itself into them.
    //    Mind Shuffle's swap: if BLS or Black Chaos is targeted, swap to the other one to dodge (both ignore untargeted
    //    effects); swap to BLS to banish a fresh monster, or to Black Chaos to protect the backrow.
    //  - Dark Magician: Ragged Records (Chaos Form) > Illusion of Chaos > Magician's Rod > Dark Magical Curtain; the ideal
    //    board is Eternal Soul + Dark Magician the Dragon Knight protecting each other, banishing with Dark Magical Circle.
    //  - The poker knights turbo Slifer (Joker's Straight / Joker's Wild / Thunderspeed Summon); Advanced Ritual Art is for
    //    Black Luster Soldier - Legendary Swordsman.
    [Deck("IRL_Yugi", "AI_IRL_Yugi")]
    public class IrlYugiExecutor : CustomExecutorBase
    {
        public class CardId
        {
            public const int DarkMagician = 46986414;
            public const int JacksKnight = 90876561;
            public const int QueensKnight = 25652259;
            public const int CelticGuardian = 91152256;
            public const int Slifer = 10000020;
            public const int BlackSkullDragon = 97818130;
            public const int TimaeusUnitedDragon = 3078380;
            public const int BusterBlader = 78193831;
            public const int PharaohsServant = 88570003;
            public const int GaiaMagicalKnight = 34130561;
            public const int MagicianOfDarkIllusion = 35191415;
            public const int SkullArchfiend = 24088928;
            public const int ApprenticeIllusionMagician = 30603688;
            public const int DarkMagicianGirl = 38033121;
            public const int CurseOfDragonfire = 7241272;
            public const int JokersKnight = 29284413;
            public const int KingsKnight = 64788463;
            public const int CelticMystic = 50073633;
            public const int MagiciansRod = 7084129;
            public const int MagiciansRobe = 71696014;
            public const int MultiplyingKuriboh = 14965712;
            public const int Magikuriboh = 31699677;
            public const int Griffoh = 97462632;
            public const int BLS = 70405001;
            public const int LegendarySwordsman = 70551291;
            public const int BlackChaos = 44001993;
            public const int MagicianOfChaos = 47963370;
            public const int IllusionOfChaos = 12266229;
            public const int EyeOfTimaeus = 1784686;
            public const int DarkMagicAttack = 2314238;
            public const int RaggedRecords = 24461358;
            public const int MonsterReborn = 83764719;
            public const int JokersStraight = 92067220;
            public const int SuperSoldierRitual = 14094090;
            public const int ChaosForm = 21082832;
            public const int LaDR = 33599853;
            public const int AdvancedRitualArt = 46052429;
            public const int ChaosMagicalHats = 2372506;
            public const int DarkMagicalCurtain = 41350417;
            public const int ThunderspeedSummon = 55557574;
            public const int RoyalStraight = 58415502;
            public const int SecretsOfDarkMagic = 59514116;
            public const int IllusionMagic = 73616671;
            public const int ChaosMysticBox = 75983808;
            public const int DarkMagicalCircle = 47222536;
            public const int GatewayToChaos = 40089744;
            public const int MagiciansSalvation = 95477924;
            public const int MagicianNavigation = 7922915;
            public const int JokersWild = 81945678;
            public const int MindShuffle = 24749710;
            public const int EternalSoul = 48680970;

            public const int QuintetMagician = 84433295;
            public const int DivineMagicianOfSlifer = 91900010;
            public const int ArcanaKnightJoker = 6150045;
            public const int ArcanaDisguiseJoker = 91900007;
            public const int DragonKnight = 41721210;
            public const int MasterOfChaos = 85059922;
            public const int AmuletDragon = 75380687;
            public const int KnightOfDragonMagic = 5829717;
            public const int DarkPaladin = 98502113;
            public const int TheDarkMagicians = 50237654;
            public const int DarkCavalry = 73452089;
            public const int GaiaMagicalKnightOfDragons = 15989522;
            public const int GaiaTheDragonChampion = 66889139;
            public const int EbonIllusionMagician = 96471335;
            public const int EbonHighMagician = 85551711;
            public const int BLSSoldierOfChaos = 49202162;
            public const int ArcanaExtraJoker = 64454614;
            public const int ArcanaGuardJoker = 91900006;
        }

        static readonly int[] LaDRMonsters = { CardId.BLS, CardId.BlackChaos };
        static readonly int[] Knights = { CardId.QueensKnight, CardId.KingsKnight, CardId.JacksKnight };
        static readonly int[] DarkMagicians = { CardId.DarkMagician, CardId.PharaohsServant, CardId.MagicianOfDarkIllusion, CardId.DragonKnight, CardId.MagicianOfChaos };

        public IrlYugiExecutor(GameAI ai, Duel duel)
            : base(ai, duel)
        {
            // Responses first: the Mind Shuffle swap and the protection pieces
            AddExecutor(ExecutorType.Activate, CardId.MindShuffle, MindShuffleEffect);
            AddExecutor(ExecutorType.Activate, CardId.IllusionOfChaos, IllusionOfChaosEffect);
            AddExecutor(ExecutorType.Activate, CardId.MagicianOfChaos, () => Duel.LastChainPlayer == 1 && TargetEnemyCard());
            AddExecutor(ExecutorType.Activate, CardId.ChaosMagicalHats, () => Duel.LastChainPlayer == 1 && Bot.GetMonsters().Any(m => m.IsCode(LaDRMonsters)));
            AddExecutor(ExecutorType.Activate, CardId.ChaosMysticBox, () => Duel.LastChainPlayer == 1);
            AddExecutor(ExecutorType.Activate, CardId.MagiciansRobe, () => !MyTurn && Bot.Hand.Any(c => c.IsSpell() || c.IsTrap()));
            AddExecutor(ExecutorType.Activate, CardId.Magikuriboh, () => { Pick(CardId.DarkMagician, CardId.DarkMagicianGirl); return true; });
            AddExecutor(ExecutorType.Activate, CardId.PharaohsServant, PharaohsServantEffect);
            AddExecutor(ExecutorType.Activate, CardId.DarkPaladin, () => Duel.LastChainPlayer == 1 && Bot.Hand.Count > 0);
            AddExecutor(ExecutorType.Activate, CardId.ArcanaKnightJoker, () => Duel.LastChainPlayer == 1);
            AddExecutor(ExecutorType.Activate, CardId.Griffoh, () => !MyTurn && (Duel.Phase == DuelPhase.BattleStep || Duel.Phase == DuelPhase.Damage));
            AddStaples();

            // Light and Darkness Ritual engine
            AddExecutor(ExecutorType.Activate, CardId.SkullArchfiend, SkullArchfiendEffect);
            AddExecutor(ExecutorType.Activate, CardId.LaDR, LaDREffect);
            // Celtic Mystic when it can tribute into a Ritual monster in hand; with Magician's Rod and no Ritual monster the
            // Rod > Curtain > Dark Magician line comes first
            AddExecutor(ExecutorType.Summon, CardId.CelticMystic, () => Bot.Hand.Any(c => c.IsCode(LaDRMonsters))
                || !Bot.HasInHand(CardId.MagiciansRod) && Bot.Hand.Any(c => c.IsCode(CardId.LaDR, CardId.SkullArchfiend, CardId.MindShuffle)));
            AddExecutor(ExecutorType.Activate, CardId.CelticMystic, CelticMysticEffect);
            AddExecutor(ExecutorType.Activate, CardId.BLS, () => TargetEnemyCard());
            AddExecutor(ExecutorType.Activate, CardId.BlackChaos, BlackChaosEffect);
            AddExecutor(ExecutorType.Activate, CardId.GatewayToChaos, () => { Pick(CardId.BLS, CardId.LegendarySwordsman, CardId.LaDR, CardId.ChaosForm); return true; });

            // Dark Magician line
            AddExecutor(ExecutorType.Activate, CardId.RaggedRecords, RaggedRecordsEffect);
            AddExecutor(ExecutorType.Summon, CardId.MagiciansRod, () => true);
            AddExecutor(ExecutorType.Activate, CardId.MagiciansRod, () => { Pick(CardId.DarkMagicalCurtain, CardId.EternalSoul, CardId.DarkMagicalCircle, CardId.MagicianNavigation); return true; });
            AddExecutor(ExecutorType.Activate, CardId.DarkMagicalCurtain, () => { Pick(CardId.DarkMagician); Pick(CardId.EternalSoul, CardId.DarkMagicalCircle, CardId.EyeOfTimaeus); return true; });
            AddExecutor(ExecutorType.Activate, CardId.MagicianNavigation, NavigationEffect);
            AddExecutor(ExecutorType.Activate, CardId.EternalSoul, EternalSoulEffect);
            AddExecutor(ExecutorType.Activate, CardId.DarkMagicalCircle, () => Card.Location == CardLocation.Hand ? PickThen(CardId.DarkMagician, CardId.EternalSoul, CardId.EyeOfTimaeus) : TargetEnemyCard());
            AddExecutor(ExecutorType.Activate, CardId.EyeOfTimaeus, EyeOfTimaeusEffect);
            AddExecutor(ExecutorType.Activate, CardId.TimaeusUnitedDragon, TimaeusEffect);
            AddExecutor(ExecutorType.Activate, CardId.IllusionMagic, () => !Bot.HasInHand(CardId.DarkMagician) && Bot.GetMonsters().Any(m => m.IsFaceup() && m.HasRace(CardRace.SpellCaster) && !m.IsCode(DarkMagicians) && !m.IsCode(LaDRMonsters)));
            AddExecutor(ExecutorType.Activate, CardId.DarkMagicAttack, () => Enemy.GetSpellCount() > 0);
            AddExecutor(ExecutorType.Activate, CardId.MagicianOfDarkIllusion, () => { Pick(CardId.DarkMagician); return true; });
            // no starter: push a Level 7+ body anyway so Mind Shuffle swaps and the fusions have something to work with
            AddExecutor(ExecutorType.SpSummon, CardId.ApprenticeIllusionMagician, () => (!Bot.HasInHand(CardId.DarkMagician) || !HasBigBody) && Bot.Hand.Count >= 2);
            AddExecutor(ExecutorType.Activate, CardId.BlackSkullDragon, BlackSkullEffect);
            AddExecutor(ExecutorType.Summon, CardId.DarkMagician, () => !HasBigBody && Bot.GetMonsters().Count(m => !m.IsCode(LaDRMonsters)) >= 2);
            AddExecutor(ExecutorType.Summon, CardId.BusterBlader, () => !HasBigBody && Bot.GetMonsters().Count(m => !m.IsCode(LaDRMonsters)) >= 2);
            AddExecutor(ExecutorType.Activate, CardId.ApprenticeIllusionMagician, () => Card.Location != CardLocation.MonsterZone || PickThen(CardId.DarkMagician));
            AddExecutor(ExecutorType.SpSummon, CardId.DragonKnight);

            // Rituals
            AddExecutor(ExecutorType.Activate, CardId.AdvancedRitualArt, () => PickThen(CardId.LegendarySwordsman, CardId.BLS, CardId.BlackChaos));
            // Chaos Form can banish Dark Magician from the GY as the whole Tribute for Magician of Chaos
            AddExecutor(ExecutorType.Activate, CardId.ChaosForm, () => PickThen(Bot.HasInGraveyard(CardId.DarkMagician)
                ? new[] { CardId.MagicianOfChaos, CardId.IllusionOfChaos, CardId.LegendarySwordsman } : new[] { CardId.LegendarySwordsman, CardId.MagicianOfChaos, CardId.IllusionOfChaos }));
            AddExecutor(ExecutorType.Activate, CardId.SuperSoldierRitual, () => PickThen(CardId.LegendarySwordsman, CardId.BLS));
            AddExecutor(ExecutorType.Activate, CardId.SecretsOfDarkMagic, () => PickThen(BridgeFusions()));
            AddExecutor(ExecutorType.Activate, CardId.CurseOfDragonfire, () => PickThen(CardId.GaiaMagicalKnightOfDragons, CardId.GaiaTheDragonChampion, CardId.KnightOfDragonMagic));
            AddExecutor(ExecutorType.Activate, CardId.MasterOfChaos, () => true);

            // Knights > Slifer
            AddExecutor(ExecutorType.Activate, CardId.JokersStraight, () => PickThen(CardId.KingsKnight, CardId.JacksKnight));
            AddExecutor(ExecutorType.Activate, CardId.JokersWild, JokersWildEffect);
            AddExecutor(ExecutorType.Activate, CardId.ThunderspeedSummon, ThunderspeedEffect);
            AddExecutor(ExecutorType.Activate, CardId.RoyalStraight, () => !Bot.HasInHand(CardId.ThunderspeedSummon) && PickThen(CardId.ArcanaKnightJoker));
            AddExecutor(ExecutorType.Activate, CardId.JokersKnight, () => PickThen(CardId.QueensKnight, CardId.KingsKnight, CardId.JacksKnight));
            AddExecutor(ExecutorType.SpSummon, CardId.ArcanaGuardJoker, () => Bot.GetMonsters().Any(m => m.IsCode(Knights) || m.IsCode(CardId.JokersKnight)));
            AddExecutor(ExecutorType.Activate, CardId.ArcanaGuardJoker, () => PickThen(CardId.JokersKnight, CardId.JokersWild));
            AddExecutor(ExecutorType.Summon, CardId.KingsKnight, () => Bot.HasInMonstersZone(CardId.QueensKnight));
            AddExecutor(ExecutorType.Summon, CardId.Slifer, () => true);
            AddExecutor(ExecutorType.SpSummon, CardId.DivineMagicianOfSlifer, () => Bot.HasInMonstersZone(CardId.Slifer) && Bot.GetMonsters().Any(m => m.IsCode(DarkMagicians)));
            AddExecutor(ExecutorType.SpSummon, CardId.ArcanaKnightJoker);
            AddExecutor(ExecutorType.SpSummon, CardId.ArcanaDisguiseJoker, () => !Bot.HasInExtra(CardId.ArcanaKnightJoker) || Bot.HasInHand(CardId.ThunderspeedSummon) == false);

            AddExecutor(ExecutorType.Summon, CardId.GaiaMagicalKnight, () => true);
            AddExecutor(ExecutorType.SummonOrSet, () => BestFallbackSummon());
            AddExecutor(ExecutorType.SpellSet, YugiSet);
            AddExecutor(ExecutorType.Repos, DefaultMonsterRepos);
            AddExecutor(ExecutorType.Activate, GenericActivate);
        }

        protected override int[] AddToHandPriority => new[] {
            CardId.SkullArchfiend, CardId.BlackChaos, CardId.BLS, CardId.CelticMystic, CardId.LaDR, CardId.MindShuffle,
            CardId.DarkMagician, CardId.MagiciansRod, CardId.IllusionOfChaos, CardId.EternalSoul, CardId.DarkMagicalCurtain
        };
        protected override int[] SpSummonPriority => new[] {
            CardId.BLS, CardId.BlackChaos, CardId.DarkMagician, CardId.MagicianOfDarkIllusion, CardId.PharaohsServant,
            CardId.DarkMagicianGirl, CardId.ApprenticeIllusionMagician, CardId.QueensKnight
        };
        protected override int[] SetPriority => new[] { CardId.MindShuffle, CardId.EternalSoul, CardId.ChaosMagicalHats, CardId.ChaosMysticBox };
        protected override int[] ExpendablePriority => new[] {
            CardId.SkullArchfiend, CardId.Griffoh, CardId.MultiplyingKuriboh, CardId.CelticGuardian, CardId.CurseOfDragonfire,
            CardId.BusterBlader, CardId.GaiaMagicalKnight, CardId.MagiciansRobe, CardId.JacksKnight, CardId.QueensKnight
        };

        bool PickThen(params int[] ids)
        {
            Pick(ids);
            return true;
        }

        ClientCard OurLaDR => Bot.GetMonsters().FirstOrDefault(m => m.IsFaceup() && m.IsCode(LaDRMonsters));

        // ---------- Light and Darkness Ritual ----------

        bool MindShuffleEffect()
        {
            if (Card.IsFacedown()) return !MyTurn && (Duel.Phase == DuelPhase.End || Duel.LastChainPlayer == 1) || MyTurn && Duel.Phase == DuelPhase.Main1;
            if (Desc(CardId.MindShuffle, 1)) return MindShuffleSwap();
            // search: Skull Archfiend (discarded, it fetches the Ritual Spell + a Ritual monster), else Celtic Mystic
            bool needRitual = !Bot.Hand.Any(c => c.IsCode(LaDRMonsters));
            if (!MyTurn && Duel.Phase != DuelPhase.End && Duel.LastChainPlayer != 1) return false;
            Pick(needRitual ? new[] { CardId.SkullArchfiend, CardId.CelticMystic } : new[] { CardId.CelticMystic, CardId.SkullArchfiend });
            Pick(CardId.SkullArchfiend, CardId.Griffoh, CardId.MultiplyingKuriboh, CardId.CelticGuardian);
            return true;
        }

        bool MindShuffleSwap()
        {
            var ours = OurLaDR;
            if (ours == null || Duel.LastChainPlayer != 1) return false;
            var other = Bot.Hand.FirstOrDefault(c => c.IsCode(LaDRMonsters) && !c.IsCode(ours.Id));
            if (other == null) return false;
            bool targeted = Duel.ChainTargets.Contains(ours);
            bool backrowHit = ours.IsCode(CardId.BLS) && other.IsCode(CardId.BlackChaos)
                && Duel.ChainTargets.Any(c => c.Controller == 0 && c.Location == CardLocation.SpellZone);
            bool banishNew = ours.IsCode(CardId.BlackChaos) && other.IsCode(CardId.BLS)
                && Enemy.GetMonsters().Any(m => m.IsFaceup() && (m.Attack >= ours.Attack || m.IsMonsterDangerous()));
            if (!targeted && !backrowHit && !banishNew) return false;
            Pick(ours);
            Pick(other);
            return true;
        }

        bool SkullArchfiendEffect()
        {
            if (Card.Location == CardLocation.Grave && (GenericTrigger || Desc(CardId.SkullArchfiend, 1)))
            {
                // sent to the GY: send Light and Darkness Ritual, add the Ritual monster we lack
                Pick(CardId.LaDR);
                Pick(Bot.Hand.Any(c => c.IsCode(CardId.BlackChaos)) ? new[] { CardId.BLS, CardId.BlackChaos } : new[] { CardId.BlackChaos, CardId.BLS });
                return true;
            }
            // shuffle 3 GY cards back to Special Summon itself: a body for the Ritual
            return MyTurn && Duel.Phase == DuelPhase.Main1 && Bot.Hand.Any(c => c.IsCode(LaDRMonsters)) && Bot.Hand.Any(c => c.IsCode(CardId.LaDR));
        }

        bool LaDREffect()
        {
            if (Card.Location == CardLocation.Grave)
            {
                // add itself plus a card that mentions it
                if (!MyTurn) return false;
                Pick(CardId.LaDR);
                Pick(Bot.Hand.Any(c => c.IsCode(LaDRMonsters)) ? new[] { CardId.CelticMystic, CardId.MindShuffle, CardId.SkullArchfiend }
                    : new[] { CardId.BlackChaos, CardId.BLS, CardId.SkullArchfiend, CardId.MindShuffle });
                return true;
            }
            // BLS when there is a card to banish, else Black Chaos (protects the backrow)
            bool target = Enemy.GetFieldCount() > 0;
            Pick(target ? new[] { CardId.BLS, CardId.BlackChaos } : new[] { CardId.BlackChaos, CardId.BLS });
            return true;
        }

        bool CelticMysticEffect()
        {
            if (Card.Location != CardLocation.MonsterZone) return false;
            if (Bot.Hand.Any(c => c.IsCode(LaDRMonsters)) && !Desc(CardId.CelticMystic, 0) && !GenericTrigger)
            {
                Pick(Bot.Hand.Any(c => c.IsCode(CardId.BLS)) && Enemy.GetFieldCount() > 0 ? new[] { CardId.BLS, CardId.BlackChaos } : new[] { CardId.BlackChaos, CardId.BLS });
                return true;
            }
            // reveal and draw 3, discard 2: Skull Archfiend first (it fetches the Ritual), then the least useful cards; keep
            // the Ritual Spell, its monsters, Mind Shuffle and one Dark Magician
            PickLater(YugiDiscards);
            PickLater(YugiDiscards);
            return true;
        }

        IEnumerable<ClientCard> YugiDiscards()
        {
            var keep = new[] { CardId.LaDR, CardId.BLS, CardId.BlackChaos, CardId.MindShuffle, CardId.CelticMystic, CardId.MagiciansRod, CardId.EternalSoul };
            int dms = Bot.Hand.Count(c => c.IsCode(CardId.DarkMagician));
            return Bot.Hand.OrderBy(c => c.IsCode(CardId.SkullArchfiend) ? 0
                    : c.IsCode(CardId.DarkMagician) && dms > 1 ? 1
                    : c.IsCode(CardId.Griffoh, CardId.MultiplyingKuriboh, CardId.CelticGuardian, CardId.BusterBlader, CardId.CurseOfDragonfire) ? 2
                    : c.IsCode(keep) ? 9 : 5)
                .ThenBy(c => Rank(ExpendablePriority, c));
        }

        bool BlackChaosEffect()
        {
            if (Card.Location != CardLocation.MonsterZone) return true;
            if (GenericTrigger || Desc(CardId.BlackChaos, 0)) { Pick(CardId.LaDR, CardId.DarkMagicAttack, CardId.MonsterReborn, CardId.JokersStraight); return true; }
            return TargetEnemyCard();
        }

        // ---------- Dark Magician ----------

        bool RaggedRecordsEffect()
        {
            if (Card.Location == CardLocation.Grave) return true;
            // Chaos Form reveals into Illusion of Chaos (the user's Dark Magician line); LaDR into Skull Archfiend / Celtic Mystic
            if (!Bot.HasInHand(CardId.IllusionOfChaos) && MaybeInDeck(CardId.IllusionOfChaos, 1) && !Bot.HasInHand(CardId.DarkMagician))
            {
                Pick(CardId.ChaosForm);
                Pick(CardId.IllusionOfChaos);
            }
            else
            {
                Pick(CardId.LaDR);
                Pick(CardId.SkullArchfiend, CardId.CelticMystic, CardId.BlackChaos, CardId.BLS);
            }
            return true;
        }

        bool IllusionOfChaosEffect()
        {
            if (Card.Location == CardLocation.Hand)
            {
                if (!MyTurn) return false;
                // reveal: add Magician's Rod (or Dark Magician), then put the least useful card on top
                Pick(Bot.HasInHand(CardId.MagiciansRod) ? new[] { CardId.DarkMagician } : new[] { CardId.MagiciansRod, CardId.DarkMagician });
                Pick(Bot.Hand.Where(c => !c.IsCode(CardId.MagiciansRod, CardId.DarkMagician)).OrderBy(c => Rank(ExpendablePriority, c)).ThenBy(CardValue));
                return true;
            }
            return Duel.LastChainPlayer == 1;
        }

        bool NavigationEffect()
        {
            if (Card.Location == CardLocation.Grave) return Duel.LastChainPlayer == 1 || Enemy.GetSpells().Any(s => s.IsFaceup());
            Pick(CardId.DarkMagician);
            Pick(CardId.MagicianOfDarkIllusion, CardId.DarkMagicianGirl, CardId.PharaohsServant, CardId.DarkMagician);
            return true;
        }

        bool EternalSoulEffect()
        {
            // negated Eternal Soul is destroyed and destroys our whole board
            if (EnemySpellTrapNegateReady) return false;
            if (Card.Location == CardLocation.Hand) return true;
            // their turn: bring Dark Magician (Dark Magical Circle banishes on its summon) in response, or before they attack
            if (!MyTurn) return (Duel.LastChainPlayer == 1 || Duel.Phase == DuelPhase.BattleStart || Duel.Phase == DuelPhase.End)
                && (Bot.HasInHand(CardId.DarkMagician) || Bot.HasInGraveyard(CardId.DarkMagician)) && PickThen(CardId.DarkMagician);
            if (Bot.HasInHand(CardId.DarkMagician) || Bot.HasInGraveyard(CardId.DarkMagician)) { Pick(CardId.DarkMagician); return true; }
            return !Bot.HasInHand(CardId.DarkMagicAttack);
        }

        bool EyeOfTimaeusEffect()
        {
            if (!Bot.GetMonsters().Any(m => m.IsFaceup() && m.IsCode(CardId.DarkMagician, CardId.PharaohsServant, CardId.MagicianOfDarkIllusion))) return false;
            Pick(Bot.GetMonsters().Where(m => m.IsFaceup() && m.IsCode(CardId.DarkMagician, CardId.PharaohsServant, CardId.MagicianOfDarkIllusion)));
            Pick(CardId.DragonKnight, CardId.AmuletDragon, CardId.KnightOfDragonMagic);
            return true;
        }

        bool TimaeusEffect()
        {
            if (Card.Location == CardLocation.Hand)
            {
                var cost = Bot.Hand.Where(c => c != Card && (c.IsCode(CardId.DarkMagicalCircle, CardId.EyeOfTimaeus, CardId.MagicianNavigation) || c.HasRace(CardRace.SpellCaster) && !c.IsCode(DarkMagicians)))
                    .OrderBy(c => Rank(ExpendablePriority, c)).FirstOrDefault();
                if (cost == null) return false;
                Pick(cost);
                return true;
            }
            Pick(BridgeFusions());
            return true;
        }

        bool HasBigBody => Bot.GetMonsters().Any(m => m.IsFaceup() && m.Level >= 7);

        // the user's bridges between the engines: Dark Magician + a Chaos/BLS Ritual into Master of Chaos, Dark Magician +
        // a Knight (Warrior) into Dark Cavalry, else the Dragon Knight that protects Eternal Soul
        int[] BridgeFusions()
        {
            var pool = Bot.GetMonsters().Concat(Bot.Hand).ToList();
            bool dm = pool.Any(c => c.IsCode(DarkMagicians));
            bool ritual = pool.Any(c => c.IsCode(CardId.BLS, CardId.BlackChaos, CardId.LegendarySwordsman, CardId.MagicianOfChaos, CardId.IllusionOfChaos));
            bool warrior = pool.Any(c => c.IsMonster() && c.HasRace(CardRace.Warrior));
            if (dm && ritual) return new[] { CardId.MasterOfChaos, CardId.DragonKnight, CardId.DarkCavalry };
            if (dm && warrior) return new[] { CardId.DarkCavalry, CardId.KnightOfDragonMagic, CardId.DragonKnight };
            return new[] { CardId.DragonKnight, CardId.TheDarkMagicians, CardId.AmuletDragon, CardId.DarkCavalry };
        }

        bool BlackSkullEffect()
        {
            if (Card.Location != CardLocation.Hand) return true;
            // discard a Ritual Spell we can't use right now for a Level 9 body
            var cost = Bot.Hand.FirstOrDefault(c => c.IsCode(CardId.AdvancedRitualArt, CardId.SuperSoldierRitual, CardId.ChaosForm) && !CanUseRitualSpell(c));
            if (cost == null || HasBigBody) return false;
            Pick(cost);
            return true;
        }

        bool CanUseRitualSpell(ClientCard spell)
        {
            if (spell.IsCode(CardId.ChaosForm)) return Bot.Hand.Any(c => c.IsCode(CardId.MagicianOfChaos, CardId.IllusionOfChaos, CardId.LegendarySwordsman));
            return Bot.Hand.Any(c => c.IsCode(CardId.LegendarySwordsman, CardId.BLS));
        }

        bool PharaohsServantEffect()
        {
            if (Card.Location == CardLocation.Hand && MyTurn) { Pick(CardId.EternalSoul, CardId.DarkMagicalCircle, CardId.MagicianNavigation); return true; }
            // discard a Spell: destroy their backrow
            return Enemy.GetSpellCount() >= 2 && Bot.Hand.Any(c => c.IsSpell());
        }

        // ---------- Knights > Slifer ----------

        int KnightsOnField => Knights.Count(id => Bot.HasInMonstersZone(id));

        bool JokersWildEffect()
        {
            if (Card.Location == CardLocation.Grave) return true;
            // becomes Thunderspeed Summon with three Knights, else Joker's Straight
            if (KnightsOnField == 3 && MaybeInDeck(CardId.Slifer, 1)) Pick(CardId.ThunderspeedSummon);
            else Pick(CardId.JokersStraight, CardId.RoyalStraight);
            Pick(CardId.KingsKnight, CardId.JacksKnight);
            return true;
        }

        bool ThunderspeedEffect()
        {
            if (Bot.HasInHand(CardId.Slifer) && Bot.GetMonsterCount() >= 3) return true;
            if (KnightsOnField == 3) { Pick(CardId.Slifer); return true; }
            return false;
        }

        bool YugiSet()
        {
            // Mind Shuffle and the protection traps go down first; never more than the zones we still need
            if (Card.IsCode(CardId.MindShuffle, CardId.EternalSoul, CardId.ChaosMagicalHats, CardId.ChaosMysticBox)) return Bot.GetSpellCountWithoutField() < 5;
            return SetTrapOrQuickPlay();
        }

        public override bool OnSelectHand()
        {
            return true;
        }
    }
}
