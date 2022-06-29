namespace Sapphire_Extract_Helpers
{
    public class NancyRect
    {
        public int p1x = 0;
        public int p1y = 0;
        public int p2x = 0;
        public int p2y = 0;

        public NancyRect(int p1x, int p1y, int p2x, int p2y)
        {
            this.p1x = p1x;
            this.p1y = p1y;
            this.p2x = p2x;
            this.p2y = p2y;
        }

        public NancyRect(BetterBinaryReader InStream, bool BigEndian = true)
        {
            if (BigEndian)
            {
                this.p1x = InStream.ReadIntBE();
                this.p1y = InStream.ReadIntBE();
                this.p2x = InStream.ReadIntBE();
                this.p2y = InStream.ReadIntBE();
            }
            //Little Endian
            else
            {
                this.p1x = InStream.ReadInt();
                this.p1y = InStream.ReadInt();
                this.p2x = InStream.ReadInt();
                this.p2y = InStream.ReadInt();
            }
        }

        public override string ToString()
        {
            return $"NancyRect: {p1x}, {p1y}, {p2x}, {p2y}";
        }
    }
}
