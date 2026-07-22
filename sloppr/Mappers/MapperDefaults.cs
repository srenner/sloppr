using Riok.Mapperly.Abstractions;

// Do not warn user if DTO does not contain each field from the corresponding model.
[assembly: MapperDefaults(RequiredMappingStrategy = RequiredMappingStrategy.Target)]