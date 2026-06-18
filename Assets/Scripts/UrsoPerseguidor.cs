using UnityEngine;
using UnityEngine.AI;
using System.Collections; // Necessário para a Coroutine

public class UrsoPerseguidor : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform player;
    private Animator animator;
    private SpriteRenderer spriteRenderer; 
    private bool estaFugindo = false;
    public bool acordado = false;
    
    [Header("Áudio e Passos")]
    public AudioSource footstepSource;
    public float tempoEntrePassos = 0.4f; 
    private float timerPasso = 0f;

    [Header("Reação ao Tiro (Atordoamento)")]
    public Color corDano = Color.red;
    public float tempoAtordoado = 1.5f; // Segundos que o urso fica parado ao levar tiro
    public float forcaKnockback = 0.5f; // O tranco pra trás
    private Color corOriginal = Color.white;
    private bool atordoado = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>(); 
        
        if (spriteRenderer != null) corOriginal = spriteRenderer.color;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    void Update()
    {
        // Se o urso levou um tiro, ele congela e ignora tudo abaixo!
        if (atordoado) return;

        if (agent.isOnNavMesh && player != null && !estaFugindo && acordado)
        {
            agent.SetDestination(player.position);
            
            if (agent.velocity.x > 0.1f) spriteRenderer.flipX = true;
            else if (agent.velocity.x < -0.1f) spriteRenderer.flipX = false;
        }

        if (agent.isOnNavMesh)
        {
            float velocidadeAtual = agent.velocity.magnitude;
            bool movendo = velocidadeAtual > 0.1f;
            animator.SetBool("isMoving", movendo);

            if (movendo && footstepSource != null)
            {
                timerPasso -= Time.deltaTime;
                
                if (timerPasso <= 0f)
                {
                    footstepSource.pitch = Random.Range(0.55f, 0.65f); 
                    footstepSource.Play();
                    timerPasso = tempoEntrePassos;
                }
            }
            else
            {
                timerPasso = 0f; 
            }
        }
    }

    // A função que o seu Projetil vai chamar!
    public void TomarDano(int quantidade)
    {
        // Só toma dano se já não estiver atordoado e se a perseguição já começou
        if (!atordoado && acordado) 
        {
            StartCoroutine(RotinaAtordoamento());
        }
    }

    IEnumerator RotinaAtordoamento()
    {
        atordoado = true;

        // 1. Para o Urso imediatamente
        if (agent != null && agent.isOnNavMesh) agent.isStopped = true;
        if (animator != null) animator.SetBool("isMoving", false);

        // 2. Dá o empurrão pra trás (Knockback)
        if (player != null) {
            Vector3 direcao = (transform.position - player.position).normalized;
            transform.position += direcao * forcaKnockback;
        }

        // 3. Pisca Vermelho
        if (spriteRenderer != null) spriteRenderer.color = corDano;

        // 4. Fica paralisado pelo tempo definido
        yield return new WaitForSeconds(tempoAtordoado);

        // 5. Acorda e volta a correr atrás do player!
        if (spriteRenderer != null) spriteRenderer.color = corOriginal;
        if (agent != null && agent.isOnNavMesh) agent.isStopped = false;
        atordoado = false;
    }
}