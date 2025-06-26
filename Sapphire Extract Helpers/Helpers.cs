using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sapphire_Extract_Helpers
{
    public static class Helpers
    {
        public static bool Raw;

        //TODO: pass multiple possible values. Helper to itter and check returns?
        //TODO: better debug messages(pass guessed value to print?)

        /// <summary>
        /// Read a byte array and print if not equal.
        /// </summary>
        /// <param name="InStream"></param>
        /// <param name="val"></param>
        /// <returns>Truth</returns>
        public static bool AssertValue(BetterBinaryReader InStream, byte[] val)
        {
            byte[] readValues = InStream.ReadBytes(val.Length);
            if (!Equal(readValues, val))
            {
                //TODO:figure out better output. prints int
                Console.WriteLine($"Value in file {InStream.FileName} at position '{InStream.Position()}'...");
                Console.WriteLine($"Expected value '{Hex(val)}' got '{Hex(readValues)}'");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Read an byte and print if not equal.
        /// </summary>
        /// <param name="InStream"></param>
        /// <param name="val"></param>
        /// <returns>Truth</returns>
        public static bool AssertByte(BetterBinaryReader InStream, int val)
        {
            byte readValue = InStream.ReadByte();
            if (readValue != val)
            {
                //TODO:figure out better output. prints int
                Console.WriteLine($"Value in file {InStream.FileName} at position '{InStream.Position()}'...");
                Console.WriteLine($"Expected value '{val}' got '{readValue}'");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Read an int and print if not equal.
        /// </summary>
        /// <param name="InStream"></param>
        /// <param name="val"></param>
        /// <returns>Truth</returns>
        public static bool AssertInt(BetterBinaryReader InStream, int val, bool LittleEndian = true)
        {
            int readValue = 0;
            if (LittleEndian)
                readValue = InStream.ReadInt();
            else
                readValue = InStream.ReadInt();

            if (readValue != val)
            {
                //TODO:figure out better output. prints int
                Console.WriteLine($"Value in file {InStream.FileName} at position '{InStream.Position() - 4}'...");
                Console.WriteLine($"Expected value '{val}' got '{readValue}'");
                return false;
            }
            return true;
        }

        public static bool AssertIntBE(BetterBinaryReader InStream, short val)
        {
            return AssertInt(InStream, val, false);
        }

        /// <summary>
        /// Read a short and print if not equal.
        /// </summary>
        /// <param name="InStream"></param>
        /// <param name="val"></param>
        /// <returns>Truth</returns>
        public static bool AssertShort(BetterBinaryReader InStream, short val, bool LittleEndian = true)
        {
            short readValue = 0;
            if (LittleEndian)
                readValue = InStream.ReadShort();
            else
                readValue = InStream.ReadShortBE();

            if (readValue != val)
            {
                //TODO:figure out better output. prints int
                Console.WriteLine($"Value in file {InStream.FileName} at position '{InStream.Position() - 2}'...");
                Console.WriteLine($"Expected value '{val}' got '{readValue}'");
                return false;
            }
            return true;
        }

        public static bool AssertShortBE(BetterBinaryReader InStream, short val)
        {
            return AssertShort(InStream, val, false);
        }

        /// <summary>
        /// Read a string and print if not equal.
        /// </summary>
        /// <param name="InStream"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static bool AssertString(BetterBinaryReader InStream, string val, bool TrimEnd = false)
        {
            long position = InStream.Position();
            string readValues = String(InStream.ReadBytes(val.Length));
            if (TrimEnd == true)
                readValues = readValues.TrimEnd();
            //string readValues = String(InStream.ReadBytes(val.Length));
            //Log.Warning(readValues);
            if (readValues != val)
            {
                Console.WriteLine($"Value in file {InStream.FileName} at position '{position}'...");
                Console.WriteLine($"Expected value '{val}' got '{readValues}'");
                return false;
            }
            else
                return true;
        }

        //same as assert string but with reset steam.
        public static bool AssertHeader(BetterBinaryReader InStream, string val)
        {
            InStream.Seek(0);
            string readValues = String(InStream.ReadBytes(val.Length));
            //string readValues = String(InStream.ReadBytes(val.Length));
            //Log.Warning(readValues);
            if (readValues != val)
            {
                Console.WriteLine($"Value in file {InStream.FileName} at position '{InStream.Position()}'...");
                Console.WriteLine($"Expected value '{val}' got '{readValues}'");
                return false;
            }
            else
                return true;
        }

        //may not use
        public static void AssertValueAbort(byte[] val)
        {
        }

        /// <summary>
        /// Write a byte array to a specified file.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fileName"></param>
        /// <param name="fileContents"></param>
        /// <param name="subdir"></param>
        public static string Write(string filePath, string fileName, byte[] fileContents, bool subdir = true)
        {
            //It means to interpret the string literally
            //@"\\servername\share\folder"
            //is nicer than this:
            //"\\\\servername\\share\\folder"
            return Writer.WriteFile(@filePath, fileName, fileContents, subdir);
        }

        public static void setOverwriteAll(bool val)
        {
            Writer.OverwriteAll = val;
        }

        public static void setAutoRename(bool val)
        {
            Writer.AutoRename = val;
        }

        public static void setRaw(bool val)
        {
            Raw = val;
        }

        public static string Hex(byte[] inArray)
        {
            return BitConverter.ToString(inArray).Replace("-", ", ");
        }

        public static string String(byte[] inArray)
        {
            return System.Text.Encoding.UTF8.GetString(inArray);
        }

        //https://stackoverflow.com/questions/18472867/checking-equality-for-two-byte-arrays/18472958
        public static bool Equal(byte[] a1, byte[] b1)
        {
            // If not same length, done
            if (a1.Length != b1.Length)
            {
                return false;
            }

            // If they are the same object, done
            if (object.ReferenceEquals(a1, b1))
            {
                return true;
            }

            // Loop all values and compare
            for (int i = 0; i < a1.Length; i++)
            {
                if (a1[i] != b1[i])
                {
                    return false;
                }
            }

            // If we got here, equal
            return true;
        }

        public static int ValidateGameNum(string argnum)
        {
            //TODO: replace with tryparseint?
            int gamenum = Int32.Parse(argnum);

            if (gamenum < 1 || gamenum > 33)
            {
                Console.WriteLine("Invalid game number. Please enter a number between 0 and 32.");
                Environment.Exit(20);
            }
            if (gamenum == 33)
            {
                Console.WriteLine("Midnight in Salem uses Unity. This is not supported.");
                Environment.Exit(21);
            }
            else if (gamenum == 34)
            {
                Console.WriteLine("Myster of the Seven Keys uses Unity. This is not supported.");
                Environment.Exit(22);
            }

            return gamenum;
        }

        public static void printStringArray(string[] arr)
        {
            Console.WriteLine("[{0}]", string.Join(", ", arr));
        }

        public static void PopulateFlags(string? flagsFileName, bool verbose = false)
        {
            if (flagsFileName != null)
            {
                if (!File.Exists(flagsFileName))
                {
                    throw new Exception($"The flags file '{flagsFileName}' does not exist");
                }

                Console.WriteLine("Parsing Flags.hif");
                BetterBinaryReader InStream = new BetterBinaryReader(flagsFileName);

                //TODO: parse htxt
                //Ensure that is actually a flags.hiff
                if (!Helpers.AssertString(InStream, "DATA"))
                {
                    throw new Exception($"The flags file specified: '{flagsFileName}' is not a valid flags.hiff file");
                }

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

                    if (num < 100)
                    {
                        if (!Enums.inv.TryAdd(num, flagName))
                            Console.WriteLine($"Duplicate flag for '{flagName}' - '{num}'");
                    }
                    else
                    {
                        //At least in WOLF, dupes seem common
                        if (!Enums.flags.TryAdd(num, flagName))
                            Console.WriteLine($"Duplicate flag for '{flagName}' - '{num}'");
                    }
                }

                int flagNum = 1010;
                for (int i = 0; i < 51; i++)
                {
                    if (verbose)
                        Console.WriteLine($"'{"EV_Generic" + i}' - '{flagNum}'");

                    if (!Enums.flags.TryAdd(flagNum, "EV_Generic" + i))
                        Console.WriteLine($"Duplicate flag for '{"EV_Generic" + i}' - '{flagNum}'");

                    flagNum++;
                }

                Enums.flagsReverse = Enums.flags.ToDictionary(x => x.Value, x => x.Key);
                Enums.invReverse = Enums.inv.ToDictionary(x => x.Value, x => x.Key);
                InStream.Dispose();
            }
        }

        public static string GetFlagName(int num)
        {
            string? properName;
            if (Enums.flags.Count != 0 && Enums.flags.TryGetValue(num, out properName))
                return properName;
            else
                return num.ToString();
        }

        public static string GetInvName(int num)
        {
            string? properName;
            if (Enums.inv.Count != 0 && Enums.inv.TryGetValue(num, out properName))
                return properName;
            else
                return num.ToString();
        }
    }
}