﻿using System.ComponentModel.DataAnnotations;

namespace IA.Identity.API.Models
{
    public class UserLogin
    {
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        [EmailAddress(ErrorMessage = "O campo {0} está em formato inválido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        [StringLength(20, ErrorMessage = "O campo {0} precisa ter entre {6} e {20} caracteres", MinimumLength = 6)]
        public string Password { get; set; }
    }
}