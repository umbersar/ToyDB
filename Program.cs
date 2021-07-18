using System;

namespace ToyDB {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine("Hello {0}! This is the ToyDB database!\n", Environment.UserName);
            Console.WriteLine("Feel free to type in commands\n");

            REPL.Start(Console.In, Console.Out);
        }
    }
}
