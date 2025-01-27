namespace SUFcoTool
{
    public class Cell
    {
        public enum TextAlign
        {
            Left = 0,
            Center = 1,
            Right = 2,
            Justified = 3
        }
        public string Name { get; set; }
        public string Message { get; set; }
        public string MessageConverseIDs { get; set; }
        public byte[] MessageRawData { get; set; }
        public int MessageLength { get; set; }
        public Color ColorMain { get; set; }
        public Color ColorSub1 { get; set; }
        public Color ColorSub2 { get; set; }
        public TextAlign Alignment { get; set; }
        public List<Color> Highlights { get; set; }
    }
}
