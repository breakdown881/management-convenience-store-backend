using ConvenienceStore.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace ConvenienceStore.UnitTests.Domain;

public class BarcodeTests
{
    [Fact]
    public void Create_ValidCode_ShouldSucceed()
    {
        var barcode = Barcode.Create("8934567890123");
        barcode.Value.Should().Be("8934567890123");
    }

    [Theory]
    [InlineData("2000123005001", true)]
    [InlineData("8934567890123", false)]
    public void IsWeightEmbeddedBarcode_ShouldDetectCorrectly(string code, bool expected)
    {
        var barcode = Barcode.Create(code);
        barcode.IsWeightEmbeddedBarcode().Should().Be(expected);
    }
}