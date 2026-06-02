using HotelGenericoApi.Models;
using HotelGenericoApi.DTOs.Request;
using HotelGenericoApi.DTOs.Response;
using Riok.Mapperly.Abstractions;

namespace HotelGenericoApi.Mappings;

[Mapper]
public partial class ProductoMapper
{
    [MapperIgnoreTarget(nameof(Producto.IdProducto))]
    [MapperIgnoreTarget(nameof(Producto.CreatedAt))]
    [MapperIgnoreTarget(nameof(Producto.IdAfectacionIgv))]
    [MapperIgnoreTarget(nameof(Producto.IdCategoria))]
    public partial Producto FromCreate(ProductoCreateDto dto);

    [MapperIgnoreTarget(nameof(Producto.IdProducto))]
    [MapperIgnoreTarget(nameof(Producto.CreatedAt))]
    [MapperIgnoreTarget(nameof(Producto.IdAfectacionIgv))]
    [MapperIgnoreTarget(nameof(Producto.IdCategoria))]
    public partial void UpdateFromDto(ProductoUpdateDto dto, Producto entity);
}