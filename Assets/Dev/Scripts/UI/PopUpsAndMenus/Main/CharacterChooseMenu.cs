using System;
using System.Collections.Generic;
using Dev.Infrastructure;
using Dev.PlayerLogic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Dev.UI.PopUpsAndMenus.Main
{
    public class CharacterChooseMenu : PopUp
    {
        [SerializeField] private CharacterPickUI _characterPickUIPrefab;
        [SerializeField] private Transform _parent;
        [SerializeField] private Image _blockMask;

        private List<CharacterPickUI> _characterPickUis = new List<CharacterPickUI>(4);
        private Action<CharacterClass> _onCharacterChose;

        private GameSettings _gameSettings;

        [Inject]
        private void Init(GameSettings gameSettings)
        {
            _gameSettings = gameSettings;
        }

        private void Start()
        {
            foreach (CharacterData data in _gameSettings.CharactersDataContainer.Datas)
            {
                CharacterPickUI characterPickUI = Instantiate(_characterPickUIPrefab, _parent);
                
                characterPickUI.Setup(data.CharacterIcon, data.CharacterClass, data.CharacterStats.Health);
                characterPickUI.ChooseButton.Clicked.TakeUntilDestroy(this).Subscribe((OnCharacterChoose));
                characterPickUI.Highlight(false);
                
                _characterPickUis.Add(characterPickUI);
            }
        }
        
        private void OnCharacterChoose(EventContext<CharacterPickUI, CharacterClass> context)
        {
            Highlight(context.Sender);
            
            _blockMask.enabled = true;
            
            _onCharacterChose?.Invoke(context.Value);

            _onCharacterChose = null;
        }

        public void StartChoosingCharacter(Action<CharacterClass> onCharacterChose)
        {
            ResetSelection();
            _onCharacterChose = onCharacterChose;
            
            Show();
        }
        
        public override void Show()
        {
            _blockMask.enabled = false;
            base.Show();
        }
      
        private void Highlight(CharacterPickUI targetUI)
        {
            foreach (CharacterPickUI characterPickUi in _characterPickUis)
            {
                characterPickUi.Highlight(characterPickUi == targetUI);
            }
        }

        private void ResetSelection()
        {
            foreach (CharacterPickUI characterPickUi in _characterPickUis)
            {
                characterPickUi.Highlight(false);
            }
        }
        
    }
}