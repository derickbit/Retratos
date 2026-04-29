using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GatilhoSaida : MonoBehaviour
{
    public string nomeDaProximaCena = "AClareira"; 
    public AudioSource somRugidoLonge; // Arraste o AudioSource do gatilho aqui
    public float tempoDeEco = 2.0f;    // Segundos que o jogo espera o rugido tocar antes de mudar de cena

    private bool jaEscapou = false;

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!jaEscapou && col.CompareTag("Player"))
        {
            jaEscapou = true;
            StartCoroutine(TransicaoDeFuga(col.gameObject));
        }
    }

    IEnumerator TransicaoDeFuga(GameObject player)
    {
        // 1. Trava o jogador para ele comemorar a vitória parado
        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null) pc.SetCanMove(false);

        // 2. Toca o rugido do urso frustrado lá atrás
        if (somRugidoLonge != null) somRugidoLonge.Play();

        // 3. Deixa o som ecoar...
        yield return new WaitForSeconds(tempoDeEco);

        // 4. Carrega a próxima cena!
            // Chama o seu fader para fazer a transição
            if (SceneFader.instance != null)
            {
                SceneFader.instance.LoadScene(nomeDaProximaCena);
            }
            else
            {
                Debug.LogError("Erro: SceneFader não encontrado na cena!");
            }
}}