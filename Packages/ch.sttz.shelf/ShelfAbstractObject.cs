/* --------------------------------------------------------
 * The Shelf v3
 * --------------------------------------------------------
 * Use of this script is subject to the Unity Asset Store
 * End User License Agreement:
 *
 * https://unity.com/legal/as-terms
 *
 * Use of this script for any other purpose than outlined
 * in the EULA linked above is prohibited.
 * --------------------------------------------------------
 * © 2024 Adrian Stutz (adrian@sttz.ch)
 */

using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace sttz.TheShelf {

/// <summary>
/// Base class for both <see cref="ShelfSceneObject"/> 
/// and <see cref="ShelfPrefabObject"/>
/// </remarks>
public abstract class ShelfAbstractObject<TStatus> : ScriptableObject
    where TStatus : struct, Enum
{
    /// <summary>
    /// The status of the reference of this wrapper.
    /// </summary>
    public TStatus ReferenceStatus {
        get => _referenceStatus;
        protected set {
            if (_referenceStatus.Equals(value)) return;
            _referenceStatus = value;
            OnReferenceStatusChanged?.Invoke(_referenceStatus);
        }
    }
    TStatus _referenceStatus;

    /// <summary>
    /// Event triggered when <see cref="ReferenceStatus"/> changes.
    /// </summary>
    public event Action<TStatus> OnReferenceStatusChanged;

    /// <summary>
    /// Bind the visual element to the reference status of this wrapper.
    /// Note that the binding must be made before the element is added
    /// to the hierarchy and that it will automatically unbind when
    /// the element is removed.
    /// </summary>
    /// <param name="visualElement">Element to bind</param>
    /// <param name="onUpdate">Function to call immediately and when the status changes</param>
    protected void BindToReferenceStatus(CallbackEventHandler visualElement, Action<TStatus> onUpdate)
    {
        Action<TStatus> statusChanged = null;
        EventCallback<AttachToPanelEvent> attach = null;
        EventCallback<DetachFromPanelEvent> detach = null;

        statusChanged = status => {
            onUpdate(status);
        };
        attach = evt => {
            OnReferenceStatusChanged += statusChanged;
        };
        detach = evt => {
            OnReferenceStatusChanged -= statusChanged;
        };

        visualElement.RegisterCallback(attach);
        visualElement.RegisterCallback(detach);

        statusChanged(ReferenceStatus);
    }
}

}
