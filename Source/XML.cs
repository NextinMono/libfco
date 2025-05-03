//using System.Numerics;
//using System.Xml;


//namespace libfco
//{
//    public static class XML
//    {
//        public static string tableNoName;
//        public static int texCount = 0, charaCount = 0, spriteIndex = 0;
//        public static bool FCO = false;
//        static List<Group> groups = new List<Group>();
//        public static List<TextureEntry> textures = new List<TextureEntry>();
//        public static List<Character> characters = new List<Character>();

//        static Cell.TextAlign GetAlignFromString(string in_String)
//        {
//            switch (in_String.ToLower())
//            {
//                case "left":
//                    return Cell.TextAlign.Left;
//                case "center":
//                    return Cell.TextAlign.Center;
//                case "right":
//                    return Cell.TextAlign.Right;
//                case "justified":
//                    return Cell.TextAlign.Justified;
//            }
//            return Cell.TextAlign.Left;
//        }
//    }
//}
//public static void ReadXML(string path)
//{
//    string filePath = Path.GetDirectoryName(path) + "\\" + Path.GetFileNameWithoutExtension(path);

//    XmlDocument xDoc = new XmlDocument();
//    xDoc.Load(filePath + ".xml");
//    Common.RemoveComments(xDoc);
//    XmlElement? xRoot = xDoc.DocumentElement;

//    if (xRoot is { Name: "FCO" })
//    {
//        tableNoName = Common.ProgramDir + "/tables/" + (xRoot.Attributes.GetNamedItem("Table")!.Value!);
//        var table = TranslationTable.Read(tableNoName + ".json");
//        TranslationService.iconsTablePath = "tables/Icons.json";

//        foreach (XmlElement node in xRoot)
//        {
//            if (node.Name == "Groups")
//            {
//                foreach (XmlElement groupNode in node.ChildNodes)
//                {
//                    Group group = new Group();
//                    group.Name = groupNode.Attributes.GetNamedItem("Name")!.Value!;    // Group's Name

//                    List<Cell> cells = new List<Cell>();
//                    foreach (XmlElement cellNode in groupNode.ChildNodes)
//                    {
//                        if (cellNode.Name == "Cell")
//                        {
//                            Cell cell = new Cell
//                            {
//                                Name = cellNode.Attributes.GetNamedItem("Name")!.Value!, // Cell's Name
//                                Alignment = GetAlignFromString(cellNode.Attributes.GetNamedItem("Alignment")!.Value!)
//                            };


//                            var messageNode = cellNode.ChildNodes[0];
//                            XmlElement ColorNode = cellNode.ChildNodes[1] as XmlElement;
//                            XmlElement ColorNode2 = cellNode.ChildNodes[2] as XmlElement;
//                            XmlElement ColorNode3 = cellNode.ChildNodes[3] as XmlElement;

//                            if (messageNode.Name == "Message")
//                            {
//                                cell.Message = messageNode.Attributes.GetNamedItem("MessageData")!.Value!;
//                                string hexString = TranslationService.TXTtoHEX(cell.Message, table);
//                                hexString = hexString.Replace(" ", "");

//                                byte[] messageByteArray = Common.StringToByteArray(hexString);
//                                messageByteArray = Common.StringToByteArray(hexString);
//                                cell.MessageLength = hexString.Length / 8;
//                                cell.MessageRawData = messageByteArray;
//                            }

//                            if (ColorNode.Name == "ColorMain")
//                            {
//                                CellColor ColorMain = new CellColor();
//                                Common.ReadXMLColor(ref ColorMain, ColorNode);
//                                cell.MainColor = ColorMain;
//                            }

//                            if (ColorNode2.Name == "ColorSub1")
//                            {
//                                CellColor ColorSub1 = new CellColor();
//                                Common.ReadXMLColor(ref ColorSub1, ColorNode2);
//                                cell.ExtraColor1 = ColorSub1;
//                            }

//                            if (ColorNode3.Name == "ColorSub2")
//                            {
//                                CellColor ColorSub2 = new CellColor();
//                                Common.ReadXMLColor(ref ColorSub2, ColorNode3);
//                                cell.ExtraColor2 = ColorSub2;
//                            }

//                            List<CellColor> highlights = new List<CellColor>();
//                            int workCount = 0;
//                            foreach (XmlElement highlightNode in cellNode.ChildNodes)
//                            {
//                                if (highlightNode.Name == "Highlight" + workCount)
//                                {
//                                    CellColor highlight = new CellColor();
//                                    Common.ReadXMLColor(ref highlight, highlightNode);
//                                    highlights.Add(highlight);
//                                    cell.Highlights = highlights;
//                                    workCount++;
//                                }
//                            }

