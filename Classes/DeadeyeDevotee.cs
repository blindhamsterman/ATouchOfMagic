// Copyright (c) 2019 Alisdair Smith
// This code is licensed under MIT license (see LICENSE for details)

using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using System;
using Kingmaker.Utility;
using Kingmaker.Enums.Damage;
using Kingmaker.UnitLogic;
using System.Collections.Generic;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;
using Kingmaker.UnitLogic.Alignments;
using CallOfTheWild;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.Designers;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;

namespace ATouchOfMagic
{
    static class DeadeyeDevoteeClass
    {
        static LibraryScriptableObject library => Main.library;
        static internal BlueprintCharacterClass deadeyeDevotee;
        // static internal BlueprintCharacterClass[] deadeyeDevoteeArray;
        static internal BlueprintProgression deadeyeDevoteeProgression;
        static internal BlueprintFeature energyArrow;
        static internal BlueprintFeatureSelection divineSpellbookSelection;
        static internal BlueprintFeature deadeyeDevoteeProficiencies;




        internal static void CreateDeadeyeDevoteeClass()
        {
            var library = Main.library;
            if (DeadeyeDevoteeClass.deadeyeDevotee != null) return;

            deadeyeDevotee = Helpers.Create<BlueprintCharacterClass>();

            deadeyeDevotee.name = "DeadeyeDevoteeClass";
            library.AddAsset(deadeyeDevotee, "");
            deadeyeDevotee.LocalizedName = Helpers.CreateString("deadeyeDevotee.Name", "Deadeye Devotee");
            deadeyeDevotee.LocalizedDescription = Helpers.CreateString("deadeyeDevotee.Description",
                "Dedicated followers of Erastil become closer to their god by mastering archery and caring for each other. They can tap into the power of the Elk Father, who guides and empowers their arrows. ");
            // Matched Druid skill progression
            deadeyeDevotee.SkillPoints = library.Get<BlueprintCharacterClass>("610d836f3a3a9ed42a4349b62f002e96").SkillPoints;
            deadeyeDevotee.HitDie = DiceType.D10;
            deadeyeDevotee.PrestigeClass = true;

            var pointBlankShot = library.Get<BlueprintFeature>("0da0c194d6e1d43419eb8d990b28e0ab"); // Point Blank Shot
            var preciseShot = library.Get<BlueprintFeature>("8f3d1e6b4be006f4d896081f2f889665"); // Precise Shot
            var weaponFocus = library.Get<BlueprintParametrizedFeature>("1e1f627d26ad36f43bbd26cc2bf8ac7e"); // Weapon Focus;

            var lbow = WeaponCategory.Longbow;
            deadeyeDevotee.SetComponents(
                pointBlankShot.PrerequisiteFeature(),
                preciseShot.PrerequisiteFeature(),
                Common.createPrerequisiteParametrizedFeatureWeapon(weaponFocus, lbow, any: true),
                StatType.BaseAttackBonus.PrerequisiteStatValue(6),
                Common.createPrerequisiteAlignment(AlignmentMaskType.LawfulGood | AlignmentMaskType.NeutralGood | AlignmentMaskType.LawfulNeutral),
                Helpers.Create<PrerequisiteCasterTypeSpellLevel>(p => { p.IsArcane = false; p.RequiredSpellLevel = 1; p.Group = Prerequisite.GroupType.All; }));


            // Used ranger stats as they seem to fit the theme pretty well
            var ranger = library.Get<BlueprintCharacterClass>("cda0615668a6df14eb36ba19ee881af6");
            var wizard = Helpers.GetClass("ba34257984f4c41408ce1dc2004e342e");
            deadeyeDevotee.BaseAttackBonus = ArcaneArcherClass.arcaneArcher.BaseAttackBonus; // BAB high
            deadeyeDevotee.FortitudeSave = ArcaneArcherClass.arcaneArcher.FortitudeSave;
            deadeyeDevotee.ReflexSave = ArcaneArcherClass.arcaneArcher.ReflexSave;
            deadeyeDevotee.WillSave = ArcaneArcherClass.arcaneArcher.WillSave;
            deadeyeDevotee.IsArcaneCaster = false;
            deadeyeDevotee.IsDivineCaster = true;

            // Perception (Wis), Ride (Dex), Stealth (Dex), and Survival (Wis).
            // knowledge nature in place of survival, there is no replacement for Ride
            deadeyeDevotee.ClassSkills = new StatType[] {
                StatType.SkillStealth,
                StatType.SkillLoreNature,
                StatType.SkillPerception
            };

            deadeyeDevotee.StartingGold = ranger.StartingGold;
            deadeyeDevotee.PrimaryColor = ranger.PrimaryColor;
            deadeyeDevotee.SecondaryColor = ranger.SecondaryColor;

            deadeyeDevotee.RecommendedAttributes = new StatType[] { StatType.Dexterity };
            deadeyeDevotee.NotRecommendedAttributes = Array.Empty<StatType>();

            deadeyeDevotee.EquipmentEntities = wizard.EquipmentEntities;
            deadeyeDevotee.MaleEquipmentEntities = wizard.MaleEquipmentEntities;
            deadeyeDevotee.FemaleEquipmentEntities = wizard.FemaleEquipmentEntities;

            deadeyeDevotee.StartingItems = ranger.StartingItems;

            createDeadeyeDevoteeProgression();
            deadeyeDevotee.Progression = deadeyeDevoteeProgression;

            // Arcane archers do not gets spells at levels 1,5 and 9, we handle level 1 by giving spellbook selection at level 2
            // we handle 5 and 9 by adding a skip levels for spell progression component to progressiom.
            var skipLevels = new List<int>();
            skipLevels.Add(5);
            skipLevels.Add(9);
            deadeyeDevotee.AddComponent(Helpers.Create<SkipLevelsForSpellProgression>(s => s.Levels = skipLevels.ToArray()));
            deadeyeDevotee.AddComponent(PrerequisiteNoClassLevel(ArcaneArcherClass.arcaneArcher));
            Helpers.RegisterClass(deadeyeDevotee);
            // ensure the player cannot have both arcane archer and deadeye devotee 
            ArcaneArcherClass.arcaneArcher.AddComponent(PrerequisiteNoClassLevel(DeadeyeDevoteeClass.deadeyeDevotee));
        }

