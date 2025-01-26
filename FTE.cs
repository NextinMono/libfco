using System.Xml;
using System.Text;
using System.Numerics;


namespace SUFcoTool
{
    public class FTE
    {
        public List<Texture> Textures = new List<Texture>();
        public List<Character> Characters = new List<Character>();

        public static FTE Read(string in_Path)
        {
            FileStream fileStream = new FileStream(in_Path, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fileStream);
            Encoding UTF8Encoding = Encoding.GetEncoding("UTF-8");
            FTE fTE = new FTE();
            try
            {
                // Start Parse
                binaryReader.ReadInt64();   // This is always the same

                // Textures
                int textureCount = Common.EndianSwap(binaryReader.ReadInt32());

                for (int i = 0; i < textureCount; i++)
                {
                    Texture textureData = new Texture();

                    // Texture Name
                    textureData.Name = UTF8Encoding.GetString(binaryReader.ReadBytes(Common.EndianSwap(binaryReader.ReadInt32())));
                    Common.SkipPadding(binaryReader);

                    textureData.Size = new Vector2(Common.EndianSwap(binaryReader.ReadInt32()), Common.EndianSwap(binaryReader.ReadInt32()));

                    fTE.Textures.Add(textureData);
                }

                // Characters
                int charaCount = Common.EndianSwap(binaryReader.ReadInt32());

                int CurrentID = 100;
                bool IndexChange = false;

                for (int i = 0; i < charaCount; i++)
                {
                    Character charaData = new Character();

                    charaData.TextureIndex = Common.EndianSwap(binaryReader.ReadInt32());

                    if (charaData.TextureIndex == 2 && IndexChange == false)
                    {
                        CurrentID += 100;
                        IndexChange = true;
                    }

                    charaData.FcoCharacterID = CurrentID.ToString("X8").Insert(2, " ").Insert(5, " ").Insert(8, " ");

                    var TexSizeX = fTE.Textures[charaData.TextureIndex].Size.X;
                    var TexSizeY = fTE.Textures[charaData.TextureIndex].Size.Y;

                    charaData.TopLeft = new Vector2(TexSizeX * Common.EndianSwapFloat(binaryReader.ReadSingle()), TexSizeY * Common.EndianSwapFloat(binaryReader.ReadSingle()));
                    charaData.BottomRight = new Vector2(TexSizeX * Common.EndianSwapFloat(binaryReader.ReadSingle()), TexSizeY * Common.EndianSwapFloat(binaryReader.ReadSingle()));
                 

                    fTE.Characters.Add(charaData);
                    CurrentID++;
                }
            }
            catch (EndOfStreamException e)
            {
                Console.WriteLine(e);

                Console.WriteLine("\nERROR: Exception occurred during parsing at: 0x" + unchecked((int)binaryReader.BaseStream.Position).ToString("X") + ".");
                Console.WriteLine("There is a structural abnormality within the FTE file!");
                Console.WriteLine("\nPress Enter to Exit.");
                Console.Read();

                throw;
            }

            binaryReader.Close();
            return fTE;
        }
        public void WriteFTE(string path)
        {
            string filePath = Path.GetDirectoryName(path) + "\\" + Path.GetFileNameWithoutExtension(path);
            File.Delete(Path.Combine(filePath + ".fte"));
            BinaryWriter binaryWriter = new BinaryWriter(File.Open(filePath + ".fte", FileMode.OpenOrCreate));

            // Writing Header
            binaryWriter.Write(Common.EndianSwap(0x00000004));
            binaryWriter.Write(0x00000000);

            // Texture Count
            binaryWriter.Write(Common.EndianSwap(Textures.Count));
            for (int t = 0; t < Textures.Count; t++)
            {
                binaryWriter.Write(Common.EndianSwap(Textures[t].Name.Length));
                Common.ConvString(binaryWriter, Common.PadString(Textures[t].Name, '@'));
                binaryWriter.Write(Common.EndianSwap((int)Textures[t].Size.X));
                binaryWriter.Write(Common.EndianSwap((int)Textures[t].Size.Y));
            }

            binaryWriter.Write(Common.EndianSwap(Characters.Count));
            for (int c = 0; c < Characters.Count; c++)
            {
                var textureData = Textures[Characters[c].TextureIndex];
                binaryWriter.Write(Common.EndianSwap(Characters[c].TextureIndex));
                binaryWriter.Write(Common.EndianSwapFloat(Characters[c].TopLeft.X / textureData.Size.X));
                binaryWriter.Write(Common.EndianSwapFloat(Characters[c].TopLeft.Y / textureData.Size.Y));
                binaryWriter.Write(Common.EndianSwapFloat(Characters[c].BottomRight.X / textureData.Size.X));
                binaryWriter.Write(Common.EndianSwapFloat(Characters[c].BottomRight.Y / textureData.Size.Y));
            }

            binaryWriter.Close();
            Console.WriteLine("FTE written!");
        }
        public void WriteXML(string path)
        {
            File.Delete(Path.Combine(Path.GetFileNameWithoutExtension(path) + ".xml"));

            var xmlWriterSettings = new XmlWriterSettings { Indent = true };
            using var writer = XmlWriter.Create(Path.GetDirectoryName(path) + "\\" +
            Path.GetFileNameWithoutExtension(path) + ".xml", xmlWriterSettings);

            writer.WriteStartDocument();
            writer.WriteStartElement("FTE");

            writer.WriteStartElement("Textures");
            foreach (Texture texture in Textures)
            {
                writer.WriteStartElement("Texture");
                writer.WriteAttributeString("Name", texture.Name);
                writer.WriteAttributeString("Size_X", texture.Size.X.ToString());
                writer.WriteAttributeString("Size_Y", texture.Size.Y.ToString());
                writer.WriteEndElement();
            }
            writer.WriteEndElement();

            writer.WriteComment("ConverseID = Hex, Points = Px, Point1 = TopLeft, Point2 = BottomRight");

            writer.WriteStartElement("Characters");
            foreach (Character character in Characters)
            {
                writer.WriteStartElement("Character");
                writer.WriteAttributeString("TextureIndex", character.TextureIndex.ToString());
                writer.WriteAttributeString("ConverseID", character.FcoCharacterID);
                writer.WriteAttributeString("Point1_X", character.TopLeft.X.ToString());
                writer.WriteAttributeString("Point1_Y", character.TopLeft.Y.ToString());
                writer.WriteAttributeString("Point2_X", character.BottomRight.X.ToString());
                writer.WriteAttributeString("Point2_Y", character.BottomRight.Y.ToString());
                writer.WriteEndElement();
            }
            writer.WriteEndElement();

            writer.WriteEndDocument();
            writer.Close();

            Textures.Clear();
            Characters.Clear();

            Console.WriteLine("XML written!");
        }
    }
}