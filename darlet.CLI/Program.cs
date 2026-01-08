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
        #region Lexer
        //const string sourceCode = @"
        //        // ===================================================================
        //        // Базовий приклад програми для мови програмування Dartlet
        //        // ===================================================================

        //        var N = 0;
        //        print(""Введіть ціле число для обчислення факторіалу:"");
        //        N = input(); 

        //        if (N < 0) {
        //            print(""Помилка: Факторіал для від'ємних чисел не визначений."");
        //        } else {
        //            var factorial = 1.0; 
        //            var counter = 1;

        //            while (counter <= N) {
        //                factorial = factorial * counter;
        //                counter = counter + 1;
        //            }

        //            print(""Результат (N!):"");
        //            print(factorial);
        //        }

        //        // Демонстрація оператора степеня
        //        var power_result = 2 ^ 3 ^ 2;
        //        print(power_result);

        //        // Демонстрація логічних значень
        //        var check_equality = (power_result == 512.0); 
        //        print(check_equality);
        //    ";

        //Console.OutputEncoding = Encoding.UTF8;
        //Console.WriteLine("--- Starting Dartlet Lexical Analysis ---");
        //Console.WriteLine($"Input source code:\n{sourceCode}\n");

        //// Створюємо лексер
        //Lexer lexer = new Lexer(sourceCode);

        //// Запускаємо аналіз та отримуємо результати
        //LexicalAnalysisResult result = new LexicalAnalysisResult(lexer);

        //// Виводимо результати у форматі ЛР2
        //result.PrintResults();

        //Console.WriteLine("\n--- Analysis Finished ---");
        #endregion

        #region Syntax & Semantic Analysis
        //Console.WriteLine("Darlet Compiler CLI (Type 'exit' to quit)");
        //Console.WriteLine("-----------------------------------------");

        //var semanticAnalyzer = new SemanticAnalyzer();

        //while (true)
        //{
        //    Console.Write("> ");
        //    string input = Console.ReadLine();

        //    if (string.IsNullOrWhiteSpace(input)) continue;
        //    if (input.Trim().ToLower() == "exit") break;

        //    try
        //    {
        //        // 1. Лексичний аналіз
        //        var lexer = new Lexer(input);
        //        var tokens = lexer.Tokenize();

        //        // 2. Синтаксичний аналіз
        //        var parser = new Parser(tokens);
        //        var root = parser.ParseProgram();

        //        semanticAnalyzer.Analyze(root);

        //        // 3. Вивід дерева
        //        Console.ForegroundColor = ConsoleColor.Green;
        //        Console.WriteLine("AST:");
        //        root.PrintTree();
        //        Console.ResetColor();
        //    }
        //    catch (CompilerException ex)
        //    {
        //        // Гарний вивід помилок для звіту
        //        Console.ForegroundColor = ConsoleColor.Red;
        //        Console.WriteLine($"[PARSER ERROR] {ex.Message}");
        //        Console.WriteLine($"Line: {ex.Line}, Column: {ex.Column}");
        //        Console.ResetColor();
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.ForegroundColor = ConsoleColor.DarkRed;
        //        Console.WriteLine($"[SYSTEM ERROR] {ex.Message}");
        //        Console.ResetColor();
        //    }
        //}
        #endregion

        string sourceCode = @"
                var x = 10;
                var y = 5;
                
                print(x + y);

                var i = 0;
                while (i < 3) {
                    print(i);
                    i = i + 1;
                }
            ";

        try
        {
            // 1. Лексичний аналіз
            var lexer = new Lexer(sourceCode);
            var tokens = lexer.Tokenize();

            // 2. Парсинг
            var parser = new Parser(tokens);
            var ast = parser.ParseProgram();

            // 3. (Опціонально) Вивід дерева
            Console.WriteLine("AST Structure:");
            ast.PrintTree();
            Console.WriteLine("----------------");
            Console.WriteLine("Output:");

            // 4. Інтерпретація
            var interpreter = new Interpreter();
            ast.Accept(interpreter);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: {ex.Message}");
            Console.ResetColor();
        }

        Console.ReadKey();
    }
}

