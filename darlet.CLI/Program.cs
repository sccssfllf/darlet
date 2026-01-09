using darlet.Core.Generating;
using darlet.Core.LexicalAnalysis;
using darlet.Core.SemanticAnalysis;
using darlet.Core.SyntaxAnalysis;

namespace darlet
{
    class Program
    {
        static void Main(string[] args)
        {

            //string demo = "C:\\Users\\okras\\darlet_programming_language\\darlet.CLI\\arithmeticDemo.darlet";
            string demo = "C:\\Users\\okras\\darlet_programming_language\\darlet.CLI\\test.darlet";
            //string demo = "C:\\Users\\okras\\darlet_programming_language\\darlet.CLI\\ifDemo.darlet";
            //string demo = "C:\\Users\\okras\\darlet_programming_language\\darlet.CLI\\whileDemo.darlet";
            //string demo = "C:\\Users\\okras\\darlet_programming_language\\darlet.CLI\\demo.darlet";


            if (!File.Exists(demo))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: File '{demo}' not found.");
                Console.ResetColor();
                return;
            }

            string sourceCode = File.ReadAllText(demo);

            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("1. Lexical Analysis...");
                var lexer = new Lexer(sourceCode);
                var tokens = lexer.Tokenize();

                foreach (var token in tokens) Console.WriteLine(token);

                Console.WriteLine("2. Parsing...");
                var parser = new Parser(tokens);
                var ast = parser.ParseProgram();

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n--- [Intermediate Representation: RPN with Jumps] ---");

                try
                {
                    var rpnGen = new RpnGenerator();

                    ast.Accept(rpnGen);

                    string rpnCode = rpnGen.GetOutput();

                    Console.WriteLine(rpnCode);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"RPN Generation failed: {ex.Message}");
                }

                Console.ResetColor();
                Console.WriteLine("-----------------------------------------------------\n");

                Console.WriteLine("3. Interpreting...");
                Console.WriteLine("----------------------------------");
                Console.ResetColor();

                // Виконання
                var interpreter = new Interpreter();
                ast.Accept(interpreter);

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("----------------------------------");
                Console.WriteLine("Done.");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Compiler Error: {ex.Message}");
                Console.ResetColor();
            }

            Console.ReadKey();
        }
    }
}