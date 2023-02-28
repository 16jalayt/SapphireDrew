using Sapphire_Extract_Helpers;
using System;
using System.Buffers.Binary;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Text;

namespace HIFFCompile
{
    internal class Program
    {
        private static string[] lines;
        private static int pos = 0;
        private static List<string[]> enumList = new List<string[]> { Enums.soundChannel, Enums.execType, Enums.loop, Enums.tf, Enums.CCTEXT_TYPE, Enums.depFlag, Enums.z };
        private static BinaryWriter outStream;
        private static long chunkClose = -1;
        private static int refCount = 0;
        private static string refCountType = "";
        private static long refCountPos = 0;

        //Used to know when to insert deps
        private static int byteCount = 0;

        //TODO:break on error

        private static void Main(string[] args)
        {
            //Tool status
            Console.WriteLine($"CURRENTLY BROKEN\n");
            //Console.WriteLine($"EXPEREMENTAL\n");
            //Console.WriteLine($"UNVALIDATED\n");

            if (args.Length < 1)
            {
                Console.WriteLine($"Usage is XSheetCompile.exe filename\n");
                return;
            }
            string inFile = args[0];

            bool olderGame = false;
            if (args.Length > 1 && args[1] == "-o")
                olderGame = true;

            //only used to get full path of input
            BetterBinaryReader testFile = new BetterBinaryReader(inFile);

            if (!File.Exists(inFile))
            {
                Console.WriteLine($"The file: '{inFile}' does not exist.\n");
                return;
            }

            Console.WriteLine($"Compiling: '{inFile}'");

            FileInfo outFile = new FileInfo(Path.GetDirectoryName(testFile.FilePath) + "/Output/" + Path.GetFileNameWithoutExtension(testFile.FilePath) + ".hiff");
            testFile.Dispose();
            outFile.Directory.Create();
            outStream = new BinaryWriter(new FileStream(outFile.FullName, FileMode.Create), Encoding.UTF8);

            //If newer game write FLAGEVNT block
            if (!olderGame)
            {
                outStream.Write(Encoding.UTF8.GetBytes("DATA"));
                //Chunk length placeholder
                long FdataPlace = outStream.BaseStream.Position;
                outStream.Write((int)-1);

                outStream.Write(Encoding.UTF8.GetBytes("FLAGEVNT"));

                outStream.Write((int)0);

                writeLength(ref outStream, FdataPlace);
            }

            outStream.Write(Encoding.UTF8.GetBytes("DATA"));
            //Chunk length placeholder
            outStream.Write((int)-1);
            //TODO: add to stack
            long dataPlace = outStream.BaseStream.Position;
            outStream.Write(Encoding.UTF8.GetBytes("SCEN"));

            //Get uncompiled file as array
            lines = File.ReadLines(inFile).ToArray();

            //Parse file line by line until error
            while (parseLine(outStream)) { }

            if (pos < lines.Length - 1)
                Console.WriteLine($"Syntax error in: '{inFile}'");
            else
            {
                //update data chunk length at beginning of file
                long endChunk = outStream.BaseStream.Position;
                outStream.Seek((int)dataPlace - 4, SeekOrigin.Begin);
                int length = BinaryPrimitives.ReverseEndianness((int)(endChunk - dataPlace));
                outStream.Write(length);
                outStream.Seek((int)endChunk, SeekOrigin.Begin);

                Console.WriteLine("Success");
            }

            outStream.Close();
        }

