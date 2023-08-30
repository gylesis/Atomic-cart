using Dev.Infrastructure;

namespace Dev.UI
{
    public class CharacterChooseReactiveButton : ReactiveButton<CharacterPickUI, CharacterClass>
    {
        protected override CharacterClass Value => _characterClass;
        protected override CharacterPickUI Sender => _characterPickUI;


        private CharacterClass _characterClass;
        private CharacterPickUI _characterPickUI;

        public void Init(CharacterClass characterClass, CharacterPickUI characterPickUI)
        {
            _characterPickUI = characterPickUI;
            _characterClass = characterClass;
        }
    }
}