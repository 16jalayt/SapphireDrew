using Sapphire_Extract_Helpers;
using System;
using System.IO;
using System.Text;

namespace HIFFCompile
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //Tool status
            //Console.WriteLine($"CURRENTLY BROKEN\n");
            Console.WriteLine($"EXPEREMENTAL\n");
            //Console.WriteLine($"UNVALIDATED\n");

            if (args.Length < 1)
            {
                Console.WriteLine($"Usage is XSheetCompile.exe filename\n");
                return;
            }

            string FileName = args[0];

            if (!File.Exists(FileName))
            {
                Console.WriteLine($"The file: '{FileName}' does not exist.\n");
                return;
            }

            BetterBinaryReader InStream = new BetterBinaryReader(FileName);

            if (args.Length > 1 && args[1] == "-v")
                InStream.debugprint = true;

            Console.WriteLine($"Printout of: '{FileName}'\n");

            FileInfo inFile = new FileInfo(Path.GetDirectoryName(InStream.FilePath) + "/Output/" + Path.GetFileNameWithoutExtension(InStream.FilePath) + ".htxt");
            inFile.Directory.Create();
            BinaryWriter outStream = new BinaryWriter(new FileStream(inFile.FullName, FileMode.Create), Encoding.UTF8);

            /*foreach (string line in File.ReadLines(FileName))
            {
            }*/
        }
    }
}