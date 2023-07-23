using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Dev.Weapons.View
{
    public class WeaponUiView : MonoBehaviour
    {
        [SerializeField] private Image _cooldownImage;
        
        private Tweener _tweener;

        public void ShootReloadView(float cooldown, float maxCooldown)
        {
            _tweener?.Kill();

            var currentValue = 1 - cooldown / maxCooldown;
            
            _cooldownImage.fillAmount = currentValue;

            _tweener = DOVirtual.Float(currentValue, 1, cooldown, (value =>
            {
                _cooldownImage.fillAmount = value;
            }));
        }
    }
}