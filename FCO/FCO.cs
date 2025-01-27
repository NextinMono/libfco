using System.Xml;
using System.Text;
using SUFcoTool;
using SUFontTool.FCO;


namespace SUFcoTool
{
    public class FCO
    {        
        public List<Group> Groups = new List<Group>();
        public TranslationTable TranslationTable;

        /// <summary>
        /// Reads a .fco file and returns a struct.
        /// </summary>
        /// <param name="in_Path">Path to the .fco file</param>
        /// <param name="in_PathTable">Path to the translation table, this can be left as blank to get the raw fco data.</param>
        /// <returns></returns>
        public static FCO Read(string in_Path, string in_PathTable = "")
        {           
            //Common.fcoTable
            FileStream fileStream = new FileStream(in_Path, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fileStream);
            Encoding UTF8Encoding = Encoding.GetEncoding("UTF-8");      // Names of Groups and Cells are in UTF-8

            FCO fCO = new FCO();
            if(!string.IsNullOrEmpty(in_PathTable))
            fCO.TranslationTable = TranslationTable.Read(in_PathTable);
            try
            {
                // Start Parse
                binaryReader.ReadInt64();   // This is 0x04 and 0x00

                // Groups
                int groupCount = Common.EndianSwap(binaryReader.ReadInt32());

                for (int g = 0; g < groupCount; g++)
                {
                    Group groupData = new Group();

                    // Group Name
                    groupData.Name = UTF8Encoding.GetString(binaryReader.ReadBytes(Common.EndianSwap(binaryReader.ReadInt32())));
                    Common.SkipPadding(binaryReader);

                    // Cells count
                    int cellCount = Common.EndianSwap(binaryReader.ReadInt32());

                    // Cells
                    List<Cell> Cells = new List<Cell>();
                    for (int c = 0; c < cellCount; c++)
                    {
                        Cell cellData = new Cell();

                        // Cell's name
                        cellData.Name = UTF8Encoding.GetString(binaryReader.ReadBytes(Common.EndianSwap(binaryReader.ReadInt32())));
                        Common.SkipPadding(binaryReader);

                        int messageLength = Common.EndianSwap(binaryReader.ReadInt32());
                        byte[] cellMessageBytes = binaryReader.ReadBytes(messageLength * 4);
                        string rawMessageData = BitConverter.ToString(cellMessageBytes).Replace('-', ' ');
                        cellData.Message = TranslationService.HEXtoTXT(rawMessageData, fCO.TranslationTable);
                        cellData.MessageConverseIDs = Common.FormatEvery4Bytes(cellMessageBytes);
                        binaryReader.ReadInt32();   // This is 0x04 before Colors

                        // Main Text Color
                        Color ColorMain = new Color();
                        Common.ReadFCOColor(binaryReader, ref ColorMain);
                        cellData.ColorMain = ColorMain;

                        // Check what this is
                        Color ColorSub1 = new Color();
                        Common.ReadFCOColor(binaryReader, ref ColorSub1);
                        cellData.ColorSub1 = ColorSub1;

                        // Check what this is
                        Color ColorSub2 = new Color();
                        Common.ReadFCOColor(binaryReader, ref ColorSub2);
                        cellData.ColorSub2 = ColorSub2;

                        binaryReader.ReadInt32();   // I'm still unsure what these values do    // int ColorExtraStart = 
                        binaryReader.ReadInt32();                                               // int ColorExtraEnd =
                        binaryReader.ReadInt32();   // This 0x03 marks the very end of the data

                        // Alignment
                        int alignment = Common.EndianSwap(binaryReader.ReadInt32());
                        if (alignment > 3) alignment = 0;
                        var enumDisplayStatus = (Cell.TextAlign)alignment;
                        cellData.Alignment = enumDisplayStatus;

                        // Highlights
                        var highlightCount = Common.EndianSwap(binaryReader.ReadInt32());  // If this is anything but 0, it's the highlight count in the cell

                        List<Color> Highlights = new List<Color>();
                        for (int h = 0; h < highlightCount; h++)
                        {
                            Color hightlightData = new Color();
                            Common.ReadFCOColor(binaryReader, ref hightlightData);
                            Highlights.Add(hightlightData);
                        }
                        cellData.Highlights = Highlights;

                        // End of Cell
                        binaryReader.ReadInt32();   // Yet to find out what this really does mean..
                        Cells.Add(cellData);
                        /*  This will add together the following into the Cell Struct:
                            Cell Name, Message, Color0, Color1, Color2 and (possibly) Highlights */
                    }

                    groupData.CellList = Cells;     // This will put every Cell into a Group
                    fCO.Groups.Add(groupData);          // This adds all the Groups together
                }
            }
            catch (EndOfStreamException e)
            {
                throw;
            }

            binaryReader.Close();
            binaryReader.Dispose();
            return fCO;
        }

