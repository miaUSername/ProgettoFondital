﻿using AutoMapper;
using Fondital.Shared.Dto;
using Fondital.Shared.Models;
using Fondital.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace Fondital.Server.Controllers
{
    [ApiController]
    [Route("listiniControl")]
    [Authorize]
    public class ListinoController : ControllerBase
    {
        private readonly ILogger<ListinoController> _logger;
        private readonly IListinoService _listinoService;
        private readonly IMapper _mapper;

        public ListinoController(ILogger<ListinoController> logger, IListinoService listinoService, IMapper mapper)
        {
            _logger = logger;
            _listinoService = listinoService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IEnumerable<ListinoDto>> Get()
        {
            return _mapper.Map<IEnumerable<ListinoDto>>(await _listinoService.GetAllListini());
        }

        [HttpGet("{id}")]
        public async Task<ListinoDto> GetById(int id)
        {
            return _mapper.Map<ListinoDto>(await _listinoService.GetListinoById(id));
        }

        [HttpPost("update/{listinoId}")]
        public async Task UpdateListino([FromBody] ListinoDto listinoDto, int listinoId)
        {
            Listino listinoToUpdate = _mapper.Map<Listino>(listinoDto);
            await _listinoService.UpdateListino(listinoId, listinoToUpdate);
        }

        [HttpPost]
        public async Task CreateListino([FromBody] ListinoDto listinoDto)
        {
            Listino newListino = _mapper.Map<Listino>(listinoDto);
            await _listinoService.AddListino(newListino);
        }
    }
}