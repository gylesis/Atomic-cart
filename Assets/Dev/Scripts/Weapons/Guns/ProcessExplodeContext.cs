using System;
using Dev.Levels;
using Dev.PlayerLogic;
using Fusion;
using UnityEngine;

public struct ProcessExplodeContext
{
    public NetworkRunner NetworkRunner;
    public float ExplosionRadius;
    public int Damage;
    public Vector3 ExplosionPos;
    public LayerMask HitMask;
    public TeamSide OwnerTeamSide;
    public PlayerRef Owner;
    public bool IsDamageFromServer;
    public Action<ObstacleWithHealth, PlayerRef, int> ObstacleWithHealthHit;
    public Action<NetworkObject, PlayerRef, int> DummyHit;
    public Action<NetworkObject, PlayerRef, int> UnitHit;
    

    public ProcessExplodeContext(
        NetworkRunner networkRunner,
        float explosionRadius,
        int damage,
        Vector3 explosionPos,
        LayerMask hitMask,
        TeamSide ownerTeamSide,
        bool isDamageFromServer,
        Action<ObstacleWithHealth, PlayerRef, int> obstacleWithHealthHit = null,
        Action<NetworkObject, PlayerRef, int> dummyHit = null,
        Action<NetworkObject, PlayerRef, int> unitHit = null)
    {
        NetworkRunner = networkRunner;  
        ExplosionRadius = explosionRadius;
        Damage = damage;
        ExplosionPos = explosionPos;
        HitMask = hitMask;
        OwnerTeamSide = ownerTeamSide;
        ObstacleWithHealthHit = obstacleWithHealthHit;
        IsDamageFromServer = isDamageFromServer;
        DummyHit = dummyHit;
        UnitHit = unitHit;
        Owner = networkRunner.LocalPlayer;
    }
}