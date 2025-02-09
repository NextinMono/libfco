using System.Xml;
using System.Text;
using SUFcoTool;
using SUFontTool.FCO;


namespace SUFcoTool
{
    public partial class FCO
    {        
        public ConverseHeaderPackage Header;
        public List<Group> Groups = new List<Group>();
        public TranslationTable? TranslationTable;

        /// <summary>
        /// Reads a .fco file and returns a struct.
        /// </summary>
        /// <param name="in_Path">Path to the .fco file</param>
        /// <param name="in_PathTable">Path to the translation table, this can be left as blank to get the raw fco data.</param>
        /// <returns></returns>
        public static FCO Read(string in_Path, string in_PathTable = "", bool in_IsGensTemp = true)
        {           
            //Common.fcoTable
            FileStream fileStream = new FileStream(in_Path, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fileStream);

            FCO fcoFile = new FCO();
            if(!string.IsNullOrEmpty(in_PathTable))
                fcoFile.TranslationTable = TranslationTable.Read(in_PathTable);

            try
            {
                // Header (always 0x4 and 0x0)
                fcoFile.Header = ConverseHeaderPackage.Read(binaryReader);   

                // Amount of groups
                int groupCount = Common.EndianSwap(binaryReader.ReadInt32());

                // Parse all groups
                for (int g = 0; g < groupCount; g++)
                {
                    fcoFile.Groups.Add(Group.Read(binaryReader, fcoFile.TranslationTable, in_IsGensTemp));
                }
            }
            catch (EndOfStreamException e)
            {
                throw;
            }

            binaryReader.Close();
            binaryReader.Dispose();
            return fcoFile;
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
                    Common.WriteFCOColor(writer, cell.MainColor);
                    writer.WriteEndElement();

                    writer.WriteStartElement("ColorSub1");                                 // Actually figure out what this was again
                    Common.WriteFCOColor(writer, cell.ExtraColor1);
                    writer.WriteEndElement();

                    writer.WriteStartElement("ColorSub2");                                 // Actually figure out what this was again
                    Common.WriteFCOColor(writer, cell.ExtraColor2);
                    writer.WriteEndElement();

                    foreach (CellColor highlight in cell.Highlights)
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

                    Common.WriteXMLColor(binaryWriter, standardArea.MainColor);  // Text Colors
                    Common.WriteXMLColor(binaryWriter, standardArea.ExtraColor1);  // Check
                    Common.WriteXMLColor(binaryWriter, standardArea.ExtraColor2);  // Check

                    //End Colors
                    binaryWriter.Write(Common.EndianSwap(standardArea.MainColor.Start));
                    binaryWriter.Write(Common.EndianSwap(standardArea.MainColor.End));
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