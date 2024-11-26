using Sapphire_Extract_Helpers;
using System;
using System.Buffers.Binary;
using System.IO;
using System.Text;

namespace HIFFCompile
{
    internal class Utils
    {
        public static int WriteLength(ref BinaryWriter outStream, long placeholder)
        {
            long endChunk = outStream.BaseStream.Position;
            outStream.Seek((int)placeholder, SeekOrigin.Begin);
            int test = (int)(endChunk - placeholder - 4);
            int length = BinaryPrimitives.ReverseEndianness((int)(endChunk - placeholder - 4));
            outStream.Write(length);
            outStream.Seek((int)endChunk, SeekOrigin.Begin);
            return length;
        }

        /*public static int ParseDeps(ref BinaryWriter outStream)
        {
            int posPlaceholder = InFile.pos;
            long depsPleceholder = outStream.BaseStream.Position;
            outStream.Write((int)-1);

            int numDeps = 0;
            while (InFile.lines[InFile.pos] != "// ------------ Dependency -------------" && InFile.lines[InFile.pos] != "}" && InFile.pos < InFile.lines.Length - 1)
            { InFile.pos++; }

            if (InFile.lines[InFile.pos] == "// ------------ Dependency -------------")
            {
                numDeps++;
                //Console.WriteLine($"Dep start {pos + 1}.");
                //TODO: double check length
                if (!InFile.GetNextObject<short>(ref outStream, enumType: Enums.depType))
                    return -1;
                //TODO: ??? game specific. Need table or something.
                ////Then again, decompiled would be number anyway
                if (!InFile.GetNextObject<short>(ref outStream, enumType: Enums.execType))
                    return -1;

                //condition FALSE=0 TRUE=1
                if (!InFile.GetNextObject<short>(ref outStream, enumType: Enums.tf))
                    return -1;
                //0=AND 1=OR
                if (!InFile.GetNextObject<short>(ref outStream, enumType: Enums.depFlag))
                    return -1;

                //Rect called "time". Not sure purpose
                if (!InFile.GetNextObject<short>(ref outStream))
                    return -1;
            }
            else
                InFile.pos--;

            ///end deps
            int posEndDeps = InFile.pos;
            InFile.pos = posPlaceholder;

            long depstemp = outStream.BaseStream.Position;
            outStream.Seek((int)depsPleceholder, SeekOrigin.Begin);
            outStream.Write(numDeps);
            outStream.Seek((int)depstemp, SeekOrigin.Begin);

            string endOfDeps = "EndOfDeps";
            endOfDeps = endOfDeps.PadRight(32, '\0');
            outStream.Write(Encoding.UTF8.GetBytes(endOfDeps));

            return posEndDeps;
        }*/

        public static int ParseDepsNew(ref BinaryWriter outStream, int actType)
        {
            int posPlaceholder = InFile.pos;
            long depsPleceholder = outStream.BaseStream.Position;
            outStream.Write((int)-1);
            int numDeps = 0;

            InFile.pos++;

            //If depenency is shorthand
            if (InFile.lines[InFile.pos].StartsWith("if"))
            {
                string[] tokens = InFile.Tokenize(InFile.GetLine());

                for (int i = 1; i < tokens.Length; i += 2)
                {
                    //Context sensive. Probably DT_EVENT=2 or DT_SOUND=17
                    //AT_FLAGS=90  AT_FLAGS_HS=91
                    if (actType == 90 || actType == 91)
                        //DT_EVENT=2
                        outStream.Write((short)2);
                    //AT_PLAY_DIGI_SOUND=150  ?AT_POP_SCENE=111
                    else if (actType == 150 || actType == 111)
                        //DT_SOUND=17
                        outStream.Write((short)17);
                    else
                    {
                        Console.WriteLine("Invalid ACT chunk type used with the if clause.");
                        return -1;
                    }

                    int variable = InFile.ParseObj(tokens[i], null, null);
                    if (variable == -1)
                        return -1;
                    outStream.Write((short)variable);

                    int truth = InFile.parseTF(tokens[i + 1]);
                    if (truth == -1)
                        return -1;
                    outStream.Write((short)truth);

                    //Truth type and time rect default to 0
                    outStream.Write((short)0);
                    outStream.Write((int)0);
                    outStream.Write((int)0);

                    numDeps++;
                }

                InFile.pos++;
            }
            //else full -- Dependency -- chunk
            else
            {
                depsHelper(ref outStream, ref numDeps);
            }

            ///end deps
            int posEndDeps = InFile.pos - 1;
            InFile.pos = posPlaceholder;

            long depstemp = outStream.BaseStream.Position;
            outStream.Seek((int)depsPleceholder, SeekOrigin.Begin);
            outStream.Write(numDeps);
            outStream.Seek((int)depstemp, SeekOrigin.Begin);

            string endOfDeps = "EndOfDeps";
            endOfDeps = endOfDeps.PadRight(32, '\0');
            outStream.Write(Encoding.UTF8.GetBytes(endOfDeps));

            return posEndDeps;
        }

        private static void depsHelper(ref BinaryWriter outStream, ref int numDeps)
        {
            //end of file
            if (InFile.pos >= InFile.lines.Length - 1)
            {
                return;
            }
            //End of chunk
            else if (InFile.lines[InFile.pos] == "}")
            {
                return;
            }
            else if (InFile.lines[InFile.pos] == "// ------------ Dependency -------------")
            {
                numDeps++;
                //Console.WriteLine($"Dep start {pos + 1}.");
                //TODO: double check length
                if (!InFile.GetNextObject<short>(ref outStream, enumType: Enums.depType))
                    return;
                //TODO: ??? game specific. Need table or something.
                ////Then again, decompiled would be number anyway
                if (!InFile.GetNextObject<short>(ref outStream, enumType: Enums.execType))
                    return;

                //condition FALSE=0 TRUE=1  When time: _EQUAL_TO, _GREATER_THAN, _GREATER_THAN_OR_EQUAL, _LESS_THAN, _LESS_THAN_OR_EQUAL
                if (!InFile.GetNextObject<short>(ref outStream, enumType: Enums.tf))
                    return;
                //0=AND 1=OR
                if (!InFile.GetNextObject<short>(ref outStream, enumType: Enums.depFlag))
                    return;

                //Rect called "time". time format: StartHr/StartMin/EndHr/EndMin
                if (!InFile.GetNextObject<short>(ref outStream))
                    return;

                //Recurse to check if another dep
                depsHelper(ref outStream, ref numDeps);
                return;
            }
            else
            {
                InFile.pos++;
                depsHelper(ref outStream, ref numDeps);
            }
        }
    }
}