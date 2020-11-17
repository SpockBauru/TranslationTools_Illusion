using System;
using System.Linq;
using System.IO;

namespace DeleteDuplicates
{
    class DeleteDuplicates
    {
        static void Main(string[] args)
        {
            //File name
            string file;

            //Check if the file is in the args, otherwose read from console
            if (args.Length != 0) { file = args[0]; }
            else
            {
                Console.Write("Enter the file name: ");
                file = Console.ReadLine();
            }

            //Check if the file exits
            if (!File.Exists(file))
            {
                Console.WriteLine("File Not Found.");
                Console.ReadKey();
                Environment.Exit(0);
            }

            //Read all the file
            string[] repeatedTranslation = File.ReadAllLines(file);

            //Name of the new file
            string outputFile = file.Replace(".txt", "_Cleaned.txt");

            //Write the cleaned file (thanks stackoverflow!)
            File.WriteAllLines(outputFile, repeatedTranslation.Distinct().ToArray());

            //Exit prompt if console was used
            if (args.Length == 0)
            {
                Console.WriteLine("Completed!");
                Console.ReadKey();
            }
        }
    }
}
