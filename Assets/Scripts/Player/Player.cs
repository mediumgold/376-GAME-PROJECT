using UnityEngine;
using System;


public class Player : MonoBehaviour
{
    [Header("Status")]

    public int maxHunger;
    public int maxHigh;

    public float m_currentHunger;
    public float m_currentHigh;

    public Action OnHighChange;
    public Action OnHungerChange;
    public Action OnMoneyChange;

    public int money;

    public int CurrentHunger => Mathf.CeilToInt(m_currentHunger);
    public int CurrentHigh => Mathf.CeilToInt(m_currentHigh);

    private Inventory inventory;


    private void Awake()
    {
        inventory = new Inventory();
    }
    void Start()
    {
        m_currentHigh = maxHigh;
        m_currentHunger = maxHunger;
        OnHighChange?.Invoke();
        OnHungerChange?.Invoke();
        OnMoneyChange?.Invoke();
    }

    // Update is called once per frame
    void Update()
    {


    }

    public void changeHunger(float delta)
    {
        m_currentHunger = Mathf.Clamp(m_currentHunger + delta, 0f, maxHunger);
        OnHungerChange?.Invoke();
    }

    public void changeHigh(float delta)
    {
        m_currentHigh = Mathf.Clamp(m_currentHigh + delta, 0f, maxHigh);
        OnHighChange?.Invoke();
    }
    public void changeMoney(int delta)
    {
        money = money + delta;
        OnMoneyChange?.Invoke();
    }
}

