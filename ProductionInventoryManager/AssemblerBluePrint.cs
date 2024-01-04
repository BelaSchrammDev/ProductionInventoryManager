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
                else AssemblingDeltaAmount = -1;
            }
            public void CalcPriority()
            {
                if (AssemblingDeltaAmount <= 0) ItemPriority = 0;
                else
                {
                    ItemPriority = 100 - (CurrentItemAmount * 100 / MaximumItemAmount);
                }
            }
        }
    }
}
