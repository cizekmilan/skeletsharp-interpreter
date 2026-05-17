using System.Globalization;

namespace SkeletSharp.Tests;

public class InterpreterTests
{
    [Fact]
    public void Execute_RunsAdditionProgram()
    {
        const string source = """
            input x
            input y

            result = x
            auxy = y

            while auxy <> 0 do
              incr result
              decr auxy
            end

            output result
            """;

        string output = ExecuteWithConsole(source, "2", "3");

        Assert.Contains("result: 5", output);
    }

    [Fact]
    public void Execute_RunsClearPreprocessorCommand()
    {
        const string source = "clear x\noutput x";

        string output = ExecuteWithConsole(source);

        Assert.Contains("x: 0", output);
    }

    [Fact]
    public void Execute_AssignmentDoesNotOverwriteUserAuxVariable()
    {
        const string source = "input aux\ninput y\nx = y\noutput aux\noutput x";

        string output = ExecuteWithConsole(source, "7", "3");

        Assert.Contains("aux: 7", output);
        Assert.Contains("x: 3", output);
    }

    [Fact]
    public void Execute_SelfAssignmentPreservesVariableValue()
    {
        const string source = "input x\nx = x\noutput x";

        string output = ExecuteWithConsole(source, "7");

        Assert.Contains("x: 7", output);
    }

    [Fact]
    public void Execute_ListsVariablesSortedByName()
    {
        const string source = "incr b\nincr a";

        Interpreter interpreter = new Interpreter(source, displayComments: false);

        interpreter.Execute();

        string[] lines = interpreter.ListAllVariables()
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal("a", lines[0].Split(':')[0]);
        Assert.Equal("b", lines[1].Split(':')[0]);
    }

    [Fact]
    public void Execute_ThrowsReadableErrorForEndWithoutWhile()
    {
        Interpreter interpreter = new Interpreter("end", displayComments: false);

        SkeletonSyntaxException exception = Assert.Throws<SkeletonSyntaxException>(() => interpreter.Execute());

        Assert.Contains("Unexpected end without matching while", exception.Message);
        Assert.Contains("column", exception.Message);
        Assert.Equal(1, exception.Line);
        Assert.Equal(1, exception.Column);
    }

    [Fact]
    public void Execute_ThrowsReadableErrorForWhileWithoutEnd()
    {
        const string source = "while x <> 0 do\n  decr x";

        Interpreter interpreter = new Interpreter(source, displayComments: false);

        SkeletonSyntaxException exception = Assert.Throws<SkeletonSyntaxException>(() => interpreter.Execute());

        Assert.Contains("While loop does not have matching end", exception.Message);
    }

    [Fact]
    public void GetVariable_ThrowsRuntimeExceptionForUnknownVariable()
    {
        Interpreter interpreter = new Interpreter(string.Empty, displayComments: false);

        SkeletonRuntimeException exception = Assert.Throws<SkeletonRuntimeException>(() => interpreter.GetVariable("missing"));

        Assert.Contains("missing", exception.Message);
    }

    private static string ExecuteWithConsole(string source, params string[] inputLines)
    {
        TextReader originalIn = Console.In;
        TextWriter originalOut = Console.Out;

        using StringReader input = new StringReader(string.Join(Environment.NewLine, inputLines));
        using StringWriter output = new StringWriter(CultureInfo.InvariantCulture);

        try
        {
            Console.SetIn(input);
            Console.SetOut(output);

            Interpreter interpreter = new Interpreter(source, displayComments: false);
            interpreter.Execute();

            return output.ToString();
        }
        finally
        {
            Console.SetIn(originalIn);
            Console.SetOut(originalOut);
        }
    }
}
