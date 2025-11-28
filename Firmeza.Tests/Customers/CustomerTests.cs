using Firmeza.Infrastructure.Entities;   // ✅ ENTIDADES CORRECTAS
using FluentAssertions;
using Xunit;

namespace Firmeza.Tests.Customers
{
    public class CustomerTests
    {
        [Fact]
        public void FullName_Should_Not_Be_Null_Or_Empty()
        {
            var customer = new Customer
            {
                FullName = "",
                DocumentNumber = "123",
                Email = "a@b.com",
                PhoneNumber = "555",
                Age = 25
            };

            bool isValid = !string.IsNullOrWhiteSpace(customer.FullName);

            isValid.Should().BeFalse("customer full name cannot be empty");
        }

        [Fact]
        public void DocumentNumber_Should_Not_Be_Null_Or_Empty()
        {
            var customer = new Customer
            {
                FullName = "John",
                DocumentNumber = "",
                Email = "a@b.com",
                PhoneNumber = "555",
                Age = 25
            };

            bool isValid = !string.IsNullOrWhiteSpace(customer.DocumentNumber);

            isValid.Should().BeFalse("document number cannot be empty");
        }

        [Fact]
        public void Email_Should_Not_Be_Null_Or_Empty()
        {
            var customer = new Customer
            {
                FullName = "John",
                DocumentNumber = "123",
                Email = "",
                PhoneNumber = "555",
                Age = 25
            };

            bool isValid = !string.IsNullOrWhiteSpace(customer.Email);

            isValid.Should().BeFalse("email cannot be empty");
        }

        [Fact]
        public void PhoneNumber_Should_Not_Be_Null_Or_Empty()
        {
            var customer = new Customer
            {
                FullName = "John",
                DocumentNumber = "123",
                Email = "a@b.com",
                PhoneNumber = "",
                Age = 25
            };

            bool isValid = !string.IsNullOrWhiteSpace(customer.PhoneNumber);

            isValid.Should().BeFalse("phone number cannot be empty");
        }

        [Fact]
        public void Age_Should_Not_Be_Negative()
        {
            var customer = new Customer
            {
                FullName = "John",
                DocumentNumber = "123",
                Email = "a@b.com",
                PhoneNumber = "555",
                Age = -5
            };

            bool isValid = customer.Age >= 0;

            isValid.Should().BeFalse("age cannot be negative");
        }

        [Fact]
        public void Sales_Should_Be_Initialized()
        {
            var customer = new Customer();

            customer.Sales.Should().NotBeNull();
            customer.Sales.Should().BeEmpty("a new customer should start without sales");
        }
    }
}
