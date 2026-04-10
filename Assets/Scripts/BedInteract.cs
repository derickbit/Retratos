using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class BedInteract : MonoBehaviour
{
    [Header("Cenários (Pastas)")]
    public GameObject propsNoite; 
    public GameObject propsDia;   
    
    [Header("Player")]
    public GameObject playerDePe; 

    [Header("Sprites da Cama")]
    public SpriteRenderer bedRenderer; 
    public Sprite camaVazia;   
    public Sprite camaOcupada; 
    
    [Header("Diálogos")]
    [TextArea] public string[] perguntaDormir; 
    [TextArea] public string[] textoAcordar;   

    [Header("Áudio")]
    public AudioSource somSusto; 
    public AudioSource somAmbiente; // Arraste o 'Audio_Ambiente' (floresta) aqui
    public AudioClip somPassarinhos; // Arraste o novo arquivo 'forest_day' aqui

    [Header("Fim do Jogo")]
    public GameObject painelFim;
    public string triggerID = "interacao_cama"; // ID para salvar a memória

    private bool esperandoConfirmacao = false;

    private void Start()
    {
        // Se o player já interagiu uma vez com a cama nesta vida, 
        // ele pula direto para o estado de "esperando confirmação" (se ele não dormiu ainda)
        if (PlayerPrefs.GetInt(triggerID, 0) == 1)
        {
            esperandoConfirmacao = true;
        }
    }

    public void TentarDormir()
    {
        // Se ele ainda não dormiu (propsDia ainda está desativado)
        if (!propsDia.activeSelf)
        {
            if (!esperandoConfirmacao)
            {
                StartCoroutine(Perguntar());
            }
            else
            {
                StartCoroutine(SequenciaDormir());
            }
        }
    }

    IEnumerator Perguntar()
    {
        DialogueManager.instance.StartDialogue(perguntaDormir);
        esperandoConfirmacao = true;
        
        // Salva que ele já teve o primeiro pensamento de sono
        PlayerPrefs.SetInt(triggerID, 1);
        PlayerPrefs.Save();
        
        yield return new WaitForSeconds(3f);
    }

    IEnumerator SequenciaDormir()
    {
        playerDePe.GetComponent<SpriteRenderer>().enabled = false;

        // --- NOVO: APAGA A SOMBRA DO JOGADOR ---
        UnityEngine.Rendering.Universal.ShadowCaster2D sombra = playerDePe.GetComponent<UnityEngine.Rendering.Universal.ShadowCaster2D>();
        if (sombra != null) sombra.enabled = false;

        PlayerController pc = playerDePe.GetComponent<PlayerController>();
        if (pc != null) 
        {
            // --- NOVO: SE ESTIVER COM A TOCHA, DROPA ELA ANTES DE DEITAR ---
            if (pc.hasTorch)
            {
                pc.DroparTocha(true);
            }
            
            pc.canMove = false;
        }

        if (bedRenderer != null && camaOcupada != null) bedRenderer.sprite = camaOcupada;

        yield return new WaitForSeconds(1.5f);

        yield return StartCoroutine(FadeToBlack());

        // --- A MÁGICA NO ESCURO ---
        propsNoite.SetActive(false); 
        propsDia.SetActive(true);    
        // 1. APAGA A TOCHA DA MÃO (Acessando direto o PlayerController)
        PlayerController pcController = playerDePe.GetComponent<PlayerController>();
        if (pcController != null)
        {
            pcController.isTorchLit = false;
            pcController.animator.SetBool("isLit", false);
            if (pcController.torchLightObject != null) pcController.torchLightObject.SetActive(false);
        }

        // 2. FORÇA A MEMÓRIA A SABER QUE O FOGO APAGOU PRA SEMPRE
        PlayerPrefs.SetInt("TochaAcesa", 0);
        PlayerPrefs.Save();

// 3. APAGA AS TOCHAS DO CHÃO (Trocando o Prefab)
        TorchItem[] todasAsTochas = Object.FindObjectsByType<TorchItem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        
        // Pegamos a referência do prefab de tocha apagada que está lá no PlayerController
        GameObject prefabApagada = pcController.tochaApagadaPrefab;

        foreach (TorchItem tocha in todasAsTochas)
        {
            // Se for uma tocha e ela estiver acesa...
            if (tocha.estavaAcesa)
            {
                // 1. Cria a tocha apagada na mesma posição e rotação
                if (prefabApagada != null)
                {
                    Instantiate(prefabApagada, tocha.transform.position, tocha.transform.rotation);
                }

                // 2. Destrói a tocha acesa velha
                Destroy(tocha.gameObject);
            }
        }

        // MUDANÇA DE ÁUDIO:
        if (somAmbiente != null && somPassarinhos != null)
        {
            somAmbiente.Stop(); // Para os grilos/corujas
            somAmbiente.clip = somPassarinhos; // Troca o disco
            somAmbiente.loop = true; // Garante que vai repetir
            somAmbiente.Play(); // Começa os passarinhos
        }

        yield return StartCoroutine(FadeToClear());
        
        if (somSusto != null) somSusto.Play();
        // --------------------------

        yield return new WaitForSeconds(2f);

        bedRenderer.sprite = camaVazia;
        playerDePe.GetComponent<SpriteRenderer>().enabled = true;

        // --- NOVO: DEVOLVE A SOMBRA PRO JOGADOR ---
        if (sombra != null) sombra.enabled = true;
        
        yield return StartCoroutine(FadeToClear());

        // Reação
        DialogueManager.instance.StartDialogue(textoAcordar);
        
        // Espera o diálogo acabar (usando a lógica que fizemos antes)
        while (DialogueManager.instance.isDialogueActive)
        {
            yield return null;
        }

        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(FadeToBlack());

        if (painelFim != null)
        {
            painelFim.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // Auxiliares de Fade
    IEnumerator FadeToBlack()
    {
        if (SceneFader.instance != null)
        {
            CanvasGroup fader = SceneFader.instance.fadeGroup;
            while (fader.alpha < 1) { fader.alpha += Time.deltaTime * 2; yield return null; }
        }
    }

    IEnumerator FadeToClear()
    {
        if (SceneFader.instance != null)
        {
            CanvasGroup fader = SceneFader.instance.fadeGroup;
            while (fader.alpha > 0) { fader.alpha -= Time.deltaTime * 2; yield return null; }
        }
    }
}