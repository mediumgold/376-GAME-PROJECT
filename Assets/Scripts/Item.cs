using Unity.VisualScripting;
using UnityEngine;

public class Item : Interactable
{
    public enum ItemType
    {
        snack,
        item,
        drug,


    }
    public ItemType itemType;

    public Player player;
    public float value;
    public int sellValue;
    public int buyPrice;

    [TextArea]
    public string description;

    private Item itemToUse;
    private Inventory inventory;


    void Start()
    {
        player = FindAnyObjectByType<Player>();
        inventory = player.GetComponent<Inventory>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected override void Interact()
    {
        Debug.Log("interacted with " + gameObject.name + "");
        if (gameObject.CompareTag("Money"))
        {
            player.changeMoney(+(int)value);
            Debug.Log("player picked up " + value + "$");
            Destroy(gameObject);
            return;
        }

        // Add to player's inventory (non-money)
        var inventory = player.GetComponent<Inventory>();
        if (inventory == null) return;

        // Disable and reparent to player so we keep the real item to drop later.
        transform.SetParent(player.transform);
        gameObject.SetActive(false);

        // Add or replace at current selection
        Item replaced = inventory.AddOrReplaceAtSelection(this);

        // If we replaced something, drop it in front of the player
        if (replaced != null)
        {
            var dropPos = player.transform.position + player.transform.forward * 1.2f + Vector3.up * 0.5f;
            replaced.transform.SetParent(null);
            replaced.transform.position = dropPos;
            replaced.gameObject.SetActive(true);
        }

    }
    




}
