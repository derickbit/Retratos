using UnityEngine;
using System.Collections;

public class RetornoTalker : MonoBehaviour
{
    [Header("Configuração do Esporro")]
    [TextArea(2, 5)]
    public string[] falaVoltou; // Ex: "Amigo: Voltou? Vai logo caçar ô mala!"
    
    public string cenaPerseguicao = "Cena_Perseguicao";
    public string spawnPerseguicao = "Spawn_InicioFuga";

    void Start()
    {
        // Verifica se o player veio do gatilho de "volta"
        if (PlayerPrefs.GetInt("PlayerDesistiu", 0) == 1)
        {
            StartCoroutine(RotinaDeEsporro());
        }
    }

    IEnumerator RotinaDeEsporro()
    {
        // 1. Limpa a marcação para não entrar em loop infinito
        PlayerPrefs.SetInt("PlayerDesistiu", 0);
        PlayerPrefs.Save();

        // 2. Trava o player
        PlayerController pc = FindAnyObjectByType<PlayerController>();
        if (pc != null) pc.SetCanMove(false);

        // 3. Aguarda um milissegundo para a câmera se estabilizar
        yield return new WaitForSeconds(0.5f);

        // 4. Inicia o diálogo (Com "Amigo:" para a câmera voar até ele automaticamente)
        bool terminouFala = false;
        if (DialogueManager.instance != null)
        {
            DialogueManager.instance.StartDialogue(falaVoltou, () => { terminouFala = true; });
        }
        else { terminouFala = true; }

        yield return new WaitUntil(() => terminouFala);

        // 5. Manda o player de volta para a floresta imediatamente
        Door.nextSpawnID = spawnPerseguicao;
        
        if (SceneFader.instance != null)
        {
            SceneFader.instance.LoadScene(cenaPerseguicao);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(cenaPerseguicao);
        }
    }
}