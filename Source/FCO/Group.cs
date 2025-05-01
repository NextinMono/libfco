using Amicitia.IO.Binary;
using System.Text;

namespace libfco
{
    public class Group : IBinarySerializable
    {
        public string Name { get; set; }
        public List<Cell> CellList { get; set; }

        public Group(string in_Name)
        {
            Name = in_Name;
            CellList = new List<Cell>();
            CellList.Add(new Cell("New_Cell"));
        }
        public Group()
        {
            CellList = new List<Cell>();
        }
        public void Read(BinaryObjectReader reader)
        {
            // Group Name
            Name = Common.ReadAscii(reader);

            // Amount of cells (text containers)
            int cellCount = reader.ReadInt32();

            //Read cells
            CellList = new List<Cell>();
            for (int c = 0; c < cellCount; c++)
            {
                CellList.Add(reader.ReadObject<Cell>());
            }
        }

        public void Write(BinaryObjectWriter writer)
        {
            Common.WriteStringTemp(writer, Name);
            // Cell Count
            writer.Write(CellList.Count);
            foreach (Cell cell in CellList)
            {
                writer.WriteObject(cell);
            }
        }
    }
}
