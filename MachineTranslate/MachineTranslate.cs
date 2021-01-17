using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Threading;

namespace MachineTranslate
{
    class MachineTranslate
    {
        //Translated Dictionary
        private static Dictionary<string, string> allTranslated = new Dictionary<string, string>();
        //Translated Dictionary
        private static Dictionary<string, string> allUntranslated = new Dictionary<string, string>();

        static void Main(string[] args)
        {
            //Defining languages
            string fromLanguage = "ja";
            string toLanguage = "en";

            //Read Current Folder
            string mainFolder;

            if (args.Length != 0) { mainFolder = args[0]; }
            else
            {
                Console.Write("Enter the folder path: ");
                mainFolder = Console.ReadLine();
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
            DirectoryInfo currDir = new DirectoryInfo(mainFolder);
            Console.WriteLine(currDir.FullName);

            //check if folder exists
            if (!currDir.Exists)
            {
                Console.WriteLine("Folder Not Found.");
                Console.ReadKey();
                Environment.Exit(0);
            }

            //Getting all files from folder and subfolders
            FileInfo[] filesInFolder = currDir.GetFiles("*.txt", SearchOption.AllDirectories);

            //populating the Translated dictionary with unique translations
            Console.WriteLine("Searching for translations in all files");
            for (int i = 0; i < filesInFolder.Length; i++)
            {
                string fileName = filesInFolder[i].FullName;
                UpdateTranslatedDictionary(fileName);

                string displayFileNumber = "\rFile " + (i + 1).ToString() + " of " + filesInFolder.Length.ToString();
                Console.Write(displayFileNumber);
            }
            Console.WriteLine();

            //populating the Untranslated dictionary with unique entries that are not in treanslated dictionary
            Console.WriteLine("Searching for Untranslated lines");
            for (int i = 0; i < filesInFolder.Length; i++)
            {
                string fileName = filesInFolder[i].FullName;
                UpdateUntranslatedDictionary(fileName);

                string displayFileNumber = "\rFile " + (i + 1).ToString() + " of " + filesInFolder.Length.ToString();
                Console.Write(displayFileNumber);
            }
            Console.WriteLine();

            //setting Output folder and file
            string outputFolder = mainFolder + "\\MachineTranslation\\";            
            Directory.CreateDirectory(outputFolder);

            string OutputFile = outputFolder + "MachineTranslation.txt";

            //Translating via GoogleTranslate
            Console.WriteLine("Translating lines:");
            int translationSize = allUntranslated.Count;
            int sleepTimer = 0;

            for (int i = 0; i < translationSize; i++)
            {
                string line = allUntranslated.ElementAt(i).Key;

                string translatedLine;
                translatedLine = GoogleTranslate.Translate(fromLanguage, toLanguage, line);
                translatedLine = line + "="+ translatedLine;

                File.AppendAllText(OutputFile, translatedLine + Environment.NewLine);

                string displayCount = "\rLine " + (i + 1) + " of " + translationSize;
                Console.Write(displayCount);

                //Google translate limit of 5 translations per second
                Thread.Sleep(200);

                //Sleep after 100 translations so your ip is not banned
                sleepTimer++;
                if (sleepTimer>=100)
                {
                    Console.WriteLine();
                    Console.WriteLine("sleeping for 10 seconds so Google Translate don't ban your IP");
                    sleepTimer = 0;
                    Thread.Sleep(10000);
                }

            }
            Console.WriteLine();

            //ending console dialogues
            stopWatch.Stop();
            
            string display= "Elapsed time " + stopWatch.Elapsed;
            Console.WriteLine(display);
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        static void UpdateTranslatedDictionary(string fileName)
        {
            //Read Current File
            string[] currentFile = File.ReadAllLines(fileName);

            //seek all lines of the current file
            for (int i = 0; i < currentFile.Length; i++)
            {
                string line = currentFile[i];

                //null check and add Uncommented lines to Translated dictionary
                if ((!string.IsNullOrEmpty(line)) && (!line[0].Equals('/')))
                {
                    string[] parts = line.Split('=');
                    if (!allTranslated.ContainsKey(parts[0]) && (parts.Length == 2))
                    {
                        allTranslated.Add(parts[0], parts[1]);
                    }
                }
            }
        }

        static void UpdateUntranslatedDictionary(string fileName)
        {
            //Read Current File
            string[] currentFile = File.ReadAllLines(fileName);

            //seek all lines of the current file
            for (int i = 0; i < currentFile.Length; i++)
            {
                string line = currentFile[i];

                //null check and add Commented lines to Untranslated dictionary
                if ((!string.IsNullOrEmpty(line)) && (line[0].Equals('/')))
                {
                    line = line.Replace("/", "");
                    string[] parts = line.Split('=');
                    if (!allUntranslated.ContainsKey(parts[0]) && !allTranslated.ContainsKey(parts[0]) && (parts.Length == 2))
                    {
                        allUntranslated.Add(parts[0], parts[1]);
                    }
                }
            }
        }
    }
}
