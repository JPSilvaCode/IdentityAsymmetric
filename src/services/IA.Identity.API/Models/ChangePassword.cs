using System.ComponentModel.DataAnnotations;

namespace ICWebAPI.Models
{
    public class ChangePassword
    {
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        [EmailAddress(ErrorMessage = "O campo {0} está em formato inválido")]
        [RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@([a-zA-Z0-9_\-\.]+)\.([a-zA-Z]{2,5})$", ErrorMessage = "O campo {0} está em formato inválido")]
        public string Email { get; set; }

        [Required]
        [StringLength(20, ErrorMessage = "O campo {0} precisa ter entre {6} e {20} caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Senha atual")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(20, ErrorMessage = "O campo {0} precisa ter entre {6} e {20} caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Nova Senha")]
        public string NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirme a nova senha")]
        [Compare("NewPassword", ErrorMessage = "As senhas não conferem..")]
        public string ConfirmPassword { get; set; }
    }
}