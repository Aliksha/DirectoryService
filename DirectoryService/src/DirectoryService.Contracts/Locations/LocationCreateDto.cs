using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Contracts.Locations
{
    public record LocationCreateDto(string Name, AddressDto Address, string Timezone);
}
