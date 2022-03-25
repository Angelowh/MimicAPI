using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MimicAPI.V2.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("2.0")]
    [Produces("application/json")]
    public class PalavrasController : ControllerBase
    {
        /// <summary>
        /// Obtem todas as palavras registradas no banco de dados
        /// </summary>
        /// <param name="query">Filtros de pesquisa</param>
        /// <returns>Todas as palavras do banco de dados</returns>
        [HttpGet("", Name = "ObterTodas")]
        public string ObterPalavras()
        {
            return ("version 2.0");
        }
    }
}
