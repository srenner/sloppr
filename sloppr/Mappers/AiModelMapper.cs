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
}
