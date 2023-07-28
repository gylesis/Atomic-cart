using UnityEngine;

namespace Dev.Utils
{
    public class PlayerTriggerBox : TriggerBox
    {
        protected override void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                base.OnTriggerEnter2D(other);
            }
        }

        protected override void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                base.OnTriggerExit2D(other);
            }
        }
    }
}