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
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.Blueprints.Validation;
using System.Linq;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.ResourceLinks;


namespace ATouchOfMagic
{
    static class ArcaneArcherClass
    {
        static LibraryScriptableObject library => Main.library;
        static internal BlueprintCharacterClass arcaneArcher;
        // static internal BlueprintCharacterClass[] arcaneArcherArray;
        static internal BlueprintProgression arcaneArcherProgression;
        static internal BlueprintFeature enhanceArrowsMagic;
        static internal BlueprintFeature enhanceArrowsElemental;
        static internal BlueprintAbilityResource enhanceArrowsElementalResource;
        static internal BlueprintBuff fireArrowBuff;
        static internal BlueprintBuff frostArrowBuff;
        static internal BlueprintBuff shockArrowBuff;
        static internal BlueprintFeature enhanceArrowsDistance;
        static internal BlueprintFeature enhanceArrowsBurst;
        static internal BlueprintFeature enhanceArrowsAligned;
        static internal BlueprintFeatureSelection spellbookSelection;
        static internal BlueprintFeatureSelection arcaneArcherFeat;
        static internal BlueprintFeature seekerArrow;
        static internal BlueprintAbilityResource seekerArrowResource;
        static internal BlueprintFeature phaseArrow;
        static internal BlueprintAbilityResource phaseArrowResource;
        static internal BlueprintFeature hailOfArrows;
        static internal BlueprintAbilityResource hailOfArrowsResource;
        static internal BlueprintFeature arrowOfDeath;
        static internal BlueprintAbilityResource arrowOfDeathResource;
        static internal BlueprintFeature arcaneArcherProficiencies;
        static internal BlueprintFeature expandedEnhanceArrows;
        static internal BlueprintFeature owlcatFamiliarBondFeature;
        static internal BlueprintBuff corrosiveArrowBuff;
        static internal BlueprintFeature corrosiveArrowsFeature;
        static internal BlueprintFeature specialArrowsFeature;
        static internal BlueprintFeature extraHailOfArrows;
    

        internal static void CreateArcaneArcherClass()
        {
            var library = Main.library;
            if (ArcaneArcherClass.arcaneArcher != null) return;

            arcaneArcher = Helpers.Create<BlueprintCharacterClass>();

            arcaneArcher.name = "ArcaneArcherClass";
            library.AddAsset(arcaneArcher, "0fbf5e3fe02f4db19492659dc8a3c411");
            arcaneArcher.LocalizedName = Helpers.CreateString("ArcanearcaneArcher.Name", "Arcane Archer");
            arcaneArcher.LocalizedDescription = Helpers.CreateString("ArcanearcaneArcher.Description",
                "Many who seek to perfect the use of the bow sometimes pursue the path of the arcane archer. " +
                "Arcane archers are masters of ranged combat, as they possess the ability to strike at targets with unerring accuracy and can imbue their arrows with powerful spells. " +
                "Arrows fired by arcane archers fly at weird and uncanny angles to strike at foes around corners, and can pass through solid objects to hit enemies that cower behind such cover. " +
                "At the height of their power, arcane archers can fell even the most powerful foes with a single, deadly shot. ");
            // Matched Druid skill progression
            arcaneArcher.SkillPoints = library.Get<BlueprintCharacterClass>("610d836f3a3a9ed42a4349b62f002e96").SkillPoints;
            arcaneArcher.HitDie = DiceType.D10;
            arcaneArcher.PrestigeClass = true;

            var pointBlankShot = library.Get<BlueprintFeature>("0da0c194d6e1d43419eb8d990b28e0ab"); // Point Blank Shot
            var preciseShot = library.Get<BlueprintFeature>("8f3d1e6b4be006f4d896081f2f889665"); // Precise Shot
            var weaponFocus = library.Get<BlueprintParametrizedFeature>("1e1f627d26ad36f43bbd26cc2bf8ac7e"); // Weapon Focus;
            var sbow = WeaponCategory.Shortbow;
            var lbow = WeaponCategory.Longbow;
            arcaneArcher.SetComponents(
                pointBlankShot.PrerequisiteFeature(),
                preciseShot.PrerequisiteFeature(),
                Common.createPrerequisiteParametrizedFeatureWeapon(weaponFocus, lbow, any: true),
                Common.createPrerequisiteParametrizedFeatureWeapon(weaponFocus, sbow, any: true),
                StatType.BaseAttackBonus.PrerequisiteStatValue(6),
                Helpers.Create<PrerequisiteCasterTypeSpellLevel>(p => { p.IsArcane = true; p.RequiredSpellLevel = 1; p.Group = Prerequisite.GroupType.All; }));

            // Used ranger stats as they seem to fit the theme pretty well
            var ranger = library.Get<BlueprintCharacterClass>("cda0615668a6df14eb36ba19ee881af6");
            var wizard = Helpers.GetClass("ba34257984f4c41408ce1dc2004e342e");
            var savesPrestigeLow = library.Get<BlueprintStatProgression>("dc5257e1100ad0d48b8f3b9798421c72");
            var savesPrestigeHigh = library.Get<BlueprintStatProgression>("1f309006cd2855e4e91a6c3707f3f700");
            arcaneArcher.BaseAttackBonus = ranger.BaseAttackBonus; // BAB high
            arcaneArcher.FortitudeSave = savesPrestigeHigh;
            arcaneArcher.ReflexSave = savesPrestigeHigh;
            arcaneArcher.WillSave = savesPrestigeLow;
            arcaneArcher.IsArcaneCaster = true;

            // Perception (Wis), Ride (Dex), Stealth (Dex), and Survival (Wis).
            // knowledge nature in place of survival, there is no replacement for Ride
            arcaneArcher.ClassSkills = new StatType[] {
                StatType.SkillStealth,
                StatType.SkillLoreNature,
                StatType.SkillPerception
            };

            arcaneArcher.StartingGold = ranger.StartingGold;
            arcaneArcher.PrimaryColor = ranger.PrimaryColor;
            arcaneArcher.SecondaryColor = ranger.SecondaryColor;

            arcaneArcher.RecommendedAttributes = new StatType[] { StatType.Dexterity };
            arcaneArcher.NotRecommendedAttributes = Array.Empty<StatType>();

            arcaneArcher.EquipmentEntities = wizard.EquipmentEntities;
            arcaneArcher.MaleEquipmentEntities = wizard.MaleEquipmentEntities;
            arcaneArcher.FemaleEquipmentEntities = wizard.FemaleEquipmentEntities;

            arcaneArcher.StartingItems = ranger.StartingItems;

            createArcaneArcherProgression();
            arcaneArcher.Progression = arcaneArcherProgression;

            // Arcane archers do not gets spells at levels 1,5 and 9, we handle level 1 by giving spellbook selection at level 2
            // we handle 5 and 9 by adding a skip levels for spell progression component to progressiom.
            var skipLevels = new List<int>();
            skipLevels.Add(5);
            skipLevels.Add(9);
            arcaneArcher.AddComponent(Helpers.Create<SkipLevelsForSpellProgression>(s => s.Levels = skipLevels.ToArray()));
            Helpers.RegisterClass(arcaneArcher);

        }


        static BlueprintCharacterClass[] getArcaneArcherArray()
        {
            return new BlueprintCharacterClass[] { arcaneArcher };
        }

