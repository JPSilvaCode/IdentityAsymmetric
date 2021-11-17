using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.Linq;

namespace IA.WebAPI.Core.Controllers
{
    [ApiController]
    public abstract class MainController : Controller
    {
        private readonly ICollection<string> _erros = new List<string>();

        protected ActionResult CustomResponse(object result = null)
        {
            if (ValidOperation())
            {
                return Ok(result);
            }

            return BadRequest(
                new ValidationProblemDetails(
                new Dictionary<string, string[]> {{ "Messages", _erros.ToArray() }}
                )
            );
        }

        protected ActionResult CustomResponse(ModelStateDictionary modelState)
        {
            var erros = modelState.Values.SelectMany(e => e.Errors);
            foreach (var erro in erros)
            {
                AddProcessingError(erro.ErrorMessage);
            }

            return CustomResponse();
        }

        protected ActionResult CustomResponse(ValidationResult validationResult)
        {
            foreach (var erro in validationResult.Errors)
            {
                AddProcessingError(erro.ErrorMessage);
            }

            return CustomResponse();
        }

        //protected ActionResult CustomResponse(ResponseResult resposta)
        //{
        //    ResponsePossuiErros(resposta);

        //    return CustomResponse();
        //}

        //protected bool ResponsePossuiErros(ResponseResult resposta)
        //{
        //    if (resposta == null || !resposta.Errors.Mensagens.Any()) return false;

        //    foreach (var mensagem in resposta.Errors.Mensagens)
        //    {
        //        AdicionarErroProcessamento(mensagem);
        //    }

        //    return true;
        //}

        protected bool ValidOperation()
        {
            return !_erros.Any();
        }

        protected void AddProcessingError(string erro)
        {
            _erros.Add(erro);
        }

        protected void ClearProcessingErrors()
        {
            _erros.Clear();
        }
    }
}