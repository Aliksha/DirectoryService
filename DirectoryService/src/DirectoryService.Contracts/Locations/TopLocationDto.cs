using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Contracts.Locations
{
    public record TopLocationDto
    {
        public Guid Id { get; init; }

        public string Name { get; init; } = null!;

        public string HouseNumber { get; init; } = null!;

        public string Street { get; init; } = null!;

        public string City { get; init; } = null!;

        public string Country { get; init; } = null!;

        public string Timezone { get; init; } = null!;

        public int DepartmentCount { get; init; } = 0!;
    }
}
