using UnityEngine;
using UnityEngine.AI;

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
    public float tempoEntrePassos = 0.4f; // Tempo FIXO entre cada pisada (Ajuste para deixar mais rápido ou lento)
    private float timerPasso = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>(); 

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
        if (agent.isOnNavMesh && player != null && !estaFugindo && acordado)
        {
            agent.SetDestination(player.position);
            
            if (agent.velocity.x > 0.1f) spriteRenderer.flipX = true;
            else if (agent.velocity.x < -0.1f) spriteRenderer.flipX = false;
        }

        if (agent.isOnNavMesh)
        {
            // Pega a velocidade real do urso
            float velocidadeAtual = agent.velocity.magnitude;
            bool movendo = velocidadeAtual > 0.1f;
            animator.SetBool("isMoving", movendo);

            // --- LÓGICA DE PASSOS CORRIGIDA (CRONÔMETRO FIXO) ---
            if (movendo && footstepSource != null)
            {
                timerPasso -= Time.deltaTime;
                
                if (timerPasso <= 0f)
                {
                    // Toca o som (com leve variação de pitch pra não ficar robótico)
                    footstepSource.pitch = Random.Range(0.55f, 0.65f); // Som grave e pesado
                    footstepSource.Play();
                    
                    // Reseta o cronômetro com o tempo fixo (acaba com o bug do silêncio)
                    timerPasso = tempoEntrePassos;
                }
            }
            else
            {
                // Zera o timer. Assim, quando ele voltar a correr, o primeiro passo toca imediatamente!
                timerPasso = 0f; 
            }
        }
    }
}