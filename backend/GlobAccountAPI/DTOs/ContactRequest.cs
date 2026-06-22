using System.ComponentModel.DataAnnotations;

namespace GlobAccountAPI.DTOs
{
    public class ContactRequest : IValidatableObject
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar 100 caracteres.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio.")]
        [EmailAddress(ErrorMessage = "El email debe tener un formato valido.")]
        [StringLength(254, ErrorMessage = "El email no puede superar 254 caracteres.")]
        public string Email { get; set; } = string.Empty;

        [StringLength(30, ErrorMessage = "El telefono no puede superar 30 caracteres.")]
        [RegularExpression(@"^[0-9+()\-\s.]*$", ErrorMessage = "El telefono contiene caracteres no validos.")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "El mensaje es obligatorio.")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "El mensaje debe tener entre 10 y 2000 caracteres.")]
        public string Message { get; set; } = string.Empty;

        [StringLength(4096, ErrorMessage = "El captcha no es valido.")]
        public string? TurnstileToken { get; set; }

        [StringLength(200)]
        public string? Website { get; set; }

        public long? StartedAt { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                yield return new ValidationResult("El nombre es obligatorio.", new[] { nameof(Name) });
            }

            if (string.IsNullOrWhiteSpace(Message))
            {
                yield return new ValidationResult("El mensaje es obligatorio.", new[] { nameof(Message) });
            }
            else if (Message.Trim().Length < 10)
            {
                yield return new ValidationResult("El mensaje debe tener al menos 10 caracteres.", new[] { nameof(Message) });
            }

            if (ContainsControlCharacters(Name))
            {
                yield return new ValidationResult("El nombre contiene caracteres no validos.", new[] { nameof(Name) });
            }

            if (ContainsControlCharacters(Email))
            {
                yield return new ValidationResult("El email contiene caracteres no validos.", new[] { nameof(Email) });
            }

            if (!string.IsNullOrEmpty(Phone) && ContainsControlCharacters(Phone))
            {
                yield return new ValidationResult("El telefono contiene caracteres no validos.", new[] { nameof(Phone) });
            }

            if (ContainsUnsafeMessageControlCharacters(Message))
            {
                yield return new ValidationResult("El mensaje contiene caracteres no validos.", new[] { nameof(Message) });
            }
        }

        private static bool ContainsControlCharacters(string value)
        {
            return value.Any(char.IsControl);
        }

        private static bool ContainsUnsafeMessageControlCharacters(string value)
        {
            return value.Any(character =>
                char.IsControl(character) &&
                character is not '\r' and not '\n' and not '\t');
        }
    }
}
