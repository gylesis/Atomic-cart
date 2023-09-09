using System;
using TMPro;
using UniRx;
using UnityEngine;

namespace Dev.UI
{
    public class NotificationPopUp : PopUp
    {
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _description;

        private int _removeCooldown = 5;

        public void Init(string title, string description, int removeCooldown)
        {
            _removeCooldown = removeCooldown;
            _title.text = title;
            _description.text = description;
        }

        public override void Show()
        {
            base.Show();

            Observable.Timer(TimeSpan.FromSeconds(_removeCooldown)).Subscribe((l => { Hide(); }));

            Observable
                .Timer(TimeSpan.Zero)
                .Concat(Observable.Interval((TimeSpan.FromSeconds(1))))
                .Take(_removeCooldown)
                .Subscribe((l =>
                {
                    //Debug.Log($"Time left to close notification {_removeCooldown - l + 1}");
                }));
        }
    }
}