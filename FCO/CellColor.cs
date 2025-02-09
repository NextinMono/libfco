using System.Numerics;

namespace SUFcoTool
{
    public class CellColor
    {
        public Vector4 ArgbColor { get; set; } = new Vector4(1, 1, 1, 1);
        public int Start { get; set; }
        public int End { get; set; }
        public int Marker { get; set; }

        public static CellColor Read(BinaryReader in_Reader)
        {
            CellColor color = new CellColor();
            color.Start = Common.EndianSwap(in_Reader.ReadInt32());
            color.End = Common.EndianSwap(in_Reader.ReadInt32());
            color.Marker = Common.EndianSwap(in_Reader.ReadInt32());
            color.ArgbColor = new Vector4
            {
                W = in_Reader.ReadByte() / 255.0f,
                X = in_Reader.ReadByte() / 255.0f,
                Y = in_Reader.ReadByte() / 255.0f,
                Z = in_Reader.ReadByte() / 255.0f
            };
            return color;
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

        internal void Write(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(Common.EndianSwap(Start));
            binaryWriter.Write(Common.EndianSwap(End));
            binaryWriter.Write(Common.EndianSwap(Marker));
            byte[] color = GetColorAsBytes();
            binaryWriter.Write(color);
        }
    }
}
