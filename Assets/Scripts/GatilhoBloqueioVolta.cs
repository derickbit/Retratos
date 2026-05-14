using UnityEngine;
using System.Collections;

public class GatilhoBloqueioVolta : MonoBehaviour
{
    [Header("Diálogo")]
    [TextArea(2, 5)]
    public string[] falaBloqueio = { "Pensamento: Não posso voltar para o acampamento... O urso deve estar logo atrás de mim!" };
    
    [Header("Configuração")]
    public float tempoAndandoPraFrente = 0.5f;

    private bool bloqueando = false;

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!bloqueando && col.CompareTag("Player"))
        {
            bloqueando = true;
            PlayerController pc = col.GetComponent<PlayerController>();
            
            if (pc != null) 
            {
                pc.SetCanMove(false); // Trava o player na hora
                
                if (DialogueManager.instance != null)
                {
                    // Chama a fala, e quando o player fechar a caixa de texto, roda a rotina de andar
                    DialogueManager.instance.StartDialogue(falaBloqueio, () => { StartCoroutine(ForcarPasso(pc)); });
                }
            }
        }
    }

    IEnumerator ForcarPasso(PlayerController pc)
    {
        // 1. Liga a animação e vira o boneco pra direita (pra clareira)
        if (pc.animator != null) pc.animator.SetBool("isWalking", true);
        pc.transform.localScale = new Vector3(1, 1, 1);

        // 2. Faz ele dar um passo para a direita
        Vector3 posInicial = pc.transform.position;
        Vector3 posFinal = posInicial + new Vector3(1.5f, 0, 0); // Anda 1.5 metros pra direita
        
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime / tempoAndandoPraFrente;
            pc.transform.position = Vector3.Lerp(posInicial, posFinal, t);
            yield return null;
        }

        // 3. Desliga a animação, libera o player e reseta o gatilho
        if (pc.animator != null) pc.animator.SetBool("isWalking", false);
        
        pc.SetCanMove(true);
        bloqueando = false; 
    }
}