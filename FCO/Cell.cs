using SUFontTool.FCO;
using System.Text;

namespace SUFcoTool
{
    public class Cell
    {
        public Cell() { }
        public Cell(string name, string message)
        {
            Name = name;
            Message = message;
            MainColor = new CellColor();
            ExtraColor1 = new CellColor();
            ExtraColor2 = new CellColor();
            MessageConverseIDs = "";
        }

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

        public void Write(BinaryWriter binaryWriter)
        {
            // Cell Name
            binaryWriter.Write(Common.EndianSwap(Name.Length));
            Common.ConvString(binaryWriter, Common.PadString(Name, '@'));

            string unformattedConverseIDs = MessageConverseIDs.Replace(", ", "").Replace(" ", "");

            //Message Data
            binaryWriter.Write(Common.EndianSwap(unformattedConverseIDs.Length / 8));
            binaryWriter.Write(Common.StringToByteArray(unformattedConverseIDs));

            // Color Start
            binaryWriter.Write(Common.EndianSwap(0x00000004));

            //Main text color
            MainColor.Write(binaryWriter);

            //Unknown, might not be colors
            ExtraColor1.Write(binaryWriter);
            ExtraColor2.Write(binaryWriter);

            //End Colors
            binaryWriter.Write(Common.EndianSwap(MainColor.Start));
            binaryWriter.Write(Common.EndianSwap(MainColor.End));
            binaryWriter.Write(Common.EndianSwap(0x00000003));

            Cell.TextAlign alignConv = (Cell.TextAlign)Enum.Parse(typeof(Cell.TextAlign), Alignment.ToString());
            binaryWriter.Write(Common.EndianSwap((int)alignConv));

            if (Highlights != null)
            {
                binaryWriter.Write(Common.EndianSwap(Highlights.Count));
                foreach (CellColor highlightColor in Highlights)
                {
                    highlightColor.Write(binaryWriter);
                }
            }

            if (Highlights != null)
            {
                binaryWriter.Write(Common.EndianSwap(0x00000000));
            }
            else
            {
                binaryWriter.Write(Common.EndianSwap(0x00000000));
                binaryWriter.Write(Common.EndianSwap(0x00000000));
            }
        }
    }
}
