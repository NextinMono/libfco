using System.Text;
using System.Text.Json;

namespace SUFcoTool
{
    public class Translator
    {
        // To whomever this may concern, I legit don't even know how I wrote some of this.. It may have been a 2AM job...
        static int tableIndex;
        static bool iconCheck = false, fontCheck = false, fontSizeFound = false;
        public static string? iconsTablePath, tempTablePath;
        public static List<string> missinglist = new List<string>();
        public static string[] extraFontFilenames = { "_Small.json", "_Large.json", "_Extra.json" };
        public enum FontSizes { S = 0, L = 1, X = 2 }

        public static string Missing = "?MISSING?";

        public static string stringConv(string tagstr, string fontsize)
        {
            tempTablePath = Common.fcoTable;
            Common.fcoTable = XML.tableNoName + fontsize;
            string hexString = Translator.TXTtoHEX(tagstr);
            Common.fcoTable = tempTablePath;

            return hexString;
        }

        public static void fontmerge(List<string> chunks)
        {
            StringBuilder mergedContent = new StringBuilder();
            bool fontsizer = false;
            int chunkinx = 0, specialinx = 0, thing = 0;
            string[] chunkre = new string[chunks.Count];
            int[] startinx = new int[chunks.Count];
            int[] endinx = new int[chunks.Count];

            foreach (var chunk in chunks)
            {
                if (chunk.Length > 2 && chunk[2] == ':' && chunk.EndsWith("}"))
                {
                    if (fontsizer == false)
                    {
                        specialinx = chunkinx;
                        fontsizer = true;
                    }
                    mergedContent.Append(chunk.Substring(3, 1));
                }

                if (chunk.Length < 2 && fontsizer)
                {
                    startinx[thing] = specialinx;
                    endinx[thing] = chunkinx;
                    chunkre[thing] = chunks[chunkinx - 1].Substring(1, 1) + "," + mergedContent.ToString();
                    mergedContent.Clear();
                    fontsizer = false;
                    Console.WriteLine(startinx[thing] + endinx[thing] + chunkre[thing]);
                    thing++;
                }

                chunkinx++;

                if (chunkinx == chunks.Count && fontsizer)
                {
                    startinx[thing] = specialinx;
                    endinx[thing] = chunkinx;
                    chunkre[thing] = chunks[chunkinx - 1].Substring(1, 1) + "," + mergedContent.ToString();
                    mergedContent.Clear();
                    fontsizer = false;
                    Console.WriteLine(startinx[thing] + endinx[thing] + chunkre[thing]);
                    thing++;
                }
            }

            for (int i = 0; i < thing; i++)
            {
                string rebuilt = chunkre[i];
                string mergedString = "{" + chunkre[i].Substring(0, 1) + ":" + rebuilt.Substring(2, rebuilt.Length - 2) + "}";

                for (int p = startinx[i]; p < endinx[i]; p++)
                {
                    chunks[p] = "{FILL}";
                }

                chunks[startinx[i]] = mergedString;
            }

            chunks.RemoveAll(EndsWithSaurus);
        }
        public static string SearchIconsTable(string in_String, bool in_ToText)
        {
            //If its still missing, try checking the icons table
            using JsonDocument docIcon = JsonDocument.Parse(File.ReadAllText(iconsTablePath));
            return SearchHexStringForLetter(docIcon.RootElement, in_String, in_ToText);
        }
        public static string SearchExtraTables(string hexString)
        {
            string searchResult = Missing;
            //Check if the value could be in the extra town tables (Small, Large, Extra)
            for (int i = 0; i < 3; i++)
            {
                if (File.Exists(Common.fcoTableDir + Common.fcoTableName + extraFontFilenames[i]))
                {
                    using JsonDocument docExtraTable = JsonDocument.Parse(File.ReadAllText(Common.fcoTableDir + Common.fcoTableName + extraFontFilenames[i]));
                    searchResult = SearchHexStringForLetter(docExtraTable.RootElement, hexString, true);
                    if (searchResult != Missing)
                    {
                        tableIndex = i;
                        return searchResult;
                    }
                }
            }
            searchResult = SearchIconsTable(hexString, true);

            return searchResult;
        }

