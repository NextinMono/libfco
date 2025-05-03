using Amicitia.IO.Binary;
using System.Text;

namespace libfco
{
    public class Group : IBinarySerializable
    {
        public string Name { get; set; }
        public List<Cell> Cells { get; set; }

        public Group(string in_Name)
        {
            Name = in_Name;
            Cells = new List<Cell>();
            Cells.Add(new Cell("New_Cell"));
        }
        public Group()
        {
            Cells = new List<Cell>();
        }
        public void Read(BinaryObjectReader reader)
        {
            // Group Name
            Name = Common.ReadAscii(reader);

            // Amount of cells (text containers)
            int cellCount = reader.ReadInt32();

            //Read cells
            Cells = new List<Cell>();
            for (int c = 0; c < cellCount; c++)
            {
                Cells.Add(reader.ReadObject<Cell>());
            }
        }

        public void Write(BinaryObjectWriter writer)
        {
            Common.WriteStringTemp(writer, Name);
            // Cell Count
            writer.Write(Cells.Count);
            foreach (Cell cell in Cells)
            {
                writer.WriteObject(cell);
            }
        }
    }
}
