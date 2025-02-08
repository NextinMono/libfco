using SUFcoTool;
using System.Text;
using System.Xml;


namespace SUFcoTool
{
    class Common
    {
        public static bool noLetter = false;
        public static string? ProgramDir
        {
            get
            {
                return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            }
        }
        // Common Functions
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
        public static void TempCheck(int mode)
        {    // This is no longer needed but will be kept for future use            
            if (mode == 1)
            {
                FileStream fs;

                if (File.Exists("temp.txt"))
                {
                    File.Delete("temp.txt");
                    fs = File.Create("temp.txt");
                    fs.Close();

                    Console.WriteLine("Deleted and Restored Temp File.");
                }
                else
                {
                    fs = File.Create("temp.txt");
                    Console.WriteLine("Created Temp File.");
                    fs.Close();
                }
            }
            if (mode == 2)
            {
                if (Common.noLetter)
                {
                    return;
                }

                File.Delete("temp.txt");
            }
        }


        public static void ExtractCheck()
        {
            Console.WriteLine("Do you want to extract sprites using " + Program.fileName + "? [Y/N]");
            string choice = Console.ReadLine();
            if (choice.ToLower() != "y") return;

            //DDS.Process();
            Table.WriteJSON();
            Console.WriteLine("\nPress Enter to Exit.");
            Console.Read();
        }

        static void IndexCheck(string userInput, int length)
        {
            int userInt = Convert.ToInt32(userInput);
            if (userInt < 1 || userInt > length)
            {
                Console.WriteLine("\nThis is not a valid selection!\nPress any key to exit.");
                Console.Read();
                Environment.Exit(0);
                return;
            }
        }


        // FCO and FTE Functions
        public static void TableAssignment()
        {      // This block of code is probably the worst thing I have ever made :)
            string fcoTableDir = Common.ProgramDir + "/tables/";
            Console.WriteLine("Please Input the number corresponding to the original location of your FCO file:");
            Console.WriteLine("\n1: Languages\n2: Subtitle");

            string fcoTableName = "";
            string[] location = { "Languages/", "Subtitle/" };
            string[] language = { "English/", "French/", "German/", "Italian/", "Japanese/", "Spanish/" };
            string[] version = { "Retail/", "DLC/", "Preview/" };

            string? userInput = Console.ReadLine();

            switch (userInput.ToLower())
            {
                case "1":
                    IndexCheck(userInput, location.Length);
                    fcoTableName += location[Convert.ToInt32(userInput) - 1];
                    TranslationService.iconsTablePath = fcoTableDir + "Icons.json";
                    break;
                case "2":
                    IndexCheck(userInput, location.Length);
                    fcoTableName += location[Convert.ToInt32(userInput) - 1];
                    break;
                case "test":
                    Console.WriteLine("\nWhat is the name of the table you want to test?");
                    userInput = Console.ReadLine();
                    fcoTableName = fcoTableDir + userInput + ".json";
                    return;
                default:
                    IndexCheck(userInput, location.Length);
                    return;
            }

            /*if (userInput.ToLower() == "test") {
                Console.WriteLine("\nWhat is the name of the table you want to test?");
                userInput = Console.ReadLine();
                fcoTableName = fcoTableDir + userInput + ".json";
                return;
            }
            if (userInput == "1") {
                IndexCheck(userInput, location.Length);
                fcoTableName += location[Convert.ToInt32(userInput) - 1];
                Translator.iconsTablePath = fcoTableDir + "Icons.json";
            }
            if (userInput == "2") {
                IndexCheck(userInput, location.Length);
                fcoTableName += location[Convert.ToInt32(userInput) - 1];
            }*/

            Console.WriteLine("\nPlease Input the number corresponding to the language of your FCO file");
            Console.WriteLine("\n1: English\n2: French\n3: German\n4: Italian\n5: Japanese\n6: Spanish\n");
            userInput = Console.ReadLine();
            IndexCheck(userInput, language.Length);
            fcoTableName += language[Convert.ToInt32(userInput) - 1];

            Console.WriteLine("\nPlease Input the number corresponding to the original version of your FCO file:");
            Console.WriteLine("\n1: Retail\n2: DLC\n3: Preview\n");
            userInput = Console.ReadLine();
            IndexCheck(userInput, version.Length);
            fcoTableName += version[Convert.ToInt32(userInput) - 1];

            Console.WriteLine("\nWhat is the name of the archive the FCO originated from?");
            fcoTableName += userInput = Console.ReadLine();

            //fcoTable = fcoTableDir + fcoTableName + ".json";
            //Console.WriteLine(fcoTable + "\n" + Translator.iconsTablePath);
        }

