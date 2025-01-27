namespace SUFcoTool
{
    public class Program
    {
        //public static string currentDir
        //{
        //    get
        //    {
        //        return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        //    }
        //}

        public static string? fileDir, fileName, tableArg;
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("SonicUnleashedFCOConv v1.0\nUsage: SonicUnleashedFCOConv <Path to .fte/.fco/.xml>");
                Console.ReadKey();
            }

            if (args.Length == 2)
            {
                fileDir = Path.GetDirectoryName(args[0]);
                fileName = Path.GetFileName(args[0]);
                tableArg = args[1];
                string file = fileDir + "\\" + fileName;

                if (file.EndsWith(".fco"))
                {
                    //FCO.ReadFCO(args[0]);
                    //FCO.WriteXML(args[0]);
                }
            }
            else
            {
                fileDir = Path.GetDirectoryName(args[0]);
                fileName = Path.GetFileName(args[0]);
                string file = fileDir + "\\" + fileName;

                if (File.Exists(file))
                {
                    if (file.EndsWith(".fte"))
                    {
                        //FTE.ReadFTE(args[0]);
                        //FTE.WriteXML(args[0]);
                    }
                    if (file.EndsWith(".fco"))
                    {
                        //FCO.ReadFCO(args[0]);
                        //FCO.WriteXML(args[0]);
                    }
                    if (file.EndsWith(".xml"))
                    {
                        XML.ReadXML(args[0]);

                        if (XML.FCO)
                        {
                           // if (Common.ErrorCheck() == false) XML.WriteFCO(args[0]);
                        }
                        else
                        {
                            //XML.WriteFTE(args[0]);
                            Common.ExtractCheck();
                        }

                    }
                }
                else
                {
                    Console.WriteLine($"Can't find file " + Path.GetFileName(args[0]) + ", aborting.");
                }
            }
        }
    }
}