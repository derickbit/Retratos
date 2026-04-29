using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine; 
using TMPro; 
using UnityEngine.Playables; // <-- NECESSÁRIO PARA A TIMELINE DO FINAL!

public class BossManager : MonoBehaviour
{
    [Header("Atores")]
    public GameObject urso;
    public Transform player;
    public List<ArbustoBoss> arbustos; 

    [Header("Configurações")]
    public float velocidadeAtaque = 3.5f; 
    public float velocidadeFuga = 6.0f;   

    [Header("Cinemática Inicial e Diálogos")]
    public CinemachineCamera vcam;        
    public Transform pontoFocoEntrada;    
    public float zoomBatalha = 4.5f;      
    [TextArea(2, 5)] public string[] falasIntro; 

    [Header("Cinemática Final (Passo 15)")]
    public PlayableDirector diretorFinal; // A sua Timeline do Final
    public CinemachineCamera vcamSaida;
    public Transform pontoFocoSaida;      // O alvo que a câmera vai olhar
    public float tempoCameraIr = 2.0f;    // Segundos da Timeline indo
    public float tempoCameraVoltar = 2.0f; // Segundos da Timeline voltando
    [TextArea(2, 5)] public string[] falaUmFuga;     // "Ele fugiu..."
    [TextArea(2, 5)] public string[] falaDoisSaida;  // "Acho que consigo sair por ali"

    [Header("O Desfecho (Gatilhos Invisíveis)")]
    public GameObject[] arbustosDeSaida; // Coloque aqui os FILHOS das moitas!

    private NavMeshAgent agentUrso;
    private SpriteRenderer srUrso;
    private bool estaAtacando = false;
    private bool estaFugindo = false; 
    private Transform arbustoAtual;
    private bool lutaTerminou = false;

    void Start()
    {
        agentUrso = urso.GetComponent<NavMeshAgent>();
        srUrso = urso.GetComponent<SpriteRenderer>();
        urso.SetActive(false);
        
        // Garante que os gatilhos de saída comecem desligados (a arte continua aparecendo!)
        foreach(GameObject arb in arbustosDeSaida) if(arb != null) arb.SetActive(false);
    }

    public void ComecarLuta() { StartCoroutine(IntroBossFight()); }

    IEnumerator IntroBossFight()
    {
        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null) pc.SetCanMove(false);

        Transform alvoOriginal = vcam.Follow; 
        vcam.Follow = pontoFocoEntrada;       
        yield return new WaitForSeconds(1.5f); 

        vcam.Follow = alvoOriginal;
        
        float t = 0f; float zInic = vcam.Lens.OrthographicSize; 
        while (t < 1.5f) { t += Time.deltaTime; vcam.Lens.OrthographicSize = Mathf.Lerp(zInic, zoomBatalha, t / 1.5f); yield return null; }

        bool terminouFala = false;
        if (DialogueManager.instance != null) DialogueManager.instance.StartDialogue(falasIntro, () => { terminouFala = true; });
        else terminouFala = true;
        yield return new WaitUntil(() => terminouFala);

