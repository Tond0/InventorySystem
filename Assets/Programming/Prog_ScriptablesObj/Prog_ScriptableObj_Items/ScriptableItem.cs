using System;
using Mono.Cecil.Cil;
using UnityEngine;

public abstract class ScriptableItem : ScriptableObject
{
    /// <summary>
    /// The name of the item.
    /// </summary>
    [SerializeField] private string itemName;
    public string ItemName => itemName; 

    /// <summary>
    /// The description of the item.
    /// </summary>
    [SerializeField] private string itemDescription;
    public string ItemDescription => itemDescription;

    /// <summary>
    /// The item max stack size
    /// </summary>
    [SerializeField] private int maxStackSize = 64;
    public int MaxStackSize => maxStackSize;

    //FIXME: Let's use sprites for now.
    /// <summary>
    /// The icon representing the item.
    /// </summary>
    [SerializeField] private Sprite itemIcon;
    public Sprite ItemIcon => itemIcon;

    //This is the generic method where we should write the behaviour of this item when used.
    public abstract void UseItem(Player player);
}
