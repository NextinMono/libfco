using System.Text;
using Amicitia.IO;
using Amicitia.IO.Binary;

namespace SUFcoTool
{
    public class SubCell : IBinarySerializable
    {
        public int Start { get; set; }
        public int End { get; set; }
        public int[] SubMessage { get; set; }

        public SubCell()
        {
            SubMessage = [];
        }
        public void Read(BinaryObjectReader reader)
        {
            Start = reader.ReadInt32();
            End = reader.ReadInt32();
            
            int messageLength = reader.ReadInt32();
            int[] subcellMessageBytes = reader.ReadArray<int>(messageLength);
            SubMessage = subcellMessageBytes;
        }

        public void Write(BinaryObjectWriter binaryWriter)
        {
            binaryWriter.Write(Start);
            binaryWriter.Write(End);
            
            // Message Data
            binaryWriter.Write(SubMessage.Length);
            binaryWriter.WriteArray(SubMessage);
        }
    }
}
