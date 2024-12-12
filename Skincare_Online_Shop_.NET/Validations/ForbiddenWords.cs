using System.ComponentModel.DataAnnotations;

namespace Skincare_Online_Shop_.NET.Validations
{
    public class ForbiddenWords : ValidationAttribute
    {
        private readonly string[] _forbiddenWords;

        public ForbiddenWords(params string[] forbiddenWords)
        {
            _forbiddenWords = forbiddenWords;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is string stringValue)
            {
                foreach (var word in _forbiddenWords)
                {
                    if (stringValue.Contains(word, StringComparison.OrdinalIgnoreCase))
                    {
                        return new ValidationResult($"Ingredientele nu pot conține cuvântul '{word}'.");
                    }
                }
            }

            return ValidationResult.Success;
        }
    }
}
