using Amicitia.IO.Binary;

namespace SUFcoTool
{
    public struct ConverseHeaderPackage : IBinarySerializable
    {
        public int Field00;
        public int Version; //0 for Unleashed, 1 for Gens
        public void Read(BinaryObjectReader reader)
        {
            Field00 = reader.ReadInt32();
            Version = reader.ReadInt32();
        }
        public void Write(BinaryObjectWriter writer)
        {
            writer.Write(Field00);
            writer.Write(Version);
        }
    }
}