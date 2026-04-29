using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class ArbustoSaida : MonoBehaviour
{
    [Header("Configurações da Saída")]
    public string cenaDaFloresta = "Floresta_v1"; 
    public float tempoDoPulo = 0.8f;   // Quão rápido é o mergulho na moita?
    public float alturaDoPulo = 1.5f;  // Quão alto ele voa antes de cair?

    public void PularNoMato(PlayerController player)
    {
        StartCoroutine(AnimarECarregar(player));
    }

    IEnumerator AnimarECarregar(PlayerController player)
    {
        // 1. Trava o boneco fisicamente
        player.SetCanMove(false);
        
        // 2. Avisa o Animator para tocar a animação (ex: o encolhimento do Scale)
        if (player.animator != null) 
        {
            player.animator.SetTrigger("PularMato");
        }

        // 3. Pega as coordenadas para a matemática
        Vector3 posInicial = player.transform.position;
        Vector3 posFinal = transform.position; // Vai exatamente pro centro desta moita
        
        float tempoAtual = 0f;

        // 4. O Loop do Pulo!
        while (tempoAtual < tempoDoPulo)
        {
            tempoAtual += Time.deltaTime;
            float progresso = tempoAtual / tempoDoPulo; // Vai de 0.0 a 1.0

            // A MÁGICA DO SENO: Cria o formato do arco
            float arcoY = Mathf.Sin(progresso * Mathf.PI) * alturaDoPulo;

            // Move o boneco em linha reta (Lerp) + O arco no eixo Y
            player.transform.position = Vector3.Lerp(posInicial, posFinal, progresso) + new Vector3(0, arcoY, 0);

            yield return null; // Espera o próximo frame
        }

        // Garante que ele encostou no centro exato da moita no final
        player.transform.position = posFinal;

        // 5. Carrega a próxima cena pelo seu sistema oficial de Fade!
        if (SceneFader.instance != null) {
            SceneFader.instance.LoadScene(cenaDaFloresta);
        } else {
            SceneManager.LoadScene(cenaDaFloresta);
        }
    }
}