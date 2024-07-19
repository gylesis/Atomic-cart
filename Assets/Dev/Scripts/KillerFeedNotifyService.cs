using UnityEngine;

namespace Dev
{
    public class KillerFeedNotifyService : MonoBehaviour
    {
        [SerializeField] private KillerFeedNotify _killerFeedNotify;
        [SerializeField] private Transform _parent;

        public void Notify(string killer, string victim)
        {
            KillerFeedNotify killerFeedNotify = Instantiate(_killerFeedNotify, _parent);
            killerFeedNotify.Setup(killer, victim);

            Destroy(killerFeedNotify.gameObject, 5);
        }
    }
}