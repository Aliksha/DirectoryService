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
    public class CreatePositionTests : DirectorytBaseTest
    {
        private readonly DirectoryTestWebFactory _factory;

        public CreatePositionTests(DirectoryTestWebFactory factory)
            : base(factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task CreatePosition_Should_Succeed()
        {
            // arrange
            var client = _factory.CreateClient();

            // реальный департамент в базе, чтобы привязать позишн к нему
            var departmentId = await CreateDepartmentForTest("TestDepartmentForPosition");

            var positionName = PositionName.Create("PositionTEst");

            var dto = new CreatePositionDto(positionName.ToString(), "Test Description", [departmentId.Value]);

            // act
            var response = await client.PostAsJsonAsync("/api/positions", dto);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var envelope = await response.Content.ReadFromJsonAsync<Envelope<Guid, object[]>>();

            Assert.NotNull(envelope);
            Assert.False(envelope.IsError);
            Assert.Null(envelope.ErrorsList);
            Assert.NotEqual(Guid.Empty, envelope.Result);

            await ExecuteInDb(async dbContext =>
            {
                var position = await dbContext.Positions
                    .FirstOrDefaultAsync(p => p.Name.Value == dto.Name);

                Assert.NotNull(position);
                Assert.Equal(position.Id.Value, envelope.Result);
            });
        }

        [Fact]
        public async Task CreatePosition_Should_Failed_Validation()
        {
            // arrange
            var client = _factory.CreateClient();

            // передаем невалидные данные
            var invalidDto = new CreatePositionDto("", "Description", []);

            // act
            var response = await client.PostAsJsonAsync("/api/positions", invalidDto);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var envelope = await response.Content.ReadFromJsonAsync<Envelope<Guid, object[]>>();

            Assert.NotNull(envelope);
            Assert.True(envelope.IsError);
            Assert.NotNull(envelope.ErrorsList);
            Assert.NotEmpty(envelope.ErrorsList);
            Assert.Equal(Guid.Empty, envelope.Result);

            await ExecuteInDb(async dbContext =>
            {
                var position = await dbContext.Positions
                    .FirstOrDefaultAsync(p => p.Name.Value == invalidDto.Name);

                Assert.Null(position);
            });
        }

        [Fact]
        public async Task CreatePosition_With_NonExistingDepartment_Should_Fail()
        {
            // arrange
            var client = _factory.CreateClient();
            var uniqueName = PositionName.Create("PositionTEst").ToString();

            // генерируем случайный Guid департамента, которого гарантированно нет в базе
            var nonExistingDepartmentId = Guid.NewGuid();
            var dto = new CreatePositionDto(uniqueName, "Description", [nonExistingDepartmentId]);

            // act
            var response = await client.PostAsJsonAsync("/api/positions", dto);

            // assert
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.NotFound);

            var envelope = await response.Content.ReadFromJsonAsync<Envelope<Guid, object[]>>();

            Assert.NotNull(envelope);
            Assert.True(envelope.IsError);
            Assert.NotNull(envelope.ErrorsList);
            Assert.NotEmpty(envelope.ErrorsList); // api должен вернуть ошибку отсутствующего департамента
            Assert.Equal(Guid.Empty, envelope.Result);

            await ExecuteInDb(async dbContext =>
            {
                var position = await dbContext.Positions
                    .FirstOrDefaultAsync(p => p.Name.Value == dto.Name);

                Assert.Null(position);
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
