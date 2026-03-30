using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Domain.Departments
{
    public sealed record Path
    {
        private const char Separator = '/';
        private Path(string value)
        {
            Value = value;
        }
        public string Value { get; }

        public static Path CreateParent(Identifier identifier)
        {
            return new Path(identifier.Value);
        }

        public static Path CreateChild(Department parent, Identifier identifier)
        {
            return new Path(parent.Path.Value + Separator + identifier.Value);
        }
    }
}
