using UnityEngine;
using UnityEngine.Serialization;

namespace Dev
{
    public class KillerFeedNotifyService : MonoBehaviour
    {
        [FormerlySerializedAs("_killerFeedNotify")] [SerializeField] private KillerFeedNotifyView killerFeedNotifyView;
        [SerializeField] private Transform _parent;

        public void Notify(string killer, string victim)
        {
            KillerFeedNotifyView killerFeedNotifyView = Instantiate(this.killerFeedNotifyView, _parent);
            killerFeedNotifyView.Setup(killer, victim);

            Destroy(killerFeedNotifyView.gameObject, 5);
        }
    }
}