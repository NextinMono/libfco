using SUFontTool.FCO;
using System.Numerics;
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
                var table = TranslationTable.Read(tableNoName + ".json");
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
                                        string hexString = Translator.TXTtoHEX(cell.cellMessage, table);
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
                                Name = textureNode.Attributes.GetNamedItem("Name")!.Value!,
                                Size = new Vector2(int.Parse(textureNode.Attributes.GetNamedItem("Size_X")!.Value!), int.Parse(textureNode.Attributes.GetNamedItem("Size_Y")!.Value!))
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
                                TextureIndex = int.Parse(charaNode.Attributes.GetNamedItem("TextureIndex")!.Value!),
                                FcoCharacterID = charaNode.Attributes.GetNamedItem("ConverseID")!.Value!,
                                TopLeft = new Vector2(int.Parse(charaNode.Attributes.GetNamedItem("Point1_X")!.Value!), int.Parse(charaNode.Attributes.GetNamedItem("Point1_Y")!.Value!)),
                                BottomRight = new Vector2(int.Parse(charaNode.Attributes.GetNamedItem("Point2_X")!.Value!), int.Parse(charaNode.Attributes.GetNamedItem("Point2_Y")!.Value!))
                            };

                            characters.Add(character);
                            charaCount++;
                        }
                    }
                }

                Console.WriteLine("XML read!");
            }
        }


        
    }
}