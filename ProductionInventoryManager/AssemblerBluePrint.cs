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
        void addBluePrint(string typeID, string subtypeID, string itemName, string alternativItemName)
        {
            if (curmod != "Vanilla" && (usedMods.ContainsKey(curmod) ? !usedMods[curmod] : true)) return;
            MyDefinitionId id;
            if (!MyDefinitionId.TryParse("MyObjectBuilder_BlueprintDefinition/" + subtypeID, out id)) return;
            if (subtypeID.StartsWith("Position0")) subtypeID = subtypeID.Substring(subtypeID.IndexOf('_') + 1);
            if (!mods.Contains(curmod)) mods.Add(curmod);
            var bpi = typeID + " " + subtypeID;
            if (!bprints_pool.ContainsKey(bpi)) bprints_pool.Add(bpi, new AssemblerBluePrint(typeID, subtypeID, curmod, (itemName == "" ? "" : typeID + " " + itemName), alternativItemName, id));
        }
        void C(string s, string astr = "", string astn = "") { addBluePrint("Component", s, astr, astn); }
        void A(string s, string astr = "", string astn = "") { addBluePrint(IG_Ammo, s, astr, astn); }
        void T(string s, string astr = "", string astn = "") { addBluePrint(IG_Tools, s, astr, astn); }
        void K(string s, string astr = "", string astn = "") { addBluePrint(IG_Kits, s, astr, astn); }
        void O(string s, string astr = "", string astn = "") { addBluePrint(IG_OBottles, s, astr, astn); }
        void H(string s, string astr = "", string astn = "") { addBluePrint(IG_HBottles, s, astr, astn); }
        void D(string s, string astr = "", string astn = "") { addBluePrint(IG_Datas, s, astr, astn); }
        void I(string blueprintNmae, string itemName = "", string alternativeItemName = "") { addBluePrint(IG_I, blueprintNmae, itemName, alternativeItemName); }
        void E(string s, string astr = "", string astn = "") { addBluePrint("Ore", s, astr, astn); }
        public class AssemblerBluePrint : IComparable<AssemblerBluePrint>
        {
            public long ItemPriority = 0;
            public List<Assembler> o = new List<Assembler>();
            public bool valid = true;
            public MyDefinitionId definition_id;
            public int NumBluePrintToAssembler = 0;
            public int CurrentItemAmount = 0;
            public int AssemblingDeltaAmount = 0;
            public int AssemblyAmount = 0;
            public int RefineryAmount = 0;
            public int MinimumAmount = 0;
            public int MaximumItemAmount = 1000;
            public string ModName = "";
            public string ItemName = "";
            public string AutoCraftingName = "";
            public string AutoCraftingType = "";
            public string BlueprintID = "";
            string type = "";
            public string subtype = "";
            string subtypename = "";
            static public void SetAutocraftingThresholdNew()
            {
                foreach (var bp in bprints.Values) bp.CalcMinimumAmount();
            }
            static string BluePrintNameToItemName(string t, string s)
            {
                var cs = "Component";
                if (s == "Magnetron_Component") return cs + " " + s; // mod item
                if (t == cs && s.EndsWith(cs)) return t + " " + s.Substring(0, s.Length - cs.Length); // components remove from bottom
                if (s.StartsWith("NATO_25")) // NATO Magazine
                {
                    cs = "Magazine";
                    return t + " " + s.Substring(0, s.Length - cs.Length);
                }
                if (t == IG_Tools) // add 'item' to all PhyicalGunItems
                {
                    if (s.Contains("Drill") || s.Contains("Grinder") || s.Contains("Welder") || s.Contains("Rifle")) return t + " " + s + "Item";
                }
                return t + " " + s;
            }
            const string AutomaticRifleGun_Mag_ = "AutomaticRifleGun_Mag_";
            string[] ToolsAndGunsTypes = { IG_Tools, IG_Datas, IG_HBottles, IG_OBottles, };
            void ConvertAutoCraftingName()
            {
                AutoCraftingType = type;
                if (ToolsAndGunsTypes.Contains(type)) AutoCraftingType = AC_ToolsAndGuns;
                else if (food_cast.Contains(ItemName)) AutoCraftingType = IG_Food;
                else if (subtype == Refinery.BluePrint_SpentFuelReprocessing) AutoCraftingType = Refinery.BluePrint_SpentFuelReprocessing;
                else if (subtype.Contains("Deuterium")) AutoCraftingType = "Deuterium";
                if (subtypename != "")
                {
                    AutoCraftingName = subtypename.Replace('_', ' ');
                    return;
                }
                AutoCraftingName = ItemName.Split(' ')[1];
                if (AutoCraftingName.StartsWith("Position")) AutoCraftingName = AutoCraftingName.Substring(AutoCraftingName.IndexOf('_') + 1);
                if (AutoCraftingName.StartsWith("K_HSR_")) AutoCraftingName = AutoCraftingName.Substring(6);
                else if (AutoCraftingName.Contains(AutomaticRifleGun_Mag_))
                {
                    if (AutoCraftingName.StartsWith(AutomaticRifleGun_Mag_)) AutoCraftingName = "AutoRifleGunMagazine";
                    else AutoCraftingName = AutoCraftingName.Substring(0, AutoCraftingName.IndexOf(AutomaticRifleGun_Mag_)) + "RifleGunMagazine";
                }
                else if (type == IG_Tools && ModName == M_Vanilla)
                {
                    string[] Tools = { "HandDrill", "Grinder", "Welder" };
                    var isTool = false;
                    foreach (var toolType in Tools)
                    {
                        if (AutoCraftingName.Contains(toolType))
                        {
                            var preString = "";
                            if (AutoCraftingName.Contains('4')) preString = "Elite";
                            else if (AutoCraftingName.Contains('3')) preString = "Professional";
                            else if (AutoCraftingName.Contains('2')) preString = "Ultimate";
                            AutoCraftingName = preString + toolType;
                            isTool = true;
                            break;
                        }
                    }
                    if (!isTool && AutoCraftingName.EndsWith("Item")) AutoCraftingName = AutoCraftingName.Substring(0, AutoCraftingName.Length - 4);
                }
                if (AutoCraftingName.EndsWith("Magazine")) AutoCraftingName = AutoCraftingName.Substring(0, AutoCraftingName.Length - 5);
                AutoCraftingName = AutoCraftingName.Replace('_', ' ');
            }
            public AssemblerBluePrint(string iTypeID, string iSubTypeID, string modName, string alter, string astype, MyDefinitionId definitionId)
            {
                subtypename = astype;
                type = iTypeID;
                subtype = iSubTypeID;
                BlueprintID = type + " " + subtype;
                definition_id = definitionId;
                ItemName = alter == "" ? BluePrintNameToItemName(type, subtype) : alter;
                ModName = modName;
                ConvertAutoCraftingName();
                SetMaximumAmount(0);
            }
            public int CompareTo(AssemblerBluePrint other)
            {
                if (other.ItemPriority < ItemPriority) return -1;
                else if (other.ItemPriority > ItemPriority) return 1;
                return 0;
            }
            public bool NeedsAssembling() { return (MinimumAmount > CurrentItemAmount + AssemblyAmount); }
            public bool IfMax() { return (MaximumItemAmount <= CurrentItemAmount); }
            public void SetMaximumAmount(int m) { MaximumItemAmount = m; CalcMinimumAmount(); }
            void CalcMinimumAmount() { MinimumAmount = (MaximumItemAmount * AutocraftingThreshold) / 100; }
            public void SetCurrentAmount(int amount)
            {
                NumBluePrintToAssembler = 0;
                CurrentItemAmount = amount;
                if (MaximumItemAmount > 0) AssemblingDeltaAmount = MaximumItemAmount - amount;
                else AssemblingDeltaAmount = -1;
            }
            public void CalcPriority()
            {
                if (AssemblingDeltaAmount <= 0) ItemPriority = 0;
                else ItemPriority = 100 - (CurrentItemAmount * 100 / MaximumItemAmount);
            }
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
    }
}
