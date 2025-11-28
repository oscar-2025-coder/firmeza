using AutoMapper;
using Firmeza.Infrastructure.Entities;              // ✅ ENTIDADES CORRECTAS

using Firmeza.API.DTOs.Products;
using Firmeza.API.DTOs.Customers;
using Firmeza.API.DTOs.Sales;

namespace Firmeza.API.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // ============================
            // PRODUCT MAPPINGS
            // ============================
            CreateMap<Product, ProductDto>();
            CreateMap<ProductCreateDto, Product>();
            CreateMap<ProductUpdateDto, Product>();

            // ============================
            // CUSTOMER MAPPINGS
            // ============================
            CreateMap<Customer, CustomerDto>();
            CreateMap<CustomerCreateDto, Customer>();
            CreateMap<CustomerUpdateDto, Customer>();

            // ============================
            // SALE ITEM MAPPINGS
            // ============================
            CreateMap<SaleItem, SaleItemDto>()
                .ForMember(dest => dest.ProductName, 
                           opt => opt.MapFrom(src => src.Product.Name));

            // ============================
            // SALE MAPPINGS
            // ============================
            CreateMap<Sale, SaleDto>()
                .ForMember(dest => dest.CustomerName, 
                           opt => opt.MapFrom(src => src.Customer.FullName))
                .ForMember(dest => dest.Status, 
                           opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<SaleCreateDto, Sale>()
                .ForMember(dest => dest.Date,          opt => opt.Ignore())
                .ForMember(dest => dest.Subtotal,      opt => opt.Ignore())
                .ForMember(dest => dest.Tax,           opt => opt.Ignore())
                .ForMember(dest => dest.Total,         opt => opt.Ignore())
                .ForMember(dest => dest.Status,        opt => opt.Ignore())
                .ForMember(dest => dest.ReceiptFileName, opt => opt.Ignore())
                .ForMember(dest => dest.Items,         opt => opt.Ignore());

            CreateMap<SaleItemCreateDto, SaleItem>()
                .ForMember(dest => dest.UnitPrice, opt => opt.Ignore())
                .ForMember(dest => dest.Amount,    opt => opt.Ignore());
        }
    }
}
