using Amicitia.IO.Binary;
using System.Text;

namespace SUFcoTool
{
    class Common
    {
        /// <summary>
        /// Returns a string where every 4 bytes are separated by a comma (00 00 00 00, 00 00 .. etc.)
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string FormatEvery4Bytes(byte[] bytes)
        {
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("X2"));
                if ((i + 1) % 4 != 0 && i != bytes.Length - 1)
                {
                    builder.Append(" ");
                }
                if ((i + 1) % 4 == 0 && i != bytes.Length - 1)
                {
                    builder.Append(", ");
                }
            }

            return builder.ToString();
        }
        public static string ReadAscii(BinaryObjectReader reader)
        {
            int stringLength = reader.ReadInt32();

            byte[] stringBuffer = reader.ReadArray<byte>(stringLength);
            string result = Encoding.ASCII.GetString(stringBuffer);

            // Check for @ padding
            while (reader.Position % 4 != 0)
            {
                char padding = (char)reader.ReadByte();
                if (padding != '@')
                {
                    throw new InvalidDataException("Invalid padding character");
                }
            }

            return result;
        }
        // XML Functions
        public static byte[] StringToByteArray(string hex)
        {
            int numberChars = hex.Length;
            byte[] bytes = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2)
            {
                var substr = hex.Substring(i, 2);
                bytes[i / 2] = Convert.ToByte(substr, 16);
            }
            return bytes;
        }
        public static void WriteStringTemp(BinaryObjectWriter writer, string text)
        {
            writer.WriteString(StringBinaryFormat.PrefixedLength32, text);
            string padding = new string('@', (4 - text.Length % 4) % 4);
            writer.WriteString(StringBinaryFormat.FixedLength, padding, padding.Length);
        }
    }
}