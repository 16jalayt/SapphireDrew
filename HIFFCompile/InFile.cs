using Sapphire_Extract_Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace HIFFCompile
{
    //Has been rewritten to this program
    internal static class InFile
    {
        //private static HashSet<string> keywords = new HashSet<string>
        //{ "byte", "int", "long", "RefDep", "RefFlag", "RefOvlStat" };
        //public static string[] keywords = { "byte", "int", "long", "RefDep", "RefFlag", "RefOvlStat", "RefSetFlag", "RefScene" };

        //public static string[] stringKeywords = { "RefAVF", "RefSound", "RefHif", "RefOvlStat" };

        //left keyword, right type
        private static Dictionary<string, string> keywordDict = new Dictionary<string, string>()
        {
            { "int", "short" },
            { "long", "int" },
            { "RefFlag", "int" }
        };

        //Insert placeholder at zero so line numbers match
        public static string[] lines = ["Line 0 Placeholder"];

        public static string[] lineTokens;

        public static int pos = 0;

        public static int tokenPos = 0;

        public static string GetNextLine()
        {
            pos++;

            //ignore comments
            if (lines[pos].Contains("//"))
            {
                lines[pos] = lines[pos].Substring(0, lines[pos].IndexOf("//"));
                //If line has only comment, get next available line
                if (lines[pos]?.Length == 0)
                    GetNextLine();
            }

            //lines[pos] = lines[pos].Trim();
            //lines[pos] = Regex.Replace(lines[pos], _regex.ToString(), " ");

            //Tokenize respecting quotes
            string pattern = "[^\\s\"']+|\"([^\"]*)\"|'([^']*)'";
            lineTokens = Regex.Matches(lines[pos], pattern)
                .OfType<Match>()
                .Select(m => m.Groups[0].Value.Replace("\"", ""))
                .ToArray();

            tokenPos = 0;

            return lines[pos];
        }

        public static string GetLine()
        {
            return lines[pos];
        }

        public static string GetNextToken()
        {
            if (!HasNextToken())
                return "";
            tokenPos++;
            return lineTokens[tokenPos];
        }

        public static bool HasNextToken()
        {
            if (tokenPos >= lineTokens.Length - 1)
                return false;
            return true;
        }

        public static string GetCurrentToken()
        {
            return lineTokens[tokenPos];
        }

        public static void WriteObject(BinaryWriter outStream, string wantedKeyword, string[]? enumType = null)
        {
            GetNextLine();
            string keyword = GetCurrentToken();
            string value = GetNextToken();
            //Has to be number unless explicitly string
            int valueInt = -1;

            //TODO: helper int getEnumValue(string);
            if (enumType != null)
                for (int i = 0; i < enumType.Length; i++)
                {
                    if (enumType[i] == value)
                    {
                        valueInt = i;
                        break;
                    }
                }

            switch (keywordDict[keyword])
            {
                case "short":
                    outStream.Write((short)valueInt);
                    break;

                case "int":
                    outStream.Write((int)valueInt);
                    break;
                //Should only hit if programmer error. Wanted word not valid.
                default:
                    throw new Exception($"\nSyntax error. Unknown keyword. at line '{InFile.pos}'");
            }
        }

        //TODO: convert functions like these to return value instead of ref. Just null check at callsite.
        public static void WriteString(BinaryWriter outStream, string wantedKeyword, int length)
        {
            GetNextLine();
            string keyword = GetCurrentToken();
            string value = GetNextToken();

            if (keyword != wantedKeyword)
                throw new Exception($"Invalid keyword. Must be: '{wantedKeyword}'");

            if (value.Length > length)
                throw new Exception($"String too long at {value.Length} chars. Must be less than: '{length}'");

            value = value.PadRight(length, '\0');
            outStream.Write(Encoding.UTF8.GetBytes(value));
        }

        //Might not need with tokens
        public static void WriteImmediateString(BinaryWriter outStream, string wantedKeyword, int length)
        {
            WriteString(outStream, wantedKeyword, length);
        }

        public static int ParseTF(string operand)
        {
            int inEnum = Array.FindIndex(Enums.tf, x => x.Contains(operand));
            if (inEnum == -1)
            {
                inEnum = Array.FindIndex(Enums.tfCamel, x => x.Contains(operand));
                if (inEnum == -1)
                {
                    inEnum = Array.FindIndex(Enums.tfLower, x => x.Contains(operand));
                    if (inEnum == -1)
                    {
                        Console.WriteLine($"'{operand}' on line {pos + 1} is not True or False.");
                        return -1;
                    }
                }
            }
            return inEnum;
        }
    }
}