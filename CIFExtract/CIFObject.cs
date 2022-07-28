namespace CIFExtract
{
    //TODO: fully populate structure.
    internal class CIFObject
    {
        public string fileName = "";
        public long filePointer = 0;
        public short FileIndex = -1;

        public int DecompressedLength = 0;
        public int CompressedLength = 0;
        public byte FileType = 0;
        public string fileExtension = ".unk";

        public int XOrigin = 0;
        public int YOrigin = 0;
        public int XStart = 0;
        public int YStart = 0;
        public int XEnd = 0;
        public int YEnd = 0;

        public int width = 0;
        public int height = 0;

        public byte[] compressed = null;
        public byte[] contents = null;
    }
}