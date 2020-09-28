﻿using Slothsoft.UnityExtensions;
using UnityEngine;

namespace TheCursedBroom.Player.AvatarStates {
    public class CrouchingState : AvatarState {
        [Header("Crouching")]
        [SerializeField, Range(0, 100)]
        int minimumCrouchFrameCount = 1;

        bool intendedJump;
        int crouchDuration;

        public override void EnterState() {
            base.EnterState();

            crouchDuration = 0;
            intendedJump = avatar.intendsJumpStart;

            avatar.broom.isFlying = false;
            avatar.broom.canBoost = true;

            avatar.UpdateMovement();
        }
        public override void FixedUpdateState() {
            base.FixedUpdateState();

            crouchDuration++;

            avatar.UpdateMovement();
        }

        public override void ExitState() {
            base.ExitState();
        }

        [Header("Transitions")]
        [SerializeField, Expandable]
        AvatarState intendsGlideState = default;
        [SerializeField, Expandable]
        AvatarState intendsJumpState = default;
        [SerializeField, Expandable]
        AvatarState notGroundedState = default;
        [SerializeField, Expandable]
        AvatarState rejectsCrouchState = default;
        public override AvatarState CalculateNextState() {
            if (!avatar.isGrounded && avatar.intendsGlide) {
                return intendsGlideState;
            }
            if (crouchDuration < minimumCrouchFrameCount) {
                return this;
            }
            if (intendedJump || avatar.intendsJumpStart) {
                return intendsJumpState;
            }
            if (!avatar.isGrounded) {
                return notGroundedState;
            }
            if (!avatar.intendsCrouch) {
                return rejectsCrouchState;
            }
            return this;
        }
    }
}