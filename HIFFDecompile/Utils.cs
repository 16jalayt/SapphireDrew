using Sapphire_Extract_Helpers;
using System;
using System.Collections.Generic;
using System.IO;

namespace HIFFDecompile
{
    internal static class Utils
    {
        public static bool preferLong = false;

        public static void FatalError()
        {
            Console.WriteLine("\nFatal Error. Exiting...");
            System.Environment.Exit(-1);
        }

        public struct Dependency
        {
            public byte depType;
            public short depRefFlag;
            public short depState;
            public short depFlag;
            public NancyRect rect;
        }

        public static Dependency[] ParseDeps(BetterBinaryReader InStream)
        {
            //number of dependencies
            //LITTLE Endian
            int numDeps = InStream.ReadInt("Num deps: ");
            Dependency[] deps = new Dependency[numDeps];

            //TODO:if numdeps != 0
            if (numDeps != 0 && InStream.debugprint) { Console.WriteLine("    ---Ref---"); }

            for (int i = 0; i < numDeps; i++)
            {
                //type of RefDep
                deps[i].depType = InStream.ReadByte("Dep type: ");

                //unknown 0. I don't think belongs to another field.
                Helpers.AssertByte(InStream, 0);

                //type of RefFlag
                deps[i].depRefFlag = InStream.ReadShort("Dep ref: ");

                //TODO: more here, need different dependency to test
                //All Shorts?
                //InStream.Skip(12);
                deps[i].depState = InStream.ReadShort("Dep state: ");
                deps[i].depFlag = InStream.ReadShort("Dep flag: ");
                deps[i].rect = new NancyRect(InStream.ReadShort(), InStream.ReadShort(), InStream.ReadShort(), InStream.ReadShort());
                if (InStream.debugprint) { Console.WriteLine(deps[i].rect); }
                if (InStream.debugprint) { Console.WriteLine("    ---End Ref---"); }
            }

            Helpers.AssertString(InStream, "EndOfDeps", true);
            //Assert only compares provided string length. Need to skip rest of field.
            InStream.Skip(23);

            return deps;
        }

        private static Dictionary<int, string> Flags = new Dictionary<int, string>();

        public static void PopulateFlags(string? flagsFileName, bool verbose = false)
        {
            if (flagsFileName != null)
            {
                if (!File.Exists(flagsFileName))
                {
                    Console.WriteLine("The flags file '{}' does not exist");
                    System.Environment.Exit(1);
                }

                Console.WriteLine("Parsing Flags.hif");
                BetterBinaryReader InStream = new BetterBinaryReader(flagsFileName);

                //TODO: better parsing or common
                InStream.Seek(20);
                while (!InStream.IsEOF())
                {
                    //Appears at end of file
                    int padding = InStream.ReadByte();
                    if (padding == 0)
                        break;
                    InStream.Seek(-1, SeekOrigin.Current);

                    string flagName = Helpers.String(InStream.ReadBytes(33)).TrimEnd('\0');
                    short num = InStream.ReadShort();
                    if (verbose)
                        Console.WriteLine($"'{flagName}' - '{num}'");

                    //At least in WOLF, dupes seem common
                    if (!Flags.TryAdd(num, flagName))
                        Console.WriteLine($"Duplicate flag for '{flagName}' - '{num}'");
                }
            }
        }

        public static string GetFlagName(int num)
        {
            string? properName;
            if (Flags.Count != 0 && Flags.TryGetValue(num, out properName))
                return properName;
            else
                return num.ToString();
        }
    }
}