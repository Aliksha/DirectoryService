using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using Framework.EndpointResults;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace DirectoryService.IntegrationTests.Department
{
    [Trait("Category", "Integration")]
    public class SoftDeleteDepartmentTests : DirectorytBaseTest
    {
        private readonly DirectoryTestWebFactory _factory;

        public SoftDeleteDepartmentTests(DirectoryTestWebFactory factory)
            : base(factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task SoftDeleteDepartment_Should_Succeed()
        {
            // arrange
            var locationId = await CreateLocation("Location 1");
            var client = _factory.CreateClient();

            var dto = new DepartmentCreateDto("DepartmentTestingTests999", "departmenttestingtests999", null, [locationId.Value]);

            // act
            var toAddResponse = await client.PostAsJsonAsync("/api/departments", dto);
            Assert.Equal(HttpStatusCode.OK, toAddResponse.StatusCode);

            // реальный Id созданного департамента из Envelope ответа
            var createEnvelope = await toAddResponse.Content.ReadFromJsonAsync<Envelope<Guid, object[]>>();
            Assert.NotNull(createEnvelope);
            var createdDepartmentId = createEnvelope.Result;

            // Soft Delete через API
            var toDeleteResponse = await client.DeleteAsync($"/api/departments/{createdDepartmentId}");
            Assert.Equal(HttpStatusCode.OK, toDeleteResponse.StatusCode);

            var deleteEnvelope = await toDeleteResponse.Content.ReadFromJsonAsync<Envelope<Guid, object[]>>();
            Assert.NotNull(deleteEnvelope);
            Assert.False(deleteEnvelope.IsError);
            Assert.Null(deleteEnvelope.ErrorsList);

            // if Soft Delete реально отработал в postgres
            await ExecuteInDb(async dbContext =>
            {
                var department = await dbContext.Departments
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(d => d.Id == DepartmentId.Current(createdDepartmentId));

                Assert.NotNull(department);
                Assert.True(department.SoftDeleted, "cущность должна быть помечена как удаленная (Soft Deleted).");
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
