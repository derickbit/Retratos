using UnityEngine;

public class AvisoDeTempo : MonoBehaviour
{
    public static AvisoDeTempo instance; // Singleton para evitar duplicatas

    [Header("Configurações de Tempo")]
    public float tempoParaAvisar = 100f; 
    private float tempoDecorrido = 0f;
    private bool jaAvisou = false;

    [Header("O que ele vai dizer?")]
    [TextArea(2, 5)]
    public string[] frasesDeSono;

    void Awake()
    {
        // Sistema para garantir que só exista UM timer rodando no jogo todo
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject); // A MÁGICA: Ele não morre na troca de cena
        } else {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (!jaAvisou)
        {
            tempoDecorrido += Time.deltaTime;

            if (tempoDecorrido >= tempoParaAvisar)
            {
                jaAvisou = true;
                
                // Só dispara o diálogo se o DialogueManager existir na cena atual
                if (DialogueManager.instance != null)
                {
                    DialogueManager.instance.StartDialogue(frasesDeSono);
                }
            }
        }
    }

    // Função para resetar o timer se o jogador começar um Novo Jogo
    public void ResetarTimer()
    {
        tempoDecorrido = 0f;
        jaAvisou = false;
    }
}