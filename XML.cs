using System.Xml;


namespace SUFcoTool
{
    public static class XML
    {
        public static string tableNoName;
        public static int texCount = 0, charaCount = 0, spriteIndex = 0;
        public static bool FCO = false;
        static List<Group> groups = new List<Group>();
        public static List<Texture> textures = new List<Texture>();
        public static List<Character> characters = new List<Character>();

        static Cell.TextAlign GetAlignFromString(string in_String)
        {
            switch (in_String.ToLower())
            {
                case "left":
                    return Cell.TextAlign.left;
                case "center":
                    return Cell.TextAlign.center;
                case "right":
                    return Cell.TextAlign.right;
                case "justified":
                    return Cell.TextAlign.justified;
            }
            return Cell.TextAlign.left;
        }
        public static void ReadXML(string path)
        {
            string filePath = Path.GetDirectoryName(path) + "\\" + Path.GetFileNameWithoutExtension(path);

            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(filePath + ".xml");
            Common.RemoveComments(xDoc);
            XmlElement? xRoot = xDoc.DocumentElement;

            if (xRoot is { Name: "FCO" })
            {
                tableNoName = Program.currentDir + "/tables/" + (xRoot.Attributes.GetNamedItem("Table")!.Value!);
                Common.fcoTable = tableNoName + ".json";
                Translator.iconsTablePath = "tables/Icons.json";

                foreach (XmlElement node in xRoot)
                {
                    if (node.Name == "Groups")
                    {
                        foreach (XmlElement groupNode in node.ChildNodes)
                        {
                            Group group = new Group();
                            group.groupName = groupNode.Attributes.GetNamedItem("Name")!.Value!;    // Group's Name

                            List<Cell> cells = new List<Cell>();
                            foreach (XmlElement cellNode in groupNode.ChildNodes)
                            {
                                if (cellNode.Name == "Cell")
                                {
                                    Cell cell = new Cell
                                    {
                                        cellName = cellNode.Attributes.GetNamedItem("Name")!.Value!, // Cell's Name
                                        alignment = GetAlignFromString(cellNode.Attributes.GetNamedItem("Alignment")!.Value!)
                                    };


                                    var messageNode = cellNode.ChildNodes[0];
                                    XmlElement ColorNode = cellNode.ChildNodes[1] as XmlElement;
                                    XmlElement ColorNode2 = cellNode.ChildNodes[2] as XmlElement;
                                    XmlElement ColorNode3 = cellNode.ChildNodes[3] as XmlElement;

                                    if (messageNode.Name == "Message")
                                    {
                                        cell.cellMessage = messageNode.Attributes.GetNamedItem("MessageData")!.Value!;
                                        string hexString = Translator.TXTtoHEX(cell.cellMessage);
                                        hexString = hexString.Replace(" ", "");

                                        byte[] messageByteArray = Common.StringToByteArray(hexString);
                                        messageByteArray = Common.StringToByteArray(hexString);
                                        cell.messageCharAmount = hexString.Length / 8;
                                        cell.cellMessageWrite = messageByteArray;
                                    }

                                    if (ColorNode.Name == "ColorMain")
                                    {
                                        Color ColorMain = new Color();
                                        Common.ReadXMLColor(ref ColorMain, ColorNode);
                                        cell.ColorMain = ColorMain;
                                    }

                                    if (ColorNode2.Name == "ColorSub1")
                                    {
                                        Color ColorSub1 = new Color();
                                        Common.ReadXMLColor(ref ColorSub1, ColorNode2);
                                        cell.ColorSub1 = ColorSub1;
                                    }

                                    if (ColorNode3.Name == "ColorSub2")
                                    {
                                        Color ColorSub2 = new Color();
                                        Common.ReadXMLColor(ref ColorSub2, ColorNode3);
                                        cell.ColorSub2 = ColorSub2;
                                    }

                                    List<Color> highlights = new List<Color>();
                                    int workCount = 0;
                                    foreach (XmlElement highlightNode in cellNode.ChildNodes)
                                    {
                                        if (highlightNode.Name == "Highlight" + workCount)
                                        {
                                            Color highlight = new Color();
                                            Common.ReadXMLColor(ref highlight, highlightNode);
                                            highlights.Add(highlight);
                                            cell.highlightList = highlights;
                                            workCount++;
                                        }
                                    }

                                    cells.Add(cell);
                                }
                                group.cellList = cells;
                            }
                            groups.Add(group);
                        }
                    }
                }

                Console.WriteLine("XML read!");
                FCO = true;
            }
            if (xRoot is { Name: "FTE" })
            {
                foreach (XmlElement node in xRoot)
                {
                    if (node.Name == "Textures")
                    {
                        foreach (XmlElement textureNode in node.ChildNodes)
                        {
                            Texture texture = new Texture()
                            {
                                textureName = textureNode.Attributes.GetNamedItem("Name")!.Value!,
                                textureSizeX = int.Parse(textureNode.Attributes.GetNamedItem("Size_X")!.Value!),
                                textureSizeY = int.Parse(textureNode.Attributes.GetNamedItem("Size_Y")!.Value!),
                            };

                            textures.Add(texture);
                            texCount++;
                        }
                    }

                    if (node.Name == "Characters")
                    {
                        foreach (XmlElement charaNode in node.ChildNodes)
                        {
                            Character character = new Character()
                            {
                                textureIndex = int.Parse(charaNode.Attributes.GetNamedItem("TextureIndex")!.Value!),
                                convID = charaNode.Attributes.GetNamedItem("ConverseID")!.Value!,
                                charPoint1X = int.Parse(charaNode.Attributes.GetNamedItem("Point1_X")!.Value!),
                                charPoint1Y = int.Parse(charaNode.Attributes.GetNamedItem("Point1_Y")!.Value!),
                                charPoint2X = int.Parse(charaNode.Attributes.GetNamedItem("Point2_X")!.Value!),
                                charPoint2Y = int.Parse(charaNode.Attributes.GetNamedItem("Point2_Y")!.Value!),
                            };

                            characters.Add(character);
                            charaCount++;
                        }
                    }
                }

                Console.WriteLine("XML read!");
            }
        }


