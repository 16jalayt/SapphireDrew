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

            if (numDeps != 0 && InStream.debugprint) { Console.WriteLine("    ---Dependency---"); }

            for (int i = 0; i < numDeps; i++)
            {
                //type of RefDep
                deps[i].depType = InStream.ReadByte("Dep type: ");

                //unknown 0. I don't think belongs to another field.
                Helpers.AssertByte(InStream, 0);

                //type of RefFlag
                deps[i].depRefFlag = InStream.ReadShort("Dep ref: ");

                deps[i].depState = InStream.ReadShort("Dep state: ");
                deps[i].depFlag = InStream.ReadShort("Dep flag: ");
                deps[i].rect = new NancyRect(InStream.ReadShort(), InStream.ReadShort(), InStream.ReadShort(), InStream.ReadShort());
                if (InStream.debugprint) { Console.WriteLine(deps[i].rect); }
                if (InStream.debugprint) { Console.WriteLine("    ---End Dep---"); }
            }

            Helpers.AssertString(InStream, "EndOfDeps", true);
            //Assert only compares provided string length. Need to skip rest of field.
            InStream.Skip(23);

            return deps;
        }

        public static void PrintDeps(Dependency[] deps, StreamWriter writetext)
        {
            foreach (var dep in deps)
            {
                writetext.WriteLine("// ------------ Dependency -------------");

                string depType = Enums.depType[dep.depType];
                writetext.WriteLine($"RefDep      {depType}");

                //depRefFlag
                if (depType == "DT_EVENT")
                    writetext.WriteLine($"RefFlag   {Helpers.GetFlagName(dep.depRefFlag)}");
                else if (depType == "DT_INVENTORY")
                    writetext.WriteLine($"RefFlag   {Helpers.GetInvName(dep.depRefFlag)}");
                else if (depType == "DT_SOUND")
                    writetext.WriteLine($"int   {Enums.soundChannel[dep.depRefFlag]}");
                else if (depType == "DT_PLAYER_TOD")
                    writetext.WriteLine($"int   {Enums.tod[dep.depRefFlag]}");
                else if (depType == "DT_CURSOR_TYPE")
                    writetext.WriteLine($"int   {Enums.cursorDict[dep.depRefFlag]}");
                else if (depType == "DT_TIMER_LESS_THAN_DEPENDENCY_TIME" || depType == "DT_TIMER_GREATER_THAN_DEPENDENCY_TIME")
                    writetext.WriteLine($"int   {Enums.timers[dep.depRefFlag]}");
                else
                    writetext.WriteLine($"int   {dep.depRefFlag}");

                //depState
                if (depType == "DT_OPEN_PARENTHESIS" || depType == "DT_OPEN_PARENTHESIS" || depType == "DT_PLAYER_TOD")
                    writetext.WriteLine($"int     {dep.depState}");
                else if (depType == "DT_DIFFICULTY_LEVEL")
                    writetext.WriteLine($"int     {Enums.difficulty[dep.depState]}   // _EASY _HARD");
                else
                    writetext.WriteLine($"int     {Enums.tf[dep.depState]}");

                writetext.WriteLine($"int     {Enums.depFlag[dep.depFlag]}");
                writetext.WriteLine($"int     {dep.rect.RawPrint()}");
            }
        }
    }
}