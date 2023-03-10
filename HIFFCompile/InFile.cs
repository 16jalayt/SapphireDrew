﻿using Sapphire_Extract_Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace HIFFCompile
{
    internal class InFile
    {
        //private static HashSet<string> keywords = new HashSet<string>
        //{ "byte", "int", "long", "RefDep", "RefFlag", "RefOvlStat" };
        public static string[] keywords = { "byte", "int", "long", "RefDep", "RefFlag", "RefOvlStat", "RefSetFlag" };

        public static string[] stringKeywords = { "RefAVF", "RefSound", "RefHif", "RefOvlStat" };

        public static string[] lines;
        public static int pos = 0;

        public static string GetNextLine()
        {
            pos++;
            //ignore comments
            if (lines[pos].Contains("//"))
            {
                lines[pos] = lines[pos].Substring(0, lines[pos].IndexOf("//"));
                //If line has only comment, get next available line
                if (lines[pos] == "")
                    GetNextLine();
            }

            lines[pos] = lines[pos].Trim();
            lines[pos] = Regex.Replace(lines[pos], @"\s+", " ");
            return lines[pos];
        }

        public static string GetLine()
        {
            return lines[pos];
        }

        public static bool GetObject<T>(ref BinaryWriter outStream, out int returnedObject, string[] enumType = null, Dictionary<int, string> dictType = null)
        {
            returnedObject = -1;

            //Remember types get downcast by one so ND long is C int
            //string[] parts = System.Text.RegularExpressions.Regex.Split(getLine(), @"\s+");
            string line = GetLine();
            string keyword = line.Substring(0, line.IndexOf(' '));

            //Should be one number unless rect. 0,0,0,0
            List<string> operand = new List<string>();
            operand.Add(line.Substring(line.IndexOf(' ')));
            operand[0] = operand[0].Trim(' ');

            if (Array.IndexOf(keywords, keyword) == -1)
            {
                Console.WriteLine($"Invalid type: '{keyword}' on line {pos + 1}. Must be a valid keyword.");
                return false;
            }

            if (operand[0].Count(f => f == ',') == 3)
            {
                operand = operand[0].Split(',').ToList();
            }

            int inEnum = -1;

            //TODO: prints same values for 1,2,3,4
            foreach (string item in operand)
            {
                inEnum = ParseObj(item, enumType, dictType);
                if (inEnum == -1)
                    return false;

                if (typeof(T) == typeof(byte))
                    outStream.Write((byte)inEnum);
                if (typeof(T) == typeof(short))
                    outStream.Write((short)inEnum);
                if (typeof(T) == typeof(int))
                    outStream.Write(inEnum);
            }

            returnedObject = inEnum;
            return true;
        }

        public static int ParseObj(string operand, string[] enumType, Dictionary<int, string> dictType)
        {
            //if not a number, either enum or syntax error
            if (!int.TryParse(operand, out int inEnum))
            {
                if (enumType != null)
                {
                    inEnum = Array.FindIndex(enumType, x => x.Contains(operand));
                    if (inEnum == -1)
                    {
                        Console.WriteLine($"'{operand}' on line {pos + 1} is not a number or enum value.");
                        return -1;
                    }
                }
                else if (dictType != null)
                {
                    inEnum = Enums.ACT_Type.FirstOrDefault(x => x.Value == operand).Key;
                    if (inEnum == 0)
                    {
                        Console.WriteLine($"'{operand}' on line {pos + 1} is not a number or 'ACT Type' value.");
                        return -1;
                    }
                }
                else
                {
                    Console.WriteLine($"'{operand}' on line {pos + 1}. Must contain either a number/enum value or a rect. i.e. 0,0,0,0");
                    return -1;
                }
            }
            return inEnum;
        }

        public static bool GetNextObject<T>(ref BinaryWriter outStream, out int returnedObject, string[] enumType = null, Dictionary<int, string> dictType = null)
        {
            GetNextLine();
            returnedObject = -1;
            return GetObject<T>(ref outStream, out returnedObject, enumType, dictType);
        }

        public static bool GetObject<T>(ref BinaryWriter outStream, string[] enumType = null, Dictionary<int, string> dictType = null)
        {
            return GetObject<T>(ref outStream, out _, enumType, dictType);
        }

        public static bool GetNextObject<T>(ref BinaryWriter outStream, string[] enumType = null, Dictionary<int, string> dictType = null)
        {
            return GetNextObject<T>(ref outStream, out _, enumType, dictType);
        }

        public static bool GetString(ref BinaryWriter outStream, int length)
        {
            string SceneDesc = GetLine();

            //split input keyword and expression
            string[] parts = System.Text.RegularExpressions.Regex.Split(GetLine(), @"\s+");

            //Reassemble the quoted part of the string. If there are spaces, it will be split
            for (int i = 2; i < parts.Length; i++)
            {
                parts[1] = parts[1] + " " + parts[i];
            }

            //validate keyword
            if (Array.IndexOf(stringKeywords, parts[0]) == -1)
            {
                if (parts[0].Contains('[') && parts[0].Contains("]"))
                {
                    parts[0] = parts[0].Substring(parts[0].IndexOf("[") + 1);
                    parts[0] = parts[0].Substring(0, parts[0].LastIndexOf("]"));
                }
                else
                {
                    Console.WriteLine($"Unknown keyword: '{parts[0]}' on line {pos + 1}. Must be a valid keyword");
                    return false;
                }
            }

            //validate expression
            if (parts[1].Length < 1 || parts[1].Length > length)
            {
                Console.WriteLine($"Expression too long: '{parts[1]}' on line {pos + 1}. Must be less than {length}");
                return false;
            }

            if (parts[1].Count(f => f == '\"') != 2)
            {
                Console.WriteLine($"Expression must be in double quotes \"x\": '{parts[1]}' on line {pos + 1}");
                return false;
            }

            //Trim line
            SceneDesc = SceneDesc.Substring(SceneDesc.IndexOf("\"") + 1);
            SceneDesc = SceneDesc.Substring(0, SceneDesc.LastIndexOf("\""));
            SceneDesc = SceneDesc.PadRight(length, '\0');
            outStream.Write(Encoding.UTF8.GetBytes(SceneDesc));

            return true;
        }

        public static bool GetNextString(ref BinaryWriter outStream, int length)
        {
            GetNextLine();
            return GetString(ref outStream, length);
        }
    }
}