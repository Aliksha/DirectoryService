using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Domain.Locations
{
    public sealed record LocationId
    {
        private LocationId(Guid value)
        {
            Value = value;
        }
        public Guid Value { get; }

        public static LocationId Create() => new LocationId(Guid.NewGuid());
        public static LocationId Current(Guid id) => new(id);
    }
}
