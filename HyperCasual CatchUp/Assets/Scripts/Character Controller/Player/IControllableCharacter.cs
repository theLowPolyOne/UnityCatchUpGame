using UnityEngine;
using System;

namespace Fallencake.CharacterController
{
    public interface IControllableCharacter
    {
        public event Action<bool, float> GroundedChanged;
        public event Action<bool> Jumped;
        public event Action AirJumped;

        public ScriptableStats PlayerStats { get; }
        public Vector2 Input { get; }
        public Vector3 Speed { get; }
        public Vector3 GroundNormal { get; }
        public void ApplyVelocity(Vector3 vel, PlayerForce forceType);
    }

    public enum PlayerForce
    {
        /// <summary>
        /// Added directly to the players movement speed, to be controlled by the standard deceleration
        /// </summary>
        Burst,

        /// <summary>
        /// An additive force handled by the decay system
        /// </summary>
        Decay
    }
}