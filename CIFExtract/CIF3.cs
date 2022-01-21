using Sapphire_Extract_Helpers;
using System;
using System.Diagnostics;
using System.IO;

namespace CIFExtract
{
    internal class CIF3
    {
        //Variables should be unchanged for rest of cif. Will be set on first run. Program should only run with one cif file.
        private static Boolean Venice = false;

        private static Boolean Untested = true;

        public static void Extract(BetterBinaryReader InStream)
        {
            //Tool status
            Console.WriteLine($"CURRENTLY BROKEN\n");
            //Console.WriteLine($"EXPEREMENTAL\n");
            //Console.WriteLine($"UNVALIDATED\n");

            string header = Helpers.String(InStream.ReadBytes(24)).Trim('\0');
            //Console.WriteLine($"header: {header}");

            //If the file has wrong id, say we can't extract
            //CIF files and trees are same structure, just one or many files.
            if (header != "CIF FILE HerInteractive" && header != "CIF TREE HerInteractive" && header != "CIF TREE WayneSikes" && header != "CIF FILE WayneSikes")
            {
                Console.WriteLine($"The file: '{InStream.FileName}' has an invalid header.");
                return;
            }

            short verMajor = InStream.ReadShort();
            short verMinor = InStream.ReadShort();
            Console.WriteLine($"CIFF version: {verMajor}.{verMinor}\n");

            //Seek past header and version. Already validated.
            InStream.Seek(28);

            ////////////////////////////
            if (verMajor == 3)
            {
                //Only way to tell if tree or chunk is to check for a first chunk header.
                string header2 = Helpers.String(InStream.ReadBytes(24)).Trim('\0');
                if (header2 != "CIF FILE HerInteractive")
                {
                    //Must be a loose cif file. Have to get name from filename and is 0 offset in file.
                    ParseChunk(InStream, Path.GetFileNameWithoutExtension(InStream.FileName), 0);
                    return;
                }
            }
            else if (header == "CIF FILE WayneSikes")
            {
                ParseChunk(InStream, InStream.FileName, 0);
            }
            //else must be a tree

            //////////////////////////////

            //seek to start of name table
            if (verMajor == 3)
            {
                //Name table is at end of file.
                //Skip to last byte in file.
                InStream.Seek(InStream.Length() - 4);
                //Last int in file is the pointer to the start of the name table.
                int start = InStream.ReadInt();
                //seek backwards by previous int, adding in the pointer integer length
                InStream.Seek(InStream.Length() - (4 + start));
            }

            //Number of name table entries
            int count;
            //only sck is a short
            if (verMajor == 2 && verMinor == 0)
                count = InStream.ReadShort();
            else
                count = InStream.ReadInt();

            Console.WriteLine($"Extracting '{count}' files.");

            //ver 2 - 2.3
            if (verMajor != 3)
            {
                //2k of some sort of hash table.
                InStream.Skip(2048);
            }

            //TODO: possible efficiency increase if parse to list and read files sequentially

            int nameLength = -1;
            //Technically saving pos here is unnessesary, just needs to be defined out of loop
            long placeholder = InStream.Position();

            //LONG section doing tests for name length, as can change randomly

            ////////////////////////////////
            if (verMajor == 2 && verMinor == 0)
                nameLength = 9;
            else if (verMajor == 2 && verMinor == 1)
            {
                //Name length was different in game 2
                //Seek to name field about 3 files in to see if empty or a number
                InStream.Skip(149);
                byte test = InStream.ReadByte();
                //if any game but dan
                if (test == 0)
                    nameLength = 33;
                //else dan
                else
                    nameLength = 9;
                //Redundant. set in loop. Here for clarity
                //InStream.Seek(placeholder);
            }
            else if (verMajor == 2 && verMinor == 2)
                nameLength = 33;
            else if (verMajor == 2 && verMinor == 3)
                nameLength = 33;
            else if (verMajor == 3)
            {
                //Check for venice. Seek to end of normal name to see if 0, if not then longer name.
                //The most notable change between cif revisions is name field length. The other being graphics format.
                InStream.Skip(69);//nice
                                  //val should either ber first char of next name or last char of next name (venice. last char should be blank).
                byte test = InStream.ReadByte();
                if (test == 0)
                    nameLength = 33;
                else
                    nameLength = 64;

                //Redundant. set in loop. Here for clarity
                //InStream.Seek(placeholder);
            }
            //////////////////////////////////

            //Done testing name length. Now parse name table.
            for (int i = 0; i < count; i++)
            {
                InStream.Seek(placeholder);
                string fileName = Helpers.String(InStream.ReadBytes(nameLength)).Trim('\0');
                long filePointer = 0;
                if (verMajor == 3)
                {
                    filePointer = InStream.ReadInt();
                    placeholder = InStream.Position();
                    ParseChunk(InStream, fileName, filePointer);
                }
                //3 only has name and pointer
                else if (verMajor != 3)
                {
                    //TODO: sck
                    short FileIndex = InStream.ReadShort();

                    //attempt to read offset. 0 man, off new
                    filePointer = InStream.ReadInt();
                    bool older = false;
                    //if early 2.1
                    if (filePointer != 0)
                    {
                        //Unknown FF
                        _ = InStream.ReadShort();

                        InStream.Skip(8);
                        older = true;
                    }
                    else
                        InStream.Skip(4);

                    //For TGA header
                    int XOrigin = InStream.ReadInt();
                    int YOrigin = InStream.ReadInt();

                    int XStart = InStream.ReadInt();
                    int YStart = InStream.ReadInt();
                    int XEnd = InStream.ReadInt();
                    int YEnd = InStream.ReadInt();

                    short width = InStream.ReadShort();
                    //Unknown
                    _ = InStream.ReadShort();
                    short height = InStream.ReadShort();

                    //Unknown
                    _ = InStream.ReadShort();

                    //If early 2.1
                    if (filePointer == 0)
                        filePointer = InStream.ReadInt();

                    //Length of final file
                    int DecompressedLength = InStream.ReadInt();
                    //Unknown
                    _ = InStream.ReadInt();
                    //Length of file in tree
                    int CompressedLength = InStream.ReadInt();
                    //Is file data or picture
                    byte FileType = InStream.ReadByte();

                    //Unknown
                    if (older)
                        _ = InStream.ReadShort();

                    placeholder = InStream.Position();
                    ParseChunk(InStream, fileName, filePointer);
                }
            }
        }

