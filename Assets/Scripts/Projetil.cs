using UnityEngine;

public class Projetil : MonoBehaviour
{
    public float velocidade = 25f;
    public int dano = 1;
    public float tempoDeVida = 2f; // Se errar o tiro, a bala some depois de 2 seg
    
    private Vector2 direcao;

    void Start()
    {
        Destroy(gameObject, tempoDeVida);
    }

    public void Configurar(Vector2 dir)
    {
        direcao = dir;
        // Inverte o desenho da bala se atirar pra esquerda
        if (dir.x < 0) transform.localScale = new Vector3(-1, 1, 1);
    }

    void Update()
    {
        // Faz a bala voar na direção escolhida!
        transform.Translate(direcao * velocidade * Time.deltaTime, Space.World);
    }

void OnTriggerEnter2D(Collider2D col)
    {
        // 0. IGNORA O JOGADOR! 
        if (col.CompareTag("Player")) return;

        // 1. Checa se bateu no Boss (A prioridade máxima)
        UrsoVida vida = col.GetComponent<UrsoVida>();
        if (vida != null)
        {
            vida.TomarDano(dano); // Dá o dano!
            Destroy(gameObject);  // A bala se destrói
            return;
        }

        // 1.5 O SALVADOR DA PÁTRIA: IGNORA GATILHOS INVISÍVEIS!
        // Se a bala bateu em algo que é apenas uma "área" (CameraConfiner, porta, etc), ela ignora e continua o voo.
        if (col.isTrigger) return;

        // DEDO-DURO: Só avisa se bater em algo SÓLIDO
        Debug.Log("🎯 A bala bateu em um obstáculo sólido: " + col.name);

        // 2. Se bateu em Árvores, Pedras ou Obstáculos Sólidos (Default)
        if (col.gameObject.layer == LayerMask.NameToLayer("Default") && col.name != "chão" && col.name != "Grid")
        {
            Destroy(gameObject);
        }
    }
}