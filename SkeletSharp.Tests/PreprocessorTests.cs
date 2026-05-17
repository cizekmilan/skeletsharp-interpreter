namespace SkeletSharp.Tests;

public class PreprocessorTests
{
    [Fact]
    public void Preprocessed_RewritesClearCommand()
    {
        Preprocessor preprocessor = new Preprocessor("clear result");

        string output = preprocessor.Preprocessed;

        Assert.Contains("while result<> 0 do", output);
        Assert.Contains("decr result", output);
        Assert.Contains("end", output);
    }

    [Fact]
    public void Preprocessed_RewritesAssignmentCommand()
    {
        Preprocessor preprocessor = new Preprocessor("left = right");

        string output = preprocessor.Preprocessed;

        Assert.Contains("while left<> 0 do", output);
        Assert.Contains("while right<> 0 do", output);
        Assert.Contains("incr left", output);
        Assert.Contains("incr right", output);
        Assert.Contains("auxAssign1", output);
        Assert.DoesNotContain("while aux<> 0 do", output);
    }

    [Fact]
    public void Preprocessed_AvoidsCollisionWithExistingHelperVariableName()
    {
        Preprocessor preprocessor = new Preprocessor("auxAssign1 = right\nleft = right");

        string output = preprocessor.Preprocessed;

        Assert.Contains("auxAssign2", output);
    }

    [Fact]
    public void Preprocessed_DoesNotRewriteCommentLine()
    {
        const string source = "# clear x";

        Preprocessor preprocessor = new Preprocessor(source);

        Assert.Contains(source, preprocessor.Preprocessed);
    }

    [Theory]
    [InlineData("clear x\r\noutput x")]
    [InlineData("clear x\noutput x")]
    [InlineData("clear x\routput x")]
    public void Preprocessed_NormalizesDifferentLineEndings(string source)
    {
        Preprocessor preprocessor = new Preprocessor(source);

        string output = preprocessor.Preprocessed;

        Assert.Contains("decr x", output);
        Assert.Contains("output x", output);
    }

    [Fact]
    public void Preprocessed_ReturnsSameValueWhenReadRepeatedly()
    {
        Preprocessor preprocessor = new Preprocessor("clear x");

        string firstRead = preprocessor.Preprocessed;
        string secondRead = preprocessor.Preprocessed;

        Assert.Equal(firstRead, secondRead);
    }
}
