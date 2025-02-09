using SUFontTool.FCO;
using System.Text;

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
        public CellColor MainColor { get; set; }
        public CellColor ExtraColor1 { get; set; }
        public CellColor ExtraColor2 { get; set; }
        public TextAlign Alignment { get; set; }
        public List<CellColor> Highlights { get; set; }
        public static Cell Read(BinaryReader in_Reader, TranslationTable in_Table, bool in_IsGens)
        {
            Cell cellData = new Cell();
            Encoding encoding = Encoding.GetEncoding("UTF-8");
            // Cell's name
            cellData.Name = encoding.GetString(in_Reader.ReadBytes(Common.EndianSwap(in_Reader.ReadInt32())));
            Common.SkipPadding(in_Reader);

            int messageLength = Common.EndianSwap(in_Reader.ReadInt32());
            byte[] cellMessageBytes = in_Reader.ReadBytes(messageLength * 4);
            string rawMessageData = BitConverter.ToString(cellMessageBytes).Replace('-', ' ');
            cellData.Message = TranslationService.HEXtoTXT(rawMessageData, in_Table);
            cellData.MessageConverseIDs = Common.FormatEvery4Bytes(cellMessageBytes);
            in_Reader.ReadInt32();   // This is 0x04 before Colors

            if (!in_IsGens)
            {
                // Main Text Color
                cellData.MainColor = CellColor.Read(in_Reader);
                // Unknown, might not be colors (according to Hedgeturd)
                cellData.ExtraColor1 = CellColor.Read(in_Reader);
                cellData.ExtraColor2 = CellColor.Read(in_Reader);

                in_Reader.ReadInt32();   // I'm still unsure what these values do    // int ColorExtraStart = 
                in_Reader.ReadInt32();                                               // int ColorExtraEnd =
                in_Reader.ReadInt32();   // This 0x03 marks the very end of the data

                // Alignment
                int alignment = Math.Clamp(Common.EndianSwap(in_Reader.ReadInt32()), 0, 3);
                var enumDisplayStatus = (Cell.TextAlign)alignment;
                cellData.Alignment = enumDisplayStatus;
            }


            int test = in_Reader.ReadInt32();
            // Highlights
            var highlightCount = Common.EndianSwap(test);  // If this is anything but 0, it's the highlight count in the cell

            List<CellColor> highlights = new List<CellColor>();
            for (int h = 0; h < highlightCount; h++)
            {
                highlights.Add(CellColor.Read(in_Reader));
            }
            cellData.Highlights = highlights;

            // End of Cell
            in_Reader.ReadInt32();   // Yet to find out what this really does mean..
            return cellData;
        }
    }
}
