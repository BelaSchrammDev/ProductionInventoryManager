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
using static IngameScript.Program;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        bool ShowInfoPBLcd = true;
        static bool delete_queueItem_if_max = true;
        static bool always_recycle_greywater = true;
        static bool assemblers_off = true;
        static bool refinerys_off = true;
        bool collect_all_Ore = true;
        bool collect_all_Ingot = true;
        bool collect_all_Component = true;
        int stacking_cycle = 10;
        StopWatch MainLoopTimeSpan = new StopWatch(3);
        static int AutocraftingThreshold = 80;
        static string debugString = "";
        const int minIC = 300, maxIC = 5000;
        bool firstRun = true;
        static Dictionary<string, float> inventar = new Dictionary<string, float>();
        static bool changeAutoCraftingSettings = true;
        static List<storageInventory> storageinvs = new List<storageInventory>();
        List<IMyRefinery> raff = new List<IMyRefinery>();
        List<IMyAssembler> ass = new List<IMyAssembler>();
        List<Assembler> okumas = new List<Assembler>();
        List<IMyUserControllableGun> ugun = new List<IMyUserControllableGun>();
        List<IMyTerminalBlock> tbl = new List<IMyTerminalBlock>();
        List<string> collectAll_List = new List<string>();
        string gungroupName = "PIM controlled Guns";
        static List<Gun> guns = new List<Gun>();
        static List<StorageCargo> storageCargos = new List<StorageCargo>();
        Dictionary<string, CargoUse> CargoUseList = new Dictionary<string, CargoUse>();
        static Dictionary<string, bool> usedMods = new Dictionary<string, bool>();
        List<string> mods = new List<string>(); string curmod = M_Vanilla;
        static Dictionary<string, AssemblerBluePrint> bprints = new Dictionary<string, AssemblerBluePrint>();
        static Dictionary<string, AssemblerBluePrint> bprints_pool = new Dictionary<string, AssemblerBluePrint>();
        double currentCycleInSec = 0f; int m0 = 1; int m1, m2 = 0; List<string> s0; static IMyGridProgramRuntimeInfo rti; IMyProgrammableBlock master;
        const string SI1 = "PIM v1.1", SI2 = "a beta (c) BelaOkuma\n", SMS = "SMS v1.4", X_StorageTag = "(sms,storage)";
        const string X_Config = "### Config ###", X_Config_end = "### Config End ###", X_Line = "  / =================================\n", X_UseConveyor = "UseConveyor";
        const string X_Autocrafting_treshold = "Autocrafting_threshold";
        const string M_Vanilla = "Vanilla", M_HSR = "HSR_Mod", M_NorthWindWeapons = "NorthWindWeaponsMod", M_AryxEpsteinDrive = "AryxEpsteinDriveMod", M_PlantCook = "PlantAndCookMod", M_EatDrinkSleep = "EatDrinkSleepRepeatMod", M_IndustrialOverhaulLLMod = "IndustrialOverhaulLockLoadMod", M_IndustrialOverhaulWaterMod = "IndustrialOverhaulWaterMod", M_IndustrialOverhaulMod = "IndustrialOverhaulMod", M_DailyNeedsSurvival = "DailyNeedsSurvivalMod", M_AzimuthThruster = "AzimuthThrusterMod", M_SG_Gates = "StarGateMod_Gates", M_SG_Ores = "StarGateMod_Ores", M_PaintGun = "PaintGunMod", M_DeuteriumReactor = "DeuteriumReactorMod", M_Shield = "DefenseShieldMod", M_RailGun = "MCRN_RailGunMod", M_HomingWeaponry = "MWI_HomingWeaponryMod";
        const string AC_ToolsAndGuns = "Tools&Guns", IG_Food = "Food", IG_Component = "Component", IG_I = "Ingot", IG_Ingot = IG_I + " ", IG_Com = IG_Component + " ", IG_Datas = "Datapad", IG_Kits = "ConsumableItem", IG_Cash = "PhysicalObject", IG_Tools = "PhysicalGunObject", IG_HBottles = "GasContainerObject", IG_OBottles = "OxygenContainerObject", IG_Ammo = "AmmoMagazine", IG_ = "MyObjectBuilder_";
        static Dictionary<string, AmmoDefs> ammoDefs = new Dictionary<string, AmmoDefs>();
        static Dictionary<string, DisplayBox> DisplayBoxList = new Dictionary<string, DisplayBox>();
        static DisplayBox getDisplayBox(string boxID, float width)
        {
            if (!DisplayBoxList.ContainsKey(boxID))
            {
                var newDisplayBox = new DisplayBox(width);
                DisplayBoxList.Add(boxID, newDisplayBox);
            }
            return DisplayBoxList[boxID];
        }
        static string getDisplayBoxString(string text, float amount, float width)
        {
            var amountStr = amount == 0 ? "  " : DisplayBox.GetMassString(amount);
            return getDisplayBox("@@@" + text, width).Get2StringWithSpaces(text, amountStr);
        }
        static string getDisplayBoxString(int amount, float width, bool left = false)
        {
            var displaytext = amount == 0 ? "  " : amount.ToString();
            return getDisplayBox(displaytext + width.ToString(), width).GetStringWithSpaces(displaytext, left);
        }
        static string getDisplayBoxString(float amount, float width, bool left = false)
        {
            var amountStr = amount == 0 ? "  " : DisplayBox.GetMassString(amount);
            return getDisplayBox(amountStr + width.ToString(), width).GetStringWithSpaces(amountStr, left);
        }
        static string getDisplayBoxStringDisplayNull(int amount, float width, bool left = false)
        {
            var displaytext = amount.ToString();
            return getDisplayBox("###" + displaytext + width.ToString() + left.ToString(), width).GetStringWithSpaces(displaytext, left);
        }
        static string getDisplayBoxString(string displaytext, float width, bool left = false)
        {
            return getDisplayBox(displaytext + width.ToString() + left.ToString(), width).GetStringWithSpaces(displaytext, left);
        }
        class DisplayBox
        {
            const char HSS = '\u00AD';
            static List<SP> SpacePoolList = new List<SP>();
            static Dictionary<char, float> CharWidthList = new Dictionary<char, float>();
            class SP
            {
                static int SpacePoolLiveCycle = 700;
                int LiveCycle = 5;
                public float Width = 0;
                string FillString = "";
                public SP(float iw)
                {
                    Width = iw;
                    if (Width < 0.6f) return;
                    int spnum = (int)(Width / 1.29166f);
                    int hsnum = (int)((Width - (spnum * 1.29166f)) / 0.287035f);
                    if (hsnum >= spnum)
                    {
                        spnum += 1;
                        hsnum = 0;
                    }
                    else spnum -= hsnum;
                    FillString += new String(' ', spnum);
                    FillString += new String(HSS, hsnum);
                }
                public string GetFillString()
                {
                    LiveCycle = 5;
                    RefreshSpacePoolList();
                    return FillString;
                }
                static void RefreshSpacePoolList()
                {
                    if (--SpacePoolLiveCycle < 1)
                    {
                        for (int i = SpacePoolList.Count - 1; i > 0; i--)
                        {
                            SP osp = SpacePoolList[i];
                            if (--osp.LiveCycle < 0) SpacePoolList.Remove(osp);
                        }
                        SpacePoolLiveCycle = 500;
                    }
                }
            }
            string LeftText = "", RightText = "", BoxText = "", SpaceString = "";
            float Width = 0;
            public DisplayBox(float iwidth)
            {
                Width = iwidth;
            }
            public string Get2StringWithSpaces(string arg1, string arg2)
            {
                if (arg1 != LeftText || arg2 != RightText)
                {
                    var strLength2 = GetStringWidth(arg2);
                    arg1 = TrimStringByWidth(arg1, Width - 2.6f - strLength2);
                    SpaceString = GetSpaceStringByWidth(Width - GetStringWidth(arg1) - strLength2);
                    LeftText = arg1;
                    RightText = arg2;
                }
                BoxText = LeftText + SpaceString + RightText;
                return BoxText;
            }
            public string GetStringWithSpaces(string arg, bool leftAlignment = false)
            {
                if (arg != LeftText)
                {
                    arg = TrimStringByWidth(arg, Width - 2.6f);
                    SpaceString = GetSpaceStringByWidth(Width - GetStringWidth(arg));
                    LeftText = arg;
                }
                if (leftAlignment) BoxText = LeftText + SpaceString;
                else BoxText = SpaceString + LeftText;
                return BoxText;
            }
            string TrimStringByWidth(string arg, float cutLength)
            {
                var strLength = GetStringWidth(arg);
                if (strLength > cutLength)
                {
                    var charDiff = (int)((strLength - cutLength - 5f) / 1.5f);
                    if (charDiff > 0 && charDiff < arg.Length - 1)
                    {
                        var lastString = arg.Substring(charDiff);
                        return arg[0] + "..." + lastString;
                    }
                }
                return arg;
            }
            static SP fspm(float with) { foreach (SP osp in SpacePoolList) { if (osp.Width == with) return osp; } SP nsp = new SP(with); SpacePoolList.Add(nsp); return nsp; }
            static string GetSpaceStringByWidth(float with) { return fspm(with).GetFillString(); }
            static void InitCharWidthList() { SetCharWidth("\n", 0f); SetCharWidth("'|ÎÏ", 1f); SetCharWidth(" !`Iiîïjl", 1.29166f); SetCharWidth("(),.:;[]{}1ft", 1.43076f); SetCharWidth("\"-r", 1.57627f); SetCharWidth("*", 1.72222f); SetCharWidth("\\", 1.86f); SetCharWidth("/", 2.16279f); SetCharWidth("«»Lvx_ƒ", 2.325f); SetCharWidth("?7Jcçz", 2.44736f); SetCharWidth("3FKTaäàâbdeèéêëghknoöôpqsuüùûßyÿ", 2.58333f); SetCharWidth("+<>=^~EÈÉÊË", 2.73529f); SetCharWidth("#0245689CÇXZ", 2.90625f); SetCharWidth("$&GHPUÜÙÛVYŸ", 3f); SetCharWidth("AÄÀÂBDNOÖÔQRS", 3.20689f); SetCharWidth("%", 3.57692f); SetCharWidth("@", 3.72f); SetCharWidth("M", 3.875f); SetCharWidth("æœmw", 4.04347f); SetCharWidth("WÆŒ", 4.65f); CharWidthList.Add(HSS, 1.578695f); }
            static void SetCharWidth(string s, float z) { foreach (var c in s) CharWidthList.Add(c, z); }
            static float GetStringWidth(string strData)
            {
                if (CharWidthList.Count == 0) InitCharWidthList();
                float fltTotal = 0;
                foreach (var c in strData) fltTotal += CharWidthList.ContainsKey(c) ? CharWidthList[c] : 2f;
                return fltTotal;
            }
            public static string GetMassString(double d) { return GetStringFromDoubleWithSuffixMask(d, pM); }
            public static string GetIntString(double d) { return GetStringFromDoubleWithSuffixMask(d, pD); }
            static string[]
                pD = new string[] { " 0.# m ", " 0.#   ", " 0.# k", " 0.# M" },
                pM = new string[] { " 0.0 g  ", " 0.0 kg", " 0.0 T  ", " 0.0 kT" };
            static string GetStringFromDoubleWithSuffixMask(double a, string[] p) { if (a > 900000.0f) return (a / 1000000).ToString(p[3]); else if (a > 900.0f) return (a / 1000).ToString(p[2]); else if (a < 1.0f) (a * 1000).ToString(p[0]); return a.ToString(p[1]); }
        }
        class AmmoDefs : IComparable<AmmoDefs>
        {
            static string CurrentSortGuntype = "";
            static public void SetCurrentSortGuntype(string type) { CurrentSortGuntype = type; }
            string Name = "";
            string PrioDefName = "";
            public string type = "";
            Dictionary<string, int> gunAmmoPrio = new Dictionary<string, int>();
            AssemblerBluePrint ammoBluePrint;
            public float ratio = 1, maxOfVolume = 0;
            public List<Gun> guns = new List<Gun>();
            public int CompareTo(AmmoDefs other)
            {
                var prio = GetAmmoPriority(CurrentSortGuntype);
                var otherprio = other.GetAmmoPriority(CurrentSortGuntype);
                if (prio == otherprio) return 0;
                if (otherprio > prio) return 1;
                return -1;
            }
            public AmmoDefs(string iname)
            {
                Name = iname;
                ammoBluePrint = GetBluePrintByItemName(iname);
                type = Name.Substring(Name.IndexOf(' ') + 1);
                if (ammoBluePrint == null) PrioDefName = type;
                else PrioDefName = ammoBluePrint.AutoCraftingName;
            }
            public void SetAmmoPriority(string iType, int iPrio)
            {
                if (iPrio < 0) iPrio = 0;
                else if (iPrio > 10) iPrio = 10;
                if (!gunAmmoPrio.ContainsKey(iType)) gunAmmoPrio.Add(iType, iPrio);
                else gunAmmoPrio[iType] = iPrio;
            }
            public int GetAmmoPriority(string gunType)
            {
                if (gunAmmoPrio.ContainsKey(gunType)) return gunAmmoPrio[gunType];
                else return 0;
            }
            public string GetAmmoBluePrintAutocraftingName()
            {
                return PrioDefName;
            }
            public void CalcAmmoInventoryRatio()
            {
                if (inventar.ContainsKey(Name))
                {
                    ratio = inventar[Name] / maxOfVolume;
                    if (ratio > 1) ratio = 1;
                }
                else ratio = 1;
            }
        }
        class MultiAmmoGuns
        {
            public string DisplayName = "";
            public string MultiAmmoGuntype = "";
            List<Gun> MultiAmmoGunList = new List<Gun>();
            public List<AmmoDefs> ammoDefs = new List<AmmoDefs>();
            public MultiAmmoGuns(string type, string dName)
            {
                MultiAmmoGuntype = type;
                DisplayName = dName;
            }
            public void addAmmoDef(AmmoDefs aDef)
            {
                if (!ammoDefs.Contains(aDef))
                {
                    ammoDefs.Add(aDef);
                    aDef.SetAmmoPriority(MultiAmmoGuntype, ammoDefs.Count);
                }
            }
            public void addGun(Gun mGun)
            {
                if (!MultiAmmoGunList.Contains(mGun)) MultiAmmoGunList.Add(mGun);
            }
            public void removeGun(Gun mGun)
            {
                if (MultiAmmoGunList.Contains(mGun)) MultiAmmoGunList.Remove(mGun);
            }
            public bool if_GunListEmpty()
            {
                return MultiAmmoGunList.Count == 0;
            }
            public AmmoDefs GetAmmoDefs(string _type)
            {
                return ammoDefs.Find(a => a.type == _type);
            }
        }
        static Dictionary<string, MultiAmmoGuns> multiAmmoGuns = new Dictionary<string, MultiAmmoGuns>();
        static MultiAmmoGuns getNewMultiAmmoGun(Gun mGun)
        {
            if (!multiAmmoGuns.ContainsKey(mGun.gunType))
            {
                multiAmmoGuns.Add(mGun.gunType, new MultiAmmoGuns(mGun.gunType, mGun.gun.DefinitionDisplayNameText));
                multiAmmoGuns[mGun.gunType].addGun(mGun);
                return multiAmmoGuns[mGun.gunType];
            }
            multiAmmoGuns[mGun.gunType].addGun(mGun);
            return null;
        }
        static void clearMultiAmmoGunsList() // ToDo: wird das noch gebraucht oder kann das weg???
        {
            var keyList = multiAmmoGuns.Keys.ToArray();
            for (int i = keyList.Length - 1; i >= 0; i--)
            {
                if (multiAmmoGuns[keyList[i]].if_GunListEmpty()) multiAmmoGuns.Remove(keyList[i]);
            }
        }
        static AmmoDefs getAmmoDefs(string name) { if (!ammoDefs.ContainsKey(name)) ammoDefs.Add(name, new AmmoDefs(name)); return ammoDefs[name]; }
        abstract class storageInventory
        {
            public IMyInventory inv = null;
            public Dictionary<string, float> items = new Dictionary<string, float>();
            abstract public bool checkItems();
            public void reloadItems()
            {
                if (!checkItems()) return;
                var invList = new List<MyInventoryItem>();
                var succlist = new List<string>();
                var einleiten = new Dictionary<string, float>();
                inv.GetItems(invList);
                for (int i = invList.Count - 1; i >= 0; i--)
                {
                    var iList = new List<MyInventoryItem>();
                    inv.GetItems(iList, v => v.Type == invList[i].Type);
                    if (iList.Count > 1)
                    {
                        inv.TransferItemTo(inv, iList[iList.Count - 1]);
                    }
                }
                invList.Clear();
                inv.GetItems(invList);
                for (int i = invList.Count - 1; i >= 0; i--)
                {
                    var iItem = invList[i];
                    var iType = GetPIMItemID(iItem.Type);
                    if (!items.ContainsKey(iType)) clearItemByType(inv, iType, iItem);
                    else
                    {
                        var adiff = items[iType] - (float)iItem.Amount;
                        if (adiff < 0)
                        {
                            clearItemByType(inv, iType, iItem, Math.Abs(adiff));
                            succlist.Add(iType);
                        }
                        else if (adiff == 0) succlist.Add(iType);
                        else einleiten.Add(iType, adiff);
                    }
                }
                foreach (var i in items.Keys)
                {
                    if (succlist.Contains(i)) continue;
                    SendItemByType(i, (einleiten.ContainsKey(i) ? einleiten[i] : items[i]), inv);
                }
            }
        }
        static AssemblerBluePrint AddProductionAmount(MyProductionItem pi)
        {
            var bprint = GetBluePrintByProductionItem(pi);
            if (bprint != null) bprint.AssemblyAmount += pi.Amount.ToIntSafe();
            return bprint;
        }
        StackItem GetStackItem(MyItemType t) { foreach (var s in StackItemList) if (s.type == t) return s; var nt = new StackItem(t); StackItemList.Add(nt); return nt; }
        static string GetPIMItemID(MyItemType type) { return type.TypeId.Substring(type.TypeId.IndexOf('_') + 1) + " " + type.SubtypeId; }
        static AssemblerBluePrint GetBluePrintByItemName(string itemName)
        {
            foreach (var b in bprints.Values) if (b.ItemName == itemName) return b;
            foreach (var b in bprints_pool.Values) if (b.ItemName == itemName) return b;
            return null;
        }
        static AssemblerBluePrint GetBluePrintByProductionItem(MyProductionItem pi)
        {
            foreach (var b in bprints.Values) if (b.definition_id.SubtypeName == pi.BlueprintId.SubtypeName) return b;
            foreach (var b in bprints_pool.Values) if (b.definition_id.SubtypeName == pi.BlueprintId.SubtypeName) return b;
            return null;
        }
        class CargoUse
        {
            public string type = "";
            public double Current = 0, Maximum = 0;
            public CargoUse(string s) { type = s; }
            public void AddCurrentAndMaxCargocapacity(double c, double m) { Current += c; Maximum += m; }
            public int GetCarcocapacityUseRatio() { return (int)(Current * 100 / Maximum); }
        }
        class StackItem : IComparable<StackItem>
        {
            public enum StackingType { Stack, Volume, VolumeBack, }
            static public StackingType CurrentStackingType = StackingType.Stack;
            public enum StackingSort { Stack, Amount, AmountBack, Delta, ItemsBack, VolumeFree, VolumeFreeBack, }
            static public StackingSort CurrentStackingSorttype = StackingSort.Stack;
            static IMyInventory big = null;
            static IMyInventory free = null;
            static public void ClearStackInventory() { big = null; free = null; }
            static public void CalculateFreeInventory(IMyInventory inv) { if (big == null || (big.MaxVolume < inv.MaxVolume)) big = inv; if (free == null || (free.MaxVolume - free.CurrentVolume < inv.MaxVolume - inv.CurrentVolume)) free = inv; }
            class Stack : IComparable<Stack>
            {
                public int items = 0;
                public float amount = 0;
                public float volume_free = 0;
                public IMyInventory inv = null;
                public Stack(IMyInventory i, MyFixedPoint a) { amount = (float)a; inv = i; refresh(); }
                public void refresh() { items = inv.ItemCount; volume_free = (float)(inv.MaxVolume - inv.CurrentVolume); }
                public float GetMaxVolume() { return (float)inv.MaxVolume; }
                public int CompareTo(Stack other)
                {
                    if (CurrentStackingSorttype == StackingSort.Amount) { if (other.amount == amount) return 0; return other.amount > amount ? 1 : -1; }
                    else if (CurrentStackingSorttype == StackingSort.AmountBack) { if (other.amount == amount) return 0; return other.amount < amount ? 1 : -1; }
                    else if (CurrentStackingSorttype == StackingSort.Delta) { if (items == 1 && other.items == 1) return 0; else if (items == 1) return 1; if (other.amount == amount) return 0; return other.amount < amount ? 1 : -1; }
                    else if (CurrentStackingSorttype == StackingSort.ItemsBack) { if (other.items == items) return 0; return other.items < items ? 1 : -1; }
                    else if (CurrentStackingSorttype == StackingSort.VolumeFree) { if (other.volume_free == volume_free) return 0; return other.volume_free > volume_free ? 1 : -1; }
                    else if (CurrentStackingSorttype == StackingSort.VolumeFreeBack) { if (other.volume_free == volume_free) return 0; return other.volume_free < volume_free ? 1 : -1; }
                    return 0;
                }
            }

            public MyItemType type;
            float typevolume = 1;
            int stacks = 0;
            public int Stackcount { get { return stacks; } }
            float amount = 0;
            float volume = 0;
            List<Stack> invs = new List<Stack>();
            IMyInventory quelle = null, ziel = null;
            public StackItem(MyItemType itype) { type = itype; typevolume = type.GetItemInfo().Volume; }
            void refreshInvs() { foreach (var i in invs) i.refresh(); }
            public void AddStack(IMyInventory inv, MyFixedPoint am) { stacks++; amount += (float)am; volume = amount * typevolume; invs.Add(new Stack(inv, am)); }
            public int CompareTo(StackItem other)
            {
                if (CurrentStackingType == StackingType.Stack) { if (other.stacks == stacks) { if (other.amount == amount) return 0; return other.amount > amount ? 1 : -1; } return other.stacks > stacks ? 1 : -1; }
                else if (CurrentStackingType == StackingType.Volume) { if (other.volume == volume) return 0; return other.volume > volume ? 1 : -1; }
                else if (CurrentStackingType == StackingType.VolumeBack) { if (other.volume == volume) return 0; return other.volume < volume ? 1 : -1; }
                return 0;
            }
            public bool check_stacking_gamma()
            {
                if (stacks == 2)
                {
                    if (invs[0].GetMaxVolume() > volume)
                    {
                        quelle = invs[1].inv;
                        ziel = invs[0].inv;
                        return true;
                    }
                    else if (invs[1].GetMaxVolume() > volume)
                    {
                        quelle = invs[0].inv;
                        ziel = invs[1].inv;
                        return true;
                    }
                }
                return false;
            }
            public bool stacking_gamma()
            {
                var von = new List<MyInventoryItem>();
                ziel.GetItems(von);
                bool zielleer = true;
                for (int x = von.Count - 1; x >= 0; x--)
                {
                    var i = von[x];
                    if (i.Type != type)
                    {
                        if (!quelle.TransferItemFrom(ziel, i, null)) zielleer = false;
                    }
                }
                var item = quelle.FindItem(type);
                if (item != null) quelle.TransferItemTo(ziel, (MyInventoryItem)item, null);
                return (zielleer && item == null);
            }
            public bool stacking_beta()
            {
                if (stacks < 2) return true;
                if (((float)(free.MaxVolume - free.CurrentVolume)) < volume) return false;
                foreach (var i in invs)
                {
                    if (i.inv != free)
                    {
                        var item = i.inv.FindItem(type);
                        if (item != null)
                        {
                            free.TransferItemFrom(i.inv, (MyInventoryItem)item, null);
                        }
                    }
                }
                return true;
            }
            public void stacking_delta()
            {
                if (stacks > 1)
                {
                    refreshInvs();
                    CurrentStackingSorttype = StackingSort.Delta;
                    invs.Sort();
                    int x = 0;
                    int y = invs.Count - 1;
                    for (; x < y; y--)
                    {
                        var item = invs[y].inv.FindItem(type);
                        if (item != null)
                        {
                            if (invs[x].inv.TransferItemFrom(invs[y].inv, (MyInventoryItem)item, null)) x++;
                        }
                    }
                }
            }
            public void stacking_alpha()
            {
                if (stacks < 2) return;
                refreshInvs();
                CurrentStackingSorttype = StackingSort.VolumeFree;
                invs.Sort();
                if (invs[0].volume_free < volume)
                {
                    var ziel = invs[0].inv;
                    CurrentStackingSorttype = StackingSort.AmountBack;
                    invs.Sort();
                    foreach (var st in invs)
                    {
                        if (st.inv != ziel)
                        {
                            var item = st.inv.FindItem(type);
                            if (item != null)
                            {
                                ziel.TransferItemFrom(st.inv, (MyInventoryItem)item, null);
                            }
                        }
                    }
                }
                else
                {
                    for (int x = 1; x < invs.Count - 1; x++)
                    {
                        var i = invs[x].inv;
                        var item = i.FindItem(type);
                        if (item != null)
                        {
                            invs[0].inv.TransferItemFrom(i, (MyInventoryItem)item, null);
                        }
                    }
                }
            }
            public void stacking_single()
            {
                if (stacks < 2) return;
                for (int x = invs.Count - 1; x > 0; x--)
                {
                    var i = invs[x].inv;
                    for (int y = x - 1; y >= 0; y--)
                    {
                        if (i == invs[y].inv)
                        {
                            var vv = new List<MyInventoryItem>();
                            i.GetItems(vv, ooo => ooo.Type == type);
                            if (vv.Count > 1)
                            {
                                i.TransferItemFrom(i, vv[vv.Count - 1], vv[vv.Count - 1].Amount);
                            }
                        }
                    }
                }
            }
        }
        DateTime StackingCounter = DateTime.Now;
        static List<StackItem> StackItemList = new List<StackItem>();
        int stack_mode = 0;
        string stack_type = "";
        int cur_stack_type = 0;
        static string[] stack_types = new string[] { "Component", "Ore", "Ingot" };
        bool if_true(string str) { return Convert.ToBoolean(str); }
        void writeConfig()
        {
            var configstr = "  / attention!!!\n  / autocraftingconfig now via LCD Display,\n  / place a LCD and add '..(sms,autocrafting) to the name.\n  / follow the instructions, multiple lcds are possible'\n\n" + X_Config + "\n\n" + X_Line + "  / options set to 'True' or 'False'.\n  / to activate changes, please restart script\n"
            + X_Line + "\n  / show info on programmable blocks LCD\n"
            + "ShowInfoPBLcd=" + ShowInfoPBLcd.ToString() + "\n\n"
            + "  / delete item from the production list (Assemblers)\n  / when the maximum value is reached.\n"
            + "delete_queueItem_if_max=" + delete_queueItem_if_max.ToString() + "\n\n"
            + "  / always recycle grey water on the Water Recycling System Block\n  / (only Daily Needs Survival Mod)\n"
            + "always_recycle_greywater=" + always_recycle_greywater.ToString() + "\n\n"
            + "  / turn all assemblers off when production queue is empty\n"
            + "assemblers_off=" + assemblers_off.ToString() + "\n\n"
            + "  / turn all refinerys off when inbound inventory is empty\n"
            + "refinerys_off=" + refinerys_off.ToString() + "\n\n"
            + "  / collect all ore\n"
            + "collect_all_Ore=" + collect_all_Ore.ToString() + "\n\n"
            + "  / collect all ingot\n"
            + "collect_all_Ingot=" + collect_all_Ingot.ToString() + "\n\n"
            + "  / collect all component\n"
            + "collect_all_Component=" + collect_all_Component.ToString() + "\n\n"
            + "  / stackingcycle in seconds, 0 = stacking off\n"
            + "stacking_cycle=" + stacking_cycle.ToString() + "\n\n"
            + "  / group of PIM controlled Weapons\n  / Control of WeaponCore Turrets is not necessary\n  / and should remain switched off.\n"
            + "PIM_controlled_Weapons=" + gungroupName + "\n\n"
            + X_Line + "  / mods that can be used.\n  /     is there a mod missing? \n  /           write it in the comments of SMS or PIM\n\n";
            foreach (var mod in usedMods.Keys) configstr += mod + "=" + usedMods[mod].ToString() + "\n";
            configstr += "\n" + X_Config_end + "\n";
            Me.CustomData = configstr;
        }
        void loadConfig()
        {
            bool config = false;
            foreach (var s1 in Me.CustomData.Split('\n'))
            {
                var s = s1.Trim();
                if (s.Length == 0 || s[0] == '/') continue;
                else if (s == X_Config)
                {
                    config = true;
                    continue;
                }
                else if (s == X_Config_end) break;
                if (config)
                {
                    var cs = s.Split('=');
                    if (cs.Length < 2) continue;
                    switch (cs[0])
                    {
                        case "ShowInfoPBLcd": ShowInfoPBLcd = if_true(cs[1]); break;
                        case "delete_queueItem_if_max": delete_queueItem_if_max = if_true(cs[1]); break;
                        case "always_recycle_greywater": always_recycle_greywater = if_true(cs[1]); break;
                        case "assemblers_off": assemblers_off = if_true(cs[1]); break;
                        case "refinerys_off": refinerys_off = if_true(cs[1]); break;
                        case "collect_all_Ore": collect_all_Ore = if_true(cs[1]); break;
                        case "collect_all_Ingot": collect_all_Ingot = if_true(cs[1]); break;
                        case "collect_all_Component": collect_all_Component = if_true(cs[1]); break;
                        case "stacking_cycle": int.TryParse(cs[1], out stacking_cycle); break;
                        case "PIM_controlled_Weapons": gungroupName = cs[1]; break;
                        default: if (usedMods.ContainsKey(cs[0])) usedMods[cs[0]] = if_true(cs[1]); break;
                    }
                    continue;
                }
            }
            writeConfig();
        }
        List<string> autocrafting_Types = new List<string>();
        void InitAutoCraftingTypes()
        {
            foreach (var bpType in bprints.Values)
                if (!autocrafting_Types.Contains(bpType.AutoCraftingType))
                    autocrafting_Types.Add(bpType.AutoCraftingType);
        }
        string bigSpaces = new string(' ', 85);
        const string AutoCraftingTypeStringName = "AutocraftingTypes";
        Filter filter = new Filter();
        void loadAutocratingDefinitions()
        {
            var lcds = new List<IMyTextPanel>();
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(lcds, block => block.CustomName.Contains(Autocrafting));
            foreach (var lcd in lcds)
            {
                var ac_Types = IG_Component + "," + IG_Ammo;
                var act_new = false;
                var ac_TypesFilter = new Dictionary<string, string>();
                foreach (var s1 in lcd.GetText().Split('\n'))
                {
                    var acLines = s1.Split('|', '=', '%', ':');
                    for (int i = 0; i < acLines.Count(); i++) acLines[i] = acLines[i].Trim(' ', '\u00AD');
                    if (acLines.Count() == 0 || acLines[0] == "" || acLines[0][0] == '/') continue;
                    else if (acLines[0] == AutoCraftingTypeStringName && acLines.Count() > 1)
                    {
                        if (acLines[1] != "") ac_Types = acLines[1];
                    }
                    else if (acLines[0] == X_Autocrafting_treshold)
                    {
                        // schwellwert auswerten
                        var oldacf = AutocraftingThreshold;
                        AutocraftingThreshold = getInteger(acLines[1]);
                        if (AutocraftingThreshold == 0) AutocraftingThreshold = 80;
                        if (oldacf != AutocraftingThreshold) act_new = true;
                    }
                    else if (acLines[0] == "Type" && acLines.Count() > 2)
                    {
                        if (!ac_TypesFilter.ContainsKey(acLines[1])) ac_TypesFilter.Add(acLines[1], acLines[2]);
                    }
                    else if (acLines.Count() == 6)
                    {
                        if (bprints.ContainsKey(acLines[4])) bprints[acLines[4]].SetMaximumAmount(getIntegerWithPräfix(acLines[2]));
                    }
                }
                if (act_new) AssemblerBluePrint.SetAutocraftingThresholdNew();
                // autocrafting config schreiben
                var acString = "/ Autocraftingdefinition:\n";
                acString += "/ add '...(sms)' to the name of assemblers to crafting their items,\n/ and set the max quantity as you want\n\n";
                acString += "/ if the quantity of items falls below this percentage value,\n/ then it will be increased to max.\n" + X_Autocrafting_treshold + " = " + AutocraftingThreshold + "%\n\n";
                acString += "/ possible autocrafting types, please add them separated by comma.\n/ ";
                foreach (var t in autocrafting_Types) acString += t + ",";
                acString += "\n" + AutoCraftingTypeStringName + "=" + ac_Types;
                acString += "\n\n/                           Item            |     current    ­|       max      ­|   assembly\n";
                var ac_TypesList = ac_Types.Split(',');
                foreach (var actype in ac_TypesList)
                {
                    acString += line2pur + "\n Type : " + actype + " = ";
                    if (ac_TypesFilter.ContainsKey(actype))
                    {
                        acString += (ac_TypesFilter[actype] == "" ? " * " : ac_TypesFilter[actype]);
                        filter.InitFilter(ac_TypesFilter[actype]);
                    }
                    else
                    {
                        acString += " * ";
                        filter.SetFilterToAll();
                    }
                    acString += line2;
                    var bpList = bprints.Values.ToList().FindAll(b => b.AutoCraftingType == actype && filter.ifFilter(b.AutoCraftingName));
                    bpList.Sort((x, y) => x.AutoCraftingName.CompareTo(y.AutoCraftingName));
                    foreach (var bp in bpList)
                    {
                        acString += " "
                            + getDisplayBoxString(bp.AutoCraftingName, 60)
                            + " | "
                            + getDisplayBoxString(bp.CurrentItemAmount, 25)
                            + " | "
                            + getDisplayBoxString(bp.MaximumItemAmount, 25)
                            + " | "
                            + getDisplayBoxString((bp.AssemblyAmount > 0 ? bp.AssemblyAmount : (bp.RefineryAmount > 0 ? bp.RefineryAmount : 0)), 25)
                            + bigSpaces
                            + "|"
                            + bp.BlueprintID
                            + "|\n";
                    }
                }
                lcd.Alignment = TextAlignment.LEFT;
                lcd.ContentType = ContentType.TEXT_AND_IMAGE;
                lcd.WriteText(acString);
            }
        }
        class Filter
        {
            List<string> FilterWhiteList = new List<string>();
            List<string> FilterBlackList = new List<string>();
            public void SetFilterToAll()
            {
                FilterBlackList.Clear();
                FilterWhiteList.Clear();
                FilterWhiteList.Add("*");
            }
            public void InitFilter(string filterString)
            {
                FilterBlackList.Clear();
                FilterWhiteList.Clear();
                foreach (var s in filterString.Split(','))
                {
                    var filterStringTrimmed = s.Trim();
                    if (filterStringTrimmed.Length > 0 && filterStringTrimmed[0] == '-') FilterBlackList.Add(filterStringTrimmed.Substring(1));
                    else FilterWhiteList.Add(filterStringTrimmed);
                }
            }
            public bool ifFilter(string testName)
            {
                foreach (string s in FilterBlackList) if (testName.Contains(s)) return false;
                foreach (string s in FilterWhiteList) if (s == "*" || testName.Contains(s)) return true;
                return false;
            }
        }
        void addBluePrint(string typeID, string subtypeID, string itemName, string alternativItemName)
        {
            if (curmod != "Vanilla" && (usedMods.ContainsKey(curmod) ? !usedMods[curmod] : true)) return;
            MyDefinitionId id;
            if (!MyDefinitionId.TryParse("MyObjectBuilder_BlueprintDefinition/" + subtypeID, out id)) return;
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
                if (s == "Magnetron_Component") return cs + " " + s;
                if (t == cs && s.EndsWith(cs)) return t + " " + s.Substring(0, s.Length - cs.Length);
                if (s.StartsWith("NATO_25"))
                {
                    cs = "Magazine";
                    return t + " " + s.Substring(0, s.Length - cs.Length);
                }
                if (t == IG_Tools)
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
                ItemName = alter == "" ? BluePrintNameToItemName(type, definition_id.SubtypeName) : alter;
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
                else AssemblingDeltaAmount = 0;
            }
            public void CalcPriority()
            {
                const int AssemblyPrioRange = 1000000;
                const int AssemblyPrioRangeMinus = -AssemblyPrioRange;
                if (MaximumItemAmount > 0) ItemPriority = (AssemblingDeltaAmount < -100 ? -100 : (AssemblingDeltaAmount > 100 ? 100 : AssemblingDeltaAmount)) * AssemblyPrioRange / MaximumItemAmount;
                else ItemPriority = 0;
                if (ItemPriority < AssemblyPrioRangeMinus) ItemPriority = AssemblyPrioRangeMinus;
                else if (ItemPriority > AssemblyPrioRange) ItemPriority = AssemblyPrioRange;
            }
        }
        static List<View> viewList = new List<View>();
        abstract class View
        {
            public const string Color_Red = "[Color=#FFFF0000]", Color_Yellow = "[Color=#FF000000]", Color_End = "[/Color]";
            public enum ViewType { NONE, INFO, WARNING, ERROR }
            DateTime burn = DateTime.Now;
            public ViewType type = ViewType.NONE;
            int lebenssekunden = -1;
            string infotext = "";
            public View()
            {
                type = ViewType.INFO;
            }
            public View(ViewType itype)
            {
                type = itype;
            }
            public View(string itext, ViewType itype, int sec = -1)
            {
                infotext = itext;
                type = itype;
                lebenssekunden = sec;
            }
            public virtual string getInfoText() { return infotext; }
            public bool isOver() { if (lebenssekunden == -1) return false; if ((DateTime.Now - burn).TotalSeconds > lebenssekunden) return true; return false; }
            public void setOver() { lebenssekunden = 0; }
        }
        class Info : View
        {
            public Info(string text, int sec = -1) : base(text, ViewType.INFO, sec) { }
        }
        class Warning : View
        {
            public enum ID { NONE, CARGOUSEHEAVY, CARGOUSEFULL, Incinerator, RefineryNotSupportet }
            public ID w_ID = ID.NONE;
            public string subType = "";
            public Warning(ID warn_ID, string isubtype = "") : base(ViewType.WARNING) { w_ID = warn_ID; subType = isubtype; }
            public override string getInfoText()
            {
                switch (w_ID)
                {
                    case ID.RefineryNotSupportet: return "refinery '" + subType + "' not supported.";
                    case ID.Incinerator: return "incinerator (IOmod) not supported.";
                    case ID.CARGOUSEHEAVY: return "cargo with " + subType + " is heavy.";
                    case ID.CARGOUSEFULL: return "cargo with " + subType + " is full !!!!!";
                }
                return "";
            }
        }
        class ErrorInfo : View
        {
            public enum ErrorID { NONE, CARGOFULL, };
            public ErrorInfo(ErrorID e_id) : base("", ViewType.ERROR) { }
        }
        class AmmoManagerInfo : View
        {
            public override string getInfoText()
            {
                if (guns.Count == 0) return "";
                return "  AmmoMan.: controls " + guns.Count + " weapons.";
            }
        }
        class StorageManagerInfo : View
        {
            public override string getInfoText()
            {
                if (storageCargos.Count == 0) return "";
                return "StorageMan.: controls " + storageCargos.Count + " containers.";
            }
        }
        static void setInfo(string warnungstext, int zeit = 10)
        {
            viewList.Add(new Info(warnungstext, zeit));
        }
        static void setWarning(Warning.ID warnID, string subtype = "")
        {
            if (getWarning(warnID, subtype) == null) viewList.Add(new Warning(warnID, subtype));
        }
        static Warning getWarning(Warning.ID warnID, string subtype)
        {
            foreach (var v in viewList)
            {
                if (v is Warning)
                {
                    var w = v as Warning;
                    if ((w.w_ID == warnID) && w.subType == subtype) return w;
                }
            }
            return null;
        }
        void clearWarning(Warning.ID warnID, string subtype)
        {
            var w = getWarning(warnID, subtype);
            if (w != null) w.setOver();
        }
        string getInfos()
        {
            var infostring = "\n";
            var warnungstring = "";
            var errorstring = "";
            foreach (var v in viewList)
            {
                switch (v.type)
                {
                    case View.ViewType.INFO:
                        var s = v.getInfoText();
                        infostring += s + (s == "" ? "" : "\n");
                        break;
                    case View.ViewType.WARNING: warnungstring += v.getInfoText() + "\n"; break;
                    case View.ViewType.ERROR: errorstring += v.getInfoText() + "\n"; break;
                }
            }
            if (warnungstring != "") infostring += "\nWarning:\n" + warnungstring + "\n";
            if (errorstring != "") infostring += "\nErrors:\n" + errorstring;
            return infostring;
        }
        void writeInfo()
        {
            var s = SI1 + SI2 + getRunningSign() + (master == null ? (" Running / " + MAXIC + " inst. per run\ncurrent cycle: " + currentCycleInSec.ToString("0.0") + " sec.\n" + getInfos()) : "Standby\nMaster: " + master.CustomName);
            Echo(s);
            if (ShowInfoPBLcd)
            {
                var tp = Me.GetSurface(0);
                tp.Alignment = TextAlignment.LEFT;
                tp.ContentType = ContentType.TEXT_AND_IMAGE;
                tp.WriteText(s);
            }
        }
        public Program()
        {
            viewList.Add(new AmmoManagerInfo());
            viewList.Add(new StorageManagerInfo());
            RefineryBlueprint.InitScrapTypeBlueprintTypes();
            loadItemDefinitionen();
            InitRefineryBlueprints();
            MAXIC = minIC;
            rti = Runtime;
            Slave();
            rti.UpdateFrequency = UpdateFrequency.Update10;
            if (collect_all_Ore) collectAll_List.Add(IG_ + "Ore");
            if (collect_all_Ingot) collectAll_List.Add(IG_ + IG_I);
            if (collect_all_Component) collectAll_List.Add(IG_ + IG_Component);
            stack_type = stack_types[0];
        }
        string Debug_RefineryBPs()
        {
            var DebugText = "Accepted BluePrints by Refinerysubtype\n";
            foreach (var refSubType in Refinery.refineryTypesAcceptedBlueprintsList.Keys)
            {
                DebugText += "SubType:" + refSubType + "\n";
                foreach (var refBP in Refinery.refineryTypesAcceptedBlueprintsList[refSubType])
                {
                    DebugText += "bprint -> " + refBP.Name + "\n";
                }
            }
            return DebugText;
        }
        string Debug_AddIPrioLists()
        {
            var DebugText = "";
            foreach (var IPrioListKey in ingotprio.Keys)
            {
                DebugText += "IPrioList:" + IPrioListKey + "\n";
                foreach (var ip in ingotprio[IPrioListKey])
                {
                    DebugText += "\t- " + ip.refineryBP.Definition_id + " % " + ip.prio + "\n";
                }
            }
            return DebugText;
        }

        void debug()
        {
            var panel = GridTerminalSystem.GetBlockWithName("PIMXXXDEBUG") as IMyTextPanel;
            if (panel == null) return;
            if (panel.CubeGrid != Me.CubeGrid) return;
            var s = "";
            s += Debug_AddIPrioLists();
            s += Debug_RefineryBPs();
            panel.WriteText(s + "\n" + debugString);
            debugString = "";
        }
        bool maxInstructions() { return rti.CurrentInstructionCount > MAXIC; }
        DateTime lastStart = DateTime.Now;
        void Main(string argument, UpdateType updateSource)
        {
            if (argument != "")
            {
                switch (argument.ToLower())
                {
                    case "flushrefinerys_all":
                        foreach (var o in oefen) o.FlushAllInventorys();
                        setInfo("all (" + oefen.Count + ") refinerys flushed.");
                        break;
                }
                return;
            }
            do
            {
                switch (m0)
                {
                    case 0:
                        if (MainLoopTimeSpan.IfTimeSpanReady()) m0++;  // ToDo: deswegen nur alle 5 sec. cyclus???
                        else { writeInfo(); return; }
                        currentCycleInSec = (DateTime.Now - lastStart).TotalSeconds;
                        lastStart = DateTime.Now;
                        break;
                    case 1:
                        if (!changeAutoCraftingSettings)
                        {
                            debugString += "kein calc_ACDef\n";
                            m0 += 2;
                            break;
                        }
                        debugString += "calc_ACDef\n";
                        GridTerminalSystem.GetBlocksOfType<IMyAssembler>(ass, block => block.CubeGrid == Me.CubeGrid);
                        GridTerminalSystem.GetBlocksOfType<IMyRefinery>(raff, block => block.CubeGrid == Me.CubeGrid);
                        s0 = new List<string>(bprints.Keys);
                        for (int i = s0.Count - 1; i >= 0; i--)
                        {
                            var b = bprints[s0[i]];
                            bprints_pool.Add(s0[i], b);
                            bprints.Remove(s0[i]);
                        }
                        s0 = new List<string>(bprints_pool.Keys);
                        m1 = s0.Count - 1;
                        m0++;
                        changeAutoCraftingSettings = false;
                        break;
                    case 2:
                        for (int i = m1; i >= 0; i--, m1--)
                        {
                            if (maxInstructions()) { writeInfo(); return; }
                            var b = bprints_pool[s0[i]];
                            if (s0[i] == Ingot.SubFresh || s0[i] == (Refinery.BluePrintID_SpentFuelReprocessing))
                            {
                                foreach (var r in raff)
                                {
                                    var subTypeName = r.BlockDefinition.SubtypeId;
                                    if (r.CustomName.Contains("(sms") && (subTypeName.Contains("Hydroponics") || subTypeName.Contains("Reprocessor")))
                                    {
                                        bprints.Add(s0[i], b);
                                        bprints_pool.Remove(s0[i]);
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                foreach (var a in ass)
                                {
                                    if (a.CustomName.Contains("(sms") && a.CanUseBlueprint(b.definition_id))
                                    {
                                        bprints.Add(s0[i], b);
                                        bprints_pool.Remove(s0[i]);
                                        break;
                                    }
                                }
                            }
                        }
                        InitAutoCraftingTypes();
                        m0++;
                        break;
                    case 3:
                        if (!Slave()) { writeInfo(); return; }
                        loadAutocratingDefinitions();
                        debug();
                        ClearInventoryList(inventar);
                        InventoryList_SMSflagged.Clear();
                        InventoryList_nonSMSflagged.Clear();
                        CargoUseList.Clear();
                        foreach (var ivl in i_l.Values) ivl.Clear();
                        i_l.Clear();
                        m0++;
                        break;
                    case 4:
                        GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(tbl, block => block.IsSameConstructAs(Me));
                        m1 = 0;
                        m0++;
                        break;
                    case 5:
                        for (int i = m1; i < tbl.Count; i++, m1++)
                        {
                            if (maxInstructions()) { writeInfo(); return; }
                            pushTerminalBlock(tbl[i]);
                        }
                        m0++;
                        break;
                    case 6:
                        GridTerminalSystem.GetBlocksOfType<IMyShipConnector>(tbl, block => block.IsSameConstructAs(Me));
                        m1 = 0;
                        m0++;
                        break;
                    case 7:
                        for (int i = m1; i < tbl.Count; i++, m1++)
                        {
                            if (maxInstructions()) { writeInfo(); return; }
                            pushTerminalBlock(tbl[i]);
                        }
                        m0++;
                        break;
                    case 8:
                        GridTerminalSystem.GetBlocksOfType<IMyShipController>(tbl, block => block.IsSameConstructAs(Me));
                        m1 = 0;
                        m0++;
                        break;
                    case 9:
                        for (int i = m1; i < tbl.Count; i++, m1++)
                        {
                            if (maxInstructions()) { writeInfo(); return; }
                            pushTerminalBlock(tbl[i]);
                        }
                        m0++;
                        break;
                    // find Weapons
                    case 10:
                        var group = GridTerminalSystem.GetBlockGroupWithName(gungroupName);
                        if (group == null)
                        {
                            if (guns.Count > 0)
                            {
                                for (int i = guns.Count - 1; i >= 0; i--) guns[i].Remove();
                                guns.Clear();
                            }
                            m0++;
                            m0++;
                            break;
                        }
                        else group.GetBlocksOfType<IMyUserControllableGun>(ugun, block => block.IsSameConstructAs(Me));
                        for (int i = guns.Count - 1; i >= 0; i--)
                        {
                            if (ugun.Contains(guns[i].gun)) ugun.Remove(guns[i].gun);
                            else
                            {
                                var gun = guns[i];
                                gun.Remove();
                                guns.Remove(gun);
                            }
                        }
                        m1 = ugun.Count - 1;
                        m0++;
                        break;
                    case 11:
                        for (int i = m1; i >= 0; i--, m1--)
                        {
                            if (maxInstructions()) { writeInfo(); return; }
                            guns.Add(new Gun(ugun[i]));
                        }
                        m0++;
                        break;
                    case 12:
                        m1 = 0;
                        m0++;
                        break;
                    case 13:
                        for (int i = m1; i < guns.Count; i++, m1++)
                        {
                            if (maxInstructions()) { writeInfo(); return; }
                            guns[i].Refresh();
                        }
                        m0++;
                        break;
                    case 14:
                        GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(tbl, cargo => (cargo.CustomName.Contains(X_StorageTag)));
                        for (int i = storageCargos.Count - 1; i >= 0; i--)
                        {
                            if (tbl.Contains(storageCargos[i].container)) tbl.Remove(storageCargos[i].container);
                            else
                            {
                                var stor = storageCargos[i];
                                stor.Remove();
                                storageCargos.Remove(stor);
                            }
                        }
                        m1 = tbl.Count - 1;
                        m0++;
                        break;
                    case 15:
                        for (int i = m1; i >= 0; i--, m1--)
                        {
                            if (maxInstructions()) { writeInfo(); return; }
                            storageCargos.Add(new StorageCargo(tbl[i]));
                        }
                        m0++;
                        break;
                    case 16:
                        m0++;
                        break;
                    case 17:
                        m0++;
                        break;
                    case 18: m0++; break;
                    case 19: m0++; break;
                    case 20: m0++; break;
                    case 21: m0++; break;
                    case 22: m0++; break;
                    case 23: m0++; break;
                    case 24:
                        GridTerminalSystem.GetBlocksOfType<IMyShipWelder>(tbl, block => block.IsSameConstructAs(Me));
                        m1 = 0;
                        m0++;
                        break;
                    case 25:
                        for (int i = m1; i < tbl.Count; i++, m1++)
                        {
                            if (maxInstructions()) { writeInfo(); return; }
                            var bd = tbl[i].BlockDefinition.SubtypeId.ToString();
                            if (bd.Contains("ShipLaserMultitool"))
                            {
                                bool weld = true;
                                var pp = tbl[i].GetProperty("ToolMode");
                                if (pp != null && pp.TypeName == "Boolean") weld = tbl[i].GetValue<bool>("ToolMode");
                                if (weld) { if (!(tbl[i] as IMyFunctionalBlock).Enabled) pushTerminalBlock(tbl[i]); }
                                else pushTerminalBlock(tbl[i]);
                            }
                            else if (!(tbl[i] as IMyFunctionalBlock).Enabled) pushTerminalBlock(tbl[i]);
                        }
                        m0++;
                        break;
                    case 26:
                        GridTerminalSystem.GetBlocksOfType<IMyShipGrinder>(tbl, block => block.IsSameConstructAs(Me));
                        m1 = 0;
                        m0++;
                        break;
                    case 27:
                        for (int i = m1; i < tbl.Count; i++, m1++)
                        {
                            if (maxInstructions()) { writeInfo(); return; }
                            pushTerminalBlock(tbl[i]);
                        }
                        m0++;
                        break;
                    case 28:
                        GridTerminalSystem.GetBlocksOfType<IMyShipDrill>(tbl, block => block.IsSameConstructAs(Me));
                        m1 = 0;
                        m0++;
                        break;
                    case 29:
                        for (int i = m1; i < tbl.Count; i++, m1++)
                        {
                            if (maxInstructions()) { writeInfo(); return; }
                            pushTerminalBlock(tbl[i]);
                        }
                        m0++;
                        break;
                    case 30:
                        m1 = 0;
                        if (stacking_cycle == 0 || (DateTime.Now - StackingCounter).TotalSeconds < stacking_cycle) m0 += 2;
                        else
                        {
                            StackItemList.Clear();
                            StackItem.ClearStackInventory();
                            if (i_l.ContainsKey(stack_type))
                            {
                                foreach (var i in i_l[stack_type])
                                {
                                    new_stackcount(i, stack_type);
                                    StackItem.CalculateFreeInventory(i);
                                }
                            }
                            if (stack_mode == 0)
                            {
                                int max_stack = 0;
                                foreach (var s in StackItemList) if (max_stack < s.Stackcount) max_stack = s.Stackcount;
                                if (max_stack > 1)
                                {
                                    StackItem.CurrentStackingType = StackItem.StackingType.Stack;
                                    StackItemList.Sort();
                                }
                                else
                                {
                                    stack_mode = -1;
                                }
                            }
                            else if (stack_mode == 1)
                            {
                                int max_stack = 0;
                                foreach (var s in StackItemList) if (max_stack < s.Stackcount) max_stack = s.Stackcount;
                                if (max_stack > 1)
                                {
                                    StackItem.CurrentStackingType = StackItem.StackingType.Stack;
                                    StackItemList.Sort();
                                }
                                else
                                {
                                    stack_mode = -1;
                                }
                            }
                            else if (stack_mode == 2)
                            {
                                StackItem.CurrentStackingType = StackItem.StackingType.VolumeBack;
                                StackItemList.Sort();
                            }
                            else if (stack_mode == 3)
                            {
                                StackItem.CurrentStackingType = StackItem.StackingType.Volume;
                                StackItemList.Sort();
                            }
                            else if (stack_mode == 4)
                            {
                                StackItem.CurrentStackingType = StackItem.StackingType.Stack;
                                StackItemList.Sort();
                                while (StackItemList.Count > 0)
                                {
                                    var s = StackItemList[0];
                                    if (s.check_stacking_gamma()) break;
                                    StackItemList.Remove(s);
                                }
                            }
                            StackingCounter = DateTime.Now;
                            m1 = 0;
                        }
                        m0++;
                        break;
                    case 31:
                        switch (stack_mode)
                        {
                            case 0:
                                for (int i = m1; i < StackItemList.Count; i++, m1++)
                                {
                                    if (maxInstructions()) { writeInfo(); return; }
                                    StackItemList[i].stacking_single();
                                }
                                m0++;
                                stack_mode++;
                                break;
                            case 1:
                                for (int i = m1; i < StackItemList.Count; i++, m1++)
                                {
                                    if (maxInstructions()) { writeInfo(); return; }
                                    StackItemList[i].stacking_alpha();
                                }
                                m0++;
                                stack_mode++;
                                break;
                            case 2:
                                for (int i = m1; i < StackItemList.Count; i++, m1++)
                                {
                                    if (maxInstructions()) { writeInfo(); return; }
                                    if (!StackItemList[i].stacking_beta()) break;
                                }
                                m0++;
                                stack_mode++;
                                break;
                            case 3:
                                for (int i = m1; i < StackItemList.Count; i++, m1++)
                                {
                                    if (maxInstructions()) { writeInfo(); return; }
                                    StackItemList[i].stacking_delta();
                                }
                                m0++;
                                stack_mode++;
                                break;
                            case 4:
                                if (StackItemList.Count > 0) for (int i = m1; i < 300; i++, m1++)
                                    {
                                        if (maxInstructions()) { writeInfo(); return; }
                                        if (StackItemList[0].stacking_gamma()) break;
                                    }
                                m0++;
                                stack_mode++;
                                break;
                            default:
                                stack_mode = 0;
                                cur_stack_type++;
                                if (cur_stack_type >= stack_types.Length) cur_stack_type = 0;
                                stack_type = stack_types[cur_stack_type];
                                m0++;
                                break;
                        }
                        break;
                    case 32:
                        m0++;
                        break;
                    case 33:
                        foreach (var b in bprints.Values) b.AssemblyAmount = 0;
                        foreach (var b in bprints_pool.Values) b.AssemblyAmount = 0;
                        GridTerminalSystem.GetBlocksOfType<IMyRefinery>(raff, block => block.CubeGrid == Me.CubeGrid);
                        for (int i = oefen.Count - 1; i >= 0; i--)
                        {
                            if (raff.Contains(oefen[i].RefineryBlock)) raff.Remove(oefen[i].RefineryBlock);
                            else
                            {
                                changeAutoCraftingSettings = true;
                                oefen.Remove(oefen[i]);
                            }
                        }
                        Refinery.priobt = "";
                        m1 = raff.Count - 1;
                        m0++;
                        break;
                    case 34:
                        for (int i = m1; i >= 0; i--, m1--)
                        {
                            if (maxInstructions()) { writeInfo(); return; }
                            oefen.Add(new Refinery(raff[i]));
                        }
                        m0++;
                        break;
                    case 35:
                        GridTerminalSystem.GetBlocksOfType<IMyAssembler>(ass, block => block.CubeGrid == Me.CubeGrid);
                        for (int i = okumas.Count - 1; i >= 0; i--)
                        {
                            if (ass.Contains(okumas[i].AssemblerBlock)) ass.Remove(okumas[i].AssemblerBlock);
                            else
                            {
                                changeAutoCraftingSettings = true;
                                okumas.Remove(okumas[i]);
                            }
                        }
                        m1 = ass.Count - 1;
                        m0++;
                        break;
                    case 36:
                        for (int i = m1; i >= 0; i--, m1--)
                        {
                            if (maxInstructions()) { writeInfo(); return; }
                            okumas.Add(new Assembler(ass[i]));
                        }
                        m0++;
                        break;
                    case 37:
                        m1 = 0;
                        m0++;
                        break;
                    case 38:
                        for (int i = m1; i < InventoryList_SMSflagged.Count; i++, m1++)
                        {
                            if (maxInstructions()) { writeInfo(); return; }
                            ClearInventory(InventoryList_SMSflagged[i]);
                        }
                        m0++;
                        break;
                    case 39:
                        m1 = 0;
                        m0++;
                        break;
                    case 40:
                        for (int i = m1; i < InventoryList_nonSMSflagged.Count; i++, m1++)
                        {
                            if (maxInstructions()) { writeInfo(); return; }
                            ClearInventory(InventoryList_nonSMSflagged[i], collectAll_List);
                        }
                        m0++;
                        break;
                    case 41:
                        m1 = 0;
                        foreach (var a in ammoDefs.Values) a.CalcAmmoInventoryRatio();
                        m0++;
                        break;
                    case 42: // ItemTransfer
                        for (int i = m1; i < storageinvs.Count; i++, m1++)
                        {
                            if (maxInstructions()) { writeInfo(); return; }
                            storageinvs[i].reloadItems();
                        }
                        m0++;
                        break;
                    case 43:
                        Refinery.cn = 0;
                        foreach (var b in refineryBlueprints) b.RefineryCount = 0;
                        m1 = 0;
                        m0++;
                        break;
                    case 44:
                        for (int i = m1; i < oefen.Count; i++, m1++)
                        {
                            if (maxInstructions()) { writeInfo(); return; }
                            oefen[i].Refresh();
                        }
                        m0++;
                        break;
                    case 45:
                        CalcIngotPrio();
                        RenderAmmoPrioLCDS();
                        RenderResourceProccesingLCD();
                        m1 = 0;
                        m0++;
                        break;
                    case 46:
                        for (int i = m1; i < oefen.Count; i++, m1++)
                        {
                            if (maxInstructions()) { writeInfo(); return; }
                            oefen[i].RefineryManager();
                        }
                        m0++;
                        break;
                    case 47:
                        m1 = 0;
                        m0++;
                        break;
                    case 48:
                        for (int i = m1; i < okumas.Count; i++, m1++)
                        {
                            if (maxInstructions()) { writeInfo(); return; }
                            okumas[i].Refresh();
                        }
                        m0++;
                        break;
                    case 49:
                        s0 = new List<string>(bprints.Keys);
                        m1 = 0;
                        m0++;
                        break;
                    case 50:
                        for (int i = m1; i < s0.Count; i++, m1++)
                        {
                            if (maxInstructions()) { writeInfo(); return; }
                            var b = bprints[s0[i]];
                            b.SetCurrentAmount((int)inventar.GetValueOrDefault(b.ItemName, 0));
                            //b.SetCurrentAmount(inventar.ContainsKey(b.ItemName) ? (int)inventar[b.ItemName] : 0);
                            b.CalcPriority();
                            if (b.NeedsAssembling())
                            {
                                if (s0[i] != Ingot.SubFresh)
                                {
                                    foreach (var o in okumas) o.AddValidBlueprint(b);
                                }
                            }
                        }
                        m0++;
                        break;
                    case 51:
                        s0 = new List<string>(bprints_pool.Keys);
                        m1 = 0;
                        m0++;
                        break;
                    case 52:
                        for (int i = m1; i < s0.Count; i++, m1++)
                        {
                            if (maxInstructions()) { writeInfo(); return; }
                            var b = bprints_pool[s0[i]];
                            if (inventar.ContainsKey(b.ItemName)) b.SetCurrentAmount((int)inventar[b.ItemName]);
                        }
                        m0++;
                        break;
                    case 53:
                        m1 = 0;
                        m2 = 0;
                        okumas.Sort();
                        m0++;
                        break;
                    case 54:
                        for (int i = m1; i < okumas.Count; i++, m1++)
                        {
                            if (maxInstructions()) { writeInfo(); return; }
                            var o = okumas[i];
                            if (o.AssemblerBlock.CubeGrid == Me.CubeGrid && o.pms.Control())
                            {
                                if (o.BlueprintList.Count > 0)
                                {
                                    o.BlueprintList.Sort();
                                    var b = o.BlueprintList[0];
                                    m2++;
                                    if (o.AddBlueprintToQueue(b))
                                    {
                                        foreach (var oo in b.o) oo.BlueprintList.Remove(b);
                                    }
                                    else o.BlueprintList.Remove(b);
                                }
                            }
                        }
                        if (m2 == 0) m0++;
                        else m0--;
                        break;
                    default:
                        for (int i = viewList.Count - 1; i >= 0; i--) { if (viewList[i].isOver()) viewList.Remove(viewList[i]); }
                        foreach (var c in CargoUseList.Keys)
                        {
                            var cargoUseRatio = CargoUseList[c].GetCarcocapacityUseRatio();
                            if (cargoUseRatio >= 90)
                            {
                                setWarning(Warning.ID.CARGOUSEHEAVY, c);
                                clearWarning(Warning.ID.CARGOUSEFULL, c);
                            }
                            else if (cargoUseRatio >= 99)
                            {
                                setWarning(Warning.ID.CARGOUSEFULL, c);
                                clearWarning(Warning.ID.CARGOUSEHEAVY, c);
                            }
                            else
                            {
                                clearWarning(Warning.ID.CARGOUSEHEAVY, c);
                                clearWarning(Warning.ID.CARGOUSEFULL, c);
                            }
                        }
                        firstRun = false;
                        m0 = 0;
                        break;
                }
            }
            while (!maxInstructions());
            writeInfo();
        }
        static void ClearInventoryList(Dictionary<string, float> invList)
        {
            var keys = invList.Keys.ToArray();
            for (int i = 0; i < keys.Length; i++) invList[keys[i]] = 0;
        }
        static void AddToInventory(IMyInventory box, Dictionary<string, float> ilist = null)
        {
            var boxl = new List<MyInventoryItem>();
            box.GetItems(boxl);
            foreach (var boxi in boxl)
            {
                string index = GetPIMItemID(boxi.Type);
                var boxia = (float)boxi.Amount;
                if (inventar.ContainsKey(index)) inventar[index] += boxia;
                else inventar.Add(index, boxia);
                if (ilist != null)
                {
                    if (ilist.ContainsKey(index)) ilist[index] += boxia;
                    else ilist.Add(index, boxia);
                }
            }
        }
        void addToInventoryList(IMyInventory inv, Dictionary<string, string> tags)
        {
            foreach (var tag in tags.Keys)
            {
                var ingame = Tag2Ingame(tag);
                if (ingame != "")
                {
                    if (!i_l.ContainsKey(ingame))
                    {
                        i_l.Add(ingame, new List<IMyInventory>());
                    }
                    if (!i_l[ingame].Contains(inv)) i_l[ingame].Add(inv);
                    if (!CargoUseList.ContainsKey(tag)) CargoUseList.Add(tag, new CargoUse(tag));
                    CargoUseList[tag].AddCurrentAndMaxCargocapacity(inv.CurrentVolume.RawValue / 1000, inv.MaxVolume.RawValue / 1000);
                }
            }
        }
        static Dictionary<string, List<IMyInventory>> i_l = new Dictionary<string, List<IMyInventory>>();
        static List<IMyInventory> InventoryList_SMSflagged = new List<IMyInventory>();
        static List<IMyInventory> InventoryList_nonSMSflagged = new List<IMyInventory>();
        void pushTerminalBlock(IMyTerminalBlock t)
        {
            if (t.HasInventory)
            {
                var inv = t.GetInventory(0);
                AddToInventory(inv);
                if (t.CustomName.Contains(X_StorageTag)) return;
                Parameter pm = new Parameter();
                if (pm.ParseArgs(t.CustomName))
                {
                    if (!pm.isPM("Keep")) InventoryList_SMSflagged.Add(inv);
                    if (!pm.isPM("Infolcd")) addToInventoryList(inv, pm.pml);
                }
                else if (t.BlockDefinition.SubtypeId.Contains("Container") || t.BlockDefinition.SubtypeId.Contains("Connector"))
                {
                    InventoryList_SMSflagged.Add(inv);
                }
                else InventoryList_nonSMSflagged.Add(inv);
            }
        }

        int MAXIC;
        void CalculateMaxIC()
        {
            if (master != null)
            {
                rti.UpdateFrequency = UpdateFrequency.Update100;
                MAXIC = minIC;
            }
            else
            {
                rti.UpdateFrequency = UpdateFrequency.Update10; // ToDo: Zeitspanne regulieren!
                if (currentCycleInSec < 3.5) MAXIC -= 100;
                else if (currentCycleInSec > 4.5) MAXIC += 100;
                if (MAXIC < minIC) MAXIC = minIC;
                else if (MAXIC > maxIC) MAXIC = maxIC;
            }
        }
        const string AssemblerQueueNameSemikolon = "@ASSEMBLERQUEUE;";
        const string ItemMaxNameSemikolon = "@ITEMMAX;";
        bool Slave()
        {
            var comp = new List<IMyProgrammableBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyProgrammableBlock>(comp, block => block.IsSameConstructAs(Me));
            master = null;
            foreach (var p in comp)
            {
                if (p.Enabled && p.DetailedInfo.StartsWith(SI1))
                {
                    if (Me.EntityId < p.EntityId)
                    {
                        master = p;
                        break;
                    }
                }
                else if (p.Enabled && p.DetailedInfo.StartsWith(SMS) && !firstRun)
                {
                    var s = "";
                    bool configteil = false;
                    foreach (var cstr in p.CustomData.Split('\n'))
                    {
                        if (cstr.Contains(X_Config)) configteil = true;
                        if (configteil) s += cstr + '\n';
                        if (cstr.Contains(X_Config_end)) configteil = false;
                    }
                    if (s != "") s += "\n\n";
                    foreach (var a in ingotprio.Keys.ToArray()) if (ingotprio.ContainsKey(a) && !Refinery.priobt.Contains("@" + a)) ingotprio.Remove(a);
                    foreach (var sx in ingotprio.Keys)
                    {
                        s += "@INGOTPRIOLIST;" + sx + "\n";
                        foreach (var i in ingotprio[sx]) if (i.initp > 0) s += "@INGOTPRIO;" + i.refineryBP.OutputIDName + ";" + i.initp + "\n";
                    }
                    foreach (var c in CargoUseList.Values) s += "@CARGOUSE;" + c.type + ";" + c.Current + ";" + c.Maximum + "\n";
                    foreach (var b in bprints.Values)
                    {
                        if (b.MaximumItemAmount > 0) s += ItemMaxNameSemikolon + b.ItemName + ";" + b.MaximumItemAmount + ";" + b.subtype + "\n";
                        if (b.AssemblyAmount > 0) s += AssemblerQueueNameSemikolon + b.ItemName + ";" + b.AssemblyAmount + ";" + b.subtype + "\n";
                        else if (b.RefineryAmount > 0) s += AssemblerQueueNameSemikolon + b.ItemName + ";" + b.RefineryAmount + ";" + b.subtype + "\n";
                    }
                    foreach (var b in bprints_pool.Values)
                    {
                        if (b.MaximumItemAmount > 0) s += ItemMaxNameSemikolon + b.ItemName + ";" + b.MaximumItemAmount + ";" + b.subtype + "\n";
                        if (b.AssemblyAmount > 0) s += AssemblerQueueNameSemikolon + b.ItemName + ";" + b.AssemblyAmount + ";" + b.subtype + "\n";
                        else if (b.RefineryAmount > 0) s += AssemblerQueueNameSemikolon + b.ItemName + ";" + b.RefineryAmount + ";" + b.subtype + "\n";
                    }
                    p.CustomData = s;
                }
            }
            CalculateMaxIC();
            return master == null;
        }

        static string[] waste_cast = new string[]{
Ore.Organic,
Ingot.GreyWater};

        static string[] food_cast = new string[]
        {
// Daily Needs Survival
Ingot.WaterFood,
Ingot.CleanWater,
Ingot.SubFresh,
Ingot.Nutrients,
IG_Ingot + "ArtificialFood",
IG_Ingot + "LuxuryMeal",
IG_Ingot + "SabiroidSteak",
IG_Ingot + "VeganFood",
IG_Ingot + "WolfSteak",
IG_Ingot + "WolfBouillon",
IG_Ingot + "SabiroidBouillon",
IG_Ingot + "CoffeeFood",
IG_Ingot + "Potatoes",
IG_Ingot + "Tomatoes",
IG_Ingot + "Carrots",
IG_Ingot + "Cucumbers",
IG_Ingot + "PotatoSeeds",
IG_Ingot + "TomatoSeeds",
IG_Ingot + "CarrotSeeds",
IG_Ingot + "CucumberSeeds",
IG_Ingot + "Ketchup",
IG_Ingot + "MartianSpecial",
"Ore WolfMeat",
"Ore SabiroidMeat",
IG_Ingot + "Fertilizer",
IG_Ingot + "NotBeefBurger",
IG_Ingot + "ToFurkey",
IG_Ingot + "SpaceMealBar",
IG_Ingot + "HotChocolate",
IG_Ingot + "SpacersBreakfast",
IG_Ingot + "ProteinShake",
IG_Ingot + "EmergencyFood",
// Eat, Drink, Sleep & Repeat
IG_Kits + " SparklingWater",
IG_Kits + " Emergency_Ration",
IG_Kits + " AppleJuice",
IG_Kits + " ApplePie",
IG_Kits + " Tofu",
IG_Kits + " MeatRoasted",
IG_Kits + " ShroomSteak",
IG_Kits + " Bread",
IG_Kits + " Burger",
IG_Kits + " Soup",
IG_Kits + " MushroomSoup",
IG_Kits + " TofuSoup",
IG_Kits + " EuropaTea",
IG_Kits + " Mushrooms",
IG_Kits + " Apple",
IG_Kits + " PrlnglesChips",
IG_Kits + " LaysChips",
IG_Kits + " InterBeer",
IG_Kits + " CosmicCoffee",
IG_Kits + " ClangCola",
IG_Kits + " Meat",
IG_Kits + " MeatRoasted",
IG_Ingot + "Soya",
IG_Ingot + "Herbs",
IG_Ingot + "Wheat",
IG_Ingot + "Pumpkin",
IG_Ingot + "Cabbage",
        };

        static string TypeCast(string t)
        {
            if (t.Contains("RifleItem") || t.Contains(IG_Ammo) || t.Contains("PistolItem") || t.Contains("LauncherItem")) return "Armory";
            if (food_cast.Contains(t)) return "Food";
            if (waste_cast.Contains(t)) return "Waste";
            return "";
        }

        // Inventar verteilen
        static void ClearInventory(IMyInventory quelle, List<string> typeID_l = null)
        {
            var von = new List<MyInventoryItem>();
            quelle.GetItems(von);
            if (von.Count() == 0) return;
            for (int j = von.Count() - 1; j >= 0; j--)
            {
                var vcon = von[j].Type;
                var clr = true;
                if (typeID_l != null)
                {
                    clr = false;
                    foreach (var nt in typeID_l)
                        if (vcon.TypeId.ToString() == nt)
                        {
                            clr = true;
                            break;
                        }
                }
                if (!clr) continue;
                var idstr = vcon.TypeId.ToString().Split('_');
                var stype = vcon.SubtypeId.ToString();
                var fullid = idstr[1] + " " + stype;
                var atype = TypeCast(fullid);
                if (i_l.ContainsKey(fullid)) SendItemByNum(quelle, j, i_l[fullid]);
                else if (atype != "" && i_l.ContainsKey(atype)) SendItemByNum(quelle, j, i_l[atype]);
                else if (i_l.ContainsKey(idstr[1])) SendItemByNum(quelle, j, i_l[idstr[1]]);
            }
        }
        void new_stackcount(IMyInventory quelle, string ti)
        {
            var von = new List<MyInventoryItem>();
            quelle.GetItems(von);
            foreach (var i in von)
            {
                if (i.Type.TypeId.Contains(ti) && !i_l.ContainsKey(GetPIMItemID(i.Type)))
                {
                    GetStackItem(i.Type).AddStack(quelle, i.Amount);
                }
            }
        }
        static bool clearItemByType(IMyInventory quelle, string type, MyInventoryItem item, float amount = 0)
        {
            var typeID = type.Substring(0, type.IndexOf(' '));
            var stypeID = type.Substring(type.IndexOf(' ') + 1);
            var atype = TypeCast(type);
            var trans = false;
            if (amount == 0) amount = (float)item.Amount;
            if (i_l.ContainsKey(type)) trans = SendItemByIItem(quelle, item, amount, i_l[type]);
            else if (atype != "" && i_l.ContainsKey(atype)) trans = SendItemByIItem(quelle, item, amount, i_l[atype]);
            else if (i_l.ContainsKey(typeID)) trans = SendItemByIItem(quelle, item, amount, i_l[typeID]);
            return trans;
        }
        static bool SendItemByIItem(IMyInventory quelle, MyInventoryItem item, float amount, List<IMyInventory> ziele)
        {
            var volume = (MyFixedPoint)amount * item.Type.GetItemInfo().Volume;
            if (ziele.Count > 0)
            {
                foreach (var zinv in ziele) if (quelle == zinv) return true;
                for (int i = 0; i < ziele.Count; i++)
                {
                    var ziel_inv = ziele[i];
                    if (volume < (ziel_inv.MaxVolume - ziel_inv.CurrentVolume))
                    {
                        if (quelle.TransferItemTo(ziel_inv, item, (MyFixedPoint)amount)) return true;
                    }
                }
            }
            return false;
        }
        static bool SendItemByNum(IMyInventory quelle, int itemnum, List<IMyInventory> ziele)
        {
            var trans = false;
            if (ziele.Count != 0)
            {
                foreach (var zinv in ziele) if (quelle == zinv) return true;
                for (int i = 0; i < ziele.Count; i++)
                {
                    var ziel_inv = ziele[i];
                    if (!ziel_inv.IsFull)
                    {
                        trans = quelle.TransferItemTo(ziel_inv, itemnum, null, true, null);
                    }
                }
            }
            return trans;
        }
        static bool SendItemByType(string iType, float itemAmount, IMyInventory ziel, int? p = null)
        {
            // PRG.Echo("sendItemByType: " + iType + "/" + itemAmount);
            return SendItemByTypeAndSubtype(IG_ + iType.Substring(0, iType.IndexOf(' ')), iType.Substring(iType.IndexOf(' ') + 1), itemAmount, ziel);
        }
        static bool SendItemByTypeAndSubtype(string itemType, string itemSubType, float itemAmount, IMyInventory ziel, int? p = null)
        {
            List<IMyInventory> quellen = null;
            var idstr = itemType.Split('_');
            var atype = TypeCast(idstr[1] + " " + itemSubType);
            if (i_l.ContainsKey(idstr[1] + " " + itemSubType)) quellen = i_l[idstr[1] + " " + itemSubType];
            else if (atype != "" && i_l.ContainsKey(atype)) quellen = i_l[atype];
            else if (i_l.ContainsKey(idstr[1])) quellen = i_l[idstr[1]];
            else return false;
            for (int i = 0; i < quellen.Count; i++)
            {
                var von = new List<MyInventoryItem>();
                quellen[i].GetItems(von);
                if (von.Count() > 0)
                {
                    for (int j = von.Count() - 1; j >= 0; j--)
                    {
                        if (von[j].Type.TypeId.ToString() == itemType)
                        {
                            if (von[j].Type.SubtypeId.ToString() == itemSubType)
                            {
                                var menge = (MyFixedPoint)itemAmount;
                                if (quellen[i].TransferItemTo(ziel, j, p, true, menge)) return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
        public class Assembler : IComparable<Assembler>
        {
            int BlueprintCount = 0;
            public List<AssemblerBluePrint> BlueprintList = new List<AssemblerBluePrint>();
            public Parameter pms = new Parameter();
            public IMyAssembler AssemblerBlock;
            bool IsSurvivalKit = false;
            bool RemoveItemMode = false;
            public Assembler(IMyAssembler a)
            {
                AssemblerBlock = a;
                IsSurvivalKit = a.BlockDefinition.TypeIdString == "SurvivalKit";
            }
            public int CompareTo(Assembler other)
            {
                if (other.BlueprintCount < BlueprintCount) return 1;
                else if (other.BlueprintCount > BlueprintCount) return -1;
                return 0;
            }
            public void AddValidBlueprint(AssemblerBluePrint bluePrint)
            {
                if (pms.Control() && AssemblerBlock.CanUseBlueprint(bluePrint.definition_id))
                {
                    bluePrint.o.Add(this);
                    bluePrint.NumBluePrintToAssembler++;
                    BlueprintList.Add(bluePrint);
                    BlueprintCount++;
                }
            }
            public bool AddBlueprintToQueue(AssemblerBluePrint bluePrint)
            {
                if (AssemblerBlock.Mode == MyAssemblerMode.Disassembly) return false;
                var ret = false;
                var bpmg = (bluePrint.MaximumItemAmount - bluePrint.CurrentItemAmount - bluePrint.AssemblyAmount);
                var mg = bpmg / bluePrint.NumBluePrintToAssembler;
                if (bpmg < 100)
                {
                    mg = bpmg;
                    ret = true;
                }
                AssemblerBlock.Repeating = false;
                try
                {
                    if (bluePrint.valid) AssemblerBlock.AddQueueItem(bluePrint.definition_id, (MyFixedPoint)mg);
                }
                catch (Exception e)
                {
                    bluePrint.valid = false;
                }
                return ret;
            }
            public void Refresh()
            {
                var proditem_list = new List<MyProductionItem>();
                var bprint_list = new List<AssemblerBluePrint>();
                if (AssemblerBlock.Mode == MyAssemblerMode.Assembly)
                {
                    ClearInventory(AssemblerBlock.GetInventory(1));
                    AssemblerBlock.GetQueue(proditem_list);
                    for (int i = proditem_list.Count - 1; i >= 0; i--)
                    {
                        var bprint = AddProductionAmount(proditem_list[i]);
                        if (bprint != null)
                        {
                            bprint_list.Add(bprint);
                        }
                    }
                }
                else ClearInventory(AssemblerBlock.GetInventory(0));
                if (!pms.ParseArgs(AssemblerBlock.CustomName, true)) return;
                BlueprintList.Clear();
                BlueprintCount = 0;
                if (AssemblerBlock.IsFunctional)
                {
                    if (AssemblerBlock.IsQueueEmpty)
                    {
                        if (!IsSurvivalKit)
                        {
                            if (!assemblers_off || pms.isPM("Nooff")) AssemblerBlock.Enabled = true;
                            else AssemblerBlock.Enabled = false;
                        }
                        if (AssemblerBlock.Mode == MyAssemblerMode.Disassembly) ClearInventory(AssemblerBlock.GetInventory(1));
                        else ClearInventory(AssemblerBlock.GetInventory(0));
                    }
                    else
                    {
                        AssemblerBlock.Enabled = true;
                        if (AssemblerBlock.Mode == MyAssemblerMode.Assembly)
                        {
                            if (RemoveItemMode)
                            {
                                RemoveItemMode = false;
                                if (delete_queueItem_if_max)
                                {
                                    for (int i = proditem_list.Count - 1; i >= 0; i--)
                                    {
                                        var productionItem = proditem_list[i];
                                        foreach (var bprint in bprint_list)
                                        {
                                            if (bprint.definition_id.SubtypeName == productionItem.BlueprintId.SubtypeName
                                                && bprint.MaximumItemAmount != 0
                                                && bprint.MaximumItemAmount <= bprint.CurrentItemAmount)
                                            {
                                                AssemblerBlock.RemoveQueueItem(i, productionItem.Amount);
                                                proditem_list.RemoveAt(i);
                                            }
                                        }

                                    }
                                }
                            }
                            else
                            {
                                RemoveItemMode = true;
                                AssemblerBluePrint firstBlueprint = proditem_list.Count > 1 ? GetBluePrintByProductionItem(proditem_list[0]) : null;
                                if (firstBlueprint != null)
                                {
                                    for (int i = proditem_list.Count - 1; i > 0; i--)
                                    {
                                        var productionItemBlueprint = GetBluePrintByProductionItem(proditem_list[i]);
                                        if (productionItemBlueprint != null
                                            && productionItemBlueprint.MaximumItemAmount != 0
                                            && productionItemBlueprint.ItemPriority > firstBlueprint.ItemPriority)
                                        {
                                            // Move
                                            AssemblerBlock.MoveQueueItemRequest(proditem_list[i].ItemId, 0);
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        class StorageCargo : storageInventory
        {
            public IMyCargoContainer container = null;
            string oldCustomdata = "";
            public StorageCargo(IMyTerminalBlock cargoContainer)
            {
                container = cargoContainer as IMyCargoContainer;
                inv = container.GetInventory();
                storageinvs.Add(this);
            }
            const string X_ItemDef = "StorageItemDefinition", X_ItemDefBegin = "### " + X_ItemDef + "_begin ###", X_ItemDefEnd = "### " + X_ItemDef + "_end ###", X_AddToList = "add_to_list:";
            public override bool checkItems()
            {
                if (container.CustomData != "" && container.CustomData == oldCustomdata) return true;
                bool itemsdef = false;
                var searchstring = "";
                items.Clear();
                foreach (var s in container.CustomData.Split('\n'))
                {
                    var trims = s.Trim();
                    if (trims.StartsWith("/")) continue;
                    else if (trims.Contains(X_ItemDefBegin)) itemsdef = true;
                    else if (trims.Contains(X_ItemDefEnd)) itemsdef = false;
                    else if (!itemsdef && trims.StartsWith(X_AddToList)) searchstring = trims;
                    else if (itemsdef)
                    {
                        var def = trims.Split(';');
                        if (def.Length > 1)
                        {
                            int amount = 0;
                            if (inventar.ContainsKey(def[1]) && int.TryParse(def[0], out amount))
                            {
                                if (amount != 0) items.Add(def[1], amount);
                            }
                        }
                    }
                }
                if (searchstring != "")
                {
                    var search = searchstring.Split(',', ':');
                    for (int i = 1; i < search.Length; i++)
                    {
                        var setr = search[i].Trim().ToLower();
                        if (setr == "") continue;
                        foreach (var t in inventar.Keys)
                        {
                            if (t.ToLower().Contains(setr) && !items.ContainsKey(t))
                            {
                                items.Add(t, 1);
                            }
                        }
                    }
                }
                var cdata = "  / Itemdefinitionen:\n  / amount and type of items to be stored in the container\n  /\n  / add items to the list:\n  / write search terms after the '" + X_AddToList + "', like 'steel' or 'tube'.\n  / close the window, after a few seconds you will find\n  / relevant items in the list below.\n" + X_AddToList + "\n" + X_Line;
                cdata += "  / List of items, delete the lines that are no longer needed,\n  / or set the value to 0.\n  / please change only the value before the semicolon\n" + X_ItemDefBegin + "\n";
                foreach (var i in items) { cdata += i.Value + ";" + i.Key + "\n"; }
                cdata += X_ItemDefEnd + "\n";
                container.CustomData = cdata;
                oldCustomdata = cdata;
                return true;
            }
            public void Remove()
            {
                storageinvs.Remove(this);
            }
        }
        class Gun : storageInventory
        {
            public IMyUserControllableGun gun = null;
            public string curAmmo = "";
            public string gunType = "";
            List<AmmoDefs> ammoTypesDefinition = new List<AmmoDefs>();
            public Dictionary<string, int> ammomax = new Dictionary<string, int>();
            public Gun(IMyUserControllableGun g)
            {
                gun = g;
                gunType = gun.BlockDefinition.SubtypeId;
                inv = g.GetInventory();
                List<MyItemType> ammotypes = new List<MyItemType>();
                inv.GetAcceptedItems(ammotypes);
                var ammoTypesCount = 0;
                MultiAmmoGuns newMultiAmmoGun = null;
                foreach (var a in ammotypes) if (a.TypeId.EndsWith(IG_Ammo) && a.SubtypeId != "Energy") ammoTypesCount++;
                if ((ammoTypesCount > 1) && !(gun is IMyLargeInteriorTurret))
                {
                    newMultiAmmoGun = getNewMultiAmmoGun(this);
                }
                var multiAmmo = (ammoTypesCount > 1) && !(gun is IMyLargeInteriorTurret) ? true : false;
                foreach (var a in ammotypes)
                {
                    if (a.TypeId.EndsWith(IG_Ammo) && a.SubtypeId != "Energy")
                    {
                        var mtype = IG_Ammo + ' ' + a.SubtypeId;
                        var newadef = getAmmoDefs(mtype);
                        newadef.guns.Add(this);
                        ammoTypesDefinition.Add(newadef);
                        var amax = (int)((float)inv.MaxVolume / a.GetItemInfo().Volume);
                        newadef.maxOfVolume += amax;
                        ammomax.Add(mtype, amax);
                        if (newMultiAmmoGun != null) newMultiAmmoGun.addAmmoDef(newadef);
                    }
                }
                storageinvs.Add(this);
            }
            public override bool checkItems()
            {
                if (gun is IMyLargeInteriorTurret) return false;
                // zum testen..................
                if (curAmmo == "") return false;
                int aamount = (int)(ammomax[curAmmo] * ammoDefs[curAmmo].ratio);
                if (aamount < 1) aamount = 1;
                if (items.Count == 0) items.Add(curAmmo, aamount);
                else if (!items.ContainsKey(curAmmo))
                {
                    items.Clear();
                    items.Add(curAmmo, aamount);
                }
                else items[curAmmo] = aamount;
                return true;
            }
            public void Refresh()
            {
                AddToInventory(inv);
                var p = gun.GetProperty(X_UseConveyor);
                if (p != null && gun.GetValue<bool>(X_UseConveyor)) gun.ApplyAction(X_UseConveyor);
                curAmmo = GetAmmoPrio();
            }
            string GetAmmoPrio()
            {
                var cura = "";
                var curap = 0;
                foreach (var a in ammomax)
                {
                    var prio = getAmmoDefs(a.Key).GetAmmoPriority(gunType);
                    if (prio > curap && inventar.ContainsKey(a.Key) && inventar[a.Key] > 0)
                    {
                        cura = a.Key;
                        curap = prio;
                    }
                }
                return cura;
            }
            public void Remove()
            {
                var keyList = ammoDefs.Keys.ToArray();
                for (int i = ammoDefs.Count - 1; i >= 0; i--)
                {
                    if (ammoDefs[keyList[i]].guns.Contains(this))
                    {
                        ammoDefs[keyList[i]].guns.Remove(this);
                        if (ammoDefs[keyList[i]].guns.Count == 0) ammoDefs.Remove(keyList[i]);
                        else ammoDefs[keyList[i]].maxOfVolume -= ammomax[keyList[i]];
                        break;
                    }
                }
                if (storageinvs.Contains(this)) storageinvs.Remove(this);
                var p = gun.GetProperty(X_UseConveyor);
                if (p != null && !gun.GetValue<bool>(X_UseConveyor)) gun.ApplyAction(X_UseConveyor);
            }
        }
        static List<Refinery> oefen = new List<Refinery>();
        //Ofen
        public class Refinery
        {
            static public string priobt = "";
            static public int cn = 0;
            public static Dictionary<string, List<RefineryBlueprint>> refineryTypesAcceptedBlueprintsList = new Dictionary<string, List<RefineryBlueprint>>();
            public static void RemoveUnusedRefinerytypeBlueprintLists()
            {
                foreach (var a in refineryTypesAcceptedBlueprintsList.Keys.ToArray())
                    if (refineryTypesAcceptedBlueprintsList.ContainsKey(a) && !priobt.Contains("@" + a))
                        refineryTypesAcceptedBlueprintsList.Remove(a);
            }
            public enum RefineryRefreshType { Unknow, VanillaRefinery, WaterRecyclingSystem, HydroponicsFarm, Reprocessor, Incinerator, }
            IMyInventory InputInventory, OutputInventory;
            public Dictionary<string, float> InputInventoryItems = new Dictionary<string, float>();
            public RefineryTypeDefinitions typeid;
            public Parameter pms = new Parameter();
            public IMyRefinery RefineryBlock = null;
            public List<RefineryBlueprint> AcceptedBlueprints = null;
            public string BlockSubType = "";
            public int fertig;
            RefineryBlueprint CurrentWorkBluePrint = null;
            RefineryBlueprint NextWorkBluePrint = null;
            float CurrentWorkOreAmount = 0;
            float NexWorkOreAmount = 0;
            public class RefineryTypeDefinitions
            {
                string TypeIDName;
                bool ComplettName;
                RefineryRefreshType TypeID;
                String AlternativName;
                public RefineryTypeDefinitions()
                {
                    TypeIDName = "";
                    ComplettName = true;
                    TypeID = RefineryRefreshType.Unknow;
                    AlternativName = RefineryRefreshType.Unknow.ToString();
                }
                public RefineryTypeDefinitions(string iTypeName, RefineryRefreshType iTypeID, string iAlternativName = "")
                {
                    TypeIDName = iTypeName;
                    ComplettName = true;
                    TypeID = iTypeID;
                    AlternativName = iAlternativName;
                }
                public RefineryTypeDefinitions(bool iComplettName, string iTypeName, RefineryRefreshType iTypeID, string iAlternativName = "")
                {
                    TypeIDName = iTypeName;
                    ComplettName = iComplettName;
                    TypeID = iTypeID;
                    AlternativName = iAlternativName;
                }
                public RefineryRefreshType GetTypeID() { return TypeID; }
                public string GetAlternativOrDefaultName() { return AlternativName == "" ? TypeIDName : AlternativName; }
                public bool IsVanillaManagment() { return TypeID == RefineryRefreshType.VanillaRefinery; }
                public bool IsUnknowType() { return TypeID == RefineryRefreshType.Unknow; }
                public bool CompareTypeName(string compareString)
                {
                    if (TypeID == RefineryRefreshType.Unknow)
                    {
                        AlternativName = compareString;
                        return true;
                    }
                    if (ComplettName && TypeIDName == compareString) return true;
                    else if (!ComplettName && TypeIDName.StartsWith(compareString)) return true;
                    return false;
                }
            }
            List<RefineryTypeDefinitions> TypeDefinitions = new List<RefineryTypeDefinitions>
            {
                new RefineryTypeDefinitions( false, "WRS", RefineryRefreshType.WaterRecyclingSystem, "Water Recycling System" ),
                new RefineryTypeDefinitions( "Blast Furnace", RefineryRefreshType.VanillaRefinery, "Basic Refinery"),
                new RefineryTypeDefinitions( "LargeRefineryIndustrial", RefineryRefreshType.VanillaRefinery, "Large Industrial Refinery"),
                new RefineryTypeDefinitions( "LargeRefinery", RefineryRefreshType.VanillaRefinery, "Large Refinery"),
                new RefineryTypeDefinitions( "K_HSR_Refinery_A", RefineryRefreshType.VanillaRefinery, "HSR Refinery A"),
                new RefineryTypeDefinitions( false, "Hydroponics", RefineryRefreshType.HydroponicsFarm, "Hydroponics Farm"),
                new RefineryTypeDefinitions( "RockCrusher", RefineryRefreshType.VanillaRefinery),
                new RefineryTypeDefinitions( "OrePurifier", RefineryRefreshType.VanillaRefinery),
                new RefineryTypeDefinitions( "ChemicalPlant", RefineryRefreshType.VanillaRefinery),
                new RefineryTypeDefinitions( "Centrifuge", RefineryRefreshType.VanillaRefinery),
                new RefineryTypeDefinitions( "Incinerator", RefineryRefreshType.Incinerator),
                new RefineryTypeDefinitions( "BitumenExtractor", RefineryRefreshType.VanillaRefinery),
                new RefineryTypeDefinitions( "Reprocessor", RefineryRefreshType.Reprocessor),
                new RefineryTypeDefinitions( "OilCracker", RefineryRefreshType.VanillaRefinery),
                new RefineryTypeDefinitions( "DeuteriumProcessor", RefineryRefreshType.VanillaRefinery, "Deuterium Refinery"),
                new RefineryTypeDefinitions(), // LastListItem    
            };
            public Refinery(IMyRefinery refinery)
            {
                RefineryBlock = refinery;
                InputInventory = refinery.GetInventory(0);
                OutputInventory = refinery.GetInventory(1);
                BlockSubType = refinery.BlockDefinition.SubtypeId;
                typeid = TypeDefinitions.Find(t => t.CompareTypeName(BlockSubType));
                BlockSubType = typeid.GetAlternativOrDefaultName();
                AcceptedBlueprints = refineryBlueprints.FindAll(b => RefineryBlock.CanUseBlueprint(b.Definition_id));
                GetScrapBluePrints();
            }
            void GetScrapBluePrints()
            {
                var acceptedItems = new List<MyItemType>();
                InputInventory.GetAcceptedItems(acceptedItems, i => i.SubtypeId.ToLower().Contains("scrap") && !RefineryBlueprint.IsKnowScrapType(i));
                foreach (var inventoryItem in acceptedItems)
                {
                    var scrapBlueprint = RefineryBlueprint.GetRefineryBlueprintByItemtypeOrCreateNew(inventoryItem);
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
                        case RefineryRefreshType.WaterRecyclingSystem:
                            if (bprints.ContainsKey(Ingot.WaterFood) && !bprints[Ingot.WaterFood].IfMax()) WaterRecyclingSystemManager();
                            else if (always_recycle_greywater && inventar.ContainsKey(Ingot.GreyWater) && inventar[Ingot.GreyWater] > 0) WaterRecyclingSystemManager(true);
                            else ClearInputInventoryIfControledByPIM();
                            break;
                        case RefineryRefreshType.HydroponicsFarm:
                            if (bprints.ContainsKey(Ingot.SubFresh) && !bprints[Ingot.SubFresh].IfMax()) HydrophonicsManager();
                            else ClearInputInventoryIfControledByPIM();
                            break;
                        case RefineryRefreshType.Reprocessor:
                            if (bprints.ContainsKey(BluePrintID_SpentFuelReprocessing) && !bprints[BluePrintID_SpentFuelReprocessing].IfMax()) ReprocessorManager();
                            else ClearInputInventoryIfControledByPIM();
                            break;
                        case RefineryRefreshType.VanillaRefinery:
                            VanillaRefineryManager();
                            if (fertig > 80 || IfForceManagerExecuting()) OfenFuellen();
                            OreSort();
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
                var type = typeid.GetTypeID();
                if (type == RefineryRefreshType.Incinerator)
                {
                    setWarning(Warning.ID.Incinerator);
                    return;
                }
                if (typeid.IsUnknowType())
                {
                    setWarning(Warning.ID.RefineryNotSupportet, pms.Name);
                    return;
                }
                if (typeid.IsVanillaManagment())
                {
                    if (!priobt.Contains("@" + BlockSubType)) priobt += "@" + BlockSubType;
                    if (!ingotprio.ContainsKey(BlockSubType)) ingotprio.Add(BlockSubType, new List<IPrio>());
                    if (!refineryTypesAcceptedBlueprintsList.ContainsKey(BlockSubType)) refineryTypesAcceptedBlueprintsList.Add(BlockSubType, AcceptedBlueprints);
                }
                GetWorkItems();
                if (RefineryBlock.IsFunctional)
                {
                    cn++;
                    RefineryBlock.UseConveyorSystem = false;
                    if (InputInventory.ItemCount == 0)
                    {
                        RefineryBlock.Enabled = (refinerys_off && !pms.isPM("Nooff")) ? false : true;
                    }
                    else RefineryBlock.Enabled = true;
                    ClearInventory(OutputInventory);
                    fertig = 100 - (int)((InputInventory.CurrentVolume.RawValue * 100) / InputInventory.MaxVolume.RawValue);
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
                    case RefineryRefreshType.HydroponicsFarm:
                        CalculateRefineryAmount(Ingot.SubFresh);
                        break;
                    case RefineryRefreshType.WaterRecyclingSystem:
                        CalculateRefineryAmount(Ingot.WaterFood);
                        break;
                    case RefineryRefreshType.Reprocessor:
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
                bool gefuellt = false;
                RefineryBlueprint newworkBP = null;
                List<IPrio> ip = ingotprio[BlockSubType];
                for (int i = 0; i < ip.Count; i++)
                {
                    IPrio p = ip[i];
                    if (!Accept(p.refineryBP) || p.prio == 0 || !inventar.ContainsKey(p.refineryBP.InputID)) continue;
                    newworkBP = p.refineryBP;
                    if (fertig < 50) ClearInputInventoryIfControledByPIM();
                    var types = newworkBP.InputID.Split(' ');
                    if (inventar.ContainsKey(newworkBP.InputID)) gefuellt = SendItemByTypeAndSubtype("MyObjectBuilder_" + types[0], types[1], inventar[newworkBP.InputID], RefineryBlock.GetInventory(0));
                    if (gefuellt) break;
                    else if (Erzklau(newworkBP)) gefuellt = true;
                }
                if (gefuellt) SetIngotPrio(ip, newworkBP, cn);
            }
            bool Erzklau(RefineryBlueprint blueprint)
            {
                var oamount = 0f;
                if (blueprint == CurrentWorkBluePrint) oamount = CurrentWorkOreAmount;
                else if (blueprint == NextWorkBluePrint) oamount = NexWorkOreAmount;
                if (RefineryBlock.GetInventory(0).CurrentVolume.RawValue < 100)
                {
                    foreach (Refinery o in oefen)
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
            void OreSort()
            {
                if (!ingotprio.ContainsKey(BlockSubType)) return;
                var rl = new List<MyInventoryItem>();
                var inv = RefineryBlock.GetInventory(0);
                inv.GetItems(rl);
                if (rl.Count > 1)
                {
                    var scrap = -1;
                    for (int i = 0; i < rl.Count; i++)
                    {
                        var typeStr = rl[i].Type.SubtypeId.ToLower();
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
                        var ostrID1 = GetPIMItemID(rl[0].Type);
                        var ostrID2 = GetPIMItemID(rl[1].Type);
                        foreach (IPrio p in ingotprio[BlockSubType])
                        {
                            if (ostrID1 == p.refineryBP.InputID) p1 = p.initp;
                            else if (ostrID2 == p.refineryBP.InputID) p2 = p.initp;
                        }
                        if (p1 < p2)
                        {
                            inv.TransferItemTo(inv, 0, 1, true, rl[0].Amount);
                        }
                        if (rl.Count == 3)
                        {
                            clearItemByType(inv, GetPIMItemID(rl[2].Type), rl[2]);
                        }
                    }
                    else
                    {
                        inv.TransferItemTo(inv, scrap, 0, true, rl[scrap].Amount);
                    }
                }
            }
            public bool Accept(RefineryBlueprint ore)
            {
                if (RefineryBlock.GetInventory(0).IsFull) return false;
                return AcceptedBlueprints.Contains(ore);
            }
            enum RefineryManagerState { NONE, CLEAR_INPUT, SWAP_ORES,};
            RefineryManagerState RMS = RefineryManagerState.NONE;
            void VanillaRefineryManager()
            {
                if (!ingotprio.ContainsKey(BlockSubType)) return;
                RMS = RefineryManagerState.NONE;
                var CurWBP_IPrio = ingotprio[BlockSubType].Find(ip => ip.refineryBP == CurrentWorkBluePrint);
                var NextWBP_IPrio = ingotprio[BlockSubType].Find(ip => ip.refineryBP == NextWorkBluePrint);
                //debugString += pms.Name + "\n";
                //if (CurWBP_IPrio != null) debugString += "CurWBP_IPrio: " + CurWBP_IPrio.prio;
                //if (NextWBP_IPrio != null) debugString += "NetxWBP_IPrio: " + NextWBP_IPrio.prio;
                //debugString += "\n";
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
                if (ipl.Count > 0 && null != (p = IPrio.getPrio(ipl, bp)))
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
            public static IPrio getPrio(List<IPrio> ingotprioList, RefineryBlueprint bp, bool newip = false)
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

        //PM
        public class Parameter
        {
            public Dictionary<string, string> pml = new Dictionary<string, string>();
            string bcn = "";
            public string Name = "";
            bool smscontrol = false;
            public bool ParseArgs(string newa, bool cACd = false)
            {
                if (newa == bcn) return smscontrol;
                else bcn = newa;
                pml.Clear();
                var args = bcn.ToLower();
                if (args.Contains("(sms"))
                {
                    Name = bcn.Substring(0, args.IndexOf("(sms"));
                    foreach (var y in args.Split('('))
                    {
                        if (y.Contains("sms") && y.Contains(")"))
                        {
                            foreach (var s in ToArgStr(y).Split(',', ')'))
                            {
                                if (s != "" && s != "Sms")
                                {
                                    var x = s.IndexOf(':');
                                    if (x > -1) addPM(s.Substring(0, x), s.Substring(x + 1));
                                    else addPM(s);
                                }
                            }
                            if (cACd && smscontrol == false) changeAutoCraftingSettings = true;
                            smscontrol = true;
                            return true;
                        }
                    }
                }
                Name = newa.Trim();
                if (cACd && smscontrol == true) changeAutoCraftingSettings = true;
                smscontrol = false;
                return false;
            }
            public void addPM(string key, string value = "") { if (!isPM(key)) pml.Add(key, value); }
            public bool isPM(string tag) { return pml.ContainsKey(tag); }
            public bool Control() { return smscontrol; }
        }
        //ST
        public class StopWatch
        {
            DateTime ls = DateTime.Now;
            int sec;
            public StopWatch(int isec = 5) { sec = isec; }
            public bool IfTimeSpanReady(bool rs = true)
            {
                if (sec == 0) return false;
                if ((DateTime.Now - ls).TotalSeconds > sec)
                {
                    if (rs) ls = DateTime.Now;
                    return true;
                }
                return false;
            }
        }
        static Dictionary<string, List<IPrio>> ingotprio = new Dictionary<string, List<IPrio>>();

        static List<RefineryBlueprint> refineryBlueprints = new List<RefineryBlueprint>();
        static string GetTimeStringFromHours(double h)
        {
            if (h < 1)
            {
                if ((h * 60) > 1) return Math.Round(h * 60, 0) + " min.";
                else return Math.Round(h * 60 * 60, 0) + " s";
            }
            if (h < 24)
            {
                return Math.Round(h, 1) + " h";
            }
            double tage = Math.Round(h / 24, 1);
            if (tage > 365)
            {
                return Math.Round(tage / 365, 1) + " years";
            }
            if (tage < 1.1) return tage + " day";
            else return tage + " days";
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
        void InitRefineryBlueprints()
        {
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
        void CalcIngotPrio()
        {
            foreach (var pl in ingotprio.Values) foreach (IPrio ip in pl) ip.setPrio(0);
            foreach (var refSubType in Refinery.refineryTypesAcceptedBlueprintsList.Keys)
            {
                foreach (var refBluePrint in Refinery.refineryTypesAcceptedBlueprintsList[refSubType])
                {
                    var inputOre = refBluePrint.InputID;
                    if (inventar.ContainsKey(inputOre) && inventar[inputOre] > 0)
                    {
                        if (refBluePrint.IsScrap) addPrio(refSubType, refBluePrint, 9999);
                        else
                        {
                            var oreamount = inventar[refBluePrint.InputID];
                            var ingotamount = inventar[refBluePrint.OutputID];
                            if (ingotamount == 0) addPrio(refSubType, refBluePrint, 200);
                            else if (ingotamount < 500) addPrio(refSubType, refBluePrint, 150);
                            else if (ingotamount < oreamount) addPrio(refSubType, refBluePrint, 100 - (int)(ingotamount / (oreamount / 97.0f)));
                            else addPrio(refSubType, refBluePrint, 1);
                        }
                    }
                }
            }
            LoadOrePrioDefs();
        }
        static Dictionary<string, Dictionary<RefineryBlueprint, int>> orePrioConfig = new Dictionary<string, Dictionary<RefineryBlueprint, int>>();
        const string OrePrioDefString = "(sms,oreprio)";
        const string ResourcenOverview = "(sms,refining)";
        const string Autocrafting = "(sms,autocrafting)";
        const string AmmoPrioDefinition = "(sms,ammoprio)";
        void LoadOrePrioDefs()
        {
            var lcds = new List<IMyTextPanel>();
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(lcds, block => block.CustomName.Contains(OrePrioDefString));
            if (lcds.Count > 0) // ToDo: mehrere LCDs, einlesen und ausgeben trennen
            {
                Refinery.RemoveUnusedRefinerytypeBlueprintLists();
                Dictionary<RefineryBlueprint, int> curPrioList = null;
                Dictionary<IMyTextPanel, string> filterStrings = new Dictionary<IMyTextPanel, string>();
                foreach (var lcd in lcds)
                {
                    var curRefineryType = "";
                    var pstrs = lcd.GetText().Split('\n');
                    // prio laden
                    foreach (var s in pstrs)
                    {
                        var line = s.Split(':', '=', '|');
                        for (int i = 0; i < line.Count(); i++) { line[i] = line[i].Trim(' ', '\u00AD'); }
                        if (line.Length == 0 || line[0].Length == 0 || line[0][0] == '/') continue;
                        if (line.Length >= 5 && line[3].ToLower().StartsWith("oreprio") && (curPrioList != null))
                        {
                            var blueprint = refineryBlueprints.Find(b => b.Name == line[4]);
                            if (blueprint != null)
                            {
                                int prio = -1;
                                if (!int.TryParse(line[1], out prio)) prio = -1;
                                if (!curPrioList.ContainsKey(blueprint)) curPrioList.Add(blueprint, -1);
                                curPrioList[blueprint] = (prio < 0 ? -1 : (prio > 10000 ? 10000 : prio));
                                if (prio >= 0)
                                {
                                    IPrio ni = IPrio.getPrio(ingotprio[curRefineryType], blueprint);
                                    if (ni != null && ni.prio > 0) ni.setPrio(prio);
                                }
                            }
                        }
                        else if (line.Length >= 2 && line[0].ToLower().StartsWith("refinerytype"))
                        {
                            curPrioList = null;
                            curRefineryType = "";
                            if ((line[1] != "") && Refinery.refineryTypesAcceptedBlueprintsList.ContainsKey(line[1]))
                            {
                                if (orePrioConfig.ContainsKey(line[1]))
                                {
                                    curPrioList = orePrioConfig[line[1]];
                                }
                                else
                                {
                                    curPrioList = new Dictionary<RefineryBlueprint, int>();
                                    orePrioConfig.Add(line[1], curPrioList);
                                }
                                curRefineryType = line[1];
                            }
                        }
                        else if (line.Length > 1 && line[0] == "Filter")
                        {
                            if (!filterStrings.ContainsKey(lcd))
                            {
                                filterStrings.Add(lcd, line[1]);
                            }
                        }
                    }
                }
                foreach (var lcd in lcds)
                {
                    // prio wieder schreiben
                    var fString = "*";
                    if (filterStrings.ContainsKey(lcd))
                    {
                        filter.InitFilter(filterStrings[lcd]);
                        fString = filterStrings[lcd];
                    }
                    else
                    {
                        filter.SetFilterToAll();
                    }
                    var priolist = "/ Orepriorityconfig:\n/ only refinerytypes with '(sms)' in the name are displayed.\n\n/ set the 'value' between 1 and 10000\n/ if value = 0 then the ore will be ignored\n/ if value empty then prio will be calculated by PIM.\n/ any type of scrap is always refined first\n\n/ Refinerytypefilter, separated by comma, '*' for all\n Filter: " + fString + "\n\n/                Resource                |    Value    |        Current\n";
                    foreach (var key in Refinery.refineryTypesAcceptedBlueprintsList.Keys)
                    {
                        if (!orePrioConfig.ContainsKey(key)) orePrioConfig.Add(key, new Dictionary<RefineryBlueprint, int>());
                        var blueprintList = Refinery.refineryTypesAcceptedBlueprintsList[key].FindAll(o => !o.IsScrap);
                        if (blueprintList.Count < 2 || !filter.ifFilter(key)) continue;
                        priolist += linepur + "\n RefineryType: " + key + line;
                        var curIngotPrioList = ingotprio[key];
                        blueprintList.Sort((x, y) => x.InputIDName.CompareTo(y.InputIDName));
                        foreach (var bp in blueprintList)
                        {
                            var priostr = "-1";
                            var curPrio = IPrio.getPrio(curIngotPrioList, bp);
                            if (orePrioConfig[key].ContainsKey(bp)) priostr = orePrioConfig[key][bp].ToString();
                            else orePrioConfig[key].Add(bp, -1);
                            priolist += " "
                                + (getDisplayBoxString(bp.InputIDName, 61, true))
                                + " | "
                                + getDisplayBoxString((priostr == "-1" ? "  |  " : priostr + "  |  "), 25)
                                + ((curPrio != null && curPrio.initp > 0) ? getDisplayBoxString(curPrio.initp.ToString(), 25) + "  " : "")
                                + bigSpaces
                                + "|OrePrio:"
                                + bp.Name
                                + "|\n";
                        }
                    }
                    lcd.Alignment = TextAlignment.LEFT;
                    lcd.ContentType = ContentType.TEXT_AND_IMAGE;
                    lcd.WriteText(priolist);
                }

            }
        }
        void RenderAmmoPrioLCDS()
        {
            var lcds = new List<IMyTextPanel>();
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(lcds, block => block.CustomName.Contains(AmmoPrioDefinition));
            if (lcds.Count != 0)
            {
                var lcd = lcds[0];
                string[] pstr = lcd.GetText().Split('\n');
                MultiAmmoGuns currentMultiGun = null;
                foreach (var s in pstr)
                {
                    var line = s.Split(':', '=', '|');
                    for (int i = 0; i < line.Count(); i++) { line[i] = line[i].Trim(' ', '\u00AD'); }
                    if (line.Length == 0 || line[0].Length == 0 || line[0][0] == '/') continue;
                    if (line.Length >= 3 && line[0] == "GunType")
                    {
                        currentMultiGun = multiAmmoGuns.GetValueOrDefault(line[2], null);
                    }
                    else if (line.Length >= 3 && currentMultiGun != null)
                    {
                        var adef = currentMultiGun.GetAmmoDefs(line[2]);
                        if (adef != null)
                        {
                            adef.SetAmmoPriority(currentMultiGun.MultiAmmoGuntype, int.Parse(line[0]));
                        }
                    }
                }
                // Prio schreiben...
                var ammoprioString = "/ Ammopriodefinitions:\n/ the prio only affects weapons that can use\n/ different ammunition types. this determines\n/ which one is loaded into the inventory first.\n/ 0 means that the ammunition is not used\n";
                var headerString = "\n" + getDisplayBoxString("Priority", 25) + " | Ammotyp\n";
                foreach (var mAmmoGuns in multiAmmoGuns)
                {
                    ammoprioString += linepur + "\nGunType: " + mAmmoGuns.Value.DisplayName + bigSpaces + "|" + mAmmoGuns.Value.MultiAmmoGuntype + headerString;
                    AmmoDefs.SetCurrentSortGuntype(mAmmoGuns.Key);
                    mAmmoGuns.Value.ammoDefs.Sort();
                    foreach (var aDef in mAmmoGuns.Value.ammoDefs)
                    {
                        ammoprioString += getDisplayBoxStringDisplayNull(aDef.GetAmmoPriority(mAmmoGuns.Key), 25) + " | " + getDisplayBoxString(aDef.GetAmmoBluePrintAutocraftingName(), 60, true) + bigSpaces + " | " + aDef.type + "\n";
                    }
                }
                lcd.Alignment = TextAlignment.LEFT;
                lcd.ContentType = ContentType.TEXT_AND_IMAGE;
                lcd.WriteText(ammoprioString);
            }
        }
        void RenderResourceProccesingLCD()
        {
            var lcds = new List<IMyTextPanel>();
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(lcds, block => block.CustomName.Contains(ResourcenOverview));
            if (lcds.Count != 0)
            {
                RefineryBlueprint.FillInputOutputAmountAndETA(); // ToDo: nicht bei jedem cycle berechnen!!!
                var lcd = lcds[0];
                var oresList = "                               Refiningprogress:\n" + line2lineonly + "\n";
                foreach (var bluePrint in refineryBlueprints) // ToDo: Stone to Gravel doppelt!
                {
                    if (bluePrint.RefineryCount > 0)
                    {
                        oresList
                            += getDisplayBoxString(bluePrint.Name, 70, true)
                            + " | "
                            + getDisplayBoxString(bluePrint.InputIDName, bluePrint.InputAmount, 70)
                            + "\n"
                            + getDisplayBoxString(bluePrint.RefineryCount.ToString() + " Refinerys.", 30, true)
                            + getDisplayBoxString("-> " + bluePrint.etaString, 40, true)
                            + " | "
                            + getDisplayBoxString(bluePrint.OutputIDName, bluePrint.OutputAmount, 70)
                            + "\n"
                            + line2lineonly
                            + "\n";

                    }
                }
                lcd.Alignment = TextAlignment.LEFT;
                lcd.ContentType = ContentType.TEXT_AND_IMAGE;
                lcd.WriteText(oresList);
            }
        }
        static Dictionary<string, string> ResourcesNameCastList = new Dictionary<string, string>
            {
                { Ore.Stone, Resources.RStone },
                { Ingot.Magnesium, Ingot.Magnesiumpowder },
                { Ingot.Stone, "Gravel" },
                { Ingot.DeuteriumContainer, Resources.RDeuterium },
                { Ore.Ice, "Ice"},
                { Ingot.Carbon, Resources.RCarbon },
            };
        static Dictionary<string, string> ResourcesNameCastListIOMod = new Dictionary<string, string>
            {
                { Ore.Coal, Resources.RCoal },
                { Ore.Bauxite, Resources.RBauxite },
                { Ore.Niter, Resources.RNiter },
                { Ingot.Lithium, Resources.RLithium + " Paste" },
                { Ingot.Sulfur, Resources.RSulfur },
                { Ingot.Niter, Resources.RPotassium + " Nitrate" },
                { Ore.Magnesium, "Crushed Niter" },
                { Ingot.Magnesium, Ingot.Gunpowder},
            };
        static string CastResourceName(string name)
        {
            if (usedMods[M_IndustrialOverhaulMod] && ResourcesNameCastListIOMod.ContainsKey(name)) return ResourcesNameCastListIOMod[name];
            if (ResourcesNameCastList.ContainsKey(name)) return ResourcesNameCastList[name];
            if (name.StartsWith("Ore Crushed")) return "Crushed " + name.Substring(11);
            if (name.StartsWith("Ore Purified")) return "Purified " + name.Substring(12);
            return name;
        }
        const string line = "\n" + linepur + "\n";
        const string linepur = "/-------------------------------------------------------------------------";
        const string line2 = "\n" + line2pur + "\n";
        const string line2pur = "/" + line2lineonly;
        const string line2lineonly = "-------------------------------------------------------------------------------------------";
        void addPrio(string reftype, RefineryBlueprint type, int prio)
        {
            if (prio == 0) prio = 1;
            if(ingotprio.ContainsKey(reftype))
            {
                IPrio.getPrio(ingotprio[reftype], type, true).setPrio(prio);
            }
        }
        static int getIntegerWithPräfix(string cstr)
        {
            if (cstr == "") return 0;
            var cstrlist = cstr.Split(' ');
            if (cstrlist.Count() == 0) return 0;
            float wr;
            if (!float.TryParse(cstrlist[0], out wr)) return 0;
            if (cstrlist.Count() == 2)
            {
                if (cstrlist[1] == "k") wr *= 1000;
                else if (cstrlist[1] == "M") wr *= 1000000;
            }
            return (int)wr;
        }
        static int getInteger(string cstr) { float wr; float.TryParse(cstr.Trim().Split('.')[0], out wr); return (int)wr; }
        static string ToArgStr(string qstr)
        {
            var zstr = "";
            var tc = true;
            foreach (var ctr in qstr)
            {
                if (tc)
                {
                    tc = false;
                    zstr += Char.ToUpper(ctr);
                    continue;
                }
                if (ctr == ' ' | ctr == ',' | ctr == '-' | ctr == '_' | ctr == '&' | ctr == ':')
                {
                    tc = true;
                    zstr += ctr;
                    continue;
                }
                zstr += Char.ToLower(ctr);
            }
            return zstr;
        }
        int r = 0;
        int rc = 1;
        int mr = 7;
        string getRunningSign()
        {
            var w = "|";
            r += rc;
            if (r < 0)
            {
                r = 1;
                rc = 1;
            }
            else if (r > mr)
            {
                r = mr - 1;
                rc = -1;
            }
            for (int i = 0; i <= mr; i++) w += i == r ? (rc < 0 ? '<' : '>') : ' ';
            return w + "| ";
        }

        string[] modInitList =
        {
            M_DailyNeedsSurvival,
            M_AzimuthThruster,
            M_SG_Gates,
            M_SG_Ores,
            M_PaintGun,
            M_DeuteriumReactor,
            M_Shield,
            M_RailGun,
            M_HomingWeaponry,
            M_IndustrialOverhaulMod,
            M_IndustrialOverhaulLLMod,
            M_IndustrialOverhaulWaterMod,
            M_EatDrinkSleep,
            M_PlantCook,
            M_AryxEpsteinDrive,
            M_NorthWindWeapons,
            M_HSR,
        };
        void loadItemDefinitionen()
        {

            foreach (var m in modInitList) usedMods.Add(m, false);

            loadConfig();

            if (!usedMods[M_IndustrialOverhaulMod])
            {
                // blueprint vanilla
                // components
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
                C("Canvas");
                C("ExplosivesComponent");
                C("SolarCell");
                C("PowerCell");
                C("Superconductor");

                D("Datapad");

                // ammo
                A("NATO_25x184mmMagazine");
                A("Missile200mm");
                A("SemiAutoPistolMagazine");
                A("FullAutoPistolMagazine");
                A("ElitePistolMagazine");
                A("AutomaticRifleGun_Mag_20rd");
                A("RapidFireAutomaticRifleGun_Mag_50rd");
                A("PreciseAutomaticRifleGun_Mag_5rd");
                A("UltimateAutomaticRifleGun_Mag_30rd");

                // tools and wapons
                T("Welder");
                T("Welder2");
                T("Welder3");
                T("Welder4");
                T("AngleGrinder");
                T("AngleGrinder2");
                T("AngleGrinder3");
                T("AngleGrinder4");
                T("HandDrill");
                T("HandDrill2");
                T("HandDrill3");
                T("HandDrill4");
                T("AutomaticRifle");
                T("PreciseAutomaticRifle");
                T("RapidFireAutomaticRifle");
                T("UltimateAutomaticRifle");
                T("BasicHandHeldLauncher");
                T("AdvancedHandHeldLauncher");
                T("SemiAutoPistol");
                T("FullAutoPistol");
                T("EliteAutoPistol");

                // bottles
                H("HydrogenBottle");
                O("OxygenBottle");
            }

            // ammo
            A("AutocannonClip");
            A("LargeCalibreAmmo");
            A("MediumCalibreAmmo");
            A("LargeRailgunAmmo");
            A("SmallRailgunAmmo");

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
        string Tag2Ingame(string ststr)
        {
            if (ststr.Contains("Dock")) return "";
            switch (ststr)
            {
                case "Deuterium": return Ingot.DeuteriumContainer;
                case "Ice": return Ore.Ice;
                case "Water": return Ingot.WaterFood;
                case "Greywater": return Ingot.GreyWater;
                case "Organic": return Ore.Organic;
                case "Stone": return Ore.Stone;
                case "Gravel": return Ingot.Stone;
                case "Tools": return IG_Tools;
                case "Kits": return IG_Kits;
                case "Cash": return IG_Cash;
                case "Datapads": return IG_Datas;
                case "H-Bottles": return IG_HBottles;
                case "O-Bottles": return IG_OBottles;
                case "Ammo": return IG_Ammo;
                case "Steelplate": return IG_Com + "SteelPlate";
                case "Metalgrid": return IG_Com + "MetalGrid";
                case "Interiorplate": return IG_Com + "InteriorPlate";
                case "Smalltube": return IG_Com + "SmallTube";
                case "Largetube": return IG_Com + "LargeTube";
                case "Glass": return IG_Com + "BulletproofGlass";
                case "Gravity": return IG_Com + "GravityGenerator";
                case "Radio": return IG_Com + "RadioCommunication";
                case "Solar": return IG_Com + "SolarCell";
                case "Power": return IG_Com + "PowerCell";
                case "Zonechip": return IG_Com + "ZoneChip";
                case "Reactor":
                case "Thrust":
                case "Medical":
                case "Detector":
                case "Explosives":
                case "Construction":
                case "Motor":
                case "Display":
                case "Girder":
                case "Computer":
                case "Canvas": return IG_Com + ststr;
                default: return ststr;
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