        private static bool EndsWithSaurus(String s)
        {
            return s.Contains("{FILL}");
        }

        public static int sizeDet(string hexString)
        {
            byte[] messageByteArray = Common.StringToByteArray(hexString);
            int numberOfBytes = hexString.Length;
            int MessageCharAmount = numberOfBytes / 8;
            //byte[] CellMessageWrite = messageByteArray;

            return MessageCharAmount;
        }

        public static string HEXtoTXT(string hex)
        {
            List<string> chunks = SplitStringIntoChunks(true, hex, 11);
            fontmerge(chunks);
            return JoinChunks(chunks);
        }

        public static string TXTtoHEX(string hex)
        {
            List<string> chunks = SplitStringIntoChunks(false, hex, 1);
            return JoinChunks(chunks);
        }

        static List<string> SplitStringIntoChunks(bool mode, string str, int chunkSize)
        {
            List<string> chunks = new List<string>();

            string fontsize = "";
            string taghexString;

            if (mode)
            {
                for (int i = 0; i < str.Length; i += chunkSize + 1)
                {
                    if (i + chunkSize > str.Length)
                    {
                        chunks.Add(GetStringFromTable(str.Substring(i), mode));
                    }
                    else
                    {
                        chunks.Add(GetStringFromTable(str.Substring(i, chunkSize), mode));
                    }
                }
            }
            else
            {
                for (int i = 0; i < str.Length; i++)
                {
                    if (str[i] == '{')
                    {

                        // Find the closing '}'
                        int endIndex = str.IndexOf('}', i);
                        // Include the closing '}' in the chunk
                        int specialChunkLength = endIndex - i + 1;

                        //{S:Small Text}, {L:Large Text}, {X:Extra Large Text}
                        if (str[i + 2] == ':')
                        {
                            switch (str[i + 1])
                            {
                                case 'S':
                                    fontsize = extraFontFilenames[0];
                                    break;
                                case 'L':
                                    fontsize = extraFontFilenames[1];
                                    break;
                                case 'X':
                                    fontsize = extraFontFilenames[2];
                                    break;
                                default:
                                    Console.WriteLine("ERROR");
                                    Environment.Exit(0);
                                    break;
                            }

                            // Breaking out substring
                            Console.WriteLine(str[i + 1]);
                            string tagstr = str.Substring(i + 3, endIndex - (i + 3));
                            Console.WriteLine(tagstr);
                            taghexString = stringConv(tagstr, fontsize);

                            int MessageCharAmount = taghexString.Length / 11;
                            int indstart = 0;
                            for (int x = 0; x < MessageCharAmount; x++)
                            {
                                string tempchunk = taghexString.Substring(indstart, 11);
                                Console.WriteLine(tempchunk);
                                chunks.Add(tempchunk);
                                indstart = indstart + 11;
                            }

                            i = endIndex + 1;
                            continue;
                        }

                        //Standard Exit
                        if (endIndex != -1)
                        {
                            chunks.Add(GetStringFromTable(str.Substring(i, specialChunkLength), mode));
                            i += specialChunkLength; // Move the index past this special chunk
                            continue;
                        }
                    }
                    else
                    {
                        int endIndex = i + chunkSize;
                        if (endIndex > str.Length)
                        {
                            endIndex = str.Length;
                        }
                        else
                        {
                            // Ensure not to break in the middle of a "{X}", "{XY}" or "{XYZ}"
                            int nextSpecialIndex = str.IndexOf("{", i, chunkSize);
                            if (nextSpecialIndex != -1 && nextSpecialIndex < endIndex)
                            {
                                endIndex = nextSpecialIndex;
                            }
                        }

                        string chunk = str.Substring(i, endIndex - i);
                        chunks.Add(GetStringFromTable(chunk, mode));
                        i = endIndex;
                    }
                }
            }

            return chunks;
        }

