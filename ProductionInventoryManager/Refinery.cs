using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
    {
        static List<Refinery> RefineryList = new List<Refinery>();

        public class Refinery
        {
            public enum RefreshType { Unknow, VanillaRefinery, WaterRecyclingSystem, HydroponicsFarm, Reprocessor, Incinerator, }
            public enum RefError { NotFilled, OutputNotEmpty, Damaged, IncineratorNoAutofill }

            static public string priobt = "";
            static public int cn = 0;
            static public Dictionary<string, List<RefineryBlueprint>> refineryTypesAcceptedBlueprintsList = new Dictionary<string, List<RefineryBlueprint>>();
            static public void RemoveUnusedRefinerytypeBlueprintLists()
            {
                foreach (var a in refineryTypesAcceptedBlueprintsList.Keys.ToArray())
                    if (refineryTypesAcceptedBlueprintsList.ContainsKey(a) && !priobt.Contains("@" + a))
                        refineryTypesAcceptedBlueprintsList.Remove(a);
            }
            IMyInventory InputInventory, OutputInventory;
            public Dictionary<string, float> InputInventoryItems = new Dictionary<string, float>();
            public TypeDefinitions typeid;
            public Parameter pms = new Parameter();
            public IMyRefinery RefineryBlock = null;
            public List<RefError> ErrorList = new List<RefError>();
            public List<RefineryBlueprint> AcceptedBlueprints = null;
            public string BlockSubType = "";
            public int fertig;
            RefineryBlueprint CurrentWorkBluePrint = null;
            RefineryBlueprint NextWorkBluePrint = null;
            float CurrentWorkOreAmount = 0;
            float NexWorkOreAmount = 0;

            public class TypeDefinitions
            {
                string TypeIDName;
                bool ComplettName;
                RefreshType TypeID;
                String AlternativName;
                public TypeDefinitions()
                {
                    TypeIDName = "";
                    ComplettName = true;
                    TypeID = RefreshType.Unknow;
                    AlternativName = RefreshType.Unknow.ToString();
                }
                public TypeDefinitions(string iTypeName, RefreshType iTypeID, string iAlternativName = "")
                {
                    TypeIDName = iTypeName;
                    ComplettName = true;
                    TypeID = iTypeID;
                    AlternativName = iAlternativName;
                }
                public TypeDefinitions(bool iComplettName, string iTypeName, RefreshType iTypeID, string iAlternativName = "")
                {
                    TypeIDName = iTypeName;
                    ComplettName = iComplettName;
                    TypeID = iTypeID;
                    AlternativName = iAlternativName;
                }
                public RefreshType GetTypeID() { return TypeID; }
                public string GetAlternativOrDefaultName() { return AlternativName == "" ? TypeIDName : AlternativName; }
                public bool IsVanillaManagment() { return TypeID == RefreshType.VanillaRefinery; }
                public bool IsUnknowType() { return TypeID == RefreshType.Unknow; }
                public bool CompareTypeName(string compareString)
                {
                    if (TypeID == RefreshType.Unknow)
                    {
                        AlternativName = compareString;
                        return true;
                    }
                    if (ComplettName && TypeIDName == compareString) return true;
                    else if (!ComplettName && TypeIDName.StartsWith(compareString)) return true;
                    return false;
                }
            }
            List<TypeDefinitions> TypeDefs = new List<TypeDefinitions>
            {
                new TypeDefinitions( false, "WRS", RefreshType.WaterRecyclingSystem, "Water Recycling System" ),
                new TypeDefinitions( "Blast Furnace", RefreshType.VanillaRefinery, "Basic Refinery"),
                new TypeDefinitions( "LargeRefineryIndustrial", RefreshType.VanillaRefinery, "Large Industrial Refinery"),
                new TypeDefinitions( "LargeRefinery", RefreshType.VanillaRefinery, "Large Refinery"),
                new TypeDefinitions( "K_HSR_Refinery_A", RefreshType.VanillaRefinery, "HSR Refinery A"),
                new TypeDefinitions( false, "Hydroponics", RefreshType.HydroponicsFarm, "Hydroponics Farm"),
                new TypeDefinitions( "RockCrusher", RefreshType.VanillaRefinery),
                new TypeDefinitions( "OrePurifier", RefreshType.VanillaRefinery),
                new TypeDefinitions( "ChemicalPlant", RefreshType.VanillaRefinery),
                new TypeDefinitions( "Centrifuge", RefreshType.VanillaRefinery),
                new TypeDefinitions( "Incinerator", RefreshType.Incinerator),
                new TypeDefinitions( "BitumenExtractor", RefreshType.VanillaRefinery),
                new TypeDefinitions( "Reprocessor", RefreshType.Reprocessor),
                new TypeDefinitions( "OilCracker", RefreshType.VanillaRefinery),
                new TypeDefinitions( "DeuteriumProcessor", RefreshType.VanillaRefinery, "Deuterium Refinery"),
                new TypeDefinitions(), // LastListItem    
            };
            public Refinery(IMyRefinery refinery)
            {
                RefineryBlock = refinery;
                InputInventory = refinery.GetInventory(0);
                OutputInventory = refinery.GetInventory(1);
                BlockSubType = refinery.BlockDefinition.SubtypeId;
                typeid = TypeDefs.Find(t => t.CompareTypeName(BlockSubType));
                BlockSubType = typeid.GetAlternativOrDefaultName();
                AcceptedBlueprints = RefineryBlueprints.FindAll(b => RefineryBlock.CanUseBlueprint(b.Definition_id));
                GetScrapBluePrints();
            }
            void GetScrapBluePrints()
            {
                var acceptedItems = new List<MyItemType>();
                InputInventory.GetAcceptedItems(acceptedItems, i => i.SubtypeId.ToLower().Contains("scrap") && !RefineryBlueprint.IsKnowScrapType(i));
                foreach (var inventoryItem in acceptedItems)
                {
                    var scrapBlueprint = RefineryBlueprint.GetScrapBlueprintByItemtypeOrCreateNew(inventoryItem);
                    if (!AcceptedBlueprints.Contains(scrapBlueprint)) AcceptedBlueprints.Add(scrapBlueprint);
                }
            }
            RefineryBlueprint GetRefineryBlueprintByItemType(MyItemType type)
            {
                return GetRefineryBlueprintByInputID(GetPIMItemID(type));
            }
            RefineryBlueprint GetRefineryBlueprintByInputID(string ore)
            {
                return AcceptedBlueprints.Find(b => b.InputID == ore);
            }
            public void RefineryManager()
            {
                if (pms.Control())
                {
                    switch (typeid.GetTypeID())
                    {
                        case RefreshType.WaterRecyclingSystem:
                            if (bprints.ContainsKey(Ingot.WaterFood) && !bprints[Ingot.WaterFood].IfMax()) WaterRecyclingSystemManager();
                            else if (always_recycle_greywater && inventar.ContainsKey(Ingot.GreyWater) && inventar[Ingot.GreyWater] > 0) WaterRecyclingSystemManager(true);
                            else ClearInputInventoryIfControledByPIM();
                            break;
                        case RefreshType.HydroponicsFarm:
                            if (bprints.ContainsKey(Ingot.SubFresh) && !bprints[Ingot.SubFresh].IfMax()) HydrophonicsManager();
                            else ClearInputInventoryIfControledByPIM();
                            break;
                        case RefreshType.Reprocessor:
                            if (bprints.ContainsKey(BluePrintID_SpentFuelReprocessing) && !bprints[BluePrintID_SpentFuelReprocessing].IfMax()) ReprocessorManager();
                            else ClearInputInventoryIfControledByPIM();
                            break;
                        case RefreshType.VanillaRefinery:
                            VanillaRefineryManager();
                            break;
                    }
                }
            }
            bool IfIngredientsNotFilled(string[] ingredients)
            {
                foreach (var s in ingredients) if (!(InputInventoryItems.ContainsKey(s) && InputInventoryItems[s] != 0)) return false;
                return true;
            }
            void LoadRecipeItems(string[] itemNames, float[] itemValues, float multipler)
            {
                for (int i = 0; i < itemNames.Length; i++)
                {
                    SendItemByType(itemNames[i], itemValues[i] * multipler, InputInventory);
                }
            }
            /*
            // ReprocessorIngots    #####################################################################################
	        SubTypeID: SpentFuelReprocessing	File: \Blueprints_POW.sbc
		        IN------>
        			Ingot SpentFuel:1
		        	Ore Ice:0.75
			        Ingot Sulfur:0.2
			        Ingot Niter:0.3
		        OUT------->
			        Ingot Uranium:0.25
			        Ingot DepletedUranium:0.5
			        Ingot NuclearWaste:0.25
            */
            public const string BluePrintID_SpentFuelReprocessing = "Ingot " + BluePrint_SpentFuelReprocessing, BluePrint_SpentFuelReprocessing = "SpentFuelReprocessing";
            static string[] ReprocessorIngredients = new string[] { Ingot.SpentFuel, Ore.Ice, Ingot.Sulfur, Ingot.Niter };
            static float[] ReprocessorRecipeValues = new float[] { 1f, 0.75f, 0.2f, 0.3f, };
            void ReprocessorManager()
            {
                if (IfIngredientsNotFilled(ReprocessorIngredients) && fertig < 90) return;
                ClearInventory(InputInventory);
                var m = ((InputInventory.MaxVolume.RawValue / 1000) / 56.9f) * 50.5f;
                LoadRecipeItems(ReprocessorIngredients, ReprocessorRecipeValues, m);
            }
            /*     Algae Recipe:
                   Volumen
                    1.0     <Item Amount="0.0075" TypeId="Ingot" SubtypeId="Nutrients" />
                    1.0     <Item Amount="0.005" TypeId="Ingot" SubtypeId="WaterFood" />
                    0.37    <Item Amount="0.12" TypeId="Ingot" SubtypeId="Stone" />
            */
            static string[] AlgaeIngredients = new string[] { Ingot.Stone, Ingot.WaterFood, Ingot.Nutrients };
            static float[] AlgaeRecipeValues = new float[] { 16.428f, 5f, 15f };
            public void HydrophonicsManager()
            {
                if (IfIngredientsNotFilled(AlgaeIngredients) && fertig < 90) return;
                ClearInventory(InputInventory);
                LoadRecipeItems(AlgaeIngredients, AlgaeRecipeValues, ((InputInventory.MaxVolume.RawValue / 1000) / 56.9f) * 0.5f);
            }
            //            const string Ingot_GreyWater = "Ingot GreyWater", Ingot_CleanWater = "Ingot CleanWater", Ice = "Ore Ice";
            public void WaterRecyclingSystemManager(bool grey = false)
            {
                if (fertig < 10) ClearInputInventoryIfControledByPIM();
                if (grey || (inventar.ContainsKey(Ingot.GreyWater) && inventar[Ingot.GreyWater] > 0))
                {
                    SendItemByType(Ingot.GreyWater, 1000, InputInventory, 0);
                    if (grey) return;
                }
                if (inventar.ContainsKey(Ingot.CleanWater) && inventar[Ingot.CleanWater] > 0)
                {
                    SendItemByType(Ingot.CleanWater, 1000, InputInventory, (!InputInventoryItems.ContainsKey(Ingot.CleanWater) || InputInventoryItems[Ingot.CleanWater] == 0 ? 0 : 1));
                }
                if (fertig > 70)
                {
                    SendItemByType(Ore.Ice, 1000, InputInventory);
                }
            }
            void AddRefineryCount()
            {
                List<MyInventoryItem> inhalt = new List<MyInventoryItem>();
                InputInventory.GetItems(inhalt);
                if (inhalt.Count > 0)
                {
                    var ore = GetPIMItemID(inhalt[0].Type);
                    var bluePrint = AcceptedBlueprints.Find(b => b.InputID == ore);
                    if (bluePrint != null) bluePrint.RefineryCount++;
                }
            }
            public void Refresh()
            {
                ClearInventoryList(InputInventoryItems);
                AddToInventory(InputInventory, InputInventoryItems);
                AddRefineryCount();
                AddToInventory(OutputInventory);
                if (!pms.ParseArgs(RefineryBlock.CustomName, true)) return;
                if (typeid.GetTypeID() == RefreshType.Incinerator)
                {
                    if (typeid.GetTypeID() == RefreshType.Incinerator) AddRefError(RefError.IncineratorNoAutofill);
                    ClearInventory(OutputInventory);
                    SetErrorByCondition(RefError.OutputNotEmpty, OutputInventory.CurrentVolume > 0);
                    SetErrorByCondition(RefError.Damaged, !RefineryBlock.IsFunctional);
                    return;
                }
                if (typeid.IsUnknowType())
                {
                    setWarning(Warning.ID.RefineryNotSupportet, pms.Name.ToString());
                    return;
                }
                if (typeid.IsVanillaManagment())
                {
                    if (!priobt.Contains("@" + BlockSubType)) priobt += "@" + BlockSubType;
                    if (!ingotprio.ContainsKey(BlockSubType)) ingotprio.Add(BlockSubType, new List<IPrio>());
                    if (!refineryTypesAcceptedBlueprintsList.ContainsKey(BlockSubType)) refineryTypesAcceptedBlueprintsList.Add(BlockSubType, AcceptedBlueprints);
                }
                GetWorkItems();
                SetErrorByCondition(RefError.Damaged, !RefineryBlock.IsFunctional);
                if (RefineryBlock.IsFunctional)
                {
                    cn++;
                    RefineryBlock.UseConveyorSystem = false;
                    if (InputInventory.ItemCount == 0)
                    {
                        RefineryBlock.Enabled = (refinerys_off && !pms.isPM("Nooff")) ? false : true;
                        DeleteRefError(RefError.NotFilled);
                    }
                    else
                    {
                        RefineryBlock.Enabled = true;
                        if (InputInventory.CurrentVolume > 0) DeleteRefError(RefError.NotFilled);
                    }
                    ClearInventory(OutputInventory);
                    SetErrorByCondition(RefError.OutputNotEmpty, OutputInventory.CurrentVolume > 0);
                    fertig = 100 - (int)((InputInventory.CurrentVolume.RawValue * 100) / InputInventory.MaxVolume.RawValue);
                }
            }
            static Dictionary<RefError, string> RefErrors = new Dictionary<RefError, string>
            {
                { RefError.NotFilled, " could not be filled\n"},
                { RefError.OutputNotEmpty, " cannot empty output.\n"},
                { RefError.Damaged, " is damaged.\n"},
                { RefError.IncineratorNoAutofill, " is not filled by PIM.\n"},
            };
            void AddRefError(RefError error) { if (!ErrorList.Contains(error)) ErrorList.Add(error); }
            void DeleteRefError(RefError error) { if (ErrorList.Contains(error)) ErrorList.Remove(error); }
            void SetErrorByCondition(RefError error, bool condition) { if (condition) AddRefError(error); else DeleteRefError(error); }
            public void GetRefineryErrorInfo(StringBuilder errString)
            {
                if (!pms.Control() && ErrorList.Count == 0) return;
                foreach (var error in ErrorList)
                {
                    errString.Append(pms.Name + RefErrors.GetValueOrDefault(error, ": unknown error\n"));
                }
            }
            public void FlushAllInventorys() { ClearInventory(InputInventory); ClearInventory(OutputInventory); }
            public void ClearInputInventoryIfControledByPIM() { if (pms.Control()) ClearInventory(InputInventory); }
            void CalculateRefineryAmount(string bluePrintName)
            {
                if (bprints.ContainsKey(bluePrintName))
                {
                    var b = bprints[bluePrintName];
                    var u = b.MaximumItemAmount - b.CurrentItemAmount;
                    if (b.MaximumItemAmount > 0 && u > 0) b.RefineryAmount = u;
                    else b.RefineryAmount = 0;
                }
            }
            void GetWorkItems()
            {
                string ws = "----";
                string nws = "----";
                float waf = 0f;
                float nwaf = 0f;
                switch (typeid.GetTypeID())
                {
                    case RefreshType.HydroponicsFarm:
                        CalculateRefineryAmount(Ingot.SubFresh);
                        break;
                    case RefreshType.WaterRecyclingSystem:
                        CalculateRefineryAmount(Ingot.WaterFood);
                        break;
                    case RefreshType.Reprocessor:
                        CalculateRefineryAmount(BluePrintID_SpentFuelReprocessing);
                        break;
                }
                var inhalt = new List<MyInventoryItem>();
                InputInventory.GetItems(inhalt);
                if (inhalt.Count() > 0)
                {
                    ws = GetPIMItemID(inhalt[0].Type);
                    waf = (float)inhalt[0].Amount;
                    if (inhalt.Count() > 1)
                    {
                        nws = GetPIMItemID(inhalt[1].Type);
                        nwaf = (float)inhalt[1].Amount;
                    }
                }
                CurrentWorkBluePrint = AcceptedBlueprints.Find(b => b.InputID == ws);
                CurrentWorkOreAmount = waf;
                NextWorkBluePrint = AcceptedBlueprints.Find(b => b.InputID == nws);
                NexWorkOreAmount = nwaf;
            }
            void OfenFuellen()
            {
                bool refineryFilled = false;
                RefineryBlueprint newworkBP = null;
                List<IPrio> ip = ingotprio[BlockSubType];
                for (int i = 0; i < ip.Count; i++)
                {
                    IPrio p = ip[i];
                    if (!Accept(p.refineryBP) || p.prio == 0 || !inventar.ContainsKey(p.refineryBP.InputID)) continue;
                    newworkBP = p.refineryBP;
                    if (fertig < 50) ClearInputInventoryIfControledByPIM();
                    var types = newworkBP.InputID.Split(' ');
                    if (inventar.ContainsKey(newworkBP.InputID)) refineryFilled = SendItemByTypeAndSubtype("MyObjectBuilder_" + types[0], types[1], inventar[newworkBP.InputID], RefineryBlock.GetInventory(0));
                    if (!refineryFilled) refineryFilled = Erzklau(newworkBP);
                    SetErrorByCondition(RefError.NotFilled, !refineryFilled && InputInventory.CurrentVolume == 0);
                }
                if (refineryFilled) SetIngotPrio(ip, newworkBP, cn);
            }
            bool Erzklau(RefineryBlueprint blueprint)
            {
                var oamount = 0f;
                if (blueprint == CurrentWorkBluePrint) oamount = CurrentWorkOreAmount;
                else if (blueprint == NextWorkBluePrint) oamount = NexWorkOreAmount;
                if (RefineryBlock.GetInventory(0).CurrentVolume.RawValue < 100)
                {
                    foreach (Refinery o in RefineryList)
                    {
                        int inum = 0;
                        var inventoryList = new List<MyInventoryItem>();
                        o.RefineryBlock.GetInventory(0).GetItems(inventoryList);
                        foreach (var inventoryItem in inventoryList)
                        {
                            var ostrID = o.GetRefineryBlueprintByItemType(inventoryItem.Type);
                            if (blueprint == ostrID && (float)inventoryItem.Amount > 1000 && (float)inventoryItem.Amount > oamount && Accept(ostrID))
                            {
                                float im = (float)inventoryItem.Amount / 2;
                                var xx = o.RefineryBlock.GetInventory(0).TransferItemTo(RefineryBlock.GetInventory(0), inum, null, true, MyFixedPoint.MultiplySafe(inventoryItem.Amount, 0.5f));
                                if (xx) return true;
                            }
                            inum++;
                        }
                    }
                }
                return false;
            }
            void NewOreSort()
            {
                var inventoryItems = new List<MyInventoryItem>();
                var inventory = RefineryBlock.GetInventory(0);
                inventory.GetItems(inventoryItems);
                var firstItemPrio = 10000;
                for (int i = 0; i < inventoryItems.Count; i++)
                {
                }
            }
            void OreSort()
            {
                var inventoryItems = new List<MyInventoryItem>();
                var inventory = RefineryBlock.GetInventory(0);
                inventory.GetItems(inventoryItems);
                if (inventoryItems.Count > 1)
                {
                    var scrap = -1;
                    for (int i = 0; i < inventoryItems.Count; i++)
                    {
                        var typeStr = inventoryItems[i].Type.SubtypeId.ToLower();
                        if (typeStr.Contains("scrap"))
                        {
                            scrap = i;
                            break;
                        }
                    }
                    if (scrap == -1)
                    {
                        int p1 = 0;
                        int p2 = 0;
                        var ostrID1 = GetPIMItemID(inventoryItems[0].Type);
                        var ostrID2 = GetPIMItemID(inventoryItems[1].Type);
                        foreach (IPrio p in ingotprio[BlockSubType])
                        {
                            if (ostrID1 == p.refineryBP.InputID) p1 = p.initp;
                            else if (ostrID2 == p.refineryBP.InputID) p2 = p.initp;
                        }
                        if (p1 < p2)
                        {
                            inventory.TransferItemTo(inventory, 0, 1, true, inventoryItems[0].Amount);
                        }
                        if (inventoryItems.Count == 3)
                        {
                            clearItemByType(inventory, GetPIMItemID(inventoryItems[2].Type), inventoryItems[2]);
                        }
                    }
                    else
                    {
                        inventory.TransferItemTo(inventory, scrap, 0, true, inventoryItems[scrap].Amount);
                    }
                }
            }
            public bool Accept(RefineryBlueprint ore)
            {
                if (RefineryBlock.GetInventory(0).IsFull) return false;
                return AcceptedBlueprints.Contains(ore);
            }
            enum RefineryManagerState { NONE, CLEAR_INPUT, SWAP_ORES, };
            RefineryManagerState RMS = RefineryManagerState.NONE;
            void VanillaRefineryManager()
            {
                if (!ingotprio.ContainsKey(BlockSubType)) return;
                RMS = RefineryManagerState.NONE;
                if (fertig > 80 || IfForceManagerExecuting()) OfenFuellen();
                OreSort();
            }
            public bool IfForceManagerExecuting()
            {
                if (!ingotprio.ContainsKey(BlockSubType)) return false;
                RMS = RefineryManagerState.NONE;
                int wp100 = 0;
                int op = 0;
                foreach (IPrio p in ingotprio[BlockSubType])
                {
                    if (p.refineryBP == CurrentWorkBluePrint || p.refineryBP == NextWorkBluePrint) op = op < p.prio ? (int)(p.prio * 1.5) : op;
                    else if (wp100 == 0 && Accept(p.refineryBP)) wp100 = p.prio;
                }
                return op < wp100 ? true : false;
            }
            static void SetIngotPrio(List<IPrio> ipl, RefineryBlueprint bp, int onum)
            {
                IPrio p = null;
                if (ipl.Count > 0 && null != (p = IPrio.GetBlueprintPrio(ipl, bp)))
                {
                    if (p.prio < 100)
                    {
                        p.prio -= p.prio / onum;
                        if (p.prio < 0) p.prio = 1;
                        ipl.Sort();
                    }
                }
            }
        }
    }
}