        if (pc != null) pc.SetCanMove(true);
        StartCoroutine(CicloDeBait());
    }

    IEnumerator CicloDeBait()
    {
        if (lutaTerminou) yield break; 

        yield return new WaitForSeconds(2f); 
        int quantidadeDeBaitsFalsos = Random.Range(1, 5); 

        for (int i = 0; i < quantidadeDeBaitsFalsos; i++)
        {
            int indexFalso = Random.Range(0, arbustos.Count);
            arbustos[indexFalso].IniciarBait(); 
            yield return new WaitForSeconds(1.5f); 
            arbustos[indexFalso].PararBait(); 
            yield return new WaitForSeconds(0.5f); 
        }

        int indexAtaque = Random.Range(0, arbustos.Count);
        arbustoAtual = arbustos[indexAtaque].transform;
        
        urso.transform.position = arbustoAtual.position;
        
        arbustos[indexAtaque].IniciarBait();
        yield return new WaitForSeconds(1f);
        arbustos[indexAtaque].PararBait();

        urso.SetActive(true);
        estaAtacando = true;
    }

    void Update()
    {
        if (estaAtacando && urso.activeSelf)
        {
            agentUrso.speed = velocidadeAtaque; 
            agentUrso.SetDestination(player.position);
            
            if (agentUrso.velocity.x > 0.1f) srUrso.flipX = true;
            else if (agentUrso.velocity.x < -0.1f) srUrso.flipX = false;
        }
    }

    public void RecuarParaMato()
    {
        if (estaFugindo || lutaTerminou) return; 
        estaAtacando = false; 
        estaFugindo = true;
        StartCoroutine(FugirProArbusto(false));
    }

    public void UrsoDerrotado()
    {
        if (lutaTerminou) return;
        lutaTerminou = true;
        estaAtacando = false;
        estaFugindo = true;
        StartCoroutine(FugirProArbusto(true));
    }

    IEnumerator FugirProArbusto(bool fugaDefinitiva)
    {
        Transform arbustoMaisProximo = arbustos[0].transform;
        float menorDistancia = Mathf.Infinity;
        foreach (ArbustoBoss arb in arbustos)
        {
            float dist = Vector2.Distance(urso.transform.position, arb.transform.position);
            if (dist < menorDistancia) { menorDistancia = dist; arbustoMaisProximo = arb.transform; }
        }

        agentUrso.speed = velocidadeFuga;
        agentUrso.SetDestination(arbustoMaisProximo.position);

        while (Vector2.Distance(urso.transform.position, arbustoMaisProximo.position) > 1.5f)
        {
            if (agentUrso.velocity.x > 0.1f) srUrso.flipX = true;
            else if (agentUrso.velocity.x < -0.1f) srUrso.flipX = false;
            yield return null; 
        }

        urso.SetActive(false);
        estaFugindo = false; 

        if (fugaDefinitiva)
        {
            StartCoroutine(CutsceneFinalTimeline());
        }
        else
        {
            StartCoroutine(CicloDeBait());
        }
    }

    // --- A CUTSCENE FINAL EXATAMENTE COMO VOCÊ PEDIU ---
   IEnumerator CutsceneFinalTimeline()
    {
        // 1. Silêncio dramático de 3 segundos
        yield return new WaitForSeconds(3f); 

        // 2. FALA 1 (Ele fugiu...)
        bool terminouFala1 = false;
        if (DialogueManager.instance != null) DialogueManager.instance.StartDialogue(falaUmFuga, () => { terminouFala1 = true; });
        else terminouFala1 = true;
        yield return new WaitUntil(() => terminouFala1);

        // 3. LIGA A CÂMERA DE SAÍDA (O Cinemachine faz o voo suave sozinho!)
        if (vcamSaida != null) vcamSaida.gameObject.SetActive(true);
        
        // Dá o play na Timeline só se você tiver animações extras lá
        if (diretorFinal != null) diretorFinal.Play();

        yield return new WaitForSeconds(tempoCameraIr); // Espera a câmera chegar lá
        if (diretorFinal != null) diretorFinal.Pause(); 

        // 4. FALA 2 (Acho que consigo sair...)
        bool terminouFala2 = false;
        if (DialogueManager.instance != null) DialogueManager.instance.StartDialogue(falaDoisSaida, () => { terminouFala2 = true; });
        else terminouFala2 = true;
        yield return new WaitUntil(() => terminouFala2);

        // 5. VOLTA A CÂMERA (Desliga a da saída, e o Cinemachine volta pro Player)
        if (vcamSaida != null) vcamSaida.gameObject.SetActive(false);
        if (diretorFinal != null) diretorFinal.Play();
        
        yield return new WaitForSeconds(tempoCameraVoltar);

        // 6. LIGA OS GATILHOS DE SAÍDA
        foreach(GameObject arb in arbustosDeSaida) if(arb != null) arb.SetActive(true);
    }
}