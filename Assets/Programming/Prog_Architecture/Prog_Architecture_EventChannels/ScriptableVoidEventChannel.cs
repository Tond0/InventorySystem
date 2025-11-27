using System;
using UnityEngine;

/// <summary>
/// A Scriptable Event Channel for broadcasting events.
/// This pattern is promoted by Unity for decoupling systems, by doing this we can bind events without the NullExeptionRisk.
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/ Event Channels/ Void Event Channel")]
public class ScriptableVoidEventChannel : ScriptableObject
{
    public event Action OnEvent;

    public void InvokeEvent() => OnEvent?.Invoke();
}
