using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// Represents the player's inventory.
/// In charge to detect, collect and store items.
/// </summary>
public class Inventory : MonoBehaviour
{
    /// Event triggered when the inventory changes.
    public event Action<IReadOnlyList<InventorySlot>> OnInventoryChanged;

    // Event triggered when the selected item change, passing the old and the new value. (first one is the old value)
    public event Action<int, int> OnSelectedItemChanged;

    [Header("Size")]
    /// The maximum size of the inventory.
    [SerializeField] private int inventorySize = 20;
    public int InventorySize => inventorySize;

    // The size of that the player can equip
    [SerializeField, Range(2,9)] private int handSize = 5;
    public int HandSize => handSize;
    

    [Header("Animation")]
    /// Duration of the item lerp animation.
    [SerializeField] private float itemLerpSpeed = 0.5f;
    /// Minimum distance to consider the item collected.
    [SerializeField, Range(0.1f,10)] private float collectingDistance = 1;
    
    [Header("UI")]
    [SerializeField] private ViewInventory inventoryView;

    //The event channel to handle choosing which item to use.
    [Header("Event Channel")]
    [SerializeField] private ScriptableVoidEventChannel inputHand1;
    [SerializeField] private ScriptableVoidEventChannel inputHand2;
    [SerializeField] private ScriptableVoidEventChannel inputHand3;
    [SerializeField] private ScriptableVoidEventChannel inputHand4;
    [SerializeField] private ScriptableVoidEventChannel inputHand5;
    [SerializeField] private ScriptableVoidEventChannel inputHand6;
    [SerializeField] private ScriptableVoidEventChannel inputHand7;
    [SerializeField] private ScriptableVoidEventChannel inputHand8;
    [SerializeField] private ScriptableVoidEventChannel inputHand9;

    [Header("Debug")]
    /// The items currently in the inventory.
    [SerializeField] private List<InventorySlot> inventoryItems;

    /// Read-only access to the items in the inventory.
    public IReadOnlyList<InventorySlot> InventoryItems => inventoryItems;

    //The currently selected item from the hand
    [SerializeField] private int selectedIndex;
    public InventorySlot SelectedItem => inventoryItems[selectedIndex];


    void OnEnable()
    {
        //Subscribe to know when the ViewInventory is trying to swap items.
        inventoryView.OnSlotsSwapped += Swap;

        //Subscribe to the channel to select the item
        inputHand1.OnEvent += () => SelectItem(0);
        inputHand2.OnEvent += () => SelectItem(1);
        inputHand3.OnEvent += () => SelectItem(2);
        inputHand4.OnEvent += () => SelectItem(3);
        inputHand5.OnEvent += () => SelectItem(4);
        inputHand6.OnEvent += () => SelectItem(5);
        inputHand7.OnEvent += () => SelectItem(6);
        inputHand8.OnEvent += () => SelectItem(7);
        inputHand9.OnEvent += () => SelectItem(8);
    }

    void OnDisable()
    {
        inventoryView.OnSlotsSwapped -= Swap;
    }

    /// <summary>
    /// Method to swap items used by the inventoryView.
    /// </summary>
    /// <param name="index1"></param>
    /// <param name="index2"></param>
    private void Swap(int index1, int index2)
    {
        //Swap items
        (inventoryItems[index1], inventoryItems[index2]) = (inventoryItems[index2], inventoryItems[index1]);

        OnInventoryChanged?.Invoke(inventoryItems);
    }

    void Start()
    {
        //Initialize the inventory with empty slots.
        inventoryItems = new List<InventorySlot>(inventorySize);

        for(int i = 0; i < inventorySize; i++)
            inventoryItems.Add(null);

        //Start by selecting the first item.
        SelectItem(0);
    }

    /// <summary>
    /// Called when another collider enters this inventory's trigger.
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerEnter(Collider other)
    {
        //Check if the collided object is an item.
        if(!other.gameObject.TryGetComponent<Item>(out Item item)) return;

        //Is there space in the inventory?
        if(!HasInventorySpaceAvailable(item)) return;

        //FIXME: Deprecated animation code because the target position is not static.
        //Lerp the item to the inventory position.
        // item.transform.DOLocalMove(transform.position, itemLerpDuration).SetEase(itemLerpEase).onComplete += () => 
        // {
        //     //Can we add the item to the inventory?
        //     if(!TryAddItem(item)) return;

        //     //Parent the item to the inventory pool.
        //     item.transform.SetParent(transform);
        //     //Disable the item game object.
        //     item.gameObject.SetActive(false);
        // };
        
        //New method using coroutine to lerp the item to the player.
        StartCoroutine(LerpToPlayer(item));
    }

