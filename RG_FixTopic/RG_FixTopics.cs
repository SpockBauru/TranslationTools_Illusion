using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RG_FixTopic
{
    internal class RG_FixTopics
    {
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
            string outputFolder = Path.Combine(thisFolder, "CleanedFiles");
            Directory.CreateDirectory(outputFolder);
            DirectoryInfo outputDir = new DirectoryInfo(outputFolder);

            

            // ========= Getting .TextAsset =================
            // These files were exported by SB3Utility after open the file, so it converts to table separated by space)
            FileInfo[] filesInSourceFolder = sourceDir.GetFiles("*.TextAsset", SearchOption.AllDirectories);
            HashSet<string> outputContent = new HashSet<string>();
            foreach (FileInfo file in filesInSourceFolder)
            {
                string[] content = File.ReadAllLines(file.FullName);
                
                outputContent.Add("\r\n"+file.Name + "\r\n");

                // skipping the first line
                for (int i  = 1; i < content.Length; i++)
                {
                    // find in 2nd column
                    string line = Regex.Replace(content[i], "^.*?\t(.*?)\t.*$", "$1");

                    // find in 4th column
                    //string line = Regex.Replace(content[i], "^.*?\t.*?\t.*?\t(.*?)\t.*$", "$1");

                    // find in 5th column
                    //string line = Regex.Replace(content[i], "^.*?\t.*?\t.*?\t.*?\t(.*?)\t.*$", "$1");

                    line = "//" + line + "=";
                    if (!outputContent.Contains(line))
                    {
                        outputContent.Add(line);
                    }
                }
            }
            string path = Path.Combine(outputDir.FullName, "translation.txt");


            File.WriteAllLines(path, outputContent);

            Console.WriteLine("Finished");
            string finish = Console.ReadLine();
        }
    }
}
