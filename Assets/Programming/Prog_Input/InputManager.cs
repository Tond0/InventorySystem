using System;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour, Controls.IGameplayActions, Controls.IUIActions
{
    //FIXME: I tried to make it singleton but I realized it's just a poor implementation.
    //Singleton instance
    //public static InputManager Current { get; private set; }


    private Controls controls;

    //FIXME: Deprecated properties, I moved to a scriptable event channel pattern.
    // /// <summary>
    // /// Indicates whether gameplay input is currently enabled.
    // /// </summary>
    // public bool IsGameplayInputEnabled => controls.Gameplay.enabled;
    // //FIXME: I know 1 bool could be enough, because if one is enabled the other is disabled. But for clarity...
    // /// <summary>
    // /// Indicates whether UI input is currently enabled.
    // /// </summary>
    // public bool IsUIInputEnabled => controls.UI.enabled;

    // public event Action<Vector2> OnMovePerformed;
    // public event Action<Vector2> OnLookPerformed;
    // public event Action OnInventoryPerformed;

    [Header("Cinemachine")]
    // The cinemachine input axis controller that handles camera movement.
    [SerializeField] private CinemachineInputAxisController cinemachineInputAxisController;

    [Header("Move and Look Event Channels")]
    [SerializeField] private ScriptableVector3EventChannel inputMoveEventChannel;
    [SerializeField] private ScriptableVector3EventChannel inputLookEventChannel;

    [Header("Inventory Event Channels")]
    [SerializeField] private ScriptableVoidEventChannel inputOpenInventoryEventChannel;
    [SerializeField] private ScriptableVoidEventChannel inputCloseInventoryEventChannel;

    [Header("Interact Item Event Channels")]
    [SerializeField] private ScriptableVoidEventChannel inputUseItemEventChannel;
    
    [Header("Select Item Event Channels")]
    [SerializeField] private ScriptableVoidEventChannel inputSelectItem1EventChannel;
    [SerializeField] private ScriptableVoidEventChannel inputSelectItem2EventChannel;
    [SerializeField] private ScriptableVoidEventChannel inputSelectItem3EventChannel;
    [SerializeField] private ScriptableVoidEventChannel inputSelectItem4EventChannel;
    [SerializeField] private ScriptableVoidEventChannel inputSelectItem5EventChannel;
    [SerializeField] private ScriptableVoidEventChannel inputSelectItem6EventChannel;
    [SerializeField] private ScriptableVoidEventChannel inputSelectItem7EventChannel;
    [SerializeField] private ScriptableVoidEventChannel inputSelectItem8EventChannel;
    [SerializeField] private ScriptableVoidEventChannel inputSelectItem9EventChannel;

    [Header("UI Event Channels")]
    [SerializeField] private ScriptableVoidEventChannel uiOpenedEventChannel;
    [SerializeField] private ScriptableVoidEventChannel uiClosedEventChannel;

    [Header("Mouse Event Channels")]
    [SerializeField] private ScriptableVoidEventChannel inputMouseClickEventChannel;
    [SerializeField] private ScriptableVoidEventChannel inputMouseUpEventChannel;
    [SerializeField] private ScriptableVector3EventChannel inputMouseDragEventChannel;

    private void Start()
    {
        //Start with gameplay input enabled
        EnableGameplayInput();
    }

    void OnEnable()
    {
        //Subscribe to UI events
        uiOpenedEventChannel.OnEvent += EnableUIInput;
        uiClosedEventChannel.OnEvent += EnableGameplayInput;

        //Initialize Controls
        controls = new Controls();
        controls.Enable();

        //Set this class as callbacks for input actions
        controls.Gameplay.SetCallbacks(this);
        controls.UI.SetCallbacks(this);

        controls.Gameplay.Move.performed += OnMove;
        controls.Gameplay.Move.canceled += OnMove;

        controls.Gameplay.Look.performed += OnLook;
        controls.Gameplay.OpenInventory.performed += OnOpenInventory;
    }

    void OnDisable()
    {
        //Unsubscribe from UI events
        uiOpenedEventChannel.OnEvent -= EnableUIInput;
        uiClosedEventChannel.OnEvent -= EnableGameplayInput;

        //Disable
        controls.Disable();
        //Disable all input maps
        controls.Gameplay.Disable();
        controls.UI.Disable();

        controls.Gameplay.Move.performed -= OnMove;
        controls.Gameplay.Move.canceled -= OnMove;

        controls.Gameplay.Look.performed -= OnLook;
        controls.Gameplay.OpenInventory.performed -= OnOpenInventory;
    }

    #region Input Mode
    /// <summary>
    /// Enables gameplay input and disables UI input.
    /// </summary>
    private void EnableGameplayInput()
    {
        //Enable Gameplay map and disable UI map
        controls.Gameplay.Enable();
        controls.UI.Disable();

        //Disable camera movement
        SetCameraInputActive(true);

        //Camera Setup
        //Lock the cursor to the center of the screen
        Cursor.lockState = CursorLockMode.Locked;
        //Hide the cursor
        Cursor.visible = false;
    }

    /// <summary>
    /// Enables UI input and disables gameplay input.
    /// </summary>
    private void EnableUIInput()
    {
        //Enable UI map and disable Gameplay map
        controls.UI.Enable();
        controls.Gameplay.Disable();

        //Disable camera movement
        SetCameraInputActive(false);

        //Camera Setup
        //Unlock the cursor
        Cursor.lockState = CursorLockMode.None;
        //Show the cursor
        Cursor.visible = true;
    }
    #endregion

    /// <summary>
    /// Sets whether the camera input is active.
    /// </summary>
    /// <param name="isActive"></param>
    private void SetCameraInputActive(bool isActive)
    {
        if(!cinemachineInputAxisController)
        {
            Debug.LogError("Cinemachine Input Axis Controller reference is missing in InputManager.");
            return;
        }

        cinemachineInputAxisController.enabled = isActive;
    }

    #region Player Map Actions
    /// <summary>
    /// Called when the Move action is performed.
    /// </summary>
    /// <param name="context"></param>
    public void OnMove(InputAction.CallbackContext context)
    {
        inputMoveEventChannel.InvokeEvent(context.ReadValue<Vector2>());
        //Debug.Log(context.ReadValue<Vector3>());
    }

    /// <summary>
    /// Called when the Look action is performed.
    /// </summary>
    /// <param name="context"></param>
    public void OnLook(InputAction.CallbackContext context)
    { 
        inputLookEventChannel.InvokeEvent(context.ReadValue<Vector2>()); 
    }

    /// <summary>
    /// Called when the OpenInventory action is performed.
    /// </summary>
    /// <param name="context"></param>
    public void OnOpenInventory(InputAction.CallbackContext context)
    {
        if(!context.performed) return;
        
        inputOpenInventoryEventChannel.InvokeEvent();
    }

    public void OnCloseInventory(InputAction.CallbackContext context)
    {
        if(!context.performed) return;
        
        inputCloseInventoryEventChannel.InvokeEvent();
    }

    public void OnUseItem(InputAction.CallbackContext context)
    {
        if(!context.performed) return;

        inputUseItemEventChannel.InvokeEvent();
    }

    #region Select Item Actions
    //All the possible inputs to select an item in the hand menu.
    public void OnSelectItem1(InputAction.CallbackContext context) => inputSelectItem1EventChannel.InvokeEvent();
    public void OnSelectItem2(InputAction.CallbackContext context) => inputSelectItem2EventChannel.InvokeEvent();
    public void OnSelectItem3(InputAction.CallbackContext context) => inputSelectItem3EventChannel.InvokeEvent();
    public void OnSelectItem4(InputAction.CallbackContext context) => inputSelectItem4EventChannel.InvokeEvent();
    public void OnSelectItem5(InputAction.CallbackContext context) => inputSelectItem5EventChannel.InvokeEvent();
    public void OnSelectItem6(InputAction.CallbackContext context) => inputSelectItem6EventChannel.InvokeEvent();
    public void OnSelectItem7(InputAction.CallbackContext context) => inputSelectItem7EventChannel.InvokeEvent();
    public void OnSelectItem8(InputAction.CallbackContext context) => inputSelectItem8EventChannel.InvokeEvent();
    public void OnSelectItem9(InputAction.CallbackContext context) => inputSelectItem9EventChannel.InvokeEvent();
    #endregion
    #endregion

    #region UI Map Actions
    public void OnMouseClicked(InputAction.CallbackContext context)
    {
        if(context.performed)
            inputMouseClickEventChannel.InvokeEvent();
        else if(context.canceled)
            inputMouseUpEventChannel.InvokeEvent();
    }

    public void OnMouseDrag(InputAction.CallbackContext context)
    {
        inputMouseDragEventChannel.InvokeEvent(context.ReadValue<Vector2>());
    }
    #endregion
}
