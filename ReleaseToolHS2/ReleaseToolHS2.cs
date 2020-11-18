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
            Console.WriteLine("Release Tool for Hone Select 2 \r\n");

            string inputRoot;

            //Read Current Folder from args, otherwise ask for user
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

            //Making the output folder path the same folder as the .exe
            string thisFolder = AppDomain.CurrentDomain.BaseDirectory;
            string outputRoot = Path.Combine(thisFolder, Path.GetFileName(inputRoot));

            //Config: Getting config dir. If it don't exist, quit!
            var configInput = Path.Combine(inputRoot, "config");
            if (!Directory.Exists(configInput) || string.IsNullOrEmpty(inputRoot))
            {
                Console.WriteLine("Not a valid folder");
                Console.ReadKey();
                Environment.Exit(0);
            }

            //Config: Getting language, if not set, quit!
            string configFile = Path.Combine(configInput, "AutoTranslatorConfig.ini");
            string language = SearchINI(configFile, "Language");
            if (string.IsNullOrEmpty(language))
            {
                Console.WriteLine("Language not found in " + configFile);
                Console.ReadKey();
                Environment.Exit(0);
            }

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

                //RedirectedResources: make a zip and clear folder
                Console.WriteLine("Making the .zip for RedirectedResources \r\n");
                string resourcesOutput = Path.Combine(outputRoot, "BepInEx", "Translation", language, "RedirectedResources");
                Directory.CreateDirectory(resourcesOutput);
                ZipFile.CreateFromDirectory(workFolder, Path.Combine(resourcesOutput, "RedirectedResources.zip"));
                Directory.Delete(Path.Combine(workFolder), true);
            }


            //TEXT
            string textInput = Path.Combine(inputRoot, "Translation", language, "Text");
            if (Directory.Exists(textInput))
            {
                //Creating a .zip with text folder
                Console.WriteLine("Making the Text folder .zip \r\n");
                string textOutput = Path.Combine(outputRoot, "BepInEx", "Translation", language, "Text");
                Directory.CreateDirectory(textOutput);
                ZipFile.CreateFromDirectory(textInput, Path.Combine(textOutput, "Text.zip"));
            }


            //TEXTURE
            string textureInput = Path.Combine(inputRoot, "Translation", language, "Texture");
            if (Directory.Exists(textureInput))
            {
                //Creating a .zip with text folder
                Console.WriteLine("Making the Texture folder .zip \r\n");
                string textureOutput = Path.Combine(outputRoot, "BepInEx", "Translation", language, "Texture");
                Directory.CreateDirectory(textureOutput);
                ZipFile.CreateFromDirectory(textureInput, Path.Combine(textureOutput, "Texture.zip"));
            }


            //Copy Readme.md
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
            string releaseName = outputRoot + "_Release_" + DateTime.Now.ToString("yyyy-MM-dd") + ".zip";
            Console.WriteLine("Making " + Path.GetFileName(releaseName) + "\r\n");
            ZipFile.CreateFromDirectory(outputRoot, releaseName);
            Directory.Delete(outputRoot, true);

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
            string[] filesInFolder = Directory.GetFiles(inputDir, "*.txt", SearchOption.AllDirectories);

            //Read each file from input directory and write output directory
            foreach (string file in filesInFolder)
            {
                string[] currentFile = File.ReadAllLines(file);
                List<string> outputFile = new List<string>();

                //seek all lines of the current file
                bool notEmpty = false;
                foreach (string line in currentFile)
                {
                    if ((!string.IsNullOrEmpty(line)) && (!line[0].Equals('/')))
                    {
                        outputFile.Add(line);
                        notEmpty = true;
                    }
                }

                //writing file to a new dir
                if (notEmpty)
                {
                    string outputPath = file.Replace(inputDir, outputDir);
                    string[] outputFileText = outputFile.ToArray();
                    Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
                    File.WriteAllLines(outputPath, outputFileText);
                }
            }

        }

    }
}