        public static int EndianSwap(int a)
        {
            byte[] x = BitConverter.GetBytes(a);
            Array.Reverse(x);
            int b = BitConverter.ToInt32(x, 0);
            return b;
        }
        public static string ReadAscii(BinaryReader reader)
        {
            int stringLength = Common.EndianSwap(reader.ReadInt32());

            byte[] stringBuffer = reader.ReadBytes(stringLength);
            string result = Encoding.ASCII.GetString(stringBuffer);

            // Check for @ padding
            while (reader.BaseStream.Position % 4 != 0)
            {
                char padding = (char)reader.ReadByte();
                if (padding != '@')
                {
                    throw new InvalidDataException("Invalid padding character");
                }
            }

            return result;
        }

        public static float EndianSwapFloat(float a)
        {
            byte[] x = BitConverter.GetBytes(a);
            Array.Reverse(x);
            float b = BitConverter.ToSingle(x, 0);
            return b;
        }

        public static void SkipPadding(BinaryReader binaryReader)
        {
            while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
            {
                int padding = Common.EndianSwap(binaryReader.ReadByte());

                if (padding == 64)
                {
                    binaryReader.BaseStream.Seek(1, SeekOrigin.Current);
                }
                else if (padding < 64)
                {
                    binaryReader.BaseStream.Seek(-1, SeekOrigin.Current);
                    break;
                }
            }
        }
        public static void WriteFCOColor(XmlWriter writer, CellColor ColorType)
        {
            writer.WriteAttributeString("Start", ColorType.Start.ToString());
            writer.WriteAttributeString("End", ColorType.End.ToString());
            writer.WriteAttributeString("Marker", ColorType.Marker.ToString());

            writer.WriteAttributeString("Alpha", ColorType.ArgbColor.W.ToString());
            writer.WriteAttributeString("Red", ColorType.ArgbColor.X.ToString());
            writer.WriteAttributeString("Green", ColorType.ArgbColor.Y.ToString());
            writer.WriteAttributeString("Blue", ColorType.ArgbColor.Z.ToString());
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

        public static void ReadXMLColor(ref CellColor ColorType, XmlElement? ColorNode)
        {
            try
            {
                ColorType.Start = int.Parse(ColorNode.Attributes.GetNamedItem("Start")!.Value!);
                ColorType.End = int.Parse(ColorNode.Attributes.GetNamedItem("End")!.Value!);
                ColorType.Marker = int.Parse(ColorNode.Attributes.GetNamedItem("Marker")!.Value!);
                ColorType.ArgbColor = new System.Numerics.Vector4
                {
                    W = byte.Parse(ColorNode.Attributes.GetNamedItem("Alpha")!.Value!),
                    X = byte.Parse(ColorNode.Attributes.GetNamedItem("Red")!.Value!),
                    Y = byte.Parse(ColorNode.Attributes.GetNamedItem("Green")!.Value!),
                    Z = byte.Parse(ColorNode.Attributes.GetNamedItem("Blue")!.Value!)
                };
            }
            catch (FormatException e)
            {
                //Console.WriteLine(e);
                var groupName = ColorNode.ParentNode.ParentNode.Attributes.GetNamedItem("Name")!.Value!;
                var cellName = ColorNode.ParentNode.Attributes.GetNamedItem("Name")!.Value!;
                Console.WriteLine("ERROR: Check your Color Values in Group: " + groupName + ", Cell: " + cellName);
                Console.ReadKey();
                throw;
            }
        }

        public static void WriteXMLColor(BinaryWriter binaryWriter, CellColor ColorType)
        {
            binaryWriter.Write(Common.EndianSwap(ColorType.Start));
            binaryWriter.Write(Common.EndianSwap(ColorType.End));
            binaryWriter.Write(Common.EndianSwap(ColorType.Marker));
            byte[] color = ColorType.GetColorAsBytes();
            binaryWriter.Write(color);
        }

        public static string PadString(string input, char fillerChar)
        {
            int padding = (4 - input.Length % 4) % 4;
            return input.PadRight(input.Length + padding, fillerChar);
        }

        public static void ConvString(BinaryWriter writer, string value)
        {
            // Turning string into Byte Array so the data can be written properly
            byte[] utf8Bytes = Encoding.UTF8.GetBytes(value);
            writer.Write(utf8Bytes);
        }

        public static void RemoveComments(XmlNode node)
        {
            if (node == null) return;

            // Remove comment nodes
            for (int i = node.ChildNodes.Count - 1; i >= 0; i--)
            {
                XmlNode? child = node.ChildNodes[i];
                if (child.NodeType == XmlNodeType.Comment)
                {
                    node.RemoveChild(child);
                }
                else
                {
                    // Recursively remove comments from child nodes
                    RemoveComments(child);
                }
            }
        }
    }
}