        static void createArcaneArcherProgression()
        {

            var allowed_weapons = new BlueprintWeaponType[4];
            allowed_weapons[0] = library.Get<BlueprintWeaponType>("99ce02fb54639b5439d07c99c55b8542"); // shortbow
            allowed_weapons[1] = library.Get<BlueprintWeaponType>("7a1211c05ec2c46428f41e3c0db9423f"); // longbow
            allowed_weapons[2] = library.Get<BlueprintWeaponType>("1ac79088a7e5dde46966636a3ac71c35"); // composite longbow
            allowed_weapons[3] = library.Get<BlueprintWeaponType>("011f6f86a0b16df4bbf7f40878c3e80b"); // composite shortbow
            CreateArcaneArcherProficiencies();
            CreateEnhanceArrowsMagic(allowed_weapons);
            CreateSpellbookSelection();
            CreateEnhanceArrowsElemental(allowed_weapons);
            CreateSeekerArrow(allowed_weapons);
            
            CreatePhaseArrow(allowed_weapons);
            CreateEnhanceArrowsBurst();
            CreateHailOfArrows();
            CreateEnhanceArrowsAligned(allowed_weapons);
            CreateArrowOfDeath(allowed_weapons);

            //Create class feats
            CreateExpandedEnhanceArrows(allowed_weapons);
            CreateExtraHailOfArrows();
            CreateArcheryFeatSelection();
            CreateOwlcatFamiliar();


            arcaneArcherProgression = Helpers.CreateProgression("ArcaneArcherProgression",
                            arcaneArcher.Name,
                            arcaneArcher.Description,
                            "780848b1fb1f4d73a4f1bf64ae5c21b2",
                            arcaneArcher.Icon,
                            FeatureGroup.None);
            arcaneArcherProgression.Classes = getArcaneArcherArray();

            arcaneArcherProgression.LevelEntries = new LevelEntry[] {
                Helpers.LevelEntry(1, arcaneArcherProficiencies, enhanceArrowsMagic),
                Helpers.LevelEntry(2, Hinterlander.imbue_arrow, spellbookSelection),
                Helpers.LevelEntry(3, enhanceArrowsElemental),
                Helpers.LevelEntry(4, seekerArrow),
                Helpers.LevelEntry(5, arcaneArcherFeat), // Distant arrows aren't possible, providing a feat for this level seems reasonable seeing as the class also doesn't get spellcasting here.
                Helpers.LevelEntry(6, phaseArrow),
                Helpers.LevelEntry(7, enhanceArrowsBurst),
                Helpers.LevelEntry(8, hailOfArrows),
                Helpers.LevelEntry(9, enhanceArrowsAligned),
                Helpers.LevelEntry(10, arrowOfDeath)
            };

            arcaneArcherProgression.UIDeterminatorsGroup = new BlueprintFeatureBase[] { arcaneArcherProficiencies };
            arcaneArcherProgression.UIGroups = new UIGroup[]  {
                                                         Helpers.CreateUIGroup(seekerArrow, phaseArrow, hailOfArrows, arrowOfDeath),
                                                         Helpers.CreateUIGroup(enhanceArrowsMagic, enhanceArrowsElemental, enhanceArrowsBurst, enhanceArrowsAligned),
                                                         Helpers.CreateUIGroup(spellbookSelection, Hinterlander.imbue_arrow, arcaneArcherFeat)
                                                        };
        }

        static void CreateArcaneArcherProficiencies()
        {
            arcaneArcherProficiencies = library.CopyAndAdd<BlueprintFeature>(
            "c5e479367d07d62428f2fe92f39c0341", // ranger proficiencies
            "ArcaneArcherProficiencies",
            "85be49f802ec4156ad34a3b88dd64fb5");
            arcaneArcherProficiencies.SetName("Arcane Archer Proficiencies");
            arcaneArcherProficiencies.SetDescription("An arcane archer is proficient with all simple and martial weapons, light armor, medium armor, and shields");

        }

        static void CreateEnhanceArrowsMagic(BlueprintWeaponType[] allowed_weapons)
        {
            enhanceArrowsMagic = Helpers.CreateFeature("ArcaneArcherEnhanceArrowsMagic", "Enhance Arrows (Magic)",
                $"At 1st level, every nonmagical arrow an arcane archer nocks and lets fly becomes magical, gaining a +1 enhancement bonus. " +
                "Unlike magic weapons created by normal means, the archer need not spend gold pieces to accomplish this task. However, an archer’s " +
                "magic arrows only function for him.",
                "f64aa29727344ed9b7fa7918943d3038",
                CallOfTheWild.LoadIcons.Image2Sprite.Create(@"ArcaneArcher/enhanceArrowsMagic.png"),
                FeatureGroup.None,
                Helpers.Create<EnhanceArrowsMagic>(u => u.weapon_types = allowed_weapons));
        }

