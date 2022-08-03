using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerConsole
{
    internal class ConsoleWriter
    {
        /// <summary>
        /// A function to print a message to the console.
        /// </summary>
        /// <param name="message">Messages to display</param>
        public static void WriteMessage(string message)
        {
            Console.WriteLine($"[{DateTime.Now.TimeOfDay}] {message}");
        }

        /// <summary>
        /// The function to display the error in the console.
        /// </summary>
        /// <param name="module">The name of the module where the error occurred</param>
        /// <param name="message">Error Message</param>
        public static void WriteError(string module, string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[{DateTime.Now.TimeOfDay}] Error {module}: {message}");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
