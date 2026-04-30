using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;

[System.Serializable]
public class ItemInventario
{
    public string id; 
    public Sprite icone; 
}

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    [Header("Os PAI (Movimentação na tela)")]
    public RectTransform slotEsquerda;
    public RectTransform slotCentro;
    public RectTransform slotDireita;

    [Header("Os FILHOS (Imagens)")]
    public Image iconeEsquerda;
    public Image iconeCentro;
    public Image iconeDireita;

    [Header("Banco de Dados de Imagens")]
    public Sprite spriteMaoVazia; 
    public Sprite spriteTocha; 
    public Sprite spriteEspingarda;

    [Header("Mochila (Memória Central)")]
    public List<ItemInventario> mochila = new List<ItemInventario>();
    public int indexAtual = 0; 

    [Header("Guia de Controles")]
    public TMP_Text textoControles;

    private Vector2 posEsqBase, posCenBase, posDirBase;
    private bool estaGirando = false;

    // Cores da Lente
    private Color corFoco = new Color(1f, 1f, 1f, 1f); 
    private Color corDesfoco = new Color(0.3f, 0.3f, 0.3f, 0.5f); 
    private Color corInvisivel = new Color(0.3f, 0.3f, 0.3f, 0f); 

    void Awake()
    {
        if (instance == null) 
        {
            instance = this;
            transform.SetParent(null); 
            DontDestroyOnLoad(gameObject); 
        }
        else if (instance != this)
        {
            Destroy(gameObject); 
            return; 
        }
    }

    void Start()
    {
        if (slotEsquerda != null) posEsqBase = slotEsquerda.anchoredPosition;
        if (slotCentro != null) posCenBase = slotCentro.anchoredPosition;
        if (slotDireita != null) posDirBase = slotDireita.anchoredPosition;

        if (mochila.Count == 0)
        {
            mochila.Add(new ItemInventario { id = "vazio", icone = spriteMaoVazia });
        }

        AtualizarTelas();
    }

    void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Canvas meuCanvas = GetComponent<Canvas>();
        if (meuCanvas != null)
        {
            meuCanvas.enabled = (scene.name != "MenuPrincipal");
        }
    }

    // --- FUNÇÕES DE CONSULTA (O Cérebro do Inventário) ---
    public bool VerificarSePossuiItem(string idItem)
    {
        return mochila.Exists(x => x.id == idItem);
    }

    public string GetItemAtual()
    {
        if (mochila.Count == 0) return "vazio";
        return mochila[indexAtual].id;
    }

    // --- FUNÇÕES DE MANIPULAÇÃO ---
    public void AdicionarItem(string idItem)
    {
        if (VerificarSePossuiItem(idItem)) return; 

        Sprite fotoCerta = spriteMaoVazia;
        if (idItem == "tocha") fotoCerta = spriteTocha;
        else if (idItem == "espingarda") fotoCerta = spriteEspingarda;

        mochila.Add(new ItemInventario { id = idItem, icone = fotoCerta });
        AtualizarTelas();
    }

    public void RemoverItem(string idItem)
    {
        for (int i = 0; i < mochila.Count; i++)
        {
            if (mochila[i].id == idItem)
            {
                mochila.RemoveAt(i);
                break;
            }
        }
        indexAtual = 0; // Volta pra mão vazia por segurança
        AtualizarTelas();
    }

    public void ForcarEquipar(string idDesejado)
    {
        for (int i = 0; i < mochila.Count; i++)
        {
            if (mochila[i].id == idDesejado) 
            { 
                indexAtual = i; 
                AtualizarTelas(); 
                break; 
            }
        }
    }

    public void ResetarInventario()
    {
        mochila.Clear(); 
        mochila.Add(new ItemInventario { id = "vazio", icone = spriteMaoVazia }); 
        indexAtual = 0;  
        AtualizarTelas();
    }

    // --- UI E ANIMAÇÕES ---
    public void AtualizarTelas()
    {
        if (mochila.Count == 0) return;

        Image bgCen = slotCentro.GetComponent<Image>();
        Image bgEsq = slotEsquerda.GetComponent<Image>();
        Image bgDir = slotDireita.GetComponent<Image>();

        slotEsquerda.anchoredPosition = posEsqBase;
        slotCentro.anchoredPosition = posCenBase;
        slotDireita.anchoredPosition = posDirBase;

        slotCentro.SetAsLastSibling();
        slotCentro.localScale = Vector3.one * 1.2f; 
        slotEsquerda.localScale = Vector3.one * 0.7f; 
        slotDireita.localScale = Vector3.one * 0.7f;

        iconeCentro.sprite = mochila[indexAtual].icone;
        iconeCentro.color = corFoco; 
        if (bgCen != null) bgCen.color = corFoco;

        if (mochila.Count <= 1)
        {
            iconeEsquerda.color = corInvisivel;
            iconeDireita.color = corInvisivel;
            if (bgEsq != null) bgEsq.color = corInvisivel;
            if (bgDir != null) bgDir.color = corInvisivel;
        }
        else
        {
            int indexEsq = (indexAtual - 1 + mochila.Count) % mochila.Count;
            int indexDir = (indexAtual + 1) % mochila.Count;

            iconeEsquerda.sprite = mochila[indexEsq].icone;
            iconeEsquerda.color = corDesfoco;
            if (bgEsq != null) bgEsq.color = corDesfoco;

            iconeDireita.sprite = mochila[indexDir].icone;
            iconeDireita.color = corDesfoco;
            if (bgDir != null) bgDir.color = corDesfoco;
        }

        AtualizarTextoControles();
    }

    private void AtualizarTextoControles()
    {
        if (textoControles == null) return;

        string item = mochila[indexAtual].id;
        if (item == "tocha") textoControles.text = "[Q/Scroll] Trocar\n[F] Acender\n[G] Largar";
        else if (item == "espingarda") textoControles.text = "[Q/Scroll] Trocar\n[F/Click] Atirar\n[G] Largar";
        else textoControles.text = "[Q/Scroll] Trocar";
    }

    public void CiclarInventario()
    {
        if (estaGirando || mochila.Count <= 1) return;

        indexAtual--;
        if (indexAtual < 0) indexAtual = mochila.Count - 1;

        int indexEsq = (indexAtual - 1 + mochila.Count) % mochila.Count;
        int indexDir = (indexAtual + 1) % mochila.Count;

        StartCoroutine(AnimarGiro(mochila[indexEsq].icone, mochila[indexAtual].icone, mochila[indexDir].icone));
    }

    IEnumerator AnimarGiro(Sprite fotoEsq, Sprite fotoCen, Sprite fotoDir)
    {
        estaGirando = true;
        slotEsquerda.SetAsLastSibling();

        GameObject fantasmaObj = Instantiate(slotDireita.gameObject, slotDireita.parent);
        fantasmaObj.transform.SetAsFirstSibling(); 
        
        RectTransform fantasmaRect = fantasmaObj.GetComponent<RectTransform>();
        Image fantasmaImg = fantasmaObj.transform.GetChild(0).GetComponent<Image>(); 
        Image bgFantasma = fantasmaObj.GetComponent<Image>(); 

        fantasmaRect.anchoredPosition = posDirBase;
        fantasmaImg.sprite = iconeDireita.sprite; 

        Vector2 posEscondidaEsquerda = posEsqBase + new Vector2(-150, 0); 
        Vector2 posEscondidaDireita = posDirBase + new Vector2(150, 0);

        slotDireita.anchoredPosition = posEscondidaEsquerda;
        iconeDireita.sprite = fotoEsq; 
        iconeDireita.color = corInvisivel;
        
        Image bgDir = slotDireita.GetComponent<Image>();
        if (bgDir != null) bgDir.color = corInvisivel;

        slotDireita.localScale = Vector3.one * 0.5f;

        Image bgCen = slotCentro.GetComponent<Image>();
        Image bgEsq = slotEsquerda.GetComponent<Image>();

        float tempo = 0f;
        while (tempo < 1f)
        {
            tempo += Time.deltaTime / 0.25f;
            if (tempo > 1f) tempo = 1f; 

            float curva = Mathf.SmoothStep(0, 1, tempo);

            slotCentro.anchoredPosition = Vector2.Lerp(posCenBase, posDirBase, curva);
            slotCentro.localScale = Vector3.Lerp(Vector3.one * 1.2f, Vector3.one * 0.7f, curva);
            iconeCentro.color = Color.Lerp(corFoco, corDesfoco, curva);
            if (bgCen != null) bgCen.color = Color.Lerp(corFoco, corDesfoco, curva);

            slotEsquerda.anchoredPosition = Vector2.Lerp(posEsqBase, posCenBase, curva);
            slotEsquerda.localScale = Vector3.Lerp(Vector3.one * 0.7f, Vector3.one * 1.2f, curva);
            iconeEsquerda.color = Color.Lerp(corDesfoco, corFoco, curva);
            if (bgEsq != null) bgEsq.color = Color.Lerp(corDesfoco, corFoco, curva);

            slotDireita.anchoredPosition = Vector2.Lerp(posEscondidaEsquerda, posEsqBase, curva);
            slotDireita.localScale = Vector3.Lerp(Vector3.one * 0.5f, Vector3.one * 0.7f, curva);
            iconeDireita.color = Color.Lerp(corInvisivel, corDesfoco, curva);
            if (bgDir != null) bgDir.color = Color.Lerp(corInvisivel, corDesfoco, curva);

            if (fantasmaRect != null)
            {
                fantasmaRect.anchoredPosition = Vector2.Lerp(posDirBase, posEscondidaDireita, curva);
                fantasmaRect.localScale = Vector3.Lerp(Vector3.one * 0.7f, Vector3.one * 0.5f, curva);
                fantasmaImg.color = Color.Lerp(corDesfoco, corInvisivel, curva);
                if (bgFantasma != null) bgFantasma.color = Color.Lerp(corDesfoco, corInvisivel, curva);
            }
            yield return null;
        }

        Destroy(fantasmaObj);

        RectTransform tempSlot = slotDireita;
        slotDireita = slotCentro;
        slotCentro = slotEsquerda;
        slotEsquerda = tempSlot;

        Image tempImg = iconeDireita;
        iconeDireita = iconeCentro;
        iconeCentro = iconeEsquerda;
        iconeEsquerda = tempImg;

        AtualizarTelas();
        estaGirando = false;
    }
}