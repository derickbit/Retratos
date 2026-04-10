using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Identificação (MUITO IMPORTANTE)")]
    [Tooltip("Digite um nome único para este gatilho. Ex: inicio, tocha, susto")]
    public string triggerID; 

    [Space(10)]
    [TextArea(3, 10)]
    public string[] sentences; 

    private void Start()
    {
        // Assim que a cena carrega, ele pergunta pro "Save" da Unity se esse ID já foi ativado (1 = sim, 0 = não)
        if (PlayerPrefs.GetInt(triggerID, 0) == 1)
        {
            // Se já foi ativado no passado, desliga o colisor IMEDIATAMENTE.
            GetComponent<Collider2D>().enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Chama a sua caixa de texto
            DialogueManager.instance.StartDialogue(sentences);
            
            // Salva no "post-it" da Unity que este gatilho específico já foi lido
            PlayerPrefs.SetInt(triggerID, 1);
            PlayerPrefs.Save();

            // Desliga o colisor pra ele não repetir nem se o boneco andar pra trás agora
            GetComponent<Collider2D>().enabled = false; 
        }
    }
}