using UnityEngine;
using UnityEngine.UI;

public class FlashManager : MonoBehaviour
{
    public static FlashManager Instance { get; private set; }

    [Header("UI Reference")]
    [SerializeField] private Image flashImage;

    [Header("Flash Settings")]
    [SerializeField] private float defaultDuration = 0.3f;

    [Header("Preset Colors")]
    [SerializeField] private Color damageColor = new Color(1f, 0f, 0f, 0.5f);      // Red
    [SerializeField] private Color drugColor = new Color(0f, 0.5f, 1f, 0.5f);      // Blue
    [SerializeField] private Color escapeFlashColor = new Color(0f, 0.5f, 1f, 0.5f); // Blue

    private float timer = 0f;
    private float currentDuration;
    private Color currentFlashColor;
    private Color transparent;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (flashImage == null)
            flashImage = GetComponent<Image>();

        transparent = new Color(0f, 0f, 0f, 0f);
        if (flashImage != null)
            flashImage.color = transparent;
    }

    private void Update()
    {
        if (timer > 0f && flashImage != null)
        {
            timer -= Time.deltaTime;
            float t = Mathf.Clamp01(timer / currentDuration);
            flashImage.color = Color.Lerp(transparent, currentFlashColor, t);
        }
    }

    /// <summary>
    /// Flash with a custom color and optional duration.
    /// </summary>
    public void Flash(Color color, float duration = -1f)
    {
        if (flashImage == null) return;

        currentFlashColor = color;
        currentDuration = duration > 0f ? duration : defaultDuration;
        timer = currentDuration;
        flashImage.color = currentFlashColor;
    }

    /// <summary>
    /// Red flash for taking damage.
    /// </summary>
    public void FlashDamage()
    {
        Flash(damageColor);
    }

    /// <summary>
    /// Blue flash for using drugs.
    /// </summary>
    public void FlashDrug()
    {
        Flash(drugColor);
    }

    /// <summary>
    /// Blue flash for escape sequence starting.
    /// </summary>
    public void FlashEscape()
    {
        Flash(escapeFlashColor);
    }
}
