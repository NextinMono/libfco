using System.Xml;
using System.Text;
using Amicitia.IO.Binary;

namespace SUFcoTool
{
    public class FTE : IBinarySerializable
    {
        public ConverseHeaderPackage Header;
        public List<Texture> Textures = new List<Texture>();
        public List<Character> Characters = new List<Character>();

        
       
        public void Read(BinaryObjectReader reader)
        {
            // Start Parse
            Header = reader.Read<ConverseHeaderPackage>();

            // Textures
            int textureCount = reader.ReadInt32();

            for (int i = 0; i < textureCount; i++)
            {
                Textures.Add(reader.ReadObject<Texture>());
            }

            // Characters
            int totalCharacterCount = reader.ReadInt32();
            int characterID = 100;
            //TODO: remove this
            bool iconsOver = false;
            //TODO: Carryover from brianuuu's stuff, what does this do?
            if (Header.Field04 != 0) Characters.Capacity = 13;
            for (int i = 0; i < totalCharacterCount; i++)
            {
                Character charaData = reader.ReadObject<Character>();
                if (Header.Field04 != 0)
                    reader.Seek(4, SeekOrigin.Current);
                charaData.FcoCharacterID = characterID.ToString("X8").Insert(2, " ").Insert(5, " ").Insert(8, " ");
                if (Header.Field04 == 0)
                {
                    if (charaData.TextureIndex == 2 && !iconsOver)
                    {
                        characterID += 100;
                        iconsOver = true;
                    }
                }
                Characters.Add(charaData);
                characterID++;
            }
        }

        public void Write(BinaryObjectWriter writer)
        {
            writer.WriteObject(Header);

            // Texture Count
            writer.Write(Textures.Count);
            for (int t = 0; t < Textures.Count; t++)
            {
                writer.WriteObject(Textures[t]);                
            }

            writer.Write(Characters.Count);
            for (int c = 0; c < Characters.Count; c++)
            {
                var textureData = Textures[Characters[c].TextureIndex];
                writer.Write(Characters[c].TextureIndex);
                writer.Write(Common.EndianSwapFloat(Characters[c].TopLeft.X / textureData.Size.X));
                writer.Write(Common.EndianSwapFloat(Characters[c].TopLeft.Y / textureData.Size.Y));
                writer.Write(Common.EndianSwapFloat(Characters[c].BottomRight.X / textureData.Size.X));
                writer.Write(Common.EndianSwapFloat(Characters[c].BottomRight.Y / textureData.Size.Y));
            }
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