using DirectoryService.Contracts.Locations;
using DirectoryService.Contracts.Positions;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;
using Framework.EndpointResults;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace DirectoryService.IntegrationTests.Positions
{
    [Trait("Category", "Integration")]
    public class SoftDeletePositionTests : DirectorytBaseTest
    {
        private readonly DirectoryTestWebFactory _factory;

        public SoftDeletePositionTests(DirectoryTestWebFactory factory)
            : base(factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task SoftDeletePosition_Should_Succeed()
        {
            var client = _factory.CreateClient();

            // реальный департамент в базе, чтобы привязать позишн к нему
            var departmentId = await CreateDepartmentForTest("TestDepartmentForPosition");
            var positionName = PositionName.Create("PositionTEst");
            var dto = new CreatePositionDto(positionName.ToString(), "Test Description", [departmentId.Value]);

            var toAddResponse = await client.PostAsJsonAsync("/api/positions", dto);
            Assert.Equal(HttpStatusCode.OK, toAddResponse.StatusCode);

            var createEnvelope = await toAddResponse.Content.ReadFromJsonAsync<Envelope<Guid, object[]>>();
            Assert.NotNull(createEnvelope);
            var createdPositionId = createEnvelope.Result;

            // Soft Delete через API
            var toDeleteResponse = await client.DeleteAsync($"/api/positions/{createdPositionId}");
            Assert.Equal(HttpStatusCode.OK, toDeleteResponse.StatusCode);

            var deleteEnvelope = await toDeleteResponse.Content.ReadFromJsonAsync<Envelope<Guid, object[]>>();
            Assert.NotNull(deleteEnvelope);
            Assert.False(deleteEnvelope.IsError);
            Assert.Null(deleteEnvelope.ErrorsList);

            await ExecuteInDb(async dbContext =>
            {
                var position = await dbContext.Positions
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(p => p.Id == PositionId.Current(createdPositionId));

                Assert.NotNull(position);
                Assert.True(position.SoftDeleted, "position must be marked as soft deleted");
            });
        }

        private async Task<DepartmentId> CreateDepartmentForTest(string value)
        {
            // реальную локация в БД для домена
            var locationId = await CreateLocation("LocationForDeptartment");

            return await ExecuteInDb(async dbContext =>
            {
                var name = DepartmentName.Create(value).Value;
                var identifier = Identifier.Create("departmentidentifier").Value;

                // временные id исключительно для прохождения валидации домена
                var tempDeptLocationId = DepartmentLocationId.Create();
                var tempDepartmentId = DepartmentId.Create();

                // объект связи с реальной локацией
                var departmentLocation = DepartmentLocation.Create(tempDeptLocationId, tempDepartmentId, locationId);

                var department = global::DirectoryService.Domain.Departments.Department
                    .CreateParent(name, identifier, [departmentLocation]).Value;

                dbContext.Departments.Add(department);
                await dbContext.SaveChangesAsync();

                return department.Id;
            });
        }

        private async Task<LocationId> CreateLocation(string value)
        {
            return await ExecuteInDb(async dbContext =>
            {
                var location = Location.Create(
                    LocationName.Create(value).Value,
                    Address.Create("1", "Street", "Moscow", "Russia").Value,
                    Timezone.Create("MST").Value).Value;

                dbContext.Locations.Add(location);
                await dbContext.SaveChangesAsync();

                return location.Id;
            });
        }
    }
}
