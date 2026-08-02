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
    public class SoftDeleteLocationTests : DirectorytBaseTest
    {
        private readonly DirectoryTestWebFactory _factory;

        public SoftDeleteLocationTests(DirectoryTestWebFactory factory)
            : base(factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task SoftDeleteLocation_Should_Succeed()
        {
            var client = _factory.CreateClient();

            var name = LocationName.Create("LocationTest").Value;
            var address = Address.Create("1", "Street", "Moscow", "Russia").Value;
            var timezone = Timezone.Create("MST").Value;

            var addressDto = new AddressDto(address.HouseNumber, address.Street, address.City, address.HouseNumber);
            var dto = new LocationCreateDto(name.Value, addressDto, timezone.Value);

            var toAddResponse = await client.PostAsJsonAsync("/api/locations", dto);
            Assert.Equal(HttpStatusCode.OK, toAddResponse.StatusCode);

            var createEnvelope = await toAddResponse.Content.ReadFromJsonAsync<Envelope<Guid, object[]>>();
            Assert.NotNull(createEnvelope);
            var createdLocationtId = createEnvelope.Result;

            // Soft Delete через API
            var toDeleteResponse = await client.DeleteAsync($"/api/locations/{createdLocationtId}");
            Assert.Equal(HttpStatusCode.OK, toDeleteResponse.StatusCode);

            var deleteEnvelope = await toDeleteResponse.Content.ReadFromJsonAsync<Envelope<Guid, object[]>>();
            Assert.NotNull(deleteEnvelope);
            Assert.False(deleteEnvelope.IsError);
            Assert.Null(deleteEnvelope.ErrorsList);

            await ExecuteInDb(async dbContext =>
            {
                var location = await dbContext.Locations
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(l => l.Id == LocationId.Current(createdLocationtId));

                Assert.NotNull(location);
                Assert.True(location.SoftDeleted, "location must be marked as soft deleted");
            });
        }
    }
}
