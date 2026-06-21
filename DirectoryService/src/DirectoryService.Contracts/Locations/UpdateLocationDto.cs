using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Contracts.Locations
{
    public record UpdateLocationDto(Guid Id, string? Name, AddressDto? Address, string? Timezone);
}
