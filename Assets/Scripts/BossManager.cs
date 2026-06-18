using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine; 
using TMPro; 
using UnityEngine.Playables;

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
    public CinemachineCamera vcam;        // A câmera normal de exploração
    public CinemachineCamera vcamCombate; // A câmera especial para a arena
    public Transform pontoFocoEntrada;    
    [TextArea(2, 5)] public string[] falasIntro;

    [Header("Cinemática Final (Passo 15)")]
    public PlayableDirector diretorFinal; 
    public CinemachineCamera vcamSaida;   // A câmera que olha pros arbustos no final
    public Transform pontoFocoSaida;      
    public float tempoCameraIr = 2.0f;    
    public float tempoCameraVoltar = 2.0f; 
    [TextArea(2, 5)] public string[] falaUmFuga;     
    [TextArea(2, 5)] public string[] falaDoisSaida;  

    [Header("O Desfecho (Gatilhos Invisíveis)")]
    public GameObject[] arbustosDeSaida; 

    private NavMeshAgent agentUrso;
    private SpriteRenderer srUrso;
    private bool estaAtacando = false;
    public bool estaFugindo = false; 
    private Transform arbustoAtual;
    private bool lutaTerminou = false;

    void Start()
    {
        agentUrso = urso.GetComponent<NavMeshAgent>();
        srUrso = urso.GetComponent<SpriteRenderer>();
        urso.SetActive(false);

        // --- GARANTE AS CÂMERAS CERTAS NO INÍCIO ---
        if (vcam != null) vcam.gameObject.SetActive(true);
        if (vcamCombate != null) vcamCombate.gameObject.SetActive(false);
        if (vcamSaida != null) vcamSaida.gameObject.SetActive(false);
        
        foreach(GameObject arb in arbustosDeSaida) if(arb != null) arb.SetActive(false);
    }

    public void ComecarLuta() { StartCoroutine(IntroBossFight()); }

    IEnumerator IntroBossFight()
    {
        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null) pc.SetCanMove(false);

        // 1. A câmera normal vai olhar a porta fechando
        Transform alvoOriginal = vcam.Follow; 
        vcam.Follow = pontoFocoEntrada;       
        yield return new WaitForSeconds(1.5f); 

        // 2. Devolvemos o alvo original
        vcam.Follow = alvoOriginal;
        
        // 3. A MÁGICA AQUI: Desliga a normal e liga a de combate
        if (vcam != null) vcam.gameObject.SetActive(false);
        if (vcamCombate != null) vcamCombate.gameObject.SetActive(true);
        
        yield return new WaitForSeconds(1.5f); 

        // 4. Inicia o diálogo
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
        yield return new WaitForSeconds(1.5f);
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

    IEnumerator CutsceneFinalTimeline()
    {
        yield return new WaitForSeconds(3f); 

        bool terminouFala1 = false;
        if (DialogueManager.instance != null) DialogueManager.instance.StartDialogue(falaUmFuga, () => { terminouFala1 = true; });
        else terminouFala1 = true;
        yield return new WaitUntil(() => terminouFala1);

        // 3. A MÁGICA DA SAÍDA AQUI: Desliga a câmera de combate e liga a de saída!
        if (vcamCombate != null) vcamCombate.gameObject.SetActive(false);
        if (vcamSaida != null) vcamSaida.gameObject.SetActive(true);
        
        if (diretorFinal != null) diretorFinal.Play();

        yield return new WaitForSeconds(tempoCameraIr); 
        if (diretorFinal != null) diretorFinal.Pause(); 

        bool terminouFala2 = false;
        if (DialogueManager.instance != null) DialogueManager.instance.StartDialogue(falaDoisSaida, () => { terminouFala2 = true; });
        else terminouFala2 = true;
        yield return new WaitUntil(() => terminouFala2);

        // 5. VOLTA AO NORMAL: Desliga a câmera de saída e liga a normal do Player
        // if (vcamSaida != null) vcamSaida.gameObject.SetActive(false);
        // if (vcam != null) vcam.gameObject.SetActive(true);
        
        // if (diretorFinal != null) diretorFinal.Play();
        
        // yield return new WaitForSeconds(tempoCameraVoltar);

        // 6. LIGA OS GATILHOS DE SAÍDA
        foreach(GameObject arb in arbustosDeSaida) if(arb != null) arb.SetActive(true);
    }
}