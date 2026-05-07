using System.ComponentModel.DataAnnotations;

namespace EventEase.Models
{
    public class DateGreaterThanAttribute : ValidationAttribute
    {
        private readonly string _comparisonProperty;

        public DateGreaterThanAttribute(string comparisonProperty)
        {
            _comparisonProperty = comparisonProperty;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var endDate = (DateTime?)value;
            var property = validationContext.ObjectType.GetProperty(_comparisonProperty);
            if (property == null) return new ValidationResult($"Unknown property: {_comparisonProperty}");

            var startDate = (DateTime?)property.GetValue(validationContext.ObjectInstance);

            if (endDate.HasValue && startDate.HasValue && endDate <= startDate)
            {
                return new ValidationResult(ErrorMessage ?? "End date must be after start date.");
            }

            return ValidationResult.Success;
        }
    }
}