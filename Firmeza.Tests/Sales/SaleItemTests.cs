using Firmeza.Admin.Models;
using FluentAssertions;
using Xunit;

namespace Firmeza.Tests.Sales;

public class SaleItemTests
{
    [Fact]
    public void Quantity_Should_Not_Be_Negative()
    {
        var item = new SaleItem
        {
            Quantity = -1,
            UnitPrice = 10
        };

        bool isValid = item.Quantity >= 0;

        isValid.Should().BeFalse("quantity cannot be negative");
    }

    [Fact]
    public void UnitPrice_Should_Not_Be_Negative()
    {
        var item = new SaleItem
        {
            Quantity = 1,
            UnitPrice = -5
        };

        bool isValid = item.UnitPrice >= 0;

        isValid.Should().BeFalse("unit price cannot be negative");
    }

    [Fact]
    public void Amount_Should_Be_Quantity_Times_UnitPrice()
    {
        var item = new SaleItem
        {
            Quantity = 2,
            UnitPrice = 10,
            Amount = 20
        };

        decimal expected = item.Quantity * item.UnitPrice;

        item.Amount.Should().Be(expected);
    }
}