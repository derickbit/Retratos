using UnityEngine;
using System.Collections;

public class GatilhoTrilha : MonoBehaviour
{
    [Header("Configurações da Próxima Cena")]
    public string nomeDaProximaCena = "Cena_Perseguicao";
    public string idPontoDeSpawn = "Spawn_InicioFuga";

    [Header("Aviso de Falta de Arma")]
    [TextArea(2, 5)]
    [Tooltip("Texto deve começar com 'Amigo:' para a câmera voar até ele!")]
    public string[] falaSemArma = new string[] { "Amigo: Ei, tá maluco? Não vai pra floresta desarmado! Volta aqui e pega o rifle." };

    private bool jaAcionado = false;
    private bool avisando = false; // Trava para não iniciar duas vezes a mesma cutscene

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (jaAcionado || avisando) return;

        if (collision.CompareTag("Player"))
        {
            // Checa a verdade absoluta no nosso novo InventoryManager!
            bool temArma = false;
            if (InventoryManager.instance != null)
            {
                temArma = InventoryManager.instance.VerificarSePossuiItem("espingarda");
            }

            // SE ELE NÃO TEM A ARMA, RODA A CUTSCENE DO ESPORRO E CANCELA A VIAGEM
            if (!temArma)
            {
                PlayerController player = collision.GetComponent<PlayerController>();
                if (player != null) StartCoroutine(CutsceneAvisoArma(player));
                return; 
            }

            // --- SE ELE TEM A ARMA, CARREGA A PRÓXIMA CENA NORMALMENTE ---
            jaAcionado = true;
            Door.nextSpawnID = idPontoDeSpawn;

            PlayerController pc = collision.GetComponent<PlayerController>();
            if (pc != null) pc.SetCanMove(false); // Trava o player no fade

            // Chama o seu fader para fazer a transição
            if (SceneFader.instance != null)
            {
                SceneFader.instance.LoadScene(nomeDaProximaCena);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(nomeDaProximaCena);
            }
        }
    }

    IEnumerator CutsceneAvisoArma(PlayerController player)
    {
        avisando = true;

        // 1. Trava o jogador e para as perninhas dele
        player.SetCanMove(false);
        player.animator.SetBool("isWalking", false);

        // 2. Abre o diálogo (como o texto começa com 'Amigo:', a câmera já vai voar pra ele!)
        bool terminouFala = false;
        if (DialogueManager.instance != null)
        {
            DialogueManager.instance.StartDialogue(falaSemArma, () => { terminouFala = true; });
        }
        else { terminouFala = true; }

        yield return new WaitUntil(() => terminouFala);

        // 3. O EMPURRÃOZINHO! Empurra o jogador meio passo pra trás (pra fora do colisor)
        // Isso obriga ele a ter que andar pra frente de novo e acionar o gatilho se quiser insistir.
        Vector3 direcaoEmpurrao = (player.transform.position - transform.position).normalized;
        player.transform.position += direcaoEmpurrao * 0.5f;

        // 4. Destrava o jogador para voltar a jogar
        player.SetCanMove(true);
        avisando = false;
    }
}