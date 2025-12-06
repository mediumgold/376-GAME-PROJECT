using UnityEngine;

[System.Serializable]
public class WeightedItem
{
    public GameObject item;
    [Range(1, 100)] public int weight = 10; // Higher = more common
}

public class ItemGeneration : MonoBehaviour
{
    [Header("Items with spawn weights (higher = more common)")]
    public WeightedItem[] items;
    
    public GameObject[] houses;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PopulateHouses();
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    public void PopulateHouses()
    {
        foreach(GameObject house in houses)
        {
            Vector3 tempPosition = new Vector3(house.transform.position.x, house.transform.position.y + 5, house.transform.position.z);
            GameObject itemToSpawn = GetWeightedRandomItem();
            if (itemToSpawn != null)
                Instantiate(itemToSpawn, tempPosition, Quaternion.identity);
        }
    }

    private GameObject GetWeightedRandomItem()
    {
        if (items == null || items.Length == 0)
            return null;

        int totalWeight = 0;
        foreach (var wi in items)
            totalWeight += wi.weight;

        int roll = Random.Range(0, totalWeight);
        int cumulative = 0;

        foreach (var wi in items)
        {
            cumulative += wi.weight;
            if (roll < cumulative)
                return wi.item;
        }

        return items[0].item; // Fallback
    }
}
