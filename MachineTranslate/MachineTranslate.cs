using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;

namespace MachineTranslate
{
    class MachineTranslate
    {
        //Translated Dictionary
        private static Dictionary<string, string> allTranslated = new Dictionary<string, string>();
        //Translated Dictionary
        private static Dictionary<string, string> allUnranslated = new Dictionary<string, string>();

        static void Main(string[] args)
        {
            //Defining languages
            string fromLanguage = "ja";
            string toLanguage = "pt";

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

            //populating the translated and untranslated dictionaries
            Console.WriteLine("Searching for all translations...");
            foreach (FileInfo fileName in filesInFolder)
            {
                UpdateDictionaries(fileName.FullName);
            }

            //Creating string arrays with translations
            string[] outputTranslated = DictionaryToStringArray(allTranslated);
            string[] outputUntranslated = DictionaryToStringArray(allUnranslated);

            //Writing translated lines
            Console.WriteLine("Writing files");
            string outputFolder = mainFolder + "\\MachineTranslation\\";
            Directory.CreateDirectory(outputFolder);
            File.WriteAllLines(outputFolder + "Translated.txt", outputTranslated);
            File.WriteAllLines(outputFolder + "Untranslated.txt", outputUntranslated);

            //Translating via GoogleTranslate
            int translationSize = outputUntranslated.Length;
            string[] machineTranslations = new string[translationSize];
            for (int i = 0; i < translationSize; i++)
            {
                string line = outputUntranslated[i].Replace("/", "");                
                line = line.Replace("=", "");

                string translatedLine;
                translatedLine = GoogleTranslate.Translate(fromLanguage, toLanguage, line);
                machineTranslations[i] = line + "=" + translatedLine;

                string displayCount = "\rLine " + (i+1) + " of " + translationSize;
                Console.Write(displayCount);
            }

            File.WriteAllLines(outputFolder + "MachineTranslation.txt", machineTranslations);
            stopWatch.Stop();
            Console.WriteLine();
            Console.WriteLine(stopWatch.Elapsed);
            Console.ReadKey();
        }

        static string[] DictionaryToStringArray(Dictionary<string, string> dictionary)
        {
            int size = dictionary.Keys.Count;
            string[] stringArray = new string[size];
            for (int i = 0; i < size; i++)
            {
                stringArray[i] = dictionary.ElementAt(i).Key + "=" + dictionary.ElementAt(i).Value;
            }
            return stringArray;
        }

        static void UpdateDictionaries(string fileName)
        {
            //Read Current File
            string[] currentFile = File.ReadAllLines(fileName);

            //seek all lines of the current file
            for (int i = 0; i <= currentFile.Length - 1; i++)
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
                //null check and add Commented lines to Untranslated dictionary
                else if ((!string.IsNullOrEmpty(line)) && (line[0].Equals('/')))
                {
                    string[] parts = line.Split('=');
                    if (!allUnranslated.ContainsKey(parts[0]) && (parts.Length == 2))
                    {
                        allUnranslated.Add(parts[0], parts[1]);
                    }
                }
            }
        }
    }
}
