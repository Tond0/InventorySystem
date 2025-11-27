using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

public class Item : MonoBehaviour
{
    /// <summary>
    /// The data of the item.
    /// </summary>
    [SerializeField] private ScriptableItem itemData;
    public ScriptableItem ItemData => itemData;

    void Start()
    {
        //For better clarity
        gameObject.name = itemData.name;

        //Get generic collider.
        if(!TryGetComponent<Collider>(out Collider collider))
        {
            Debug.Log("Item: No collider found on item " + gameObject.name);
            return;
        }

        //Assure the collider is set as a trigger.
        collider.isTrigger = true;

        // Assure the GameObject is on the correct layer.
        gameObject.layer = LayerMask.NameToLayer("Collectible"); 

        //Tween animations
        transform.DORotate(new Vector3(0f, 360f, 0f), 3f, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);

        transform.DOMoveY(transform.position.y + 0.25f, 1f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }
}
