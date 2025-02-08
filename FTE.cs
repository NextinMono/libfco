using System.Xml;
using System.Text;

namespace SUFcoTool
{
    public class FTE
    {
        public ConverseHeaderPackage Header;
        public List<Texture> Textures = new List<Texture>();
        public List<Character> Characters = new List<Character>();

        public static FTE Read(string in_Path, bool in_IsGensTemp = true)
        {
            FileStream fileStream = new FileStream(in_Path, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fileStream);
            Encoding UTF8Encoding = Encoding.GetEncoding("UTF-8");
            FTE fteFile = new FTE();
            try
            {
                // Start Parse
                fteFile.Header = ConverseHeaderPackage.Read(binaryReader);

                // Textures
                int textureCount = Common.EndianSwap(binaryReader.ReadInt32());

                for (int i = 0; i < textureCount; i++)
                {
                    fteFile.Textures.Add(Texture.Read(binaryReader, in_IsGensTemp));
                }

                // Characters
                int charaCount = Common.EndianSwap(binaryReader.ReadInt32());
                int charaIndex = 100;
                bool iconsOver = false;
                if (in_IsGensTemp) fteFile.Characters.Capacity = 13;
                for (int i = 0; i < charaCount; i++)
                {
                    Character charaData = Character.Read(binaryReader, charaIndex, fteFile.Textures, in_IsGensTemp);
                    if (!in_IsGensTemp)
                    {
                        if (charaData.TextureIndex == 2 && !iconsOver)
                        {
                            charaIndex += 100;
                            iconsOver = true;
                        }
                    }
                    fteFile.Characters.Add(charaData);
                    charaIndex++;
                }
            }
            catch (EndOfStreamException e)
            {
                throw;
            }

            binaryReader.Close();
            return fteFile;
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