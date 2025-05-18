using Amicitia.IO.Binary;

namespace libfco
{
    public class FontTexture : IBinarySerializable
    {
        public ConverseHeaderPackage Header;
        public List<TextureEntry> Textures = new List<TextureEntry>();
        public List<Character> Characters = new List<Character>();

        public void Read(BinaryObjectReader reader)
        {
            Header = reader.Read<ConverseHeaderPackage>();

            // Textures
            int textureCount = reader.ReadInt32();
            for (int i = 0; i < textureCount; i++)
            {
                Textures.Add(reader.ReadObject<TextureEntry>());
            }
            int totalCharacterCount = reader.ReadInt32();
            int characterID = 100;

            //TODO: Carryover from brianuuu's stuff, what does this do?
            if (Header.Version != 0) Characters.Capacity = 13;

            for (int i = 0; i < totalCharacterCount; i++)
            {
                Character charaData = reader.ReadObject<Character>();
                if (Header.Version != 0)
                    reader.Seek(4, SeekOrigin.Current);
                charaData.CharacterID = characterID;
                Characters.Add(charaData);
                characterID++;
            }
            if (Header.Version == 0)
            {
                for (int i = 0; i < Characters.Count; i++)
                {
                    if (Characters[i].TextureIndex >= 2)
                    {
                        Character chara = Characters[i];
                        chara.CharacterID += 100;
                        Characters[i] = chara;
                    }
                }
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
                writer.Write(Characters[c].TopLeft.X);
                writer.Write(Characters[c].TopLeft.Y);
                writer.Write(Characters[c].BottomRight.X);
                writer.Write(Characters[c].BottomRight.Y);
            }
        }
    }
}