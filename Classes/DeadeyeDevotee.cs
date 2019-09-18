// Copyright (c) 2019 Alisdair Smith
// This code is licensed under MIT license (see LICENSE for details)

using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using System;
using Kingmaker.Utility;
using Kingmaker.Enums.Damage;
using Kingmaker.UnitLogic;
using System.Collections.Generic;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Mechanics.Actions;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.Blueprints.Items.Ecnchantments;
using CallOfTheWild;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using static Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityResourceLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Abilities;


namespace ATouchOfMagic
{
    static class DeadeyeDevoteeClass
    {
        static LibraryScriptableObject library => Main.library;
        static internal BlueprintCharacterClass deadeyeDevotee;
        // static internal BlueprintCharacterClass[] deadeyeDevoteeArray;
        static internal BlueprintProgression deadeyeDevoteeProgression;
        static internal BlueprintFeature energyArrow;
        static internal BlueprintAbilityResource energyArrowResource;
        static internal BlueprintFeatureSelection divineSpellbookSelection;

        static internal BlueprintFeature enhanceArrowsMagic;
        static internal BlueprintFeature enhanceArrowsElemental;
        static internal BlueprintFeature enhanceArrowsDistance;
        static internal BlueprintFeature enhanceArrowsBurst;
        static internal BlueprintFeature enhanceArrowsAligned;
        static internal BlueprintFeatureSelection deadeyeDevoteeArcherFeat;
        static internal BlueprintFeature phaseArrow;
        static internal BlueprintFeature hailOfArrows;
        static internal BlueprintFeature arrowOfDeath;
        static internal BlueprintFeature deadeyeDevoteeProficiencies;




