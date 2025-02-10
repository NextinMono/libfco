using System.Xml;
using System.Text;
using SUFontTool.FCO;
using Amicitia.IO.Binary;

namespace SUFcoTool
{
    public partial class FCO : IBinarySerializable
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


        public void Read(BinaryObjectReader binaryReader)
        {
            // Header
           Header = binaryReader.ReadObject<ConverseHeaderPackage>();

            //Apparently gens fcos use 1, unleashed fcos use 0
            if (Header.Field04 != 0)
            {
                int masterGroupCount = binaryReader.ReadInt32();
                MasterGroupName = Common.ReadAscii(binaryReader);
            }
            // Amount of groups
            int groupCount =binaryReader.ReadInt32();
            
            // Parse all groups
            for (int g = 0; g < groupCount; g++)
            {
               Groups.Add(binaryReader.ReadObject<Group>());
            }
        }

        public void Write(BinaryObjectWriter writer)
        {
            // Writing Header
            writer.WriteObject(Header);
            if (Header.Field04 != 0)
            {
                //Master group count (always 1 in gens according to brianuu)
                writer.Write(1);

                Common.WriteStringTemp(writer, MasterGroupName);
            }
            // Group Count
            writer.Write(Groups.Count);

            //Write groups
            foreach (Group group in Groups)
            {
                writer.WriteObject(group);
            }
        }
    }
}