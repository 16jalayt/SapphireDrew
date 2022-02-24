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
        public string FilePath { get; }

        public BetterBinaryReader(string filePath)
        {
            FilePath = Path.GetFullPath(@filePath);
            FileNameWithoutExtension = Path.GetFileNameWithoutExtension(FilePath);
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

        public int ReadInt()
        {
            return _br.ReadInt32();
        }

        public short ReadShort()
        {
            return _br.ReadInt16();
        }

        public byte[] ReadBytes(int len)
        {
            return _br.ReadBytes(len);
        }
        public byte ReadByte()
        {
            return _br.ReadByte();
        }

        public void Dispose()
        {
            _br.Dispose();
            _fs.Dispose();
        }
    }
}