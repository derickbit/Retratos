using UnityEngine;
using System.Collections;

public class UrsoVida : MonoBehaviour
{
    [Header("Status do Boss")]
    public int vidaAtual = 5; // Como ele foge em 5 tiros, a "vida útil" dessa fase é 5!

    [Header("Barra Flutuante (Pixel Art)")]
    public GameObject objetoBarraVida; // Arraste o objeto "BarraDeVida_Sprite" aqui
    public SpriteRenderer spriteDaBarra; // Arraste o mesmo objeto aqui
    public Sprite[] spritesDeVida; // Onde vamos colocar os 6 desenhos (do 0 ao 5)
    public float tempoVisivel = 2f; // Quanto tempo a barra fica na tela após o tiro

    [Header("Visual e Juice")]
    private SpriteRenderer srUrso;
    private Color corOriginal;
    public Color corDano = Color.red;

    [Header("Sprite do Choro")]
    public Sprite spriteChorando; // Arraste seu sprite dele chorando aqui!
    private Animator animUrso;
    
    // Adicione a referência pro Cérebro do Boss
    public BossManager cerebroBoss; // Vamos criar esse script agora!

    private bool jaFugiu = false;

void Start()
    {
        srUrso = GetComponent<SpriteRenderer>();
        animUrso = GetComponent<Animator>(); // Pega o Animator
        corOriginal = srUrso.color;
    }

 public void TomarDano(int quantidade)
    {
        vidaAtual -= quantidade;
        
        // Avisa o cérebro IMEDIATAMENTE para ele fugir, sem esperar!
        if (vidaAtual <= 0) {
            if (cerebroBoss != null) cerebroBoss.UrsoDerrotado();
        } else {
            if (cerebroBoss != null) cerebroBoss.RecuarParaMato();
        }

        // Roda o piscar vermelho em paralelo
        StartCoroutine(EfeitoDano());
    }

    IEnumerator MostrarBarraFlutuante()
    {
        // Troca a imagem para a quantidade de vida atual
        if (spritesDeVida.Length > vidaAtual)
        {
            spriteDaBarra.sprite = spritesDeVida[vidaAtual];
        }

        // Liga a barra
        objetoBarraVida.SetActive(true);

        // Espera 2 segundos
        yield return new WaitForSeconds(tempoVisivel);

        // Se ele não fugiu, desliga a barra de novo
        if (!jaFugiu) objetoBarraVida.SetActive(false);
    }

// E substitua o IEnumerater EfeitoDano por este:
IEnumerator EfeitoDano()
    {
        // Desliga as garras pro urso não te matar enquanto foge chorando
        UrsoAtaque ataque = GetComponent<UrsoAtaque>();
        if (ataque != null) ataque.enabled = false; 

        // Trava a animação de corrida pra forçar a mostrar o sprite de choro
        if (animUrso != null) animUrso.enabled = false;
        
        srUrso.sprite = spriteChorando;
        srUrso.color = corDano;

        // O Urso continua correndo pro arbusto durante esse 0.5s!
        yield return new WaitForSeconds(0.5f);

        srUrso.color = corOriginal;
        if (animUrso != null) animUrso.enabled = true;
        if (ataque != null) ataque.enabled = true; 
    }

    void IniciarFuga()
    {
        Debug.Log("O URSO SENTIU A BALA! Correndo pro arbusto!");
        // AQUI VAMOS CHAMAR O URSO PERSEGUIDOR PARA MUDAR O DESTINO DELE!
    }
}