        private static bool parseLine(BinaryWriter outStream)
        {
            string[] currentLine = getNextLine();

            //Handle special because not just token=, there is a variable inside 1st token
            if (currentLine[0].Contains("char") == true)
            {
                string[] parts = prepExp(currentLine);
                string number;
                if (parts[0].Contains("[") && parts[0].Contains("]"))
                {
                    number = parts[0].Substring(parts[0].IndexOf("[") + 1);
                    number = number.Substring(0, number.LastIndexOf("]"));

                    int length = getNumber(number);
                    if (length == -9999)
                        return false;

                    if (!writeStringExpression(parts[1], length))
                        return false;
                }
                else
                {
                    Console.WriteLine($"Unknown keyword: '{parts[0]}' on line {pos + 1}. Must be char[x]");
                    return false;
                }
                return true;
            }

            //Check first token in line
            switch (currentLine[0])
            {
                case "CHUNK":
                    //validate tokens
                    if (currentLine.Length != 3)
                    {
                        Console.WriteLine($"Chunk must specify a chunk type and contain an open bracket: '{getLineRaw()}' on line {pos + 1}");
                        return false;
                    }
                    if (currentLine[2] != "{")
                    {
                        Console.WriteLine($"Chunk must contain an open bracket: '{getLineRaw()}' on line {pos + 1}");
                        return false;
                    }
                    if (currentLine[1].Length > 4)
                    {
                        Console.WriteLine($"Chunk type too long: '{getLineRaw()}' on line {pos + 1}");
                        return false;
                    }
                    //TODO: validate all caps

                    currentLine[1] = currentLine[1].PadRight(4, '\0');
                    //Just write header.
                    outStream.Write(Encoding.UTF8.GetBytes(currentLine[1]));
                    chunkClose = outStream.BaseStream.Position;
                    outStream.Write((int)-1);
                    break;

                case "byte":
                    handleCount("byte");
                    byteCount++;
                    int byteVal = getNumber(currentLine[1]);
                    if (byteVal == -9999)
                        return false;
                    outStream.Write((byte)byteVal);

                    if (byteCount == 2)
                    {
                        //Parse deps

                        //look ahead until RefDep
                    }

                    break;

                case "int":
                    handleCount("int");
                    int intVal = getNumber(currentLine[1]);
                    if (intVal == -9999)
                        return false;
                    outStream.Write((short)intVal);
                    break;

                case "long":
                    handleCount("long");
                    int longVal = getNumber(currentLine[1]);
                    if (longVal == -9999)
                        return false;
                    outStream.Write((int)longVal);
                    break;

                case "RefAVF":
                    handleCount("RefAVF");
                    string[] RefAVF = prepExp(currentLine);
                    if (!writeStringExpression(RefAVF[1], 33))
                        return false;
                    break;

                case "RefSound":
                    handleCount("RefSound");
                    string[] RefSound = prepExp(currentLine);
                    if (!writeStringExpression(RefSound[1], 33))
                        return false;
                    break;

                case "RefHif":
                    handleCount("RefHif");
                    string[] RefHif = prepExp(currentLine);
                    if (!writeStringExpression(RefHif[1], 33))
                        return false;
                    break;

                case "RefDep":
                    //TODO: skip to }
                    break;

                case "BeginCount":
                    refCount = 0;
                    refCountType = currentLine[1];
                    refCountPos = outStream.BaseStream.Position;
                    outStream.Write((short)-1);
                    break;

                case "EndCount":
                    long currentPos = outStream.BaseStream.Position;
                    outStream.Seek((int)refCountPos, SeekOrigin.Begin);
                    outStream.Write((short)refCount);
                    outStream.Seek((int)currentPos, SeekOrigin.Begin);

                    refCount = 0;
                    refCountType = "";
                    break;

                case "}":
                    if (chunkClose == -1)
                    {
                        Console.WriteLine($"Too many closures at line: {pos + 1}");
                        return false;
                    }

                    writeLength(ref outStream, chunkClose);
                    chunkClose = -1;
                    break;

                default:
                    Console.WriteLine($"Unknown line contents: '{getLineRaw()}' on line {pos + 1}");
                    return false;
            }
            return true;
        }

        private static void handleCount(string type)
        {
            if (type == refCountType)
                refCount++;
        }

        private static string getNextLineRaw()
        {
            pos++;
            //ignore comments
            if (lines[pos].Contains("//"))
            {
                lines[pos] = lines[pos].Substring(0, lines[pos].IndexOf("//"));
                //If line has only comment, get next available line
                if (lines[pos] == "")
                    getNextLineRaw();
            }
            while (lines[pos] == "")
                getNextLineRaw();

            lines[pos] = lines[pos].Trim();
            return lines[pos];
        }

        private static string getLineRaw()
        {
            return lines[pos];
        }

        private static string[] getNextLine()
        {
            String line = getNextLineRaw();
            return tokenize(line);
        }

        private static string[] getLine()
        {
            //TODO: cache result?
            return tokenize(getLineRaw());
        }

        private static string[] tokenize(string input)
        {
            //split input keyword and expression
            string[] parts = System.Text.RegularExpressions.Regex.Split(input, @"\s+");

            return parts;
        }

        private static int getNumber(string input)
        {
            int result;
            //if not a number, either enum or syntax error
            if (int.TryParse(input, out result))
                return result;
            else
            {
                foreach (string[] arr in enumList)
                {
                    result = Array.FindIndex(arr, x => x.Contains(input));
                    if (result != -1)
                    {
                        return result;
                    }
                }
                int myKey = Enums.ACT_Type.FirstOrDefault(x => x.Value == input).Key;
                if (myKey != 0)
                    return myKey;
            }
            Console.WriteLine($"Not in enum:{input}, on line: {pos + 1}");
            return -9999;
        }

        private static bool writeStringExpression(string outString, int length)
        {
            if (outString.Count(f => f == '\"') != 2)
            {
                Console.WriteLine($"Expression must be in double quotes \"x\": '{outString}' on line {pos + 1}");
                return false;
            }
            outString = outString.Substring(outString.IndexOf("\"") + 1);
            outString = outString.Substring(0, outString.LastIndexOf("\""));
            outString = outString.PadRight(length, '\0');
            outStream.Write(Encoding.UTF8.GetBytes(outString));

            return true;
        }

        private static string[] prepExp(string[] input)
        {
            string[] output = new string[2];

            output[0] = input[0];

            //Reassemble the quoted part of the string. If there are spaces, it will be split
            for (int i = 1; i < input.Length; i++)
            {
                output[1] = output[1] + " " + input[i];
            }

            return output;
        }

        private static void writeLength(ref BinaryWriter outStream, long placeholder)
        {
            long endChunk = outStream.BaseStream.Position;
            outStream.Seek((int)placeholder, SeekOrigin.Begin);
            int length = BinaryPrimitives.ReverseEndianness((int)(endChunk - placeholder - 4));
            outStream.Write(length);
            outStream.Seek((int)endChunk, SeekOrigin.Begin);
        }
    }
}