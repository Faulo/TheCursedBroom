﻿using Slothsoft.UnityExtensions;
using UnityEngine;

public abstract class ComponentFeature<TComponent> : MonoBehaviour where TComponent : Component {
    [SerializeField, Expandable]
    TComponent observedComponent = default;
    protected virtual void OnValidate() {
        if (observedComponent == null) {
            observedComponent = GetComponentInParent<TComponent>();
        }
    }
}
