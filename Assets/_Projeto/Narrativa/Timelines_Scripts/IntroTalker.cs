using UnityEngine;
using System; // <-- Precisa disso para funcionar o "Callback"

public class IntroTalker : MonoBehaviour 
{
    [Header("Configuração de Texto")]
    [TextArea(3, 10)]
    public string[] frasesDaCutscene;

    void Start()
    {
        // Assim que o jogo começa, ele já amarra o boneco. Simples e direto.
        PlayerController p = FindAnyObjectByType<PlayerController>();
        if (p != null) p.SetCanMove(false);
    }

    public void DispararDialogo() 
    {
        if (DialogueManager.instance != null) 
        {
            // Agora enviamos as frases E dizemos qual função rodar no final!
            DialogueManager.instance.StartDialogue(frasesDaCutscene, AtivarCameraDaArma);
        }
        else
        {
            Debug.LogError("DialogueManager não encontrado nesta cena!");
        }
    }

    // Criamos a função separada para organizar a casa
void AtivarCameraDaArma()
    {
        ChamaCamera scriptCamera = GetComponent<ChamaCamera>();
        if (scriptCamera != null)
        {
            scriptCamera.DispararCamera();
        }
    }
}