using UnityEngine;
using UnityEngine.SceneManagement;

public class EndScreenManager : MonoBehaviour
{
public void VoltarAoMenu()
    {
        // 1. Garante que o tempo está rodando
        Time.timeScale = 1f;

        // 2. Desliga ESTE painel para ele não pegar "carona" para o menu
        gameObject.SetActive(false);

        // 3. A MÁGICA: Limpa a tela preta do Fade! 
        // Deixa o alpha zero (transparente) para não tapar a câmera do menu
        // 4. Agora sim, carrega a cena do Menu Principal
        SceneManager.LoadScene("MenuPrincipal");
    }

    public void SairDoJogo()
    {
        Debug.Log("Saiu do Jogo");
        Application.Quit();
    }
}