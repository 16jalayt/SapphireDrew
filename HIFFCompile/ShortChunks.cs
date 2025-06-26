using Sapphire_Extract_Helpers;
using System;
using System.IO;
using System.Text;

namespace HIFFCompile
{
    internal static class ShortChunks
    {
        public static void OVLChunk(BinaryWriter outStream)
        {
            outStream.Write(Encoding.UTF8.GetBytes("ACT\0"));
            long actPlace = outStream.BaseStream.Position;
            outStream.Write((int)-1);

            outStream.Write(Encoding.UTF8.GetBytes("Static Overlay Image".PadRight(48, '\0')));

            outStream.Write((byte)Enums.ACT_TypeReverse["AT_OVERLAY"]);

            //Exec type: AE_SINGLE_EXEC
            outStream.Write((byte)1);

            Utils.ParseDepsShort(outStream);

            outStream.Write(Encoding.UTF8.GetBytes(InFile.GetNextToken().PadRight(33, '\0')));
            //VIEWPORT_OVERLAY1_Z
            outStream.Write((short)10);

            //BeginCount    int
            outStream.Write((short)1);

            //Scene frame to show this ovl in
            outStream.Write((short)0);

            int rect;
            int.TryParse(InFile.GetNextToken(), out rect);
            int rect2;
            int.TryParse(InFile.GetNextToken(), out rect2);
            int rect3;
            int.TryParse(InFile.GetNextToken(), out rect3);
            int rect4;
            int.TryParse(InFile.GetNextToken(), out rect4);

            //src: Assume starting top left of image
            outStream.Write((int)1);
            outStream.Write((int)1);
            outStream.Write((int)rect3 - 2);
            outStream.Write((int)rect4 - 2);

            //dest: Assume starting top left of image
            outStream.Write((int)rect);
            outStream.Write((int)rect2);
            outStream.Write((int)(rect + rect3) - 2);
            outStream.Write((int)(rect2 + rect4) - 2);

            int actLength = Utils.WriteLength(outStream, actPlace);
        }

        public static void FLAGChunk(BinaryWriter outStream)
        {
            //TODO: make util for act header
            outStream.Write(Encoding.UTF8.GetBytes("ACT\0"));
            long actPlace = outStream.BaseStream.Position;
            outStream.Write((int)-1);

            outStream.Write(Encoding.UTF8.GetBytes("Event Flags with Cursor and HS".PadRight(48, '\0')));

            outStream.Write((byte)Enums.ACT_TypeReverse["AT_FLAGS_HS"]);

            //Exec type: AE_SINGLE_EXEC
            outStream.Write((byte)1);

            Utils.ParseDepsShort(outStream);

            //numFlags placeholder
            long numFlagsPlace = outStream.BaseStream.Position;
            outStream.Write((short)-1);
            short numFlags = 0;

            //Currently unknown purpose
            string unk = InFile.GetNextToken();

            //Frame to change to
            outStream.Write((byte)0);

            //cursor
            int cursor = Enums.cursorDictReverse[InFile.GetNextToken()];
            outStream.Write((byte)cursor);

            //TODO: helper with error handling
            //rect
            int rect;
            int.TryParse(InFile.GetNextToken(), out rect);
            int rect2;
            int.TryParse(InFile.GetNextToken(), out rect2);
            int rect3;
            int.TryParse(InFile.GetNextToken(), out rect3);
            int rect4;
            int.TryParse(InFile.GetNextToken(), out rect4);

            while (InFile.GetNextToken() != "if")
            {
                //We consumed in our check so have to reset
                InFile.tokenPos--;
                //Flag to set
                InFile.WriteTokenObject(outStream, "RefSetFlag", Enums.flagsReverse);

                //Flag conditional
                InFile.WriteTokenObject(outStream, "int", Enums.tf);
                numFlags++;
            }

            //Write the number of flags back at the placeholder
            long endFlags = outStream.BaseStream.Position;
            outStream.Seek((int)numFlagsPlace, SeekOrigin.Begin);
            outStream.Write((short)numFlags);
            outStream.Seek((int)endFlags, SeekOrigin.Begin);

            //print rect ints end
            //need to make util and convert string to int
            outStream.Write((int)rect);
            outStream.Write((int)rect2);
            outStream.Write((int)rect3);
            outStream.Write((int)rect4);

            int actLength = Utils.WriteLength(outStream, actPlace);
        }

        public static void USEChunk(BinaryWriter outStream)
        {
            outStream.Write(Encoding.UTF8.GetBytes("USE\0"));
            long usePlace = outStream.BaseStream.Position;
            outStream.Write((int)-1);

            //numDeps placeholder
            long numDepsPlace = outStream.BaseStream.Position;
            outStream.Write((short)-1);

            short numDeps = 0;
            //USE token is consumed by switch looking for chunk
            while (InFile.HasNextToken())
            {
                //TODO: wrap and simplify
                outStream.Write(Encoding.UTF8.GetBytes(InFile.GetNextToken().PadRight(33, '\0')));
                numDeps++;
            }

            //TODO: Convert to util?
            long endDeps = outStream.BaseStream.Position;
            outStream.Seek((int)numDepsPlace, SeekOrigin.Begin);
            outStream.Write(numDeps);
            outStream.Seek((int)endDeps, SeekOrigin.Begin);

            Utils.WriteLength(outStream, usePlace);
        }
    }
}