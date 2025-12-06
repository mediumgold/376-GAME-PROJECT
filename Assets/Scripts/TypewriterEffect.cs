using System.Collections;
using TMPro;
using UnityEngine;

public class TypewriterEffect : MonoBehaviour
{
    [SerializeField] private float typeWriterSpeed = 50f;
    public Coroutine Run(string textToType, TMP_Text textBox)
    {
        return StartCoroutine(TypeText(textToType, textBox));
    }

    private IEnumerator TypeText(string textToType, TMP_Text textbox)
    {
        float t = 0;
        int charIndex = 0;
        while(charIndex < textToType.Length)
        {
            t += Time.deltaTime * typeWriterSpeed;
            charIndex = Mathf.FloorToInt(t);
            charIndex = Mathf.Clamp(charIndex, 0, textToType.Length);

            textbox.text = textToType.Substring(0, charIndex);
            yield return null;
        }

        textbox.text = textToType;
    }
}
