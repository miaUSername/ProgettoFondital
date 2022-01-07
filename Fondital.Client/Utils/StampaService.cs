﻿using Fondital.Shared.Dto;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Telerik.Documents.Core.Fonts;
using Telerik.Windows.Documents.Common.FormatProviders;
using Telerik.Windows.Documents.Fixed.Model.Fonts;
using Telerik.Windows.Documents.Flow.FormatProviders.Docx;
using Telerik.Windows.Documents.Flow.FormatProviders.Pdf;
using Telerik.Windows.Documents.Flow.Model;
using Telerik.Windows.Documents.Flow.Model.Editing;

namespace Fondital.Client.Utils
{
    public class StampaService
    {
        private IJSRuntime _jsRuntime { get; set; }
        private NavigationManager _navManager { get; set; }
        private HttpClient _httpClient { get; set; }
        private IConfiguration _config { get; set; }
        private RapportoDto Rapporto { get; set; }
        private RadFlowDocumentEditor Editor { get; set; }
        private int templateTableRows { get; set; }

        public StampaService(IJSRuntime jsRuntime, NavigationManager navManager, HttpClient httpClient, IConfiguration config)
        {
            _jsRuntime = jsRuntime;
            _navManager = navManager;
            _httpClient = httpClient;
            _config = config;
        }

