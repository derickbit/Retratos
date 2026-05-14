using UnityEngine;
using System; 

public class IntroTalker : MonoBehaviour 
{
    [Header("Configuração de Identidade")]
    public string idDestaIntro = "Intro_Acampamento"; // <-- Dê um nome diferente para cada cena!

    [Header("Configuração de Texto")]
    [TextArea(3, 10)]
    public string[] frasesDaCutscene;

    void Awake()
    {
        // 1. Verifica se ESTA intro específica já rolou
        if (PlayerPrefs.GetInt(idDestaIntro, 0) == 1)
        {
            Destroy(gameObject);
            return;
        }

        // 2. Trava específica para o Acampamento: Se o player "desistiu" da perseguição, 
        // não queremos a intro de "Início de Jogo" tocando na cara dele.
        if (idDestaIntro == "Intro_Acampamento" && PlayerPrefs.GetInt("PlayerDesistiu", 0) == 1)
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        PlayerController p = FindAnyObjectByType<PlayerController>();
        if (p != null) p.SetCanMove(false);
    }

    public void DispararDialogo() 
    {
        if (DialogueManager.instance != null) 
        {
            DialogueManager.instance.StartDialogue(frasesDaCutscene, AtivarCameraDaArma);
        }
    }

    void AtivarCameraDaArma()
    {
        // SALVA NA MEMÓRIA que ESTA intro específica já aconteceu
        PlayerPrefs.SetInt(idDestaIntro, 1);
        PlayerPrefs.Save();

        ChamaCamera scriptCamera = GetComponent<ChamaCamera>();
        if (scriptCamera != null)
        {
            scriptCamera.DispararCamera();
        }
    }
}