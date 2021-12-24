using Sapphire_Extract_Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

//TODO: have cif ext call xsheet decompile

namespace XSheetCompile
{
    internal class Program
    {
        private static void Main(string[] args)
        {
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

            //If the file has wrong id, say we can't extract
            if (!Helpers.AssertString(InStream, "XS1"))
            {
                Console.WriteLine($"The file: '{FileName}' has an invalid header.\n");
                return;
            }
            InStream.Dispose();

            //since reading is out of order need variables to hold required data
            int fps; //currently unused
            String body = "";
            int[] bodyRect = new int[4];
            String head = "";
            int[] headRect = new int[4];
            //List<string> frameStore = new List<string>();
            Dictionary<int, int> frameStore = new Dictionary<int, int>();

            //IMPORTANT: lines can be in any order and duplicates will overwrite
            foreach (string line in File.ReadLines(FileName))
            {
                string[] parts = line.Split('=');

                if (parts.Length != 2)
                {
                    if (line == "XS1" || line == "[Options]" || line == "[Level 1]")
                        continue;
                    else
                    {
                        Console.WriteLine($"Unknown line: {line}");
                        continue;
                    }
                }

                // cant just do substring check because crash on unexpected
                if (parts[0].Length == 7 || parts[0].Length == 8 && parts[0].Substring(0, 6) == "FRAME ")
                {
                    int bodyFrame;
                    if (!Int32.TryParse(parts[1], out bodyFrame))
                    {
                        Console.WriteLine($"{parts[1]} is not a number.");
                        continue;
                    }

                    int frameNum;
                    if (!Int32.TryParse(parts[0].Substring(6), out frameNum))
                    {
                        Console.WriteLine($"{parts[0]} is not a number.");
                        continue;
                    }

                    frameStore.Add(frameNum, bodyFrame);
                }
                else if (parts[0] == "FPS")
                {
                    if (!Int32.TryParse(parts[1], out fps))
                        Console.WriteLine($"{parts[1]} is not a number.");
                }
                else if (parts[0] == "CalLevel 1")
                {
                    body = parts[1];
                }
                else if (parts[0] == "VidRect 1")
                {
                    string[] numbers = parts[1].Split(',');
                    if (numbers.Length < 4)
                    {
                        Console.WriteLine($"{parts[0]} not enough arguments.");
                        continue;
                    }
                    if (!Int32.TryParse(numbers[0], out bodyRect[0]))
                        Console.WriteLine($"{parts[1]} is not a number.");
                    if (!Int32.TryParse(numbers[1], out bodyRect[1]))
                        Console.WriteLine($"{parts[1]} is not a number.");
                    if (!Int32.TryParse(numbers[2], out bodyRect[2]))
                        Console.WriteLine($"{parts[1]} is not a number.");
                    if (!Int32.TryParse(numbers[3], out bodyRect[3]))
                        Console.WriteLine($"{parts[1]} is not a number.");
                }
                else if (parts[0] == "VidLevel 2")
                {
                    head = parts[1];
                }
                else if (parts[0] == "VidRect 2")
                {
                    string[] numbers = parts[1].Split(',');
                    if (numbers.Length < 4)
                    {
                        Console.WriteLine($"{parts[0]} not enough arguments.");
                        continue;
                    }
                    if (!Int32.TryParse(numbers[0], out headRect[0]))
                        Console.WriteLine($"{parts[1]} is not a number.");
                    if (!Int32.TryParse(numbers[1], out headRect[1]))
                        Console.WriteLine($"{parts[1]} is not a number.");
                    if (!Int32.TryParse(numbers[2], out headRect[2]))
                        Console.WriteLine($"{parts[1]} is not a number.");
                    if (!Int32.TryParse(numbers[3], out headRect[3]))
                        Console.WriteLine($"{parts[1]} is not a number.");
                }
                else
                    Console.WriteLine($"Unknown equate: {line}");
            }

            //everything read, now validate.
            if (body == "")
            {
                Console.WriteLine("CalLevel 1 is a required perameter.");
                System.Environment.Exit(-2);
            }
            else if (head == "")
            {
                Console.WriteLine("VidLevel 2 is a required perameter.");
                System.Environment.Exit(-3);
            }
            else if (frameStore.Count == 0)
            {
                Console.WriteLine("No frames were specified.");
                System.Environment.Exit(-4);
            }

            //Everything is in order. Time to write the output.
            FileInfo file = new FileInfo(Path.GetDirectoryName(FileName) + "/Output/" + Path.GetFileNameWithoutExtension(FileName) + ".xsheet");
            file.Directory.Create();
            BinaryWriter outStream = new BinaryWriter(new FileStream(file.FullName, FileMode.Create), Encoding.UTF8);
            //have to convert string to byte array to prevent string being prefixed by length automatically.
            outStream.Write(Encoding.UTF8.GetBytes("XSHEET HerInteractive"));
            //0 padding
            outStream.Write(new byte[9]);
            outStream.Write(2);
            outStream.Write((short)frameStore.Count);
            outStream.Write((short)2);
            outStream.Write(Encoding.UTF8.GetBytes(body.ToUpper()));
            outStream.Write(new byte[33 - body.Length]);
            outStream.Write(Encoding.UTF8.GetBytes(head.ToUpper()));
            outStream.Write(new byte[99 - head.Length]);
            outStream.Write(bodyRect[0]);
            outStream.Write(bodyRect[1]);
            outStream.Write(bodyRect[2]);
            outStream.Write(bodyRect[3]);
            outStream.Write(headRect[0]);
            outStream.Write(headRect[1]);
            outStream.Write(headRect[2]);
            outStream.Write(headRect[3]);
            outStream.Write(new byte[32]);
            outStream.Write(66);
            for (int i = 1; i < frameStore.Count + 1; i++)
            {
                outStream.Write(frameStore.GetValueOrDefault(i));
                outStream.Write(-1);
                outStream.Write(new byte[16]);
            }

            outStream.Close();
        }
    }
}