        public void WriteXML(string path)
        {
            File.Delete(Path.Combine(Path.GetFileNameWithoutExtension(path) + ".xml"));

            var xmlWriterSettings = new XmlWriterSettings { Indent = true };
            using var writer = XmlWriter.Create(Path.GetDirectoryName(path) + "\\" +
            Path.GetFileNameWithoutExtension(path) + ".xml", xmlWriterSettings);

            writer.WriteStartDocument();
            writer.WriteStartElement("FCO");
            writer.WriteAttributeString("Table", TranslationTable.Name);      // This is used later once the XML is used to convert the data back into an FCO format

            writer.WriteStartElement("Groups");
            foreach (Group group in Groups)
            {
                writer.WriteStartElement("Group");
                writer.WriteAttributeString("Name", group.Name);

                foreach (Cell cell in group.CellList)
                {
                    writer.WriteStartElement("Cell");
                    writer.WriteAttributeString("Name", cell.Name);                     // These parameters are part of the "Cell" Element Header
                    writer.WriteAttributeString("Alignment", cell.Alignment.ToString());
                    // The following Elements are all within the "Cell" Element
                    writer.WriteStartElement("Message");
                    writer.WriteAttributeString("MessageData", cell.Message);
                    writer.WriteEndElement();

                    writer.WriteStartElement("ColorMain");
                    Common.WriteFCOColor(writer, cell.ColorMain);
                    writer.WriteEndElement();

                    writer.WriteStartElement("ColorSub1");                                 // Actually figure out what this was again
                    Common.WriteFCOColor(writer, cell.ColorSub1);
                    writer.WriteEndElement();

                    writer.WriteStartElement("ColorSub2");                                 // Actually figure out what this was again
                    Common.WriteFCOColor(writer, cell.ColorSub2);
                    writer.WriteEndElement();

                    foreach (Color highlight in cell.Highlights)
                    {
                        writer.WriteStartElement("Highlight" + cell.Highlights.IndexOf(highlight));
                        Common.WriteFCOColor(writer, highlight);
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }
            writer.WriteEndElement();

            writer.WriteEndDocument();
            writer.Close();

            Console.WriteLine("XML written!");
        }

        public void Write(string path)
        {
            BinaryWriter binaryWriter = new BinaryWriter(File.Open(path, FileMode.OpenOrCreate));

            // Writing Header
            binaryWriter.Write(Common.EndianSwap(0x00000004));
            binaryWriter.Write(0x00000000);

            // Group Count
            binaryWriter.Write(Common.EndianSwap(Groups.Count));
            for (int g = 0; g < Groups.Count; g++)
            {
                // Group Name
                binaryWriter.Write(Common.EndianSwap(Groups[g].Name.Length));
                Common.ConvString(binaryWriter, Common.PadString(Groups[g].Name, '@'));

                // Cell Count
                binaryWriter.Write(Common.EndianSwap(Groups[g].CellList.Count));
                for (int c = 0; c < Groups[g].CellList.Count; c++)
                {
                    var standardArea = Groups[g].CellList[c];
                    // Cell Name
                    binaryWriter.Write(Common.EndianSwap(standardArea.Name.Length));
                    Common.ConvString(binaryWriter, Common.PadString(standardArea.Name, '@'));

                    string unformattedConverseIDs = standardArea.MessageConverseIDs.Replace(", ", "").Replace(" ", "");
                    //Message Data
                    binaryWriter.Write(Common.EndianSwap(unformattedConverseIDs.Length / 8));
                    binaryWriter.Write(Common.StringToByteArray(unformattedConverseIDs));

                    // Color Start
                    binaryWriter.Write(Common.EndianSwap(0x00000004));

                    Common.WriteXMLColor(binaryWriter, standardArea.ColorMain);  // Text Colors
                    Common.WriteXMLColor(binaryWriter, standardArea.ColorSub1);  // Check
                    Common.WriteXMLColor(binaryWriter, standardArea.ColorSub2);  // Check

                    //End Colors
                    binaryWriter.Write(Common.EndianSwap(standardArea.ColorMain.ColorStart));
                    binaryWriter.Write(Common.EndianSwap(standardArea.ColorMain.ColorEnd));
                    binaryWriter.Write(Common.EndianSwap(0x00000003));

                    Cell.TextAlign alignConv = (Cell.TextAlign)Enum.Parse(typeof(Cell.TextAlign), standardArea.Alignment.ToString());
                    binaryWriter.Write(Common.EndianSwap((int)alignConv));

                    if (standardArea.Highlights != null)
                    {
                        binaryWriter.Write(Common.EndianSwap(standardArea.Highlights.Count));
                        for (int h = 0; h < standardArea.Highlights.Count; h++)
                        {
                            var highlights = standardArea.Highlights[h];
                            Common.WriteXMLColor(binaryWriter, highlights);
                        }
                    }

                    if (standardArea.Highlights != null)
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

            binaryWriter.Close();
        }
    }
}