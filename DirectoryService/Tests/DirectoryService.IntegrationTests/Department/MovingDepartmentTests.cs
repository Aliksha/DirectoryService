 using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Departments.UpdateRoot;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.IntegrationTests.Locations;
using Docker.DotNet.Models;
using Framework.EndpointResults;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace DirectoryService.IntegrationTests.Department
{
    [Trait("Category", "Integration")]
    public class MovingDepartmentTests : DirectorytBaseTest
    {
        private readonly DirectoryTestWebFactory _factory;

        public MovingDepartmentTests(DirectoryTestWebFactory factory)
            : base(factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task MoveDepartment_ToNewParent_Should_Succeed()
        {
            // arrange
            var locationId = await CreateLocation("Location Test 1");
            var client = _factory.CreateClient();

            var parentId = await CreateDepartment("Parent Department", "parentdepartment", null, locationId);
            var movingDepartmentId = await CreateDepartment("Moving Department", "movingdepartment", null, locationId);

            var moveDto = new MoveDepartmentDto(ParentId: parentId);

            // act
            var response = await client.PutAsJsonAsync($"/api/departments/{movingDepartmentId}/parent", moveDto);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            await ExecuteInDb(async dbContext =>
            {
                var department = await dbContext.Departments.FindAsync(DepartmentId.Current(movingDepartmentId));
                Assert.NotNull(department);
                Assert.Equal(DepartmentId.Current(parentId), department.ParentId);
                Assert.Equal("parentdepartment.movingdepartment", department.Path.Value);
                Assert.Equal(1, department.Depth);
            });
        }

        [Fact]
        public async Task MoveDepartment_ToRoot_WhenParentIsNull_Should_Succeed()
        {
            // arrange
            var locationId = await CreateLocation("Location Test 2");
            var client = _factory.CreateClient();

            var parentId = await CreateDepartment("InitialParent", "initialparent", null, locationId);
            var movingDepartmentId = await CreateDepartment("Moving Department", "movingdepartment", parentId, locationId);

            var moveDto = new MoveDepartmentDto(ParentId: null);

            // act
            var response = await client.PutAsJsonAsync($"/api/departments/{movingDepartmentId}/parent", moveDto);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            await ExecuteInDb(async dbContext =>
            {
                var department = await dbContext.Departments.FindAsync(DepartmentId.Current(movingDepartmentId));
                Assert.NotNull(department);
                Assert.Null(department.ParentId);
                Assert.Equal("movingdepartment", department.Path.Value);
                Assert.Equal(0, department.Depth);
            });
        }

        [Fact]
        public async Task MoveDepartment_IntoItsOwnSubtree_CYCLE_VALIDATION_Should_ReturnConflict()
        {
            // arrange
            var locationId = await CreateLocation("Location Test 3");
            var client = _factory.CreateClient();

            var rootId = await CreateDepartment("RootBranch", "rootbranch", null, locationId);
            var childId = await CreateDepartment("ChildBranch", "rootbranch-childbranch", rootId, locationId);

            // попытка зацикливания. переносим родителя внутрь его же дочерней ветки
            var moveDto = new MoveDepartmentDto(ParentId: childId);

            // act
            var response = await client.PutAsJsonAsync($"/api/departments/{rootId}/parent", moveDto);

            // assert
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

            var envelope = await response.Content.ReadFromJsonAsync<Envelope<object, object[]>>();
            Assert.NotNull(envelope);
            Assert.True(envelope.IsError);
        }

        [Fact]
        public async Task MoveDepartment_ToSoftDeletedParent_Should_ReturnConflict()
        {
            // arrange
            var locationId = await CreateLocation("Location Test 4");
            var client = _factory.CreateClient();

            var parentId = await CreateDepartment("DeletedParent", "deletedparent", null, locationId);
            var movingId = await CreateDepartment("MovingDept", "movingdept", null, locationId);

            // cимулиция soft deleted parent напрямую в бд
            await ExecuteInDb(async dbContext =>
            {
                var parent = await dbContext.Departments.FindAsync(DepartmentId.Current(parentId));
                if (parent != null)
                {
                    parent.SoftDelete(); // помечаем как удаленный
                    await dbContext.SaveChangesAsync();
                }
            });

            var moveDto = new MoveDepartmentDto(ParentId: parentId);

            // act
            var response = await client.PutAsJsonAsync($"/api/departments/{movingId}/parent", moveDto);

            // assert
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task MoveDepartment_IntoItself_Should_ReturnBadRequest()
        {
            // arrange
            var locationId = await CreateLocation("Location Test 5");
            var client = _factory.CreateClient();

            var movingId = await CreateDepartment("SelfMoveDepartment", "selfmovedepartment", null, locationId);
            var moveDto = new MoveDepartmentDto(ParentId: movingId);

            // act
            var response = await client.PutAsJsonAsync($"/api/departments/{movingId}/parent", moveDto);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task MoveDepartment_When_TargetDepartmentDoesNotExist_Should_ReturnNotFound()
        {
            // arrange
            var client = _factory.CreateClient();
            var nonExistentId = Guid.NewGuid();
            var moveDto = new MoveDepartmentDto(ParentId: null);

            // act
            var response = await client.PutAsJsonAsync($"/api/departments/{nonExistentId}/parent", moveDto);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact] // идемпотентность
        public async Task MoveDepartment_ToTheSameParent_Should_ReturnSuccessImmediately_WithoutChangingDb()
        {
            // arrange
            var locationId = await CreateLocation("Location Teat 6");
            var client = _factory.CreateClient();

            var parentId = await CreateDepartment("SameParent", "sameparent", null, locationId);
            var movingId = await CreateDepartment("MovingDepartment", "sameparent-movingdepartment", parentId, locationId);

            var moveDto = new MoveDepartmentDto(ParentId: parentId);

            // act
            var response = await client.PutAsJsonAsync($"/api/departments/{movingId}/parent", moveDto);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var envelope = await response.Content.ReadFromJsonAsync<Envelope<MovedDepartmentResponseDto, object[]>>();
            Assert.NotNull(envelope);
            Assert.NotNull(envelope.Result);
            Assert.Equal(parentId, envelope.Result.ParentId);
        }

        [Fact]
        public async Task MoveDepartment_ToDifferentParents_NoCycle_ShouldSucceed()
        {
            // arrange
            var locationId = await CreateLocation("Location Test 7");

            Guid departmentA = Guid.Empty;
            Guid parentX = Guid.Empty;
            Guid parentY = Guid.Empty;

            await ExecuteInDb(async dbContext =>
            {
                var idA = DepartmentId.Create();
                var idX = DepartmentId.Create();
                var idY = DepartmentId.Create();

                var locLinkA = DepartmentLocation.Create(DepartmentLocationId.Create(), idA, locationId);
                var locLinkX = DepartmentLocation.Create(DepartmentLocationId.Create(), idX, locationId);
                var locLinkY = DepartmentLocation.Create(DepartmentLocationId.Create(), idY, locationId);

                var deptX = Domain.Departments.Department.CreateParent(
                    DepartmentName.Create("Parent X").Value,
                    Identifier.Create("parent-x").Value,
                    new List<DepartmentLocation> { locLinkX },
                    idX).Value;

                var deptY = Domain.Departments.Department.CreateParent(
                    DepartmentName.Create("Parent Y").Value,
                    Identifier.Create("parent-y").Value,
                    new List<DepartmentLocation> { locLinkY },
                    idY).Value;

                var deptA = Domain.Departments.Department.CreateParent(
                    DepartmentName.Create("Department A").Value,
                    Identifier.Create("dept-a").Value,
                    new List<DepartmentLocation> { locLinkA },
                    idA).Value;

                dbContext.Departments.AddRange(deptA, deptX, deptY);
                await dbContext.SaveChangesAsync();

                departmentA = idA.Value;
                parentX = idX.Value;
                parentY = idY.Value;
            });

            // плоские тела для put запроса перемещения
            var bodyToX = new { parentId = parentX };
            var bodyToY = new { parentId = parentY };

            var client1 = _factory.CreateClient();
            var client2 = _factory.CreateClient();

            // act
            // конкурентные запросы на перенос
            var task1 = client1.PutAsJsonAsync($"/api/departments/{departmentA}/parent", bodyToX);
            var task2 = client2.PutAsJsonAsync($"/api/departments/{departmentA}/parent", bodyToY);

            // код останавливается и ждет, пока оба запроса завершатся на стороне бэкенда
            var responses = await Task.WhenAll(task1, task2);

            // assert проверка результатов
            var successResponse = responses.FirstOrDefault(r => r.StatusCode == HttpStatusCode.OK);
            Assert.NotNull(successResponse);

            await ExecuteInDb(async dbContext =>
            {
                var department = await dbContext.Departments.FindAsync(DepartmentId.Current(departmentA));
                Assert.NotNull(department);

                // из-за FOR UPDATE узел А обязан консистентно привязаться либо к X либо к Y
                bool isUnderX = department.ParentId == DepartmentId.Current(parentX) && department.Path.Value == "parent-x.dept-a";
                bool isUnderY = department.ParentId == DepartmentId.Current(parentY) && department.Path.Value == "parent-y.dept-a";

                Assert.True(isUnderX || isUnderY, "Департамент должен консистентно закрепиться под одним из родителей.");
                Assert.Equal(1, department.Depth);
            });
        }

        [Fact]
        public async Task MoveDepartment_ParallelCyclicMove_ShouldReturnConflictForOneRequest()
        {
            // arrange
            var locationId = await CreateLocation("Location Test 8");

            Guid departmentA = Guid.Empty;
            Guid departmentB = Guid.Empty;

            await ExecuteInDb(async dbContext =>
            {
                var idA = DepartmentId.Create();
                var idB = DepartmentId.Create();

                var locLinkA = DepartmentLocation.Create(DepartmentLocationId.Create(), idA, locationId);
                var locLinkB = DepartmentLocation.Create(DepartmentLocationId.Create(), idB, locationId);

                var deptA = Domain.Departments.Department.CreateParent(
                    DepartmentName.Create("Department A").Value,
                    Identifier.Create("dept-a").Value,
                    new List<DepartmentLocation> { locLinkA },
                    idA).Value;

                var deptB = Domain.Departments.Department.CreateChild(
                    DepartmentName.Create("Department B").Value,
                    Identifier.Create("dept-b").Value,
                    parent: deptA,
                    new List<DepartmentLocation> { locLinkB },
                    idB).Value;

                dbContext.Departments.AddRange(deptA, deptB);
                await dbContext.SaveChangesAsync();

                departmentA = idA.Value;
                departmentB = idB.Value;
            });

            var bodyAtoB = new { parentId = departmentB };
            var bodyBtoA = new { parentId = departmentA };

            var client1 = _factory.CreateClient();
            var client2 = _factory.CreateClient();

            // act
            var task1 = client1.PutAsJsonAsync($"/api/departments/{departmentA}/parent", bodyAtoB);
            var task2 = client2.PutAsJsonAsync($"/api/departments/{departmentB}/parent", bodyBtoA);

            var responses = await Task.WhenAll(task1, task2);

            // assert
            // поток обязан вернуть 409 Conflict
            var conflictResponse = responses.FirstOrDefault(r => r.StatusCode == HttpStatusCode.Conflict);
            Assert.NotNull(conflictResponse);

            // считываем сырую строку json ответа во избежание NotSupportedException
            string jsonString = await conflictResponse.Content.ReadAsStringAsync();
            Assert.NotEmpty(jsonString);

            // проверка текстового кода ошибки
            bool hasExpectedErrorCode = jsonString.Contains("department.move.cycle") ||
                jsonString.Contains("department.move.conflict");

            Assert.True(hasExpectedErrorCode, $"Ожидался код ошибки department.move.conflict. Ответ: {jsonString}");

            // проверка СУБД. физическая петля отсутствует, дерево не сломано
            await ExecuteInDb(async dbContext =>
            {
                var currentA = await dbContext.Departments.FindAsync(DepartmentId.Current(departmentA));
                var currentB = await dbContext.Departments.FindAsync(DepartmentId.Current(departmentB));

                Assert.NotNull(currentA);
                Assert.NotNull(currentB);

                bool isMutualCycle = currentA.ParentId == DepartmentId.Current(departmentB) &&
                                     currentB.ParentId == DepartmentId.Current(departmentA);

                Assert.False(isMutualCycle, "Замкнутый цикл в базе");
            });
        }

        private async Task<LocationId> CreateLocation(string value)
        {
            return await ExecuteInDb(async dbContext =>
            {
                var location = Location.Create(
                    LocationName.Create(value).Value,
                    Address.Create("1", "Street", "Vitebsk", "Belarus").Value,
                    Timezone.Create("MST").Value).Value;

                dbContext.Locations.Add(location);
                await dbContext.SaveChangesAsync();

                return location.Id;
            });
        }

        private async Task<Guid> CreateDepartment(string name, string path, Guid? parentId, LocationId locationId)
        {
            var client = _factory.CreateClient();
            var dto = new DepartmentCreateDto(name, path, parentId, [locationId.Value]);

            var response = await client.PostAsJsonAsync("/api/departments", dto);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var envelope = await response.Content.ReadFromJsonAsync<Envelope<Guid, object[]>>();
            Assert.NotNull(envelope);
            return envelope.Result;
        }
    }
}
