using CSharpFunctionalExtensions;
using FluentValidation;
using FluentValidation.Results;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace DirectoryService.Application.Locations.Create.Validation
{
    public static class CustomValidators
    {
        public static IRuleBuilderOptionsConditions<T, TElement> MustBeValueObject<T, TElement, TValueObject>(
            this IRuleBuilder<T, TElement> reluBuilder,
            Func<TElement, Result<TValueObject, Error>> factoryMethod)
        {
            return reluBuilder.Custom((value, context) =>
            {
                Result<TValueObject, Error> result = factoryMethod.Invoke(value);

                if (result.IsSuccess)
                    return;

                // context.AddFailure(JsonSerializer.Serialize(result.Error));
                context.AddFailure(new ValidationFailure(context.PropertyPath, result.Error.Message)
                {
                    CustomState = result.Error, // передаем ссылку на сам объект Error
                });
            });
        }

        public static IRuleBuilderOptions<T, TProperty> WithError<T, TProperty>(
            this IRuleBuilderOptions<T, TProperty> rule, Error error)
        {
            // return rule.WithMessage(JsonSerializer.Serialize(error));
            // привязываем сообщение, а ошибку сохраняем в состояние правила
            return rule.WithMessage(error.Message).WithState(_ => error);
        }

        // перегрузка метода для работы с Errors
        public static IRuleBuilderOptionsConditions<T, TElement> MustBeValueObject<T, TElement, TValueObject>(
            this IRuleBuilder<T, TElement> ruleBuilder,
            Func<TElement, Result<TValueObject, Errors>> factoryMethod) // принимает Errors вместо Error
        {
            return ruleBuilder.Custom((value, context) =>
            {
                Result<TValueObject, Errors> result = factoryMethod.Invoke(value);

                if (result.IsSuccess)
                    return;

                foreach (var error in result.Error)
                {
                    context.AddFailure(new ValidationFailure(context.PropertyPath, error.Message)
                    {
                        CustomState = error, // передаем доменную ошибку дальше в конвейер ToErrorList()
                    });
                }
            });
        }
    }
}
