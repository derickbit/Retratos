using UnityEngine;

public class GatilhoTrilha : MonoBehaviour
{
    [Header("Configurações")]
    public string nomeDaProximaCena = "Cena_Perseguicao";
    public string idPontoDeSpawn = "Spawn_InicioFuga";

    private bool jaAcionado = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (jaAcionado) return;

        if (collision.CompareTag("Player"))
        {
            // Bloqueia se não tiver a espingarda salva na memória
            if (PlayerPrefs.GetInt("PossuiEspingarda", 0) == 0)
            {
                Debug.Log("Amigo: 'Vai lá pegar o rifle antes de ir!'");
                return; 
            }

            jaAcionado = true;

            // Define onde o player vai nascer na próxima cena
            Door.nextSpawnID = idPontoDeSpawn;

            // Trava o player para ele não andar enquanto a tela escurece
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null) player.canMove = false;

            // Chama o seu fader para fazer a transição
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
}