        public static PrerequisiteNoClassLevel PrerequisiteNoClassLevel(this BlueprintCharacterClass @class, bool any = false)
        {
            var result = Helpers.Create<PrerequisiteNoClassLevel>();
            result.CharacterClass = @class;
            result.Group = any ? Prerequisite.GroupType.Any : Prerequisite.GroupType.All;
            return result;
        }


        static BlueprintCharacterClass[] getDeadeyeDevoteeArray()
        {
            return new BlueprintCharacterClass[] { deadeyeDevotee };
        }

        static void createDeadeyeDevoteeProgression()
        {

            var allowed_weapons = new BlueprintWeaponType[4];
            allowed_weapons[0] = library.Get<BlueprintWeaponType>("99ce02fb54639b5439d07c99c55b8542"); // shortbow
            allowed_weapons[1] = library.Get<BlueprintWeaponType>("7a1211c05ec2c46428f41e3c0db9423f"); // longbow
            allowed_weapons[2] = library.Get<BlueprintWeaponType>("1ac79088a7e5dde46966636a3ac71c35"); // composite longbow
            allowed_weapons[3] = library.Get<BlueprintWeaponType>("011f6f86a0b16df4bbf7f40878c3e80b"); // composite shortbow

            CreateEnergyArrow();
            CreateDivineSpellbookSelection();
            CreateDeadeyeDevoteeProficiencies();
            CreateDeadeyeDevoteeFeatures();


            deadeyeDevoteeProgression = Helpers.CreateProgression("DeadeyeDevoteeProgression",
                            deadeyeDevotee.Name,
                            deadeyeDevotee.Description,
                            "",
                            deadeyeDevotee.Icon,
                            FeatureGroup.None);
            deadeyeDevoteeProgression.Classes = getDeadeyeDevoteeArray();

            deadeyeDevoteeProgression.LevelEntries = new LevelEntry[] {
                Helpers.LevelEntry(1, deadeyeDevoteeProficiencies, ArcaneArcherClass.enhanceArrowsMagic),
                Helpers.LevelEntry(2, Hinterlander.imbue_arrow, divineSpellbookSelection),
                Helpers.LevelEntry(3, ArcaneArcherClass.enhanceArrowsElemental),
                Helpers.LevelEntry(4, energyArrow),
                Helpers.LevelEntry(5, ArcaneArcherClass.arcaneArcherFeat), // Distant arrows aren't possible, providing a feat for this level seems reasonable seeing as the class also doesn't get spellcasting here.
                Helpers.LevelEntry(6, ArcaneArcherClass.phaseArrow),
                Helpers.LevelEntry(7, ArcaneArcherClass.enhanceArrowsBurst),
                Helpers.LevelEntry(8, ArcaneArcherClass.hailOfArrows),
                Helpers.LevelEntry(9, ArcaneArcherClass.enhanceArrowsAligned),
                Helpers.LevelEntry(10, ArcaneArcherClass.arrowOfDeath)
            };

            deadeyeDevoteeProgression.UIDeterminatorsGroup = new BlueprintFeatureBase[] { deadeyeDevoteeProficiencies };
            deadeyeDevoteeProgression.UIGroups = new UIGroup[]  {
                                                         Helpers.CreateUIGroup(energyArrow, ArcaneArcherClass.phaseArrow, ArcaneArcherClass.hailOfArrows, ArcaneArcherClass.arrowOfDeath),
                                                         Helpers.CreateUIGroup(ArcaneArcherClass.enhanceArrowsMagic, ArcaneArcherClass.enhanceArrowsElemental, ArcaneArcherClass.enhanceArrowsBurst, ArcaneArcherClass.enhanceArrowsAligned),
                                                         Helpers.CreateUIGroup(divineSpellbookSelection, Hinterlander.imbue_arrow, ArcaneArcherClass.arcaneArcherFeat)
                                                        };
        }


