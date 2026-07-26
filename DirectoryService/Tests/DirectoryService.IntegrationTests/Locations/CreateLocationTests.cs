using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Locations;
using Framework.EndpointResults;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace DirectoryService.IntegrationTests.Locations
{
    [Trait("Category", "Integration")]
    public class CreateLocationTests : DirectorytBaseTest
    {
        private readonly DirectoryTestWebFactory _factory;

        public CreateLocationTests(DirectoryTestWebFactory factory)
            : base(factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task CreateLocation_With_Valid_Data_Should_Succeed()
        {
            // arrange
            var client = _factory.CreateClient();

            var name = LocationName.Create("LocationTest").Value;
            var address = Address.Create("1", "Street", "Moscow", "Russia").Value;
            var timezone = Timezone.Create("MST").Value;

            var addressDto = new AddressDto(address.HouseNumber, address.Street, address.City, address.HouseNumber);
            var dto = new LocationCreateDto(name.Value, addressDto, timezone.Value);

            // act
            var response = await client.PostAsJsonAsync("/api/locations", dto);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var envelope = await response.Content.ReadFromJsonAsync<Envelope<Guid, object[]>>();

            Assert.NotNull(envelope);
            Assert.False(envelope.IsError);
            Assert.Null(envelope.ErrorsList);
            Assert.NotEqual(Guid.Empty, envelope.Result);

            await ExecuteInDb(async dbContext =>
            {
                var location = await dbContext.Locations
                    .FirstOrDefaultAsync(l => l.Name.Value == dto.Name);

                Assert.NotNull(location);
                Assert.Equal(location.Id.Value, envelope.Result);
            });
        }

        [Fact]
        public async Task CreateLocation_Should_Failed_Conflict()
        {
            // создали локацию и пытаемся создать локацию с таким же названием чтобы получить conflict

            // arrange
            var existingLocationId = await CreateLocation("LocationExists");
            var client = _factory.CreateClient();

            var name = LocationName.Create("LocationExists").Value;
            var address = Address.Create("1", "Street", "Moscow", "Russia").Value;
            var timezone = Timezone.Create("MST").Value;

            var addressDto = new AddressDto(address.HouseNumber, address.Street, address.City, address.HouseNumber);
            var dto = new LocationCreateDto(name.Value, addressDto, timezone.Value);

            // act
            // пытаемся добавить дубликат через api
            var response = await client.PostAsJsonAsync("/api/locations", dto);

            // assert
            // check if 409
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

            // object[] для ErrorsList чтобы System.Text.Json не падал
            var envelope = await response.Content.ReadFromJsonAsync<Envelope<Guid, object[]>>();

            Assert.NotNull(envelope);
            Assert.True(envelope.IsError);
            Assert.NotNull(envelope.ErrorsList);
            Assert.NotEmpty(envelope.ErrorsList);
            Assert.Equal(Guid.Empty, envelope.Result);

            await ExecuteInDb(async dbContext =>
            {
                var location = await dbContext.Locations
                    .Where(l => l.Name.Value == dto.Name)
                    .ToListAsync();

                // проверяем что в базе осталась одна первая запись
                // а вторая была успешно заблокирована
                Assert.Single(location);
                Assert.Equal(existingLocationId.ToString(), location.First().Id.ToString());
            });
        }

        [Fact]
        public async Task GetLocation_With_NonExistengId_Should_Return_NotFound()
        {
            // arrange
            var client = _factory.CreateClient();
            var nonExistingId = Guid.NewGuid();

            // act
            var response = await client.GetAsync($"/api/locations/{nonExistingId}");

            // assert http
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            // assert envelope
            var envelope = await response.Content.ReadFromJsonAsync<Envelope<Guid, object[]>>();

            Assert.NotNull(envelope);
            Assert.True(envelope.IsError);
            Assert.NotNull(envelope.ErrorsList);
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
