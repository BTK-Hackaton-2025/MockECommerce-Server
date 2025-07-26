using AutoMapper;
using MockECommerce.DAL.Entities;
using MockECommerce.DtoLayer.CategoryDtos;

namespace MockECommerce.BusinessLayer.Mapping;

public class CategoryMapping : Profile
{
    public CategoryMapping()
    {
        CreateMap<Category, CategoryListDto>().ReverseMap();
        CreateMap<Category, CategoryDetailDto>().ReverseMap();
        CreateMap<CreateCategoryDto, Category>().ReverseMap();
        CreateMap<UpdateCategoryDto, Category>().ReverseMap();
    }
}