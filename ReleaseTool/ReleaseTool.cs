using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using System.Linq;

namespace ReleaseTool
{
    class ReleaseTool
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
                ClearFolders(resourcesInput, workFolder, "translation.txt");

                //RedirectedResources: make a zip and clear the work folder
                Console.WriteLine("Making the .zip for RedirectedResources \r\n");
                string resourcesOutput = Path.Combine(outputRoot, "BepInEx", "Translation", language, "RedirectedResources");
                Directory.CreateDirectory(resourcesOutput);
                ZipFile.CreateFromDirectory(workFolder, Path.Combine(resourcesOutput, "assets.zip"));
                Directory.Delete(workFolder, true);


                //MachineTranslation: Clear empty lines and write in the new work folder
                ClearFolders(resourcesInput, workFolder, "zz_machineTranslation.txt");

                //MachineTranslation: make a zip and clear the work folder
                if (Directory.Exists(workFolder))
                {
                    Console.WriteLine("Making the .zip for MachineTranslation \r\n");
                    Directory.CreateDirectory(resourcesOutput);
                    ZipFile.CreateFromDirectory(workFolder, Path.Combine(resourcesOutput, "zz_MachineTranslations.zip"));
                    Directory.Delete(workFolder, true);
                }
            }

            //TEXT
            string textInput = Path.Combine(inputRoot, "Translation", language, "Text");
            if (Directory.Exists(textInput))
            {
                //Text: Clear empty lines and write in the new work folder
                Console.WriteLine("Cleaning Commented lines in Text \r\n");
                string workFolder = Path.Combine(outputRoot, "workFolder");
                ClearFolders(textInput, workFolder, "*.txt");

                //Text: make a zip and clear the work folder
                Console.WriteLine("Making the Text folder .zip \r\n");
                string textOutput = Path.Combine(outputRoot, "BepInEx", "Translation", language, "Text");
                Directory.CreateDirectory(textOutput);
                ZipFile.CreateFromDirectory(workFolder, Path.Combine(textOutput, "Text.zip"));
                Directory.Delete(workFolder, true);
            }


            //TEXTURE
            string textureInput = Path.Combine(inputRoot, "Translation", language, "Texture");
            if (Directory.Exists(textureInput))
            {
                //Creating a .zip with Texture folders
                Console.WriteLine("Making the Texture folder .zip \r\n");
                string textureOutput = Path.Combine(outputRoot, "BepInEx", "Translation", language, "Texture");
                string workFolder = Path.Combine(outputRoot, "workFolder");
                Directory.CreateDirectory(textureOutput);
                Directory.CreateDirectory(workFolder);

                //Creating a .zip with textures from Texture root folder
                foreach (var rootTexture in Directory.GetFiles(textureInput, "*.png"))
                {
                    File.Copy(rootTexture, Path.Combine(workFolder, Path.GetFileName(rootTexture)));
                }
                if (Directory.GetFiles(workFolder, "*.png").Any())
                    ZipFile.CreateFromDirectory(workFolder, Path.Combine(textureOutput, "Texture.zip"));
                Directory.Delete(workFolder, true);

                //Creating one .zip for each folder in Texture
                foreach (var subDirTexure in Directory.GetDirectories(textureInput))
                {
                    string outputFile = textureOutput + "\\" + Path.GetFileName(subDirTexure) + ".zip";
                    ZipFile.CreateFromDirectory(subDirTexure, outputFile);
                }
            }


            //Copy README.md
            string readmeInput = Path.Combine(inputRoot, "README.md");
            if (Directory.Exists(Path.GetDirectoryName(readmeInput)))
            {
                string readmeOutput = Path.Combine(outputRoot, "README.md");
                File.Copy(readmeInput, readmeOutput);
            }

            //Copy LICENSE
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


        static void ClearFolders(string inputDir, string outputDir, string fileName)
        {
            //Getting all files
            string[] allFiles = Directory.GetFiles(inputDir, fileName, SearchOption.AllDirectories);

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

    }
}
