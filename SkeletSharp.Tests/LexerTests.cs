namespace SkeletSharp.Tests;

public class LexerTests
{
    [Fact]
    public void GetToken_ReadsKnownKeywords()
    {
        Lexer lexer = new Lexer("input output incr decr while do end");

        Assert.Equal(Token.Input, lexer.GetToken());
        Assert.Equal(Token.Output, lexer.GetToken());
        Assert.Equal(Token.Increment, lexer.GetToken());
        Assert.Equal(Token.Decrement, lexer.GetToken());
        Assert.Equal(Token.While, lexer.GetToken());
        Assert.Equal(Token.Do, lexer.GetToken());
        Assert.Equal(Token.End, lexer.GetToken());
    }

    [Fact]
    public void GetToken_ReadsIdentifier()
    {
        Lexer lexer = new Lexer("result123");

        Token token = lexer.GetToken();

        Assert.Equal(Token.Identifier, token);
        Assert.Equal("result123", lexer.Identifier);
    }

    [Fact]
    public void GetToken_SeparatesZeroFromOtherNumbers()
    {
        Lexer lexer = new Lexer("0 42");

        Assert.Equal(Token.Zero, lexer.GetToken());
        Assert.Equal(Token.Number, lexer.GetToken());
        Assert.Equal((uint)42, lexer.LiteralValue.IntegerValue);
    }

    [Fact]
    public void GetToken_ReadsNotEqualOperator()
    {
        Lexer lexer = new Lexer("<>");

        Assert.Equal(Token.NotEqual, lexer.GetToken());
    }

    [Fact]
    public void GetToken_ReadsCommentUntilEndOfLine()
    {
        Lexer lexer = new Lexer("# hello\ninput x");

        Assert.Equal(Token.Comment, lexer.GetToken());
        Assert.Equal("# hello", lexer.CommentText);
        Assert.Equal(Token.NewLine, lexer.GetToken());
        Assert.Equal(Token.Input, lexer.GetToken());
    }

    [Fact]
    public void GetToken_ReturnsEndOfFileForEmptySource()
    {
        Lexer lexer = new Lexer(string.Empty);

        Assert.Equal(Token.EndOfFile, lexer.GetToken());
    }
}
