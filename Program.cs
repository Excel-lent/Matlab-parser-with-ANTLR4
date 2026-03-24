using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using MatlabANTLR.Visitor;

namespace MatlabParserApp;

class Program
{
    static void Main(string[] args)
    {
        string code = File.ReadAllText("Examples/Hello.m").Replace("\r\n", "\n");

        // --- Лексический и синтаксический анализ ---
        var tree = Parse(code);

        Console.WriteLine("=== Abstract Syntax Tree ===");
        var parser = CreateParser(code);
        Console.WriteLine(tree.ToStringTree(parser));

        Console.WriteLine("\n=== Visitor: Tree Traversal ===");
        var printer = new MatlabAstPrinter();
        printer.Visit(tree);

        Console.WriteLine("\n=== Listener: Variable Collection ===");
        var walker = new ParseTreeWalker();
        var scopeListener = new MatlabScopeListener();
        walker.Walk(scopeListener, tree);
        scopeListener.PrintScope();

        Console.WriteLine("\n=== Semantic Analysis ===");
        var analyzer = new MatlabSemanticAnalyzer();
        analyzer.Visit(tree);
        analyzer.PrintErrors();
    }

    static IParseTree Parse(string source)
    {
        var parser = CreateParser(source);
        return parser.program();
    }

    static MatlabParser CreateParser(string source)
    {
        var inputStream = new AntlrInputStream(source);
        var lexer = new MatlabLexer(inputStream);
        var tokenStream = new CommonTokenStream(lexer);
        var parser = new MatlabParser(tokenStream);

        // Обработка ошибок
        parser.RemoveErrorListeners();
        parser.AddErrorListener(new ConsoleErrorListener());

        return parser;
    }
}

// Простой вывод ошибок
class ConsoleErrorListener : BaseErrorListener
{
    public override void SyntaxError(
        System.IO.TextWriter output,
        IRecognizer recognizer,
        IToken offendingSymbol,
        int line, int charPositionInLine,
        string msg,
        RecognitionException e)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine($"[Error] Line {line}: {charPositionInLine} — {msg}");
        Console.ResetColor();
    }
}