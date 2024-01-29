using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using VRage.Game;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        // to be removed ---------------------------------------------------------------------------------
        void addPrio(string reftype, RefineryBlueprint type, int prio)
        {
            if (prio == 0) prio = 1;
            if (ingotprio.ContainsKey(reftype))
            {
                IPrio.GetBlueprintPrio(ingotprio[reftype], type, true).setPrio(prio);
            }
        }


        static Dictionary<string, List<IPrio>> ingotprio = new Dictionary<string, List<IPrio>>();
        //Ip
        public class IPrio : IComparable<IPrio>
        {
            public RefineryBlueprint refineryBP;
            public int prio = 0;
            public int initp = 0;
            public int CompareTo(IPrio other)
            {
                if (other.initp == initp) return 0;
                return other.initp > initp ? 1 : -1;
            }
            public IPrio(RefineryBlueprint bluePrint, int pr = 0)
            {
                refineryBP = bluePrint;
                prio = pr;
                initp = pr;
            }
            public void setPrio(int np)
            {
                if (np > 10000) np = 10000;
                prio = np;
                if (np == 0 || initp != np) initp = np;
            }
            public static IPrio GetBlueprintPrio(List<IPrio> ingotprioList, RefineryBlueprint bp, bool newip = false)
            {
                foreach (IPrio ip in ingotprioList)
                {
                    if (ip.refineryBP == bp) return ip;
                }
                if (newip)
                {
                    IPrio np = new IPrio(bp);
                    ingotprioList.Add(np);
                    return np;
                }
                else return null;
            }
        }
        // to be removed # end  ---------------------------------------------------------------------------------




        static List<RefineryBlueprint> RefineryBlueprints = new List<RefineryBlueprint>();
        static Dictionary<string, Dictionary<RefineryBlueprint, int>> OrePrioConfig = new Dictionary<string, Dictionary<RefineryBlueprint, int>>();

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
            RefineryBlueprints.Sort((x, y) => x.InputIDName.CompareTo(y.InputIDName));
        }

        public class RefineryBlueprint
        {
            public static void FillInputOutputAmountAndETA()
            {
                if ((DateTime.Now - FillTimestamp).TotalSeconds > 12)
                {
                    FillTimestamp = DateTime.Now;
                    foreach (var bluePrint in RefineryBlueprints)
                    {
                        var OldInputAmount = bluePrint.InputAmount;
                        bluePrint.InputAmount = inventar.GetValueOrDefault(bluePrint.InputID, 0);
                        bluePrint.OutputAmount = inventar.GetValueOrDefault(bluePrint.OutputID, 0);
                        var OldAmountSnapshot = bluePrint.AmountSnapshot;
                        bluePrint.AmountSnapshot = DateTime.Now;
                        var diff = (OldInputAmount - bluePrint.InputAmount);
                        bluePrint.ETA_String = diff < 0 ? "..." : GetTimeStringFromHours((bluePrint.InputAmount / diff) * (bluePrint.AmountSnapshot - OldAmountSnapshot).TotalHours);
                    }
                }
            }
            static DateTime FillTimestamp = DateTime.Now;
            static string[] ScrapTypeBlueprintNames = { "Component C100ShellCasing", Ore.Scrap, Ingot.Scrap };
            static MyItemType[] ScrapItemTypes;
            static Dictionary<MyItemType, RefineryBlueprint> KnowScrapTypes = new Dictionary<MyItemType, RefineryBlueprint>();
            public static RefineryBlueprint GetScrapBlueprintByItemtypeOrCreateNew(MyItemType scrapType)
            {
                if (!KnowScrapTypes.ContainsKey(scrapType))
                {
                    KnowScrapTypes.Add(scrapType, new RefineryBlueprint(scrapType));
                    RefineryBlueprints.Add(KnowScrapTypes[scrapType]);
                }
                return KnowScrapTypes[scrapType];
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
            public string OutputID = "";
            public string OutputIDName = "";
            public float InputAmount = 0;
            public float OutputAmount = 0;
            DateTime AmountSnapshot = DateTime.Now;
            public string ETA_String = "";
            public int RefineryCount = 0;
            public bool IsScrap = false;
            public int Prio = 0;
            public int InitPrio = 0;

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
            if (RefineryBlueprints.Find(b => b.Definition_id == id) == null)
            {
                RefineryBlueprints.Add(new RefineryBlueprint(id, inputItem, outputItem));
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