        public async Task StampaDocumenti(RapportoDto rapporto)
        {
            Rapporto = rapporto;
            templateTableRows = Convert.ToInt32(_config["TemplateTableRows"]);

            try
            {
                //REGISTRA FONTS
                Console.WriteLine($"registra fonts - {DateTime.Now.ToLongTimeString()}");
                List<string> fontList = new() { "Arial_.ttf", "Arial_b.ttf", "Arial_b_i.ttf", "Calibri_.ttf", "Cambria_.ttc", "Cambria_b.ttf", "Micross_.ttf" };
                await ImportFonts(fontList);

                //CREAZIONE ZIP
                Console.WriteLine($"crea<ioone zip - {DateTime.Now.ToLongTimeString()}");
                using MemoryStream zipStream = new();
                using ZipArchive archive = new(zipStream, ZipArchiveMode.Create, true);

                //CREAZIONE DOCS
                Console.WriteLine($"creazione docs - {DateTime.Now.ToLongTimeString()}");
                List<string> docList = new List<string> { "BUH-IT", "BUH-RU", "AKT-IT", "AKT-RU" };
                Dictionary<string, RadFlowDocument> documents = new();
                List<Task> readDocsTasks = new();
                
                foreach(var docName in docList)
                {
                    readDocsTasks.Add(OpenDocument($"{docName}.docx", documents));
                }
                Console.WriteLine($"whenall - {DateTime.Now.ToLongTimeString()}");
                await Task.WhenAll(readDocsTasks);

                //ELABORAZIONE DOCS
                Console.WriteLine($"elaborazione docs - {DateTime.Now.ToLongTimeString()}");
                Parallel.ForEach(documents, doc =>
                {
                    //APERTURA DOCUMENTO
                    Editor = new(doc.Value);

                    //POPOLAMENTO
                    PopolaCampi(doc.Key);

                    //CONVERSIONE IN PDF E AGGIUNTA ALLO ZIP
                    PdfFormatProvider pdfProvider = new();
                    using Stream pdfStream = archive.CreateEntry($"{doc.Key}.pdf", CompressionLevel.Optimal).Open();
                    pdfProvider.Export(doc.Value, pdfStream);
                });

                //CHIUSURA ZIP E DOWNLOAD
                Console.WriteLine($"chiusura zip - {DateTime.Now.ToLongTimeString()}");
                archive.Dispose();
                var js = (IJSInProcessRuntime)_jsRuntime;
                Console.WriteLine($"download zip - {DateTime.Now.ToLongTimeString()}");
                await js.InvokeVoidAsync("saveFile", Convert.ToBase64String(zipStream.ToArray()), "application/zip", $"docs_{rapporto.Id}.zip");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void PopolaCampi(string docName)
        {
            var sp = Rapporto.Utente.ServicePartner;
            decimal costoTotVoci = 0;
            decimal costoTotRicambi = 0;
            decimal costoVoce;
            string indirizzo;

            switch (docName)
            {
                case "BUH-IT":
                case "BUH-RU":
                    TrimRigheTabella(0, Rapporto.RapportiVociCosto.Count);
                    TrimRigheTabella(1, Rapporto.Ricambi.Count);

                    #region SERVICE PARTNER
                    Editor.ReplaceText("$SPRagioneSociale$", sp.RagioneSociale ?? "");
                    indirizzo = $"{sp.PostalCode}, {sp.Region}, {sp.City}, {sp.Street}, {sp.HouseNr}";
                    Editor.ReplaceText("$SPIndirizzo$", indirizzo);
                    Editor.ReplaceText("$SPNrRegistraz$", sp.StateRegistrationNr ?? "");
                    Editor.ReplaceText("$SPCF$", sp.INN ?? "");
                    Editor.ReplaceText("$SPCausaleRegistraz$", sp.KPP ?? "");
                    Editor.ReplaceText("$SPCC$", sp.CC ?? "");
                    Editor.ReplaceText("$SPBanca$", sp.BankName ?? "");
                    Editor.ReplaceText("$SPContoCorrisp$", sp.CCC ?? "");
                    Editor.ReplaceText("$SPCodBanca$", sp.BankCode ?? "");
                    Editor.ReplaceText("$SPTelefono$", sp.Phone ?? "");
                    Editor.ReplaceText("$SPEmail$", sp.Email ?? "");
                    #endregion

                    #region CLIENTE
                    Editor.ReplaceText("$ClienteRagioneSociale$", Rapporto.Cliente.FullName ?? "");
                    indirizzo = $"{Rapporto.Cliente.Citta}, {Rapporto.Cliente.Via}, {Rapporto.Cliente.NumCivico}";
                    Editor.ReplaceText("$ClienteIndirizzo$", indirizzo);
                    Editor.ReplaceText("$ClienteCF$", ""); //TO DO 
                    Editor.ReplaceText("$ClienteCausaleRegistraz$", ""); //TO DO
                    Editor.ReplaceText("$ClienteCC$", ""); //TO DO
                    Editor.ReplaceText("$ClienteBanca$", ""); //TO DO
                    Editor.ReplaceText("$ClienteContoCorrisp$", ""); //TO DO
                    Editor.ReplaceText("$ClienteCodBanca$", ""); //TO DO
                    Editor.ReplaceText("$ClienteTelefono$", Rapporto.Cliente.NumTelefono ?? "");
                    #endregion

                    #region CALDAIA
                    Editor.ReplaceText("$Lavorazione$", Rapporto.TipoLavoro ?? "");
                    Editor.ReplaceText("$Matricola$", Rapporto.Caldaia.Matricola ?? "");
                    Editor.ReplaceText("$Indirizzo$", $"{Rapporto.Cliente.Via} {Rapporto.Cliente.NumCivico}, {Rapporto.Cliente.Citta}");
                    Editor.ReplaceText("$Tecnico$", Rapporto.NomeTecnico ?? "");
                    #endregion

                    #region VOCI COSTO
                    foreach (var (voce, i) in Rapporto.RapportiVociCosto.Select((value, index) => (value, index+1)))
                    {
                        Editor.ReplaceText($"$VoceDescr{i}$", docName == "BUH-IT" ? voce.VoceCosto.NomeItaliano ?? "" : voce.VoceCosto.NomeRusso ?? "");
                        Editor.ReplaceText($"$VoceData{i}$", Rapporto.DataIntervento?.ToShortDateString());
                        Editor.ReplaceText($"$VoceQuantita{i}$", voce.Quantita.ToString());
                        costoVoce = voce.Quantita * voce.VoceCosto.Listini.SingleOrDefault()?.Valore ?? 0; //i listini sono già filtrati sul service partner
                        costoTotVoci += costoVoce;
                        Editor.ReplaceText($"$VoceCosto{i}$", $"₽ {costoVoce:0.##}");
                    }
                    Editor.ReplaceText("$VociNumTot$", Rapporto.RapportiVociCosto.Sum(x => x.Quantita).ToString());
                    Editor.ReplaceText("$VociCostoTot$", $"₽ {costoTotVoci:0.##}");
                    #endregion

                    #region RICAMBI
                    foreach (var (ricambio, i) in Rapporto.Ricambi.Select((value, index) => (value, index+1)))
                    {
                        Editor.ReplaceText($"$RicambioCode{i}$", ricambio.Code);
                        Editor.ReplaceText($"$RicambioDescr{i}$", docName == "BUH-IT" ? ricambio.ITDescription ?? "" : ricambio.RUDescription ?? "");
                        Editor.ReplaceText($"$RicambioCosto{i}$", $"₽ {ricambio.Amount:0.##}");
                        Editor.ReplaceText($"$RicambioQta{i}$", ricambio.Quantita.ToString());
                        Editor.ReplaceText($"$RicambioTot{i}$", $"₽ {ricambio.Quantita * ricambio.Amount}");
                    }
                    Editor.ReplaceText("$RicambiNumTot$", Rapporto.Ricambi.Sum(x => x.Quantita).ToString());
                    costoTotRicambi = Rapporto.Ricambi.Sum(x => x.Quantita * x.Amount);
                    Editor.ReplaceText("$RicambiCostoTot$", $"₽ {costoTotRicambi:0.##}");
                    #endregion

                    #region FONDO PAGINA
                    Editor.ReplaceText("$Totale$", $"₽ {costoTotVoci + costoTotRicambi:0.##}");
                    Editor.ReplaceText("$NomeDitta$", sp.Name ?? "");
                    Editor.ReplaceText("$NomeDirettore$", sp.ManagerName ?? "");
                    #endregion

                    break;
                case "AKT-IT":
                case "AKT-RU":
                    TrimRigheTabella(1, Rapporto.Ricambi.Count);
                    TrimRigheTabella(2, Rapporto.RapportiVociCosto.Count);

                    #region SERVICE PARTNER
                    Editor.ReplaceText("$SPRagioneSociale$", sp.RagioneSociale ?? "");
                    indirizzo = $"{sp.PostalCode}, {sp.Region}, {sp.City}, {sp.Street}, {sp.HouseNr}";
                    Editor.ReplaceText("$SPIndirizzo$", indirizzo);
                    Editor.ReplaceText("$SPTelefono$", sp.Phone ?? "");
                    Editor.ReplaceText("$SPEmail$", sp.Email ?? "");
                    #endregion

                    #region CALDAIA
                    Editor.ReplaceText("$MatricolaCaldaia$", Rapporto.Caldaia.Matricola ?? "");
                    Editor.ReplaceText("$TipoCaldaia$", Rapporto.Caldaia.Versione ?? "");
                    Editor.ReplaceText("$DataVendita$", Rapporto.Caldaia.DataVendita?.ToShortDateString());
                    Editor.ReplaceText("$MarcaCaldaia$", Rapporto.Caldaia.Brand?.Desc ?? ""); //torna stringa vuota
                    Editor.ReplaceText("$Venditore$", Rapporto.Caldaia.Manufacturer ?? ""); //torna stringa vuota
                    Editor.ReplaceText("$ModelloCaldaia$", Rapporto.Caldaia.Model ?? "");
                    Editor.ReplaceText("$DataInstallazione$", Rapporto.Caldaia.DataMontaggio?.ToShortDateString());
                    Editor.ReplaceText("$DataPrimaAccens$", Rapporto.Caldaia.DataAvvio?.ToShortDateString());
                    Editor.ReplaceText("$Produttore$", Rapporto.Caldaia.Manufacturer ?? ""); //torna stringa vuota
                    Editor.ReplaceText("$TecnicoPrimaAccensione$", Rapporto.Caldaia.TecnicoPrimoAvvio ?? "");
                    Editor.ReplaceText("$NumCertificatoTecnico$", Rapporto.Caldaia.NumCertificatoTecnico.ToString());
                    Editor.ReplaceText("$DittaPrimaAccensione$", Rapporto.Caldaia.DittaPrimoAvvio ?? "");
                    #endregion

                    #region CLIENTE
                    Editor.ReplaceText("$CittaUtente$", Rapporto.Cliente.Citta ?? "");
                    Editor.ReplaceText("$ViaUtente$", $"{Rapporto.Cliente.Via}, {Rapporto.Cliente.NumCivico}");
                    Editor.ReplaceText("$TelefonoUtente$", Rapporto.Cliente.NumTelefono ?? "");
                    Editor.ReplaceText("$NomeUtente$", Rapporto.Cliente.FullName ?? "");
                    #endregion

                    #region INTERVENTO
                    Editor.ReplaceText("$DataIntervento$", Rapporto.DataIntervento?.ToShortDateString());
                    Editor.ReplaceText("$TecnicoIntervento$", Rapporto.NomeTecnico ?? "");
                    Editor.ReplaceText("$MotivoRiparazione$", Rapporto.MotivoIntervento ?? "");
                    Editor.ReplaceText("$LavoroEffettuato$", Rapporto.TipoLavoro ?? "");
                    #endregion

                    #region RICAMBI
                    foreach (var (ricambio, i) in Rapporto.Ricambi.Select((value, index) => (value, index+1)))
                    {
                        Editor.ReplaceText($"$RicambioCode{i}$", ricambio.Code ?? "");
                        Editor.ReplaceText($"$RicambioDescr{i}$", docName == "AKT-IT" ? ricambio.ITDescription ?? "" : ricambio.RUDescription ?? "");
                        Editor.ReplaceText($"$RicambioQta{i}$", ricambio.Quantita.ToString());
                    }
                    #endregion

                    #region VOCI COSTO
                    costoTotRicambi = Rapporto.Ricambi.Sum(x => x.Quantita * x.Amount);

                    Editor.ReplaceText("$CostoRicambi$", $"₽ {costoTotRicambi:0.##}");
                    foreach (var (voce, i) in Rapporto.RapportiVociCosto.Select((value, index) => (value, index + 1)))
                    {
                        Editor.ReplaceText($"$VoceDescr{i}$", docName == "AKT-IT" ? voce.VoceCosto.NomeItaliano ?? "" : voce.VoceCosto.NomeRusso ?? "");
                        costoVoce = voce.Quantita * voce.VoceCosto.Listini.SingleOrDefault()?.Valore ?? 0;  //i listini sono già filtrati sul service partner
                        costoTotVoci += costoVoce;
                        Editor.ReplaceText($"$VoceCosto{i}$", $"₽ {costoVoce:0.##}");
                    }
                    Editor.ReplaceText("$CostoTotale$", $"₽ {costoTotRicambi + costoTotVoci:0.##}");
                    #endregion

                    #region FONDO PAGINA
                    Editor.ReplaceText("$NomeDitta$", sp.Name ?? "");
                    Editor.ReplaceText("$NomeDirettore$", sp.ManagerName ?? "");
                    #endregion

                    break;
            }
        }

        private void TrimRigheTabella(int tableIndex, int righeDaTenere)
        {
            //seleziona la tabella numero tableIndex e elimina le righe in eccesso per tenere solo quelle che servono
            Table table = Editor.Document.EnumerateChildrenOfType<Table>().ToList()[tableIndex];
            table.Rows.RemoveRange(table.Rows.Count - templateTableRows - 1 + righeDaTenere, templateTableRows - righeDaTenere);
        }

        private async Task ImportFonts(List<string> FileNames)
        {
            foreach (var font in FileNames)
            {
                var response = await _httpClient.GetAsync(Path.Combine(_navManager.BaseUri, "Documents/Fonts", $"{font}"));
                FontsRepository.RegisterFont(
                    new FontFamily(font.Substring(0, font.IndexOf('_'))),
                    font.Contains("_i") ? FontStyles.Italic : FontStyles.Normal,
                    font.Contains("_b") ? FontWeights.Bold : FontWeights.Normal,
                    response.Content.ReadAsByteArrayAsync().Result);
            }
        }

        private async Task<Dictionary<string, RadFlowDocument>> OpenDocument(string docName, Dictionary<string, RadFlowDocument> docList)
        {
            var response = await _httpClient.GetAsync(Path.Combine(_navManager.BaseUri, "Documents/Templates", docName));

            IFormatProvider<RadFlowDocument> fileFormatProvider = new DocxFormatProvider();
            Stream stream = response.Content.ReadAsStream();
            docList.Add(docName, fileFormatProvider.Import(stream));
            
            return docList;
        }
    }
}