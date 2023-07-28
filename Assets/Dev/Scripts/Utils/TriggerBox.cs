﻿using Dev.Infrastructure;
using UniRx;
using UnityEngine;

namespace Dev.Utils
{
    public class TriggerBox : NetworkContext
    {
        public Subject<Collider2D> TriggerEntered { get; } = new Subject<Collider2D>();
        public Subject<Collider2D> TriggerExit { get; } = new Subject<Collider2D>();

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            TriggerEntered.OnNext(other);
        }

        protected virtual void OnTriggerExit2D(Collider2D other)
        {
            TriggerExit.OnNext(other);
        }
    }
}