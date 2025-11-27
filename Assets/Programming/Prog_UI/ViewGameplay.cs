using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

/// <summary>
/// This is the view that will be used during gameplay, showing things like: health, stamina, inventoryHand, ecc...
/// </summary>
public class ViewGameplay : View
{
    [Header("General")]
    //The grid layout display the hand of the player.
    [SerializeField] private GridLayoutGroup handGridLayout;
    //The prefab used for the inventory slot.
    [SerializeField] private GameObject inventorySlotPrefab;

    //The inventory we are targeting
    [SerializeField] private Inventory targetInventory;

    [Header("Event Channels")]
    [SerializeField] private ScriptableVoidEventChannel onViewOpened;
    [SerializeField] private ScriptableVoidEventChannel onViewClosed;
    
    protected override void OnEnable()
    {
        base.OnEnable();

        onViewOpened.OnEvent += OpenView;
        onViewClosed.OnEvent += CloseView;

        // Subscribe to inventory changes.
        targetInventory.OnInventoryChanged += UpdateUI;
        targetInventory.OnSelectedItemChanged += UpdateSelectedItem;
    }

    void OnDisable()
    {
        onViewOpened.OnEvent -= OpenView;
        onViewClosed.OnEvent -= CloseView;

        // Subscribe to inventory changes.
        targetInventory.OnInventoryChanged -= UpdateUI;
        targetInventory.OnSelectedItemChanged -= UpdateSelectedItem;
    }

    protected override void OpenView()
    {
        // Call base method to handle activation.
        base.OpenView();

        // Build the UI if we didn't do it yet.
        if(handGridLayout.transform.childCount < targetInventory.HandSize || handGridLayout.transform.childCount > targetInventory.HandSize)
            //Initial UI build.
            BuildUI(targetInventory.InventoryItems);
        else
            // Initial UI update.
            UpdateUI(targetInventory.InventoryItems);
    }

    /// <summary>
    /// Build the inventory hand.
    /// FIXME: Not proud to be copying the ViewInventory script, would be nice to find a way to not replicate code.
    /// </summary>
    /// <param name="items"></param>
    private void BuildUI(IReadOnlyList<InventorySlot> items)
    {
        //First we clear and populate the bottom grid.
        //It is supposed that the grids have no children at start. But if they do...
        //Let's clear all children starting form the last one, to avoid index shifting (faster).
        for (int i = handGridLayout.transform.childCount - 1; i >= 0; i--)
            Destroy(handGridLayout.transform.GetChild(i).gameObject);

        //Populate the bottomGrid first
        for(int i = 0; i < targetInventory.HandSize; i++)
        {
            InventorySlot inventorySlot = items[i];

            GameObject instantiatedSlot = Instantiate(inventorySlotPrefab, handGridLayout.transform);
            
            if(!instantiatedSlot.TryGetComponent<UIInventorySlot>(out UIInventorySlot uiInventorySlot)) 
            {
                Debug.LogError("Failed to initialize inventory slot UI.");
                return;
            }

            //Initialize the slot UI with the inventory slot data.
            uiInventorySlot.Initialize(inventorySlot);
        } 
    }

    private void UpdateUI(IReadOnlyList<InventorySlot> items)
    {
        //Update each slot in the bottomGrid
        for(int i = 0; i < targetInventory.HandSize; i++)
        {
            //Get the inventory slot and the corresponding UI slot.
            InventorySlot inventorySlot = items[i];
            GameObject instantiatedSlot = handGridLayout.transform.GetChild(i).gameObject;

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

    private void UpdateSelectedItem(int oldIndex, int newIndex)
    {
        Transform oldSelectedItemGameobject = handGridLayout.transform.GetChild(oldIndex);
        Transform newSelectedItemGameobject = handGridLayout.transform.GetChild(newIndex);

        //Null check
        if(oldSelectedItemGameobject == null) return;
        if(newSelectedItemGameobject == null) return;

        //Assure they are both UIInventorySlot.
        if(!oldSelectedItemGameobject.TryGetComponent<UIInventorySlot>(out UIInventorySlot oldSelectedInventorySlot)) return;
        if(!newSelectedItemGameobject.TryGetComponent<UIInventorySlot>(out UIInventorySlot newSelectedInventorySlot)) return;

        //Assire they both have the outline component
        if(!oldSelectedItemGameobject.TryGetComponent<Outline>(out Outline oldSelectedOutlineComponent)) return;
        if(!newSelectedItemGameobject.TryGetComponent<Outline>(out Outline newSelectedOutlineComponent)) return;

        //Change selected outline component
        oldSelectedOutlineComponent.enabled = false;
        newSelectedOutlineComponent.enabled = true;
    }
}
