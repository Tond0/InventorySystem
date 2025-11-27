using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "ScriptableObjects/Inventory/Sphere Item")]
public class ScriptableItemSphere : ScriptableItem
{
    [SerializeField] private float sprintForce = 10;
    public override void UseItem(Player player)
    {
        //Null check
        if(player == null) return;

        //Get the rb
        if(!player.TryGetComponent<Rigidbody>(out Rigidbody playerRb)) return;

        //Apply jump force to velocity
        Vector3 playerVel = playerRb.linearVelocity;

        Vector3 relativeDirection = player.transform.forward * sprintForce;

        playerVel.x = relativeDirection.x;
        playerVel.z = relativeDirection.z;

        //Apply new velocity
        playerRb.linearVelocity = playerVel;
    }
}