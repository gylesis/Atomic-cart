using System;
using Dev.Infrastructure;
using Dev.Levels;
using Dev.PlayerLogic;
using Fusion;
using UnityEngine;

public struct ProcessExplodeContext : INetworkStruct
{
    public SessionPlayer Owner { get; private set; }
    public bool IsDamageFromServer { get; private set; }
    public bool IsOwnerBot => Owner.IsBot;
    public TeamSide OwnerTeamSide => Owner.TeamSide;
    public Vector3 ExplosionPos { get; private set; }
    public float ExplosionRadius { get; private set; }
    public int Damage { get; private set; }


    public ProcessExplodeContext(
        SessionPlayer owner,
        float explosionRadius,
        int damage,
        Vector3 explosionPos,
        bool isDamageFromServer
    )   
    {
        ExplosionRadius = explosionRadius;
        Damage = damage;
        ExplosionPos = explosionPos;
        IsDamageFromServer = isDamageFromServer;
        Owner = owner;
    }
}