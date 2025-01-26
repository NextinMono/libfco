using System.Numerics;
namespace SUFcoTool
{
    public struct Character
    {
        public int TextureIndex { get; set; }
        public string FcoCharacterID { get; set; }
        public Vector2 TopLeft { get; set; }
        public Vector2 BottomRight { get; set; }
    }
}
