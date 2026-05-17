namespace SkeletSharp.Tests;

public class RuntimeValueTests
{
    [Fact]
    public void Compare_ReturnsOneWhenValuesAreEqual()
    {
        RuntimeValue left = new RuntimeValue(5);
        RuntimeValue right = new RuntimeValue(5);

        RuntimeValue result = left.Compare(right, Token.Equal);

        Assert.Equal((uint)1, result.IntegerValue);
    }

    [Fact]
    public void Compare_ReturnsZeroWhenValuesAreNotEqual()
    {
        RuntimeValue left = new RuntimeValue(5);
        RuntimeValue right = new RuntimeValue(6);

        RuntimeValue result = left.Compare(right, Token.Equal);

        Assert.Equal((uint)0, result.IntegerValue);
    }

    [Fact]
    public void CreateRandom_ReturnsValueInDocumentedRange()
    {
        RuntimeValue value = RuntimeValue.CreateRandom();

        Assert.InRange(value.IntegerValue, (uint)1, (uint)999);
    }

    [Fact]
    public void ToString_UsesIntegerValue()
    {
        RuntimeValue value = new RuntimeValue(123);

        Assert.Equal("123", value.ToString());
    }
}
