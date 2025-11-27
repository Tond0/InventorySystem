using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class ViewInventory : View
{
    // Called when an Item changed slot. (On mouse event up)
    public event Action<int, int> OnSlotsSwapped;

    [Header("General")]
    /// The inventory to visualize.
    [SerializeField] private Inventory targetInventory;

    // The raycaster for UI interactions.
    [SerializeField] private GraphicRaycaster canvasRaycaster;
    
    // The last inventory slot interacted with.
    private UIInventorySlot lastInventorySlot = null;

    private Dictionary<UIInventorySlot, int> inventorySlots = new();

    // Data for pointer events.
    PointerEventData pointerData;

    [Header("Input Event Channels")]

    [Space(10)]
    [SerializeField] private ScriptableVoidEventChannel inputMouseClickEventChannel;
    [SerializeField] private ScriptableVoidEventChannel inputMouseUpEventChannel;
    [SerializeField] private ScriptableVector3EventChannel inputMouseDragEventChannel;

    [Space(10)]
    [SerializeField] private ScriptableVoidEventChannel uiViewOpenedEventChannel;
    [SerializeField] private ScriptableVoidEventChannel uiViewClosedEventChannel;

    [Header("UI Components")]
    /// The inventory slot template.
    [SerializeField] private GameObject inventorySlotPrefab;
    /// The top grid layout (inventory).
    [SerializeField] private GridLayoutGroup topGridLayout;
    /// The bottom grid layout (hand).
    [SerializeField] private GridLayoutGroup bottomGridLayout;
    /// The ghost inventory slot (follows the mouse).
    [SerializeField] private UIInventorySlot ghostInventorySlot;
    

    protected override void OnEnable()
    {
        base.OnEnable();

        uiViewOpenedEventChannel.OnEvent += OpenView;
        uiViewClosedEventChannel.OnEvent += CloseView;

        targetInventory.OnInventoryChanged += UpdateUI;

        inputMouseClickEventChannel.OnEvent += OnMouseClicked;
        inputMouseUpEventChannel.OnEvent += OnMouseReleased;
        inputMouseDragEventChannel.OnEvent += OnMouseDragged;
    }

    void OnDisable()
    {
        uiViewOpenedEventChannel.OnEvent -= OpenView;
        uiViewClosedEventChannel.OnEvent -= CloseView;

        targetInventory.OnInventoryChanged -= UpdateUI;

        inputMouseClickEventChannel.OnEvent -= OnMouseClicked;
        inputMouseUpEventChannel.OnEvent -= OnMouseReleased;
        inputMouseDragEventChannel.OnEvent -= OnMouseDragged;
    }
    
    protected override void OpenView()
    {
        // Call base method to handle activation.
        base.OpenView();

        // Build the UI if we didn't do it yet.
        if(topGridLayout.transform.childCount <= 0 || topGridLayout.transform.childCount > targetInventory.InventorySize)
            //Initial UI build.
            BuildUI(targetInventory.InventoryItems);
        else
            // Initial UI update.
            UpdateUI(targetInventory.InventoryItems);
    }

    protected override void CloseView()
    {
        // Call base method to handle deactivation.
        base.CloseView();
    }
    
    void Start()
    {
        //Initial UI build.
        BuildUI(targetInventory.InventoryItems);

        //Initialize pointer data.
        pointerData = new PointerEventData(EventSystem.current);

        //Initialize ghost slot as empty.
        ghostInventorySlot.Initialize(null);
    }

    /// <summary>
    /// Builds the inventory UI based on the current items.
    /// </summary>
    /// <param name="items">The current inventory</param>
    private void BuildUI(IReadOnlyList<InventorySlot> items)
    {  
        //First we clear and populate the bottom grid.
        //It is supposed that the grids have no children at start. But if they do...
        //Let's clear all children starting form the last one, to avoid index shifting (faster).
        for (int i = bottomGridLayout.transform.childCount - 1; i >= 0; i--)
            Destroy(bottomGridLayout.transform.GetChild(i).gameObject);

        //Populate the bottomGrid first
        for(int i = 0; i < targetInventory.HandSize; i++)
        {
            InventorySlot inventorySlot = items[i];

            GameObject instantiatedSlot = Instantiate(inventorySlotPrefab, bottomGridLayout.transform);
            
            if(!instantiatedSlot.TryGetComponent<UIInventorySlot>(out UIInventorySlot uiInventorySlot)) 
            {
                Debug.LogError("Failed to initialize inventory slot UI.");
                return;
            }

            //Initialize the slot UI with the inventory slot data.
            uiInventorySlot.Initialize(inventorySlot);

            inventorySlots.Add(uiInventorySlot, i);
        } 

        //Clear topGrid
        for (int i = topGridLayout.transform.childCount - 1; i >= 0; i--)
            Destroy(topGridLayout.transform.GetChild(i).gameObject);

        //Populate the topGrid
        for(int i = targetInventory.HandSize; i < targetInventory.InventorySize; i++)
        {
            InventorySlot inventorySlot = items[i];

            GameObject instantiatedSlot = Instantiate(inventorySlotPrefab, topGridLayout.transform);
            
            if(!instantiatedSlot.TryGetComponent<UIInventorySlot>(out UIInventorySlot uiInventorySlot)) 
            {
                Debug.LogError("Failed to initialize inventory slot UI.");
                return;
            }

            //Initialize the slot UI with the inventory slot data.
            uiInventorySlot.Initialize(inventorySlot);

            inventorySlots.Add(uiInventorySlot, i);
        }   
    }

    /// <summary>
    /// Updates the inventory UI when the inventory changes.
    /// </summary>
    /// <param name="items"></param>
    private void UpdateUI(IReadOnlyList<InventorySlot> items)
    {
        //Update each slot in the bottomGrid
        for(int i = 0; i < targetInventory.HandSize; i++)
        {
            //Get the inventory slot and the corresponding UI slot.
            InventorySlot inventorySlot = items[i];
            GameObject instantiatedSlot = bottomGridLayout.transform.GetChild(i).gameObject;

            //Get the UIInventorySlot component.
            if(!instantiatedSlot.TryGetComponent<UIInventorySlot>(out UIInventorySlot uiInventorySlot)) 
            {
                Debug.LogError("Failed to initialize inventory slot UI.");
                return;
            }

            //Initialize the slot UI with the inventory slot data.
            uiInventorySlot.Initialize(inventorySlot);
        }

        //Update each slot in the topGrid
        for(int i = targetInventory.HandSize; i < targetInventory.InventorySize; i++)
        {   
            //Check if there are more items than UI slots.
            if(items.Count <= i)
            {
                Debug.LogWarning("More UI slots than inventory items. Consider rebuilding the UI.");
                break;
            }
            
            //Get the inventory slot and the corresponding UI slot.
            InventorySlot inventorySlot = items[i];
            GameObject instantiatedSlot = topGridLayout.transform.GetChild(i - targetInventory.HandSize).gameObject;
            
            //Get the UIInventorySlot component.
            if(!instantiatedSlot.TryGetComponent<UIInventorySlot>(out UIInventorySlot uiInventorySlot)) 
            {
                Debug.LogError("Failed to initialize inventory slot UI.");
                return;
            }

            //Initialize the slot UI with the inventory slot data.
            uiInventorySlot.Initialize(inventorySlot);
        }   
    }

    #region Mouse Events
    private void OnMouseClicked()
    {
        //Check for mouse presence.
        if(Mouse.current == null) return;

        //Get mouse position.
        pointerData.position = Mouse.current.position.ReadValue();

        //Raycast to find UI elements under the mouse.
        List<RaycastResult> results = new List<RaycastResult>();
        canvasRaycaster.Raycast(pointerData, results);

        //If no UI element was hit, we consider it a drop action.
        if(results.Count > 0)
        {
            //Get the topmost hit object.
            GameObject hitObject = results[0].gameObject;

            //Check if it is an inventory slot.
            if(hitObject.TryGetComponent<UIInventorySlot>(out UIInventorySlot uiInventorySlot))
            {
                //Make the ghost slot visible and populated with the data in the slot
                ghostInventorySlot.Initialize(uiInventorySlot.CurrentInventorySlot);
                //Make the slot selected empty
                uiInventorySlot.Initialize(null);
                
                lastInventorySlot = uiInventorySlot;
            }
        }
    }
    private void OnMouseReleased()
    {
        //Check for mouse presence.
        if(Mouse.current == null) return;
        
        //Did we grab something in the first place?
        if(lastInventorySlot == null) return;

        //Get mouse position.
        pointerData.position = Mouse.current.position.ReadValue();

        //Raycast to find UI elements under the mouse.
        List<RaycastResult> results = new List<RaycastResult>();
        canvasRaycaster.Raycast(pointerData, results);

        //If no UI element was hit, we consider it a drop action.
        if(results.Count > 0)
        {
            //Get the topmost hit object.
            GameObject hitObject = results[0].gameObject;

            //Check if it is an inventory slot.
            if(hitObject.TryGetComponent<UIInventorySlot>(out UIInventorySlot uiInventorySlot))
            {
                //FIXME: We just use UpdateUI thanks to the inventory
                //inventorySlotToPopulate = uiInventorySlot;
                // //Swap the index in the dictionary.
                // (inventorySlots[uiInventorySlot], inventorySlots[lastInventorySlot]) = (inventorySlots[lastInventorySlot], inventorySlots[uiInventorySlot]);
                // //Draw the tile we are swapping (if we are not swapping we are just going to redraw the tile)
                // lastInventorySlot.Initialize(uiInventorySlot.CurrentInventorySlot);

                //Swap items in the inventory script as well.   
                OnSlotsSwapped?.Invoke(inventorySlots[uiInventorySlot], inventorySlots[lastInventorySlot]);

                //Empty the ghost slot and make it invisible.
                ghostInventorySlot.Initialize(null);

                //Clear the last slot reference.
                lastInventorySlot = null;
                return;
            }
        }

        //UpdateUI will do the dirty work here
        //inventorySlotToPopulate.Initialize(ghostInventorySlot.CurrentInventorySlot);

        //Reset the inventory slot to the previous slot if we didn't find a slot
        lastInventorySlot.Initialize(ghostInventorySlot.CurrentInventorySlot);

        //Empty the ghost slot and make it invisible.
        ghostInventorySlot.Initialize(null);

        //Clear the last slot reference.
        lastInventorySlot = null;
    }
    private void OnMouseDragged(Vector3 vector)
    {
        //Update ghost slot position to always follow the mouse.
        ghostInventorySlot.transform.position = Mouse.current.position.ReadValue();
    }
    #endregion
}
