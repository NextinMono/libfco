using System.Xml;
using System.Text;
using SUFontTool.FCO;

namespace SUFcoTool
{
    public partial class FCO
    {        
        public ConverseHeaderPackage Header;
        public string? MasterGroupName;
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
            if (!string.IsNullOrEmpty(in_PathTable))
                fcoFile.TranslationTable = TranslationTable.Read(in_PathTable);

            // Header
            fcoFile.Header = ConverseHeaderPackage.Read(binaryReader);

            //Apparently gens fcos use 1, unleashed fcos use 0
            if (fcoFile.Header.Field04 != 0)
            {
                int masterGroupCount = Common.EndianSwap(binaryReader.ReadInt32());

                Encoding encoding = Encoding.GetEncoding("UTF-8");

                fcoFile.MasterGroupName = encoding.GetString(binaryReader.ReadBytes(Common.EndianSwap(binaryReader.ReadInt32())));
                Common.SkipPadding(binaryReader);
            }
            // Amount of groups
            int groupCount = Common.EndianSwap(binaryReader.ReadInt32());

            // Parse all groups
            for (int g = 0; g < groupCount; g++)
            {
                fcoFile.Groups.Add(Group.Read(binaryReader, fcoFile.TranslationTable, in_IsGensTemp));
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

        public void Write(string in_Path)
        {
            BinaryWriter binaryWriter = new BinaryWriter(File.Open(in_Path, FileMode.OpenOrCreate));

            // Writing Header
            Header.Write(binaryWriter);
            if (Header.Field04 != 0)
            {
                //Master group count (always 1 in gens according to brianuu)
                binaryWriter.Write(Common.EndianSwap(1));

                binaryWriter.Write(Common.EndianSwap(MasterGroupName.Length));
                Common.ConvString(binaryWriter, Common.PadString(MasterGroupName, '@'));
            }
            // Group Count
            binaryWriter.Write(Common.EndianSwap(Groups.Count));

            //Write groups
            foreach (Group group in Groups)
            {
                group.Write(binaryWriter);
            }
            binaryWriter.Close();
        }
    }
}