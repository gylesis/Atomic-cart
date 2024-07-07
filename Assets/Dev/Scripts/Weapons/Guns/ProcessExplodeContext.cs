using System;
using Dev.Infrastructure;
using Dev.Levels;
using Dev.PlayerLogic;
using Fusion;
using UnityEngine;

public struct ProcessExplodeContext
{
    public NetworkRunner NetworkRunner { get; private set; }
    public SessionPlayer Owner { get; private set; }
    public bool IsDamageFromServer { get; private set; }
    public bool IsOwnerBot => Owner.IsBot;
    public TeamSide OwnerTeamSide => Owner.TeamSide;
    
    public Vector3 ExplosionPos { get; private set; }
    public float ExplosionRadius { get; private set; }
    public int Damage { get; private set; }
    public LayerMask HitMask { get; private set; }


    public ProcessExplodeContext(
        NetworkRunner networkRunner,
        SessionPlayer owner,
        float explosionRadius,
        int damage,
        Vector3 explosionPos,
        LayerMask hitMask,
        bool isDamageFromServer
    )   
    {
        NetworkRunner = networkRunner;
        ExplosionRadius = explosionRadius;
        Damage = damage;
        ExplosionPos = explosionPos;
        HitMask = hitMask;
        IsDamageFromServer = isDamageFromServer;
        Owner = owner;
    }
}