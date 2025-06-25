using Sapphire_Extract_Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
            { "byte", "byte" },
            { "int", "short" },
            { "long", "int" },
            { "RefFlag", "short" },
            { "RefSetFlag", "short" },
            { "RefScene", "short" },
            { "RefDep", "short" },
            { "RefSound", "short" },
        };

        //Insert placeholder at zero so line numbers match
        public static string[] lines = ["Line 0 Placeholder"];

        public static string[] lineTokens;

        public static int pos = 0;

        public static int tokenPos = 0;

        public static string GetNextLine()
        {
            pos++;

            if (pos >= lines.Length)
                return "";

            //TODO: out of spec block comments
            //ignore comments
            if (lines[pos].Contains("//"))
            {
                lines[pos] = lines[pos].Substring(0, lines[pos].IndexOf("//"));
                lines[pos] = lines[pos].TrimEnd();
                //If line has only comment, get next available line
                if (lines[pos]?.Length == 0)
                    GetNextLine();
            }

            //lines[pos] = lines[pos].Trim();
            //lines[pos] = Regex.Replace(lines[pos], _regex.ToString(), " ");

            //Tokenize respecting quotes
            string pattern = "[^\\s\"',]+|\"([^\"]*)\"|'([^']*)'";
            lineTokens = Regex.Matches(lines[pos], pattern)
                .OfType<Match>()
                .Select(m => m.Groups[0].Value.Replace("\"", ""))
                .ToArray();

            //Wont work with quotes or comments
            //char[] delimiters = new[] { ',', ' ' };
            //lineTokens = lines[pos].Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

            tokenPos = 0;

            //Remove multiple spaces
            lines[pos] = Regex.Replace(lines[pos], @"\s+", " ");
            //Alt replace whitespace with same character respecting tabs... etc
            //Regex.Replace(source, @"(\s)\s+", "$1");

            return lines[pos];
        }

        public static string PeekNextLine()
        {
            if (pos >= lines.Length)
                return "";

            //Remove multiple spaces
            lines[pos + 1] = Regex.Replace(lines[pos + 1], @"\s+", " ");
            return lines[pos + 1];
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
            if (tokenPos >= (lineTokens.Length - 1))
                return false;
            return true;
        }

        public static string GetCurrentToken()
        {
            return lineTokens[tokenPos];
        }

        //TODO: wantedKeyword not used
        public static void WriteObject(BinaryWriter outStream, string wantedKeyword, string[]? enumType = null)
        {
            GetNextLine();
            string keyword = GetCurrentToken();
            string value = GetNextToken();

            if (keyword != wantedKeyword)
                throw new Exception($"Invalid keyword. Must be: '{wantedKeyword}'");
            //Has to be number unless explicitly string
            int valueInt = -1;

            //TODO: helper int getEnumValue(string);
            if (enumType != null)
            {
                for (int i = 0; i < enumType.Length; i++)
                {
                    if (enumType[i] == value)
                    {
                        valueInt = i;
                        break;
                    }
                }
            }
            else
            {
                if (!int.TryParse(value, out valueInt))
                {
                    throw new Exception($"Value not a number: '{value}'");
                }
            }

            switch (keywordDict[keyword])
            {
                case "byte":
                    outStream.Write((byte)valueInt);
                    break;

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

        public static void WriteObject(BinaryWriter outStream, string wantedKeyword, Dictionary<string, int> enumType)
        {
            GetNextLine();
            string keyword = GetCurrentToken();
            string value = GetNextToken();

            if (keyword != wantedKeyword)
                throw new Exception($"Invalid keyword. Must be: '{wantedKeyword}'");
            //Has to be number unless explicitly string
            int valueInt = -1;

            if (enumType != null)
            {
                valueInt = enumType[value];
            }
            else
            {
                if (!int.TryParse(value, out valueInt))
                {
                    throw new Exception($"Value not a number: '{value}'");
                }
            }
            //Can throw System.Collections.Generic.KeyNotFoundException: 'The given key 'byte' was not present in the dictionary.'
            switch (keywordDict[keyword])
            {
                case "byte":
                    outStream.Write((byte)valueInt);
                    break;

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

        public static void WriteTokenObject(BinaryWriter outStream, string valueType, string[]? enumType = null)
        {
            string value = GetNextToken();
            //Has to be number unless explicitly string
            int valueInt = -1;

            //TODO: helper int getEnumValue(string);
            if (enumType != null)
            {
                for (int i = 0; i < enumType.Length; i++)
                {
                    if (enumType[i] == value)
                    {
                        valueInt = i;
                        break;
                    }
                }
            }
            else
            {
                if (!int.TryParse(value, out valueInt))
                {
                    throw new Exception($"Value not a number: '{value}'");
                }
            }

            switch (keywordDict[valueType])
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
            WriteImmediateString(outStream, wantedKeyword, length);
        }

        public static void WriteImmediateString(BinaryWriter outStream, string wantedKeyword, int length)
        {
            string keyword = GetCurrentToken();
            string value = GetNextToken();

            if (keyword != wantedKeyword)
                throw new Exception($"Invalid keyword. Must be: '{wantedKeyword}'");

            if (value.Length > length)
                throw new Exception($"String too long at {value.Length} chars. Must be less than: '{length}'");

            value = value.PadRight(length, '\0');
            outStream.Write(Encoding.UTF8.GetBytes(value));
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