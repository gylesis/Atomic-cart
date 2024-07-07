using UnityEngine;

namespace Dev.Infrastructure
{
    public struct PlayerInput
    {
        public Vector2 MoveDirection { get; private set; }
        public Vector2 AimDirection { get; private set; }
        public Vector2 CastDirection { get; private set; }
        
        public bool ToCastAbility { get; private set; }
        public bool ToResetAbility { get; private set; }
        
        PlayerInput(Vector2 moveDirection, Vector2 aimDirection, bool toCastAbility, bool toResetAbility, Vector2 castDirection)
        {
            MoveDirection = moveDirection;
            AimDirection = aimDirection;
            ToCastAbility = toCastAbility;
            ToResetAbility = toResetAbility;
            CastDirection = castDirection;
        }

        public void WithMoveDirection(Vector2 direction)
        {
            MoveDirection = direction;
        }
        
        public void WithAimDirection(Vector2 direction)
        {
            AimDirection = direction;
        }
        
        public void WithCastDirection(Vector2 direction)
        {
            CastDirection = direction;
        }

        public void WithCast(bool toCast)
        {
            ToCastAbility = toCast;
        }

        public void WithReset(bool torReset)
        {
            ToResetAbility = torReset;
        }
            
    }
}