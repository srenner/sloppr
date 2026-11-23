using Riok.Mapperly.Abstractions;
using sloppr.DTOs;
using sloppr.Models;

namespace sloppr.Mappers;

[Mapper]
public partial class AiCapabilitySettingMapper
{
    public partial AiCapabilitySettingDTO ToDto(AiCapabilitySetting setting);
}