        static void CreateDeadeyeDevoteeFeatures()
        {
            //expanded arrows feat
            ArcaneArcherClass.expandedEnhanceArrows.AddComponent(Helpers.CreateAddFeatureOnClassLevel(ArcaneArcherClass.corrosiveArrowsFeature, 3, getDeadeyeDevoteeArray(), null, false));
            ArcaneArcherClass.expandedEnhanceArrows.AddComponent(Helpers.CreateAddFeatureOnClassLevel(ArcaneArcherClass.specialArrowsFeature, 5, getDeadeyeDevoteeArray(), null, false));

        }

        static void CreateDeadeyeDevoteeProficiencies()
        {
            deadeyeDevoteeProficiencies = library.CopyAndAdd<BlueprintFeature>(
            "c5e479367d07d62428f2fe92f39c0341", // ranger proficiencies
            "DeadeyeDevoteeProficiencies",
            "");
            deadeyeDevoteeProficiencies.SetName("Deadeye Devotee Proficiencies");
            deadeyeDevoteeProficiencies.SetDescription("A deadeye devotee is proficient with all simple and martial weapons, light armor, medium armor, and shields");

        }

        static void CreateDivineSpellbookSelection()
        {
            var comps = new List<BlueprintComponent>();
            var compsArray = comps.ToArray();
            divineSpellbookSelection = Helpers.CreateFeatureSelection("DeadeyeDevoteeSpellbookSelection",
            "Divine Spellcasting",
            $"At 2nd level, and at every level thereafter, with an exception for 5th and 9th levels, " +
                                       "a deadeye devotee gains new spells per day as if he had also gained a level in a divine spellcasting " +
                                       "class he belonged to before adding the prestige class. He does not, however, gain any other benefit a " +
                                       "character of that class would have gained, except for additional spells per day, spells known, and an " +
                                       "increased effective level of spellcasting. If a character had more than one divine spellcasting class " +
                                       "before becoming a deadeye devotee, he must decide to which class he adds the new level for purposes of " +
                                       "determining spells per day.",
                                       "",
                                       CallOfTheWild.LoadIcons.Image2Sprite.Create(@"FeatIcons/Icon_Casting_Combat.png"),
                                       FeatureGroup.EldritchKnightSpellbook, compsArray);
            Common.addSpellbooksToSpellSelection("Deadeye Devotee", 1, divineSpellbookSelection, divine: true, arcane: false, alchemist: false);
        }
        static void CreateEnergyArrow()
        {
            energyArrow = Helpers.CreateFeature("EnergyArrowFeature",
                                                "Energy Arrow",
                                                "At 4th level, when casting either a cure or inflict spell, a deadeye devotee can create an arrow composed of positive or negative energy. This ability allows the devotee to use the bow's range rather than the spell's range to make a ranged touch attack. The physical damage that would be dealt from a mundane arrow is converted into additional damage to the inflict spell or into additional healing for a cure spell. A spell cast in this way is a standard action and the deadeye devotee can fire the arrow as part of the casting. If the arrow misses, the spell is wasted. ",
                                                "",
                                                CallOfTheWild.LoadIcons.Image2Sprite.Create(@"ArcaneArcher/enhanceArrowsHoly.png"),
                                                FeatureGroup.None,
                                                Helpers.Create<CallOfTheWild.SpellManipulationMechanics.FactStoreSpell>(f => f.ignore_target_checkers = true));

            int maxVariants = 6; //due to ui limitation

            var inflictSpellArray = new List<BlueprintAbility>();
            var cureSpellArray = new List<BlueprintAbility>();
            inflictSpellArray.Add(library.Get<BlueprintAbility>("651110ed4f117a948b41c05c5c7624c0")); //inflictCriticalWounds
            inflictSpellArray.Add(library.Get<BlueprintAbility>("5ee395a2423808c4baf342a4f8395b19")); //inflictCriticalWoundsMass
            inflictSpellArray.Add(library.Get<BlueprintAbility>("e5af3674bb241f14b9a9f6b0c7dc3d27")); //inflictLightWounds
            inflictSpellArray.Add(library.Get<BlueprintAbility>("9da37873d79ef0a468f969e4e5116ad2")); //inflictLightWoundsMass
            inflictSpellArray.Add(library.Get<BlueprintAbility>("65f0b63c45ea82a4f8b8325768a3832d")); //inflictModerateWounds
            inflictSpellArray.Add(library.Get<BlueprintAbility>("03944622fbe04824684ec29ff2cec6a7")); //inflictModerateWoundsMass
            inflictSpellArray.Add(library.Get<BlueprintAbility>("bd5da98859cf2b3418f6d68ea66cabbe")); //inflictSeriousWounds
            inflictSpellArray.Add(library.Get<BlueprintAbility>("820170444d4d2a14abc480fcbdb49535")); //inflictSeriousWoundsMass
            cureSpellArray.Add(library.Get<BlueprintAbility>("41c9016596fe1de4faf67425ed691203")); //cureCriticalWounds
            cureSpellArray.Add(library.Get<BlueprintAbility>("1f173a16120359e41a20fc75bb53d449")); //cureCriticalWoundsMass
            cureSpellArray.Add(library.Get<BlueprintAbility>("5590652e1c2225c4ca30c4a699ab3649")); //cureLightWounds
            cureSpellArray.Add(library.Get<BlueprintAbility>("5d3d689392e4ff740a761ef346815074")); //cureLightWoundsMass
            cureSpellArray.Add(library.Get<BlueprintAbility>("6b90c773a6543dc49b2505858ce33db5")); //cureModerateWounds
            cureSpellArray.Add(library.Get<BlueprintAbility>("571221cc141bc21449ae96b3944652aa")); //cureModerateWoundsMass
            cureSpellArray.Add(library.Get<BlueprintAbility>("3361c5df793b4c8448756146a88026ad")); //cureSeriousWounds
            cureSpellArray.Add(library.Get<BlueprintAbility>("0cea35de4d553cc439ae80b3a8724397")); //cureSeriousWoundsMass

            Predicate<AbilityData> checkSlotPredicateI = delegate (AbilityData spell)
            {
                return (inflictSpellArray.Contains(spell.Blueprint))
                        && (!spell.Blueprint.HasVariants || spell.Variants.Count < maxVariants)
                        && (!spell.RequireMaterialComponent || spell.HasEnoughMaterialComponent);
            };

            Predicate<AbilityData> checkSlotPredicateC = delegate (AbilityData spell)
            {
                return (cureSpellArray.Contains(spell.Blueprint))
                        && (!spell.Blueprint.HasVariants || spell.Variants.Count < maxVariants)
                        && (!spell.RequireMaterialComponent || spell.HasEnoughMaterialComponent);
            };

            var undeadType = library.Get<BlueprintFeature>("734a29b693e9ec346ba2951b27987e33");
            var dice = Helpers.CreateContextDiceValue(DiceType.D8, Common.createSimpleContextValue(1), Helpers.CreateContextValue(AbilityRankType.DamageBonus));
            var healAction = Common.createContextActionHealTarget(dice);
            var damageUndeadAction = Helpers.CreateActionDealDamage(DamageEnergyType.PositiveEnergy, dice);
            var damageLivingAction = Helpers.CreateActionDealDamage(DamageEnergyType.NegativeEnergy, dice);

            var hitInflictAction = Helpers.CreateActionList(Helpers.Create<CallOfTheWild.SpellManipulationMechanics.ReleaseSpellStoredInSpecifiedBuff>(r => r.fact = energyArrow), Helpers.CreateConditional(Common.createContextConditionHasFact(undeadType),
                            healAction,
                            damageLivingAction));
            var hitCureAction = Helpers.CreateActionList(Helpers.Create<CallOfTheWild.SpellManipulationMechanics.ReleaseSpellStoredInSpecifiedBuff>(r => r.fact = energyArrow), Helpers.CreateConditional(Common.createContextConditionHasFact(undeadType),
                            damageUndeadAction,
                            healAction));
            var missAction = Helpers.CreateActionList(Helpers.Create<CallOfTheWild.SpellManipulationMechanics.ClearSpellStoredInSpecifiedBuff>(r => r.fact = energyArrow));


            for (int i = 0; i < maxVariants; i++)
            {
                var energyArrowInflicAbility = Helpers.CreateAbility($"DeadeyeDevoteeInflictEnergyArrow{i + 1}Ability",
                                                          energyArrow.Name,
                                                          energyArrow.Description,
                                                          "",
                                                          CallOfTheWild.LoadIcons.Image2Sprite.Create(@"ArcaneArcher/enhanceArrowsUnholy.png"),
                                                          AbilityType.Supernatural,
                                                          CommandType.Standard,
                                                          AbilityRange.Weapon,
                                                          "",
                                                          "",
                                                          Helpers.Create<CallOfTheWild.SpellManipulationMechanics.InferIsFullRoundFromParamSpellSlot>(),
                                                          Helpers.Create<CallOfTheWild.NewMechanics.AttackAnimation>(),
                                                          Helpers.Create<CallOfTheWild.SpellManipulationMechanics.AbilityStoreSpellInFact>(s =>
                                                                                            {
                                                                                                s.fact = energyArrow;
                                                                                                s.check_slot_predicate = checkSlotPredicateI;
                                                                                                s.variant = i;
                                                                                                s.actions = Helpers.CreateActionList(createContextActionFakeTouchAttack(hitInflictAction, missAction));
                                                                                            }),
                                                          Common.createAbilityCasterMainWeaponCheck(WeaponCategory.Longbow, WeaponCategory.Shortbow),
                                                          Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.StatBonus, stat: StatType.Strength, type: AbilityRankType.DamageBonus));
                energyArrowInflicAbility.setMiscAbilityParametersSingleTargetRangedHarmful(works_on_allies: true);
                energyArrowInflicAbility.NeedEquipWeapons = true;

                energyArrow.AddComponent(Helpers.CreateAddFacts(energyArrowInflicAbility));
            }
            for (int i = 0; i < maxVariants; i++)
            {
                var energyArrowCureAbility = Helpers.CreateAbility($"DeadeyeDevoteeCureEnergyArrow{i + 1}Ability",
                                                          energyArrow.Name,
                                                          energyArrow.Description,
                                                          "",
                                                          energyArrow.Icon,
                                                          AbilityType.Supernatural,
                                                          CommandType.Standard,
                                                          AbilityRange.Weapon,
                                                          "",
                                                          "",
                                                          Helpers.Create<CallOfTheWild.SpellManipulationMechanics.InferIsFullRoundFromParamSpellSlot>(),
                                                          Helpers.Create<CallOfTheWild.NewMechanics.AttackAnimation>(),
                                                          Helpers.Create<CallOfTheWild.SpellManipulationMechanics.AbilityStoreSpellInFact>(s =>
                                                                                            {
                                                                                                s.fact = energyArrow;
                                                                                                s.check_slot_predicate = checkSlotPredicateC;
                                                                                                s.variant = i;
                                                                                                s.actions = Helpers.CreateActionList(createContextActionFakeTouchAttack(hitCureAction, missAction));
                                                                                            }),
                                                          Common.createAbilityCasterMainWeaponCheck(WeaponCategory.Longbow, WeaponCategory.Shortbow),
                                                          Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.StatBonus, stat: StatType.Strength, type: AbilityRankType.DamageBonus));
                energyArrowCureAbility.setMiscAbilityParametersSingleTargetRangedHarmful(works_on_allies: true);
                energyArrowCureAbility.NeedEquipWeapons = true;

