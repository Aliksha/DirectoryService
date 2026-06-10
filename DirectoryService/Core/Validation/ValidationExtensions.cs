using System.Text.Json;
using FluentValidation.Results;
using SharedKernel;

namespace Core.Validation
{
    public static class ValidationExtensions
    {
        public static Errors ToErrorList(this ValidationResult validationResult)
        {
            List<ValidationFailure> validationErrors = validationResult.Errors;

            //var errors = from validationError in validationErrors
            //             let errorMessage = validationError.ErrorMessage
            //             let error = JsonSerializer.Deserialize<Error>(errorMessage)
            //             select Error.Validation(error.Code, errorMessage, validationError.PropertyName);

            var errors = from validationError in validationErrors
                         // Безопасно приводим CustomState обратно к нашему доменному классу Error
                         let error = validationError.CustomState as Error
                         select Error.Validation(
                             error?.Code ?? "value.is.invalid-validation-extensions",
                             validationError.ErrorMessage, // Теперь здесь лежит чистый читаемый текст ошибки!
                             validationError.PropertyName);

            //return errors.ToList();

            // Оборачиваем получившийся список в ваш доменный контейнер Errors
            return new Errors(errors.ToList());
        }
    }
}
