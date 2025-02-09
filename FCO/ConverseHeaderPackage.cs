
namespace SUFcoTool
{
    public struct ConverseHeaderPackage
    {
        public int Field00;
        public int Field04;
        public static ConverseHeaderPackage Read(BinaryReader in_BinaryReader)
        {
            ConverseHeaderPackage header = new ConverseHeaderPackage();
            header.Field00 = Common.EndianSwap(in_BinaryReader.ReadInt32());
            header.Field04 = Common.EndianSwap(in_BinaryReader.ReadInt32());
            return header;
        }
        public void Write(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(Common.EndianSwap(Field00));
            binaryWriter.Write(Common.EndianSwap(Field04));
        }
    }
}