using System.ComponentModel.DataAnnotations;

namespace meeplematch_web.Validation
{
    public class DateValidationAttribute : ValidationAttribute
    {
        public DateValidationAttribute()
        {
            ErrorMessage = "Date must be in the future.";
        }
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            if (value is DateTime dateTime)
            {
                if (dateTime < DateTime.Now)
                {
                    return new ValidationResult(ErrorMessage);
                }
            }
            return ValidationResult.Success!;
        }
    }
}
