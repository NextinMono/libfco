using System.Numerics;
namespace SUFcoTool
{
    public struct Character
    {
        public int TextureIndex { get; set; }
        public string FcoCharacterID { get; set; }
        public Vector2 TopLeft { get; set; }
        public Vector2 BottomRight { get; set; }
        public static Character Read(BinaryReader in_Reader, int in_CharaIndex, List<Texture> in_Textures, bool in_IsGens)
        {
            Character charaData = new Character();
            float texSizeX = in_Textures[charaData.TextureIndex].Size.X;
            float texSizeY = in_Textures[charaData.TextureIndex].Size.Y;

            charaData.TextureIndex = Common.EndianSwap(in_Reader.ReadInt32());
            var left = Common.EndianSwapFloat(in_Reader.ReadSingle());
            var top = Common.EndianSwapFloat(in_Reader.ReadSingle());
            var right = Common.EndianSwapFloat(in_Reader.ReadSingle());
            var bottom = Common.EndianSwapFloat(in_Reader.ReadSingle());
            charaData.TopLeft = new Vector2(texSizeX * left, texSizeY * top);
            charaData.BottomRight = new Vector2(texSizeX * right, texSizeY * bottom);

            charaData.FcoCharacterID = in_CharaIndex.ToString("X8").Insert(2, " ").Insert(5, " ").Insert(8, " ");
            if (in_IsGens)
            {
                var WChar = BitConverter.ToUInt16(in_Reader.ReadBytes(2), 0);
                in_Reader.BaseStream.Seek(2, SeekOrigin.Current);
            }
            return charaData;
        }
    }
}
