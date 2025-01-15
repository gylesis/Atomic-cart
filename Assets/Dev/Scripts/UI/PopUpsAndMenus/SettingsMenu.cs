using System;
using Cysharp.Threading.Tasks;
using Dev.Infrastructure;
using Dev.Infrastructure.Networking;
using Dev.UI.PopUpsAndMenus.Lobby;
using Dev.UI.PopUpsAndMenus.Main;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Dev.UI.PopUpsAndMenus
{
    public class SettingsMenu : PopUp
    {
        [SerializeField] private Slider _musicVolume;
        [SerializeField] private Slider _soundVolume;

        [SerializeField] private Toggle _volumeMute;
        
        [SerializeField] private DefaultReactiveButton _showStatsButton;
        [SerializeField] private TextReactiveButton _showProfileSettingsButton;

        [SerializeField] private DefaultReactiveButton _resetToPrevValues;
        [SerializeField] private DefaultReactiveButton _saveValues;
        
        private SaveLoadService _saveLoadService;
        private AuthService _authService;

        private float _musicVolumePrev;
        private float _soundVolumePrev;
        private bool _volumeMutePrev;
            
        protected override void Awake()
        {
            base.Awake();

            _showStatsButton.Clicked.Subscribe(unit => OnShowStatButtonClicked()).AddTo(this);
            _showProfileSettingsButton.Clicked.Subscribe(unit =>  PopUpService.ShowPopUp<ProfileSettingsMenu>(() => PopUpService.HidePopUp<ProfileSettingsMenu>())).AddTo(this);
           
            _saveValues.Clicked.Subscribe(unit => OnSaveButtonClicked()).AddTo(this);
            _resetToPrevValues.Clicked.Subscribe(unit => OnResetClicked()).AddTo(this);
        }
        
        [Inject]
        private void Construct(SaveLoadService saveLoadService, AuthService authService)
        {
            _authService = authService;
            _saveLoadService = saveLoadService;
        }

        private void Start()
        {
            _musicVolume.OnValueChangedAsObservable().Skip(1).Subscribe(OnMusicVolumeChanged).AddTo(this);
            _soundVolume.OnValueChangedAsObservable().Skip(1).Subscribe(OnSoundVolumeChanged).AddTo(this);
            
            _volumeMute.OnValueChangedAsObservable().Skip(1).Subscribe(OnMuteChanged).AddTo(this);
        }

        public override void Show()
        {
            base.Show();

            Profile profile = _authService.MyProfile;

            _musicVolume.SetValueWithoutNotify(profile.MusicVolume);
            _soundVolume.SetValueWithoutNotify(profile.AdditionalSoundVolume);
            _volumeMute.SetIsOnWithoutNotify(profile.IsVolumeMuted);
            
            SavePrevValues(profile);
            UpdateButtons();
        }

        private void SavePrevValues(Profile profile)
        {
            _musicVolumePrev = profile.MusicVolume;
            _soundVolumePrev = profile.AdditionalSoundVolume;
            _volumeMutePrev = profile.IsVolumeMuted; 
        }
        
        private void OnSoundVolumeChanged(float value)
        {
            UpdateButtons();
        }

        private void OnMusicVolumeChanged(float value)
        {
            UpdateButtons();
        }

        private void OnMuteChanged(bool value)  
        {
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            float musicVolumeValue = _musicVolume.value;
            float soundVolume = _soundVolume.value;
            bool isMuted = _volumeMute.isOn;

            bool hasChanges = musicVolumeValue != _musicVolumePrev || soundVolume != _soundVolumePrev || isMuted != _volumeMutePrev;
            
            _resetToPrevValues.gameObject.SetActive(hasChanges);
            _saveValues.gameObject.SetActive(hasChanges);
        }
        
        private void OnShowStatButtonClicked()
        {
            PopUpService.ShowPopUp<PlayerStatsMenu>();
            PopUpService.HidePopUp<SettingsMenu>();
            
            PopUpService.ShowPopUp<PlayerStatsMenu>((() =>
            {
                PopUpService.HidePopUp<PlayerStatsMenu>();
                PopUpService.ShowPopUp<SettingsMenu>((() =>
                {
                    PopUpService.HidePopUp<SettingsMenu>();

                    if (ConnectionManager.IsInitialized && ConnectionManager.IsConnected)
                        PopUpService.ShowPopUp<LobbyMainMenu>();
                    else
                        PopUpService.ShowPopUp<InGameMenu>();
                }));
            }));
        }
        
        private void OnResetClicked()
        {
            _musicVolume.SetValueWithoutNotify(_musicVolumePrev);
            _soundVolume.SetValueWithoutNotify(_soundVolumePrev);
            _volumeMute.SetIsOnWithoutNotify(_volumeMutePrev);
            
            UpdateButtons();
        }

        private async void OnSaveButtonClicked()
        {
            Curtains.Instance.ShowWithDotAnimation(0);

            var saveResult = await _saveLoadService.Save(profile =>
            {
                profile.MusicVolume = _musicVolume.value;
                profile.AdditionalSoundVolume = _soundVolume.value;
                profile.IsVolumeMuted = _volumeMute.isOn;
            });
            
            SavePrevValues(_authService.MyProfile);
            UpdateButtons();
            
            Curtains.Instance.SetText(saveResult.IsError ? $"{saveResult.ErrorMessage}" : "Settings saved.");
            
            Curtains.Instance.HideWithDelay(saveResult.IsError ? 2 : 0.5f, 0);
        }
    }
}   