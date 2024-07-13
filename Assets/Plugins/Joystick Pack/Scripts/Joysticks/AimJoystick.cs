using UnityEngine;
using UnityEngine.UI;

public class AimJoystick : FloatingJoystick
{
    [SerializeField] private Image _shootThresholdImage;

    public void SetThresholdImageState(bool isOn)
    {
        _shootThresholdImage.enabled = isOn;
    }
    
}