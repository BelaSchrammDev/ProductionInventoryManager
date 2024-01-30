using System.Collections.Generic;

namespace IngameScript
{
    partial class Program
    {
        public class Parameter
        {
            public Dictionary<string, string> ParameterList = new Dictionary<string, string>();
            StringBuilderExtended LastCustomName = new StringBuilderExtended(150);
            public StringBuilderExtended Name = new StringBuilderExtended(150);
            bool PIMcontrolled = false;


            public bool ParseArgs(string newCustomName, bool canChangeAutocraftingStatus = false)
            {
                if (LastCustomName.Equal(newCustomName)) return PIMcontrolled;
                LastCustomName.Clear();
                LastCustomName.Append(newCustomName);
                ParameterList.Clear();
                var nameLower = LastCustomName.ToString().ToLower();
                if (nameLower.Contains("(sms"))
                {
                    LastCustomName.Substring(Name, 0, nameLower.IndexOf("(sms") - 1);
                    Name.Trim();
                    foreach (var tag in nameLower.Split('('))
                    {
                        if (tag.Contains("sms") && tag.Contains(")"))
                        {
                            foreach (var s in ToArgStr(tag).Split(',', ')'))
                            {
                                if (s != "" && s != "Sms")
                                {
                                    var x = s.IndexOf(':');
                                    if (x > -1) AddParameter(s.Substring(0, x), s.Substring(x + 1));
                                    else AddParameter(s);
                                }
                            }
                            if (canChangeAutocraftingStatus && PIMcontrolled == false) changeAutoCraftingSettings = true;
                            PIMcontrolled = true;
                            return true;
                        }
                    }
                }
                Name.Clear();
                Name.Append(newCustomName.Trim());
                if (canChangeAutocraftingStatus && PIMcontrolled == true) changeAutoCraftingSettings = true;
                PIMcontrolled = false;
                return false;
            }

            void AddParameter(string key, string value = "") { if (!IsParameter(key)) ParameterList.Add(key, value); }

            public bool IsParameter(string key) { return ParameterList.ContainsKey(key); }

            public bool ControledByPIM() { return PIMcontrolled; }
        }
    }
}