//                            cells.Add(cell);
//                        }
//                        group.Cells = cells;
//                    }
//                    groups.Add(group);
//                }
//            }
//        }

//        Console.WriteLine("XML read!");
//        FCO = true;
//    }
//    if (xRoot is { Name: "FTE" })
//    {
//        foreach (XmlElement node in xRoot)
//        {
//            if (node.Name == "Textures")
//            {
//                foreach (XmlElement textureNode in node.ChildNodes)
//                {
//                    Texture texture = new Texture()
//                    {
//                        Name = textureNode.Attributes.GetNamedItem("Name")!.Value!,
//                        Size = new Vector2(int.Parse(textureNode.Attributes.GetNamedItem("Size_X")!.Value!), int.Parse(textureNode.Attributes.GetNamedItem("Size_Y")!.Value!))
//                    };

//                    textures.Add(texture);
//                    texCount++;
//                }
//            }

//            if (node.Name == "Characters")
//            {
//                foreach (XmlElement charaNode in node.ChildNodes)
//                {
//                    Character character = new Character()
//                    {
//                        TextureIndex = int.Parse(charaNode.Attributes.GetNamedItem("TextureIndex")!.Value!),
//                        FcoCharacterID = charaNode.Attributes.GetNamedItem("ConverseID")!.Value!,
//                        TopLeft = new Vector2(int.Parse(charaNode.Attributes.GetNamedItem("Point1_X")!.Value!), int.Parse(charaNode.Attributes.GetNamedItem("Point1_Y")!.Value!)),
//                        BottomRight = new Vector2(int.Parse(charaNode.Attributes.GetNamedItem("Point2_X")!.Value!), int.Parse(charaNode.Attributes.GetNamedItem("Point2_Y")!.Value!))
//                    };

//                    characters.Add(character);
//                    charaCount++;
//                }
//            }
//        }

//        Console.WriteLine("XML read!");
//    }
//}
//public void WriteXML(string path)
//{
//    File.Delete(Path.Combine(Path.GetFileNameWithoutExtension(path) + ".xml"));

//    var xmlWriterSettings = new XmlWriterSettings { Indent = true };
//    using var writer = XmlWriter.Create(Path.GetDirectoryName(path) + "\\" +
//    Path.GetFileNameWithoutExtension(path) + ".xml", xmlWriterSettings);

//    writer.WriteStartDocument();
//    writer.WriteStartElement("FCO");
//    writer.WriteAttributeString("Table", TranslationTable.Name);      // This is used later once the XML is used to convert the data back into an FCO format

//    writer.WriteStartElement("Groups");
//    foreach (Group group in Groups)
//    {
//        writer.WriteStartElement("Group");
//        writer.WriteAttributeString("Name", group.Name);

//        foreach (Cell cell in group.Cells)
//        {
//            writer.WriteStartElement("Cell");
//            writer.WriteAttributeString("Name", cell.Name);                     // These parameters are part of the "Cell" Element Header
//            writer.WriteAttributeString("Alignment", cell.Alignment.ToString());
//            // The following Elements are all within the "Cell" Element
//            writer.WriteStartElement("Message");
//            writer.WriteAttributeString("MessageData", cell.Message);
//            writer.WriteEndElement();

//            writer.WriteStartElement("ColorMain");
//            Common.WriteFCOColor(writer, cell.MainColor);
//            writer.WriteEndElement();

//            writer.WriteStartElement("ColorSub1");                                 // Actually figure out what this was again
//            Common.WriteFCOColor(writer, cell.ExtraColor1);
//            writer.WriteEndElement();

//            writer.WriteStartElement("ColorSub2");                                 // Actually figure out what this was again
//            Common.WriteFCOColor(writer, cell.ExtraColor2);
//            writer.WriteEndElement();

//            foreach (CellColor highlight in cell.Highlights)
//            {
//                writer.WriteStartElement("Highlight" + cell.Highlights.IndexOf(highlight));
//                Common.WriteFCOColor(writer, highlight);
//                writer.WriteEndElement();
//            }

//            writer.WriteEndElement();
//        }
//        writer.WriteEndElement();
//    }
//    writer.WriteEndElement();

//    writer.WriteEndDocument();
//    writer.Close();

//    Console.WriteLine("XML written!");
//}

