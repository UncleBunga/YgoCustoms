using System.Collections.Generic;
using System.Linq;
using YGOSharp.OCGWrapper.Enums;

namespace WindBot.Game.AI.Decks
{
    // IRL Cyber Dragon, "mixed by hand": the Cyber Dragon engine (Core, Nachster, Herz, Jormungardr, Horizon,
    // Emergency, Machine Duplication) feeds whichever payoff the board supports - Nova into Infinity for control,
    // Cyber End / Rampage through Polymerization or Power Bond, Megafleet against Extra Monster Zone monsters,
    // Cyberdark Dragon through Cyberdark Impact / Overload Fusion, and Limiter Removal / Power Bond for lethal.
    [Deck("IRL_CyberDragon", "AI_IRL_CyberDragon")]
    public class IrlCyberDragonExecutor : CustomExecutorBase
    {
        public class CardId
        {
            public const int Jizukiru = 63941210;
            public const int CyberDragon = 70095154;
            public const int Jormungardr = 19715246;
            public const int CyberdarkWurm = 56100345;
            public const int Drei = 59281922;
            public const int Zwei = 5373478;
            public const int Vier = 29975188;
            public const int CyberdarkChimera = 5370235;
            public const int CyberdarkKeel = 3019642;
            public const int CyberdarkHorn = 41230939;
            public const int CyberdarkEdge = 77625948;
            public const int CyberdarkCannon = 45078193;
            public const int AttachmentCybern = 79875526;
            public const int CyberdarkClaw = 82562802;
            public const int GaleDogra = 16229315;
            public const int Core = 23893227;
            public const int Nachster = 1142880;
            public const int Herz = 56364287;
            public const int CyberPharos = 29719112;
            public const int CyberValley = 3657444;
            public const int OverloadFusion = 3659803;
            public const int FusionDeployment = 6498706;
            public const int Polymerization = 24094653;
            public const int CyberRevsystem = 33041277;
            public const int PowerBond = 37630732;
            public const int FusionTag = 59432181;
            public const int CyberEmergency = 60600126;
            public const int CyberneticHorizon = 63031396;
            public const int MachineDuplication = 63995093;
            public const int EvolutionEndBurst = 78217065;
            public const int CyberdarkImpact = 80033124;
            public const int CyberRepairPlant = 86686671;
            public const int CyberEternal = 32768230;
            public const int CyberloadFusion = 55704856;
            public const int OverloadFutureFusion = 33099732;
            public const int CyberdarkRealm = 64753988;
            public const int FutureFusion = 511002997;
            public const int CyberdarkInferno = 44352516;
            public const int CyberneticRevolution = 18597560;
            public const int CyberneticOverflow = 82428674;
            public const int CyberdarkInvasion = 1157683;

            public const int CyberdarkEndDragon = 37542782;
            public const int CyberEndDragon = 1546123;
            public const int CyberdarknessDragon = 18967507;
            public const int Megafleet = 87116928;
            public const int Overdragon = 64599569;
            public const int CyberTwinDragon = 74157028;
            public const int CyberdarkDragon = 40418351;
            public const int Fortress = 79229522;
            public const int Rampage = 84058253;
            public const int Infinity = 10443957;
            public const int Nova = 58069384;
            public const int Sieger = 46724542;
        }

        // every card whose name is "Cyber Dragon" on the field / in the GY
        static readonly int[] CyberDragonNames = {
            CardId.CyberDragon, CardId.CyberdarkWurm, CardId.Drei, CardId.Vier, CardId.Core, CardId.Nachster, CardId.Herz, CardId.Sieger
        };

