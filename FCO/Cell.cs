

namespace SUFcoTool
{
    public struct Cell
    {
        public enum TextAlign
        {
            left = 0,
            center = 1,
            right = 2,
            justified = 3
        }
        public string cellName { get; set; }
        public string cellMessage { get; set; }
        public byte[] cellMessageWrite { get; set; }
        public int messageCharAmount { get; set; }
        public Color ColorMain { get; set; }
        public Color ColorSub1 { get; set; }
        public Color ColorSub2 { get; set; }
        public TextAlign alignment { get; set; }
        public int highlightCount { get; set; }
        public List<Color> highlightList { get; set; }
    }
}
