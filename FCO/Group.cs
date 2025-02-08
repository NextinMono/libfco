using SUFontTool.FCO;
using System.Text;

namespace SUFcoTool
{
    public struct Group
    {
        public string Name { get; set; }
        public List<Cell> CellList { get; set; }
        public static Group Read(BinaryReader in_Reader, TranslationTable in_Table, bool in_IsGens)
        {
            Group groupData = new Group();
            Encoding encoding = Encoding.GetEncoding("UTF-8");

            // Group Name
            groupData.Name = encoding.GetString(in_Reader.ReadBytes(Common.EndianSwap(in_Reader.ReadInt32())));
            Common.SkipPadding(in_Reader);

            // Amount of cells (text containers)
            int cellCount = Common.EndianSwap(in_Reader.ReadInt32());

            //Read cells
            groupData.CellList = new List<Cell>();
            for (int c = 0; c < cellCount; c++)
            {
                groupData.CellList.Add(Cell.Read(in_Reader, in_Table, in_IsGens));
            }
            return groupData;
        }
    }
}
