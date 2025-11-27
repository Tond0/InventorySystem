using System;
using UnityEngine;

/// <summary>
/// Base class for all UI views.
/// It is used as a tag class.
/// </summary>
public abstract class View : MonoBehaviour
{    
    [Header("View Settings")]
    //Is this view persistent in the gameplay?
    [SerializeField] private bool isGameplayView;
    public bool IsGameplayView => isGameplayView;
    protected virtual void OnEnable()
    {
        //If the view is enabled, make sure it's closed at the beginning.
        CloseView();    
    }

    // Children must implement these methods defining what to do when the view is opened or closed.
    protected virtual void OpenView() 
    {
        //Activate all children.
        foreach(Transform child in transform)
            child.gameObject.SetActive(true);
    }
    protected virtual void CloseView() 
    {
        //Deactivate all children.
        foreach(Transform child in transform)
            child.gameObject.SetActive(false);
    }
}