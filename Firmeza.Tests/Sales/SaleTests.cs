using Firmeza.Admin.Models;
using FluentAssertions;
using Xunit;

namespace Firmeza.Tests.Sales;

public class SaleTests
{
    [Fact]
    public void Items_Should_Be_Initialized()
    {
        var sale = new Sale();

        sale.Items.Should().NotBeNull();
        sale.Items.Should().BeEmpty("a sale should start with no items");
    }

    [Fact]
    public void Subtotal_Should_Be_Sum_Of_Item_Amounts()
    {
        var sale = new Sale();
        sale.Items.Add(new SaleItem { Amount = 10 });
        sale.Items.Add(new SaleItem { Amount = 15 });

        decimal expected = 25;

        sale.Subtotal = sale.Items.Sum(i => i.Amount);

        sale.Subtotal.Should().Be(expected);
    }

    [Fact]
    public void Tax_Should_Be_Calculated_As_15_Percent()
    {
        // IVA 15% (o el que tú uses)
        decimal taxRate = 0.15m;

        var sale = new Sale();
        sale.Subtotal = 100;

        sale.Tax = sale.Subtotal * taxRate;

        sale.Tax.Should().Be(15);
    }

    [Fact]
    public void Total_Should_Be_Subtotal_Plus_Tax()
    {
        var sale = new Sale
        {
            Subtotal = 100,
            Tax = 15
        };

        sale.Total = sale.Subtotal + sale.Tax;

        sale.Total.Should().Be(115);
    }

    [Fact]
    public void Sale_Should_Have_A_Customer()
    {
        var sale = new Sale
        {
            Customer = new Customer
            {
                FullName = "Test"
            }
        };

        sale.Customer.Should().NotBeNull();
        sale.Customer.FullName.Should().Be("Test");
    }
}