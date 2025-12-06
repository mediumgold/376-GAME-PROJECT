using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/DialogueObject")]
public class DialogueObject : ScriptableObject
{
    [SerializeField] [TextArea] private string[] dialogue;
    public string[] Dialogue => dialogue;

    [Header("Optional Choices")]
    public bool hasChoices = false;
    public string[] choices;
}
