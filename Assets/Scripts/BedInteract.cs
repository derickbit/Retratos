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
    
    [Header("Pontos Fixos (Spawns/Drops)")]
    public Transform pontoDeAcordar;   // Onde o player vai aparecer de manhã
    public Transform pontoDeDropTocha; // Onde a tocha vai cair
    public Transform pontoDeDropArma;  // Onde a arma vai cair

    [Header("Diálogos")]
    [TextArea] public string[] perguntaDormir; 
    [TextArea] public string[] textoAcordar;   

    [Header("Áudio")]
    public AudioSource somSusto; 
    public AudioSource somAmbiente; 
    public AudioClip somPassarinhos; 

    [Header("Fim do Jogo")]
    public GameObject painelFim;
    public string triggerID = "interacao_cama"; 

    private bool esperandoConfirmacao = false;

    private void Start()
    {
        if (PlayerPrefs.GetInt(triggerID, 0) == 1)
        {
            esperandoConfirmacao = true;
        }
    }

    public void TentarDormir()
    {
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
        
        PlayerPrefs.SetInt(triggerID, 1);
        PlayerPrefs.Save();
        
        yield return new WaitForSeconds(3f);
    }

  IEnumerator SequenciaDormir()
    {
        PlayerController pc = playerDePe.GetComponent<PlayerController>();

        // 1. ORGANIZA OS DROPS DIRETAMENTE NOS PONTOS (Olhando direto para o Inventário)
        if (pc != null) 
        {
            // CORREÇÃO DA TOCHA: Pergunta direto para a mochila se ela existe!
            bool temTochaNaMochila = InventoryManager.instance != null && InventoryManager.instance.VerificarSePossuiItem("tocha");
            if (temTochaNaMochila)
            {
                pc.DroparTocha(false, pontoDeDropTocha); 
            }

            // CORREÇÃO DA ARMA: Pergunta direto para a mochila se ela existe!
            bool temArmaNaMochila = InventoryManager.instance != null && InventoryManager.instance.VerificarSePossuiItem("espingarda");
            if (temArmaNaMochila)
            {
                pc.DroparEspingarda(false, pontoDeDropArma);
            }

            // Agora teleporta o player definitivamente para o lugar de acordar seguro
            if (pontoDeAcordar != null)
            {
                pc.transform.position = pontoDeAcordar.position;
            }

            pc.canMove = false;
        }

        // 2. Esconde o jogador e a sombra da cena
        playerDePe.GetComponent<SpriteRenderer>().enabled = false;
        UnityEngine.Rendering.Universal.ShadowCaster2D sombra = playerDePe.GetComponent<UnityEngine.Rendering.Universal.ShadowCaster2D>();
        if (sombra != null) sombra.enabled = false;

        // 3. Muda a arte da cama
        if (bedRenderer != null && camaOcupada != null) bedRenderer.sprite = camaOcupada;

        yield return new WaitForSeconds(1.5f);
        yield return StartCoroutine(FadeToBlack());

        // --- A MÁGICA NO ESCURO ---
        propsNoite.SetActive(false); 
        propsDia.SetActive(true);    
        
        // 4. GARANTE QUE O FOGO INTERNO ESTEJA APAGADO
        if (pc != null)
        {
            pc.isTorchLit = false;
            pc.animator.SetBool("isLit", false);
            if (pc.torchLightObject != null) pc.torchLightObject.SetActive(false);
        }

        // 5. FORÇA A MEMÓRIA A SABER QUE O FOGO APAGOU PRA SEMPRE
        PlayerPrefs.SetInt("TochaAcesa", 0);
        PlayerPrefs.Save();

        // 6. APAGA AS TOCHAS DO CHÃO (As antigas que já estavam no mapa)
        TorchItem[] todasAsTochas = Object.FindObjectsByType<TorchItem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        GameObject prefabApagada = null;
        if (pc != null) prefabApagada = pc.tochaApagadaPrefab;

        foreach (TorchItem tocha in todasAsTochas)
        {
            if (tocha.estavaAcesa)
            {
                if (prefabApagada != null)
                {
                    Instantiate(prefabApagada, tocha.transform.position, tocha.transform.rotation);
                }
                Destroy(tocha.gameObject);
            }
        }

        // MUDANÇA DE ÁUDIO
        if (somAmbiente != null && somPassarinhos != null)
        {
            somAmbiente.Stop(); 
            somAmbiente.clip = somPassarinhos; 
            somAmbiente.loop = true; 
            somAmbiente.Play(); 
        }

        yield return StartCoroutine(FadeToClear());
        if (somSusto != null) somSusto.Play();
        yield return new WaitForSeconds(2f);

        // O PLAYER ACORDA!
        bedRenderer.sprite = camaVazia;
        playerDePe.GetComponent<SpriteRenderer>().enabled = true;
        if (sombra != null) sombra.enabled = true;
        
        yield return StartCoroutine(FadeToClear());

        DialogueManager.instance.StartDialogue(textoAcordar);
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