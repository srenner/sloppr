using Riok.Mapperly.Abstractions;
using sloppr.DTOs;
using sloppr.Models;

namespace sloppr.Mappers;

[Mapper]
public partial class AiProviderMapper
{
    public partial AiProviderDTO ToDto(AiProvider provider);

    public partial ICollection<AiProviderDTO> ToDto(ICollection<AiProvider> providers);

    public partial AiProviderHealthDTO ToHealthDto(AiProvider provider);

    public partial AiProvider ToEntity(AiProviderRequest request);
    public partial void UpdateEntity(AiProviderRequest request, AiProvider provider);

#pragma warning disable RMG012 // Source member was not found for target member
    public partial AiProvider FromHealthRequest(AiProviderHealthRequest healthRequest);
#pragma warning restore RMG012 // Source member was not found for target member
}
