using UnityEngine;

public class Examinable : MonoBehaviour
{
    [TextArea(3, 10)]
    public string[] description; // O texto que aparece ao clicar

    public void Examine()
    {
        // Chama o DialogueManager.
        // Como o Manager já tem a lógica de travar o player, não precisamos fazer nada aqui!
        DialogueManager.instance.StartDialogue(description);
    }
}