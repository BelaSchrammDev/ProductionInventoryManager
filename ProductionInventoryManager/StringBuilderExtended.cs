using System.Text;

namespace IngameScript
{
    partial class Program
    {
        public class StringBuilderExtended
        {
            readonly StringBuilder sb;

            public StringBuilderExtended(int size) { sb = new StringBuilder(size); }

            public StringBuilder GetSB() { return sb; }

            public string GetString() { return sb.ToString(); }

            public override string ToString() { return sb.ToString(); }


            public void Trim()
            {
                int wsFromBegin = 0;
                int wsFromEnd = 0;
                for (int i = sb.Length - 1; i >= 0; i--)
                {
                    if (char.IsWhiteSpace(sb[i])) wsFromEnd++;
                    else break;
                }
                for (int i = 0; i < sb.Length; i++)
                {
                    if (char.IsWhiteSpace(sb[i])) wsFromBegin++;
                    else break;
                }
                sb.Remove(0, wsFromBegin);
                sb.Remove(sb.Length - wsFromEnd, wsFromEnd);
            }


            public void Substring(StringBuilderExtended targetSBX, int startindex, int lenght = -1)
            {
                Substring(targetSBX.sb, startindex, lenght);
            }


            public void Substring(StringBuilder targetSB, int startindex, int lenght = -1)
            {
                targetSB.Clear();
                if (startindex > sb.Length) return;
                int endindex;
                if (lenght > 0) endindex = startindex + lenght;
                else endindex = sb.Length - 1;
                for (int i = startindex; i <= endindex; i++) targetSB.Append(sb[i]);
            }

            public bool IsEmpty() { return sb.Length == 0; }

            public bool Equal(string text) { if (sb.Length == text.Length && Contains(text)) return true; return false; }

            public void Insert(int pos, string text) { sb.Insert(pos, text); }

            public void Append(StringBuilderExtended sbx) { if (sbx == null) return; sb.Append(sbx.sb); }

            public void Append(string text) { sb.Append(text); }

            public void Append(char c) { sb.Append(c); }

            public void AppendLF(string text) { Append(text + "\n"); }

            public void AppendLF(StringBuilderExtended sbx) { if (sbx == null) return; Append(sbx); Append('\n'); }

            public void AppendLFifNotEmpty(string text) { if (text.Length > 0) AppendLF(text); }

            public void AppendLFifNotEmpty(StringBuilderExtended sbx) { if (sbx != null && !sbx.IsEmpty()) AppendLF(sbx); }

            public void Clear() { sb.Clear(); }


            public void SetText(string text1, string text2 = "", string text3 = "", string text4 = "", string text5 = "")
            {
                Clear();
                Append(text1);
                if (text2 != "") Append(text2);
                if (text3 != "") Append(text3);
                if (text4 != "") Append(text4);
                if (text5 != "") Append(text5);
            }

            public void SetText(StringBuilderExtended sbx) { Clear(); Append(sbx); }

            public void SetText(char c) { Clear(); Append(c); }

            public void ToLower() { for (int i = 0; i < sb.Length; i++) sb[i] = char.ToLower(sb[i]); }

            public bool Contains(string s) { return IndexOf(s) != -1; }


            public int IndexOf(string s)
            {
                int sIndex = 0;
                int contains = 0;
                int findIndex = -1;
                for (int i = 0; sIndex < s.Length && i < sb.Length && i + (s.Length - sIndex) < sb.Length; i++, sIndex += contains)
                {
                    if (sb[i] == s[sIndex])
                    {
                        if (contains == 0) findIndex = i;
                        contains = 1;
                    }
                    else { contains = 0; sIndex = 0; findIndex = -1; }
                }
                if (sIndex < s.Length - 1) findIndex = -1;
                return findIndex;
            }
        }
    }
}
