using Amicitia.IO.Binary;
using System.Numerics;
using System.Text;

namespace SUFcoTool
{
    public struct TextureEntry : IBinarySerializable
    {
        public string Name { get; set; }
        public Vector2 Size { get; set; }

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
