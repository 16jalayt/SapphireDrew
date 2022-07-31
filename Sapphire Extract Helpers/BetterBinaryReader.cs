using System;
using System.IO;
using System.Text;

namespace Sapphire_Extract_Helpers
{
    public class BetterBinaryReader
    {
        private FileStream _fs;
        private BinaryReader _br;
        public string FileName { get; }
        public string FileNameWithoutExtension { get; }
        public string FileExtension { get; }
        public string FilePath { get; }

        public bool debugprint = false;

        public BetterBinaryReader(string filePath)
        {
            FilePath = Path.GetFullPath(@filePath);
            FileNameWithoutExtension = Path.GetFileNameWithoutExtension(FilePath);
            FileExtension = Path.GetExtension(FilePath);
            FileName = Path.GetFileName(FilePath);
            _fs = new FileStream(@filePath, FileMode.Open);
            _br = new BinaryReader(_fs, Encoding.Default);
        }

        public void Seek(long pos)
        {
            _br.BaseStream.Seek(pos, SeekOrigin.Begin);
        }

        public void Seek(long pos, SeekOrigin origin)
        {
            _br.BaseStream.Seek(pos, origin);
        }

        public long Length()
        {
            return _fs.Length;
        }

        public long Skip(long offset)
        {
            //return current pos
            return _br.BaseStream.Seek(offset, SeekOrigin.Current);
        }

        public long Position()
        {
            return _br.BaseStream.Position;
        }

        public int ReadInt(string msg = "")
        {
            int data = _br.ReadInt32();
            print(msg, data.ToString());
            return data;
        }

        public short ReadShort(string msg = "")
        {
            short data = _br.ReadInt16();
            print(msg, data.ToString());
            return data;
        }

        public byte[] ReadBytes(int len, string msg = "")
        {
            byte[] data = _br.ReadBytes(len);
            print(msg, data.ToString());
            return data;
        }

        public byte ReadByte(string msg = "")
        {
            byte data = _br.ReadByte();
            print(msg, data.ToString());
            return data;
        }

        //Big Endian
        public int ReadIntBE(string msg = "")
        {
            byte[] data = ReadBytes(4);
            Array.Reverse(data);
            int dataout = BitConverter.ToInt32(data, 0);
            print(msg, dataout.ToString());
            return dataout;
        }

        public short ReadShortBE(string msg = "")
        {
            byte[] data = ReadBytes(2);
            Array.Reverse(data);
            short dataout = BitConverter.ToInt16(data, 0);
            print(msg, dataout.ToString());
            return dataout;
        }

        public byte[] ReadBytesBE(int len, string msg = "")
        {
            byte[] data = ReadBytes(len);
            Array.Reverse(data);
            print(msg, data.ToString());
            return data;
        }

        public bool IsEOF()
        {
            return this.Position() >= this.Length() ? true : false;
        }

        public Stream GetStream()
        {
            return _br.BaseStream;
        }

        private void print(string msg, string data)
        {
            if (msg.Length != 0 && debugprint)
                Console.WriteLine(msg + data);
        }

        public void Dispose()
        {
            _br.Dispose();
            _fs.Dispose();
        }
    }
}