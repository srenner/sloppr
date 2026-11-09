using Riok.Mapperly.Abstractions;
using sloppr.DTOs;
using sloppr.Models;

[Mapper]
public partial class CuisineMapper
{
    public partial CuisineDTO ToDto(Cuisine cuisine);
    public partial Cuisine ToEntity(CuisineRequest request);
    public partial void UpdateEntity(CuisineRequest request, Cuisine cuisine);
}