        public static void WriteFTE(string path)
        {
            string filePath = Path.GetDirectoryName(path) + "\\" + Path.GetFileNameWithoutExtension(path);
            File.Delete(Path.Combine(filePath + ".fte"));
            BinaryWriter binaryWriter = new BinaryWriter(File.Open(filePath + ".fte", FileMode.OpenOrCreate));

            // Writing Header
            binaryWriter.Write(Common.EndianSwap(0x00000004));
            binaryWriter.Write(0x00000000);

            // Texture Count
            binaryWriter.Write(Common.EndianSwap(textures.Count));
            for (int t = 0; t < textures.Count; t++)
            {
                binaryWriter.Write(Common.EndianSwap(textures[t].textureName.Length));
                Common.ConvString(binaryWriter, Common.PadString(textures[t].textureName, '@'));
                binaryWriter.Write(Common.EndianSwap(textures[t].textureSizeX));
                binaryWriter.Write(Common.EndianSwap(textures[t].textureSizeY));
            }

            binaryWriter.Write(Common.EndianSwap(characters.Count));
            for (int c = 0; c < characters.Count; c++)
            {
                var textureData = textures[characters[c].textureIndex];
                binaryWriter.Write(Common.EndianSwap(characters[c].textureIndex));
                binaryWriter.Write(Common.EndianSwapFloat(characters[c].charPoint1X / textureData.textureSizeX));
                binaryWriter.Write(Common.EndianSwapFloat(characters[c].charPoint1Y / textureData.textureSizeY));
                binaryWriter.Write(Common.EndianSwapFloat(characters[c].charPoint2X / textureData.textureSizeX));
                binaryWriter.Write(Common.EndianSwapFloat(characters[c].charPoint2Y / textureData.textureSizeY));
            }

            binaryWriter.Close();
            Console.WriteLine("FTE written!");
        }
    }
}