using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.AI;

public class UrsoAtaque : MonoBehaviour
{
    [Header("Configurações de Morte (UI)")]
    public GameObject canvasGameOver;
    public string nomeCenaMenu = "MenuPrincipal"; // Coloque o nome exato da sua cena de menu

    [Header("Juice (O Bote)")]
    public GameObject efeitoGarras; // A imagem do arranhão
    public AudioSource somAtaqueSource; // O rugido/som de rasgar
    public float shakeDuracao = 0.5f;
    public float shakeIntensidade = 6.0f;
    public float tempoParaTelaVermelha = 1.2f; // Tempo do susto antes de aparecer o menu

    private bool jaAtacou = false;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!jaAtacou && collision.gameObject.CompareTag("Player"))
        {
            jaAtacou = true;
            StartCoroutine(SequenciaDeMorte(collision.gameObject));
        }
    }

    private IEnumerator SequenciaDeMorte(GameObject player)
    {
        // 1. Congela o jogo (Urso e Player)
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null) agent.isStopped = true;

        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null) pc.SetCanMove(false);
        
        // 2. O BOTE (Juice puro!)
        if (somAtaqueSource != null) somAtaqueSource.Play();
        if (efeitoGarras != null) efeitoGarras.SetActive(true);
        if (CameraShake.Instance != null) CameraShake.Instance.Shake(shakeDuracao, shakeIntensidade);

        // 3. Deixa o player assistir a morte por um tempinho
        yield return new WaitForSeconds(tempoParaTelaVermelha);

        // 4. Sobe a UI de Game Over e pausa o jogo de verdade
        if (canvasGameOver != null) canvasGameOver.SetActive(true);
        Time.timeScale = 0f; 
    }

    // --- FUNÇÕES DOS BOTÕES ---
    public void TentarNovamente()
    {
        Time.timeScale = 1f; // Despausa o mundo
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Recarrega a cena atual
    }

    public void VoltarMenu()
    {
        Time.timeScale = 1f; // Despausa o mundo
        SceneManager.LoadScene(nomeCenaMenu);
    }
}