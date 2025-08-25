using UnityEngine;

public class ItemsPickable : MonoBehaviour, IPickable
{
    public KeyItemsSO itemScriptableObject;

    public void PickItem()
    {
        Destroy(gameObject);
    }
}