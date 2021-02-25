﻿using Plugin.ValidationRules;
using Plugin.ValidationRules.Rules;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;
using SisVac.Framework.Domain;
using SisVac.Framework.Http;
using SisVac.Framework.Services;
using SisVac.Helpers.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using XF.Material.Forms.UI.Dialogs;

namespace SisVac.ViewModels.Vaccine
{
    public class TrackingVaccinePageViewModel : ScanDocumentViewModel
    {
        List<ApplicationUser> _vaccinatorsList;
        ApplicationUser _vaccinatorUser;

        public TrackingVaccinePageViewModel(
            INavigationService navigationService,
            IPageDialogService dialogService,
            IScannerService scannerService,
            ICitizensApiClient citizensApiClient,
            ICacheService cacheService) : base(navigationService, dialogService, scannerService, cacheService, citizensApiClient)
        {
            _dialogService = dialogService;
            NextCommand = new DelegateCommand(OnNextCommandExecute);
            BackCommand = new DelegateCommand(OnBackCommandExecute);
            ConfirmCommand = new DelegateCommand(OnConfirmCommandExecute);
            ProgressBarIndicator = 0.0f;

            DocumentScanned = async (id) => await GoNextAfterDocumentRead(id);

            VaccinatorSelected = new Validatable<string>();
            VaccinatorSelected.Validations.Add(new IsNotNullOrEmptyRule() { ValidationMessage = "Necesitas seleccionar un vacunador" });

            VaccinationBatch = new Validatable<string>();
            VaccinationBatch.Validations.Add(new IsNotNullOrEmptyRule() { ValidationMessage = "Necesitas seleccionar un lote" });

            Init();
        }
        public int PositionView { get; set; }
        public bool IsBackButtonVisible { get; set; } = false;
        public bool IsNextButtonVisible { get; set; } = true;
        public bool IsConfirmButtonVisible { get; set; } = false;
        public double ProgressBarIndicator { get; set; }
        public string ProgressTextIndicator
        {
            get
            {
                return $"Paso {PositionView + 1} de 4";
            }
        }
        public Person Patient { get; set; } = new Person();
        public Consent Consent { get; set; } = new Consent();
        public Validatable<string> VaccinatorSelected { get; set; }
        public Validatable<string> VaccinationBatch { get; set; }
        public List<string> VaccinatorsName { get; set; }
        public int IndexSelected { get; set; }

        public ICommand NextCommand { get; }
        public ICommand ConfirmCommand { get; set; }
        public ICommand BackCommand { get; }

        public string LotName { get; set; }

        async void Init()
        {
            if (_vaccinatorUser == null)
                _vaccinatorUser = await _cacheService.GetLocalObject<ApplicationUser>(CacheKeyDictionary.VaccinatorDefault);

            if (_vaccinatorsList == null)
                _vaccinatorsList = await _cacheService.GetLocalObject<List<ApplicationUser>>(CacheKeyDictionary.VaccinatorsList);

            if (_vaccinatorsList?.Count > 0)
            {
                VaccinatorsName = _vaccinatorsList.Select(v => v.FullName).ToList();
                await Task.Run(() => IndexSelected = _vaccinatorsList.FindIndex(v => v.Document == _vaccinatorUser.Document));
            }
        }
        private async void OnConfirmCommandExecute()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            if (!VaccinatorSelected.Validate() || !VaccinationBatch.Validate())
            {
                IsBusy = false;
                return;
            }

            var result = await _dialogService.DisplayAlertAsync("Confirmación para la aplicación de dosis", "Primero aplica la dosis de la vacuna al paciente, después confirma la aplicación de la dosis.", "Confirmar", "Cancelar");
            
            if (result)
            {
                using (await MaterialDialog.Instance.LoadingDialogAsync(message: "Validando..."))
                {
                    // TODO: Call API Here
                    // TODO: Send confirmation to the server
                }

                await _dialogService.DisplayAlertAsync("Proceso finalizado", "Has terminado satisfactoriamente.", "Ok");
                await _navigationService.GoBackAsync();
             }
            
            IsBusy = false;
        }

        private async Task GoNextAfterDocumentRead(string id)
        {
            await GetQualificationData(id);

            if (!Qualification.IsValidDocument)
            {
                await _dialogService.DisplayAlertAsync("Ups", "Documento no valido.", "Ok");
                return;
            }

            var patientData = await GetDocumentData(id);
            if (patientData != null && patientData.IsValid)
            { 
                Patient = new Person
                {
                    Age = patientData.Age,
                    Document = patientData.Cedula,
                    FullName = patientData.Name
                };

                Consent = await _citizensApiClient.GetConsent(patientData.Cedula);

                if(Consent.Citizen != null)
                { 
                    IsBackButtonVisible = true;
                    PositionView = 1;
                    ProgressBarIndicator = PositionView / 3.0f;
                }
                else
                {
                    await _dialogService.DisplayAlertAsync("El usuario no ha sido registrado", "El usuario no ha dado consentimiento para vacunación", "OK");
                }
            }
        }

        private async void OnNextCommandExecute()
        {
            switch (PositionView)
            {
                case 0:
                    if (DocumentID.Validate())
                    {
                        await GoNextAfterDocumentRead(DocumentID.Value);
                    }
                    break;
                case 1:
                    //TODO: This validation is not going to use it now. Its here for later.
                    //if (!Qualification.IsEnabled)
                    //{
                    //    await _dialogService.DisplayAlertAsync("Ups", "Paciente no habilitado para vacunar.", "Ok");
                    //    return;
                    //}
                    PositionView = 2;
                    break;
                case 2:
                    IsNextButtonVisible = false;
                    IsConfirmButtonVisible = true;
                    PositionView = 3;
                    break;
                case 3:
                    
                    break;
            }
            ProgressBarIndicator = PositionView / 3.0f;
        }

        private void OnBackCommandExecute()
        {
            switch (PositionView)
            {
                case 0:
                    break;
                case 1:
                    IsBackButtonVisible = false;
                    PositionView = 0;
                    break;
                case 2:
                    PositionView = 1;
                    break;
                case 3:
                    IsNextButtonVisible = true;
                    IsConfirmButtonVisible = false;
                    PositionView = 2;
                    break;
            }
            ProgressBarIndicator = PositionView / 3.0f;
        }
    }
}
