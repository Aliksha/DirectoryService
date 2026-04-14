using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Contracts.Locations
{
    public record AddressDto(string HouseNumber, string Street, string City, string Country);
}
