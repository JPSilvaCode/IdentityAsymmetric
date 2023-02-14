using System.ComponentModel.DataAnnotations;

namespace IA.Identity.API.Models
{
	public class UserUpdate
	{
		[Required(ErrorMessage = "O campo {0} é obrigatório")]
		[StringLength(30, ErrorMessage = "O campo {0} precisa ter entre {2} e {30} caracteres", MinimumLength = 2)]
		public string FirstName { get; set; }

		[Required(ErrorMessage = "O campo {0} é obrigatório")]
		[StringLength(60, ErrorMessage = "O campo {0} precisa ter entre {2} e {60} caracteres", MinimumLength = 2)]
		public string LastName { get; set; }

		[Required(ErrorMessage = "O campo {0} é obrigatório")]
		[StringLength(11, ErrorMessage = "O campo {0} precisa ter {11} caracteres", MinimumLength = 11)]
		public string ITIN { get; set; }		
	}
}
