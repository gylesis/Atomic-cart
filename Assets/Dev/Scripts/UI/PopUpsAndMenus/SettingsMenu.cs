using System;
using Cysharp.Threading.Tasks;
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
        
        private SaveLoadService _saveLoadService;
        private AuthService _authService;

        protected override void Awake()
        {
            base.Awake();

            _showStatsButton.Clicked.Subscribe(unit => OnShowStatButtonClicked()).AddTo(this);
            _showProfileSettingsButton.Clicked.Subscribe(unit =>  PopUpService.ShowPopUp<ProfileSettingsMenu>(() => PopUpService.HidePopUp<ProfileSettingsMenu>())).AddTo(this);
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
            
            Profile profile = _saveLoadService.Profile;

            _musicVolume.SetValueWithoutNotify(profile.MusicVolume);
            _soundVolume.SetValueWithoutNotify(profile.AdditionalSoundVolume);
            
            _volumeMute.SetIsOnWithoutNotify(profile.IsVolumeMuted);
        }
        
        private void OnSoundVolumeChanged(float value)
        {
            _saveLoadService.Save(profile => profile.AdditionalSoundVolume = value).Forget();
        }

        private void OnMusicVolumeChanged(float value)
        {
            _saveLoadService.Save(profile => profile.MusicVolume = value).Forget();
        }

        private void OnMuteChanged(bool value)  
        {
            _saveLoadService.Save(profile => profile.IsVolumeMuted = value).Forget();
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
                    PopUpService.ShowPopUp<LobbyMainMenu>();
                }));
            }));
        }
    }
}   