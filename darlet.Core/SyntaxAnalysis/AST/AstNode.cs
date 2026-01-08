using darlet.Core.LexicalAnalysis;
using darlet.Core.SemanticAnalysis;
using System;
using System.Collections.Generic;
using System.Linq; // Необхідно для методу .ToList() у PrintTree

namespace darlet.Core.SyntaxAnalysis.AST
{
    /// <summary>
    /// Базовий абстрактний клас для всіх вузлів Абстрактного Синтаксичного Дерева (AST).
    /// Кожен вузол представляє певну синтаксичну конструкцію (число, операцію, блок коду тощо).
    /// </summary>
    public abstract class AstNode
    {
        /// <summary>
        /// Лексичний токен, на основі якого створено цей вузол.
        /// Зберігає корисні метадані: номер рядка, позицію та оригінальний текст.
        /// Це критично важливо для генерації точних повідомлень про помилки.
        /// </summary>
        public Token Token { get; }

        protected AstNode(Token token)
        {
            Token = token;
        }

        /// <summary>
        /// Повертає список дочірніх вузлів.
        /// Необхідно для універсального обходу дерева (наприклад, для візуалізації).
        /// </summary>
        public abstract IEnumerable<AstNode> GetChildren();

        public override string ToString()
        {
            return GetType().Name;
        }

        /// <summary>
        /// Точка входу для патерну Visitor (Відвідувач).
        /// Дозволяє зовнішнім класам (SemanticAnalyzer, CodeGenerator) виконувати операції над вузлом,
        /// не змінюючи код самого вузла.
        /// </summary>
        public abstract void Accept(IVisitor visitor);

        /// <summary>
        /// Рекурсивно виводить структуру дерева в консоль у вигляді ASCII-графіки.
        /// Дуже корисно для налагодження парсера.
        /// </summary>
        /// <param name="indent">Поточний відступ (для рекурсії).</param>
        /// <param name="isLast">Чи є цей вузол останнім у списку дітей свого батька.</param>
        public void PrintTree(string indent = "", bool isLast = true)
        {
            // Формуємо гілку: "+-- " для останнього елемента або проміжний вигляд
            var marker = isLast ? "+-- " : "+-- ";

            Console.Write(indent);
            Console.Write(marker);

            // Виводимо назву класу вузла (наприклад, BinOpNode)
            Console.Write(this.ToString());

            // Якщо є токен, дописуємо його значення для ясності (крім очевидних вузлів)
            if (Token != null && !(this is BinOpNode) && !(this is NumberNode))
            {
                Console.Write($" [{Token.Lexeme}]");
            }
            Console.WriteLine();

            // Підготовка відступу для дітей наступного рівня
            // Якщо це останній елемент, лінія вниз не потрібна ("    "), інакше малюємо вертикальну лінію ("|   ")
            indent += isLast ? "    " : "|   ";

            var children = GetChildren().ToList(); // Конвертуємо в список для доступу за індексом

            for (int i = 0; i < children.Count; i++)
            {
                // Передаємо true тільки для останнього елемента списку
                children[i].PrintTree(indent, i == children.Count - 1);
            }
        }
    }
}