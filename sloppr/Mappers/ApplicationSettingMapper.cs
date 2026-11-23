using Riok.Mapperly.Abstractions;
using sloppr.DTOs;
using sloppr.Models;

namespace sloppr.Mappers;

[Mapper]
public partial class ApplicationSettingMapper
{
    public partial ApplicationSettingDTO ToDto(ApplicationSetting settings);
}
