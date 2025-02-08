namespace SUFcoTool
{
    public struct ConverseHeaderPackage
    {
        public int Field00;
        public int Field04;
        public static ConverseHeaderPackage Read(BinaryReader in_BinaryReader)
        {
            ConverseHeaderPackage header = new ConverseHeaderPackage();
            header.Field00 = in_BinaryReader.ReadInt32();
            header.Field04 = in_BinaryReader.ReadInt32();
            return header;
        }
    }
}