using System;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StyleCheck
{
    class StyleCheck
    {
        static void Main(string[] args)
        {
            //==================== Folder Management ====================
            //Read Folder with text to be translated
            string mainFile;

            if (args.Length != 0) { mainFile = args[0]; }
            else
            {
                Console.Write("Enter the source file path: ");
                mainFile = Console.ReadLine();
                Console.WriteLine();
                if (string.IsNullOrEmpty(mainFile))
                {
                    Console.Write("Invalid");
                    Console.ReadKey();
                    Environment.Exit(0);
                }
            }

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            //check if folder exists
            if (!File.Exists(mainFile))
            {
                Console.WriteLine("Folder Not Found.");
                Console.ReadKey();
                Environment.Exit(0);
            }

            //Making the output path the same as the .exe
            string thisFolder = AppDomain.CurrentDomain.BaseDirectory;

            //==================== Reading Substitutions.txt ====================
            Console.WriteLine("Reading Substitutions.txt");
            string subtitutionsFile = Path.Combine(thisFolder, "Substitutions.txt");
            string[] substitutionsString = File.ReadAllLines(subtitutionsFile);
            

            //Cleaning substitutions file
            List<string> list = new List<string>();

            for (int i = 0; i < substitutionsString.Length; i++)
            {
                string line = substitutionsString[i];
                if (!string.IsNullOrEmpty(line) && !line.StartsWith("//"))
                {
                    list.Add(line);
                }
            }
            substitutionsString = list.ToArray();

            //Populating matrix substitutions[from,to] (must follow the order)
            int substitutionsLenght = substitutionsString.Length;
            string[] substitutionsFrom = new string[substitutionsLenght];
            string[] substitutionsTo = new string[substitutionsLenght];

            for (int i = 0; i < substitutionsLenght; i++)
            {
                string line = substitutionsString[i];

                //seeking for regex
                if (line.StartsWith("r:\""))
                {
                    int separator = line.IndexOf("\"=\"");
                    string from = line.Substring(0, separator);
                    separator += 3;

                    //taking out the " at the end
                    int endIndex = line.Length - 1;
                    string to = line.Substring(separator, endIndex - separator);

                    substitutionsFrom[i] = from;
                    substitutionsTo[i] = to;
                }
                //if its not a regex
                else
                {
                    string[] parts = line.Split('=');
                    if (parts.Length == 2)
                    {
                        string from = parts[0];
                        string to = parts[1];
                        substitutionsFrom[i] = from;
                        substitutionsTo[i] = to;
                    }
                    else Console.WriteLine("Substitution error: " + line);
                }
            }


            //==================== Making the substitutions ====================
            string display = "Making substitutions in " + mainFile;
            Console.WriteLine(display);
            //Reading the file
            string[] allText = File.ReadAllLines(mainFile);

            //Substituting according rules in substitution.txt
            for (int i = 0; i < allText.Length; i++)
            {
                string line = allText[i];
                if (!string.IsNullOrEmpty(line))
                {
                    char[] separator = new char[] { '=' };
                    string[] parts = line.Split(separator, 2);
                    if (parts.Length == 2)
                    {
                        string key = parts[0];
                        string text = parts[1];

                        //parsing all substititions rules
                        for (int j = 0; j < substitutionsLenght; j++)
                        {
                            string from = substitutionsFrom[j];
                            string to = substitutionsTo[j];

                            //if its a regex
                            if (from.StartsWith("r:\""))
                            {
                                //striping the r:"
                                from = from.Substring(3, from.Length - 3);
                                text = Regex.Replace(text, from, to);
                            }
                            else
                            {
                                text = text.Replace(from, to);
                            }
                        }
                        line = key + "=" + text;
                    }
                }
                allText[i] = line;
            }

            //==================== Writing final file ====================
            string outputFile = mainFile.Replace(".txt", "_Cleaned.txt");
            File.WriteAllLines(outputFile, allText);

            //==================== Ending Console Dialogues ====================
            stopWatch.Stop();

            display = "Elapsed time " + stopWatch.Elapsed;
            Console.WriteLine(display);
            Console.WriteLine("Press ENTER to exit");
            Console.ReadLine();
        }
    }
}
