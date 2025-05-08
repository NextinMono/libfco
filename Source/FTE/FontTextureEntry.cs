using Amicitia.IO.Binary;
using System.Numerics;

namespace libfco
{
    public struct TextureEntry : IBinarySerializable
    {
        public string Name { get; set; }
        public Vector2 Size { get; set; }

        public TextureEntry(string in_TexName, Vector2 in_Size)
        {
            Name = in_TexName;
            Size = in_Size;
        }

        public void Read(BinaryObjectReader reader)
        {
            // Texture Name
            Name = Common.ReadAscii(reader);

            int width = reader.ReadInt32();
            int height = reader.ReadInt32();
            Size = new Vector2(width, height);
        }

        public void Write(BinaryObjectWriter writer)
        {
            Common.WriteStringTemp(writer, Name);
            writer.Write((int)Size.X);
            writer.Write((int)Size.Y);
        }
    }
}
