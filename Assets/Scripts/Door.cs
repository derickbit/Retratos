using UnityEngine;
using UnityEngine.SceneManagement;

public class Door : MonoBehaviour
{
    [Header("Configuração")]
    public string sceneToLoad; 
    public string spawnPointID; // <--- NOVO: Qual o ID do spawn destino? (ex: "FrenteCabana")

    // Variável Global (Estática) que lembra o ID da próxima porta
    // "static" significa que ela pertence à CLASSE, não ao objeto. Ela não morre quando muda de cena.
    public static string nextSpawnID; 

    public void Enter()
    {
        if (sceneToLoad != "")
        {
            // Antes de viajar, anota no bloquinho onde devemos nascer
            nextSpawnID = spawnPointID; 
            
            if (SceneFader.instance != null)
        {
            SceneFader.instance.LoadScene(sceneToLoad);
        }
        else
        {
            // Fallback caso esqueça de colocar o Fader na cena
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoad);
        }
        }
    }
}