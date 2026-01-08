using System.Collections.Generic;

namespace darlet.Core.SemanticAnalysis
{
    /// <summary>
    /// Таблиця символів (Symbol Table).
    /// Це ключова структура даних компілятора, яка відстежує всі ідентифікатори (змінні) у програмі
    /// та враховує їх області видимості (Scopes).
    /// </summary>
    public class SymbolTable
    {
        // Стек областей видимості. 
        // Вершина стеку (_scopes.Peek()) — це поточна область видимості (найбільш вкладена).
        // Дно стеку — це глобальна область видимості.
        // Використовуємо HashSet для швидкої перевірки наявності імені (O(1)).
        private readonly Stack<HashSet<string>> _scopes = new Stack<HashSet<string>>();

        /// <summary>
        /// Ініціалізує таблицю символів та створює базову (глобальну) область видимості.
        /// </summary>
        public SymbolTable()
        {
            EnterScope(); // Створення Global Scope
        }

        /// <summary>
        /// Вхід у нову область видимості (наприклад, при вході в блок if, while або функцію).
        /// </summary>
        public void EnterScope()
        {
            _scopes.Push(new HashSet<string>());
        }

        /// <summary>
        /// Вихід з поточної області видимості. Всі змінні, оголошені в цьому рівні, забуваються.
        /// </summary>
        public void ExitScope()
        {
            _scopes.Pop();
        }

        /// <summary>
        /// Оголошення нової змінної в поточній області видимості.
        /// </summary>
        /// <param name="name">Ім'я змінної.</param>
        /// <returns>True, якщо змінну успішно додано. False, якщо вона вже існує в цьому блоці (помилка redeclaration).</returns>
        public bool Declare(string name)
        {
            var currentScope = _scopes.Peek();

            // Перевіряємо лише поточний рівень. 
            // Це дозволяє "затінення" (shadowing) змінних із зовнішніх рівнів, якщо мова це підтримує.
            if (currentScope.Contains(name))
            {
                return false; // Помилка: змінна з таким ім'ям вже оголошена в цьому блоці
            }

            currentScope.Add(name);
            return true;
        }

        /// <summary>
        /// Пошук змінної за ім'ям (Resolution).
        /// Використовується, коли ми звертаємося до змінної (читання/запис).
        /// </summary>
        /// <param name="name">Ім'я змінної.</param>
        /// <returns>True, якщо змінна знайдена в будь-якій доступній області видимості.</returns>
        public bool Lookup(string name)
        {
            // Реалізація лексичної області видимості (Lexical Scoping).
            // Шукаємо від поточної (внутрішньої) області вгору по стеку до глобальної.
            foreach (var scope in _scopes)
            {
                if (scope.Contains(name)) return true;
            }

            return false; // Змінна не оголошена
        }
    }
}