        //CURRENTLY v3 specific
        private static void ParseChunk(BetterBinaryReader InStream, string FileName, long FilePointer)
        {
            InStream.Seek(FilePointer);

            if (verMajor == 3)
            {
                //CIF FILE HerInteractive
                InStream.Skip(24);

                //Should be version 3 if ciftree is also 3.
                Helpers.AssertInt(InStream, 3);

                int FileType = InStream.ReadInt();
                string FileExtension = GetExtension(InStream, FileType);

                //null padded
                InStream.Skip(12);
            }

            int FileLength = InStream.ReadInt();

            byte[] FileContents = InStream.ReadBytes(FileLength);

            Console.WriteLine($"Extracting: {FileName}{FileExtension}");

            string outName = FileName + FileExtension;
            string outRaw;
            //If cif file outside of tree
            if (FilePointer == 0)
                outRaw = Helpers.Write(InStream.FilePath, outName, FileContents, false);
            else
                outRaw = Helpers.Write(InStream.FilePath, outName, FileContents);

            //LUAC
            if (FileExtension == ".luac")
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.CreateNoWindow = false;
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;
                startInfo.FileName = "unluac.exe";
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                //put quotes around path in argument
                startInfo.Arguments = $"\"{outRaw}\"";

                try
                {
                    // Start the process with the info we specified.
                    // Call WaitForExit and then the using statement will close.
                    using (Process process = Process.Start(startInfo))
                    {
                        var outputStream = new StreamWriter(Path.Combine(Path.GetDirectoryName(outRaw), FileName + ".lua"));
                        //File.WriteLine("test.txt", exeProcess.StandardOutput.ReadToEnd());
                        process.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
                        {
                            if (!String.IsNullOrEmpty(e.Data))
                            {
                                outputStream.WriteLine(e.Data);
                            }
                        });

                        process.Start();

                        process.BeginOutputReadLine();

                        process.WaitForExit();
                        process.Close();

                        outputStream.Close();

                        File.Delete(outRaw);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to launch unluac:\n" + e);
                }
            }
        }

        private static string GetExtension(BetterBinaryReader InStream, int FileType)
        {
            /*
		 * Chunk types are officially documented as:
		 * PLAIN --  image that IS NOT used as a transparent overlay
	     * DECAL --  image that IS used as a transparent overlay
	     * DATA  --  non-image data such as the Cif Listing file.
		 */

            //2 is an OVL file or overlay. A sprite that goes on top of a scene. Usually a pickup or interactable.
            if (FileType == 2)
                //For png there are x and y size. everything else 0. This is presumably an engine convenience
                return ".png";
            else if (FileType == 3)
            {
                if (Untested)
                {
                    long Pos = InStream.Position();
                    InStream.Skip(16);

                    if (Helpers.String(InStream.ReadBytes(4)) == "DATA")
                        Venice = true;
                    Untested = false;
                    InStream.Seek(Pos);
                }

                if (Venice)
                    return ".hiff";
                else
                    return ".luac";
            }
            //ERRATA: extension is officially .xs1. I have modified this to differentiate compiled vs uncompiled.
            //The XSheetDecompile will name the resultant file .xs1
            else if (FileType == 6)
                return ".xsheet";
            else
            {
                Console.WriteLine($"Unknown file type {FileType}. Please report game this occured in.");
                return ".unk";
            }
        }
    }
}