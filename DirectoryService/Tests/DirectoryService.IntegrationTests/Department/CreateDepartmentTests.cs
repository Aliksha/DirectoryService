using DirectoryService.Application.Departments.Create;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Department
{
    //public class CreateDepartmentTests : DirectorytBaseTest
    //{
    //    public CreateDepartmentTests(DirectoryTestWebFactory factory)
    //        : base(factory)
    //    { }

    //    [Fact]
    //    public async Task CreateDepartmentHandler_With_valid_data_Should_succeed()
    //    {
    //        // arrange
    //        var locationId = await CreateLocation("location 1");

    //        var cancellationToken = CancellationToken.None;

    //        // act
    //        var result = await ExecuteHandler((sut) =>
    //        {
    //            var command = new CreateDepartmentCommand(
    //                new DepartmentCreateDto("DepartmentTestingTests55", "departmenttestingtests55", null, [locationId.Value]));
    //            return sut.Handle(command, cancellationToken);
    //        });

    //        // assert
    //        await ExecuteInDb(async dbContext =>
    //        {
    //            var department = await dbContext.Departments
    //                .FirstAsync(d => d.Id == DepartmentId.Current(result.Value), cancellationToken);

    //            Assert.NotNull(department);
    //            Assert.Equal(department.Id.Value, result.Value);

    //            Assert.True(result.IsSuccess);
    //            Assert.NotEqual(Guid.Empty, result.Value);
    //        });
    //    }

    //    [Fact]
    //    public async Task CreateDepartmentHandler_With_invalid_data_Should_failed()
    //    {
    //        // arrange
    //        var locationId = await CreateLocation("location 2");

    //        var cancellationToken = CancellationToken.None;

    //        // act
    //        var result = await ExecuteHandler((sut) =>
    //        {
    //            var command = new CreateDepartmentCommand(
    //                new DepartmentCreateDto("DepartmentTestingTests55", "departmenttestingtests55", null, [locationId.Value]));
    //            return sut.Handle(command, cancellationToken);
    //        });

    //        // assert
    //        Assert.True(result.IsFailure);
    //        Assert.NotEmpty(result.Error);
    //    }

    //    private async Task<LocationId> CreateLocation(string value)
    //    {
    //        return await ExecuteInDb(async dbContext =>
    //        {
    //            var location = Location.Create(
    //                LocationName.Create(value).Value,
    //                Address.Create("1", "Street", "Moscow", "Russia").Value,
    //                Timezone.Create("MST").Value).Value;

    //            dbContext.Locations.Add(location);
    //            await dbContext.SaveChangesAsync();

    //            return location.Id;
    //        });
    //    }

    //    private async Task<T> ExecuteHandler<T>(Func<CreateDepartmentHandler, Task<T>> action)
    //    {
    //        await using var scope = Services.CreateAsyncScope();

    //        var sut = scope.ServiceProvider.GetRequiredService<CreateDepartmentHandler>();

    //        return await action(sut);
    //    }
    //}
}
