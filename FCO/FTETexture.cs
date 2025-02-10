using Amicitia.IO.Binary;
using System.Numerics;
using System.Text;

namespace SUFcoTool
{
    public struct Texture : IBinarySerializable
    {
        public string Name { get; set; }
        public Vector2 Size { get; set; }

        public void Read(BinaryObjectReader reader)
        {
            // Texture Name
            Name = Common.ReadAscii(reader);
            //if (in_IsGens)
            //{
            //    //textureData.Name = Common.ReadAscii(in_BinaryReader);
            //}
            //else
            //{
            //    textureData.Name = encoding.GetString(in_BinaryReader.ReadBytes(Common.EndianSwap(in_BinaryReader.ReadInt32())));
            //    Common.SkipPadding(in_BinaryReader);
            //}

            int width = reader.ReadInt32();
            int height = reader.ReadInt32();
            Size = new Vector2(width, height);
        }

        public void Write(BinaryObjectWriter writer)
        {
            writer.WriteString(StringBinaryFormat.PrefixedLength32, Name);
            int nameLength = Name.Length;
            int namePadding = (4 - Name.Length % 4) % 4;
            string nameWithPadding = "".PadRight(Name.Length + namePadding, '@');
            writer.WriteString(StringBinaryFormat.FixedLength, nameWithPadding, nameWithPadding.Length);
            writer.Write((int)Size.X);
            writer.Write((int)Size.Y);
        }
    }
}
