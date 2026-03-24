using Antlr4.Runtime.Misc;

namespace MatlabParserApp;

/// <summary>
/// Listener для сбора объявлений переменных и функций по области видимости.
/// </summary>
public class MatlabScopeListener : MatlabParserBaseListener
{
    // Текущая функция (null = глобальная область)
    private string _currentScope = "<global>";

    // scope → список переменных
    private readonly Dictionary<string, HashSet<string>> _scopes = new()
    {
        ["<global>"] = new()
    };

    // Список функций
    private readonly List<(string Name, string Params, string Returns)> _functions = new();

    // --- Вход в функцию ---
    public override void EnterFunctionDecl(
        [NotNull] MatlabParser.FunctionDeclContext ctx)
    {
        string name = ctx.ID().GetText();
        string @params = ctx.paramList()?.GetText() ?? "—";
        string returns = ctx.returnVars()?.GetText() ?? "—";

        _functions.Add((name, @params, returns));
        _currentScope = name;
        _scopes[name] = new HashSet<string>();

        // Параметры — тоже переменные функции
        if (ctx.paramList() != null)
            foreach (var param in ctx.paramList().ID())
                _scopes[name].Add(param.GetText());
    }

    // --- Выход из функции ---
    public override void ExitFunctionDecl(
        [NotNull] MatlabParser.FunctionDeclContext ctx)
    {
        _currentScope = "<global>";
    }

    // --- Присваивание: фиксируем переменную ---
    public override void EnterAssignStatement(
        [NotNull] MatlabParser.AssignStatementContext ctx)
    {
        string varName = ctx.lvalue().ID().GetText();
        _scopes[_currentScope].Add(varName);
    }

    // --- For-переменная ---
    public override void EnterForStatement(
        [NotNull] MatlabParser.ForStatementContext ctx)
    {
        _scopes[_currentScope].Add(ctx.ID().GetText());
    }

    // --- Вывод результатов ---
    public void PrintScope()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Functions:");
        Console.ResetColor();

        foreach (var f in _functions)
            Console.WriteLine($"  fn {f.Name}({f.Params}) → {f.Returns}");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\nVariables by scope:");
        Console.ResetColor();

        foreach (var (scope, vars) in _scopes)
        {
            Console.Write($"  [{scope}]: ");
            Console.WriteLine(vars.Count > 0
                ? string.Join(", ", vars)
                : "—");
        }
    }
}