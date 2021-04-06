using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplitMTL
{
    class SplitMtl
    {
        //Dictionary with all translations from both source and output folder
        private static Dictionary<string, string> allTranslated = new Dictionary<string, string>();

        static void Main(string[] args)
        {
            //==================== Folder Management ====================
            //Read Folder with text to be translated
            string mainFolder;

            if (args.Length != 0) { mainFolder = args[0]; }
            else
            {
                Console.Write("Enter the source folder path: ");
                mainFolder = Console.ReadLine();
                Console.WriteLine();
                if (string.IsNullOrEmpty(mainFolder))
                {
                    Console.Write("Invalid");
                    Console.ReadKey();
                    Environment.Exit(0);
                }
            }

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            //get data from main folder
            DirectoryInfo sourceDir = new DirectoryInfo(mainFolder);

            //check if folder exists
            if (!sourceDir.Exists)
            {
                Console.WriteLine("Folder Not Found.");
                Console.ReadKey();
                Environment.Exit(0);
            }

            //Making the output path the same as the .exe
            string thisFolder = AppDomain.CurrentDomain.BaseDirectory;
            string outputFolder = Path.Combine(thisFolder, "MachineTranslation");
            Directory.CreateDirectory(outputFolder);
            DirectoryInfo outputDir = new DirectoryInfo(outputFolder);

            //Reading the Header file
            string[] warningHeader = File.ReadAllLines("Header.txt");

            //==================== Populating Dictionaries ====================
            //Getting all files from folder and subfolders
            FileInfo[] filesInSourceFolder = sourceDir.GetFiles("*.txt", SearchOption.AllDirectories);

            //populating the Translated dictionary with unique translations from source folder
            Console.WriteLine("Searching for translated lines in source folder");
            for (int i = 0; i < filesInSourceFolder.Length; i++)
            {
                string fileName = filesInSourceFolder[i].FullName;
                UpdateTranslatedDictionary(fileName);

                string displayFileNumber = "\rFile " + (i + 1).ToString() + " of " + filesInSourceFolder.Length.ToString();
                Console.Write(displayFileNumber);
            }
            Console.WriteLine();

            //populating the Translated dictionary with translations from the output folder (in case of a second run)
            Console.WriteLine("Searching for translated lines in the output folder");
            FileInfo[] filesInOutputFolder = outputDir.GetFiles("*.txt", SearchOption.AllDirectories);
            for (int i = 0; i < filesInOutputFolder.Length; i++)
            {
                string fileName = filesInOutputFolder[i].FullName;
                UpdateTranslatedDictionary(fileName);

                string displayFileNumber = "\rFile " + (i + 1).ToString() + " of " + filesInOutputFolder.Length.ToString();
                Console.Write(displayFileNumber);
            }
            Console.WriteLine();


            //==================== Writing machine translations from untranslated text for each file ====================

            //Read each file from input directory and write in output directory the untranslated text with the MTL
            Console.WriteLine("Splitting files to MachineTranslation folder");
            for (int i = 0; i < filesInSourceFolder.Length; i++)
            {
                //current file
                string currentFile = filesInSourceFolder[i].FullName;

                if (currentFile.EndsWith("translation.txt"))
                {
                    string[] allLines = File.ReadAllLines(currentFile);

                    //output file
                    List<string> outputFile = new List<string>();
                    //output file header
                    outputFile.AddRange(warningHeader);

                    //seek all untranslated lines of the current file, adds translation to this line and write in the output array
                    bool isTranslated = true;
                    foreach (string line in allLines)
                    {
                        if ((!string.IsNullOrEmpty(line)) && line.StartsWith("//") && line.Contains("="))
                        {
                            string outputLine = line.Replace("/", "");
                            char[] separator = new char[] { '=' };
                            string[] parts = outputLine.Split(separator, 2);

                            //Adding translations to untranslated line
                            if (parts.Length == 2)
                            {
                                string key = parts[0];
                                string value = parts[1];
                                if (allTranslated.ContainsKey(key))
                                {
                                    isTranslated = false;

                                    value = allTranslated[key];
                                    outputLine = key + "=" + value;
                                    outputFile.Add(outputLine);
                                }
                            }
                        }
                    }

                    //writing file to a new dir
                    if (!isTranslated)
                    {
                        string outputPath = currentFile.Replace(mainFolder, outputFolder);
                        outputPath = outputPath.Replace("translation.txt", "zz_machineTranslation.txt");
                        string[] outputFileText = outputFile.ToArray();
                        Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
                        File.WriteAllLines(outputPath, outputFileText);
                    }
                }
            }


            //==================== Ending Console Dialogues ====================
            stopWatch.Stop();

            string display = "Elapsed time " + stopWatch.Elapsed;
            Console.WriteLine(display);
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }


        ///<summary>Seek for new translations in the file. Substitute if the translation exists. </summary>
        static void UpdateTranslatedDictionary(string fileName)
        {
            //Read Current File
            string[] currentFile = File.ReadAllLines(fileName);

            //seek all lines of the current file
            for (int i = 0; i < currentFile.Length; i++)
            {
                string line = currentFile[i];

                //null check and add Uncommented lines to Translated dictionary
                if (!string.IsNullOrEmpty(line) && !line.StartsWith("//"))
                {
                    char[] separator = new char[] { '=' };
                    string[] parts = line.Split(separator, 2);
                    if (parts.Length == 2)
                    {
                        string key = parts[0];
                        string value = parts[1];
                        if (!allTranslated.ContainsKey(key))
                        {
                            allTranslated.Add(key, value);
                        }
                        //adds new values
                        else if (allTranslated.ContainsKey(key) && (value != ""))
                        {
                            allTranslated[key] = value;
                        }
                    }
                }
            }
        }
    }
}
