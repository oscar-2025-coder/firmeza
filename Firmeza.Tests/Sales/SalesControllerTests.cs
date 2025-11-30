using AutoMapper;
using Firmeza.API.Controllers;
using Firmeza.API.DTOs.Sales;
using Firmeza.Infrastructure.Data;
using Firmeza.Infrastructure.Entities;
using Firmeza.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;
using Xunit;

namespace Firmeza.Tests.Sales
{
    public class SalesControllerTests
    {
        private readonly FirmezaDbContext _context;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<IPdfService> _mockPdfService;
        private readonly SalesController _controller;

        public SalesControllerTests()
        {
            var options = new DbContextOptionsBuilder<FirmezaDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new FirmezaDbContext(options);

            _mockMapper = new Mock<IMapper>();
            _mockEmailService = new Mock<IEmailService>();
            _mockPdfService = new Mock<IPdfService>();

            _controller = new SalesController(_context, _mockMapper.Object, _mockEmailService.Object, _mockPdfService.Object);
        }

        [Fact]
        public async Task Create_Should_Use_CustomerId_From_Claim()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var customer = new Customer 
            { 
                Id = customerId, 
                Email = "test@test.com", 
                FullName = "Test User",
                DocumentNumber = "123456789",
                PhoneNumber = "555-0000"
            };
            _context.Customers.Add(customer);
            
            var productId = Guid.NewGuid();
            var product = new Product { Id = productId, Name = "Product 1", UnitPrice = 100 };
            _context.Products.Add(product);
            
            await _context.SaveChangesAsync();

            // Mock User Claims
            var claims = new List<Claim>
            {
                new Claim("customerId", customerId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var dto = new SaleCreateDto
            {
                // CustomerId is NOT set here, simulating the frontend behavior
                Items = new List<SaleItemCreateDto>
                {
                    new SaleItemCreateDto { ProductId = productId, Quantity = 1 }
                }
            };

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnedDto = createdResult.Value as SaleDto;
            
            // Verify the sale was created in DB with correct customer
            var saleInDb = await _context.Sales.FirstOrDefaultAsync();
            Assert.NotNull(saleInDb);
            Assert.Equal(customerId, saleInDb.CustomerId);
        }
        
        [Fact]
        public async Task Create_Should_Return_Unauthorized_If_Claim_Missing()
        {
             // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() } // No claims
            };

            var dto = new SaleCreateDto();

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            Assert.Equal("Missing customerId claim.", unauthorizedResult.Value);
        }
    }
}
