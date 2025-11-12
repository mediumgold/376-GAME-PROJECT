using UnityEngine;
using System.Collections;

public class HighTicker : MonoBehaviour
{
    public Player player;
    
    public float secondsPerTick = 5f;
    public int HighPerTick = 1;

    private Coroutine tickRoutine;

    void OnEnable()
    {
        if (player == null)
            player = FindFirstObjectByType<Player>();

        if (tickRoutine == null && player != null)
            tickRoutine = StartCoroutine(Tick());
    }

    void OnDisable()
    {
        if (tickRoutine != null)
        {
            StopCoroutine(tickRoutine);
            tickRoutine = null;
        }
    }

    private IEnumerator Tick()
    {
        // Use realtime so it isn't affected by Time.timeScale (e.g., pause/slow-mo)
        WaitForSecondsRealtime wait = new WaitForSecondsRealtime(secondsPerTick);

        while (true)
        {
            yield return wait;
            if (player != null)
                player.changeHigh(-HighPerTick);
        }
    }
}
