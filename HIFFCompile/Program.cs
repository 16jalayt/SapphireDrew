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
                outStream.Write((int)-1);
                long FdataPlace = outStream.BaseStream.Position;

                outStream.Write(Encoding.UTF8.GetBytes("FLAGEVNT"));

                outStream.Write((int)0);

                long FendChunk = outStream.BaseStream.Position;
                outStream.Seek((int)FdataPlace - 4, SeekOrigin.Begin);
                int Flength = BinaryPrimitives.ReverseEndianness((int)(FendChunk - FdataPlace));
                outStream.Write(Flength);
                outStream.Seek((int)FendChunk, SeekOrigin.Begin);
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
                else if (getLine() == "CHUNK TSUM {")
                {
                    outStream.Write(Encoding.UTF8.GetBytes("SCENTSUM"));
                    outStream.Write((int)-1);
                    long scenPlace = outStream.BaseStream.Position;

                    string SceneDesc = getNextLine();
                    SceneDesc = SceneDesc.Substring(SceneDesc.IndexOf("\"") + 1);
                    SceneDesc = SceneDesc.Substring(0, SceneDesc.LastIndexOf("\""));
                    SceneDesc = SceneDesc.PadRight(50, '\0');
                    outStream.Write(Encoding.UTF8.GetBytes(SceneDesc));

                    string RefAVF = getNextLine();
                    RefAVF = RefAVF.Substring(RefAVF.IndexOf("\"") + 1);
                    RefAVF = RefAVF.Substring(0, RefAVF.LastIndexOf("\""));
                    RefAVF = RefAVF.PadRight(50, '\0');
                    outStream.Write(Encoding.UTF8.GetBytes(RefAVF));

                    string RefSound = getNextLine();
                    RefSound = RefSound.Substring(RefSound.IndexOf("\"") + 1);
                    RefSound = RefSound.Substring(0, RefSound.LastIndexOf("\""));
                    RefSound = RefSound.PadRight(50, '\0');
                    outStream.Write(Encoding.UTF8.GetBytes(RefSound));

                    //int chan = (int)getNextObject();

                    long endChunk = outStream.BaseStream.Position;
                    outStream.Seek((int)scenPlace - 4, SeekOrigin.Begin);
                    int length = BinaryPrimitives.ReverseEndianness((int)(endChunk - scenPlace));
                    outStream.Write(length);
                    outStream.Seek((int)endChunk, SeekOrigin.Begin);

                    if (getNextLine() != "}")
                    {
                        Console.WriteLine("TSUM chunk not closed.");
                    }
                }
                else if (getLine() == "CHUNK USE {")
                {
                    outStream.Write(Encoding.UTF8.GetBytes("USE\0"));
                    outStream.Write((int)-1);
                    long usePlace = outStream.BaseStream.Position;

                    //numDeps placeholder
                    outStream.Write((short)-1);
                    long numDepsPlace = outStream.BaseStream.Position;

                    if (getNextLine() != "BeginCount RefHif")
                    {
                        Console.WriteLine($"Unknown use contents: '{getLine()}'");
                        break;
                    }

                    while (getNextLine() != "EndCount RefHif")
                    {
                        string useRef = getLine();
                        useRef = useRef.Substring(useRef.IndexOf("\"") + 1);
                        useRef = useRef.Substring(0, useRef.LastIndexOf("\""));
                        useRef = useRef.PadRight(50, '\0');
                        outStream.Write(Encoding.UTF8.GetBytes(useRef));
                    }

                    long endDeps = outStream.BaseStream.Position;
                    outStream.Seek((int)usePlace - 2, SeekOrigin.Begin);
                    int deplength = BinaryPrimitives.ReverseEndianness((int)(endDeps - numDepsPlace));
                    outStream.Write(deplength);
                    outStream.Seek((int)endDeps, SeekOrigin.Begin);

                    long endChunk = outStream.BaseStream.Position;
                    outStream.Seek((int)usePlace - 4, SeekOrigin.Begin);
                    int length = BinaryPrimitives.ReverseEndianness((int)(endChunk - usePlace));
                    outStream.Write(length);
                    outStream.Seek((int)endChunk, SeekOrigin.Begin);

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

        /*private static Object getObject()
        {
            return null;
        }

        private static Object getNextObject()
        {
            string[] parts = System.Text.RegularExpressions.Regex.Split(getNextLine(), @"\s+");
            if (parts[0] == "int")
            {
                int intToReturn;
                if (!int.TryParse(parts[1], out intToReturn))
                {
                    Console.WriteLine($"'{parts[1]}' on line {pos + 1} is not a number.");
                    return null;
                }
                return intToReturn;
            }

            return getObject();
        }*/
    }
}