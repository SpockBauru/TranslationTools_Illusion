using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace MissingMTL
{
    class MissingTranslations
    {
        //Dictionary with all translations from both source and output folder
        private static Dictionary<string, string> allTranslated = new Dictionary<string, string>();
        //Dictionary with untranslated lines just from source folder
        private static Dictionary<string, string> sourceUntranslated = new Dictionary<string, string>();
        //Dictionary with lines that are in Untranslated but not in Translated
        private static Dictionary<string, string> toTranslate = new Dictionary<string, string>();

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
            string outputFolder = Path.Combine(thisFolder, "MissingTranslations");
            Directory.CreateDirectory(outputFolder);
            DirectoryInfo outputDir = new DirectoryInfo(outputFolder);

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

            //populating the Untranslated dictionary with unique entries from source folder
            Console.WriteLine("Searching for untranslated lines in the source folder");
            for (int i = 0; i < filesInSourceFolder.Length; i++)
            {
                string fileName = filesInSourceFolder[i].FullName;
                UpdateUntranslatedDictionary(fileName);

                string displayFileNumber = "\rFile " + (i + 1).ToString() + " of " + filesInSourceFolder.Length.ToString();
                Console.Write(displayFileNumber);
            }
            Console.WriteLine();

            //pupulating toTranslate dictionary with entries that are in Untranslated but not in Translated
            string[] untranslatedArray = sourceUntranslated.Keys.ToArray();
            for (int i = 0; i < untranslatedArray.Length; i++)
            {
                string key = untranslatedArray[i];
                if (!allTranslated.ContainsKey(key))
                    toTranslate.Add(key, "");
            }

            //==================== Writing untranslated entries ====================
            string outputFile = Path.Combine(outputFolder, "Untranslated.txt");

            Console.WriteLine("Writing output file");
            int toTranslateSize = toTranslate.Count;
            Stopwatch translateTimer = new Stopwatch();
            translateTimer.Start();

            for (int i = 0; i < toTranslateSize; i++)
            {
                string line = toTranslate.ElementAt(i).Key;

                File.AppendAllText(outputFile, line + Environment.NewLine);

                string displayCount = "\rLine " + (i + 1) + " of " + toTranslateSize;
                Console.Write(displayCount);
            }
            Console.WriteLine();
        }


        ///<summary>Cleans commented and empty lines</summary>
        static string[] CleanFile(string[] fileContent)
        {
            List<string> list = new List<string>();

            for (int i = 0; i < fileContent.Length; i++)
            {
                string line = fileContent[i];
                if (!string.IsNullOrEmpty(line) && !line.StartsWith("//"))
                {
                    list.Add(line);
                }
            }
            string[] cleanFile = list.ToArray();
            return cleanFile;
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

        /// <summary>Considers untranslated lines starting with "//" and having just one "=". Everything after the "=" will be ignored </summary>
        static void UpdateUntranslatedDictionary(string fileName)
        {
            //Read Current File
            string[] currentFile = File.ReadAllLines(fileName);

            //seek all lines of the current file
            for (int i = 0; i < currentFile.Length; i++)
            {
                string line = currentFile[i];

                //null check and add Commented lines to Untranslated dictionary
                if (!string.IsNullOrEmpty(line) && line.StartsWith("//"))
                {
                    line = line.Replace("/", "");
                    string[] parts = line.Split('=');
                    if (parts.Length == 2)
                    {
                        string key = parts[0];
                        string value = parts[1];
                        if (!sourceUntranslated.ContainsKey(key))
                        {
                            sourceUntranslated.Add(key, "");
                        }
                    }
                }
            }
        }
    }
}
