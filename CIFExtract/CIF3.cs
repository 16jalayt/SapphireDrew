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
            //Console.WriteLine($"CURRENTLY BROKEN\n");
            //Console.WriteLine($"EXPEREMENTAL\n");
            Console.WriteLine($"UNVALIDATED\n");

            //Seek past header and version. Already validated.
            InStream.Seek(28);

            //Only way to tell if tree or chunk is to check for a first chunk header.
            string header2 = Helpers.String(InStream.ReadBytes(24)).Trim('\0');
            if (header2 != "CIF FILE HerInteractive")
            {
                //Must be a loose cif file. Have to get name from filename and is 0 offset in file.
                ParseChunk(InStream, InStream.FileName, 0);
                return;
            }

            //Name table is at end of file.
            //Skip to last byte in file.
            InStream.Seek(InStream.Length() - 4);
            //Last int in file is the pointer to the start of the name table.
            int start = InStream.ReadInt();
            //seek backwards by previous int, adding in the pointer integer length
            InStream.Seek(InStream.Length() - (4 + start));

            //Number of name table entries
            int count = InStream.ReadInt();
            Console.WriteLine($"Extracting '{count}' files.");

            long placeholder = InStream.Position();

            //Check for venice. Seek to end of normal name to see if 0, if not then longer name.
            //The most notable change between cif revisions is name field length. The other being graphics format.
            int nameLength = 64;
            InStream.Skip(69);//nice
            //val should either ber first char of next name or last char of next name (venice. last char should be blank).
            byte test = InStream.ReadByte();
            if (test == 0)
                nameLength = 33;

            //Redundant. set in loop. Here for clarity
            //InStream.Seek(placeholder);

            //Done testing name length. Now parse name table.
            for (int i = 0; i < count; i++)
            {
                InStream.Seek(placeholder);
                string fileName = Helpers.String(InStream.ReadBytes(nameLength)).Trim('\0');
                //Console.WriteLine(fileName);
                long filePointer = InStream.ReadInt();
                placeholder = InStream.Position();
                ParseChunk(InStream, fileName, filePointer);
            }
        }

        private static void ParseChunk(BetterBinaryReader InStream, string FileName, long FilePointer)
        {
            InStream.Seek(FilePointer);
            //CIF FILE HerInteractive
            InStream.Skip(24);

            //Should be version 3 if ciftree is also 3.
            Helpers.AssertInt(InStream, 3);

            string FileExtension = GetExtension(InStream);

            //null padded
            InStream.Skip(12);

            int FileLength = InStream.ReadInt();

            byte[] FileContents = InStream.ReadBytes(FileLength);

            if (FilePointer == 0)
                FileName = Path.GetFileNameWithoutExtension(FileName);

            Console.WriteLine($"Extracting: {FileName}{FileExtension}");

            string outName = FileName + FileExtension;
            string outRaw;
            //If cif file outside of tree
            if (FilePointer == 0)
                outRaw = Helpers.Write(InStream.FilePath, outName, FileContents, false);
            else
                outRaw = Helpers.Write(InStream.FilePath, outName, FileContents);

            //IMPORTANT: test with single file

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

        private static string GetExtension(BetterBinaryReader InStream)
        {
            int FileType = InStream.ReadInt();

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