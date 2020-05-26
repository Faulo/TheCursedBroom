﻿using System;
using Slothsoft.UnityExtensions;
using UnityEngine;


namespace AvatarStateMachine {
    public class Gliding : AvatarState {
        enum GlideMode {
            RotationControl,
            AngularVelocityControl
        }
        [Header("Gliding movement")]
        [SerializeField]
        GlideMode mode = GlideMode.RotationControl;
        [SerializeField, Range(0, 720)]
        float rotationSpeed = 360;
        [SerializeField, Range(0, 1)]
        float rotationLerp = 1;

        [Header("Sub-components")]
        [SerializeField, Expandable]
        ParticleSystem particles = default;
        bool particlesEnabled {
            set {
                if (value) {
                    particles.Play();
                } else {
                    particles.Stop();
                }
            }
        }

        public override void EnterState() {
            base.EnterState();

            avatar.isGliding = true;
            particlesEnabled = true;

            avatar.attachedRigidbody.constraints = RigidbodyConstraints2D.None;
            avatar.attachedSprite.transform.rotation = avatar.transform.rotation * Quaternion.Euler(0, 0, 90 * avatar.facingSign);
        }
        public override void FixedUpdateState() {
            base.FixedUpdateState();

            var velocity = avatar.attachedRigidbody.velocity;
            var currentRotation = avatar.currentRotation;
            var intendedRotation = avatar.intendedRotation;

            velocity = currentRotation * Vector2.up * velocity.magnitude;

            avatar.attachedRigidbody.velocity = velocity;

            float angularVelocity = 0;
            switch (mode) {
                case GlideMode.RotationControl:
                    if (currentRotation != intendedRotation) {
                        angularVelocity = Math.Sign((currentRotation * Quaternion.Inverse(intendedRotation)).eulerAngles.z - 180);
                    }
                    break;
                case GlideMode.AngularVelocityControl:
                    angularVelocity = avatar.intendedMovement.y;
                    break;
            }
            avatar.attachedRigidbody.angularVelocity = Mathf.Lerp(avatar.attachedRigidbody.angularVelocity, rotationSpeed * angularVelocity, rotationLerp);
        }

        public override void ExitState() {
            base.ExitState();

            avatar.isGliding = false;
            particlesEnabled = false;

            avatar.attachedRigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
            avatar.attachedRigidbody.rotation = 0;
            avatar.attachedSprite.transform.rotation = avatar.transform.rotation;
        }

        [Header("Transitions")]
        [SerializeField, Expandable]
        AvatarState airborneState = default;
        public override AvatarState CalculateNextState() {
            if (!avatar.intendsGlide) {
                return airborneState;
            }
            return base.CalculateNextState();
        }
    }
}