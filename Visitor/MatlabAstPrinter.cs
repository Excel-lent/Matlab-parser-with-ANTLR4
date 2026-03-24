using Antlr4.Runtime.Misc;

namespace MatlabANTLR.Visitor;

/// <summary>
/// Visitor для красивого вывода AST-дерева с отступами.
/// </summary>
public class MatlabAstPrinter : MatlabParserBaseVisitor<object?>
{
    private int _depth = 0;

    private string Indent => new string(' ', _depth * 2);

    private void Enter(string label)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"{Indent}┌─ {label}");
        Console.ResetColor();
        _depth++;
    }

    private void Leave()
    {
        _depth--;
    }

    private void Leaf(string label, string value)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"{Indent}  {label}: {value}");
        Console.ResetColor();
    }

    // --- Программа ---
    public override object? VisitProgram(
        [NotNull] MatlabParser.ProgramContext ctx)
    {
        Enter("Program");
        var result = VisitChildren(ctx);
        Leave();
        return result;
    }

    // --- Объявление функции ---
    public override object? VisitFunctionDecl(
        [NotNull] MatlabParser.FunctionDeclContext ctx)
    {
        Enter("FunctionDecl");
        Leaf("name", ctx.ID().GetText());

        if (ctx.returnVars() != null)
            Leaf("returns", ctx.returnVars().GetText());

        if (ctx.paramList() != null)
            Leaf("params", ctx.paramList().GetText());

        var result = VisitChildren(ctx);
        Leave();
        return result;
    }

    // --- Присваивание ---
    public override object? VisitAssignStatement(
        [NotNull] MatlabParser.AssignStatementContext ctx)
    {
        Enter("Assign");
        Leaf("lvalue", ctx.lvalue().GetText());
        var result = VisitChildren(ctx);
        Leave();
        return result;
    }

    // --- If ---
    public override object? VisitIfStatement(
        [NotNull] MatlabParser.IfStatementContext ctx)
    {
        Enter("If");
        var result = VisitChildren(ctx);
        Leave();
        return result;
    }

    // --- For ---
    public override object? VisitForStatement(
        [NotNull] MatlabParser.ForStatementContext ctx)
    {
        Enter("For");
        Leaf("var", ctx.ID().GetText());
        var result = VisitChildren(ctx);
        Leave();
        return result;
    }

    // --- While ---
    public override object? VisitWhileStatement(
        [NotNull] MatlabParser.WhileStatementContext ctx)
    {
        Enter("While");
        var result = VisitChildren(ctx);
        Leave();
        return result;
    }

    // --- Бинарные выражения ---
    public override object? VisitAddExpr(
        [NotNull] MatlabParser.AddExprContext ctx)
    {
        Enter($"BinaryOp [{ctx.GetChild(1).GetText()}]");
        var result = VisitChildren(ctx);
        Leave();
        return result;
    }

    public override object? VisitMulExpr(
        [NotNull] MatlabParser.MulExprContext ctx)
    {
        Enter($"BinaryOp [{ctx.GetChild(1).GetText()}]");
        var result = VisitChildren(ctx);
        Leave();
        return result;
    }

    public override object? VisitRelExpr(
        [NotNull] MatlabParser.RelExprContext ctx)
    {
        Enter($"Compare [{ctx.GetChild(1).GetText()}]");
        var result = VisitChildren(ctx);
        Leave();
        return result;
    }

    // --- Первичные выражения ---
    public override object? VisitPrimaryExpr(
        [NotNull] MatlabParser.PrimaryExprContext ctx)
    {
        var primary = ctx.primary();

        if (primary.INT() != null)
            Leaf("Int", primary.INT().GetText());
        else if (primary.FLOAT() != null)
            Leaf("Float", primary.FLOAT().GetText());
        else if (primary.STRING() != null)
            Leaf("String", primary.STRING().GetText());
        else if (primary.ID() != null && primary.LPAREN() == null)
            Leaf("Var", primary.ID().GetText());
        else if (primary.ID() != null)
        {
            Enter($"Call [{primary.ID().GetText()}]");
            var result = VisitChildren(ctx);
            Leave();
            return result;
        }

        return null;
    }
}