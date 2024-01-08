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
            public virtual string GetInfoText() { return infotext; }
            public bool IsOver() { if (lebenssekunden == -1) return false; if ((DateTime.Now - burn).TotalSeconds > lebenssekunden) return true; return false; }
            public void SetOver() { lebenssekunden = 0; }
        }
        class Info : View
        {
            public Info(string text, int sec = -1) : base(text, ViewType.INFO, sec) { }
        }
        class Warning : View
        {
            public enum ID { NONE, CARGOUSEHEAVY, CARGOUSEFULL, CargoMissing, Incinerator, RefineryNotSupportet }
            public ID w_ID = ID.NONE;
            public string subType = "";
            public Warning(ID warn_ID, string isubtype = "") : base(ViewType.WARNING) { w_ID = warn_ID; subType = isubtype; }
            public override string GetInfoText()
            {
                var testsb = new StringBuilder(300);
                switch (w_ID)
                {
                    case ID.RefineryNotSupportet: return "refinery '" + subType + "' not supported.";
                    case ID.Incinerator: return "incinerator (IOmod) not supported.";
                    case ID.CargoMissing: return "please define cargo for " + subType + ".";
                    case ID.CARGOUSEHEAVY: return "cargo with " + subType + " is heavy.";
                    case ID.CARGOUSEFULL: return "cargo with " + subType + " is full !!!!!";
                }
                return "";
            }
        }
        class AmmoManagerInfo : View
        {
            public override string GetInfoText()
            {
                if (guns.Count == 0) return "";
                return "  AmmoMan.: controls " + guns.Count + " weapons.";
            }
        }
        class StorageManagerInfo : View
        {
            public override string GetInfoText()
            {
                if (storageCargos.Count == 0) return "";
                return "StorageMan.: controls " + storageCargos.Count + " containers.";
            }
        }
        class RefineryManagerInfo : View
        {
            StringBuilder errString = new StringBuilder(1000);
            public override string GetInfoText()
            {
                errString.Clear();
                foreach (var o in oefen) o.GetRefErrorInfo(errString);
                return errString.Length == 0 ? "" : "RefineryManager:\n" + errString.ToString();
            }
        }
        class AssemblerManagerInfo : View
        {
            StringBuilder errString = new StringBuilder(500);
            public override string GetInfoText()
            {
                errString.Clear();
                foreach (var o in okumas) errString.Append(o.GetAssemblerErrorInfo());
                return errString.Length == 0 ? "" : errString.ToString();
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
        static void clearWarning(Warning.ID warnID, string subtype)
        {
            var w = getWarning(warnID, subtype);
            if (w != null) w.SetOver();
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
                        var s = v.GetInfoText();
                        infostring += s + (s == "" ? "" : "\n");
                        break;
                    case View.ViewType.WARNING: warnungstring += v.GetInfoText() + "\n"; break;
                    case View.ViewType.ERROR: errorstring += v.GetInfoText() + "\n"; break;
                }
            }
            if (warnungstring != "") infostring += "Warning:\n" + warnungstring;
            if (errorstring != "") infostring += "Errors:\n" + errorstring;
            return infostring;
        }
    }
}
