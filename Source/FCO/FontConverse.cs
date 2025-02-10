using System.Xml;
using System.Text;
using Amicitia.IO.Binary;

namespace SUFcoTool
{
    public partial class FontConverse : IBinarySerializable
    {        
        public ConverseHeaderPackage Header;
        public string? MasterGroupName;
        public List<Group> Groups = new List<Group>();

        /// <summary>
        /// Reads a .fco file and returns a struct.
        /// </summary>
        /// <param name="in_Path">Path to the .fco file</param>
        /// <param name="in_PathTable">Path to the translation table, this can be left as blank to get the raw fco data.</param>
        /// <returns></returns>

        
        public void Read(BinaryObjectReader binaryReader)
        {
            // Header
           Header = binaryReader.ReadObject<ConverseHeaderPackage>();

            //Apparently gens fcos use 1, unleashed fcos use 0
            if (Header.Version != 0)
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
            if (Header.Version != 0)
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