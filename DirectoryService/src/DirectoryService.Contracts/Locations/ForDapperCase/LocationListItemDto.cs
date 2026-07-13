using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Contracts.Locations.ForDapperCase
{
    public record LocationListItemDto(
      Guid Id,
      string Name,
      string HouseNumber,
      string Street,
      string City,
      string Country,
      DateTime CreatedAt,
      int DepartmentCount);
}
