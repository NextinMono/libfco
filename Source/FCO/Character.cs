using Amicitia.IO.Binary;
using System.Numerics;
namespace libfco
{
    public struct Character : IBinarySerializable
    {
        public int TextureIndex { get; set; }
        public int CharacterID { get; set; }
        public Vector2 TopLeft { get; set; }
        public Vector2 BottomRight { get; set; }

        public void Read(BinaryObjectReader reader)
        {
            TextureIndex = reader.ReadInt32();
            var left = reader.ReadSingle();
            var top = reader.ReadSingle();
            var right = reader.ReadSingle();
            var bottom = reader.ReadSingle();
            TopLeft = new Vector2(left, top);
            BottomRight = new Vector2(right, bottom);
        }

        public void Write(BinaryObjectWriter writer)
        {
            writer.Write(TextureIndex);
            writer.Write(TopLeft.X);
            writer.Write(TopLeft.Y);
            writer.Write(BottomRight.X);
            writer.Write(BottomRight.Y);
        }
    }
}
