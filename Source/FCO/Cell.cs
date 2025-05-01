using System.Text;
using Amicitia.IO;
using Amicitia.IO.Binary;

namespace libfco
{
    public class Cell : IBinarySerializable
    {
        public enum TextAlign
        {
            Left = 0,
            Center = 1,
            Right = 2,
            Justified = 3
        }
        public string Name { get; set; }
        public int[] Message { get; set; }
        public CellColor MainColor { get; set; }
        public CellColor ExtraColor1 { get; set; }
        public CellColor ExtraColor2 { get; set; }
        public TextAlign Alignment { get; set; }
        public List<CellColor> Highlights { get; set; }
        public List<SubCell> SubCells { get; set; }

        public Cell()
        {
            Name = "NewCell";
            MainColor = new CellColor(2);
            ExtraColor1 = new CellColor(1);
            ExtraColor2 = new CellColor(0);
            Message = [];
            Highlights = new List<CellColor>();
            SubCells = new List<SubCell>();
        }
        public Cell(string name) : this()
        {
            Name = name;
        }
        public void Read(BinaryObjectReader reader)
        {
            // Cell's name
            Name = Common.ReadAscii(reader);
            //Common.SkipPadding(in_Reader);

            int messageLength = reader.ReadInt32();
            int[] cellMessageBytes = reader.ReadArray<int>(messageLength);
            //string rawMessageData = BitConverter.ToString(cellMessageBytes).Replace('-', ' ');
            Message = cellMessageBytes;
            reader.Seek(4, SeekOrigin.Current);   // This is 0x04 before Colors

            // Main Text Color
            MainColor = reader.ReadObject<CellColor>();
            // Unknown, might not be colors (according to Hedgeturd)
            ExtraColor1 = reader.ReadObject<CellColor>();
            ExtraColor2 = reader.ReadObject<CellColor>();

            //Unknown values, the last 4 bytes (0x3) mark "the end of the data"
            reader.Seek(0xC, SeekOrigin.Current);
            // There's a good chance these values have some relation to the 3 clumps of data proceeding

            // Alignment
            int alignment = Math.Clamp(reader.ReadInt32(), 0, 3);
            var enumDisplayStatus = (Cell.TextAlign)alignment;
            Alignment = enumDisplayStatus;
            
            // Highlight count - If this is anything but 0, it's the highlight count in the cell
            int highlightCount = reader.ReadInt32();

            List<CellColor> highlights = new List<CellColor>();
            for (int h = 0; h < highlightCount; h++)
            {
                highlights.Add(reader.ReadObject<CellColor>());
            }
            Highlights = highlights;
            
            //reader.Seek(0x4, SeekOrigin.Current);   // Yet to find out what this really does mean..
            // I found out Nextin :D
            int subCellCount = reader.ReadInt32();
            List<SubCell> subcells = new List<SubCell>();
            for (int s = 0; s < subCellCount; s++)
            {
                subcells.Add(reader.ReadObject<SubCell>());
            }
            SubCells = subcells;
        }

        public void Write(BinaryObjectWriter binaryWriter)
        {
            // Cell Name
            Common.WriteStringTemp(binaryWriter, Name);
            //binaryWriter.Write(Name.Length));
            //Common.ConvString(binaryWriter, Common.PadString(Name, '@'));
            
            //Message Data
            binaryWriter.Write(Message.Length);
            binaryWriter.WriteArray(Message);

            // Color Start
            binaryWriter.Write(0x00000004);

            //Main text color
            binaryWriter.WriteObject<CellColor>(MainColor);
            //Unknown, might not be colors
            binaryWriter.WriteObject<CellColor>(ExtraColor1);
            binaryWriter.WriteObject<CellColor>(ExtraColor2);

            //End Colors
            binaryWriter.Write(MainColor.Start);
            binaryWriter.Write(MainColor.End);
            binaryWriter.Write(0x00000003);

            // Cell Formatting
            TextAlign alignConv = (TextAlign)Enum.Parse(typeof(TextAlign), Alignment.ToString());
            binaryWriter.Write((int)alignConv);

            binaryWriter.Write(Highlights.Count);
            foreach (CellColor highlightColor in Highlights)
            {
                binaryWriter.WriteObject(highlightColor);
            }
            
            binaryWriter.Write(SubCells.Count);
            foreach (SubCell subcell in SubCells)
            {
                binaryWriter.WriteObject(subcell);
            }
        }
    }
}
