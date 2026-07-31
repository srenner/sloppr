using Riok.Mapperly.Abstractions;
using sloppr.DTOs;
using sloppr.Models;

namespace sloppr.Mappers;

[Mapper]
public partial class AiProviderMapper
{
    public partial AiProviderDTO ToDto(AiProvider provider);
}
