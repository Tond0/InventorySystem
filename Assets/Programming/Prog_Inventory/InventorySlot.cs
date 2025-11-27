using System;
using UnityEngine;

/// <summary>
/// Represents a single slot in the inventory.
/// </summary>
[Serializable]
public class InventorySlot
{
    /// <summary>
    /// The item data stored in this slot.
    /// </summary>
    [SerializeField] private ScriptableItem itemData;
    public ScriptableItem ItemData => itemData;

    /// <summary>
    /// The quantity of the item in this slot.
    /// </summary>
    [SerializeField] private int quantity = 0;
    public int Quantity => quantity;

    public InventorySlot(ScriptableItem itemData)
    {
        this.itemData = itemData;

        TryAddQuantity(1);
    }

    /// <summary>
    /// Adds quantity to the item in this slot.
    /// </summary>
    /// <param name="amount"></param>
    public bool TryAddQuantity(int amount)
    {
        //Null check
        if(itemData == null) return false;

        //Check for max stack size.
        int newQuantity = quantity + amount;
        if(newQuantity > itemData.MaxStackSize)
            return false;

        //Add the quantity.
        quantity = newQuantity;
        return true;
    }

    public void RemoveQuantity(int amount)
    {
        int newQuantity = quantity - amount;
        
        if(newQuantity < 0)
            Debug.LogError("The quantity of this item is negative!");

        quantity = newQuantity;
    }

    
    //Called when the item has been consumed
    public event Action OnItemUsed;
    /// <summary>
    /// Use the item in this slot
    /// </summary>
    public void UseItem(Player player)
    {
        if(itemData == null) return;

        itemData.UseItem(player);

        OnItemUsed?.Invoke();
    }
}
