﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleScripts
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting");

            NotebookMerge.Replace();

            Console.WriteLine("Ended");
            Console.ReadLine();
            Console.WriteLine("Never See");
        }
    }
}
