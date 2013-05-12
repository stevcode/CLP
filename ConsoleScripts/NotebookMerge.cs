using System;
using CLP.Models;

namespace ConsoleScripts
{
   public class NotebookMerge
    {
        public static void Replace()
        {
            string studentDirectoryPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\NotebookMerge";
            string fromFilePath = studentDirectoryPath + @"\from.clp";
            string toFilePath = studentDirectoryPath + @"\to.clp";

            CLPNotebook fromNotebook = MassSubmit.OpenNotebook(fromFilePath);
            Console.WriteLine("From Notebook Open");
            CLPNotebook toNotebook = MassSubmit.OpenNotebook(toFilePath);
            Console.WriteLine("To Notebook Open");
            CLPNotebook mergedNotebook1 = ReplacePageFromToAtIndex(fromNotebook, toNotebook, 0);
            Console.WriteLine("First Merge");
            CLPNotebook mergedNotebook2 = ReplacePageFromToAtIndex(fromNotebook, mergedNotebook1, 1);
            Console.WriteLine("Second Merge");
            mergedNotebook2.Save(toFilePath);
            Console.WriteLine("Notebook Saved at {0}", toFilePath);
        }

        public static CLPNotebook ReplacePageFromToAtIndex(CLPNotebook from, CLPNotebook to, int index)
        {
            CLPPage page = from.Pages[index];
            to.RemovePageAt(index);
            Console.WriteLine("Removed Page at Index {0}", index);
            to.InsertPageAt(index, page);
            Console.WriteLine("Inserted Page at Index {0}", index);
            return to;
        }
    }
}
