using Riok.Mapperly.Abstractions;
using sloppr.DTOs;
using sloppr.Models;

namespace sloppr.Mappers;

[Mapper]
public partial class AiModelMapper
{
    public partial AiModelDTO ToDto(AiModel model);
    public partial ICollection<AiModelDTO> ToDto(ICollection<AiModel> models);

    public partial AiModelMinimalDTO ToMinimalDto(AiModel model);
    public partial ICollection<AiModelMinimalDTO> ToMinimalDto(ICollection<AiModel> models);

#pragma warning disable RMG012 // Source member was not found for required target member
    public partial ICollection<AiModel> FromDto(ICollection<AiModelDTO> dtos);
    public partial ICollection<AiModel> FromMinimalDto(ICollection<AiModelMinimalDTO> dtos);
#pragma warning restore RMG012 // Source member was not found for required target member

}
