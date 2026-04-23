using UnityEngine;
using TMPro; // Importante para usar o Texto novo
using System.Collections;
using System;



public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance; // Singleton para ser acessado de qualquer lugar

    [Header("Componentes da UI")]
    public GameObject dialogueBox; // A caixa preta inteira (Pai)
    public TMP_Text dialogueText;  // O texto dentro dela (Filho)

    [Header("Configurações")]
    public float typingSpeed = 0.04f; // Velocidade da digitação (menor = mais rápido)

    public bool isDialogueActive = false;
    private string[] currentSentences; // As frases da conversa atual
    private int index = 0; // Qual frase estamos lendo agora

    private Coroutine typingCoroutine; // Guarda a memória da digitação atual
    private bool isTyping = false;     // Sabe se está digitando ou não

    private Action acaoAoTerminarDialogo;

    public GameObject continueIcon;

    [Header("Câmeras Cinematográficas")]
public Unity.Cinemachine.CinemachineCamera camAmigo;
public Unity.Cinemachine.CinemachineCamera camCacador;
public Unity.Cinemachine.CinemachineCamera camPrincipal;

void Awake()
    {
        // Em vez de destruir, ele simplesmente diz: 
        // "Eu sou o gerente desta cena atual, esqueça os anteriores!"
        instance = this; 

        // Garante que a caixa comece escondida
        dialogueBox.SetActive(false);
    }

void Update()
    {
        if (isDialogueActive && Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                // Se apertar espaço no meio da digitação, para tudo e mostra a frase inteira!
                StopCoroutine(typingCoroutine);
                dialogueText.text = currentSentences[index];
                isTyping = false;
                continueIcon.SetActive(true);
            }
            else
            {
                // Se já acabou de digitar, vai para a próxima frase normal
                NextSentence();
            }
        }
    }

public void StartDialogue(string[] sentences, Action acaoFinal = null)
    {
        acaoAoTerminarDialogo = acaoFinal;
        currentSentences = sentences;
        index = 0;
        dialogueBox.SetActive(true);
        isDialogueActive = true;

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) player.GetComponent<PlayerController>().canMove = false;

        // Inicia a primeira frase com segurança
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeSentence(currentSentences[index]));
    }

public void NextSentence()
    {
        if (index < currentSentences.Length - 1)
        {
            index++;
            dialogueText.text = ""; 
            
            // Inicia a próxima frase com segurança
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeSentence(currentSentences[index]));
        }
        else
        {
            EndDialogue();
        }
    }

    void EndDialogue()
    {
        dialogueBox.SetActive(false); // Fecha a caixa
        continueIcon.SetActive(false);
        isDialogueActive = false;

        // Se tiver alguma tarefa guardada na memória, dispara ela agora!
    acaoAoTerminarDialogo?.Invoke();
        
// --- DESCONGELA O PLAYER (Estava comentado/errado antes) ---
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            player.GetComponent<PlayerController>().canMove = true;
        }
        // ---------------------------

        // Devolve o controle para a câmera principal abaixando a prioridade
        if (camAmigo != null) camAmigo.Priority = 0;
        if (camCacador != null) camCacador.Priority = 0;
        if (camPrincipal != null) camPrincipal.Priority = 10;
    }


// Dentro do método TypeSentence ou logo antes de começar a digitar a frase
        void AlternarCamera(string frase)
{
    if (frase.StartsWith("Amigo:"))
    {
        camAmigo.Priority = 20;
        camCacador.Priority = 10;
    }
    else if (frase.StartsWith("Caçador:"))
    {
        camCacador.Priority = 20;
        camAmigo.Priority = 10;
    }
}

    // A Mágica da Máquina de Escrever
IEnumerator TypeSentence(string sentence)
    {
 
        AlternarCamera(sentence);
        isTyping = true; // Avisa que começou a digitar
        continueIcon.SetActive(false);
        dialogueText.text = "";

        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false; // Avisa que terminou
        continueIcon.SetActive(true);

        

}}