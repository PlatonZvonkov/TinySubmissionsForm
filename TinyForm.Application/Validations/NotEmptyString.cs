using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace TinyForm.Application.Validations
{
    public class NotEmptyString : ValidationAttribute, IClientModelValidator
    {
        protected override ValidationResult? IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            if (value is string valueStr)
            {
                return valueStr.Length > 0 && string.IsNullOrEmpty(valueStr)
                    ? new ValidationResult(ErrorMessageString)
                    : ValidationResult.Success;
            }

            return new ValidationResult(ErrorMessageString);
        }

        public void AddValidation(ClientModelValidationContext context)
        {
            if (!context.Attributes.ContainsKey("data-val"))
                context.Attributes.Add("data-val", "true");

            var errorMessage = FormatErrorMessage(context.ModelMetadata.GetDisplayName());
            context.Attributes.Add("data-val-notemptystring", errorMessage);
        }
    }
}
