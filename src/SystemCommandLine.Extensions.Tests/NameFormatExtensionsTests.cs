namespace SystemCommandLine.Extensions.Tests;

public class NameFormatExtensionsTests
{
    [Theory]
    [InlineData("TestOptionName", "--test-option-name")]
    [InlineData("Name", "--name")]
    public void ToKebabCase_formats_names_correctly(string input, string expectedResult)
    {
        Assert.Equal(expectedResult, NameFormatExtensions.ToKebabCase("--", input));
    }
}
