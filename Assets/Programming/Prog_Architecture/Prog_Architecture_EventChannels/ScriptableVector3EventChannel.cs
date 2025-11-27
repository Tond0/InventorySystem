using System;
using UnityEngine;

/// <summary>
/// A Scriptable Event Channel for broadcasting events.
/// This pattern is promoted by Unity for decoupling systems, by doing this we can bind events without the NullExeptionRisk.
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/ Event Channels/ Vector3 Event Channel")]
public class ScriptableVector3EventChannel : ScriptableObject
{
    public event Action<Vector3> OnEvent;

    public void InvokeEvent(Vector3 vector) => OnEvent?.Invoke(vector);
}

