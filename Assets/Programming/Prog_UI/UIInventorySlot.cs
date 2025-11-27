using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIInventorySlot : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image slotImage;
    public Image SlotImage => slotImage;
    [SerializeField] private TextMeshProUGUI slotQuantityText;
    public TextMeshProUGUI SlotQuantityText => slotQuantityText;


    //The inventory slot this UISlot is holding
    private InventorySlot currentInventorySlot;
    public InventorySlot CurrentInventorySlot => currentInventorySlot;

    public void Initialize(InventorySlot inventorySlot)
    {
        //If the slot is empty.
        if(inventorySlot == null || inventorySlot.ItemData == null) 
        {
            //Clear the current inventory slot that this slot is holding.
            currentInventorySlot = null;

            //Clear the slot UI.
            slotImage.sprite = null;
            //Make the image transparent.
            slotImage.color = new Color(1,1,1,0);
            //Clear the quantity text.
            slotQuantityText.text = string.Empty;

            return;
        }

        //The inventory slot we are holding
        currentInventorySlot = inventorySlot;

        //Set the slot image.
        slotImage.sprite = inventorySlot.ItemData.ItemIcon;
        slotImage.color = new Color(1,1,1,1); //Make visible

        //Set the quantity text.
        if(inventorySlot.Quantity > 1)
            slotQuantityText.text = inventorySlot.Quantity.ToString();
        else
            slotQuantityText.text = string.Empty;
    }

    public void Initialize(Image inImage, TextMeshProUGUI inQuantityText)
    {
        //If no slot image is provided we clear the UI.
        if(slotImage == null || slotImage.sprite == null) 
        {
            //Clear the slot UI.
            slotImage.sprite = null;
            //Make the image transparent.
            slotImage.color = new Color(1,1,1,0);
            //Clear the quantity text.
            slotQuantityText.text = string.Empty;

            return;
        }

        //Set the slot image.
        slotImage.sprite = inImage.sprite;
        slotImage.color = new Color(1,1,1,1); //Make visible

        //Set the quantity text
        slotQuantityText.text = inQuantityText.text;
    }
}
