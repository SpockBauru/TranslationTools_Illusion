using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;

namespace ReleaseToolHS2
{
    class ReleaseToolHS2
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Release Tool for Honey Select 2 \r\n");

            string inputRoot;

            //Read input folder from args, otherwise ask for user
            if (args.Length != 0) inputRoot = args[0];
            else
            {
                Console.Write("Enter GitHub folder path: ");
                inputRoot = Console.ReadLine();
                Console.WriteLine();
            }


            //Stopwatch because I like it
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            //Making the output path the same as the .exe
            string thisFolder = AppDomain.CurrentDomain.BaseDirectory;
            string outputRoot = Path.Combine(thisFolder, "workFolder");

            //Config: Getting language, if not set, quit!
            var configInput = Path.Combine(inputRoot, "config");
            string configFile = Path.Combine(configInput, "AutoTranslatorConfig.ini");
            string language = SearchINI(configFile, "Language");
            if (string.IsNullOrEmpty(language))
            {
                Console.WriteLine("Language not found in " + configFile);
                Console.ReadKey();
                Environment.Exit(0);
            }
            Console.WriteLine("Language: " + language + "\r\n");

            //Config: Copy translation file
            string configOutput = Path.Combine(outputRoot, "BepInEx", "config");
            Directory.CreateDirectory(configOutput);
            File.Copy(configFile, Path.Combine(configOutput, "AutoTranslatorConfig.ini"));


            //REDIRECTED RESOURCES
            string resourcesInput = Path.Combine(inputRoot, "Translation", language, "RedirectedResources");
            if (Directory.Exists(resourcesInput))
            {
                //RedirectedResources: Clear empty lines and write in the new work folder
                Console.WriteLine("Cleaning Commented lines in RedirectedResources \r\n");
                string workFolder = Path.Combine(outputRoot, "workFolder");
                ClearFolders(resourcesInput, workFolder);

                //RedirectedResources: make a zip and clear the work folder
                Console.WriteLine("Making the .zip for RedirectedResources \r\n");
                string resourcesOutput = Path.Combine(outputRoot, "BepInEx", "Translation", language, "RedirectedResources");
                Directory.CreateDirectory(resourcesOutput);
                ZipFile.CreateFromDirectory(workFolder, Path.Combine(resourcesOutput, "RedirectedResources.zip"));
                Directory.Delete(workFolder, true);
            }


            //TEXT
            string textInput = Path.Combine(inputRoot, "Translation", language, "Text");
            if (Directory.Exists(textInput))
            {
                //Creating a .zip with text folder
                Console.WriteLine("Copying the Text folder \r\n");
                string textOutput = Path.Combine(outputRoot, "BepInEx", "Translation", language, "Text");
                Directory.CreateDirectory(textOutput);
                CopyAll(textInput, textOutput);
            }


            //MACHINE TRANSLATIONS
            string machineInput = Path.Combine(inputRoot, "Translation", language, "Text", "zz_MachineTranslations");
            if (Directory.Exists(machineInput))
            {
                //Creating a .zip with text folder
                Console.WriteLine("Making the Machine Translations .zip \r\n");
                string machineOutput = Path.Combine(outputRoot, "BepInEx", "Translation", language, "Text", "zz_MachineTranslations");
                Directory.Delete(machineOutput,true);
                Directory.CreateDirectory(machineOutput);
                ZipFile.CreateFromDirectory(machineInput, Path.Combine(machineOutput, "zz_MachineTranslations.zip"));
            }

            //TEXTURE
            string textureInput = Path.Combine(inputRoot, "Translation", language, "Texture");
            if (Directory.Exists(textureInput))
            {
                //Creating a .zip with Texture folder
                Console.WriteLine("Making the Texture folder .zip \r\n");
                string textureOutput = Path.Combine(outputRoot, "BepInEx", "Translation", language, "Texture");
                Directory.CreateDirectory(textureOutput);
                CopyAll(textureInput, textureOutput);
            }


            //README.md
            string readmeInput = Path.Combine(inputRoot, "README.md");
            if (Directory.Exists(Path.GetDirectoryName(readmeInput)))
            {
                string readmeOutput = Path.Combine(outputRoot, "README.md");
                File.Copy(readmeInput, readmeOutput);
            }

            //LICENSE
            string licenceInput = Path.Combine(inputRoot, "LICENSE");
            if (Directory.Exists(Path.GetDirectoryName(licenceInput)))
            {
                string licenceOutput = Path.Combine(outputRoot, "LICENSE");
                File.Copy(licenceInput, licenceOutput);
            }


            //Making the final zip and cleaning the output folder
            string translationName = Path.GetFileName(inputRoot);
            translationName.Replace("-master", "");
            string releaseName = thisFolder + translationName + "_Release_" + DateTime.Now.ToString("yyyy-MM-dd") + ".zip";
            Console.WriteLine("Making " + Path.GetFileName(releaseName) + "\r\n");
            ZipFile.CreateFromDirectory(outputRoot, releaseName);
            Directory.Delete(Path.Combine(thisFolder, "workFolder"), true);

            //Finishing
            stopWatch.Stop();
            Console.WriteLine("Total Time: " + stopWatch.Elapsed + "\r\n\r\nPress any key to exit");
            Console.ReadKey();
        }


        static string SearchINI(string iniFile, string wantedName)
        {
            // I'm not making a full .ini file handler just for a couple of keys
            //Return Empty if value is not found
            string value = "";
            wantedName += "=";

            if (File.Exists(iniFile))
            {
                string[] file = File.ReadAllLines(iniFile);
                foreach (string name in file)
                {
                    if (name.StartsWith(wantedName))
                    {
                        value = name.Replace(wantedName, "");
                        break;
                    }
                }
            }
            return value;
        }


        static void ClearFolders(string inputDir, string outputDir)
        {
            //Getting all files
            string[] allFiles = Directory.GetFiles(inputDir, "*.txt", SearchOption.AllDirectories);

            //Read each file from input directory and write in output directory only if its not empty
            foreach (string currentFile in allFiles)
            {
                string[] allLines = File.ReadAllLines(currentFile);
                List<string> outputFile = new List<string>();

                //seek all lines of the current file
                bool notEmpty = false;
                foreach (string line in allLines)
                {
                    if ((!string.IsNullOrEmpty(line)) && (!line.StartsWith("//")))
                    {
                        outputFile.Add(line);
                        notEmpty = true;
                    }
                }

                //writing file to a new dir
                if (notEmpty)
                {
                    string outputPath = currentFile.Replace(inputDir, outputDir);
                    string[] outputFileText = outputFile.ToArray();
                    Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
                    File.WriteAllLines(outputPath, outputFileText);
                }
            }

        }

        static void CopyAll(string inputDir, string outputDir)
        {
            //Copy all Folders
            string[] allFolders = Directory.GetDirectories(inputDir, "*", SearchOption.AllDirectories);
            foreach (string folder in allFolders)
            {
                    Directory.CreateDirectory(folder.Replace(inputDir, outputDir));
            }
            //Copy all Files
            string[] allFiles = Directory.GetFiles(inputDir, "*.*", SearchOption.AllDirectories);
            foreach (string file in allFiles)
            {
                    File.Copy(file, file.Replace(inputDir, outputDir), true);
            }

        }

    }
}
