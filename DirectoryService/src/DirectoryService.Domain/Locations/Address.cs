using CSharpFunctionalExtensions;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Domain.Locations
{
    public sealed record Address
    {
        private Address(string houseNumber, string street, string city, string country)
        {
            HouseNumber = houseNumber;
            Street = street;
            City = city;
            Country = country;
        }

        public string HouseNumber { get; }
        public string Street { get; }
        public string City { get; }
        public string Country { get; }

        public static Result<Address, Errors> Create(string houseNumber, string street, string city, string country)
        {
            // для накопления всех упавших проверок
            var errors = new List<Error>();

            //if (string.IsNullOrWhiteSpace(houseNumber))
            //    return GeneralErrors.ValueIsInvalid("house_number-is-null");
            //if (string.IsNullOrWhiteSpace(street))
            //    return GeneralErrors.ValueIsInvalid("street-is-null");
            //if (string.IsNullOrWhiteSpace(city))
            //    return GeneralErrors.ValueIsInvalid("city-is-null");
            //if (string.IsNullOrWhiteSpace(country))
            //    return GeneralErrors.ValueIsInvalid("country-is-null");

            if (string.IsNullOrWhiteSpace(houseNumber))
                errors.Add(GeneralErrors.ValueIsInvalid("house_number.empty"));

            if (string.IsNullOrWhiteSpace(street))
                errors.Add(GeneralErrors.ValueIsInvalid("street.empty"));

            if (string.IsNullOrWhiteSpace(city))
                errors.Add(GeneralErrors.ValueIsInvalid("city.empty"));

            if (string.IsNullOrWhiteSpace(country))
                errors.Add(GeneralErrors.ValueIsInvalid("country.empty"));

            if (errors.Count > 0)
            {
                //return new Errors(errors);

                // implicit operator - из List<Error> в Errors
                return Result.Failure<Address, Errors>(errors);
            }

            return new Address(houseNumber, street, city, country);
        }
    }
}
