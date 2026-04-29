using UnityEngine;
using UnityEngine.AI;

public class UrsoPerseguidor : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform player;
    private Animator animator;
    private SpriteRenderer spriteRenderer; // <-- Adicionado para virar a imagem
    private bool estaFugindo = false;
    public bool acordado = false;
    public AudioSource footstepSource;
    public float multiplicadorDePassos = 1.5f; // Ajuste para deixar os passos mais rápidos ou lentos
    private float timerPasso = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>(); // <-- Pega o componente do desenho

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
            // Pega a velocidade real do urso (magnitude linear, não a "sqrMagnitude")
            float velocidadeAtual = agent.velocity.magnitude;
            bool movendo = velocidadeAtual > 0.1f;
            animator.SetBool("isMoving", movendo);

            // --- LÓGICA DE PASSOS DINÂMICOS ---
            if (movendo && footstepSource != null)
            {
                timerPasso -= Time.deltaTime;
                
                if (timerPasso <= 0f)
                {
                    // Toca o som (com leve variação de pitch pra não ficar robótico)
                    footstepSource.pitch = Random.Range(0.55f, 0.65f); // Som grave e pesado
                    footstepSource.Play();
                    
                    // O pulo do gato: O tempo pro próximo passo diminui se a velocidade for alta!
                    timerPasso = multiplicadorDePassos / velocidadeAtual;
                }
            }
            else
            {
                timerPasso = 0f; // Zera o timer se ele parar
            }
        }
    }
}