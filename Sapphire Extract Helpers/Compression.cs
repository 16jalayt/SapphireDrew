using System.Collections.Generic;
using System.Linq;

namespace Sapphire_Extract_Helpers
{
    public static class Compression
    {
        /*
            * Copyright 2008-2013, David Karnok
            * The file is part of the Open Imperium Galactica project.
            *
            * The code should be distributed under the LGPL license.
            * See http://www.gnu.org/licenses/lgpl.html for details.
            */

        /**
         * Translated from Java
         * Decompress the given byte array using the LZSS algorithm and
         * produce the output into the given out array.
         */

        public static byte[]? decompressLZSS(byte[]? data)
        {
            if (data == null)
                return null;

            List<byte> outdata = [];
            int src = 0;
            int nextChar = 0xFEE;
            const int windowSize = 4096;
            byte[] slidingWindow = Enumerable.Repeat((byte)0x20, windowSize).ToArray();

            while (src < data.Length)
            {
                int marker = data[src++] & 0xFF;
                for (int i = 0; i < 8 && src < data.Length; i++)
                {
                    bool type = (marker & (1 << i)) != 0;
                    if (type)
                    {
                        byte d = data[src++];
                        outdata.Add(d);
                        slidingWindow[nextChar] = d;
                        nextChar = (nextChar + 1) % windowSize;
                    }
                    else
                    {
                        int offset = data[src++] & 0xFF;
                        int len = data[src++] & 0xFF;
                        offset |= (len & 0xF0) << 4;
                        len = (len & 0x0F) + 3;
                        for (int j = 0; j < len; j++)
                        {
                            byte d = slidingWindow[(offset + j) % windowSize];
                            outdata.Add(d);
                            slidingWindow[nextChar] = d;
                            nextChar = (nextChar + 1) % windowSize;
                        }
                    }
                }
            }
            return outdata.ToArray();
        }

        /*
        //https://github.com/knight0fdragon/LZSSTest/blob/main/LZSS.cs

        private const int BufferSize = 4096;
        private const int DictionarySize = 34;

        public static byte[] Decompress(byte[] input)
        {
            List<byte> output = new List<byte>();
            const int THRESHOLD = 2;
            var text_buf = new byte[BufferSize];
            int inputIdx = 0;
            int bufferIdx = 0xFEE; //r
            byte c = 0;
            ushort flags;

            for (int i = 0; i < BufferSize - DictionarySize; i++)
                text_buf[i] = 0x20;

            flags = 0;
            for (; ; )
            {
                if (((flags >>= 1) & 0x100) == 0)
                {
                    if (inputIdx < input.Length) c = input[inputIdx++]; else break;
                    flags = (ushort)(c | 0xFF00);  // uses higher byte cleverly
                }   // to count eight
                if ((flags & 1) > 0)
                {
                    if (inputIdx < input.Length) c = input[inputIdx++]; else break;
                    output.Add(c);
                    text_buf[bufferIdx++] = c;
                    bufferIdx &= (BufferSize - 1);
                }
                else
                {
                    int i = 0;
                    int j = 0;
                    if (inputIdx < input.Length) i = input[inputIdx++]; else break;
                    if (inputIdx < input.Length) j = input[inputIdx++]; else break;
                    i |= ((j & 0xE0) << 3);
                    j = (j & 0x1F) + THRESHOLD;
                    for (int k = 0; k <= j; k++)
                    {
                        c = text_buf[(i + k) & (BufferSize - 1)];
                        output.Add(c);
                        text_buf[bufferIdx++] = c;
                        bufferIdx &= (BufferSize - 1);
                    }
                }
            }

            return output.ToArray();
        }*/
        /*
        //https://github.com/mmmati1996/LZSS/blob/master/LZSS/MainWindow.xaml.cs
        private static string Decompress_LZSS(string fulltext_input, int offset_size, int length_size)
        {
            try
            {
                DateTime date1 = DateTime.Now;
                string output = string.Empty;
                int offset_size_bit = (int)Math.Log(offset_size, 2);
                int length_size_bit = (int)Math.Log(length_size, 2);

                while (fulltext_input.Length > 0)
                {
                    if (fulltext_input[0] == '1')
                    {
                        output += Convert.ToChar(Convert.ToInt32(fulltext_input.Substring(1, 8), 2));
                        fulltext_input = fulltext_input.Substring(9);
                    }
                    else
                    {
                        int offset = Convert.ToInt32(fulltext_input.Substring(1, offset_size_bit), 2);
                        int length = Convert.ToInt32(fulltext_input.Substring(1 + offset_size_bit, length_size_bit), 2);
                        if (output.Length <= length_size)
                            output += output.Substring(offset, length + 1);
                        else
                        {
                            string lastchars = output.Substring(output.Length - length_size, length_size);
                            output += lastchars.Substring(offset, length + 1);
                        }
                        fulltext_input = fulltext_input.Substring(1 + length_size_bit + offset_size_bit);
                    }
                }
                DateTime date2 = DateTime.Now;
                TimeSpan span = date2 - date1;
                string time_formating = "" + span.Minutes + ":" + span.Seconds + ":" + span.Milliseconds;
                return output;
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return null;
            }
        }
        */
    }
}