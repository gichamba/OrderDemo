using System.ComponentModel.DataAnnotations;
using OrderDemo.Application.Common.Results;
using OneOf;

namespace OrderDemo.Application.Utilities {
    /// <summary>
    /// Provides validation services for Data Transfer Objects (DTOs) using DataAnnotations attributes.
    /// </summary>
    /// <remarks>
    /// This class performs validation on DTOs by leveraging <see cref="System.ComponentModel.DataAnnotations"/> attributes,
    /// returning a <see cref="OneOf{T0, T1}"/> result indicating success or validation failure.
    /// </remarks>
    public static class ValidatorService {
        /// <summary>
        /// Validates a DTO instance using DataAnnotations attributes.
        /// </summary>
        /// <typeparam name="T">The type of the DTO to validate, which must be a reference type.</typeparam>
        /// <param name="dto">The DTO instance to validate. Can be null, which results in a validation failure.</param>
        /// <returns>
        /// A <see cref="OneOf{T0, T1}"/> containing either:
        /// <list type="bullet">
        ///    <item><description>A <see cref="Success{T}"/> with the validated DTO if validation passes.</description></item>
        ///    <item><description>A <see cref="ValidationFailure"/> with a list of validation errors if validation fails or the DTO is null.</description></item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// Validation is performed using <see cref="Validator.TryValidateObject"/>, which checks for <see cref="System.ComponentModel.DataAnnotations"/>
        /// attributes (e.g., <see cref="RequiredAttribute"/>, <see cref="StringLengthAttribute"/>) on the DTO's properties.
        /// If the DTO is null, a single validation error is returned indicating the null state.
        /// </remarks>
        public static OneOf<Success<T>, ValidationFailure> ValidateDto<T>(T? dto) where T : class {
            if (dto == null) {
                List<ValidationError> nullDtoErrors =
                [
                    new(
                        Message: $"{typeof(T).Name} DTO cannot be null",
                        MemberNames: [])
                ];
                return new ValidationFailure(nullDtoErrors.AsReadOnly());
            }

            ValidationContext context = new(dto);
            List<ValidationResult> results = [];
            bool isValid = Validator.TryValidateObject(dto, context, results, true);

            if (isValid) {
                return new Success<T>(dto);
            }

            List<ValidationError> validationErrors = results.Select(r => new ValidationError(
                Message: r.ErrorMessage ?? "Validation error",
                MemberNames: r.MemberNames?.ToArray() ?? [])).ToList();

            return new ValidationFailure(validationErrors.AsReadOnly());
        }
    }
}