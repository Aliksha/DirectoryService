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
    // Trait для быстрой фильтрации и запуска только интеграционных тестов
    [Trait("Category", "Integration")]
    public class CreateDepartmentTests_NoHandler : DirectorytBaseTest
    {
        private readonly DirectoryTestWebFactory _factory;

        // передаем фабрику в базовый класс через : base(factory)
        public CreateDepartmentTests_NoHandler(DirectoryTestWebFactory factory)
            : base(factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task CreateDepartment_With_Valid_Data_Should_Succeed()
        {
            // arrange
            var locationId = await CreateLocation("Location 1");

            var client = _factory.CreateClient();

            var dto = new DepartmentCreateDto("DepartmentTestingTests55", "departmenttestingtests55", null, [locationId.Value]);

            // act (запрос через API)
            // контроллер, а не handler напрямую
            var response = await client.PostAsJsonAsync("/api/departments", dto);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var envelope = await response.Content.ReadFromJsonAsync<Envelope<Guid>>();

            Assert.NotNull(envelope);
            Assert.False(envelope.IsError);
            Assert.Null(envelope.ErrorsList);
            Assert.NotEqual(Guid.Empty, envelope.Result);

            // assert DB, проверка состояния базы через ExecuteInDb
            await ExecuteInDb(async dbContext =>
            {
                var department = await dbContext.Departments
                    .FirstOrDefaultAsync(d => d.Id == DepartmentId.Current(envelope.Result));

                Assert.NotNull(department);
                Assert.Equal(dto.Name, department.Name.Value);
                Assert.Equal(department.Id.Value, envelope.Result);
            });
        }

        [Fact]
        public async Task CreateDepartment_Should_Failed_Validation()
        {
            var locationId = await CreateLocation("Location 1");

            var client = _factory.CreateClient();

            var invalidDto = new DepartmentCreateDto("", "departmenttestingtests55", null, []);

            var response = await client.PostAsJsonAsync("/api/departments", invalidDto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var envelope = await response.Content.ReadFromJsonAsync<Envelope<Guid, object[]>>();

            Assert.NotNull(envelope);
            Assert.True(envelope.IsError);
            Assert.NotNull(envelope.ErrorsList);
            Assert.NotEmpty(envelope.ErrorsList);
            Assert.Equal(Guid.Empty, envelope.Result);

            await ExecuteInDb(async dbContext =>
            {
                var department = await dbContext.Departments
                    .FirstOrDefaultAsync(d => d.Name.Value == invalidDto.Name);

                Assert.Null(department);
            });
        }

        [Fact]
        public async Task CreateDepartment_With_NonExistingLocation_Should_Fail()
        {
            var client = _factory.CreateClient();

            var nonExistingLocationId = Guid.NewGuid();

            var dto = new DepartmentCreateDto("DepartmentTestingTests55", "departmenttestingtests55", null, [nonExistingLocationId]);

            var response = await client.PostAsJsonAsync("/api/departments", dto);

            Assert.True(response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.BadRequest);

            var envelope = await response.Content.ReadFromJsonAsync<Envelope<Guid, object[]>>();

            Assert.NotNull(envelope);
            Assert.True(envelope.IsError);
            Assert.NotNull(envelope.ErrorsList);
            Assert.NotEmpty(envelope.ErrorsList); // if API вернул описание ошибки
            Assert.Equal(Guid.Empty, envelope.Result);

            await ExecuteInDb(async dbContext =>
            {
                var department = await dbContext.Departments
                    .FirstOrDefaultAsync(d => d.Name.Value == dto.Name);

                Assert.Null(department);
            });
        }

        [Fact]
        public async Task GetDepartment_With_NonExistentId_Should_Return_NotFound()
        {
            // arrange
            var client = _factory.CreateClient();
            var nonExistentId = Guid.NewGuid();

            // act
            var response = await client.GetAsync($"/api/departments/{nonExistentId}");

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            // десериализуем как object?, так как структуры DTO подразделения при 404 не будет
            var envelope = await response.Content.ReadFromJsonAsync<Envelope<object, object[]>>();

            Assert.NotNull(envelope);
            Assert.True(envelope.IsError);
            Assert.NotNull(envelope.ErrorsList); // API должен вернуть понятную ошибку "Department not found"
            Assert.Null(envelope.Result);        // в ответе нет никаких данных
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

namespace Framework.EndpointResults
{
    // класс двойник исключительно для нужд интеграционного теста
    public record Envelope<TResult, TErrors>
    {
        public TResult? Result { get; init; }
        public TErrors? ErrorsList { get; init; }
        public bool IsError { get; init; }
    }
}
