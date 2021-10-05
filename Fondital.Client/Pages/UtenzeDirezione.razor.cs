﻿using Fondital.Shared.Dto;
using Fondital.Shared.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telerik.Blazor;
using Telerik.Blazor.Components;

namespace Fondital.Client.Pages
{
    public partial class UtenzeDirezione
	{
		public string SearchText = "";
		public StatoUtente ConStato { get; set; } = new();
		[CascadingParameter]
		public DialogFactory Dialogs { get; set; }
		public List<string> ListaScelta { get; set; } = new List<string>() { };
		public string SceltaCorrente = string.Empty;
		[Parameter]
		public string servicePId { get; set; }
		private List<UtenteDto> ListaUtenti = new List<UtenteDto>();
		private List<RuoloDto> Roles = new List<RuoloDto>();
		private List<UserRolesDto> AssociazioneUserRoles = new List<UserRolesDto>();
		private List<UtenteDto> UtentiDirezione = new List<UtenteDto>();
		private List<int> ListID = new List<int>();
		protected bool ShowAddDialog { get; set; } = false;
		protected bool ShowEditDialog { get; set; } = false;
		protected bool ShowRuoloDialog { get; set; } = false;
		protected UtenteDto UtenteSelected { get; set; }
		protected ServicePartnerDto SpSelected { get; set; } = new ServicePartnerDto() { CodiceCliente = "", CodiceFornitore = "", RagioneSociale = "" };


		protected override async Task OnInitializedAsync()
		{
			ListaScelta = new List<string>() { @localizer["Tutti"], @localizer["Abilitati"], @localizer["Disabilitati"] };
			ListaUtenti = (List<UtenteDto>) await  utenteClient.GetUtenti();

			//UtentiDirezione = (List<UtenteDto>)ListaUtenti.AsQueryable().Where(x => x.Ruoli == (x.Ruoli.Where(x => x.Name.Equals("direction"))));
			foreach (var utente in ListaUtenti)
			{
				
				foreach (var ruolo in utente.Ruoli)
				{
					if (ruolo.Name.Equals("Direzione"))
					{
						if (!UtentiDirezione.Contains(utente))
						{
							ListID.Add(utente.Id);
							UtentiDirezione.Add(utente);
							break;
						}
					}
				}
			}

			SceltaCorrente = null;
			await RefreshUtenti();
		}

		public List<UtenteDto> ListaUtenti_Filtered => ConStato == StatoUtente.Abilitati ? UtentiDirezione.Where(x => x.Email.ToLower().Contains(SearchText.ToLower()) && x.IsAbilitato == true).ToList() :
													   ConStato == StatoUtente.Disabilitati ? UtentiDirezione.Where(x => x.Email.ToLower().Contains(SearchText.ToLower()) && x.IsAbilitato == false).ToList() :
													   UtentiDirezione.Where(x => x.Email.ToLower().Contains(SearchText.ToLower())).ToList();

		protected async Task CloseAndRefresh()
		{
			ShowAddDialog = false;
			ShowEditDialog = false;
			ShowRuoloDialog = false;
			await RefreshUtenti();
		}

		protected async Task RefreshUtenti()
		{
			ListaUtenti = (List<UtenteDto>) await utenteClient.GetUtenti();

			foreach (var utente in ListaUtenti)
			{
				foreach (var ruolo in utente.Ruoli)
				{
					if (ruolo.Name.Equals("Direzione") && !ListID.Contains(utente.Id))
					{
						UtentiDirezione.Add(utente);
						ListID.Add(utente.Id);
					}
				}
			}
			StateHasChanged();
		}

		protected void EditUtente(int utenteId)
		{
			UtenteSelected = ListaUtenti.Single(x => x.Id == utenteId);
			ShowEditDialog = true;
		}


		protected void AssegnaRuolo(int utenteId)
		{
			UtenteSelected = ListaUtenti.Single(x => x.Id == utenteId);
			ShowRuoloDialog = true;
		}

		protected async Task sendMail(int utenteId)
		{
			UtenteSelected = ListaUtenti.Single(x => x.Id == utenteId);
			UtenteDto UtenteToSendMail = (UtenteDto)await utenteClient.GetUtente(UtenteSelected.UserName);
			bool isConfirmed = await Dialogs.ConfirmAsync($"{@localizer["InviaMail"]} {UtenteToSendMail.Nome} {UtenteToSendMail.Cognome} {localizer["ResetPassword"]}");
			if (isConfirmed)
			{
				MailRequest mailRequest = new MailRequest()
				{
					ToEmail = UtenteToSendMail.UserName,
					Subject = localizer["RisettaPassword"],
				};

				await mailClient.sendMail(mailRequest);
				await Dialogs.AlertAsync($"{@localizer["MailInviata"]} {UtenteToSendMail.Email} {@localizer["ResetPassword"]}");
			}
		}

		protected async Task UpdateEnableUtente(int Id)
		{
			UtenteDto ut = ListaUtenti_Filtered.Single(x => x.Id == Id);
			bool isConfirmed = false;
			if (ut.IsAbilitato) isConfirmed = await Dialogs.ConfirmAsync($"{@localizer["ConfermaModificaUtenteAb"]} {ut.Nome} {ut.Cognome} ?", @localizer["ModificaUtente"]);
			else isConfirmed = await Dialogs.ConfirmAsync($"{@localizer["ConfermaModificaUtente"]} {ut.Nome} {ut.Cognome} ?", localizer["ModificaUtente"]);

			if (isConfirmed)
			{
				try
				{
					await utenteClient.UpdateUtente(Id, ut);
					await CloseAndRefresh();
				}
				catch (Exception e)
				{
					throw;
				}
			}
			else
			{
				//	//fai revert: ^ restituisce lo XOR dei due valori
				//	//true XOR true = false
				//	//false XOR true = true kjkj
				ListaUtenti_Filtered.Single(x => x.Id == Id).IsAbilitato ^= true;
			}
		}

		public async Task MyValueChangeHandler(string theUserChoice)
		{
			switch (theUserChoice)
			{
				case "Tutti":
					ConStato = StatoUtente.Tutti;
					break;
				case "Abilitati":
					ConStato = StatoUtente.Abilitati;
					break;
				case "Disabilitati":
					ConStato = StatoUtente.Disabilitati;
					break;
			}
		}

		public enum StatoUtente
		{
			Tutti = 0,
			Abilitati,
			Disabilitati
		}
	}
}
