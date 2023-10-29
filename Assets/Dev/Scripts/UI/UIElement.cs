using UniRx;

namespace Dev.UI
{
    public abstract class UIElement<TBaseType> : UIElementBase where TBaseType : UIElementBase
    {
        public Subject<TBaseType> Clicked { get; } = new Subject<TBaseType>();

        private void Awake()
        {
            _reactiveButton.Clicked.TakeUntilDestroy(this).Subscribe((unit => OnClicked()));
        }

        protected virtual void OnClicked()
        {
            Clicked.OnNext(this as TBaseType);
        }
    }
}