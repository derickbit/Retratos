using UnityEngine;
using System.Collections;

public class UrsoVida : MonoBehaviour
{
    [Header("Status do Boss")]
    public int vidaAtual = 5;

    [Header("Barra Flutuante")]
    public GameObject objetoBarraVida; 
    public SpriteRenderer spriteDaBarra; 
    public Sprite[] spritesDeVida; 
    public float tempoVisivelBarra = 1.5f; // Tempo que a barra fica na tela

    [Header("Visual e Juice")]
    private SpriteRenderer srUrso;
    private Color corOriginal = Color.white;
    public Color corDano = Color.red;
    public Sprite spriteChorando;
    public float forcaKnockback = 0.5f; 
    private Animator animUrso;
    
    public BossManager cerebroBoss; 
    private Transform playerTransform;
    
    private Coroutine rotinaBarra;
    private Coroutine rotinaSusto;

    void Awake()
    {
        srUrso = GetComponent<SpriteRenderer>();
        if (srUrso != null) corOriginal = srUrso.color;
    }

    void Start()
    {
        animUrso = GetComponent<Animator>();
        if (objetoBarraVida != null) objetoBarraVida.SetActive(false);

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) playerTransform = p.transform;
    }

    public void TomarDano(int quantidade)
    {
        if (vidaAtual <= 0) return;

        vidaAtual -= quantidade;
        if (vidaAtual < 0) vidaAtual = 0; 
        
        if (vidaAtual <= 0) {
            if (cerebroBoss != null) cerebroBoss.UrsoDerrotado();
        } else {
            if (cerebroBoss != null) cerebroBoss.RecuarParaMato();
        }

        // Para qualquer animação de dano anterior para não bugar
        if (rotinaBarra != null) StopCoroutine(rotinaBarra);
        if (rotinaSusto != null) StopCoroutine(rotinaSusto);
        
        ResetarSusto(); // Descongela a perna antes de tomar outro tiro

        rotinaBarra = StartCoroutine(MostrarBarra());
        rotinaSusto = StartCoroutine(EfeitoSusto());
    }

    IEnumerator MostrarBarra()
    {
        if (objetoBarraVida != null) {
            objetoBarraVida.SetActive(true);
            if (vidaAtual >= 0 && vidaAtual < spritesDeVida.Length)
                spriteDaBarra.sprite = spritesDeVida[vidaAtual];
        }
        yield return new WaitForSeconds(tempoVisivelBarra);
        if (objetoBarraVida != null) objetoBarraVida.SetActive(false);
    }

    IEnumerator EfeitoSusto()
    {
        // 1. Solavanco rápido pra trás
        if (playerTransform != null) {
            Vector3 direcao = (transform.position - playerTransform.position).normalized;
            transform.position += direcao * forcaKnockback;
        }

        // 2. Congela o urso na pose de choro por uma FRAÇÃO de segundo
        if (animUrso != null) animUrso.enabled = false;
        if (srUrso != null) {
            srUrso.sprite = spriteChorando;
            srUrso.color = corDano;
        }

        // SUSTO RÁPIDO: 0.2s não deixa ele deslizar congelado!
        yield return new WaitForSeconds(0.2f); 

        ResetarSusto();
    }

    void ResetarSusto()
    {
        if (srUrso != null) srUrso.color = corOriginal;
        if (animUrso != null) animUrso.enabled = true; // Volta a correr instantaneamente
    }

    void OnDisable()
    {
        ResetarSusto();
        if (objetoBarraVida != null) objetoBarraVida.SetActive(false);
    }
}