
using Amicitia.IO.Binary;

namespace SUFcoTool
{
    public struct ConverseHeaderPackage : IBinarySerializable
    {
        public int Field00;
        public int Field04;
        public void Read(BinaryObjectReader reader)
        {
            Field00 = reader.ReadInt32();
            Field04 = reader.ReadInt32();
        }
        public void Write(BinaryObjectWriter writer)
        {
            writer.Write(Field00);
            writer.Write(Field04);
        }
    }
}