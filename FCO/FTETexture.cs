using System.Numerics;
using System.Text;

namespace SUFcoTool
{
    public struct Texture
    {
        public string Name { get; set; }
        public Vector2 Size { get; set; }
        public static Texture Read(BinaryReader in_BinaryReader, bool in_IsGens)
        {
            // Names of Groups and Cells are in UTF-8
            Encoding encoding = Encoding.GetEncoding("UTF-8");      
            Texture textureData = new Texture();

            // Texture Name
            if (in_IsGens)
            {
                textureData.Name = Common.ReadAscii(in_BinaryReader);
            }
            else
            {
                textureData.Name = encoding.GetString(in_BinaryReader.ReadBytes(Common.EndianSwap(in_BinaryReader.ReadInt32())));
                Common.SkipPadding(in_BinaryReader);
            }

            int width = in_BinaryReader.ReadInt32();
            int height = in_BinaryReader.ReadInt32();
            textureData.Size = new Vector2(Common.EndianSwap(width), Common.EndianSwap(height));
            return textureData;
        }
    }
}