        static string JoinChunks(List<string> chunks)
        {
            return string.Join("", chunks);
        }
        static string GetStringFromTable(string hexString, bool inToText)
        {
            if (!File.Exists(Common.fcoTable))
            {
                Console.WriteLine("\nThis table does not exist\nPlease check your files!\nPress any key to exit.");
                return null;
            }

            //Search for the string in the fco table json
            using JsonDocument doc = JsonDocument.Parse(File.ReadAllText(Common.fcoTable));
            string searchResult = SearchHexStringForLetter(doc.RootElement, hexString, inToText);

            //If it couldn't be found, search the extra tables
            if (searchResult == Missing)
            {
                //Hedgeturd made it so it only checks the icons table if its text to hex, so I did the same
                searchResult = inToText ? SearchExtraTables(hexString) : SearchIconsTable(hexString, false);

                if (searchResult == Missing)
                {
                    missinglist.Add($"\"{hexString}\" not found in the table \"{Common.fcoTableName}\".");
                }
            }

            return searchResult;
        }

        //static string GetStringFromTable(string hexString, bool in_ToText)
        //{
        //    // Read the JSON file
        //    // Parse the JSON string into a JsonDocument
        //
        //    if (File.Exists(Common.fcoTable) == false)
        //    {
        //        Console.WriteLine("\nThis table does not exist\nPlease check your files!\nPress any key to exit.");
        //        return null;
        //    }
        //
        //    using JsonDocument doc = JsonDocument.Parse(File.ReadAllText(Common.fcoTable));
        //
        //    if (in_ToText)
        //    {
        //        string searchResult = SearchHexStringForLetter(doc.RootElement, hexString, true);
        //
        //        //If the string could not be found the first time, try checking the other tables
        //        if (searchResult == Missing)
        //        {
        //            searchResult = SearchExtraTables(hexString);
        //            //If it's still missing, warn user
        //            if (searchResult == Missing)
        //            {
        //                missinglist.Add($"\"{hexString}\" not found in the table \"{Common.fcoTableName}\".");
        //            }
        //        }
        //        return searchResult;
        //    }
        //    else 
        //    {
        //        bool couldBeIcon = hexString.Contains("{");
        //
        //        if (couldBeIcon)
        //        {
        //            string searchResult = SearchHexStringForLetter(doc.RootElement, hexString, false);
        //
        //            if (searchResult == null)
        //            {
        //                using JsonDocument docIcon = JsonDocument.Parse(File.ReadAllText(iconsTablePath));
        //                searchResult = SearchHexStringForLetter(docIcon.RootElement, hexString, false);
        //
        //                if (searchResult == null)
        //                {
        //                    Common.noLetter = true;
        //                    missinglist.Add("Letter: " + hexString + " not found in the table " + Common.fcoTableName + "!");
        //                }
        //
        //                return searchResult;
        //            }
        //
        //            return searchResult;
        //        }
        //        else
        //        {
        //            string searchResult = SearchHexStringForLetter(doc.RootElement, hexString, false);
        //
        //            if (searchResult == null)
        //            {
        //                Common.noLetter = true;
        //                missinglist.Add("Letter: " + hexString + " not found in the table " + Common.fcoTableName + "!");
        //            }
        //
        //            return searchResult;
        //        }
        //    }
        //
        //    return null;
        //}

        static string SearchHexStringForLetter(JsonElement element, string searchKey, bool inToText)
        {
            if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement item in element.EnumerateArray())
                {
                    if (item.TryGetProperty(inToText ? "HexString" : "Letter", out JsonElement keyElement) &&
                        keyElement.GetString() == searchKey &&
                        item.TryGetProperty(inToText ? "Letter" : "HexString", out JsonElement resultElement))
                    {
                        return resultElement.GetString() ?? Missing;
                    }
                }
            }
            else if (element.ValueKind == JsonValueKind.Object)
            {
                foreach (JsonProperty property in element.EnumerateObject())
                {
                    if (property.Value.ValueKind == JsonValueKind.Array || property.Value.ValueKind == JsonValueKind.Object)
                    {
                        string result = SearchHexStringForLetter(property.Value, searchKey, inToText);
                        if (result != null) return result;
                    }
                }
            }

            return Missing;
        }

    }
}