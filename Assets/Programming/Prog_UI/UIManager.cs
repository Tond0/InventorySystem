using System;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class UIManager : MonoBehaviour
{   
    //FIXME: Deprecated singleton pattern, I moved to a scriptable event channel pattern.
    // //Singleton instance    
    // public static UIManager Current { get; private set; }

    // /// <summary>
    // /// Event triggered when the UI is opened.
    // /// </summary>
    // public event Action OnUIOpened;

    // /// <summary>
    // /// Event triggered when the UI is closed.
    // /// </summary>
    // public event Action OnUIClosed;

    [Serializable]
    public class ViewHandler
    {
        //FIXME: To Add.
        //[SerializeField] private ViewGame gameView;
        [SerializeField] private View view;
        public View View => view;

        /// <summary>
        /// Event triggered when the inventory view is opened.
        /// </summary>
        [SerializeField] private ScriptableVoidEventChannel onViewOpened;
        
        /// <summary>
        /// Event triggered when the inventory view is closed.
        /// </summary>
        [SerializeField] private ScriptableVoidEventChannel onViewClosed;

        public void Open() => onViewOpened.InvokeEvent();
        public void Close() => onViewClosed.InvokeEvent();
    }
    
    [Header("Events")]
    [SerializeField] private ScriptableVoidEventChannel inputOpenInventoryEventChannel;
    [SerializeField] private ScriptableVoidEventChannel inputCloseInventoryEventChannel;

    [Space(5)]
    [SerializeField] private ScriptableVoidEventChannel uiOpenedEventChannel;
    [SerializeField] private ScriptableVoidEventChannel uiClosedEventChannel;

    [Header("UI Views")]

    /// The inventory view.
    [SerializeField] private ViewHandler inventoryViewHandler;
    public ViewHandler InventoryView => inventoryViewHandler;

    [SerializeField] private ViewHandler gameplayViewHandler;
    public ViewHandler GameplayViewHandler => gameplayViewHandler;
    

    /// The current view handler active on the screen.
    private ViewHandler currentViewHandler;


    void OnEnable()
    {
        //FIXME: New method promoted by Unity 6, but using strings is not safe at all.
        //InputSystem.actions.FindAction("OpenInventory").performed += OnToggleInventory;

        inputOpenInventoryEventChannel.OnEvent += OnToggleInventory;
        inputCloseInventoryEventChannel.OnEvent += OnToggleInventory;
    }

    void OnDisable()
    {
        //FIXME: New method promoted by Unity 6, but using strings is not safe at all.
        //InputSystem.actions.FindAction("OpenInventory").performed -= OnToggleInventory;

        inputOpenInventoryEventChannel.OnEvent -= OnToggleInventory;
        inputCloseInventoryEventChannel.OnEvent -= OnToggleInventory;
    }

    void Start()
    {
        //Start with no UI view opened.
        SwitchView(gameplayViewHandler);
    }

    /// <summary>
    /// Called when the inventory toggle input is performed.
    /// </summary>
    private void OnToggleInventory()
    {
        //If the inventory view is not the current view, open it.
        if(currentViewHandler != inventoryViewHandler)
            SwitchView(inventoryViewHandler);
        //Else, close the current view.
        else
            //FIXME: For the sake of the project I'll not implement a full stack system for views. Or maybe yes if I have spare time.
            SwitchView(gameplayViewHandler);
    }

    /// <summary>
    /// Switches the current view to the specified view.
    /// If an empty struct is provided, disable the current view and switch to gameplay input.
    /// </summary>
    /// <param name="viewToDisplay"></param>
    private void SwitchView(ViewHandler viewToDisplay)
    {
        if(viewToDisplay == null)
        {
            //If there is no current view, do nothing.
            if(currentViewHandler == null) return;

            //Notify listeners that a UI has been closed.
            uiClosedEventChannel.InvokeEvent();

            //Set current view.
            currentViewHandler = null;

            currentViewHandler.Open();
        }
        else if(viewToDisplay.View.IsGameplayView)
        {
            if(currentViewHandler != null && !currentViewHandler.View.IsGameplayView)
                //Notify listeners that a UI has been closed.
                uiClosedEventChannel.InvokeEvent();

            if(currentViewHandler != null)
                //Close current view.
                currentViewHandler.Close();
                
            currentViewHandler = viewToDisplay;
            currentViewHandler.Open();
        }
        else
        {
            //Check if there is a current view.
            bool isViewNull = currentViewHandler == null;

            //Switch views.
            if(!isViewNull)
                currentViewHandler.Close();

            //If no view is currently displayed, notify listeners that a UI has been opened.
            if(isViewNull || currentViewHandler.View.IsGameplayView)
                //Notify listeners that a UI has been opened.
                uiOpenedEventChannel.InvokeEvent();

            currentViewHandler = viewToDisplay;
            currentViewHandler.Open();
        }
    }
}
