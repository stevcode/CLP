using System;
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
            MassSubmit.SubmitSinglePage(4);
            Console.WriteLine("Ended");
            Console.ReadLine();
        }
    }
}
