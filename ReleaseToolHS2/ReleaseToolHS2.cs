using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using System.Reflection;

namespace ReleaseToolHS2
{
    class ReleaseToolHS2
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Release Tool for Hone Select 2 \r\n");

            //Read Current Folder from args, otherwise ask for user
            string mainFolder;
            if (args.Length != 0) mainFolder = args[0];
            else
            {
                Console.Write("Enter GitHub folder path: ");
                mainFolder = Console.ReadLine();
                Console.WriteLine();
            }


            //Stopwatch because I like it
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            //Making the release folder path the same folder as the .exe
            string exeFilePath = Assembly.GetExecutingAssembly().Location;
            string thisFolder = Path.GetDirectoryName(exeFilePath);
            string releaseFolder = Path.Combine(thisFolder, Path.GetFileName(mainFolder));

            //Config: Getting config dir. If it don't exist, quit!
            var configDir = Path.Combine(mainFolder, "config");
            if (!Directory.Exists(configDir) || string.IsNullOrEmpty(mainFolder))
            {
                Console.WriteLine("Not a valid folder");
                Console.ReadKey();
                Environment.Exit(0);
            }

            //Config: Getting language, if not set, quit!
            string configFile = Path.Combine(configDir, "AutoTranslatorConfig.ini");
            string language = SearchINI(configFile, "Language");
            if (string.IsNullOrEmpty(language))
            {
                Console.WriteLine("Language not found in " + configFile);
                Console.ReadKey();
                Environment.Exit(0);
            }

            //Config: Copy translation file
            string configDirOutput = Path.Combine(releaseFolder, "BepInEx", "config");
            Directory.CreateDirectory(configDirOutput);
            File.Copy(Path.Combine(configDir, "AutoTranslatorConfig.ini"), Path.Combine(configDirOutput, "AutoTranslatorConfig.ini"));


            //RedirectedResources: Clear empty lines and write in the new release folder
            Console.WriteLine("Cleaning Commented lines in RedirectedResources \r\n");
            string resourcesDir = Path.Combine(mainFolder, "Translation", language, "RedirectedResources");
            string resourcesDirOutput = Path.Combine(releaseFolder, "BepInEx", "Translation", language, "RedirectedResources");
            ClearFolders(resourcesDir, resourcesDirOutput);

            //RedirectedResources: make a zip and clear folder
            Console.WriteLine("Making the .zip for RedirectedResources \r\n");
            ZipFile.CreateFromDirectory(resourcesDirOutput, Path.Combine(releaseFolder, "RedirectedResources.zip"));
            Directory.Delete(Path.Combine(resourcesDirOutput, "assets"), true);
            File.Move(Path.Combine(releaseFolder, "RedirectedResources.zip"), Path.Combine(resourcesDirOutput, "RedirectedResources.zip"));


            //Text: Creating names for source and destination folders
            Console.WriteLine("Copying Text folder \r\n");
            string textDir = Path.Combine(mainFolder, "Translation", language, "Text");
            string textDirOutput = Path.Combine(releaseFolder, "BepInEx", "Translation", language, "Text");

            //Text: Create Folders
            CopyAllFolder(textDir, textDirOutput);


            //Texture: Creating names for source and destination folders
            Console.WriteLine("Copying Texture folder \r\n");
            string textureDir = Path.Combine(mainFolder, "Translation", language, "Texture");
            string textureDirOutput = Path.Combine(releaseFolder, "BepInEx", "Translation", language, "Texture");

            //Texture: Create Folders
            CopyAllFolder(textureDir, textureDirOutput);

            //Copy Readme.md and LICENCE
            File.Copy(Path.Combine(mainFolder, "README.md"), Path.Combine(releaseFolder, "README.md"));
            File.Copy(Path.Combine(mainFolder, "LICENSE"), Path.Combine(releaseFolder, "LICENSE"));


            //Making the final zip and cleaning the work folder
            string releaseName = releaseFolder + "_Release_" + DateTime.Now.ToString("yyyy-MM-dd") + ".zip";
            Console.WriteLine("Making " + Path.GetFileName(releaseName));
            ZipFile.CreateFromDirectory(releaseFolder, releaseName + "\r\n");
            Directory.Delete(releaseFolder, true);

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


        static void CopyAllFolder(string fromFolder, string toFolder)
        {
            //Create Folders
            Directory.CreateDirectory(toFolder);
            string[] allDir = Directory.GetDirectories(fromFolder, "*", SearchOption.AllDirectories);
            foreach (string oldDir in allDir)
            {
                string newDir = oldDir.Replace(fromFolder, toFolder);
                Directory.CreateDirectory(newDir);
            }

            //Copy files
            string[] allFiles = Directory.GetFiles(fromFolder, "*.*", SearchOption.AllDirectories);
            foreach (string fromFile in allFiles)
            {
                string toFile = fromFile.Replace(fromFolder, toFolder);
                File.Copy(fromFile, toFile, true);
            }
        }

    }
}
