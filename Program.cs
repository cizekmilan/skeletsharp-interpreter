using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SkeletSharp
{
    /// <summary>
    /// Konzolový vstupní bod aplikace.
    /// </summary>
    class Program
    {
        private static bool displayComments = false;
        private static bool displaySource = false;
        private static bool displayVariables = false;

        /// <summary>
        /// Zobrazí zdrojový kód po zpracování preprocesorem.
        /// </summary>
        private static void DisplayPreprocessedSource(string input)
        {
            Preprocessor preprocessor = new Preprocessor(input);
            string preprocessedSource = preprocessor.Preprocessed;

            Console.ResetColor();
            string[] lines = preprocessedSource.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            for (int i = 0; i < lines.Length; i++)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("{0,4}  ", (i + 1));
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(lines[i]);
            }

            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            WaitForKeyIfInteractive();
            Console.WriteLine();
        }

        /// <summary>
        /// Počká na klávesu pouze při interaktivním spuštění z konzole.
        /// </summary>
        private static void WaitForKeyIfInteractive()
        {
            if (!Console.IsInputRedirected)
                Console.ReadKey(true);
        }

        /// <summary>
        /// Zobrazí hodnoty všech proměnných známých interpretu.
        /// </summary>
        private static void DisplayVariables(Interpreter interpreter)
        {
            if (interpreter != null)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(interpreter.ListAllVariables());
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Vypíše nápovědu k parametrům programu.
        /// </summary>
        private static void DisplayUsage()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Usage: {0} filename.skl [-c] [-d] [-l] ", AppDomain.CurrentDomain.FriendlyName);
            Console.WriteLine();
            Console.WriteLine("Optional arguments:");
            Console.WriteLine("  -c display comment lines during execution (including loops, may be slow)");
            Console.WriteLine("  -l display source code after preprocessing");
            Console.WriteLine("  -d display all used variables after execution");
            Console.WriteLine("     (shown automatically after an error)");
            Console.ResetColor();
        }

        /// <summary>
        /// Ověří argumenty příkazové řádky a nastaví přepínače aplikace.
        /// </summary>
        private static bool TryParseArguments(string[] args)
        {
            bool result = true;
            if (args.Length < 1 || args.Length > 4)
            {
                DisplayUsage();
                return false;
            }
            if (!File.Exists(args[0]))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Source file " + args[0] + " was not found.");
                Console.WriteLine();
                Console.ResetColor();
                result = false;
            }
            if (args.Length != 1)
            {
                string[] allowedParams = new string[] { "-c", "-l", "-d" };
                for (int i = 1; i < args.Length; i++)
                {
                    if (!allowedParams.Contains(args[i]))
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Unknown argument " + args[i]);
                        Console.ResetColor();
                        Console.WriteLine();

                        result = false;
                    }
                    else
                    {
                        if (args[i] == allowedParams[0])
                            displayComments = true;
                        if (args[i] == allowedParams[1])
                            displaySource = true;
                        if (args[i] == allowedParams[2])
                            displayVariables = true;
                    }

                }
                if (!result)
                    DisplayUsage();
            }

            return result;
        }

        /// <summary>
        /// Spustí konzolovou aplikaci.
        /// </summary>
        /// <param name="args">Argumenty příkazové řádky.</param>
        static void Main(string[] args)
        {
            if (TryParseArguments(args))
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                string fileName = args[0];
                Interpreter interpreter = null;
                try
                {
                    string source = File.ReadAllText(fileName);
                    if (displaySource) DisplayPreprocessedSource(source);

                    interpreter = new Interpreter(source, displayComments);
                    interpreter.Execute();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("OK");
                    Console.ResetColor();
                    if (displayVariables) DisplayVariables(interpreter);
                }
                catch (Exception exception)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("BAD");
                    Console.WriteLine(exception.Message);
                    Console.WriteLine("Use -c -l for more details about the error.");
                    Console.ResetColor();
                    DisplayVariables(interpreter);
                }
                stopwatch.Stop();
                TimeSpan elapsedTime = stopwatch.Elapsed;
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("Completed in " + elapsedTime.ToString("c"));
                Console.ResetColor();
            }

            WaitForKeyIfInteractive();
        }
    }
}
