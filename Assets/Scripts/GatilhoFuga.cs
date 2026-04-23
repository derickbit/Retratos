using UnityEngine;
using UnityEngine.Playables; // Necessário para controlar a Timeline
using System.Collections;
using UnityEngine.AI;

public class GatilhoFuga : MonoBehaviour
{
    [Header("Cinemática (Timeline)")]
    public PlayableDirector diretor; // A nossa Timeline
    public float tempoAteOUrso = 2.0f; // Segundos até a câmera focar no urso
    public float tempoDeVolta = 2.0f;  // Segundos para a câmera voltar ao player

    [Header("Diálogos")]
    [TextArea(2, 5)]
    public string[] pensamentosIniciais;
    
    [Header("Dica (Arma Pesada)")]
    [TextArea(2, 5)]
    public string[] pensamentoDicaArma;
    public float tempoParaDica = 2.0f;

    [Header("Atores")]
    public UrsoPerseguidor scriptUrso;
    private PlayerController playerControl;

    private bool jaAtivou = false;
    private bool dicaJaMostrada = false;

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!jaAtivou && col.CompareTag("Player"))
        {
            jaAtivou = true;
            playerControl = col.GetComponent<PlayerController>();
            
            // Trava o player fisicamente
            playerControl.SetCanMove(false);

            // Liga o Urso de surpresa!
if (scriptUrso != null) scriptUrso.gameObject.SetActive(true);
            
            // Inicia a super coreografia!
            StartCoroutine(RotinaDaCutscene());
        }
    }

    IEnumerator RotinaDaCutscene()
    {
        // 1. Dá o play na Timeline (A câmera começa a viajar pro urso)
        if (diretor != null) diretor.Play();

        // 2. Espera os exatos segundos da câmera chegar e focar no urso
        yield return new WaitForSeconds(tempoAteOUrso);

        // 3. Pausa a Timeline (A tela congela com o urso em evidência)
        if (diretor != null) diretor.Pause();

        // 4. Mostra o balão de fala "PQP, um urso!"
        bool terminouDeFalar = false;
        if (DialogueManager.instance != null)
        {
            // O "() => { terminouDeFalar = true; }" é um atalho que muda a variável quando o diálogo fecha
            DialogueManager.instance.StartDialogue(pensamentosIniciais, () => { terminouDeFalar = true; });
        }
        else { terminouDeFalar = true; }

        // 5. O jogo fica esperando aqui ATÉ o player fechar a última caixa de diálogo
        yield return new WaitUntil(() => terminouDeFalar);

        // 6. Solta a Timeline (A câmera viaja de volta pro player)
        if (diretor != null) diretor.Play();

        // 7. Espera o tempo da câmera voltar
        yield return new WaitForSeconds(tempoDeVolta);

        // 8. O BOTE! Solta o Urso e destrava o Player
        if (scriptUrso != null) scriptUrso.acordado = true;
        if (playerControl != null) playerControl.SetCanMove(true);

        // 9. Começa a vigiar se o player vai correr com a espingarda na mão
        StartCoroutine(MonitorarArmaNaFuga());
    }

    // --- A LÓGICA DA DICA CONTINUA IGUAL! ---
    IEnumerator MonitorarArmaNaFuga()
    {
        float tempoComArma = 0f;
        while (!dicaJaMostrada && scriptUrso != null)
        {
            if (playerControl != null && playerControl.hasEspingarda && playerControl.rb.linearVelocity.sqrMagnitude > 0.1f)
            {
                tempoComArma += Time.deltaTime;
                if (tempoComArma >= tempoParaDica)
                {
                    MostrarDicaArma();
                    yield break; 
                }
            }
            else
            {
                tempoComArma = 0f;
            }
            yield return null; 
        }
    }

    void MostrarDicaArma()
    {
        dicaJaMostrada = true;
        NavMeshAgent agentUrso = scriptUrso.GetComponent<NavMeshAgent>();
        if (agentUrso != null) agentUrso.isStopped = true;
        if (playerControl != null) playerControl.SetCanMove(false);
        if (DialogueManager.instance != null)
        {
            DialogueManager.instance.StartDialogue(pensamentoDicaArma, RetomarFuga);
        }
    }

    void RetomarFuga()
    {
        NavMeshAgent agentUrso = scriptUrso.GetComponent<NavMeshAgent>();
        if (agentUrso != null) agentUrso.isStopped = false;
        if (playerControl != null) playerControl.SetCanMove(true);
    }
}