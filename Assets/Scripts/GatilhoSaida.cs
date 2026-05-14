using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GatilhoSaida : MonoBehaviour
{
    public string nomeDaProximaCena = "AClareira"; 
    public string idPontoDeSpawn = "Spawn_EntradaClareira"; // <-- NOVO: Onde ele vai nascer na clareira?
    
    public AudioSource somRugidoLonge; 
    public float tempoDeEco = 2.0f;    

    private bool jaEscapou = false;

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!jaEscapou && col.CompareTag("Player"))
        {
            jaEscapou = true;

            // --- A TRAVA DE SEGURANÇA ENTRA AQUI! ---
            GameObject urso = GameObject.Find("Urso"); 
            if (urso != null)
            {
                Collider2D colisorUrso = urso.GetComponent<Collider2D>();
                if (colisorUrso != null) colisorUrso.enabled = false;

                UnityEngine.AI.NavMeshAgent agente = urso.GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (agente != null) 
                {
                    agente.isStopped = true;
                    agente.velocity = Vector3.zero;
                }
            }
            // ----------------------------------------

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

        // --- NOVO: Define onde ele vai nascer na próxima cena! ---
        Door.nextSpawnID = idPontoDeSpawn;

        // 4. Carrega a próxima cena!
        if (SceneFader.instance != null)
        {
            SceneFader.instance.LoadScene(nomeDaProximaCena);
        }
        else
        {
            Debug.LogError("Erro: SceneFader não encontrado na cena!");
        }
    }
}