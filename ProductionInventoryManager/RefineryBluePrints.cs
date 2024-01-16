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
        static List<RefineryBlueprint> refineryBlueprints = new List<RefineryBlueprint>();
        static Dictionary<string, Dictionary<RefineryBlueprint, int>> orePrioConfig = new Dictionary<string, Dictionary<RefineryBlueprint, int>>();

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

        public class RefineryBlueprint
        {
            public static void FillInputOutputAmountAndETA()
            {
                if ((DateTime.Now - fillTimestamp).TotalSeconds > 12)
                {
                    fillTimestamp = DateTime.Now;
                    foreach (var b in refineryBlueprints)
                    {
                        var OldInputAmount = b.InputAmount;
                        b.InputAmount = inventar.GetValueOrDefault(b.InputID, 0);
                        b.OutputAmount = inventar.GetValueOrDefault(b.OutputID, 0);
                        var OldAmountSnapshot = b.AmountSnapshot;
                        b.AmountSnapshot = DateTime.Now;
                        var diff = (OldInputAmount - b.InputAmount);
                        b.etaString = diff < 0 ? "..." : GetTimeStringFromHours((b.InputAmount / diff) * (b.AmountSnapshot - OldAmountSnapshot).TotalHours);
                    }
                }
            }
            static DateTime fillTimestamp = DateTime.Now;
            static string[] ScrapTypeBlueprintNames = { "Component C100ShellCasing", Ore.Scrap, Ingot.Scrap };
            static MyItemType[] ScrapItemTypes;
            static Dictionary<MyItemType, RefineryBlueprint> knowScrapTypes = new Dictionary<MyItemType, RefineryBlueprint>();
            public static RefineryBlueprint GetRefineryBlueprintByItemtypeOrCreateNew(MyItemType scrapType)
            {
                if (!knowScrapTypes.ContainsKey(scrapType))
                {
                    knowScrapTypes.Add(scrapType, new RefineryBlueprint(scrapType));
                    refineryBlueprints.Add(knowScrapTypes[scrapType]);
                }
                return knowScrapTypes[scrapType];
            }
            public static bool IsKnowScrapType(MyItemType scrapType)
            {
                return ScrapItemTypes.Contains(scrapType);
            }
            public static void InitScrapTypeBlueprintTypes()
            {
                ScrapItemTypes = new MyItemType[ScrapTypeBlueprintNames.Length];
                for (int i = 0; i < ScrapTypeBlueprintNames.Length; i++)
                {
                    var t = ScrapTypeBlueprintNames[i];
                    var parts = t.Split(' ');
                    var newType = MyItemType.Parse(IG_ + parts[0] + "/" + parts[1]);
                    ScrapItemTypes[i] = newType;
                }
            }
            // ----------------------------------------------------------------------------------------------------------------
            public MyDefinitionId Definition_id;
            public string Name = "";
            public string InputID = "";
            public string InputIDName = "";
            public float InputAmount = 0;
            public string OutputID = "";
            public string OutputIDName = "";
            public float OutputAmount = 0;
            public DateTime AmountSnapshot = DateTime.Now;
            public string etaString = "";
            public int RefineryCount = 0;
            public bool IsScrap = false;
            public RefineryBlueprint(MyDefinitionId iDefinitionID, string iInputID, string iOutputID)
            {
                Definition_id = iDefinitionID;
                Name = iDefinitionID.SubtypeName;
                InputID = iInputID;
                InputIDName = CastResourceName(InputID);
                OutputID = iOutputID;
                OutputIDName = CastResourceName(OutputID);
                IsScrap = ScrapTypeBlueprintNames.Contains(InputID);
            }
            public RefineryBlueprint(MyItemType iScrapType) // add AWWScrap Ore
            {
                InputID = GetPIMItemID(iScrapType);
                InputIDName = iScrapType.SubtypeId;
                Name = InputIDName + "ToIngots";
                IsScrap = true;
            }
        }
        static void AddRefineryBlueprint(string bpName, string inputItem, string outputItem)
        {
            MyDefinitionId id;
            if (!MyDefinitionId.TryParse("MyObjectBuilder_BlueprintDefinition/" + bpName, out id)) return;
            if (refineryBlueprints.Find(b => b.Definition_id == id) == null)
            {
                refineryBlueprints.Add(new RefineryBlueprint(id, inputItem, outputItem));
            }
        }
        static void AddRefineryBlueprintOreToIngot(string resource)
        {
            AddRefineryBlueprint(resource + "OreToIngot", "Ore " + resource, "Ingot " + resource);
        }
        static void AddRefineryBlueprintsArray(string[] resArray, string bpNamePrefix, string bpNameSuffix, string inputNamePrefix, string outputNamePrefix)
        {
            foreach (var bp in resArray)
            {
                AddRefineryBlueprint(bpNamePrefix + bp + bpNameSuffix, inputNamePrefix + bp, outputNamePrefix + bp);
            }
        }


        public class Resources
        {
            public const string
                RPowder = "powder",
                RMagnesium = "Magnesium",
                RStone = "Stone",
                RIron = "Iron",
                RNickel = "Nickel",
                RSilicon = "Silicon",
                RCobalt = "Cobalt",
                RPlatinum = "Platinum",
                RUranium = "Uranium",
                RScrap = "Scrap",
                RCarbon = "Carbon",
                RPotassium = "Potassium",
                RPhosphorus = "Phosphorus",
                RNaquadah = "Naquadah",
                RTrinium = "Trinium",
                RNeutronium = "Neutronium",
                RCopper = "Copper",
                RLithium = "Lithium",
                RBauxite = "Bauxite",
                RTitanium = "Titanium",
                RTantalum = "Tantalum",
                RSulfur = "Sulfur",
                RNiter = "Niter",
                RCoal = "Coal",
                RDeuterium = "Deuterium";
            // public const string R = "";
        }

        public class Ingot : Resources
        {
            const string prefix = "Ingot ";
            public const string
                Scrap = prefix + RScrap,
                Magnesium = prefix + RMagnesium,
                Magnesiumpowder = RMagnesium + RPowder,
                Gunpowder = "Gun" + RPowder,
                Stone = prefix + RStone,
                Iron = prefix + RIron,
                Nickel = prefix + RNickel,
                Silicon = prefix + RSilicon,
                Cobalt = prefix + RCobalt,
                Platinum = prefix + RPlatinum,
                Uranium = prefix + RUranium,
                WaterFood = prefix + "WaterFood",
                Nutrients = prefix + "Nutrients",
                SubFresh = prefix + "SubFresh",
                GreyWater = prefix + "GreyWater",
                CleanWater = prefix + "CleanWater",
                SpentFuel = prefix + "SpentFuel",
                Niter = prefix + RNiter,
                DeuteriumContainer = prefix + RDeuterium + "Container",
                Carbon = prefix + RCarbon,
                Potassium = prefix + RPotassium,
                Phosphorus = prefix + RPhosphorus,
                Naquadah = prefix + RNaquadah,
                Trinium = prefix + RTrinium,
                Neutronium = prefix + RNeutronium,
                Copper = prefix + RCopper,
                Lithium = prefix + RLithium,
                Titanium = prefix + RTitanium,
                Tantalum = prefix + RTantalum,
                Sulfur = prefix + RSulfur,
                Aluminium = prefix + "Aluminium";
            // public const string  = prefix + "";
        }

        public class Ore : Resources
        {
            const string prefix = "Ore ";
            public const string
                Scrap = prefix + RScrap,
                Magnesium = prefix + RMagnesium,
                Stone = prefix + RStone,
                Iron = prefix + RIron,
                Nickel = prefix + RNickel,
                Silicon = prefix + RSilicon,
                Cobalt = prefix + RCobalt,
                Platinum = prefix + RPlatinum,
                Uranium = prefix + RUranium,
                Organic = prefix + "Organic",
                Ice = prefix + "Ice",
                Deuterium = prefix + RDeuterium,
                Carbon = prefix + RCarbon,
                Potassium = prefix + RPotassium,
                Phosphorus = prefix + RPhosphorus,
                Naquadah = prefix + RNaquadah,
                Trinium = prefix + RTrinium,
                Neutronium = prefix + RNeutronium,
                Niter = prefix + RNiter,
                Copper = prefix + RCopper,
                Lithium = prefix + RLithium,
                Bauxite = prefix + RBauxite,
                Titanium = prefix + RTitanium,
                Tantalum = prefix + RTantalum,
                Sulfur = prefix + RSulfur,
                Coal = prefix + RCoal;
            // public const string  = prefix + "";
        }
    }
}
