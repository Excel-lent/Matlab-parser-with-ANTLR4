using Antlr4.Runtime.Misc;

namespace MatlabParserApp;

public class MatlabSemanticAnalyzer : MatlabParserBaseVisitor<object?>
{
    // Стек областей видимости: каждый вход в функцию добавляет новый слой
    private readonly Stack<HashSet<string>> _scopes = new();

    // Список найденных ошибок
    private readonly List<string> _errors = new();

    public bool HasErrors => _errors.Count > 0;

    public void PrintErrors()
    {
        if (!HasErrors)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("No semantic errors found.");
            Console.ResetColor();
            return;
        }

        foreach (var error in _errors)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(error);
            Console.ResetColor();
        }
    }

    // --- Вспомогательные методы ---

    private void PushScope() => _scopes.Push(new HashSet<string>());
    private void PopScope() => _scopes.Pop();

    private void Declare(string name)
    {
        if (_scopes.Count > 0)
            _scopes.Peek().Add(name);
    }

    private bool IsDeclared(string name)
    {
        // Ищем во всех слоях — от текущего к глобальному
        foreach (var scope in _scopes)
            if (scope.Contains(name))
                return true;
        return false;
    }

    private void Error(int line, string message)
    {
        _errors.Add($"[Semantic Error] Line {line}: {message}");
    }

    // --- Программа ---

    public override object? VisitProgram(
        [NotNull] MatlabParser.ProgramContext ctx)
    {
        PushScope(); // глобальная область
        var result = VisitChildren(ctx);
        PopScope();
        return result;
    }

    // --- Функция: параметры и return-переменная объявляются внутри ---

    public override object? VisitFunctionDecl(
        [NotNull] MatlabParser.FunctionDeclContext ctx)
    {
        // Имя функции доступно в глобальной области
        Declare(ctx.ID().GetText());

        PushScope();

        // Объявить параметры
        if (ctx.paramList() != null)
            foreach (var param in ctx.paramList().ID())
                Declare(param.GetText());

        // Объявить return-переменную
        if (ctx.returnVars() != null)
            foreach (var rv in ctx.returnVars().ID())
                Declare(rv.GetText());

        var result = VisitChildren(ctx);
        PopScope();
        return result;
    }

    // --- Присваивание: lvalue объявляется, rvalue проверяется ---

    public override object? VisitAssignStatement(
        [NotNull] MatlabParser.AssignStatementContext ctx)
    {
        // Сначала проверяем правую часть
        Visit(ctx.expression());

        // Потом объявляем переменную слева
        Declare(ctx.lvalue().ID().GetText());

        return null;
    }

    // --- For: переменная цикла объявляется ---

    public override object? VisitForStatement(
        [NotNull] MatlabParser.ForStatementContext ctx)
    {
        Declare(ctx.ID().GetText());
        return VisitChildren(ctx);
    }

    // --- Использование переменной в выражении ---

    public override object? VisitPrimaryExpr(
        [NotNull] MatlabParser.PrimaryExprContext ctx)
    {
        var primary = ctx.primary();

        // Просто переменная: x, y, z
        if (primary.ID() != null && primary.LPAREN() == null)
        {
            string name = primary.ID().GetText();
            int line = primary.Start.Line;

            if (!IsDeclared(name))
                Error(line, $"Variable '{name}' is used but never declared.");
        }

        // Вызов функции: add(x, y) — проверяем аргументы
        if (primary.ID() != null && primary.LPAREN() != null)
        {
            // Имя функции не проверяем (может быть встроенной — disp, sin и т.д.)
            // Но аргументы проверяем
            return VisitChildren(ctx);
        }

        return VisitChildren(ctx);
    }
}