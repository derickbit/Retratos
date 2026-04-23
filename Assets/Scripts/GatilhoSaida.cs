using UnityEngine;
using UnityEngine.SceneManagement;

public class GatilhoSaida : MonoBehaviour
{
    public string nomeDaProximaCena = "Clareira"; // Nome exato da cena do Boss
    private bool jaEscapou = false;

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!jaEscapou && col.CompareTag("Player"))
        {
            jaEscapou = true;
            
            // 1. Opcional: Tocar um som de porta fechando ou mato balançando
            
            // 2. Chama a próxima cena
            // Se você tiver um script de Fade (escurecer a tela), chame-o aqui primeiro!
            SceneManager.LoadScene(nomeDaProximaCena);
        }
    }
}