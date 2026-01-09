using darlet.Core.Errors;
using darlet.Core.LexicalAnalysis;
using darlet.Core.SemanticAnalysis;
using darlet.Core.SyntaxAnalysis;
using darlet.Core.SyntaxAnalysis.AST;
using darlet.Core.Generating;
using System;
using System.Text;

namespace darlet.CLI;

class Program
{
    static void Main(string[] args)
    {
        // Шлях до вашого файлу. 
        // Якщо файл лежить поруч з exe (у bin/Debug), можна просто ім'я файлу.
        // Для тесту під час розробки зручніше вказати повний шлях або відносний від кореня проєкту.
        string filePath = "C:\\Users\\okras\\darlet_programming_language\\darlet.CLI\\demo.darlet";

        if (!File.Exists(filePath))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: File '{filePath}' not found.");
            Console.ResetColor();
            return;
        }

        string sourceCode = File.ReadAllText(filePath);

        try
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("1. Lexical Analysis...");
            var lexer = new Lexer(sourceCode);
            var tokens = lexer.Tokenize();

            // Можна розкоментувати, щоб показати токени викладачеві
            // foreach (var token in tokens) Console.WriteLine(token);

            Console.WriteLine("2. Parsing...");
            var parser = new Parser(tokens);
            var ast = parser.ParseProgram();

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

