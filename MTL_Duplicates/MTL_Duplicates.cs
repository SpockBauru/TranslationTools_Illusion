using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Linq;

namespace MTL_Duplicates
{
    class MTL_Duplicates
    {
        //Translated Dictionary
        private static Dictionary<string, string> allTranslated = new Dictionary<string, string>();

        static void Main(string[] args)
        {
            //----------------------------------------
            // Folder Management
            //----------------------------------------

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

            //-----------------------------------------
            //Main Code
            //-----------------------------------------

            //Getting all translation.txt from folder and subfolders
            FileInfo[] filesTranslationTxt = currDir.GetFiles("translation.txt", SearchOption.AllDirectories);

            //Getting all zz_machineTranslation.txt from folder and subfolders
            FileInfo[] filesMachineTranslation = currDir.GetFiles("zz_machineTranslation.txt", SearchOption.AllDirectories);

            //populating the dictionary
            Console.WriteLine("Searching for all translations...");
            filesTranslationTxt = filesTranslationTxt.OrderByDescending(f => f.LastWriteTime).ThenByDescending(f => f.FullName).ToArray();
            foreach (FileInfo fileName in filesTranslationTxt)
            {
                UpdateDictionary(fileName.FullName);
            }

            //overwriting untranslated lines that are present in dictionary
            Console.WriteLine("Writing translations in commented lines...");
            foreach (FileInfo fileName in filesTranslationTxt)
            {
                WriteNewTranslations(fileName.FullName);
            }

            //comenting translated lines in machine translation
            Console.WriteLine("Commenting lines in Machine Translation...");
            foreach (FileInfo fileName in filesMachineTranslation)
            {
                CommentTranslation(fileName.FullName);
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
            foreach (string line in currentFile)
            {
                //Null Check and add uncommented lines to translated dictionary
                if ((!string.IsNullOrEmpty(line)) && (!line.StartsWith("//")))
                {
                    string[] parts = line.Split('=');
                    if (!allTranslated.ContainsKey(parts[0]) && (parts.Length == 2))
                    {
                        allTranslated.Add(parts[0], parts[1]);
                    }
                }
            }
        }

        static void WriteNewTranslations(string fileName)
        {
            //Read Current File
            string[] currentFile = File.ReadAllLines(fileName);
            Boolean fileChanged = false;

            //seek all lines in the current file
            for (int i = 0; i < currentFile.Length; i++)
            {
                string line = currentFile[i];

                //Null Check and see if lines are in dictionary already. Add/Update translation if positive.
                if ((!string.IsNullOrEmpty(line)) && line.Contains("="))
                {
                    string[] parts = line.Split('=');

                    if (parts[0].StartsWith("//"))
                        parts[0] = parts[0].TrimStart('/');

                    //compare if translation in dictionary is different OR if line is commented
                    if ((allTranslated.ContainsKey(parts[0]) && allTranslated[parts[0]] != parts[1]) || (allTranslated.ContainsKey(parts[0]) && line.StartsWith("//")))
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

        static void CommentTranslation(string fileName)
        {
            //Read Current File
            string[] currentFile = File.ReadAllLines(fileName);
            Boolean fileChanged = false;

            //seek all lines of the current file
            for (int i = 0; i < currentFile.Length; i++)
            {
                string line = currentFile[i];

                //Null Check and see if Uncommented lines are in dictionary already. Adds comment if positive.
                if ((!string.IsNullOrEmpty(line)) && !line.StartsWith("//") && line.Contains("="))
                {                    
                    string[] parts = line.Split('=');
                    if (allTranslated.ContainsKey(parts[0]))
                    {
                        line = "//" + line;
                        currentFile[i] = line;
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
