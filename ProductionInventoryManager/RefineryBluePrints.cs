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