        static void CreateEnhanceArrowsElemental(BlueprintWeaponType[] allowed_weapons)
        {
            enhanceArrowsElementalResource = Helpers.CreateAbilityResource("EnhanceArrowsElementalResource", "", "", "", library.Get<BlueprintFeature>("6aa84ca8918ac604685a3d39a13faecc").Icon);
            enhanceArrowsElementalResource.SetFixedResource(1);
            var name = "EnhanceArrows";
            var displayName = "Enhance Arrows";
            corrosiveArrowBuff = Helpers.CreateBuff(name + "Corrosive" + "Buff", displayName + " (Corrosive)", $"Whilst active, your arrows deal 1d6 additional Acid damage.", "",
                CallOfTheWild.LoadIcons.Image2Sprite.Create(@"ArcaneArcher/enhanceArrowsAcid.png"), null,
                Helpers.Create<EnhanceArrowsElemental>(u => { u.weapon_types = allowed_weapons; u.damage_type = DamageEnergyType.Acid; }));
            fireArrowBuff = Helpers.CreateBuff(name + "Fire" + "Buff", displayName + " (Fire)", $"Whilst active, your arrows deal 1d6 additional Fire damage.", "",
                CallOfTheWild.LoadIcons.Image2Sprite.Create(@"ArcaneArcher/enhanceArrowsFire.png"), null,
                Helpers.Create<EnhanceArrowsElemental>(u => { u.weapon_types = allowed_weapons; u.damage_type = DamageEnergyType.Fire; }));
            frostArrowBuff = Helpers.CreateBuff(name + "Frost" + "Buff", displayName + " (Frost)", $"Whilst active, your arrows deal 1d6 additional Frost damage.", "",
                CallOfTheWild.LoadIcons.Image2Sprite.Create(@"ArcaneArcher/enhanceArrowsFrost.png"), null,
                Helpers.Create<EnhanceArrowsElemental>(u => { u.weapon_types = allowed_weapons; u.damage_type = DamageEnergyType.Cold; }));
            shockArrowBuff = Helpers.CreateBuff(name + "Shock" + "Buff", displayName + " (Shock)", $"Whilst active, your arrows deal 1d6 additional Shock damage.", "",
                CallOfTheWild.LoadIcons.Image2Sprite.Create(@"ArcaneArcher/enhanceArrowsShock.png"), null,
                Helpers.Create<EnhanceArrowsElemental>(u => { u.weapon_types = allowed_weapons; u.damage_type = DamageEnergyType.Electricity; }));

            var actionFire = Helpers.CreateRunActions(Common.createContextActionApplyBuff(fireArrowBuff,
                Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.StatBonus)), is_permanent: true, dispellable: false),
                Common.createContextActionRemoveBuff(shockArrowBuff), Common.createContextActionRemoveBuff(frostArrowBuff),
                Common.createContextActionRemoveBuff(corrosiveArrowBuff));
            var actionFrost = Helpers.CreateRunActions(Common.createContextActionApplyBuff(frostArrowBuff,
                Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.StatBonus)), is_permanent: true, dispellable: false),
                Common.createContextActionRemoveBuff(shockArrowBuff), Common.createContextActionRemoveBuff(fireArrowBuff),
                Common.createContextActionRemoveBuff(corrosiveArrowBuff));
            var actionShock = Helpers.CreateRunActions(Common.createContextActionApplyBuff(shockArrowBuff,
                Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.StatBonus)), is_permanent: true, dispellable: false),
                Common.createContextActionRemoveBuff(fireArrowBuff), Common.createContextActionRemoveBuff(frostArrowBuff),
                Common.createContextActionRemoveBuff(corrosiveArrowBuff));

            var abilityFire = Helpers.CreateAbility("EnhanceArrowsFireAbility",
                fireArrowBuff.Name, fireArrowBuff.Description, "", fireArrowBuff.Icon, AbilityType.Supernatural, CommandType.Free,
                AbilityRange.Weapon, "Permanent", "N/A", actionFire, Helpers.CreateResourceLogic(enhanceArrowsElementalResource));
            var abilityFrost = Helpers.CreateAbility("EnhanceArrowsFrostAbility",
                frostArrowBuff.Name, frostArrowBuff.Description, "", frostArrowBuff.Icon, AbilityType.Supernatural, CommandType.Free,
                AbilityRange.Weapon, "Permanent", "N/A", actionFrost, Helpers.CreateResourceLogic(enhanceArrowsElementalResource));
            var abilityShock = Helpers.CreateAbility("EnhanceArrowsShockAbility",
                shockArrowBuff.Name, shockArrowBuff.Description, "", shockArrowBuff.Icon, AbilityType.Supernatural, CommandType.Free,
                AbilityRange.Weapon, "Permanent", "N/A", actionShock, Helpers.CreateResourceLogic(enhanceArrowsElementalResource));

            enhanceArrowsElemental = Helpers.CreateFeature("ArcaneArcherEnhanceArrowsElemental", "Enhance Arrows (Elemental)",
                $"At 3rd level, In addition, the arcane archer’s arrows gain a number of additional qualities as he gains additional " +
                "levels. The elemental, elemental burst, and aligned qualities can be changed once per day, when the arcane archer prepares " +
                "spells or, in the case of spontaneous spellcasters, after 8 hours of rest." +
                "\n At 3rd level, every non-magical arrow fired by an arcane archer gains one of the following elemental themed weapon qualities: flaming, frost, or shock.",
                "",
                CallOfTheWild.LoadIcons.Image2Sprite.Create(@"ArcaneArcher/enhanceArrowsMagic.png"), // hurricane bow
                FeatureGroup.None,
                Helpers.CreateAddFact(abilityFire),
                Helpers.CreateAddFact(abilityFrost),
                Helpers.CreateAddFact(abilityShock),
                Helpers.CreateAddAbilityResource(enhanceArrowsElementalResource));

        }

        static void CreateEnhanceArrowsAligned(BlueprintWeaponType[] allowed_weapons)
        {
            var resource = Helpers.CreateAbilityResource("EnhanceArrowsAlignedResource", "", "", "", library.Get<BlueprintFeature>("6aa84ca8918ac604685a3d39a13faecc").Icon);
            resource.SetFixedResource(1);
            var name = "EnhanceArrows";
            var displayName = "Enhance Arrows";

            //buffs
            var holyArrowBuff = Helpers.CreateBuff(name + "Holy" + "Buff", displayName + " (Holy)",
                $"Whilst active, your arrows deal 2d6 additional Holy damage against creatures of evil alignment", "",
                CallOfTheWild.LoadIcons.Image2Sprite.Create(@"ArcaneArcher/enhanceArrowsHoly.png"), null,
                Helpers.Create<EnhanceArrowsAligned>(u => { u.weapon_types = allowed_weapons; u.alignment = "Unoly"; u.damage_type = DamageEnergyType.Holy; }));
            var unholyArrowBuff = Helpers.CreateBuff(name + "Unoly" + "Buff", displayName + " (Unoly)",
                $"Whilst active, your arrows deal 2d6 additional Unholy damage against creatures of good alignment", "",
                CallOfTheWild.LoadIcons.Image2Sprite.Create(@"ArcaneArcher/enhanceArrowsUnholy.png"), null,
                Helpers.Create<EnhanceArrowsAligned>(u => { u.weapon_types = allowed_weapons; u.alignment = "Unoly"; u.damage_type = DamageEnergyType.Unholy; }));
            var anarchicArrowBuff = Helpers.CreateBuff(name + "Anarchic" + "Buff", displayName + " (Anarchic)",
                $"Whilst active, your arrows deal 2d6 additional Unholy damage against creatures of lawful alignment", "",
                CallOfTheWild.LoadIcons.Image2Sprite.Create(@"ArcaneArcher/enhanceArrowsChaos.png"), null,
                Helpers.Create<EnhanceArrowsAligned>(u => { u.weapon_types = allowed_weapons; u.alignment = "Anarchic"; u.damage_type = DamageEnergyType.Unholy; }));
            var axiomaticArrowBuff = Helpers.CreateBuff(name + "Axiomic" + "Buff", displayName + " (Axiomic)",
                $"Whilst active, your arrows deal 2d6 additional Holy damage against creatures of chaotic alignment", "",
                CallOfTheWild.LoadIcons.Image2Sprite.Create(@"ArcaneArcher/enhanceArrowsLaw.png"), null,
                Helpers.Create<EnhanceArrowsAligned>(u => { u.weapon_types = allowed_weapons; u.alignment = "Axiomic"; u.damage_type = DamageEnergyType.Holy; }));

            //actions
            var actionHoly = Helpers.CreateRunActions(Common.createContextActionApplyBuff(holyArrowBuff,
                Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.StatBonus)), is_permanent: true, dispellable: false),
                Common.createContextActionRemoveBuff(unholyArrowBuff), Common.createContextActionRemoveBuff(anarchicArrowBuff),
                Common.createContextActionRemoveBuff(axiomaticArrowBuff));
            var actionUnholy = Helpers.CreateRunActions(Common.createContextActionApplyBuff(unholyArrowBuff,
                Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.StatBonus)), is_permanent: true, dispellable: false),
                Common.createContextActionRemoveBuff(holyArrowBuff), Common.createContextActionRemoveBuff(anarchicArrowBuff),
                Common.createContextActionRemoveBuff(axiomaticArrowBuff));
            var actionAnarchic = Helpers.CreateRunActions(Common.createContextActionApplyBuff(anarchicArrowBuff,
                Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.StatBonus)), is_permanent: true, dispellable: false),
                Common.createContextActionRemoveBuff(holyArrowBuff), Common.createContextActionRemoveBuff(unholyArrowBuff),
                Common.createContextActionRemoveBuff(axiomaticArrowBuff));
            var actionAxiomatic = Helpers.CreateRunActions(Common.createContextActionApplyBuff(axiomaticArrowBuff,
                Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.StatBonus)), is_permanent: true, dispellable: false),
                Common.createContextActionRemoveBuff(holyArrowBuff), Common.createContextActionRemoveBuff(unholyArrowBuff),
                Common.createContextActionRemoveBuff(anarchicArrowBuff));

            //abilities
            var abilityHoly = Helpers.CreateAbility("EnhanceArrowsHolyAbility",
                            holyArrowBuff.Name,
                            holyArrowBuff.Description, "", holyArrowBuff.Icon, AbilityType.Supernatural, CommandType.Free,
                            AbilityRange.Weapon, "Permanent", "N/A", actionHoly, Helpers.CreateResourceLogic(resource),
                            Helpers.Create<AbilityCasterAlignment>(c => c.Alignment = AlignmentMaskType.Any & ~AlignmentMaskType.Evil));
            var abilityUnoly = Helpers.CreateAbility("EnhanceArrowsUnholyAbility",
                            unholyArrowBuff.Name,
                            unholyArrowBuff.Description, "", unholyArrowBuff.Icon, AbilityType.Supernatural, CommandType.Free,
                            AbilityRange.Weapon, "Permanent", "N/A", actionUnholy, Helpers.CreateResourceLogic(resource),
                            Helpers.Create<AbilityCasterAlignment>(c => c.Alignment = AlignmentMaskType.Any & ~AlignmentMaskType.Good));
            var abilityAnarchic = Helpers.CreateAbility("EnhanceArrowsAnarchicAbility",
                            anarchicArrowBuff.Name,
                            anarchicArrowBuff.Description, "", anarchicArrowBuff.Icon, AbilityType.Supernatural, CommandType.Free,
                            AbilityRange.Weapon, "Permanent", "N/A", actionAnarchic, Helpers.CreateResourceLogic(resource),
                            Helpers.Create<AbilityCasterAlignment>(c => c.Alignment = AlignmentMaskType.Any & ~AlignmentMaskType.Lawful));
            var abilityAxiomatic = Helpers.CreateAbility("EnhanceArrowsAxiomaticAbility",
                            axiomaticArrowBuff.Name,
                            axiomaticArrowBuff.Description, "", axiomaticArrowBuff.Icon, AbilityType.Supernatural, CommandType.Free,
                            AbilityRange.Weapon, "Permanent", "N/A", actionAxiomatic, Helpers.CreateResourceLogic(resource),
                            Helpers.Create<AbilityCasterAlignment>(c => c.Alignment = AlignmentMaskType.Any & ~AlignmentMaskType.Chaotic));

            //feature
            enhanceArrowsAligned = Helpers.CreateFeature("ArcaneArcherEnhanceArrowsAligned", "Enhance Arrows (Aligned)",
                $"At 9th level, every non-magical arrow fired by an arcane archer gains one of the following aligned weapon qualities: " +
                "anarchic, axiomatic, holy, or unholy. The arcane archer cannot choose an ability that is the opposite of his alignment " +
                "(for example, a lawful good arcane archer could not choose anarchic or unholy as his weapon quality).",
                "",
                CallOfTheWild.LoadIcons.Image2Sprite.Create(@"ArcaneArcher/enhanceArrowsMagic.png"),
                FeatureGroup.None,
                Helpers.CreateAddFact(abilityHoly),
                Helpers.CreateAddFact(abilityUnoly),
                Helpers.CreateAddFact(abilityAnarchic),
                Helpers.CreateAddFact(abilityAxiomatic),
                Helpers.CreateAddAbilityResource(resource));

        }

        static void CreateEnhanceArrowsBurst()
        {
            enhanceArrowsBurst = Helpers.CreateFeature("ArcaneArcherEnhanceArrowsBurst", "Enhance Arrows (Burst)",
                $"At 7th level, every non-magical arrow fired by an arcane archer gains one of the following elemental burst weapon qualities: " +
                "flaming burst, icy burst, or shocking burst. This ability replaces the ability gained at 3rd level.",
                "",
                CallOfTheWild.LoadIcons.Image2Sprite.Create(@"ArcaneArcher/enhanceArrowsMagic.png"),
                FeatureGroup.None);
        }


        static void CreateArcheryFeatSelection()
        {
            arcaneArcherFeat = library.CopyAndAdd<BlueprintFeatureSelection>("6c799d09d5b93f344b9ade0e0c765c2d", "ArcaneArcherFeat", "c7179c618cc84a9283ceb95f2f4fcc46");//archery feat 6
            arcaneArcherFeat.SetDescription("At 5th level an arcane archer gains an additional archery feat.");
            arcaneArcherFeat.AllFeatures = arcaneArcherFeat.AllFeatures.AddToArray(NewFeats.deadeyes_blessing, expandedEnhanceArrows);

        }
        // Not currently using this feature, if we can find a way to get it to work, then it may get added.
        static void CreateEnhanceArrowsDistance()
        {
            enhanceArrowsDistance = Helpers.CreateFeature("ArcaneArcherEnhanceArrowsDistance", "Enhance Arrows (Distance)",
                $"At 5th level, every non-magical arrow fired by an arcane archer gains the distance weapon quality.",
                "",
                CallOfTheWild.LoadIcons.Image2Sprite.Create(@"ArcaneArcher/enhanceArrowsMagic.png"),
                FeatureGroup.None);
        }

        static void CreateSpellbookSelection()
        {
            var comps = new List<BlueprintComponent>();
            var compsArray = comps.ToArray();
            spellbookSelection = Helpers.CreateFeatureSelection("ArcaneArcherSpellbookSelection",
            "Arcane Spellcasting",
            $"At 2nd level, and at every level thereafter, with an exception for 5th and 9th levels, " +
                                       "an Arcane Archer  gains new spells per day as if he had also gained a level in an arcane spellcasting " +
                                       "class he belonged to before adding the prestige class. He does not, however, gain any other benefit a " +
                                       "character of that class would have gained, except for additional spells per day, spells known, and an " +
                                       "increased effective level of spellcasting. If a character had more than one arcane spellcasting class " +
                                       "before becoming an Arcane Archer, he must decide to which class he adds the new level for purposes of " +
                                       "determining spells per day.",
                                       "ea4c7c56d90d413886876152b03f9f5f",
                                       CallOfTheWild.LoadIcons.Image2Sprite.Create(@"FeatIcons/Icon_Casting_Combat.png"),
                                       FeatureGroup.EldritchKnightSpellbook, compsArray);
            Common.addSpellbooksToSpellSelection("Arcane Archer", 1, spellbookSelection, divine: false, alchemist: false);
        }


        static void CreateSeekerArrow(BlueprintWeaponType[] allowed_weapons)
        {
            seekerArrowResource = Helpers.CreateAbilityResource("SeekerArrowResource", "", "", "", library.Get<BlueprintFeature>("6aa84ca8918ac604685a3d39a13faecc").Icon);
            seekerArrowResource.SetIncreasedByLevelStartPlusDivStep(0, 4, 1, 2, 1, 0, 0.0f, getArcaneArcherArray());
            seekerArrow = Helpers.CreateFeature("ArcaneArcherSeekerArrow", "Seeker Arrow",
            $"At 4th level, an arcane archer can launch an arrow at a target known to him within range, and the arrow travels " +
            "to the target, even around corners. Only an unavoidable obstacle or the limit of the arrow’s range prevents the arrow’s flight. " +
            "This ability negates cover and concealment modifiers, but otherwise the attack is rolled normally. Using this ability is a " +
            "standard action (and shooting the arrow is part of the action). An arcane archer can use this ability once per day at 4th level, " +
            "and one additional time per day for every two levels beyond 4th, to a maximum of four times per day at 10th level.",
            "",
            Helpers.GetIcon("2c38da66e5a599347ac95b3294acbe00"), // truestrike
            FeatureGroup.None,
            Helpers.CreateAddAbilityResource(seekerArrowResource));

            var seekerArrowBuff = Helpers.CreateBuff(seekerArrow.name + "Buff", "Seeker Arrow", $"This arrow ignores concealment and cover", "",
               library.Get<BlueprintAbility>("2c38da66e5a599347ac95b3294acbe00").Icon, null,
               Helpers.Create<SeekerArrow>());

            var applyCasterBuff = Common.createContextActionApplyBuff(seekerArrowBuff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Rounds), dispellable: false);
            var applyBuffAction = Helpers.CreateRunActions(Helpers.Create<ContextActionOnContextCaster>(c => c.Actions = Helpers.CreateActionList(applyCasterBuff)), Common.createContextActionAttack());

            var removeCasterBuff = Common.createContextActionRemoveBuff(seekerArrowBuff);
            var removeBuffAction = Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(removeCasterBuff), only_hit: false, wait_for_attack_to_resolve: true, on_initiator: true);
            seekerArrowBuff.AddComponent(removeBuffAction);
            var seekerArrowAbility = Helpers.CreateAbility($"SeekerArrowAbility",
                                            seekerArrow.Name,
                                            seekerArrow.Description,
                                            "",
                                            seekerArrow.Icon,
                                            AbilityType.Supernatural,
                                            CommandType.Standard,
                                            AbilityRange.Weapon,
                                            "",
                                            "",
                                            Helpers.Create<CallOfTheWild.NewMechanics.AttackAnimation>(),
                                            applyBuffAction,
                                            Common.createAbilityCasterMainWeaponCheck(WeaponCategory.Longbow, WeaponCategory.Shortbow),
                                            Helpers.CreateResourceLogic(seekerArrowResource)
                                                     );

            seekerArrowAbility.setMiscAbilityParametersSingleTargetRangedHarmful(works_on_allies: true);
            seekerArrowAbility.NeedEquipWeapons = true;
            seekerArrow.AddComponent(Helpers.CreateAddFacts(seekerArrowAbility));
        }

        static void CreatePhaseArrow(BlueprintWeaponType[] allowed_weapons)
        {
            phaseArrowResource = Helpers.CreateAbilityResource("PhaseArrowResource", "", "", "", library.Get<BlueprintFeature>("6aa84ca8918ac604685a3d39a13faecc").Icon);
            phaseArrowResource.SetIncreasedByLevelStartPlusDivStep(0, 6, 1, 2, 1, 0, 0.0f, getArcaneArcherArray());
            phaseArrow = Helpers.CreateFeature("ArcaneArcherPhaseArrow", "Phase Arrow",
            $"At 6th level, an archer can launch an arrow once per day at a target known to him within range, and the arrow travels " +
            "to the target in a straight path, passing through any nonmagical barrier or wall in its way. (Any magical barrier stops the arrow.) " +
            "This ability negates cover, concealment, armor, and shield modifiers, but otherwise the attack is rolled normally. Using this ability " +
            "is a standard action (and shooting the arrow is part of the action). An archer can use this ability once per day at 6th level, " +
            "and one additional time per day for every two levels beyond 6th, to a maximum of three times per day at 10th level.",
            "",
             Helpers.GetIcon("2c38da66e5a599347ac95b3294acbe00"), // truestrike
            FeatureGroup.None,
            Helpers.CreateAddAbilityResource(phaseArrowResource));

            var phaseArrowBuff = Helpers.CreateBuff(phaseArrow.name + "Buff", "Phase Arrow", $"This arrow ignores concealment, cover and the targets armour", "",
               library.Get<BlueprintAbility>("2c38da66e5a599347ac95b3294acbe00").Icon, null,
               Helpers.Create<PhaseArrow>());

            var applyCasterBuff = Common.createContextActionApplyBuff(phaseArrowBuff, Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Rounds), dispellable: false);
            var applyBuffAction = Helpers.CreateRunActions(Helpers.Create<ContextActionOnContextCaster>(c => c.Actions = Helpers.CreateActionList(applyCasterBuff)), Common.createContextActionAttack());

            var removeCasterBuff = Common.createContextActionRemoveBuff(phaseArrowBuff);
            var removeBuffAction = Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(removeCasterBuff), only_hit: false, wait_for_attack_to_resolve: true, on_initiator: true);
            phaseArrowBuff.AddComponent(removeBuffAction);

            var phaseArrowAbility = Helpers.CreateAbility($"PhaseArrowAbility",
                                            phaseArrow.Name,
                                            phaseArrow.Description,
                                            "",
                                            phaseArrow.Icon,
                                            AbilityType.Supernatural,
                                            CommandType.Standard,
                                            AbilityRange.Weapon,
                                            "",
                                            "",
                                            Helpers.Create<CallOfTheWild.NewMechanics.AttackAnimation>(),
                                            applyBuffAction,
                                            Common.createAbilityCasterMainWeaponCheck(WeaponCategory.Longbow, WeaponCategory.Shortbow),
                                            Helpers.CreateResourceLogic(phaseArrowResource)
                                                     );

            phaseArrowAbility.setMiscAbilityParametersSingleTargetRangedHarmful(works_on_allies: true);
            phaseArrowAbility.NeedEquipWeapons = true;
            phaseArrow.AddComponent(Helpers.CreateAddFacts(phaseArrowAbility));
        }
        static void CreateHailOfArrows()
        {
            hailOfArrowsResource = Helpers.CreateAbilityResource("HailofArrowsResource", "", "", "", library.Get<BlueprintFeature>("6aa84ca8918ac604685a3d39a13faecc").Icon);
            hailOfArrowsResource.SetFixedResource(1);
            hailOfArrows = Helpers.CreateFeature("ArcaneArcherHailofArrows", "Hail of Arrows",
            $"In lieu of his regular attacks, once per day an archer of 8th level or higher can fire an arrow at each and every " +
            "target within range. Each attack uses the archer’s primary attack bonus, and each enemy may only be targeted by a single arrow",
            "",
            CallOfTheWild.LoadIcons.Image2Sprite.Create(@"ArcaneArcher/hailOfArrows.png"),
            FeatureGroup.None,
            Helpers.CreateAddAbilityResource(hailOfArrowsResource));

            // no maximum number so if there were more enemies than the arcane archers current level they'd get too many shots., but more than 8 is unusual.
            var hailOfArrowsAbility = Helpers.CreateAbility($"HailOfArrowsAbility",
                                            hailOfArrows.Name,
                                            hailOfArrows.Description,
                                            "",
                                            hailOfArrows.Icon,
                                            AbilityType.Supernatural,
                                            CommandType.Standard,
                                            AbilityRange.Weapon,
                                            "",
                                            "",
                                            Helpers.Create<CallOfTheWild.NewMechanics.AttackAnimation>(),
                                            Helpers.CreateRunActions(createContextActionAttackInRange()),
                                            Common.createAbilityCasterMainWeaponCheck(WeaponCategory.Longbow, WeaponCategory.Shortbow),
                                            Helpers.CreateAbilityTargetsAround(50.Feet(), TargetType.Enemy),
                                            Helpers.CreateResourceLogic(hailOfArrowsResource));

            hailOfArrowsAbility.NeedEquipWeapons = true;
            hailOfArrows.AddComponent(Helpers.CreateAddFacts(hailOfArrowsAbility));

        }
        static void CreateArrowOfDeath(BlueprintWeaponType[] allowed_weapons)
        {
            arrowOfDeathResource = Helpers.CreateAbilityResource("ArrowOfDeathArrowResource", "", "", "", library.Get<BlueprintFeature>("6aa84ca8918ac604685a3d39a13faecc").Icon);
            arrowOfDeathResource.SetFixedResource(1);
            arrowOfDeath = Helpers.CreateFeature("ArcaneArcherArrowOfDeath", "Arrow of Death",
            $"At 10th level, an archer can create a special type of slaying arrow that forces the target, if damaged by the arrow’s " +
            "attack, to make a Fortitude save or be slain immediately. The DC of this save is equal to 20 + the archer’s Charisma modifier. " +
            "It takes 1 day to make a slaying arrow, and the arrow only functions for the archer who created it. The slaying arrow lasts no " +
            "longer than 1 year, and the archer can only have one such arrow in existence at a time.",
            "",
            CallOfTheWild.LoadIcons.Image2Sprite.Create(@"ArcaneArcher/arrowOfDeath.png"),
            FeatureGroup.None,
            Helpers.CreateAddAbilityResource(arrowOfDeathResource));
            var save_condition = Helpers.CreateConditionalSaved(new Kingmaker.ElementsSystem.GameAction[0], new Kingmaker.ElementsSystem.GameAction[] { Helpers.Create<ContextActionKillTarget>() });
            var save_action = Helpers.CreateActionList(Common.createContextActionSavingThrow(SavingThrowType.Fortitude, Helpers.CreateActionList(save_condition)));
            var action = Helpers.CreateRunActions(Common.createContextActionAttack(save_action));

            var saveDC = createContextCalculateHighestMentalStat();

            var arrowOfDeathBuff = Helpers.CreateBuff(arrowOfDeath.name + "Buff", "Arrow of Death", $"The target of this arrow must make a Fortitude save or be slain immediately. The DC of this save is equal to 20 + the arcane archer's Charisma modifier.", "",
                     arrowOfDeath.Icon, null,
                     Common.createAddInitiatorAttackWithWeaponTrigger(save_action, range_type: AttackTypeAttackBonus.WeaponRangeType.Ranged, wait_for_attack_to_resolve: false),
                     saveDC);

            var arrowOfDeathActivatableAbility = Helpers.CreateActivatableAbility("ArrowOfDeathActivatableAbility",
                                            arrowOfDeath.Name,
                                            arrowOfDeath.Description,
                                            "",
                                            arrowOfDeath.Icon,
                                            arrowOfDeathBuff,
                                            AbilityActivationType.Immediately,
                                            CommandType.Free,
                                            null,
                                            Helpers.Create<CallOfTheWild.NewMechanics.ActivatableAbilityMainWeaponTypeAllowed>(c => c.weapon_types = allowed_weapons),
                                            Helpers.CreateActivatableResourceLogic(arrowOfDeathResource, ResourceSpendType.Attack));
            arrowOfDeathActivatableAbility.DeactivateImmediately = true;
            arrowOfDeath.AddComponent(Helpers.CreateAddFacts(arrowOfDeathActivatableAbility));
        }

        static public ContextActionAttackInRange createContextActionAttackInRange(ActionList action_on_hit = null, ActionList action_on_miss = null)
        {
            var c = Helpers.Create<ContextActionAttackInRange>();
            c.action_on_success = action_on_hit;
            c.action_on_miss = action_on_miss;
            return c;
        }

        //TODO
        static void CreateExtraHailOfArrows()
        {
            extraHailOfArrows = Helpers.CreateFeature("ExtraHailOfArrows", "Extra Hail of Arrows",
            $"You can use your hail of arrows ability one additional time per day",
            "",
            CallOfTheWild.LoadIcons.Image2Sprite.Create(@"FeatIcons/Icon_Expanded_Enhanced_Arrows.png"),
            FeatureGroup.CombatFeat,
            Helpers.PrerequisiteFeature(hailOfArrows),
            Helpers.CreateIncreaseResourceAmount(hailOfArrowsResource, 1));
            library.AddFeats(extraHailOfArrows);
        }

        static void CreateOwlcatFamiliar()
        {
            var charmAnimal = library.Get<BlueprintAbility>("08df458bd00ba704dab32dd493c61518");
            
            owlcatFamiliarBondFeature = library.CopyAndAdd<BlueprintFeature>(
           "1cb0b559ca2e31e4d9dc65de012fa82f", // cat familiar proficiencies
           "OwlcatFamiliarBondFeature",
           "a1044d9ef9444fa18cdeba353806531a");
            owlcatFamiliarBondFeature.SetName("Owlcat Familiar");
            owlcatFamiliarBondFeature.SetDescription("A familiar is a magical pet that enhances the charater's skills and senses. An Owlcat's master gains a +3 bonus on Stealth checks and a +2 bonus on Perception checks.");
            var owlcatFamiliarBuff = Helpers.CreateBuff("OwlcatFamiliarBuff", "Owlcat Familiar",
                "A familiar is a magical pet that enhances the charater's skills and senses. An Owlcat's master gains a +3 bonus on Stealth checks and a +2 bonus on Perception checks.",
                "2be38e63df4f43d0a88bca197e67f48d", charmAnimal.Icon, null);
            owlcatFamiliarBuff.AddComponent(CreateAddFamiliar());
            var owlcatFamiliarAbility = library.CopyAndAdd<BlueprintActivatableAbility>("39f45f50742fe8a4aa6b295d036e4c28", "OwlcatFamiliarAbility", "f1acc4ef459f4156b0c489672d7ce4d9");
            owlcatFamiliarAbility.SetDescription(owlcatFamiliarBondFeature.Description);
            owlcatFamiliarAbility.SetName(owlcatFamiliarBondFeature.Name);
            owlcatFamiliarAbility.Buff = owlcatFamiliarBuff;
            owlcatFamiliarBondFeature.ReplaceComponent<AddFacts>(Helpers.CreateAddFacts(owlcatFamiliarAbility));

            var arcaneBondFeature = library.Get<BlueprintFeatureSelection>("03a1781486ba98043afddaabf6b7d8ff");
            arcaneBondFeature.AllFeatures = arcaneBondFeature.AllFeatures.AddToArray(owlcatFamiliarBondFeature);
        }

        static public AddFamiliar CreateAddFamiliar()
        {
            var sc = Helpers.Create<Kingmaker.UnitLogic.FactLogic.AddFamiliar>();
            sc.name = "AddFamiliar";
            FamiliarLink p = new FamiliarLink();
            p.AssetId = "fc8044c092e46db43895023e08cb8d62";
            sc.PrefabLink = p;
            return sc;
        }

        static void CreateExpandedEnhanceArrows(BlueprintWeaponType[] allowed_weapons)
        {
            var actionCorrosive = Helpers.CreateRunActions(Common.createContextActionApplyBuff(corrosiveArrowBuff,
                Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.StatBonus)), is_permanent: true, dispellable: false),
                Common.createContextActionRemoveBuff(shockArrowBuff), Common.createContextActionRemoveBuff(frostArrowBuff),
                Common.createContextActionRemoveBuff(fireArrowBuff));

            var abilityCorrosive = Helpers.CreateAbility("EnhanceArrowsCorrosiveAbility",
                corrosiveArrowBuff.Name, corrosiveArrowBuff.Description, "", corrosiveArrowBuff.Icon, AbilityType.Supernatural, CommandType.Free,
                AbilityRange.Weapon, "Permanent", "N/A", actionCorrosive, Helpers.CreateResourceLogic(enhanceArrowsElementalResource),
                Helpers.PrerequisiteClassLevel(arcaneArcher, 3));

            var resource = Helpers.CreateAbilityResource("EnhanceArrowsLevel5Resource", "", "", "", library.Get<BlueprintFeature>("6aa84ca8918ac604685a3d39a13faecc").Icon);
            resource.SetFixedResource(1);

            var blur = library.Get<BlueprintBuff>("dd3ad347240624d46a11a092b4dd4674");
            var displacement = library.Get<BlueprintBuff>("00402bae4442a854081264e498e7a833");
            var invisibility = library.Get<BlueprintBuff>("525f980cb29bc2240b93e953974cb325");
            var greaterInvisibility = library.Get<BlueprintBuff>("e6b35473a237a6045969253beb09777c");
            var mist = library.Get<BlueprintBuff>("61b312b8f91cc48418768b77cd6dcc02");
            var checkConcealment = Helpers.CreateConditionsCheckerOr(Helpers.CreateConditionHasBuff(blur),
                        Helpers.CreateConditionHasBuff(displacement),
                        Helpers.CreateConditionHasBuff(invisibility),
                        Helpers.CreateConditionHasBuff(greaterInvisibility),
                        Helpers.CreateConditionHasBuff(mist));
            var applyFaerieFire = Helpers.CreateActionList(Helpers.CreateConditional(checkConcealment, ifTrue: Common.createContextActionApplyBuff(library.Get<BlueprintBuff>("cc383a9eaae4d2b45a925d442b367b54"),
            Helpers.CreateContextDuration(Common.createSimpleContextValue(1), DurationRate.Rounds))));


            //buffs
            var name = "EnhanceArrows";
            var displayName = "Enhance Arrows";
            var ghoustTouchBuff = Helpers.CreateBuff(name + "GhostTouch" + "Buff", displayName + " (Ghoust Touch)",
                $"Whilst active, your arrows gain the Ghost Touch Enchantment", "",
                CallOfTheWild.LoadIcons.Image2Sprite.Create(@"ArcaneArcher/enhanceArrowsGhost.png"), null,
                Helpers.Create<EnhanceArrowsGhost>(u => { u.weapon_types = allowed_weapons; }));
            var limningBuff = Helpers.CreateBuff(name + "Limning" + "Buff", displayName + " (Limning)",
                $"Whilst active, your arrows gain the Limning Enchantment", "",
                CallOfTheWild.LoadIcons.Image2Sprite.Create(@"ArcaneArcher/enhanceArrowsLimning.png"), null,
                Common.createAddInitiatorAttackWithWeaponTrigger(applyFaerieFire, check_weapon_range_type: true, range_type: AttackTypeAttackBonus.WeaponRangeType.Ranged, wait_for_attack_to_resolve: false),
                Common.createAbilityCasterMainWeaponCheck(WeaponCategory.Longbow, WeaponCategory.Shortbow));
            var planarBuff = Helpers.CreateBuff(name + "Planar" + "Buff", displayName + " (Planar)",
                $"Whilst active, your arrows gain the Planar Enchantment", "",
                CallOfTheWild.LoadIcons.Image2Sprite.Create(@"ArcaneArcher/enhanceArrowsPlanar.png"), null,
                Helpers.Create<EnhanceArrowsPlanar>(u => { u.weapon_types = allowed_weapons; }));

            var actionGhostTouch = Helpers.CreateRunActions(Common.createContextActionApplyBuff(ghoustTouchBuff,
                Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.StatBonus)), is_permanent: true, dispellable: false),
                Common.createContextActionRemoveBuff(limningBuff), Common.createContextActionRemoveBuff(planarBuff));
            var actionLimning = Helpers.CreateRunActions(Common.createContextActionApplyBuff(limningBuff,
                Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.StatBonus)), is_permanent: true, dispellable: false),
                Common.createContextActionRemoveBuff(ghoustTouchBuff), Common.createContextActionRemoveBuff(planarBuff));
            var actionPlanar = Helpers.CreateRunActions(Common.createContextActionApplyBuff(planarBuff,
                Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.StatBonus)), is_permanent: true, dispellable: false),
                Common.createContextActionRemoveBuff(ghoustTouchBuff), Common.createContextActionRemoveBuff(limningBuff));

            var abilityGhostTouch = Helpers.CreateAbility("EnhanceArrowsGhostTouchAbility",
                ghoustTouchBuff.Name, ghoustTouchBuff.Description, "", ghoustTouchBuff.Icon, AbilityType.Supernatural, CommandType.Free,
                AbilityRange.Weapon, "Permanent", "N/A", actionGhostTouch, Helpers.CreateResourceLogic(resource),
                Helpers.PrerequisiteClassLevel(arcaneArcher, 5));
            var abilityLimning = Helpers.CreateAbility("EnhanceArrowsLimningAbility",
                limningBuff.Name, limningBuff.Description, "", limningBuff.Icon, AbilityType.Supernatural, CommandType.Free,
                AbilityRange.Weapon, "Permanent", "N/A", actionLimning, Helpers.CreateResourceLogic(resource),
                Helpers.PrerequisiteClassLevel(arcaneArcher, 5));
            var abilityPlanar = Helpers.CreateAbility("EnhanceArrowsPlanarAbility",
                planarBuff.Name, planarBuff.Description, "", planarBuff.Icon, AbilityType.Supernatural, CommandType.Free,
                AbilityRange.Weapon, "Permanent", "N/A", actionPlanar, Helpers.CreateResourceLogic(resource),
                Helpers.PrerequisiteClassLevel(arcaneArcher, 5));

            corrosiveArrowsFeature = Helpers.CreateFeature("CorrosiveArrows", "Enhance Arrows (Corrosive)", "You add corrosive to the list of special properties you can grant your nonmagical arrows with your enhance arrows ability at 3rd level.",
            "", CallOfTheWild.LoadIcons.Image2Sprite.Create(@"ArcaneArcher/enhanceArrowsMagic.png"), FeatureGroup.None, Helpers.CreateAddFact(abilityCorrosive));

            specialArrowsFeature = Helpers.CreateFeature("SpecialArrows", "Enhance Arrows (Special)", "At 5th level, you can add the ghost touch, limning, or planar special ability.",
             "", CallOfTheWild.LoadIcons.Image2Sprite.Create(@"ArcaneArcher/enhanceArrowsMagic.png"), FeatureGroup.None, Helpers.CreateAddFact(abilityGhostTouch),
             Helpers.CreateAddFact(abilityLimning),
             Helpers.CreateAddFact(abilityPlanar),
             Helpers.CreateAddAbilityResource(resource));

            expandedEnhanceArrows = Helpers.CreateFeature("ExpandedEnhanceArrows", "Expanded Enhance Arrows",
            $"You add corrosive to the list of special properties you can grant your nonmagical arrows with your enhance arrows ability at 3rd level. " +
            "At 5th level, you can add the ghost touch, limning, or planar special ability. At 7th level, you can add the corrosive burst special " +
            "ability.",
            "",
            CallOfTheWild.LoadIcons.Image2Sprite.Create(@"FeatIcons/Icon_Expanded_Enhanced_Arrows.png"),
            FeatureGroup.Feat,
            Helpers.CreateAddFeatureOnClassLevel(corrosiveArrowsFeature, 3, getArcaneArcherArray(), null, false),
            Helpers.CreateAddFeatureOnClassLevel(specialArrowsFeature, 5, getArcaneArcherArray(), null, false),
            Helpers.PrerequisiteFeature(enhanceArrowsMagic)
            );
            library.AddFeats(expandedEnhanceArrows);
        }

        public static ContextCalculateHighestMentalStatForArrowOfDeath createContextCalculateHighestMentalStat()
        {
            var c = Helpers.Create<ContextCalculateHighestMentalStatForArrowOfDeath>();
            return c;
        }

    }

    public class EnhanceArrowsMagic : OwnedGameLogicComponent<UnitDescriptor>, IInitiatorRulebookHandler<RuleCalculateWeaponStats>, IInitiatorRulebookHandler<RuleCalculateAttackBonusWithoutTarget>
    {
        public BlueprintWeaponType[] weapon_types;

        public void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
        {
            if (!Array.Exists(weapon_types, t => t == evt.Weapon.Blueprint.Type))
            {
                return;
            }

            foreach (var e in evt.Weapon.Enchantments)
            {
                if (e.Blueprint.GetComponent<WeaponEnhancementBonus>() != null) { return; }
            }
            evt.AddBonusDamage(1);
            evt.Enhancement = 1;
        }

        public void OnEventDidTrigger(RuleCalculateWeaponStats evt) { }

        public void OnEventAboutToTrigger(RuleCalculateAttackBonusWithoutTarget evt)
        {
            if (!Array.Exists(weapon_types, t => t == evt.Weapon.Blueprint.Type))
            {
                return;
            }

            foreach (var e in evt.Weapon.Enchantments)
            {
                if (e.Blueprint.GetComponent<WeaponEnhancementBonus>() != null || e.Blueprint.GetComponent<WeaponMasterwork>() != null) { return; }
            }
            evt.AddBonus(1, Fact);
        }


        public void OnEventDidTrigger(RuleCalculateAttackBonusWithoutTarget evt) { }
    }

    public class EnhanceArrowsElemental : OwnedGameLogicComponent<UnitDescriptor>, IInitiatorRulebookHandler<RuleCalculateWeaponStats>, IInitiatorRulebookHandler<RuleDealDamage>
    {
        public BlueprintWeaponType[] weapon_types;
        public DamageEnergyType damage_type;
        static LibraryScriptableObject library => Main.library;
        public void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
        {
            if (!Array.Exists(weapon_types, t => t == evt.Weapon.Blueprint.Type))
            {
                return;
            }

            foreach (var e in evt.Weapon.Enchantments)
            {
                if (e.Blueprint.GetComponent<WeaponEnergyDamageDice>() != null) { if (e.Blueprint.GetComponent<WeaponEnergyDamageDice>().Element == damage_type) { return; } }
            }

            DamageDescription damageDescription = new DamageDescription()
            {
                TypeDescription = new DamageTypeDescription()
                {
                    Type = DamageType.Energy,
                    Energy = damage_type
                },
                Dice = new DiceFormula(1, DiceType.D6)
            };

            evt.DamageDescription.Add(damageDescription);

        }

        public void OnEventDidTrigger(RuleCalculateWeaponStats evt) { }

        public void OnEventAboutToTrigger(RuleDealDamage evt)
        {
            var weapon = Owner.Body.PrimaryHand.HasWeapon ? Owner.Body.PrimaryHand.MaybeWeapon : Owner.Body.EmptyHandWeapon;
            if (!Array.Exists(weapon_types, t => t == weapon.Blueprint.Type))
            {
                return;
            }
            if (Owner.Progression.GetClassLevel(library.Get<BlueprintCharacterClass>("0fbf5e3fe02f4db19492659dc8a3c411")) >= 7)
            {
                RuleAttackRoll attackRoll = evt.AttackRoll;
                if (base.Owner == null || attackRoll == null || !attackRoll.IsCriticalConfirmed || attackRoll.FortificationNegatesCriticalHit)
                {
                    return;
                }
                RuleCalculateWeaponStats ruleCalculateWeaponStats = Rulebook.Trigger<RuleCalculateWeaponStats>(new RuleCalculateWeaponStats(evt.Initiator, weapon, null));
                DiceFormula dice = new DiceFormula(Math.Max(ruleCalculateWeaponStats.CriticalMultiplier - 1, 1), DiceType.D10);
                evt.DamageBundle.Add(new EnergyDamage(dice, damage_type));

            }
        }

        public void OnEventDidTrigger(RuleDealDamage evt) { }

    }

    public class EnhanceArrowsAligned : OwnedGameLogicComponent<UnitDescriptor>, IInitiatorRulebookHandler<RuleDealDamage>
    {
        public BlueprintWeaponType[] weapon_types;
        public string alignment;
        public DamageEnergyType damage_type;
        static LibraryScriptableObject library => Main.library;
        public void OnEventAboutToTrigger(RuleDealDamage evt)
        {
            var weapon = Owner.Body.PrimaryHand.HasWeapon ? Owner.Body.PrimaryHand.MaybeWeapon : Owner.Body.EmptyHandWeapon;
            if (!Array.Exists(weapon_types, t => t == weapon.Blueprint.Type))
            {
                return;
            }
            if (alignment == "Holy")
            {
                if (evt.Target.Blueprint.Alignment != Alignment.ChaoticEvil
                || evt.Target.Blueprint.Alignment != Alignment.LawfulEvil
                || evt.Target.Blueprint.Alignment != Alignment.NeutralEvil)
                {
                    { return; }
                }
                var holy = library.Get<BlueprintWeaponEnchantment>("28a9964d81fedae44bae3ca45710c140");
                if (weapon.HasEnchantment(holy)) { return; }
            }
            if (alignment == "Unholy")
            {
                if (evt.Target.Blueprint.Alignment != Alignment.ChaoticGood
                || evt.Target.Blueprint.Alignment != Alignment.LawfulGood
                || evt.Target.Blueprint.Alignment != Alignment.NeutralGood)
                {
                    { return; }
                }
                var unholy = library.Get<BlueprintWeaponEnchantment>("d05753b8df780fc4bb55b318f06af453");
                if (weapon.HasEnchantment(unholy)) { return; }
            }
            if (alignment == "Anarchic")
            {
                if (evt.Target.Blueprint.Alignment != Alignment.LawfulGood
                || evt.Target.Blueprint.Alignment != Alignment.LawfulNeutral
                || evt.Target.Blueprint.Alignment != Alignment.LawfulEvil)
                {
                    { return; }
                }
                var anarchic = library.Get<BlueprintWeaponEnchantment>("57315bc1e1f62a741be0efde688087e9");
                if (weapon.HasEnchantment(anarchic)) { return; }
            }
            if (alignment == "Axiomatic")
            {
                if (evt.Target.Blueprint.Alignment != Alignment.ChaoticGood
                || evt.Target.Blueprint.Alignment != Alignment.ChaoticNeutral
                || evt.Target.Blueprint.Alignment != Alignment.ChaoticEvil)
                {
                    { return; }
                }
                var axiomatic = library.Get<BlueprintWeaponEnchantment>("0ca43051edefcad4b9b2240aa36dc8d4");
                if (weapon.HasEnchantment(axiomatic)) { return; }
            }

            RuleCalculateWeaponStats ruleCalculateWeaponStats = Rulebook.Trigger<RuleCalculateWeaponStats>(new RuleCalculateWeaponStats(evt.Initiator, weapon, null));
            var dice = new DiceFormula(2, DiceType.D6);
            evt.DamageBundle.Add(new EnergyDamage(dice, damage_type));
        }

        public void OnEventDidTrigger(RuleDealDamage evt) { }
    }

    public class SeekerArrow : OwnedGameLogicComponent<UnitDescriptor>, IInitiatorRulebookHandler<RuleAttackRoll>
    {

        static LibraryScriptableObject library => Main.library;
        public void OnEventAboutToTrigger(RuleAttackRoll evt)
        {
            evt.IgnoreConcealment = true;
        }

        public void OnEventDidTrigger(RuleAttackRoll evt)
        {
        }
    }

    public class PhaseArrow : OwnedGameLogicComponent<UnitDescriptor>, IInitiatorRulebookHandler<RuleAttackRoll>
    {

        static LibraryScriptableObject library => Main.library;
        public void OnEventAboutToTrigger(RuleAttackRoll evt)
        {
            evt.IgnoreConcealment = true;
            evt.AttackType = AttackType.RangedTouch;
        }

        public void OnEventDidTrigger(RuleAttackRoll evt)
        {
        }
    }

    public class ContextActionAttackInRange : ContextAction
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

                RuleAttackWithWeapon attackWithWeapon = new RuleAttackWithWeapon(maybeCaster, target.Unit, maybeCaster.Body.PrimaryHand.MaybeWeapon, 0);
                attackWithWeapon.Reason = (RuleReason)this.Context;
                RuleAttackWithWeapon rule = attackWithWeapon;

                if (maybeCaster.DistanceTo(target.Unit) <= maybeCaster.Body.PrimaryHand.MaybeWeapon.AttackRange.Meters)
                {
                    Log.Write("Attacking");
                    this.Context.TriggerRule<RuleAttackWithWeapon>(rule);
                    if (rule.AttackRoll.IsHit)
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


    public class EnhanceArrowsGhost : OwnedGameLogicComponent<UnitDescriptor>, IInitiatorRulebookHandler<RulePrepareDamage>
    {
        public BlueprintWeaponType[] weapon_types;

        static LibraryScriptableObject library => Main.library;
        public void OnEventAboutToTrigger(RulePrepareDamage evt)
        {
            ItemEntityWeapon weapon = evt.DamageBundle.Weapon;
            if (!Array.Exists(weapon_types, t => t == weapon.Blueprint.Type))
            {
                return;
            }

            if (weapon.HasEnchantment(library.Get<BlueprintWeaponEnchantment>("47857e1a5a3ec1a46adf6491b1423b4f")))
            {
                return;
            }

            foreach (BaseDamage baseDamage in evt.DamageBundle)
            {
                baseDamage.Reality = DamageRealityType.Ghost;
            }

        }

        public void OnEventDidTrigger(RulePrepareDamage evt) { }

    }

    public class EnhanceArrowsPlanar : OwnedGameLogicComponent<UnitDescriptor>, IInitiatorRulebookHandler<RuleCalculateDamage>
    {
        public BlueprintWeaponType[] weapon_types;

        static LibraryScriptableObject library => Main.library;
        public void OnEventAboutToTrigger(RuleCalculateDamage evt)
        {
            var weapon = Owner.Body.PrimaryHand.HasWeapon ? Owner.Body.PrimaryHand.MaybeWeapon : Owner.Body.EmptyHandWeapon;
            if (!Array.Exists(weapon_types, t => t == weapon.Blueprint.Type))
            {
                return;
            }

            if (evt.Target.Descriptor.HasFact(library.Get<BlueprintUnitFact>("9054d3988d491d944ac144e27b6bc318")))
            {
                evt.DamageBundle.WeaponDamage.SetReductionPenalty(5);
            }

        }

        public void OnEventDidTrigger(RuleCalculateDamage evt) { }

    }


 public class ContextCalculateHighestMentalStatForArrowOfDeath : ContextAbilityParamsCalculator
        {
            public StatType statType = StatType.Charisma;

            public override AbilityParams Calculate(MechanicsContext context)
            {
                UnitEntityData maybeCaster = context.MaybeCaster;
                if (maybeCaster == null)
                {
                    return context.Params;
                }

                var charisma = maybeCaster.Stats.Charisma.ModifiedValue;
                var intelligence = maybeCaster.Stats.Intelligence.ModifiedValue;
                var wisdom = maybeCaster.Stats.Wisdom.ModifiedValue;

                if(wisdom >= intelligence && wisdom >= charisma && ATouchOfMagic.Main.settings.bestMentalStat){
                    statType = StatType.Wisdom;
                } else if (intelligence >= charisma && intelligence >= wisdom && ATouchOfMagic.Main.settings.bestMentalStat){
                    statType = StatType.Intelligence;
                } else {
                    statType = StatType.Charisma;
                }

                AbilityData ability = context.SourceAbilityContext?.Ability;
                RuleCalculateAbilityParams rule = !(ability != (AbilityData)null) ? new RuleCalculateAbilityParams(maybeCaster, context.AssociatedBlueprint, (Spellbook)null) : new RuleCalculateAbilityParams(maybeCaster, ability);
                rule.ReplaceStat = new StatType?(statType);
                rule.ReplaceCasterLevel = 20;

                return context.TriggerRule<RuleCalculateAbilityParams>(rule).Result;
            }

            public override void Validate(ValidationContext context)
            {
                base.Validate(context);
                if (this.statType.IsAttribute() || this.statType == StatType.BaseAttackBonus)
                    return;
                string str = string.Join(", ", ((IEnumerable<StatType>)StatTypeHelper.Attributes).Select<StatType, string>((Func<StatType, string>)(s => s.ToString())));
                context.AddError("StatType must be Base Attack Bonus or an attribute: {0}", (object)str);
            }
        }

}


