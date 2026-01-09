using darlet.Core.LexicalAnalysis;
using darlet.Core.SemanticAnalysis;
using darlet.Core.SyntaxAnalysis.AST;
using System.Text;

namespace darlet.Core.Generating
{
    /// <summary>
    /// Генератор проміжного коду.
    /// Перетворює дерево AST у лінійний рядок інструкцій у форматі ПОЛІЗ (Reverse Polish Notation).
    /// </summary>
    public class RpnGenerator : IVisitor
    {
        // Буфер для накопичення згенерованого рядка команд
        private readonly StringBuilder _output = new StringBuilder();

        // Лічильник для створення унікальних імен міток (m0, m1, ...)
        private int _labelCounter = 0;

        /// <summary>
        /// Повертає готовий рядок ПОЛІЗ, видаляючи зайві пробіли.
        /// </summary>
        public string GetOutput() => _output.ToString().Trim();

        /// <summary>
        /// Відвідує блок коду (наприклад, тіло функції або циклу) і обробляє всі вкладені інструкції.
        /// </summary>
        public void Visit(BlockNode node)
        {
            foreach (var child in node.GetChildren())
            {
                child.Accept(this);
            }
        }

        /// <summary>
        /// Додає числову константу до вихідного рядка.
        /// </summary>
        public void Visit(NumberNode node)
        {
            _output.Append($"{node.Token.Lexeme} ");
        }

        /// <summary>
        /// Додає ім'я змінної до вихідного рядка.
        /// </summary>
        public void Visit(VariableNode node)
        {
            _output.Append($"{node.Token.Lexeme} ");
        }

        /// <summary>
        /// Обробляє бінарні операції, присвоєння та спеціальні команди (наприклад, print).
        /// </summary>
        public void Visit(BinOpNode node)
        {
            // Спеціальний випадок: команда PRINT
            if (node.Token.Type == TokenType.KW_PRINT)
            {
                node.Right.Accept(this); // Обчислюємо вираз для друку
                _output.Append("PRINT ");
                return;
            }

            // Спеціальний випадок: операція присвоєння (=)
            if (node.Token.Type == TokenType.OP_ASSIGN)
            {
                node.Left.Accept(this);  // Куди записуємо (змінна)
                node.Right.Accept(this); // Що записуємо (значення)
                _output.Append("= ");
            }
            // Звичайна математика (+, -, *, /)
            else
            {
                node.Left.Accept(this);  // Лівий операнд
                node.Right.Accept(this); // Правий операнд
                _output.Append($"{node.Token.Lexeme} "); // Оператор
            }
        }

        /// <summary>
        /// Генерує код для умовної конструкції IF/ELSE з використанням міток і переходів.
        /// </summary>
        public void Visit(IfNode node)
        {
            var labelElse = NewLabel(); // Мітка для блоку Else
            var labelEnd = NewLabel();  // Мітка виходу з усього If

            // 1. Генерація коду умови
            node.Condition.Accept(this);

            // 2. Умовний перехід: якщо False (!F), стрибаємо на Else
            _output.Append($"{labelElse} !F ");

            // 3. Тіло блоку Then (виконується, якщо True)
            node.ThenBody.Accept(this);

            // 4. Безумовний перехід (!) в кінець, щоб пропустити Else
            _output.Append($"{labelEnd} ! ");

            // 5. Початок блоку Else (мітка)
            // Примітка: тут у вашому коді був знак "!", можливо це друкарська помилка,
            // зазвичай ставиться просто двокрапка. Я залишив як у оригіналі.
            _output.Append($"{labelElse}: !");

            if (node.ElseBody != null)
            {
                node.ElseBody.Accept(this);
            }

            // 6. Мітка кінця конструкції
            _output.Append($"{labelEnd}: ");
        }

        /// <summary>
        /// Генерує код для циклу WHILE.
        /// </summary>
        public void Visit(WhileNode node)
        {
            var labelStart = NewLabel(); // Початок ітерації
            var labelEnd = NewLabel();   // Вихід з циклу

            // 1. Мітка початку (сюди повертаємось)
            _output.Append($"{labelStart}: ");

            // 2. Перевірка умови
            node.Condition.Accept(this);

            // 3. Якщо умова False, виходимо з циклу
            _output.Append($"{labelEnd} !F ");

            // 4. Тіло циклу
            node.Body.Accept(this);

            // 5. Безумовний стрибок на початок для нової перевірки
            _output.Append($"{labelStart} ! ");

            // 6. Мітка виходу
            _output.Append($"{labelEnd}: ");
        }

        public void Visit(StringNode node)
        {
            // RPN (Зворотній Польський Запис) просто додає рядок у вихідний буфер
            _output.Append($"\"{node.Token.Lexeme}\"");
        }



        // Допоміжний метод для генерації унікальних імен міток
        private string NewLabel() => $"m{_labelCounter++}";
    }
}