using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Item : MonoBehaviour
{
    private enum EItemType
    {
        CONSUMABLE,
        DURABLE
    }

    [SerializeField] private string itemID;
    [SerializeField] private EItemType type;
    [SerializeField] private int quantity = 1;

    private int initialQuantity;

    public string ItemID { get { return itemID; } }
    public int Quantity { get { return quantity; } }

    private void OnValidate()
    {
        if (type == EItemType.DURABLE)
        {
            quantity = 1;
        }
    }

    private void Awake()
    {
        initialQuantity = quantity;    
    }

    public bool ConsumeItem()
    {
        if (type == EItemType.CONSUMABLE)
        {
            if (quantity > 0)
            {
                quantity -= 1;
                return true;
            }
            else
            {
                Debug.LogWarning("Cannot consume item. Quantity is zero.");
                return false;
            }
        }
        else
        {
            Debug.LogWarning("Cannot consume durable item.");
            return false;
        }
    }

    public bool AddItem(int quantity)
    {
        if (type == EItemType.CONSUMABLE)
        {
            this.quantity += quantity;
            return true;
        }
        else
        {
            Debug.LogWarning("Cannot add quantity to durable item.");
            return false;
        }
    }

    public void ResetItem()
    {
        gameObject.SetActive(true);
        quantity = initialQuantity;
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerController tPlayer = other.GetComponent<PlayerController>();
        if (tPlayer)
        {
            InventoryManager.Instance.AddItem(this);
            gameObject.SetActive(false);
        }
    }
}