    //FIXME: Would be nice if it was static and more generic.
    /// <summary>
    /// Lerp the item to the player position.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    private IEnumerator LerpToPlayer(Item item)
    {   
        //FIXME: This is done because the items are tweening their position on the vertical axis. So the next while loop would never end.
        //Pause any existing tweens on the item.
        item.transform.DOPause();

        //Lerp until close enough.
        //Squared magnitude used to avoid sqrt calculation, which are cpu heavy.
        while(true)
        {
            if((item.transform.position - transform.position).sqrMagnitude <= collectingDistance * collectingDistance)
                break;

            //This is not the currect way to lerp something, but it's good enough to create an animation whith a target position that is constantly moving.
            item.transform.position = Vector3.MoveTowards(item.transform.position, transform.position, itemLerpSpeed * Time.deltaTime);
            yield return null;
        }

        //Can we add the item to the inventory?
        if(!TryAddItem(item)) yield break;

        //Parent the item to the inventory pool.
        item.transform.SetParent(transform);
        
        //FIXME: Would be nice to add a pool system
        //Disable the item game object.
        //item.gameObject.SetActive(false);
        
        //Destroy the item
        Destroy(item.gameObject);
    }

    /// <summary>
    /// Try to add the item to the inventory.
    /// </summary>
    /// <param name="item">Item to add</param>
    private bool TryAddItem(Item item)
    {   
        //First, try to find a stack avaiable for this item.
        if(FindStackableItem(item, out int stackableIndex))
        {
            //Stack the item.
            inventoryItems[stackableIndex].TryAddQuantity(1); //FIXME: Now we just add 1 to the stack. This should be changed to consider the actual amount picked up.
            
            Debug.Log("Inventory: Stacked item - " + item.name);
            
            //Notify listeners about the inventory change.
            OnInventoryChanged?.Invoke(inventoryItems);

            return true;
        }   

        //FIXME: This is ugly fixme please
        //If no stack was found, we check for the first empty slot.
        //Try to find the first empty slot.
        int firstNullIndex = inventoryItems.FindIndex(slot => slot == null);
        
        //Didn't find any null slot?
        if(firstNullIndex < 0)
            firstNullIndex = inventoryItems.FindIndex(slot => slot.ItemData == null);

        //If there's an empty slot, add the item there.
        if(firstNullIndex >= 0)
        {
            //The new itemSlot created for the item
            InventorySlot newInventorySlot = new InventorySlot(item.ItemData);

            //Add the item to the first empty slot.
            inventoryItems[firstNullIndex] = newInventorySlot;

            //Bind to event when the item is used
            newInventorySlot.OnItemUsed += () => OnItemUsed(newInventorySlot);

            Debug.Log("Inventory: Added item - " + item.name);

            //Notify listeners about the inventory change.
            OnInventoryChanged?.Invoke(inventoryItems);  

            return true;
        }
        

        //No space found.
        Debug.Log("Inventory: Item not added - " + item.name);

        return false;
    }

    private bool TryRemoveItem(InventorySlot inventorySlotToRemove)
    {
        int indexToClean = inventoryItems.IndexOf(inventorySlotToRemove);

        //Not found?
        if (indexToClean < 0)
            return false;

        //Clean this index
        inventoryItems[indexToClean] = new InventorySlot(null);

        return true;
    }

    /// <summary>
    /// Method called when an item has been used.
    /// </summary>
    /// <param name="itemUsedIndex"></param>
    private void OnItemUsed(InventorySlot usedInventorySlot)
    {
        if(usedInventorySlot == null || usedInventorySlot.ItemData == null) return;

        usedInventorySlot.RemoveQuantity(1);

        //If the quantity is 0 or less we don't have the item anymore
        if(usedInventorySlot.Quantity <= 0)
            TryRemoveItem(usedInventorySlot);

        OnInventoryChanged?.Invoke(inventoryItems);
    }

    /// <summary>
    /// Try to find a stackable item in the inventory.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    private bool FindStackableItem(Item item, out int stackableIndex)
    {
        //Try to stack the item first.
        for(stackableIndex = 0; stackableIndex < inventoryItems.Count; stackableIndex++)
        {
            //Get the current inventory slot.
            InventorySlot currentInventorySlot = inventoryItems[stackableIndex];

            //If the slot is empty, continue.
            if(currentInventorySlot == null || currentInventorySlot.ItemData == null) continue;

            //FIXME: Currently stacking only works by item name. This should be changed to a unique ID or similar.
            if(currentInventorySlot.ItemData.name == item.ItemData.name)
            {
                //Can we stack more of this item?
                //FIXME: Now we just add 1 to the stack. This should be changed to consider the actual amount picked up.
                if(currentInventorySlot.Quantity + 1 > currentInventorySlot.ItemData.MaxStackSize) continue;

                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if there's space available in the inventory.
    /// </summary>
    /// <returns>True if there's space in the inventory avaiable</returns>
    private bool HasInventorySpaceAvailable(Item item)
    {
        //If there's even a null slot, there's space.
        if(inventoryItems.Any(slot => slot == null)) return true;
        //We need to check if the itemData is null because as long as we serialize the list to have a visual indicator of the inventory,
        //Unity will create a slot for each item.
        if(inventoryItems.Any(slot => slot.ItemData == null)) return true;

        //If there's a stackable item, there's space.
        if(FindStackableItem(item, out int stackableIndex)) return true;

        //There's no space.
        return false;
    }

    /// <summary>
    /// Select a new item based on the index
    /// </summary>
    /// <param name="index"></param>
    private void SelectItem(int index)
    {
        //We can't select the items that are not shown in the hand.
        if(index >= handSize) return;

        //Notify the UI
        OnSelectedItemChanged?.Invoke(selectedIndex, index);

        //Select the item.
        selectedIndex = index;
    }
}
