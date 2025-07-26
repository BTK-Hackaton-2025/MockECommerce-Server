using AutoMapper;
using MockECommerce.DAL.Entities;
using MockECommerce.DtoLayer.ProductDtos;

namespace MockECommerce.BusinessLayer.Mapping;

public class ProductMapping : Profile
{
    public ProductMapping()
    {
        // Product Entity to ProductDto
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.CategoryName))
            .ForMember(dest => dest.SellerName, opt => opt.MapFrom(src => 
                $"{src.Seller.User.FirstName} {src.Seller.User.LastName}"))
            .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images));

        // CreateProductDto to Product Entity
        CreateMap<CreateProductDto, Product>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.Seller, opt => opt.Ignore())
            .ForMember(dest => dest.Images, opt => opt.Ignore()); // Handle separately

        // UpdateProductDto to Product Entity
        CreateMap<UpdateProductDto, Product>()
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.SellerId, opt => opt.Ignore()) // Don't allow seller change
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.Seller, opt => opt.Ignore())
            .ForMember(dest => dest.Images, opt => opt.Ignore()); // Handle separately

        // ProductImage mappings
        CreateMap<ProductImage, ProductImageDto>();
        
        CreateMap<CreateProductImageDto, ProductImage>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ProductId, opt => opt.Ignore())
            .ForMember(dest => dest.Product, opt => opt.Ignore());
        
        CreateMap<UpdateProductImageDto, ProductImage>()
            .ForMember(dest => dest.ProductId, opt => opt.Ignore())
            .ForMember(dest => dest.Product, opt => opt.Ignore());
    }
}
