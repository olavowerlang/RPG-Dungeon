using UnityEngine;

[System.Serializable]
public struct DialogueLine
{
    public string speakerName;
    [TextArea(2, 5)]
    public string text;
}

[CreateAssetMenu(fileName = "DialogueData", menuName = "Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    public DialogueLine[] lines;
}
