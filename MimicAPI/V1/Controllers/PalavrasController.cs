﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimicAPI.Database;
using MimicAPI.Helpers;
using MimicAPI.V1.Models;
using MimicAPI.V1.Models.DTO;
using MimicAPI.V1.Repositories;
using MimicAPI.Repositories.Contracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MimicAPI.V1.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0", Deprecated = true)]
    [ApiVersion("1.1")]
    [Produces("application/json")]
    public class PalavrasController : ControllerBase
    {
        private readonly IPalavraRepository _repository;
        private readonly IMapper _mapper;
        private readonly MimicContext _banco;

        public PalavrasController(IPalavraRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        /// <summary>
        /// Obtem todas as palavras registradas no banco de dados
        /// </summary>
        /// <param name="query">Filtros de pesquisa</param>
        /// <returns>Todas as palavras do banco de dados</returns>
        [MapToApiVersion("1.0")]
        [MapToApiVersion("1.1")]
        [HttpGet("", Name = "ObterTodas")]
        public ActionResult ObterPalavras([FromQuery] PalavraUrlQuery query)
        {
            var item = _repository.ObterPalavras(query);
            var lista = _mapper.Map<PaginationList<Palavra>, PaginationList<PalavraDTO>>(item);

            foreach (var palavra in lista.Results)
            {
                palavra.Links.Add(new LinkDTO("self", Url.Link("ObterPalavra", new { id = palavra.Id }), "GET"));
            }

            lista.Links.Add(new LinkDTO("self", Url.Link("ObterTodas", query), "GET"));

            if (query.PagNumero.HasValue)
            {
                if (query.PagNumero > item.Paginacao.TotalPaginas)
                {
                    return NotFound();
                }

                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(item.Paginacao));

                if (query.PagNumero + 1 <= item.Paginacao.TotalPaginas)
                {
                    var queryString = new PalavraUrlQuery() { PagNumero = query.PagNumero + 1, PagRegistro = query.PagRegistro, Data = query.Data };
                    lista.Links.Add(new LinkDTO("next", Url.Link("ObterTodas", queryString), "GET"));
                }
                if (query.PagNumero - 1 > 0)
                {
                    var queryString = new PalavraUrlQuery() { PagNumero = query.PagNumero - 1, PagRegistro = query.PagRegistro, Data = query.Data };
                    lista.Links.Add(new LinkDTO("prev", Url.Link("ObterTodas", queryString), "GET"));
                }
            }

            return Ok(lista);
        }

        /// <summary>
        /// Obtem uma única palavra do banco de dados
        /// </summary>
        /// <param name="id">Identificador da palavra</param>
        /// <returns>Uma palavra com seus atributos</returns>
        [MapToApiVersion("1.0")]
        [MapToApiVersion("1.1")]
        [HttpGet("{id}", Name = "ObterPalavra")]
        public ActionResult Obter(int id)
        {
            var obj = _repository.Obter(id);
            if (obj == null)
            {
                return NotFound();
            }

            PalavraDTO palavraDTO = _mapper.Map<Palavra, PalavraDTO>(obj);
            palavraDTO.Links.Add(new LinkDTO("self", Url.Link("ObterPalavra", new { id = palavraDTO.Id }), "GET"));
            palavraDTO.Links.Add(new LinkDTO("update", Url.Link("AtualizarPalavra", new { id = palavraDTO.Id }), "PUT"));
            palavraDTO.Links.Add(new LinkDTO("delete", Url.Link("DeletarPalavra", new { id = palavraDTO.Id }), "DELETE"));

            return Ok(palavraDTO);
        }

        /// <summary>
        /// Insere uma palavra no banco de dados
        /// </summary>
        /// <param name="palavra">Objeto palavra</param>
        /// <returns>Retorna a palavra juntamente de seu id</returns>
        [MapToApiVersion("1.0")]
        [MapToApiVersion("1.1")]
        [Route("")]
        [HttpPost]
        public ActionResult Cadastrar([FromBody] Palavra palavra)
        {
            if (palavra == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState);
            }

            palavra.Ativo = true;
            palavra.Criado = DateTime.Now;
            _repository.Cadastrar(palavra);

            PalavraDTO palavraDTO = _mapper.Map<Palavra, PalavraDTO>(palavra);
            palavraDTO.Links.Add(new LinkDTO("self", Url.Link("ObterPalavra", new { id = palavraDTO.Id }), "GET"));
            return Created($"api/palavras/{palavra.Id}", palavraDTO);
        }

        /// <summary>
        /// Modifica uma palavra do banco
        /// </summary>
        /// <param name="id">Identificador da palavra</param>
        /// <param name="palavra">Objeto palavra</param>
        /// <returns></returns>
        [MapToApiVersion("1.0")]
        [MapToApiVersion("1.1")]
        [HttpPut("{id}", Name = "AtualizarPalavra")]
        public ActionResult Atualizar(int id, [FromBody] Palavra palavra)
        {
            /* var obj = _repository.Obter(id);
             if (obj == null)
             {
                 NotFound();
             }*/

            if (palavra == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState);
            }

            palavra.Id = id;
            /* palavra.Ativo = obj.Ativo;
             palavra.Criado = obj.Criado;*/
            palavra.Atualizado = DateTime.Now;
            _repository.Atualizar(palavra);

            PalavraDTO palavraDTO = _mapper.Map<Palavra, PalavraDTO>(palavra);
            palavraDTO.Links.Add(new LinkDTO("self", Url.Link("ObterPalavra", new { id = palavraDTO.Id }), "GET"));

            return Ok();
        }

        /// <summary>
        /// Exclui uma palavra do banco
        /// </summary>
        /// <param name="id">Identificador da palavra</param>
        /// <returns></returns>
        [MapToApiVersion("1.1")]
        [HttpDelete("{id}", Name = "DeletarPalavra")]
        public ActionResult Deletar(int id)
        {
            var palavra = _repository.Obter(id);
            if (palavra == null)
            {
                return NotFound();
            }

            _repository.Deletar(id);

            return NoContent();
        }
    }
}