                energyArrow.AddComponent(Helpers.CreateAddFacts(energyArrowCureAbility));
            }
        }
        static public ContextActionFakeTouchAttack createContextActionFakeTouchAttack(ActionList action_on_hit = null, ActionList action_on_miss = null)
        {
            var c = Helpers.Create<ContextActionFakeTouchAttack>();
            c.action_on_success = action_on_hit;
            c.action_on_miss = action_on_miss;
            return c;
        }

    }

    public class ContextActionFakeTouchAttack : ContextAction
    {
        public ActionList action_on_success = null;
        public ActionList action_on_miss = null;
        public override string GetCaption()
        {
            return string.Format("Caster attack");
        }

        public override void RunAction()
        {
            UnitEntityData maybeCaster = this.Context.MaybeCaster;
            if (maybeCaster == null)
            {
                UberDebug.LogError((object)"Caster is missing", (object[])Array.Empty<object>());
            }
            else
            {
                var target = this.Target;
                if (target == null)
                    return;
                var weapon = maybeCaster.Body.PrimaryHand.MaybeWeapon;
                RuleAttackRoll attackWithWeapon = new RuleAttackRoll(maybeCaster, target.Unit, weapon, 0);
                attackWithWeapon.Reason = (RuleReason)this.Context;
                attackWithWeapon.AttackType = AttackType.RangedTouch;
                RuleAttackRoll rule = attackWithWeapon;
                this.Context.TriggerRule<RuleAttackRoll>(rule);
                if (rule.IsHit)
                {
                    action_on_success?.Run();
                }
                else
                {
                    action_on_miss?.Run();
                }
            }
        }
    }


}


