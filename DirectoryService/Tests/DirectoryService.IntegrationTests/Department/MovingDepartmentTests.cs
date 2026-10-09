 using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Departments.UpdateRoot;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.IntegrationTests.Locations;
using Docker.DotNet.Models;
using Framework.EndpointResults;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace DirectoryService.IntegrationTests.Department
{
    [Trait("Category", "Inegration")]
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
