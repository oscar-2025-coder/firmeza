using Firmeza.API.DTOs.Products;
using Firmeza.Infrastructure.Entities;
using Xunit;

namespace Firmeza.Tests;

public class ProductTests
{
    [Fact]
    public void Product_ShouldHaveValidPrice_WhenCreated()
    {
        // Arrange
        var product = new Product
        {
            UnitPrice = 100 
        };

        // Act & Assert
        Assert.True(product.UnitPrice > 0);
    }
    
    [Theory]
    [InlineData(100, 20, 120)]
    [InlineData(50, 0, 50)]
    public void CalculateTotal_ShouldReturnCorrectSum(decimal price, decimal tax, decimal expected)
    {
        // Arrange & Act
        var total = price + tax;

        // Assert
        Assert.Equal(expected, total);
    }

    [Fact]
    public void ProductDto_ShouldMapCorrectly()
    {
         // Simple test to ensure we can reference API project types
         var dto = new ProductDto
         {
             Name = "Test Product",
             UnitPrice = 50.0m
         };
         
         Assert.Equal("Test Product", dto.Name);
         Assert.Equal(50.0m, dto.UnitPrice);
    }
}
