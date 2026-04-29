using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject painelPrincipal; // Arraste o Painel_Principal aqui
    public GameObject painelCreditos;  // Arraste o Painel_Creditos aqui

public void PlayGame()
{
    // Limpa o HD (Save)
    PlayerPrefs.DeleteAll();

    if(AvisoDeTempo.instance != null) AvisoDeTempo.instance.ResetarTimer();

    // --- NOVO: LIMPA A MEMÓRIA DO INVENTÁRIO (Se ele já existir na memória) ---
        if (InventoryManager.instance != null) 
        {
            InventoryManager.instance.ResetarInventario();
        }

    SceneManager.LoadScene("Acampamento");
}

    public void AbrirCreditos()
    {
        painelPrincipal.SetActive(false); // Desliga os botões Iniciar/Sair
        painelCreditos.SetActive(true);   // Liga o texto de créditos
    }

    public void FecharCreditos()
    {
        painelCreditos.SetActive(false); // Desliga os créditos
        painelPrincipal.SetActive(true); // Traz os botões de volta
    }

    public void QuitGame()
    {
        Debug.Log("Saindo do jogo...");
        Application.Quit();
    }
}