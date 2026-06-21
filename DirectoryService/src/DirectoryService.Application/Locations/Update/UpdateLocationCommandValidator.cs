using Core.Validation;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Locations;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Application.Locations.Update
{
    public class UpdateLocationCommandValidator : AbstractValidator<UpdateLocationCommand>
    {
        public UpdateLocationCommandValidator()
        {
            // _ чтобы заглушить предупреждение IDE0058

            // 1. Валидация Name (только если оно передано клиентом)
            _ = When(x => !string.IsNullOrWhiteSpace(x.Dto.Name), () =>
            {
                RuleFor(x => x.Dto.Name)
                    .MustBeValueObject(LocationName.Create);
            });

            // 2. Валидация Address (только если объект адреса присутствует в запросе)
            _ = When(x => x.Dto.Address != null, () =>
            {
                RuleFor(x => x.Dto.Address)
                    // Благодаря перегрузке, передаем лямбду,
                    // которая принимает на вход dto адреса (addr) и возвращает Result<Address, Errors>
                    .MustBeValueObject(address => Address.Create(
                        address.HouseNumber,
                        address.Street,
                        address.City,
                        address.Country
                    ));
            });

            // 3. Валидация Timezone (только если она передана клиентом)
            _ = When(x => !string.IsNullOrWhiteSpace(x.Dto.Timezone), () =>
            {
                RuleFor(x => x.Dto.Timezone)
                    .MustBeValueObject(Timezone.Create);
            });
        }
    }
}
