using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Inventory playerInventory;

    [Header("Event Channels")]
    [SerializeField] private ScriptableVector3EventChannel inputMovePerformedEventChannel;
    [SerializeField] private ScriptableVector3EventChannel inputLookPerformedEventChannel;
    [SerializeField] private ScriptableVoidEventChannel inputUseItemEventChannel;
    
    [Header("Movement")]
    /// <summary>
    /// The Rigidbody component for physics-based movement.
    /// </summary>
    [SerializeField] private Rigidbody rb;
    /// <summary>
    /// The movement component handling player movement logic.
    /// </summary>
    [SerializeField] private MovementComponent movementComponent;
    public MovementComponent MovementComponent => movementComponent;
    
    /// <summary>
    /// The input direction from the player.
    /// </summary>
    //FIXME: I think this should be in the movement component.
    private Vector2 inputDirection = Vector2.zero;

    void OnEnable()
    {
        inputMovePerformedEventChannel.OnEvent += OnMove;
        inputLookPerformedEventChannel.OnEvent += OnLook;

        inputUseItemEventChannel.OnEvent += OnUseItem;
    }

    void OnDisable()
    {
        inputMovePerformedEventChannel.OnEvent -= OnMove;
        inputLookPerformedEventChannel.OnEvent -= OnLook;

        inputUseItemEventChannel.OnEvent -= OnUseItem;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        //Get the movement direction relative to the camera
        Vector3 movementDirection = movementComponent.GetCameraRelativeDirection(inputDirection, Camera.main.transform);

        //Call the movement function
        movementComponent.Move(transform, rb, movementDirection);
    }

    private void Look()
    {
        Vector3 cameraForward = Camera.main.transform.forward;
        cameraForward.y = 0; // Keep the rotation on the horizontal plane
        cameraForward.Normalize();

        Quaternion targetRotation = Quaternion.LookRotation(cameraForward);
        transform.rotation = targetRotation;
    }

    public void OnMove(Vector3 newInputDirection) => inputDirection = new Vector2(newInputDirection.x, newInputDirection.y);

    /// <summary>
    /// Called when the Look action is performed.
    /// We don't actually care about the vector value because we turn the camera natively with cinemachine.
    /// </summary>
    /// <param name="vector"></param>
    private void OnLook(Vector3 vector)
    {
        Look();
    }

    /// <summary>
    /// Method to get from the inventory the item and use it.
    /// </summary>
    private void OnUseItem()
    {
        //The current selected slot is empty
        if(playerInventory.SelectedItem == null || playerInventory.SelectedItem.ItemData == null) return;

        //Use the current selected item
        playerInventory.SelectedItem.UseItem(this);
    }
}
