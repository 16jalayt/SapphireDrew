using Sapphire_Extract_Helpers;
using System;
using System.IO;
using System.Text;

namespace HIFFCompile
{
    internal static class LongChunks
    {
        public static void ACTChunk(BinaryWriter outStream)
        {
            outStream.Write(Encoding.UTF8.GetBytes("ACT\0"));
            long actPlace = outStream.BaseStream.Position;
            outStream.Write((int)-1);

            outStream.Write(Encoding.UTF8.GetBytes("Scene Change with Hotspot".PadRight(48, '\0')));
            InFile.WriteString(outStream, "char[48]", 48);

            InFile.WriteObject(outStream, "byte", Enums.ACT_Type);

            //Exec type
            InFile.WriteObject(outStream, "byte");

            //RefScene
            InFile.WriteObject(outStream, "int");
        }

        public static void TSUMChunk(BinaryWriter outStream)
        {
            outStream.Write(Encoding.UTF8.GetBytes("SCENTSUM"));
            long scenPlace = outStream.BaseStream.Position;
            outStream.Write((int)-1);

            //Scene description
            InFile.WriteString(outStream, "char[50]", 50);
            //Background file without extension
            InFile.WriteString(outStream, "RefAVF", 33);
            //Background sound
            InFile.WriteString(outStream, "RefSound", 33);

            //Channel of backgound sound
            InFile.WriteObject(outStream, "int", Enums.soundChannel);
            //Loop background sound?
            InFile.WriteObject(outStream, "long", Enums.loop);
            //Left channel volume for background sound
            InFile.WriteObject(outStream, "int");
            //Right channel volume for background sound
            InFile.WriteObject(outStream, "int");

            Utils.WriteLength(outStream, scenPlace);
        }

        public static void USEChunk(BinaryWriter outStream)
        {
            outStream.Write(Encoding.UTF8.GetBytes("USE\0"));
            long usePlace = outStream.BaseStream.Position;
            outStream.Write((int)-1);

            //numDeps placeholder
            long numDepsPlace = outStream.BaseStream.Position;
            outStream.Write((short)-1);

            if (InFile.GetNextLine() != "BeginCount RefHif")
            {
                throw new Exception($"Unknown use contents: '{InFile.GetLine()}'. Should be: 'BeginCount RefHif'");
            }

            //TODO: EOF guard?
            //TODO: Use peek next to fix immediate issue?
            short numDeps = 0;
            while (InFile.GetNextLine() != "EndCount RefHif")
            {
                //Must be immediate string becausse we already advanced the line pointer with the loop
                InFile.WriteImmediateString(outStream, "RefHif", 33);
                numDeps++;
            }

            Utils.WriteShortAtPos(outStream, numDeps, numDepsPlace);

            Utils.WriteLength(outStream, usePlace);
        }
    }
}