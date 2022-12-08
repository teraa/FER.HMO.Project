namespace Tests;

public class InstanceLoaderTests
{
    [Theory]
    [InlineData("i1.txt")]
    [InlineData("i2.txt")]
    [InlineData("i3.txt")]
    [InlineData("i4.txt")]
    [InlineData("i5.txt")]
    public void LoadInstance(string fileName)
    {
        InstanceLoader.LoadFromFile(Path.Join("../../../../instances", fileName));
    }
}
