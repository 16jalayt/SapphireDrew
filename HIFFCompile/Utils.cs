using Sapphire_Extract_Helpers;
using System;
using System.Buffers.Binary;
using System.IO;
using System.Text;

namespace HIFFCompile
{
    internal class Utils
    {
        public static int WriteLength(BinaryWriter outStream, long placeholder)
        {
            long endChunk = outStream.BaseStream.Position;
            outStream.Seek((int)placeholder, SeekOrigin.Begin);
            //int test = (int)(endChunk - placeholder - 4);
            int length = BinaryPrimitives.ReverseEndianness((int)(endChunk - placeholder - 4));
            outStream.Write(length);
            outStream.Seek((int)endChunk, SeekOrigin.Begin);
            return length;
        }

        public static int ParseDeps(BinaryWriter outStream)
        {
            int posPlaceholder = InFile.pos;
            long depsPleceholder = outStream.BaseStream.Position;
            outStream.Write((int)-1);

            int numDeps = 0;
            //Need raw line reads because will be removed as comment
            while (InFile.lines[InFile.pos] != "// ------------ Dependency -------------" && InFile.lines[InFile.pos] != "}" && InFile.pos < InFile.lines.Length - 1)
            { InFile.pos++; }

            if (InFile.lines[InFile.pos] == "// ------------ Dependency -------------")
            {
                numDeps++;
                //Console.WriteLine($"Dep start {InFile.pos + 1}.");
                InFile.WriteObject(outStream, "RefDep", Enums.depType);
                InFile.WriteObject(outStream, "RefFlag", Enums.flagsReverse);

                //condition FALSE=0 TRUE=1
                InFile.WriteObject(outStream, "int", Enums.tf);
                //0=AND 1=OR
                InFile.WriteObject(outStream, "int", Enums.depFlag);

                //Rect called "time". Not sure purpose
                //TODO: find better way?
                InFile.GetNextLine();
                InFile.WriteTokenObject(outStream, "int");
                InFile.WriteTokenObject(outStream, "int");
                InFile.WriteTokenObject(outStream, "int");
                InFile.WriteTokenObject(outStream, "int");
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
        }

        public static int ParseDepsShort(BinaryWriter outStream)
        {
            int tokenPosPlaceholder = InFile.tokenPos;
            long depsPleceholder = outStream.BaseStream.Position;
            outStream.Write((int)-1);

            int numDeps = 0;
            //Need raw line reads because will be removed as comment
            while (InFile.HasNextToken() && InFile.GetNextToken() != "if")
            { }

            //loop pair
            //string dep1 = InFile.GetNextToken();
            //string dep2 = InFile.GetNextToken();
            if (InFile.GetCurrentToken() == "if")
            {
                numDeps++;

                //RefDep
                outStream.Write((short)2);

                InFile.WriteTokenObject(outStream, "RefFlag", Enums.flagsReverse);

                //condition FALSE=0 TRUE=1
                InFile.WriteTokenObject(outStream, "int", Enums.tf);
                //DepFlag 0=AND 1=OR
                outStream.Write((short)0);

                //Rect called "time". Not sure purpose
                outStream.Write((short)0);
                outStream.Write((short)0);
                outStream.Write((short)0);
                outStream.Write((short)0);
            }
            //else
            //    InFile.pos--;

            ///end deps
            int posEndDeps = InFile.tokenPos;

            long depstemp = outStream.BaseStream.Position;
            outStream.Seek((int)depsPleceholder, SeekOrigin.Begin);
            outStream.Write(numDeps);
            outStream.Seek((int)depstemp, SeekOrigin.Begin);

            string endOfDeps = "EndOfDeps";
            endOfDeps = endOfDeps.PadRight(32, '\0');
            outStream.Write(Encoding.UTF8.GetBytes(endOfDeps));

            InFile.tokenPos = tokenPosPlaceholder;

            return posEndDeps;
        }

        public static void CheckOpenClosure()
        {
            //Covers both coding styles. Same line prefered.
            if (InFile.GetNextToken() == "{" || InFile.GetNextLine() == "{")
                return;

            throw new Exception($"Chunk needs a \'{{\' character to open the closure. On line '{InFile.pos}' found instead: '{InFile.GetLine()}'");
        }

        public static void CheckCloseClosure()
        {
            //Covers both coding styles. Same line prefered.
            if (InFile.GetNextToken() == "}" || InFile.GetNextLine() == "}")
                return;

            if (InFile.GetCurrentToken() == "RefDep")
            {
                while (InFile.GetLine() != "}" && InFile.pos < InFile.lines.Length - 1)
                { InFile.GetNextLine(); }
                if (InFile.GetLine() == "}")
                    return;
            }

            throw new Exception($"Chunk needs a \'}}\' character to close the closure. On line '{InFile.pos}' found instead: '{InFile.GetLine()}'");
        }

        public static void WriteIntAtPos(BinaryWriter outStream, int data, long location)
        {
            long placeholder = outStream.BaseStream.Position;
            outStream.Seek((int)location, SeekOrigin.Begin);
            outStream.Write((short)data);
            outStream.Seek((int)placeholder, SeekOrigin.Begin);
        }

        //For chunk length use Utils.WriteLength instead
        public static void WriteShortAtPos(BinaryWriter outStream, short data, long location)
        {
            long placeholder = outStream.BaseStream.Position;
            outStream.Seek((int)location, SeekOrigin.Begin);
            outStream.Write((short)data);
            outStream.Seek((int)placeholder, SeekOrigin.Begin);
        }
    }
}