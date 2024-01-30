using System;
using System.Collections.Generic;

namespace IngameScript
{
    partial class Program
    {

        static List<View> viewList = new List<View>();
        abstract class View
        {
            public const string Color_Red = "[Color=#FFFF0000]", Color_Yellow = "[Color=#FF000000]", Color_End = "[/Color]";
            public enum ViewType { NONE, INFO, WARNING }
            DateTime burn = DateTime.Now;
            public ViewType type = ViewType.NONE;
            int SecondsToLive = -1;
            public StringBuilderExtended InfoString = new StringBuilderExtended(1000);
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
                InfoString.SetText(itext);
                type = itype;
                SecondsToLive = sec;
            }
            public virtual StringBuilderExtended GetInfoText() { return InfoString; }
            public bool IsOver() { if (SecondsToLive == -1) return false; if ((DateTime.Now - burn).TotalSeconds > SecondsToLive) return true; return false; }
            public void SetOver() { SecondsToLive = 0; }
        }


        class Info : View
        {
            public Info(string text, int sec = -1) : base(text, ViewType.INFO, sec) { }
        }


        class Warning : View
        {
            public enum ID { NONE, CARGOUSEHEAVY, CARGOUSEFULL, CARGOMISSING, CARGORECOMMENDED, REFINERYNOTSUPPORTED }
            public ID w_ID = ID.NONE;
            public string subType = "";


            public Warning(ID warn_ID, string isubtype = "") : base(ViewType.WARNING)
            {
                w_ID = warn_ID;
                subType = isubtype;
                SetInfoString(isubtype);
            }


            void SetInfoString(string subType)
            {
                switch (w_ID)
                {
                    case ID.REFINERYNOTSUPPORTED:
                        InfoString.SetText("! refinery '", subType, "' not supported.");
                        break;
                    case ID.CARGORECOMMENDED:
                        InfoString.SetText("   - ", subType, " found. you can define cargo for it with ...(sms,", subType.ToLower(), ")");
                        break;
                    case ID.CARGOMISSING:
                        InfoString.SetText("!! please define cargo for ", subType, ". name container like ...(sms,", subType.ToLower(), ")");
                        break;
                    case ID.CARGOUSEHEAVY:
                        InfoString.SetText("!!! cargo with ", subType, " is heavy.");
                        break;
                    case ID.CARGOUSEFULL:
                        InfoString.SetText("!!!! cargo with ", subType, " is full !!!!!");
                        break;
                }
            }
        }


        class AmmoManagerInfo : View
        {
            public override StringBuilderExtended GetInfoText()
            {
                if (guns.Count == 0) return null;
                InfoString.SetText("AmmonitionManager: ", guns.Count.ToString(), " weapons.");
                return InfoString;
            }
        }


        class StorageManagerInfo : View
        {
            public override StringBuilderExtended GetInfoText()
            {
                if (storageCargos.Count == 0) return null;
                InfoString.SetText("StorageManager: ", storageCargos.Count.ToString(), " containers.");
                return InfoString;
            }
        }


        class RefineryManagerInfo : View
        {
            public override StringBuilderExtended GetInfoText()
            {
                InfoString.Clear();
                foreach (var refinery in RefineryList) refinery.GetErrorInfo(InfoString);
                if (InfoString.IsEmpty()) return null;
                InfoString.Insert(0, "--------------- RefineryManager ---------------\n");
                return InfoString;
            }
        }


        class AssemblerManagerInfo : View
        {
            public override StringBuilderExtended GetInfoText()
            {
                InfoString.Clear();
                foreach (var assembler in AssemblerList) assembler.GetErrorInfo(InfoString);
                if (InfoString.IsEmpty()) return null;
                InfoString.Insert(0, "--------------- AssemblerManager ----------------\n");
                return InfoString;
            }
        }


        static void SetInfo(string warnungstext, int zeit = 10)
        {
            viewList.Add(new Info(warnungstext, zeit));
        }


        static void SetWarningByCondition(bool condition, Warning.ID warnID, string subtype = "")
        {
            if (condition) SetWarning(warnID, subtype);
            else ClearWarning(warnID, subtype);
        }


        static void SetWarning(Warning.ID warnID, string subtype = "")
        {
            if (GetWarning(warnID, subtype) == null) viewList.Add(new Warning(warnID, subtype));
        }


        static Warning GetWarning(Warning.ID warnID, string subtype)
        {
            foreach (var v in viewList)
            {
                if (!(v is Warning)) continue;
                var w = v as Warning;
                if ((w.w_ID == warnID) && w.subType == subtype) return w;
            }
            return null;
        }


        static void ClearWarning(Warning.ID warnID, string subtype)
        {
            var w = GetWarning(warnID, subtype);
            if (w != null) w.SetOver();
        }


        StringBuilderExtended infoString = new StringBuilderExtended(2000);
        StringBuilderExtended warnungstring = new StringBuilderExtended(1000);


        void CalcutateInfos()
        {
            infoString.SetText("\n");
            warnungstring.Clear();
            foreach (var v in viewList)
            {
                switch (v.type)
                {
                    case View.ViewType.INFO: infoString.AppendLFifNotEmpty(v.GetInfoText()); break;
                    case View.ViewType.WARNING: warnungstring.AppendLF(v.GetInfoText()); break;
                }
            }
            if (!warnungstring.IsEmpty())
            {
                infoString.Append("\n--------------------- Hints ---------------------\n");
                infoString.Append(warnungstring);
            }
        }
    }
}
