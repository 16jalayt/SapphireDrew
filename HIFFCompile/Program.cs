using Sapphire_Extract_Helpers;
using System;
using System.Buffers.Binary;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace HIFFCompile
{
    internal class Program
    {
        private static string[] lines;
        private static int pos = 0;

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
            string FileName = args[0];

            bool olderGame = false;
            if (args.Length > 1 && args[1] == "-o")
                olderGame = true;

            //only used to get full path of input
            BetterBinaryReader testFile = new BetterBinaryReader(FileName);

            if (!File.Exists(FileName))
            {
                Console.WriteLine($"The file: '{FileName}' does not exist.\n");
                return;
            }

            Console.WriteLine($"Compiling: '{FileName}'");

            FileInfo outFile = new FileInfo(Path.GetDirectoryName(testFile.FilePath) + "/Output/" + Path.GetFileNameWithoutExtension(testFile.FilePath) + ".hiff");
            testFile.Dispose();
            outFile.Directory.Create();
            BinaryWriter outStream = new BinaryWriter(new FileStream(outFile.FullName, FileMode.Create), Encoding.UTF8);

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
            long dataPlace = outStream.BaseStream.Position;

            //write data header
            lines = File.ReadLines(FileName).ToArray();
            for (; pos < lines.Length; pos++)
            {
                if (getLine().StartsWith("//") || getLine() == "")
                    continue;
                //TODO: change to generic switch token insted of exact match
                else if (getLine() == "CHUNK ACT {")
                {
                }
                else if (getLine() == "CHUNK TSUM {")
                {
                    outStream.Write(Encoding.UTF8.GetBytes("SCENTSUM"));
                    long scenPlace = outStream.BaseStream.Position;
                    outStream.Write((int)-1);

                    //Scene description
                    if (!getNextString(ref outStream, 50))
                        break;
                    //Background file without extension
                    if (!getNextString(ref outStream, 33))
                        break;
                    //Background sound
                    if (!getNextString(ref outStream, 33))
                        break;

                    //Channel of backgound sound
                    if (!getNextObject<short>(ref outStream, Enums.soundChannel))
                        break;
                    //Loop background sound?
                    if (!getNextObject<int>(ref outStream, Enums.loop))
                        break;
                    //Left channel volume for background sound
                    if (!getNextObject<short>(ref outStream, null))
                        break;
                    //Right channel volume for background sound
                    if (!getNextObject<short>(ref outStream, null))
                        break;

                    writeLength(ref outStream, scenPlace);

                    if (getNextLine() != "}")
                    {
                        Console.WriteLine("TSUM chunk not closed.");
                    }
                }
                else if (getLine() == "CHUNK USE {")
                {
                    outStream.Write(Encoding.UTF8.GetBytes("USE\0"));
                    long usePlace = outStream.BaseStream.Position;
                    outStream.Write((int)-1);

                    //numDeps placeholder
                    long numDepsPlace = outStream.BaseStream.Position;
                    outStream.Write((short)-1);

                    if (getNextLine() != "BeginCount RefHif")
                    {
                        Console.WriteLine($"Unknown use contents: '{getLine()}'");
                        break;
                    }

                    short numDeps = 0;
                    while (getNextLine() != "EndCount RefHif")
                    {
                        getString(ref outStream, 33);
                        numDeps++;
                    }

                    long endDeps = outStream.BaseStream.Position;
                    outStream.Seek((int)numDepsPlace, SeekOrigin.Begin);
                    outStream.Write(numDeps);
                    outStream.Seek((int)endDeps, SeekOrigin.Begin);

                    writeLength(ref outStream, usePlace);

                    if (getNextLine() != "}")
                    {
                        Console.WriteLine("TSUM chunk not closed.");
                    }
                }
                else
                {
                    Console.WriteLine($"Unknown line contents: '{getLine()}' on line {pos + 1}");
                    break;
                }
            }

            if (pos < lines.Length)
                Console.WriteLine($"Syntax error in: '{FileName}'");
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

        private static string getNextLine()
        {
            pos++;
            //ignore comments
            if (lines[pos].Contains("//"))
            {
                lines[pos] = lines[pos].Substring(0, lines[pos].IndexOf("//"));
                //If line has only comment, get next available line
                if (lines[pos] == "")
                    getNextLine();
            }

            lines[pos] = lines[pos].Trim();
            return lines[pos];
        }

        private static string getLine()
        {
            return lines[pos];
        }

        private static bool getObject<T>(ref BinaryWriter outStream, string[] enumType)
        {
            //Remember types get downcast by one so ND long is C int
            string[] parts = System.Text.RegularExpressions.Regex.Split(getLine(), @"\s+");
            if (parts[0] != "long" && parts[0] != "int")
            {
                Console.WriteLine($"Invalid type: '{parts[0]}' on line {pos + 1}. Must be int or long.");
                return false;
            }
            int inEnum;
            //if not a number, either enum or syntax error
            if (!int.TryParse(parts[1], out inEnum))
            {
                inEnum = Array.FindIndex(enumType, x => x.Contains(parts[1]));
                if (inEnum == -1)
                {
                    Console.WriteLine($"'{parts[1]}' on line {pos + 1} is not a number or enum value.");
                    return false;
                }
            }
            if (typeof(T) == typeof(int))
                outStream.Write(inEnum);
            if (typeof(T) == typeof(short))
                outStream.Write((short)inEnum);
            return true;
        }

        private static bool getNextObject<T>(ref BinaryWriter outStream, string[] enumType)
        {
            getNextLine();
            return getObject<T>(ref outStream, enumType);
        }

        private static bool getString(ref BinaryWriter outStream, int length)
        {
            string SceneDesc = getLine();

            //split input keyword and expression
            string[] parts = System.Text.RegularExpressions.Regex.Split(getLine(), @"\s+");

            //Reassemble the quoted part of the string. If there are spaces, it will be split
            for (int i = 2; i < parts.Length; i++)
            {
                parts[1] = parts[1] + " " + parts[i];
            }

            //validate keyword
            if (parts[0] != "RefAVF" && parts[0] != "RefSound" && parts[0] != "RefHif")
            {
                if (parts[0].Contains("[") && parts[0].Contains("]"))
                {
                    parts[0] = parts[0].Substring(parts[0].IndexOf("[") + 1);
                    parts[0] = parts[0].Substring(0, parts[0].LastIndexOf("]"));
                }
                else
                {
                    Console.WriteLine($"Unknown keyword: '{parts[0]}' on line {pos + 1}. Must be char[x], RefAVF, RefHif, or RefSound");
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

        private static bool getNextString(ref BinaryWriter outStream, int length)
        {
            getNextLine();
            return getString(ref outStream, length);
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