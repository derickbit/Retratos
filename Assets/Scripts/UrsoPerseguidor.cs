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
            
            // --- NOVA LÓGICA DE VIRAR O SPRITE ---
            // agent.velocity.x nos diz a direção: positivo = direita, negativo = esquerda
            if (agent.velocity.x > 0.1f)
            {
                // Se a arte original do seu urso olha para a esquerda, true faz ele olhar pra direita
                spriteRenderer.flipX = true; 
            }
            else if (agent.velocity.x < -0.1f)
            {
                spriteRenderer.flipX = false;
            }
            // -------------------------------------
        }

// Adicione esta variável lá no topo do script:
// public AudioSource footstepSource; // Arraste o AudioSource do urso aqui

if (agent.isOnNavMesh)
{
    float velocidadeAtual = agent.velocity.sqrMagnitude;
    bool movendo = velocidadeAtual > 0.1f;
    animator.SetBool("isMoving", movendo);

    // Toca os passos pesados do urso
    if (movendo && footstepSource != null && !footstepSource.isPlaying)
    {
        footstepSource.Play();
    }
    else if (!movendo && footstepSource != null)
    {
        footstepSource.Stop();
    }
}
    }
}