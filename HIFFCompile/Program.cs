using Sapphire_Extract_Helpers;
using System;
using System.Buffers.Binary;
using System.IO;
using System.Linq;
using System.Text;

namespace HIFFCompile
{
    internal class Program
    {
        //TODO:delete partial file on faliure

        public static void Main(string[] args)
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

            if (!File.Exists(inFile))
            {
                Console.WriteLine($"The file: '{inFile}' does not exist.\n");
                return;
            }

            Console.WriteLine($"Compiling: '{inFile}'\n");

            //only used to get full path of input
            BetterBinaryReader testFile = new BetterBinaryReader(inFile);

            FileInfo outFile = new FileInfo(Path.GetDirectoryName(testFile.FilePath) + "/Output/" + Path.GetFileNameWithoutExtension(testFile.FilePath) + ".hiff");
            testFile.Dispose();
            outFile.Directory.Create();
            BinaryWriter outStream = new BinaryWriter(new FileStream(outFile.FullName, FileMode.Create), Encoding.UTF8);

            InFile.lines = File.ReadLines(inFile).ToArray();

            ParseHIFF.Parse(olderGame, ref outStream, inFile);

            outStream.Close();
        }
    }
}