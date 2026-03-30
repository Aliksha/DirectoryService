using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Domain.Positions
{
    public sealed record PositionId
    {
        private PositionId(Guid value)
        {
            Value = value;
        }
        public Guid Value { get; }

        public static PositionId Create() => new PositionId(Guid.NewGuid());
        public static PositionId Current(Guid id) => new PositionId(id);
    }
}