        internal static void CreateDeadeyeDevoteeClass()
        {
            var library = Main.library;
            if (DeadeyeDevoteeClass.deadeyeDevotee != null) return;

            deadeyeDevotee = Helpers.Create<BlueprintCharacterClass>();

            deadeyeDevotee.name = "DeadeyeDevoteeClass";
            library.AddAsset(deadeyeDevotee, "");
            deadeyeDevotee.LocalizedName = Helpers.CreateString("deadeyeDevotee.Name", "Deadeye Devotee");
            deadeyeDevotee.LocalizedDescription = Helpers.CreateString("ArcanedeadeyeDevotee.Description",
                "Dedicated followers of Erastil become closer to their god by mastering archery and caring for each other. They can tap into the power of the Elk Father, who guides and empowers their arrows. ");
            // Matched Druid skill progression
            deadeyeDevotee.SkillPoints = library.Get<BlueprintCharacterClass>("610d836f3a3a9ed42a4349b62f002e96").SkillPoints;
            deadeyeDevotee.HitDie = DiceType.D10;
            deadeyeDevotee.PrestigeClass = true;

            var preciseShot = library.Get<BlueprintFeature>("8f3d1e6b4be006f4d896081f2f889665"); // Precise Shot
            var weaponFocus = library.Get<BlueprintParametrizedFeature>("1e1f627d26ad36f43bbd26cc2bf8ac7e"); // Weapon Focus;
            var sbow = WeaponCategory.Shortbow;
            var lbow = WeaponCategory.Longbow;
            deadeyeDevotee.SetComponents(
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
            deadeyeDevotee.AddComponent(Helpers.Create<PrerequisiteNoClassLevel>(c => c.CharacterClass = ArcaneArcherClass.arcaneArcher));
            Helpers.RegisterClass(deadeyeDevotee);

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


            deadeyeDevoteeProgression = Helpers.CreateProgression("DeadeyeDevoteeProgression",
                            deadeyeDevotee.Name,
                            deadeyeDevotee.Description,
                            "",
                            deadeyeDevotee.Icon,
                            FeatureGroup.None);
            deadeyeDevoteeProgression.Classes = getDeadeyeDevoteeArray();

            deadeyeDevoteeProgression.LevelEntries = new LevelEntry[] {
                Helpers.LevelEntry(1, deadeyeDevoteeProficiencies, enhanceArrowsMagic),
                Helpers.LevelEntry(2, Hinterlander.imbue_arrow, divineSpellbookSelection),
                Helpers.LevelEntry(3, enhanceArrowsElemental),
                Helpers.LevelEntry(4, energyArrow),
                Helpers.LevelEntry(5, ArcaneArcherClass.arcaneArcherFeat), // Distant arrows aren't possible, providing a feat for this level seems reasonable seeing as the class also doesn't get spellcasting here.
                Helpers.LevelEntry(6, phaseArrow),
                Helpers.LevelEntry(7, enhanceArrowsBurst),
                Helpers.LevelEntry(8, hailOfArrows),
                Helpers.LevelEntry(9, enhanceArrowsAligned),
                Helpers.LevelEntry(10, arrowOfDeath)
            };

            deadeyeDevoteeProgression.UIDeterminatorsGroup = new BlueprintFeatureBase[] { deadeyeDevoteeProficiencies };
            deadeyeDevoteeProgression.UIGroups = new UIGroup[]  {
                                                         Helpers.CreateUIGroup(energyArrow, phaseArrow, hailOfArrows, arrowOfDeath),
                                                         Helpers.CreateUIGroup(enhanceArrowsMagic, enhanceArrowsElemental, enhanceArrowsBurst, enhanceArrowsAligned),
                                                         Helpers.CreateUIGroup(divineSpellbookSelection, Hinterlander.imbue_arrow, ArcaneArcherClass.arcaneArcherFeat)
                                                        };
        }


        static void CreateDeadeyeDevoteeFeatures()
        {
            deadeyeDevoteeProficiencies = library.CopyAndAdd<BlueprintFeature>(ArcaneArcherClass.arcaneArcherProficiencies.AssetGuid
            , "DeadeyeDevoteeProficiencies", "", "");
            deadeyeDevoteeProficiencies.SetName("Deadeye Devotee Proficiencies");
            deadeyeDevoteeProficiencies.SetDescription("A deadeye devotee is proficient with all simple and martial weapons, light armor, medium armor, and shields");

            enhanceArrowsMagic = library.CopyAndAdd<BlueprintFeature>(ArcaneArcherClass.enhanceArrowsMagic.AssetGuid
            , "DeadeyeDevoteeEnhanceArrowsMagic", "", "");
            enhanceArrowsMagic.SetDescription($"At 1st level, every nonmagical arrow a deadeye devotee nocks and lets fly becomes magical, gaining a +1 enhancement bonus. " +
                "Unlike magic weapons created by normal means, the archer need not spend gold pieces to accomplish this task. However, an archer’s " +
                "magic arrows only function for him.");

            enhanceArrowsElemental = library.CopyAndAdd<BlueprintFeature>(ArcaneArcherClass.enhanceArrowsElemental.AssetGuid
            , "DeadeyeDevoteeEnhanceArrowsElemental", "", "");
            enhanceArrowsElemental.SetDescription($"At 3rd level, In addition, the deadeye devotee’s arrows gain a number of additional qualities as he gains additional " +
                "levels. The elemental, elemental burst, and aligned qualities can be changed once per day, when the deadeye devotee prepares " +
                "spells or, in the case of spontaneous spellcasters, after 8 hours of rest." +
                "\n At 3rd level, every non-magical arrow fired by an deadeye devotee gains one of the following elemental themed weapon qualities: flaming, frost, or shock.");

            enhanceArrowsBurst = library.CopyAndAdd<BlueprintFeature>(ArcaneArcherClass.enhanceArrowsBurst.AssetGuid
            , "DeadeyeDevoteeEnhanceArrowsBurst", "", "");
            enhanceArrowsBurst.SetDescription($"At 7th level, every non-magical arrow fired by a deadeye devotee gains one of the following elemental burst weapon qualities: " +
                "flaming burst, icy burst, or shocking burst. This ability replaces the ability gained at 3rd level.");

            enhanceArrowsAligned = library.CopyAndAdd<BlueprintFeature>(ArcaneArcherClass.enhanceArrowsAligned.AssetGuid
            , "DeadeyeDevoteeEnhanceArrowsAligned", "", "");
            enhanceArrowsAligned.SetDescription("At 9th level, every non-magical arrow fired by a deadeye devotee gains one of the following aligned weapon qualities: " +
                "anarchic, axiomatic, holy, or unholy. The deadeye devotee cannot choose an ability that is the opposite of his alignment " +
                "(for example, a lawful good deadeye devotee could not choose anarchic or unholy as his weapon quality).");

            phaseArrow = library.CopyAndAdd<BlueprintFeature>(ArcaneArcherClass.phaseArrow.AssetGuid
       , "DeadeyeDevoteePhaseArrow", "", "");
            phaseArrow.SetDescription("At 6th level, a deadeye devotee can launch an arrow once per day at a target known to him within range, and the arrow travels " +
            "to the target in a straight path, passing through any nonmagical barrier or wall in its way. (Any magical barrier stops the arrow.) " +
            "This ability negates cover, concealment, armor, and shield modifiers, but otherwise the attack is rolled normally. Using this ability " +
            "is a standard action (and shooting the arrow is part of the action). A deadeye devotee can use this ability once per day at 6th level, " +
            "and one additional time per day for every two levels beyond 6th, to a maximum of three times per day at 10th level.");
            
            hailOfArrows = library.CopyAndAdd<BlueprintFeature>(ArcaneArcherClass.hailOfArrows.AssetGuid
       , "DeadeyeDevoteeHailOfArrows", "", "");
            hailOfArrows.SetDescription("In lieu of his regular attacks, once per day a deadeye devotee of 8th level or higher can fire an arrow at each and every " +
            "target within range. Each attack uses the archer’s primary attack bonus, and each enemy may only be targeted by a single arrow");
            
            arrowOfDeath = library.CopyAndAdd<BlueprintFeature>(ArcaneArcherClass.arrowOfDeath.AssetGuid
       , "DeadeyeDevoteeArrowOfDeath", "", "");
            arrowOfDeath.SetDescription("At 10th level, a deadeye devotee can create a special type of slaying arrow that forces the target, if damaged by the arrow’s " +
            "attack, to make a Fortitude save or be slain immediately. The DC of this save is equal to 20 + the deadeye devotee’s Charisma modifier. " +
            "It takes 1 day to make a slaying arrow, and the arrow only functions for the deadeye devotee who created it. The slaying arrow lasts no " +
            "longer than 1 year, and the archer can only have one such arrow in existence at a time.");

            //expanded arrows feat
            ArcaneArcherClass.expandedEnhanceArrows.AddComponent(Helpers.CreateAddFeatureOnClassLevel(ArcaneArcherClass.corrosiveArrowsFeature, 3, getDeadeyeDevoteeArray(), null, false));
            ArcaneArcherClass.expandedEnhanceArrows.AddComponent(Helpers.CreateAddFeatureOnClassLevel(ArcaneArcherClass.specialArrowsFeature, 5, getDeadeyeDevoteeArray(), null, false));

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


            var hit_action = Helpers.CreateActionList(Helpers.Create<CallOfTheWild.SpellManipulationMechanics.ReleaseSpellStoredInSpecifiedBuff>(r => r.fact = energyArrow));
            var miss_action = Helpers.CreateActionList(Helpers.Create<CallOfTheWild.SpellManipulationMechanics.ClearSpellStoredInSpecifiedBuff>(r => r.fact = energyArrow));

            int max_variants = 6; //due to ui limitation

            var inflictCriticalWounds = library.Get<BlueprintAbility>("3cf05ef7606f06446ad357845cb4d430");
            var inflictCriticalWoundsMass = library.Get<BlueprintAbility>("5ee395a2423808c4baf342a4f8395b19");
            var inflictLightWounds = library.Get<BlueprintAbility>("e5cb4c4459e437e49a4cd73fde6b9063");
            var inflictLightWoundsMass = library.Get<BlueprintAbility>("9da37873d79ef0a468f969e4e5116ad2");
            var inflictModerateWounds = library.Get<BlueprintAbility>("14d749ecacca90a42b6bf1c3f580bb0c");
            var inflictModerateWoundsMass = library.Get<BlueprintAbility>("03944622fbe04824684ec29ff2cec6a7");
            var inflictSeriousWounds = library.Get<BlueprintAbility>("b0b8a04a3d74e03489862b03f4e467a6");
            var inflictSeriousWoundsMass = library.Get<BlueprintAbility>("820170444d4d2a14abc480fcbdb49535");
            var cureCriticalWounds = library.Get<BlueprintAbility>("0d657aa811b310e4bbd8586e60156a2d");
            var cureCriticalWoundsMass = library.Get<BlueprintAbility>("1f173a16120359e41a20fc75bb53d449");
            var cureLightWounds = library.Get<BlueprintAbility>("47808d23c67033d4bbab86a1070fd62f");
            var cureLightWoundsMass = library.Get<BlueprintAbility>("5d3d689392e4ff740a761ef346815074");
            var cureModerateWounds = library.Get<BlueprintAbility>("1c1ebf5370939a9418da93176cc44cd9");
            var cureModerateWoundsMass = library.Get<BlueprintAbility>("571221cc141bc21449ae96b3944652aa");
            var cureSeriousWounds = library.Get<BlueprintAbility>("6e81a6679a0889a429dec9cedcf3729c");
            var cureSeriousWoundsMass = library.Get<BlueprintAbility>("0cea35de4d553cc439ae80b3a8724397");

            var spellArray = new List<BlueprintAbility>().ToArray();
            spellArray.AddToArray(inflictCriticalWounds, inflictCriticalWoundsMass, inflictLightWounds, inflictLightWoundsMass, inflictModerateWounds, inflictModerateWoundsMass,
            inflictSeriousWounds, inflictSeriousWoundsMass, cureCriticalWounds, cureCriticalWoundsMass, cureLightWounds, cureLightWoundsMass, cureModerateWounds, cureModerateWoundsMass,
            cureSeriousWounds, cureSeriousWoundsMass);

            Predicate<AbilityData> check_slot_predicate = delegate (AbilityData spell)
            {
                return (spellArray.HasItem(spell.Blueprint))
                        && (!spell.Blueprint.HasVariants || spell.Variants.Count < max_variants)
                        && (!spell.RequireMaterialComponent || spell.HasEnoughMaterialComponent);
            };

            for (int i = 0; i < max_variants; i++)
            {
                var imbue_ability = Helpers.CreateAbility($"DeadeyeDevoteeEnergyArrow{i + 1}Ability",
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
                                                                                                s.check_slot_predicate = check_slot_predicate;
                                                                                                s.variant = i;
                                                                                                s.actions = Helpers.CreateActionList(Common.createContextActionAttack(hit_action, miss_action));
                                                                                            }),
                                                          Common.createAbilityCasterMainWeaponCheck(WeaponCategory.Longbow, WeaponCategory.Shortbow),
                                                          Helpers.Create<EnhanceArrowsAligned>(u => { u.damage_type = DamageEnergyType.PositiveEnergy; })
                                                          );
                imbue_ability.setMiscAbilityParametersSingleTargetRangedHarmful(works_on_allies: true);
                imbue_ability.NeedEquipWeapons = true;

                energyArrow.AddComponent(Helpers.CreateAddFacts(imbue_ability));
            }
        }

    }


    public class EnergyArrowReplaceDamage : OwnedGameLogicComponent<UnitDescriptor>, IInitiatorRulebookHandler<RuleCalculateDamage>
    {

        public DamageEnergyType damage_type;

        static LibraryScriptableObject library => Main.library;
        public void OnEventAboutToTrigger(RuleCalculateDamage evt)
        {

            foreach (BaseDamage baseDamage in evt.DamageBundle)
            {
                EnergyDamage energyDamage = baseDamage as EnergyDamage;
                energyDamage.ReplaceEnergy(damage_type);
            }

        }

        public void OnEventDidTrigger(RuleCalculateDamage evt) { }

    }

}


