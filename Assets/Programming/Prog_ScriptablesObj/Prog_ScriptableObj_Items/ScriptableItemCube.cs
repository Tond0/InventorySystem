using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "ScriptableObjects/Inventory/Cube Item")]
public class ScriptableItemCube : ScriptableItem
{
    [SerializeField] private float jumpForce = 10;
    public override void UseItem(Player player)
    {   
        //Null check
        if(player == null) return;

        //Get the rb
        if(!player.TryGetComponent<Rigidbody>(out Rigidbody playerRb)) return;

        //Apply jump force to velocity
        Vector3 playerVel = playerRb.linearVelocity;

        playerVel.x = 0;
        playerVel.y = jumpForce;
        playerVel.z = 0;

        //Apply new velocity
        playerRb.linearVelocity = playerVel;
    }
}