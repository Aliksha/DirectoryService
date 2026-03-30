using CSharpFunctionalExtensions;
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

        public static Result<Address> Create(string houseNumber, string street, string city, string country)
        {
            if (string.IsNullOrWhiteSpace(houseNumber))
                return Result.Failure<Address>("House Number cannot be empty.");
            if (string.IsNullOrWhiteSpace(street))
                return Result.Failure<Address>("House Number cannot be empty.");
            if (string.IsNullOrWhiteSpace(city))
                return Result.Failure<Address>("House Number cannot be empty.");
            if (string.IsNullOrWhiteSpace(country))
                return Result.Failure<Address>("House Number cannot be empty.");

            return new Address(houseNumber, street, city, country);
        }
    }
}