        public IrlCyberDragonExecutor(GameAI ai, Duel duel)
            : base(ai, duel)
        {
            // Quick responses
            AddExecutor(ExecutorType.Activate, CardId.Infinity, InfinityEffect);
            AddExecutor(ExecutorType.Activate, CardId.CyberdarknessDragon, () => Duel.LastChainPlayer == 1 || Card.Location != CardLocation.MonsterZone);
            AddExecutor(ExecutorType.Activate, CardId.CyberValley, CyberValleyEffect);
            AddExecutor(ExecutorType.Activate, CardId.CyberdarkInvasion, CyberdarkInvasionEffect);
            AddExecutor(ExecutorType.Activate, CardId.CyberneticOverflow, () => Enemy.GetFieldCount() > 0 && (Duel.LastChainPlayer == 1 || MyTurn));

            // Kaiju answer to a big monster
            AddExecutor(ExecutorType.SpSummon, CardId.Jizukiru, () => Util.GetProblematicEnemyMonster(2500) != null && TargetEnemyMonster());

            // Searchers first
            AddExecutor(ExecutorType.Activate, CardId.CyberEmergency, CyberEmergencyEffect);
            AddExecutor(ExecutorType.Activate, CardId.CyberRepairPlant, () => { Pick(CardId.Nachster, CardId.CyberDragon, CardId.Jormungardr, CardId.Herz); return true; });
            AddExecutor(ExecutorType.Activate, CardId.CyberneticHorizon, HorizonEffect);
            AddExecutor(ExecutorType.Activate, CardId.CyberdarkRealm, () => { Pick(CardId.CyberdarkEdge, CardId.CyberdarkHorn, CardId.CyberdarkKeel); return true; });
            AddExecutor(ExecutorType.Activate, CardId.EvolutionEndBurst, () => !Bot.HasInHand(CardId.OverloadFusion));
            AddExecutor(ExecutorType.Activate, CardId.CyberdarkChimera, () => { Pick(Bot.Hand.Where(c => !c.IsMonster()).OrderBy(CardValue)); return true; });
            AddExecutor(ExecutorType.Activate, CardId.CyberdarkClaw, () => { Pick(CardId.CyberdarkRealm, CardId.CyberdarkInferno, CardId.CyberdarkImpact); return true; });
            AddExecutor(ExecutorType.Activate, CardId.CyberdarkCannon, () => { Pick(CardId.CyberdarkEdge, CardId.CyberdarkHorn, CardId.CyberdarkKeel); return true; });
            AddStaples();

            // Normal Summon the searcher before the extenders
            AddExecutor(ExecutorType.Summon, CardId.Core, NormalSummon);
            AddExecutor(ExecutorType.Activate, CardId.Core, CoreEffect);

            // Extenders
            AddExecutor(ExecutorType.SpSummon, CardId.CyberDragon, () => CountSummon());
            AddExecutor(ExecutorType.Activate, CardId.Jormungardr, JormungardrEffect);
            AddExecutor(ExecutorType.Activate, CardId.Nachster, NachsterEffect);
            AddExecutor(ExecutorType.Activate, CardId.Vier);
            AddExecutor(ExecutorType.Activate, CardId.Herz, () => { Pick(CardId.CyberDragon, CardId.Nachster); return true; });
            AddExecutor(ExecutorType.Activate, CardId.CyberRevsystem, () => { Pick(CardId.CyberDragon); return true; });
            AddExecutor(ExecutorType.Activate, CardId.MachineDuplication, MachineDuplicationEffect);
            AddExecutor(ExecutorType.Activate, CardId.CyberdarkWurm, () => { Pick(CardId.CyberDragon, CardId.Herz); return true; });
            AddExecutor(ExecutorType.Activate, CardId.FusionDeployment, () => { Pick(CardId.CyberEndDragon); Pick(CardId.CyberDragon); return true; });
            AddExecutor(ExecutorType.Activate, CardId.AttachmentCybern, () => Bot.GetMonsters().Any(m => m.IsFaceup() && m.HasRace(CardRace.Machine)));
            AddExecutor(ExecutorType.SpSummon, CardId.CyberPharos, PharosSummon);
            AddExecutor(ExecutorType.Activate, CardId.CyberPharos, PharosEffect);
            AddExecutor(ExecutorType.Activate, CardId.CyberEternal, () => { Pick(CardId.CyberEndDragon, CardId.Rampage, CardId.CyberTwinDragon); return true; });

            // Payoffs
            AddExecutor(ExecutorType.SpSummon, CardId.Megafleet, MegafleetSummon);
            AddExecutor(ExecutorType.Activate, CardId.CyberdarkImpact);
            AddExecutor(ExecutorType.Activate, CardId.CyberneticRevolution, () => { Pick(CardId.CyberEndDragon); return true; });
            AddExecutor(ExecutorType.Activate, CardId.PowerBond, PowerBondEffect);
            AddExecutor(ExecutorType.Activate, CardId.Polymerization, PolymerizationEffect);
            AddExecutor(ExecutorType.Activate, CardId.CyberloadFusion, () => { Pick(CardId.CyberEndDragon, CardId.Rampage, CardId.CyberTwinDragon); return true; });
            AddExecutor(ExecutorType.Activate, CardId.OverloadFusion, OverloadFusionEffect);
            AddExecutor(ExecutorType.Activate, CardId.OverloadFutureFusion, OverloadFutureFusionEffect);
            // 3000 LP with no once-per-turn: never through the generic rule (it paid 6000 LP on turn 1)
            AddExecutor(ExecutorType.Activate, CardId.GaleDogra, () => false);
            AddExecutor(ExecutorType.Activate, CardId.FutureFusion, () => { Pick(CardId.CyberEndDragon); return true; });
            AddExecutor(ExecutorType.SpSummon, CardId.Nova, NovaSummon);
            AddExecutor(ExecutorType.SpSummon, CardId.Infinity, () => Bot.HasInMonstersZone(CardId.Nova));
            AddExecutor(ExecutorType.Activate, CardId.Nova, () => Card.Location == CardLocation.MonsterZone && Bot.HasInGraveyard(CardId.CyberDragon));
            AddExecutor(ExecutorType.SpSummon, CardId.Sieger, () => Bot.GetMonsters().Count(m => m.IsFaceup() && m.HasRace(CardRace.Machine)) >= 2
                && !Bot.HasInExtra(CardId.Nova) || Bot.GetMonsters().Count(m => m.Level == 5) < 2);
            AddExecutor(ExecutorType.Activate, CardId.Rampage, () => { Pick(Enemy.GetSpells()); return true; });
            AddExecutor(ExecutorType.Activate, CardId.CyberdarkEndDragon, TargetEnemyGraveMonster);
            AddExecutor(ExecutorType.Activate, CardId.CyberdarkDragon, () => { Pick(Bot.Graveyard.Where(c => c.HasRace(CardRace.Dragon)).OrderByDescending(c => c.Attack)); return true; });

            // Cyberdark beaters
            AddExecutor(ExecutorType.Summon, CardId.CyberdarkEdge, () => CyberdarkSummon());
            AddExecutor(ExecutorType.Summon, CardId.CyberdarkHorn, () => CyberdarkSummon());
            AddExecutor(ExecutorType.Summon, CardId.CyberdarkKeel, () => CyberdarkSummon());
            AddExecutor(ExecutorType.Activate, CardId.CyberdarkEdge, EquipDragonFromGY);
            AddExecutor(ExecutorType.Activate, CardId.CyberdarkHorn, EquipDragonFromGY);
            AddExecutor(ExecutorType.Activate, CardId.CyberdarkKeel, EquipDragonFromGY);
            AddExecutor(ExecutorType.Activate, CardId.CyberdarkInferno, () => !Bot.HasInSpellZone(CardId.CyberdarkInferno, true));

            AddExecutor(ExecutorType.Summon, CardId.Drei, NormalSummon);
            AddExecutor(ExecutorType.Summon, CardId.Zwei, NormalSummon);
            AddExecutor(ExecutorType.Summon, CardId.Nachster, NormalSummon);
            AddExecutor(ExecutorType.Summon, CardId.Herz, NormalSummon);
            AddExecutor(ExecutorType.SummonOrSet, () => BestFallbackSummon() && NormalSummon());

            AddExecutor(ExecutorType.SpellSet, SetTrapOrQuickPlay);
            AddExecutor(ExecutorType.Repos, DefaultMonsterRepos);
            AddExecutor(ExecutorType.Activate, GenericActivate);
        }

