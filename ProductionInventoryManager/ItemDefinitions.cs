using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {

        void InitRefineryBlueprints()
        {

            RefineryBlueprint.InitScrapTypeBlueprintTypes();

            // Ingots
            AddRefineryBlueprint("StoneOreToIngotBasic", Ore.Stone, Ingot.Stone);
            AddRefineryBlueprint("ScrapToIronIngot", Ore.Scrap, Ingot.Iron);
            AddRefineryBlueprint("ScrapIngotToIronIngot", Ingot.Scrap, Ingot.Iron);
            AddRefineryBlueprintOreToIngot("Gold");
            AddRefineryBlueprintOreToIngot("Platinum");
            AddRefineryBlueprintOreToIngot("Stone");
            AddRefineryBlueprintOreToIngot("Silver");
            AddRefineryBlueprintOreToIngot("Iron");
            AddRefineryBlueprintOreToIngot("Nickel");
            AddRefineryBlueprintOreToIngot("Cobalt");
            AddRefineryBlueprintOreToIngot("Silicon");
            AddRefineryBlueprintOreToIngot("Uranium");

            if (usedMods[M_DeuteriumReactor])
            {
                AddRefineryBlueprint("StonetoDeuterium", Ore.Stone, Ingot.DeuteriumContainer);
                AddRefineryBlueprint("IcetoDeuterium", Ore.Ice, Ingot.DeuteriumContainer);
                AddRefineryBlueprint("DeuteriumOreToIngot", Ore.Deuterium, Ingot.DeuteriumContainer);
            }
            if (usedMods[M_DailyNeedsSurvival])
            {
                AddRefineryBlueprintOreToIngot("Carbon");
                AddRefineryBlueprintOreToIngot("Potassium");
                AddRefineryBlueprintOreToIngot("Phosphorus");
            }
            if (usedMods[M_SG_Ores])
            {
                AddRefineryBlueprintOreToIngot("Naquadah");
                AddRefineryBlueprintOreToIngot("Trinium");
                AddRefineryBlueprintOreToIngot("Neutronium");
            }
            if (!usedMods[M_IndustrialOverhaulMod])
            {
                AddRefineryBlueprintOreToIngot("Magnesium");
            }
            else // IndustrialOverhaulMod
            {
                string[] ioModResources = { "Iron", "Nickel", "Cobalt", "Silicon", "Silver", "Gold", "Platinum", "Uranium", "Copper", "Lithium", "Bauxite", "Titanium", "Tantalum", "Sulfur", };
                // SmelterIngots & RefineryIngots
                AddRefineryBlueprintOreToIngot("Copper");
                AddRefineryBlueprint("BauxiteOreToIngot", Ore.Bauxite, Ingot.Aluminium);
                AddRefineryBlueprintOreToIngot("Titanium");
                AddRefineryBlueprintOreToIngot("Tantalum");
                AddRefineryBlueprint("CoalToCarbonBasic", Ore.Coal, Ingot.Carbon);
                // CrushedToIngot
                AddRefineryBlueprintsArray(ioModResources, "Crushed", "OreToIngot", "Ore Crushed", "Ingot ");
                // PurifiedToIngot
                AddRefineryBlueprintsArray(ioModResources, "Purified", "OreToIngot", "Ore Purified", "Ingot ");
                // Crusher
                AddRefineryBlueprintsArray(ioModResources, "Crush", "Ore", "Ore ", "Ore Crushed");
                AddRefineryBlueprint("CrushNiterOre", Ore.Niter, Ore.Magnesium);
                AddRefineryBlueprint("CrushStoneOre", Ore.Stone, Ingot.Stone);
                // Purifier
                AddRefineryBlueprintsArray(ioModResources, "Purify", "Ore", "Ore Crushed", "Ore Purified");
                AddRefineryBlueprint("PurifyNiterOre", Ore.Magnesium, "Ore PurifiedNiter");
                // ChemicalPlantOre
                AddRefineryBlueprintOreToIngot("Niter");
                AddRefineryBlueprintOreToIngot("Lithium");
                AddRefineryBlueprintOreToIngot("Sulfur");
                AddRefineryBlueprint("CoalToCarbon", Ore.Coal, Ingot.Carbon);
                AddRefineryBlueprint("CrushedNiterOreToIngot", Ore.Magnesium, Ingot.Niter);
                AddRefineryBlueprint("PurifiedNiterOreToIngot", "Ore PurifiedNiter", Ingot.Niter);
                // BitumenExtractor
                AddRefineryBlueprint("OilSandToCrudeOil", "Ore OilSand", "Ore CrudeOil");
                // OilCracking
                AddRefineryBlueprint("CrudeOilCracking", "Ore CrudeOil", "Ingot FuelOil");
                // CentrifugeIngots
                AddRefineryBlueprintOreToIngot("Uranium");
            }
            refineryBlueprints.Sort((x, y) => x.InputIDName.CompareTo(y.InputIDName));
        }



        void InitAssemblerBluePrints()
        {


            if (!usedMods[M_IndustrialOverhaulMod])
            {
                // blueprint vanilla
                // Components
                C("ConstructionComponent");
                C("GirderComponent");
                C("MetalGrid");
                C("InteriorPlate");
                C("SteelPlate");
                C("SmallTube");
                C("LargeTube");
                C("MotorComponent");
                C("Display");
                C("BulletproofGlass");
                C("ComputerComponent");
                C("ReactorComponent");
                C("ThrustComponent");
                C("GravityGeneratorComponent");
                C("MedicalComponent");
                C("RadioCommunicationComponent");
                C("DetectorComponent");
                C("ExplosivesComponent");
                C("SolarCell");
                C("PowerCell");
                C("Superconductor");
                // Equipment
                O("Position0010_OxygenBottle");
                H("Position0020_HydrogenBottle");
                C("Position0030_Canvas");
                D("Position0040_Datapad");
                T("Position0005_FlareGun", "FlareGunItem");
                A("Position0005_FlareGunMagazine", "FlareClip");
                A("Position0007_FireworksBoxBlue");
                A("Position00071_FireworksBoxGreen");
                A("Position00072_FireworksBoxRed");
                A("Position00073_FireworksBoxYellow");
                A("Position00074_FireworksBoxPink");
                A("Position00075_FireworksBoxRainbow");
                // Tools
                T("Position0010_AngleGrinder");
                T("Position0020_AngleGrinder2");
                T("Position0030_AngleGrinder3");
                T("Position0040_AngleGrinder4");
                T("Position0050_HandDrill");
                T("Position0060_HandDrill2");
                T("Position0070_HandDrill3");
                T("Position0080_HandDrill4");
                T("Position0090_Welder");
                T("Position0100_Welder2");
                T("Position0110_Welder3");
                T("Position0120_Welder4");
                // Weapons
                T("Position0010_SemiAutoPistol");
                T("Position0020_FullAutoPistol");
                T("Position0030_EliteAutoPistol");
                T("Position0040_AutomaticRifle");
                T("Position0050_RapidFireAutomaticRifle");
                T("Position0060_PreciseAutomaticRifle");
                T("Position0070_UltimateAutomaticRifle");
                T("Position0080_BasicHandHeldLauncher");
                T("Position0090_AdvancedHandHeldLauncher");
                // Ammo
                A("Position0010_SemiAutoPistolMagazine");
                A("Position0020_FullAutoPistolMagazine");
                A("Position0030_ElitePistolMagazine");
                A("Position0040_AutomaticRifleGun_Mag_20rd");
                A("Position0050_RapidFireAutomaticRifleGun_Mag_50rd");
                A("Position0060_PreciseAutomaticRifleGun_Mag_5rd");
                A("Position0080_NATO_25x184mmMagazine");
                A("Position0070_UltimateAutomaticRifleGun_Mag_30rd");
                A("Position0090_AutocannonClip");
                A("Position0100_Missile200mm");
                A("Position0110_MediumCalibreAmmo");
                A("Position0120_LargeCalibreAmmo");
                A("Position0130_SmallRailgunAmmo");
                A("Position0140_LargeRailgunAmmo");
            }


            /* DailyNeedsSurvivalMod */
            curmod = M_DailyNeedsSurvival;
            I("SubFresh", "", "Algae-Soy Product");
            I("WaterFood", "", "Drinking Water Packet");
            I("OrganicToNutrients", "Nutrients", "Nutrients");
            I("ArtificialFood");
            I("LuxuryMeal");
            I("SabiroidSteak");
            I("VeganFood");
            I("WolfSteak");
            I("WolfBouillon");
            I("SabiroidBouillon");
            I("CoffeeFood", "", "Coffee");
            I("EmergencyWater", "WaterFood", "Emergency Water Packets");
            I("EmergencyFood");
            E("ClonedAnimalMeat", "WolfMeat");
            E("ClonedSabiroidMeat", "SabiroidMeat");
            I("Potatoes");
            I("Tomatoes");
            I("Carrots");
            I("Cucumbers");
            I("PotatoSeeds");
            I("TomatoSeeds");
            I("CarrotSeeds");
            I("CucumberSeeds");
            I("Ketchup");
            I("MartianSpecial");
            I("OrganicToFertilizer", "Fertilizer", "Organic Fertilizer");
            I("IngotsToFertilizer", "Fertilizer", "Artificial Fertilizer");
            I("NotBeefBurger");
            I("ToFurkey", "", "ToFurkey Dinner");
            I("SpaceMealBar");
            I("HotChocolate");
            I("SpacersBreakfast");
            I("ProteinShake");

            /* AzimuthThrusterMod */
            curmod = M_AzimuthThruster;
            C("AzimuthSuperchargerComponent", "AzimuthSupercharger");

            /* StargateMods */
            curmod = M_SG_Ores;
            C("Naquadah", "", "Naquadah Bars");
            C("Trinium", "", "Trinium Plate");
            C("Neutronium", "", "Neutronium Crate");

            if (!usedMods[M_SG_Ores])
            {
                curmod = M_SG_Gates;
                C("Naquadah", "", "Naquadah Bars");
            }

            /* PaintGunMod */
            curmod = M_PaintGun;
            T("Blueprint_PaintGun", "PhysicalPaintGun");
            A("Blueprint_PaintGunMag", "PaintGunMag");

            /* DeuteriumReactorMod */
            curmod = M_DeuteriumReactor;
            C("Magnetron_Component");
            I("DeuteriumOreToIngot", "DeuteriumContainer", "Deuterium");
            I("StonetoDeuterium", "DeuteriumContainer", "Deuterium (Stone)");
            I("IcetoDeuterium", "DeuteriumContainer", "Deuterium (Ice)");

            /* DefenseShieldMod */
            curmod = M_Shield;
            C("ShieldComponentBP", "ShieldComponent", "Field Emitter");

            /* MCRN RailGunMod */
            curmod = M_RailGun;
            A("RailGunAmmoMag");

            /* MWI Homing Weaponry Mod */
            curmod = M_HomingWeaponry;
            A("TorpedoMk1_Blueprint", "TorpedoMk1");
            A("SwarmMissileMk1_Blueprint", "SwarmMissile50mm");
            A("DestroyerMissileX_Blueprint", "DestroyerMissileX");
            A("DestroyerMissileMk1_Blueprint", "DestroyerMissileMk1");

            /* Industrial Overhaul Mod */
            curmod = M_IndustrialOverhaulMod;
            // Reprocessor
            I(Refinery.BluePrint_SpentFuelReprocessing, "Uranium", "Nuclear Fuel");
            // AssemblingBenchComponents
            C("CopperWire");
            C("Electromagnet");
            C("Lightbulb");
            C("AcidPowerCell");
            C("AlkalinePowerCell");
            C("HeatingElement");
            C("POConstructionComponent", "Construction");
            C("POSteelPlate", "SteelPlate");
            C("POSmallTube", "SmallTube");
            C("POLargeTube", "LargeTube");
            C("POMotorComponent", "Motor");
            C("POComputerComponent", "Computer");
            C("PORadioCommunicationComponent", "RadioCommunication");
            // FabricatorComponents
            C("Electromagnet");
            C("Lightbulb");
            C("AlkalinePowerCell");
            C("HeatingElement");
            C("POMetalGrid", "MetalGrid");
            // WireDrawerComponents
            C("GoldWire");
            C("POSuperconductor", "Superconductor");
            // PlateStampComponents
            C("CompositeArmor");
            C("TitaniumPlate");
            C("ArmoredPlate");
            C("POInteriorPlate", "InteriorPlate");
            // ExtruderComponents
            C("POGirderComponent", "Girder");
            // SiliconFuserComponents
            C("ArmorGlass");
            C("Ceramic");
            C("POBulletproofGlass", "BulletproofGlass");
            // CementKilnComponents
            C("Concrete");
            // AssemblerComponents
            C("Capacitor");
            C("Cryocooler");
            C("POMedicalComponent", "Medical");
            C("POSolarCell", "SolarCell");
            // MicroelectronicsFactoryComponents
            C("Thermocouple");
            C("AdvancedComputer");
            C("PODisplay", "Display");
            C("PODetectorComponent", "Detector");
            // AutoLoomComponents
            C("Fabric");
            C("POCanvas", "Canvas");
            // AdvancedAssemblerComponents
            C("TokamakBlanket");
            C("SuperMagnet");
            C("LaserEmitter");
            C("POReactorComponent", "Reactor");
            C("POPowerCell", "PowerCell");
            // NanoAssemblerComponents
            C("ElectronMatrix");
            C("QuantumComputer");
            C("POThrustComponent", "Thrust");
            C("POGravityGeneratorComponent", "GravityGenerator");
            C("FSSolarCell");
            // MunitionsFactory
            C("POExplosivesComponent", "Explosives");
            I("Gunpowder", "Magnesium");
            A("POInteriorTurret_Mag_50rd", "InteriorTurret_Mag_50rd");
            A("PONATO_25x184mmMagazine", "NATO_25x184mm");
            A("DUNATO_25x184mmMagazine", "DUNATO_25x184mm");
            A("POMissile200mm", "Missile200mm");
            A("POFullAutoPistolMagazine", "FullAutoPistolMagazine");
            A("POElitePistolMagazine", "ElitePistolMagazine");
            A("POAutomaticRifleGun_Mag_20rd", "AutomaticRifleGun_Mag_20rd");
            A("PORapidFireAutomaticRifleGun_Mag_50rd", "RapidFireAutomaticRifleGun_Mag_50rd");
            A("POPreciseAutomaticRifleGun_Mag_5rd", "PreciseAutomaticRifleGun_Mag_5rd");
            A("POUltimateAutomaticRifleGun_Mag_30rd", "UltimateAutomaticRifleGun_Mag_30rd");
            A("AutocannonClipDUAP");
            A("MediumCalibreAmmoHE");
            A("MediumCalibreAmmoDUAP");
            A("LargeCalibreAmmoHE");
            A("LargeCalibreAmmoDUAP");
            A("SmallRailgunAmmoDUAP");
            // ChemicalPlantComponents
            C("PolymerToPlastic", "Plastic");
            C("Rubber");
            I("SyntheticPolymer", "Polymer");
            C("Asphalt");
            // T1Tools
            T("POWelder", "WelderItem");
            T("POAngleGrinder", "AngleGrinderItem");
            T("POHandDrill", "HandDrillItem");
            T("POSemiAutoPistol", "SemiAutoPistolItem");
            A("POSemiAutoPistolMagazine", "SemiAutoPistolMagazine");
            // Bottles
            O("POOxygenBottle", "OxygenBottle");
            H("POHydrogenBottle", "HydrogenBottle");
            // T2Tools
            T("POWelder2", "Welder2Item");
            T("POAngleGrinder2", "AngleGrinder2Item");
            T("POHandDrill2", "HandDrill2Item");
            T("POAutomaticRifle", "AutomaticRifleItem");
            T("POPreciseAutomaticRifle", "PreciseAutomaticRifleItem");
            T("PORapidFireAutomaticRifle", "RapidFireAutomaticRifleItem");
            T("POBasicHandHeldLauncher", "BasicHandHeldLauncherItem");
            T("POFullAutoPistol", "FullAutoPistolItem");
            // T3Tools
            T("POWelder3", "Welder3Item");
            T("POAngleGrinder3", "AngleGrinder3Item");
            T("POHandDrill3", "HandDrill3Item");
            T("POUltimateAutomaticRifle", "UltimateAutomaticRifleItem");
            T("POAdvancedHandHeldLauncher", "AdvancedHandHeldLauncherItem");
            T("POEliteAutoPistol", "ElitePistolItem");
            // T4Tools
            T("POWelder4", "Welder4Item");
            T("POAngleGrinder4", "AngleGrinder4Item");
            T("POHandDrill4", "HandDrill4Item");

            // IndustrialOverhaulWater Mod
            curmod = M_IndustrialOverhaulWaterMod;
            C("Foam");
            C("BuoyancyTube");

            // IndustrialOverhaulLockLoad Mod
            curmod = M_IndustrialOverhaulLLMod;
            // CompressedGravel
            A("GravelMag");
            A("GravelMagBig");
            // MunitionsFactory
            A("155mmAPShell");
            A("155mmDUAPShell");
            A("155mmHEShell");
            A("305mmAPShell");
            A("305mmDUAPShell");
            A("305mmHEShell");
            A("APCoilgunShell");
            A("DUAPCoilgunShell");
            A("CLGGMag", "CLGG");

            curmod = M_EatDrinkSleep;
            // Emergency
            K("SparklingWater");
            K("Emergency_Ration");

            curmod = M_PlantCook;
            // Farming
            I("Soya");
            I("Herbs");
            // Cooking
            K("AppleJuice");
            K("ApplePie");
            K("Tofu");
            K("MeatRoasted");
            K("ShroomSteak");
            K("Bread");
            K("Burger");
            K("Soup");
            K("MushroomSoup");
            K("TofuSoup");
            K("SparklingWaterCan", "SparklingWater");
            K("EuropaTea");
            // MushroomsFarm
            K("FarmedMushrooms", "Mushrooms");
            // AppleFarm
            K("FarmedApples", "Apple");
            // WheatFarm
            I("FarmedWheat", "Wheat");
            // HerbsFarm
            I("FarmedHerbs", "Herbs");
            // Soya_Farm
            I("FarmedSoya", "Soya");
            // VegetablesFarm
            I("FarmedPumpkin", "Pumpkin");
            I("FarmedCabbage", "Cabbage");

            curmod = M_AryxEpsteinDrive;
            C("AryxLynxon_FusionComponentBP", "AryxLynxon_FusionComponent", "Fusion Coil");

            curmod = M_HSR;
            // Components
            C("K_HSR_Component_Rail_Vanilla", "K_HSR_RailComponents");
            A("K_HSR_Ammuntion_Recipe_Slug", "K_HSR_Slug");
            C("K_HSR_Component_AssemblerStabilizer_Vanilla", "K_HSR_AssemblerSystem");
            // HSR_BlueprintClassFolder
            C("K_HSR_Component_Rail", "K_HSR_RailComponents");
            C("K_HSR_Component_PulseSystem", "K_HSR_PulseSystem");
            C("K_HSR_Component_Globe", "K_HSR_Globe");
            C("K_HSR_Component_Circuitry", "K_HSR_HyperConductiveCircuitry");
            C("K_HSR_Component_HexPlate", "K_HSR_HexagolPlating");
            C("K_HSR_Component_Conduit", "K_HSR_GelConduit");
            C("K_HSR_Component_GlobeII", "K_HSR_Globe_II");
            C("K_HSR_Component_RailII", "K_HSR_RailComponents_II");
            C("K_HSR_Component_HexPlateII", "K_HSR_HexagolPlating_II");
            C("K_HSR_Component_RailIII", "K_HSR_RailComponents_III");
            // HSR_BlueprintClassFolderII
            A("K_HSR_Ammuntion_Recipe_Slug_Automated", "K_HSR_Slug");
            // HSR_BlueprintClassFolderIII
            I("K_HSR_NaniteSludge_Recipe", "K_HSR_Nanites_Sludge");
            I("K_HSR_EnergizedGel_Recipe", "K_HSR_Nanites_EnergizedGel");
            I("K_HSR_Hexagol_Recipe", "K_HSR_Nanites_Hexagol");
            I("K_HSR_Chromium_Recipe", "K_HSR_Nanites_Chromium");

            curmod = M_NorthWindWeapons;
            // Ammo
            A("R75ammo", "", "75mm Railgun Ammo");
            A("R150ammo", "", "150mm Railgun Ammo");
            A("R250ammo", "", "250mm Railgun Ammo");
            A("H203Ammo", "", "203mm HE Ammo");
            A("H203AmmoAP", "", "203mm AP Ammo");
            A("C30Ammo", "", "30mm Standard Ammo");
            A("C30DUammo", "", "30mm DU Ammo");
            A("CRAM30mmAmmo", "", "30mm C-RAM Ammo");
            A("C100mmAmmo", "", "100mm HE Ammo");
            A("C300AmmoAP", "", "300mm AP Ammo");
            A("C300AmmoHE", "", "300mm HE Ammo");
            A("C300AmmoG", "", "300mm Guided Ammo");
            A("C400AmmoAP", "", "400mm AP Ammo");
            A("C400AmmoHE", "", "400mm HE Ammo");
            A("C400AmmoCluster", "", "400mm Cluster Ammo");
            A("C500AmmoAP", "", "500mm AP Ammo");
            A("C500AmmoHE", "", "500mm HE Ammo");
            A("C500AmmoCasaba", "", "500mm Casaba Ammo");
            A("PlasmaCell10MJ", "", "10MJ PlasmaCannon Cell");

            curmod = "";
        }


        public class Resources
        {
            public const string RPowder = "powder";
            public const string RMagnesium = "Magnesium";
            public const string RStone = "Stone";
            public const string RIron = "Iron";
            public const string RNickel = "Nickel";
            public const string RSilicon = "Silicon";
            public const string RCobalt = "Cobalt";
            public const string RPlatinum = "Platinum";
            public const string RUranium = "Uranium";
            public const string RScrap = "Scrap";
            public const string RCarbon = "Carbon";
            public const string RPotassium = "Potassium";
            public const string RPhosphorus = "Phosphorus";
            public const string RNaquadah = "Naquadah";
            public const string RTrinium = "Trinium";
            public const string RNeutronium = "Neutronium";
            public const string RCopper = "Copper";
            public const string RLithium = "Lithium";
            public const string RBauxite = "Bauxite";
            public const string RTitanium = "Titanium";
            public const string RTantalum = "Tantalum";
            public const string RSulfur = "Sulfur";
            public const string RNiter = "Niter";
            public const string RCoal = "Coal";
            public const string RDeuterium = "Deuterium";
            // public const string R = "";
        }
        public class Ingot : Resources
        {
            const string prefix = "Ingot ";
            public const string Scrap = prefix + RScrap;
            public const string Magnesium = prefix + RMagnesium;
            public const string Magnesiumpowder = RMagnesium + RPowder;
            public const string Gunpowder = "Gun" + RPowder;
            public const string Stone = prefix + RStone;
            public const string Iron = prefix + RIron;
            public const string Nickel = prefix + RNickel;
            public const string Silicon = prefix + RSilicon;
            public const string Cobalt = prefix + RCobalt;
            public const string Platinum = prefix + RPlatinum;
            public const string Uranium = prefix + RUranium;
            public const string WaterFood = prefix + "WaterFood";
            public const string Nutrients = prefix + "Nutrients";
            public const string SubFresh = prefix + "SubFresh";
            public const string GreyWater = prefix + "GreyWater";
            public const string CleanWater = prefix + "CleanWater";
            public const string SpentFuel = prefix + "SpentFuel";
            public const string Niter = prefix + RNiter;
            public const string DeuteriumContainer = prefix + RDeuterium + "Container";
            public const string Carbon = prefix + RCarbon;
            public const string Potassium = prefix + RPotassium;
            public const string Phosphorus = prefix + RPhosphorus;
            public const string Naquadah = prefix + RNaquadah;
            public const string Trinium = prefix + RTrinium;
            public const string Neutronium = prefix + RNeutronium;
            public const string Copper = prefix + RCopper;
            public const string Lithium = prefix + RLithium;
            public const string Titanium = prefix + RTitanium;
            public const string Tantalum = prefix + RTantalum;
            public const string Sulfur = prefix + RSulfur;
            public const string Aluminium = prefix + "Aluminium";
            // public const string  = prefix + "";
        }
        public class Ore : Resources
        {
            const string prefix = "Ore ";
            public const string Scrap = prefix + RScrap;
            public const string Magnesium = prefix + RMagnesium;
            public const string Stone = prefix + RStone;
            public const string Iron = prefix + RIron;
            public const string Nickel = prefix + RNickel;
            public const string Silicon = prefix + RSilicon;
            public const string Cobalt = prefix + RCobalt;
            public const string Platinum = prefix + RPlatinum;
            public const string Uranium = prefix + RUranium;
            public const string Organic = prefix + "Organic";
            public const string Ice = prefix + "Ice";
            public const string Deuterium = prefix + RDeuterium;
            public const string Carbon = prefix + RCarbon;
            public const string Potassium = prefix + RPotassium;
            public const string Phosphorus = prefix + RPhosphorus;
            public const string Naquadah = prefix + RNaquadah;
            public const string Trinium = prefix + RTrinium;
            public const string Neutronium = prefix + RNeutronium;
            public const string Niter = prefix + RNiter;
            public const string Copper = prefix + RCopper;
            public const string Lithium = prefix + RLithium;
            public const string Bauxite = prefix + RBauxite;
            public const string Titanium = prefix + RTitanium;
            public const string Tantalum = prefix + RTantalum;
            public const string Sulfur = prefix + RSulfur;
            public const string Coal = prefix + RCoal;
            // public const string  = prefix + "";
        }
    }
}
