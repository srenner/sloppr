---
name: dto
description: Create C# DTO
invokable: true
---

Convert the selected C# model class into a DTO following our conventions:
- DTO files are placed in the .NET project's "DTOs" folder
- DTO filenames are of the format ModelNameDTO.cs
- Prompt the user for a base class, and if provided, add all fields to the DTO. the DTO should not inherit the base class.
- Do not update the Model class.