        protected override int[] AddToHandPriority => new[] {
            CardId.CyberDragon, CardId.Nachster, CardId.Core, CardId.Jormungardr, CardId.Herz, CardId.CyberEmergency,
            CardId.CyberneticHorizon, CardId.PowerBond, CardId.Polymerization, CardId.OverloadFusion, CardId.CyberRevsystem,
            CardId.MachineDuplication, CardId.CyberneticOverflow, CardId.CyberdarkEdge, CardId.CyberdarkRealm
        };
        protected override int[] SpSummonPriority => new[] {
            CardId.CyberEndDragon, CardId.CyberdarkEndDragon, CardId.Rampage, CardId.CyberTwinDragon, CardId.CyberdarkDragon,
            CardId.CyberDragon, CardId.Nachster, CardId.Vier, CardId.Herz, CardId.Core
        };
        protected override int[] ExpendablePriority => new[] {
            CardId.Herz, CardId.CyberdarkCannon, CardId.CyberdarkClaw, CardId.GaleDogra, CardId.CyberValley, CardId.Zwei,
            CardId.Drei, CardId.CyberdarkChimera, CardId.Core
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

        bool CountSummon() => true;

        bool IsCyberDragon(ClientCard c) => c != null && c.IsCode(CyberDragonNames);
        int CyberDragonsOnField => Bot.GetMonsters().Count(m => m.IsFaceup() && IsCyberDragon(m));
        int CyberDragonsForFusion => CyberDragonsOnField + Bot.Hand.Count(c => c.IsCode(CardId.CyberDragon, CardId.Vier, CardId.Drei, CardId.Core, CardId.Nachster, CardId.Herz, CardId.CyberdarkWurm));
        bool EnemyHasMonster => Enemy.GetMonsterCount() > 0;

        // ---------- engine ----------

        bool CyberEmergencyEffect()
        {
            if (!summonedThisTurn && !Bot.HasInHand(CardId.Core)) Pick(CardId.Core, CardId.Nachster, CardId.CyberDragon);
            else if (EnemyHasMonster && Bot.GetMonsterCount() == 0 && !Bot.HasInHand(CardId.CyberDragon)) Pick(CardId.CyberDragon, CardId.Jormungardr, CardId.Nachster);
            else Pick(CardId.Nachster, CardId.Jormungardr, CardId.CyberDragon, CardId.Herz);
            return true;
        }

        bool HorizonEffect()
        {
            // hand: a DARK Cyber (Cyberdark) or the least useful Cyber; Deck: Herz (it searches a Cyber Dragon when sent)
            var handCost = Bot.Hand.Where(c => c.IsMonster() && c.HasRace(CardRace.Machine | CardRace.Dragon))
                .OrderBy(c => c.HasAttribute(CardAttribute.Dark) ? 0 : 1).ThenBy(c => Rank(ExpendablePriority, c)).FirstOrDefault();
            if (handCost == null) return false;
            Pick(handCost);
            if (handCost.HasAttribute(CardAttribute.Dark)) Pick(CardId.Herz, CardId.CyberDragon, CardId.Core);
            else Pick(CardId.CyberdarkChimera, CardId.CyberdarkClaw, CardId.CyberdarkCannon, CardId.CyberdarkEdge);
            Pick(CardId.Nachster, CardId.Jormungardr, CardId.CyberDragon);
            Pick(CardId.CyberEndDragon, CardId.Overdragon, CardId.Rampage);
            return true;
        }

        bool CoreEffect()
        {
            if (Card.Location == CardLocation.Grave) { Pick(CardId.CyberDragon, CardId.Nachster, CardId.Vier); return true; }
            if (!Bot.HasInHand(CardId.CyberEmergency) && MaybeInDeck(CardId.CyberEmergency, 1)) Pick(CardId.CyberEmergency);
            else Pick(CardId.CyberneticHorizon, CardId.CyberRevsystem, CardId.OverloadFusion, CardId.CyberneticOverflow, CardId.CyberEmergency);
            return true;
        }

        bool JormungardrEffect()
        {
            if (Card.Location == CardLocation.Hand) return EnemyHasMonster;
            // return a Cyber Dragon to hand for Polymerization only if we still have Fusion bodies
            return !Bot.HasInHand(CardId.Polymerization) && !Bot.HasInHand(CardId.PowerBond) && CyberDragonsOnField >= 2;
        }

        bool NachsterEffect()
        {
            if (Card.Location == CardLocation.Hand)
            {
                var discard = Bot.Hand.Where(c => c.IsMonster() && !c.IsCode(CardId.Nachster)).OrderBy(c => Rank(ExpendablePriority, c)).ThenBy(CardValue).FirstOrDefault();
                if (discard == null) return false;
                Pick(discard);
                return true;
            }
            Pick(CardId.Herz, CardId.CyberDragon); // Herz in the GY searches another Cyber Dragon
            return true;
        }

        // Pharos: tribute a Machine (ideally Herz, or a Cyber Dragon that Repair Plant / Revsystem / Nachster then use
        // from the GY), then Fusion Summon from hand and field.
        bool PharosSummon()
        {
            var tribute = Bot.GetMonsters().Where(m => m.IsFaceup() && m.HasRace(CardRace.Machine) && m.Attack < 2500)
                .OrderBy(m => m.IsCode(CardId.Herz) ? 0 : IsCyberDragon(m) ? 1 : 2).ThenBy(CardValue).FirstOrDefault();
            if (tribute == null) return false;
            bool gyPayoff = Bot.HasInHand(CardId.CyberRepairPlant) || Bot.HasInHand(CardId.CyberRevsystem) || Bot.HasInHand(CardId.Nachster)
                || tribute.IsCode(CardId.Herz);
            bool fusionNow = CyberDragonsForFusion - (IsCyberDragon(tribute) ? 1 : 0) >= 2;
            if (!gyPayoff && !fusionNow) return false;
            AI.SelectMaterials(tribute);
            Pick(tribute);
            return true;
        }

        bool PharosEffect()
        {
            if (Card.Location == CardLocation.Grave) return true; // Power Bond search
            if (!MyTurn) return false;
            // wait for Repair Plant / Revsystem to bring more Cyber Dragons first
            if (CyberDragonsForFusion < 3 && (Bot.HasInHand(CardId.CyberRevsystem) || Bot.HasInHand(CardId.CyberRepairPlant))
                && Bot.Graveyard.Any(IsCyberDragon)) return false;
            return PolymerizationEffect();
        }

        bool MachineDuplicationEffect()
        {
            var t = Bot.GetMonsters().Where(m => m.IsFaceup() && m.HasRace(CardRace.Machine) && m.Attack <= 500 && IsCyberDragon(m)).FirstOrDefault();
            if (t == null) return false;
            Pick(t);
            return true;
        }

        bool CyberdarkSummon()
        {
            if (!Bot.Graveyard.Any(c => c.HasRace(CardRace.Dragon) && c.Level <= 3) && !Bot.HasInSpellZone(CardId.CyberdarkRealm, true)) return false;
            return NormalSummon();
        }

        bool EquipDragonFromGY()
        {
            var pool = Bot.Graveyard.Where(c => c.HasRace(CardRace.Dragon) && c.Level <= 3).ToList();
            if (Bot.HasInSpellZone(CardId.CyberdarkRealm, true)) pool.AddRange(Enemy.Graveyard.Where(c => c.HasRace(CardRace.Dragon) && c.Level <= 3));
            var t = pool.OrderByDescending(c => c.Attack).FirstOrDefault();
            if (t == null) return false;
            Pick(t);
            return true;
        }

        bool CyberValleyEffect()
        {
            if (Duel.Phase == DuelPhase.BattleStep || Duel.Phase == DuelPhase.BattleStart) return !MyTurn;
            return false;
        }

        bool CyberdarkInvasionEffect()
        {
            if (Duel.LastChainPlayer == 0) return false;
            return Enemy.GetFieldCount() > 0 && TargetEnemyCard();
        }

        bool InfinityEffect()
        {
            if (Duel.CurrentChain.Count > 0) return Duel.LastChainPlayer == 1;
            var t = Enemy.GetMonsters().Where(m => m.IsFaceup() && m.IsAttack()).OrderByDescending(m => m.Attack).FirstOrDefault();
            if (t == null) return false;
            Pick(t);
            return true;
        }

        bool TargetEnemyGraveMonster()
        {
            var t = Enemy.Graveyard.Concat(Bot.Graveyard).Where(c => c.IsMonster()).OrderByDescending(c => c.Attack).FirstOrDefault();
            if (t == null) return false;
            Pick(t);
            return true;
        }

        // ---------- payoffs ----------

        int EnemyLP => Enemy.LifePoints;
        int OurAttack => Bot.GetMonsters().Where(m => m.IsFaceup() && m.IsAttack()).Sum(m => m.Attack);

        bool MegafleetSummon()
        {
            // removes monsters sitting in the Extra Monster Zones (theirs included)
            var emz = Enemy.GetMonstersInExtraZone();
            if (emz.Count == 0 || CyberDragonsOnField == 0) return false;
            var cyber = Bot.GetMonsters().Where(m => m.IsFaceup() && IsCyberDragon(m)).OrderBy(CardValue).First();
            AI.SelectMaterials(new List<ClientCard> { cyber }.Concat(emz).ToList());
            return true;
        }

        bool NovaSummon()
        {
            var lv5 = Bot.GetMonsters().Where(m => m.IsFaceup() && m.Level == 5 && m.HasRace(CardRace.Machine)).OrderBy(CardValue).Take(2).ToList();
            if (lv5.Count < 2) return false;
            // keep three Cyber Dragons for Cyber End if a Fusion spell is in hand
            if ((Bot.HasInHand(CardId.PowerBond) || Bot.HasInHand(CardId.Polymerization)) && CyberDragonsForFusion >= 3) return false;
            AI.SelectMaterials(lv5);
            return true;
        }

        IList<ClientCard> FusionMaterialsFor(int count)
        {
            return Bot.GetMonsters().Where(m => m.IsFaceup() && IsCyberDragon(m))
                .Concat(Bot.Hand.Where(c => c.IsCode(CardId.CyberDragon, CardId.Vier, CardId.Drei, CardId.Core, CardId.Nachster, CardId.Herz, CardId.CyberdarkWurm)))
                .OrderBy(c => c.Location == CardLocation.Hand ? 0 : 1).ThenBy(CardValue).Take(count).ToList();
        }

        bool PowerBondEffect()
        {
            // Power Bond doubles ATK and burns us at End Phase: use it to win, or when we can afford the damage
            if (!MyTurn || Duel.Phase != DuelPhase.Main1) return false;
            int n = CyberDragonsForFusion;
            if (n >= 3)
            {
                bool lethalish = 8000 >= EnemyLP || Bot.LifePoints > 4500;
                if (!lethalish) return false;
                Pick(CardId.CyberEndDragon);
                AI.SelectMaterials(FusionMaterialsFor(3));
                return true;
            }
            if (n >= 2 && (5600 >= EnemyLP || Bot.LifePoints > 3500))
            {
                Pick(CardId.CyberTwinDragon, CardId.Rampage);
                AI.SelectMaterials(FusionMaterialsFor(2));
                return true;
            }
            return false;
        }

        bool PolymerizationEffect()
        {
            if (!MyTurn) return false;
            int n = CyberDragonsForFusion;
            if (n >= 3) { Pick(CardId.CyberEndDragon); AI.SelectMaterials(FusionMaterialsFor(3)); return true; }
            if (n >= 2)
            {
                if (Enemy.GetSpellCount() >= 2) Pick(CardId.Rampage, CardId.CyberTwinDragon);
                else Pick(CardId.CyberTwinDragon, CardId.Rampage);
                AI.SelectMaterials(FusionMaterialsFor(2));
                return true;
            }
            return false;
        }

        bool OverloadFusionEffect()
        {
            if (!MyTurn) return false;
            int machinesInGY = Bot.Graveyard.Count(c => c.HasRace(CardRace.Machine) && c.IsMonster());
            bool cyberdarkSet = new[] { CardId.CyberdarkHorn, CardId.CyberdarkEdge, CardId.CyberdarkKeel }
                .All(id => Bot.Graveyard.Any(c => c.IsCode(id)) || Bot.GetMonsters().Any(c => c.IsCode(id)));
            if (cyberdarkSet) { Pick(CardId.CyberdarkDragon); return true; }
            // Overdragon: every Machine in the GY as material (800 ATK and one attack each). It sends the rest of our
            // field to the GY, so only when it beats what we already have.
            var mats = Bot.Graveyard.Where(c => c.IsMonster() && c.HasRace(CardRace.Machine)).ToList();
            if (mats.Any(IsCyberDragon) && mats.Count >= 4)
            {
                int atk = mats.Count * 800;
                int best = Enemy.GetMonsters().Where(m => m.IsFaceup()).Select(m => m.Attack).DefaultIfEmpty(0).Max();
                bool ourBoardWorse = !Bot.GetMonsters().Any(m => m.Attack >= atk || m.Attack >= 2800);
                if (ourBoardWorse && (atk > best || atk >= EnemyLP))
                {
                    Pick(CardId.Overdragon);
                    AI.SelectMaterials(mats);
                    Pick(mats);
                    return true;
                }
            }
            return false;
        }

        bool OverloadFutureFusionEffect()
        {
            if (Card.Location == CardLocation.Grave) return OverloadFusionEffect();
            Pick(CardId.CyberEndDragon);
            Pick(CardId.Herz, CardId.CyberDragon); // Herz in the GY searches another Cyber Dragon
            return true;
        }

        public override bool OnSelectHand()
        {
            return false; // Cyber Dragon wants the opponent to have a monster
        }
    }
}
