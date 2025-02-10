using Amicitia.IO.Binary;
using System.Numerics;
namespace SUFcoTool
{
    public struct Character : IBinarySerializable
    {
        public int TextureIndex { get; set; }
        public string FcoCharacterID { get; set; }
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
            //if (in_IsGens)
            //{
            //    reader.Seek(4, SeekOrigin.Current);
            //}
        }

        public void Write(BinaryObjectWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
