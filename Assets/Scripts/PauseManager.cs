using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour {
    public GameObject painelPausa;
    private bool estaPausado = false;

    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (estaPausado) Continuar();
            else Pausar();
        }
    }

    public void Pausar() {
        painelPausa.SetActive(true);
        Time.timeScale = 0f; // Para o tempo do jogo (física, movimentos, etc)
        estaPausado = true;
    }

    public void Continuar() {
        painelPausa.SetActive(false);
        Time.timeScale = 1f; // Volta o tempo ao normal
        estaPausado = false;
    }

    public void IrParaMenu() {
        estaPausado = false;
        Time.timeScale = 1f; // NUNCA esqueça de resetar o tempo antes de mudar de cena
        if(painelPausa != null) painelPausa.SetActive(false);
        SceneManager.LoadScene("MenuPrincipal");
    }
}