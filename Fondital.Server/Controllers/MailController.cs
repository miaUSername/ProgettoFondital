﻿using AutoMapper;
using Fondital.Shared.Dto;
using Fondital.Shared.Models.Auth;
using Fondital.Shared.Services;
using Fondital.Shared.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Fondital.Server.Controllers
{
    [Route("MailController")]
    [ApiController]
    [Authorize(Roles = "Direzione,Service Partner")]
    public class MailController : ControllerBase
    {
        private readonly UserManager<Utente> _userManager;
        private readonly Serilog.ILogger _logger;
        private readonly IMailService _mailService;
        private readonly JwtSettings _jwtSettings;
        private readonly IMapper _mapper;
        private readonly IUtenteService _utService;

        public MailController(Serilog.ILogger logger, UserManager<Utente> userManager, IMailService mailService, IOptionsSnapshot<JwtSettings> jwtSettings, IMapper mapper, IUtenteService utService)
        {
            _userManager = userManager;
            _logger = logger;
            _mailService = mailService;
            _jwtSettings = jwtSettings.Value;
            _mapper = mapper;
            _utService = utService;
        }

        [HttpPost]
        public async Task<IActionResult> SendMail([FromBody] MailRequestDto MailRequest)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(MailRequest.ToEmail);
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var urlConfirmation = $"{_jwtSettings.Audience}/account/resetpassword/{HttpUtility.UrlEncode(MailRequest.ToEmail)}/{HttpUtility.UrlEncode(code)}";
                MailRequest.Body = $"Inserisci una Nuova Passord per confermare l'account cliccando <a href='{urlConfirmation}'>Account/Password</a>";
                _mailService.SendEmail(MailRequest);

                _logger.Information("Info: {Action} {Object} {ObjectId} effettuato con successo", "SENDMAIL", "Utente", MailRequest.ToEmail);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Eccezione {Action} {Object} {ObjectId}", "SENDMAIL", "Utente", MailRequest.ToEmail);
                throw;
            }
        }

        [HttpPost("NewUser")]
        [Authorize(Roles = "Direzione")]
        public async Task<IActionResult> SendMailForNewUser([FromBody] UtenteDto utenteDto)
        {
            Utente utente = _mapper.Map<Utente>(utenteDto);
            try
            {
                //brutto workaround per mettere una pezza alla gestione del tracking di EF Core che si perde dopo il mapping di Automapper
                var sp = utente.ServicePartner;
                utente.ServicePartner = null;
                var result = await _userManager.CreateAsync(utente);
                utente.ServicePartner = sp;
                await _utService.UpdateUtente(utente.UserName, utente);

                var user = await _userManager.FindByEmailAsync(utente.Email);

                var code = await _userManager.GeneratePasswordResetTokenAsync(utente);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var urlConfirmation = $"{_jwtSettings.Audience}/account/resetpassword/{HttpUtility.UrlEncode(utente.Email)}/{HttpUtility.UrlEncode(code)}";
                MailRequestDto _mailRequest = new()
                {
                    ToEmail = utente.Email,
                    Subject = "SETTARE LA PRIMA PASSWORD",
                    Body = $"Inserisci la prima Passord per confermare l'account cliccando <a href='{urlConfirmation}'>Account/Password</a>"
                };
                _mailService.SendEmail(_mailRequest);

                _logger.Information("Info: {Action} {Object} {ObjectId} effettuato con successo", "CREATE", "Utente", utente.UserName);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Eccezione {Action} {Object} {ObjectId}", "CREATE", "Utente", utente.UserName);
                throw;
            }
        }
    }
}