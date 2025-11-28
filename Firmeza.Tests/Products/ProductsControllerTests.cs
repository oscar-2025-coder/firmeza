using AutoMapper;
using Firmeza.API.Controllers;
using Firmeza.API.DTOs.Products;
using Firmeza.Infrastructure.Data;
using Firmeza.Infrastructure.Entities;        // ✅ ENTIDADES CORRECTAS
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Firmeza.Tests.Products
{
    public class ProductsControllerTests
    {
        private readonly IMapper _mapper;

        public ProductsControllerTests()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Product, ProductDto>();
            });

            _mapper = config.CreateMapper();
        }

        [Fact]
        public async Task GetAll_ReturnsOk_WithProductList()
        {
            var options = new DbContextOptionsBuilder<FirmezaDbContext>()
                .UseInMemoryDatabase("ProductsTestDB")
                .Options;

            using var context = new FirmezaDbContext(options);

            context.Products.Add(new Product
            {
                Id = Guid.NewGuid(),
                Name = "Producto Prueba",
                UnitPrice = 10,
                IsActive = true
            });

            await context.SaveChangesAsync();

            var controller = new ProductsController(context, _mapper);

            var result = await controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var products = Assert.IsAssignableFrom<IEnumerable<ProductDto>>(okResult.Value);

            Assert.Single(products);
            Assert.Equal("Producto Prueba", products.First().Name);
        }
    }
}