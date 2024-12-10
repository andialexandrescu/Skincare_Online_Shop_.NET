using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Skincare_Online_Shop_.NET.Validations
{
    public class IngredientsSeparatedByCommaValidation: ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // verifica ca val primita ingredientsValue sa fie un string si sa nu fie null sau pur si simplu un space
            if(value is string ingredientsValue && !string.IsNullOrWhiteSpace(ingredientsValue))// ingredientsValue e de fapt string-ul primit la validare
            {
                string pattern = @"^[a-zA-Z]+(?:\s*,\s*[a-zA-Z]+)*$";
                // (?:\s*,\s*[a-zA-Z0-9]+)* verifica aparitia unei virgule in cazul in care sunt mai multe cuvinte si nu permite virgule in plus
                // practic verifica 0 sau mai multe aparitii pentru spatii optionale inainte si dupa virgula (adica \s*,\s*) si mai multe caractere numerice care apar dupa blocul tocmai descris (adica [a-zA-Z]+)
                // spatiile si caracterele alfa din al doilea bloc sunt optionale, virgula e required
                // m-am asigurat ca e validat si un singur ingredient care nu trebuie sa aiba virgula dupa datorita * de la finalul blocului al doilea
                if (Regex.IsMatch(ingredientsValue, pattern))// matching pentru regex, atucni string-ul e valid
                {
                    return ValidationResult.Success;
                }
                else
                {
                    // daca nu se returneaza nicio valoare pana in acest moment, inseamna ca produsul nu respecta sa fie format dintr-un cunvant sau mai multe cuvinte separate printr-un spatiu (oricare doua cuvinte consecutive)
                    return new ValidationResult("The product's ingredients must be separated by a comma when containing more than one ingredient");
                }
            }
            // cand string-ul e null
            return new ValidationResult("The product's ingredients are required before proceeding with the changes");
        }
    }
}
