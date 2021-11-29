using System.ComponentModel.DataAnnotations;

namespace IA.Identity.API.Models
{
    public class RoleChange
    {
        [Required]
        [StringLength(256, ErrorMessage = "O {0} deve ter pelo menos {2} caracteres.", MinimumLength = 2)]
        [Display(Name = "Nome do Antigo Papel")]
        public string OldName { get; set; }

        [Required]
        [StringLength(256, ErrorMessage = "O {0} deve ter pelo menos {2} caracteres.", MinimumLength = 2)]
        [Display(Name = "Nome do Novo Papel")]
        public string NewName { get; set; }
    }
}