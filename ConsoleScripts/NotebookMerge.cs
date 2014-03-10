using System;
using CLP.Models;

namespace ConsoleScripts
{
    public class NotebookMerge
    {
        public static void Replace()
        {
            var studentDirectoryPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\NotebookMerge";
            var fromFilePath = studentDirectoryPath + @"\from.clp";
            var toFilePath = studentDirectoryPath + @"\to.clp";

            var fromNotebook = MassSubmit.OpenNotebook(fromFilePath);
            Console.WriteLine("From Notebook Open");
            var toNotebook = MassSubmit.OpenNotebook(toFilePath);
            Console.WriteLine("To Notebook Open");
            var mergedNotebook1 = ReplacePageFromToAtIndex(fromNotebook, toNotebook, 0);
            Console.WriteLine("First Merge");
            var mergedNotebook2 = ReplacePageFromToAtIndex(fromNotebook, mergedNotebook1, 1);
            Console.WriteLine("Second Merge");
            mergedNotebook2.Save(toFilePath);
            Console.WriteLine("Notebook Saved at {0}", toFilePath);
        }

        public static CLPNotebook ReplacePageFromToAtIndex(CLPNotebook from, CLPNotebook to, int index)
        {
            var page = from.Pages[index];
            to.RemovePageAt(index);
            Console.WriteLine("Removed Page at Index {0}", index);
            to.InsertPageAt(index, page);
            Console.WriteLine("Inserted Page at Index {0}", index);
            return to;
        }

        public static void Combine()
        {
            var studentDirectoryPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\NotebookMerge";
            var baseFilePath = studentDirectoryPath + @"\base.clp";
            var newFilePath = studentDirectoryPath + @"\new.clp";

            var baseNotebook = MassSubmit.OpenNotebook(baseFilePath);
            Console.WriteLine("Base Notebook Open");
            var newNotebook = MassSubmit.OpenNotebook(newFilePath);
            Console.WriteLine("New Notebook Open");

            foreach(var clpPage in newNotebook.Pages)
            {
                baseNotebook.AddPage(clpPage);
            }

            var filePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\NotebookMerge\" + baseNotebook.NotebookName + @" - combined.clp";

            var saveTime = DateTime.Now;
            baseNotebook.LastSavedTime = saveTime;
            
            baseNotebook.Save(filePath);
            Console.WriteLine("Combined Notebook Saved");
        }
    }
}