using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Locations;
using SharedKernel;

namespace DirectoryService.Domain.Departments
{
    public class Department
    {
        // ef core
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor.
        private Department()
        {
        }
#pragma warning restore CS8618

        private readonly List<Department> _childDepartments = [];

        private readonly List<DepartmentLocation> _departmentsLocations = [];

        private readonly List<DepartmentPosition> _departmentsPositions = [];

        private Department(
            DepartmentId id,
            DepartmentName name,
            Identifier identifier,
            List<DepartmentLocation> locations, // in Create methods it's already list
            Path path,
            short depth = 0,
            DepartmentId? parentId = null)
        {
            Id = id;
            Name = name;
            Identifier = identifier;
            _departmentsLocations = locations;
            Path = path;
            Depth = depth;
            IsActive = true;
            ParentId = parentId;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            SoftDeleted = false;
            DeletedAt = null;
        }

        public DepartmentId Id { get; private set; }
        public DepartmentName Name { get; private set; }
        public Identifier Identifier { get; private set; }
        public DepartmentId? ParentId { get; private set; }
        public Path Path { get; private set; }
        public short Depth { get; private set; }
        public bool IsActive { get; private set; }
        public IReadOnlyList<Department> ChildDepartments => _childDepartments;
        public IReadOnlyList<DepartmentLocation> Locations => _departmentsLocations;
        public IReadOnlyList<DepartmentPosition> Positions => _departmentsPositions;
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        public bool SoftDeleted { get; private set; }
        public DateTime? DeletedAt { get; private set; }

        public static Result<Department, Error> CreateParent(
            DepartmentName name,
            Identifier identifier,
            IEnumerable<DepartmentLocation> connectionsWithLocations,
            DepartmentId? departmentId = null)
        {
            var connectionsWithLocationsList = connectionsWithLocations.ToList(); // список не самих локаций, а связей с ними

            if (connectionsWithLocationsList.Count == 0)
                return GeneralErrors.ValueIsRequired("department.locations.empty");

            var path = Path.CreateParent(identifier);

            return new Department(departmentId ?? DepartmentId.Create(), name, identifier, connectionsWithLocationsList, path, 0);
        }

        public static Result<Department, Error> CreateChild(
            DepartmentName name,
            Identifier identifier,
            Department parent,
            IEnumerable<DepartmentLocation> connectionsWithLocations,
            DepartmentId? departmentId = null)
        {
            var connectionsWithLocationsList = connectionsWithLocations.ToList();

            if (connectionsWithLocationsList.Count == 0)
                return GeneralErrors.ValueIsRequired("department.locations.empty");

            var path = Path.CreateChild(parent.Path, identifier);

            var parentId = parent.Id;

            return new Department(departmentId ?? DepartmentId.Create(), name, identifier, connectionsWithLocationsList, path, (short)(parent.Depth + 1), parentId);
        }

        public void Rename(DepartmentName newName)
        {
            if(newName is null)
                throw new ArgumentNullException(nameof(newName));

            Name = newName;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateIdentifier(Identifier identifier)
        {
            if (identifier is null)
                throw new ArgumentNullException(nameof(identifier));

            Identifier = identifier;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateLocations(IEnumerable<DepartmentLocation> newConnectionsWithLocations)
        {
            if (newConnectionsWithLocations is null)
                throw new ArgumentNullException(nameof(newConnectionsWithLocations));

            // Очищаем старые связи в приватном списке (EF Core зафиксирует удаление)
            // (меняем содержимое списка, не меняя ссылку на сам список
            // ТАК НЕЛЬЗЯ (будет ошибка компиляции из-за readonly)
            //_departmentsLocations = newConnectionsWithLocations.ToList(); )

            _departmentsLocations.Clear();
            _departmentsLocations.AddRange(newConnectionsWithLocations);
            UpdatedAt = DateTime.UtcNow;
        }

        public void SoftDelete()
        {
            SoftDeleted = true;
            DeletedAt = DateTime.UtcNow;
        }

        public void Touch()
        {
            UpdatedAt = DateTime.Now;
        }

        public void Activate()
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
