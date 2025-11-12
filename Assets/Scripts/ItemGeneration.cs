using UnityEngine;

public class ItemGeneration : MonoBehaviour
{
    public GameObject[] items;
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
            Vector3 tempPosition = new Vector3(house.transform.position.x - 7, house.transform.position.y - 4.5f, house.transform.position.z);
            int itemToUse = Random.Range(0, items.Length);
            GameObject item = Instantiate(items[itemToUse], tempPosition, Quaternion.identity);
        }
    }
}
