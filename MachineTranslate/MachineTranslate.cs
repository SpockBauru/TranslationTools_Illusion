using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.Net.Http;

namespace MachineTranslate
{
    class MachineTranslate
    {
        //Translated Dictionary
        private static Dictionary<string, string> allTranslated = new Dictionary<string, string>();
        //Untranslated Dictionary
        private static Dictionary<string, string> allUntranslated = new Dictionary<string, string>();
        //Machine Translations Dictinary
        private static Dictionary<string, string> machineTranslated = new Dictionary<string, string>();
        //Error Dictionary
        private static Dictionary<string, string> translationErrors = new Dictionary<string, string>();

        //Maintain the HTTP Client open for more speed
        private static readonly HttpClient httpClient = new HttpClient();

        static void Main(string[] args)
        {
            //==================== Defining Languages ===================
            string fromLanguage = "ja";
            string toLanguage = "en";

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


            //==================== Populating Dictionaries ====================
            //Getting all files from folder and subfolders
            FileInfo[] filesInFolder = sourceDir.GetFiles("*.txt", SearchOption.AllDirectories);

            //populating the Translated dictionary with unique translations
            Console.WriteLine("Searching for translated lines in source folder");
            for (int i = 0; i < filesInFolder.Length; i++)
            {
                string fileName = filesInFolder[i].FullName;
                UpdateTranslatedDictionary(fileName);

                string displayFileNumber = "\rFile " + (i + 1).ToString() + " of " + filesInFolder.Length.ToString();
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


            //populating the Untranslated dictionary with unique entries that are not in treanslated dictionary
            Console.WriteLine("Searching for untranslated lines in the source folder");
            for (int i = 0; i < filesInFolder.Length; i++)
            {
                string fileName = filesInFolder[i].FullName;
                UpdateUntranslatedDictionary(fileName);

                string displayFileNumber = "\rFile " + (i + 1).ToString() + " of " + filesInFolder.Length.ToString();
                Console.Write(displayFileNumber);
            }
            Console.WriteLine();


            //==================== Translating via GoogleTranslate ====================
            string rawTranslations = Path.Combine(outputFolder,"1-GoogleTranslateRAW.txt");

            Console.WriteLine("Translating lines with Google Translate:");
            int untranslatedSize = allUntranslated.Count;
            int sleepTimer = 0;

            for (int i = 0; i < untranslatedSize; i++)
            {
                string line = allUntranslated.ElementAt(i).Key;

                string translatedLine;
                translatedLine = GoogleTranslate.Translate(fromLanguage, toLanguage, line, httpClient);
                translatedLine = line + "=" + translatedLine;

                File.AppendAllText(rawTranslations, translatedLine + Environment.NewLine);

                string displayCount = "\rLine " + (i + 1) + " of " + untranslatedSize;
                Console.Write(displayCount);

                //Google translate limit of  translations per second
                Thread.Sleep(200);

                //Sleep after 100 translations so your ip is not banned
                sleepTimer++;
                if (sleepTimer >= 100)
                {
                    Console.Write(" Sleeping for 20 seconds so Google Translate don't ban your IP");
                    sleepTimer = 0;
                    Thread.Sleep(20000);
                    Console.Write("\r                                                                               ");
                }
            }
            Console.WriteLine();


            //==================== Checking for Errors ====================
            Console.WriteLine("Checking for errors from CommonErrors.txt");

            //populating machine dictionary
            filesInOutputFolder = outputDir.GetFiles("*.txt", SearchOption.AllDirectories);
            for (int i = 0; i < filesInOutputFolder.Length; i++)
            {
                string fileName = filesInOutputFolder[i].FullName;

                //Read Current File
                string[] currentFile = File.ReadAllLines(fileName);

                //seek all lines of the current file
                for (int j = 0; j < currentFile.Length; j++)
                {
                    string line = currentFile[j];
                    string[] parts = line.Split('=');
                    if (parts.Length == 2)
                    {
                        string key = parts[0];
                        string value = parts[1];
                        if (!machineTranslated.ContainsKey(key))
                            machineTranslated.Add(key, value);
                        else if (machineTranslated.ContainsKey(key))
                            machineTranslated[key] = value;
                    }
                }
            }

            //populating error dictionary
            string[] errorList = File.ReadAllLines(Path.Combine(thisFolder, "CommonErrors.txt"));
           
            for (int i = 0; i < machineTranslated.Count; i++)
            {
                for (int j = 0; j < errorList.Length; j++)
                {
                    string key = machineTranslated.ElementAt(i).Key;
                    string value = machineTranslated.ElementAt(i).Value;
                    string error = errorList[j];
                    if (value.Contains(error) && !translationErrors.ContainsKey(key))
                    {
                        translationErrors.Add(key, value);
                    }
                }
            }

            //Translating errors with Bing translate
            string bingTranslationsFile = Path.Combine(outputFolder,"2-BingTranslateRAW.txt");
            int errorSize = translationErrors.Count;

            Console.WriteLine("Trying to translate errors with Bing translator");

            int bingIndex = 0;
            for (int i = 0; i < errorSize; i++)
            {
                string[] setup = new string[2];
                if (i==0)
                {
                    setup = BingTranslator.Setup(httpClient);
                }
                string line = translationErrors.ElementAt(i).Key;
                string translatedLine;

                //Translating text between parenthesis separately
                if (line.IndexOfAny("（(".ToCharArray()) != -1)
                {
                    int startindex = line.IndexOfAny("（(".ToCharArray());
                    int endindex = line.IndexOfAny("）)".ToCharArray());
                    int lineLenght = line.Length-1;

                    string before = line.Substring(0, startindex);
                    string translatedBefore= BingTranslator.Translate(fromLanguage, toLanguage, before, httpClient, setup, bingIndex);
                    bingIndex++;

                    startindex++;
                    string between = line.Substring(startindex, endindex - startindex);
                    string translatedBetween= BingTranslator.Translate(fromLanguage, toLanguage, between, httpClient, setup, bingIndex);
                    bingIndex++;

                    string after = line.Substring(endindex + 1, lineLenght - endindex);
                    string translatedAfter = "";
                    if (after.Length > 0)
                    {
                        translatedAfter = BingTranslator.Translate(fromLanguage, toLanguage, after, httpClient, setup, bingIndex);
                        bingIndex++;
                    }

                    translatedLine = translatedBefore + " (" + translatedBetween + ") " + translatedAfter;
                }
                else
                {
                    translatedLine = BingTranslator.Translate(fromLanguage, toLanguage, line, httpClient, setup, bingIndex);
                    bingIndex++;
                }                
                
                translatedLine = line + "=" + translatedLine;

                File.AppendAllText(bingTranslationsFile, translatedLine + Environment.NewLine);

                string displayCount = "\rLine " + (i + 1) + " of " + errorSize;
                Console.Write(displayCount);

                //Bing translate limit of translations per second
                Thread.Sleep(100);
            }
            Console.WriteLine();

            //Updating translated dictionary with Bing translations
            if (File.Exists(bingTranslationsFile))
             {
                string[] bingTranslationsString = File.ReadAllLines(bingTranslationsFile);
                for (int i = 0; i < bingTranslationsString.Length; i++)
                {
                    string line = bingTranslationsString[i];
                    string[] parts = line.Split('=');
                    if (parts.Length == 2)
                    {
                        string key = parts[0];
                        string value = parts[1];
                        machineTranslated[key] = value;
                    }
                }
            }

            //writing machine translations corrected
            string correctedMachineFile = Path.Combine(outputFolder, "3-MachineTranslationsCorrected.txt");
            string[] correctedMachineString = new string[machineTranslated.Count];
            for (int i = 0; i < machineTranslated.Count; i++)
            {
                correctedMachineString[i] = machineTranslated.ElementAt(i).Key + "=" + machineTranslated.ElementAt(i).Value;                
            }
            File.WriteAllLines(correctedMachineFile, correctedMachineString);


            //==================== Fixing Style Errors ====================
            Dictionary<string, string> substitutions = new Dictionary<string, string>();

            Console.WriteLine("Fixing Style errors from Substitutions.txt");

            //Readubg Substitutions file
            string subtitutionsFile = Path.Combine(thisFolder, "Substitutions.txt");
            string[] substitutionsString = File.ReadAllLines(subtitutionsFile);
            for (int i = 0; i < substitutionsString.Length; i++)
            {
                string line = substitutionsString[i];
                string[] parts = line.Split('=');
                if (parts.Length == 2)
                {
                    string key = parts[0];
                    string value = parts[1];
                    substitutions.Add(key, value);
                }
            }

            //Fixing machineTranslated dictionary
            for (int i= 0; i < machineTranslated.Count; i++)
            {
                string key= machineTranslated.ElementAt(i).Key;
                string text = machineTranslated.ElementAt(i).Value;

                for (int j = 0; j < substitutions.Count; j++)
                {
                    string from = substitutions.ElementAt(j).Key;
                    string to = substitutions.ElementAt(j).Value;
                    text= text.Replace(from, to);
                }

                machineTranslated[key] = text;
            }

            //Writing final file
            string machineTranslationsFinalFile = Path.Combine(outputFolder, "MachineTranslationsFinal.txt");
            string[] machineTranslationsFinalString = new string[machineTranslated.Count];
            for (int i = 0; i < machineTranslated.Count; i++)
            {
                machineTranslationsFinalString[i] = machineTranslated.ElementAt(i).Key + "=" + machineTranslated.ElementAt(i).Value;
            }
            File.WriteAllLines(machineTranslationsFinalFile, machineTranslationsFinalString);


            //==================== Ending Console Dialogues ====================
            stopWatch.Stop();

            string display = "Elapsed time " + stopWatch.Elapsed;
            Console.WriteLine(display);
            Console.WriteLine("Press any key to exit");
            Console.WriteLine();
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
                    if (parts.Length == 2)
                    {
                        string key = parts[0];
                        string value = parts[1];
                        if (!allTranslated.ContainsKey(key))
                        {
                            allTranslated.Add(key, value);
                        }
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
                    if (parts.Length == 2)
                    {
                        string key = parts[0];
                        string value = parts[1];
                        if (!allUntranslated.ContainsKey(key) && !allTranslated.ContainsKey(key))
                        {
                            allUntranslated.Add(key, value);
                        }
                    }
                }
            }
        }
    }
}
