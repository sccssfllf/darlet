using System.Collections.Generic;

namespace darlet.Core.SemanticAnalysis
{
    public class SymbolTable
    {
        // Стек областей видимості (Scopes). 
        // Кожен елемент стеку - це словник змінних (Ім'я -> Тип).
        private readonly Stack<HashSet<string>> _scopes = new Stack<HashSet<string>>();

        public SymbolTable() 
        {
            // Створюємо глобальну область видимості
            EnterScope();
        }

        public void EnterScope()
        {
            _scopes.Push(new HashSet<string>());
        }

        public void ExitScope()
        { 
            _scopes.Pop(); 
        }
        
        // Оголошення змінної (var x)
        public bool Declare (string name)
        {
            var currentScope = _scopes.Peek();
            if (currentScope.Contains(name))
            {
                return false; // Помилка: вже існує
            }
            currentScope.Add(name);
            return true;
        }

        // Пошук змінної (використання x)
        public bool Lookup(string name)
        {
            // Шукаємо від поточної області до глобальної
            foreach (var scope in _scopes)
            {
                if (scope.Contains(name)) return true;
            }
            return false;
        }
    }
}
