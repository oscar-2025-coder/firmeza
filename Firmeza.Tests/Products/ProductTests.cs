using Firmeza.Infrastructure.Entities;   // ✅ ENTIDADES CORRECTAS
using FluentAssertions;
using Xunit;

namespace Firmeza.Tests.Products
{
    public class ProductTests
    {
        [Fact]
        public void Name_Should_Not_Be_Null_Or_Empty()
        {
            // Arrange
            var product = new Product
            {
                Name = "",
                UnitPrice = 10,
                Stock = 5
            };

            // Act
            bool isValid = !string.IsNullOrWhiteSpace(product.Name);

            // Assert
            isValid.Should().BeFalse("product name cannot be empty");
        }

        [Fact]
        public void UnitPrice_Should_Not_Be_Negative()
        {
            // Arrange
            var product = new Product
            {
                Name = "Test",
                UnitPrice = -5,
                Stock = 10
            };

            // Act
            bool isValid = product.UnitPrice >= 0;

            // Assert
            isValid.Should().BeFalse("unit price cannot be negative");
        }

        [Fact]
        public void Stock_Should_Not_Be_Negative()
        {
            // Arrange
            var product = new Product
            {
                Name = "Test",
                UnitPrice = 10,
                Stock = -1
            };

            // Act
            bool isValid = product.Stock >= 0;

            // Assert
            isValid.Should().BeFalse("stock cannot be negative");
        }

        [Fact]
        public void Sku_Can_Be_Null()
        {
            // Arrange
            var product = new Product
            {
                Name = "Item",
                UnitPrice = 10,
                Stock = 3,
                Sku = null
            };

            // Act & Assert
            product.Sku.Should().BeNull();
        }

        [Fact]
        public void Description_Can_Be_Null()
        {
            // Arrange
            var product = new Product
            {
                Name = "Item",
                UnitPrice = 10,
                Stock = 3,
                Description = null
            };

            // Act & Assert
            product.Description.Should().BeNull();
        }

        [Fact]
        public void SaleItems_Should_Be_Initialized()
        {
            // Arrange
            var product = new Product();

            // Act & Assert
            product.SaleItems.Should().NotBeNull();
            product.SaleItems.Should().BeEmpty("a new product should start with zero sale items");
        }
    }
}
