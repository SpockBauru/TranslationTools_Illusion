using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace TranslateDuplicates
{
    class TranslateDuplicates
    {
        //Translated Dictionary
        private static Dictionary<string, string> allTranslated = new Dictionary<string, string>();

        static void Main(string[] args)
        {
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

            //populating the dictionary
            Console.WriteLine("Searching for all translations...");
            foreach (FileInfo fileName in filesInFolder)
            {
                UpdateDictionary(fileName.FullName);
            }

            //overwriting untranslated lines that are present in dictionary
            Console.WriteLine("Writing translations in commented lines...");
            foreach (FileInfo fileName in filesInFolder)
            {
                WriteUntranslated(fileName.FullName);
            }

            //Finishing
            Console.WriteLine("Finished!");
            stopWatch.Stop();
            Console.WriteLine("Time Spent:" + stopWatch.Elapsed);
            Console.ReadKey();
        }

        static void UpdateDictionary(string fileName)
        {
            //Read Current File
            string[] currentFile = File.ReadAllLines(fileName);

            //seek all lines of the current file
            for (int i = 0; i <= currentFile.Length - 1; i++)
            {
                string line = currentFile[i];

                //Null Check and add uncommented lines to translated dictionary
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

        static void WriteUntranslated(string fileName)
        {
            //Read Current File
            string[] currentFile = File.ReadAllLines(fileName);
            Boolean fileChanged = false;

            //seek all lines of the current file
            for (int i = 0; i <= currentFile.Length - 1; i++)
            {
                string line = currentFile[i];

                //Null Check and see if commented lines are in dictionary already. Adds translation if positive.
                if ((!string.IsNullOrEmpty(line)) && line.StartsWith("//") && line.Contains("="))
                {
                    string uncommented = line.Replace("//", "");
                    string[] parts = uncommented.Split('=');
                    if (allTranslated.ContainsKey(parts[0]))
                    {
                        currentFile[i] = parts[0] + "=" + allTranslated[parts[0]];
                        fileChanged = true;
                    }
                }
            }

            //Overwriting file
            if (fileChanged)
            {
                File.WriteAllLines(fileName, currentFile);
            }

        }
    }
}

