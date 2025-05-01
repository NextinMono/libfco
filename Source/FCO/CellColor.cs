using Amicitia.IO.Binary;
using System.Numerics;

namespace libfco
{
    public class CellColor : IBinarySerializable
    {

        public int Start { get; set; }
        public int End { get; set; }
        public int Type { get; set; }
        public Vector4 ArgbColor { get; set; }

        public CellColor()
        {
        }
        public CellColor(int in_Type)
        {
            Type = in_Type;
            ArgbColor = new Vector4(1, 1, 1, 1);
        }
        /// <summary>
        /// Returns ArgbColor as a byte array
        /// </summary>
        /// <returns></returns>
        public byte[] GetColorAsBytes()
        {
            byte[] bytes =
            [
                (byte)(ArgbColor.W * 255.0f),
                (byte)(ArgbColor.X * 255.0f),
                (byte)(ArgbColor.Y * 255.0f),
                (byte)(ArgbColor.Z * 255.0f),
            ];
            return bytes;
        }

        public void Read(BinaryObjectReader reader)
        {
            Start = reader.ReadInt32();
            End = reader.ReadInt32();
            Type = reader.ReadInt32();
            ArgbColor = new Vector4
            {
                W = reader.ReadByte() / 255.0f,
                X = reader.ReadByte() / 255.0f,
                Y = reader.ReadByte() / 255.0f,
                Z = reader.ReadByte() / 255.0f
            };
        }

        public void Write(BinaryObjectWriter writer)
        {
            writer.Write(Start);
            writer.Write(End);
            writer.Write(Type);
            writer.WriteArray(GetColorAsBytes());
        }
    }
}
