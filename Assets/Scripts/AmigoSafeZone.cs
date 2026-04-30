using UnityEngine;
using System.Collections;

public class AmigoSafeZone : MonoBehaviour
{
    [Header("Diálogo do Esporro")]
    [TextArea(2, 5)]
    [Tooltip("ATENÇÃO: O texto DEVE começar com 'Amigo:' para o DialogueManager mover a câmera!")]
    public string[] falaDoSusto;

    private bool dandoSusto = false;

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            PlayerController pc = col.GetComponent<PlayerController>();
            if (pc != null) { pc.inSafeZone = true; pc.zonaAtual = this; }
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            PlayerController pc = col.GetComponent<PlayerController>();
            if (pc != null) { pc.inSafeZone = false; pc.zonaAtual = null; }
        }
    }

    public void DarEsporro(PlayerController player)
    {
        if (dandoSusto) return;
        StartCoroutine(CutsceneSusto(player));
    }

    IEnumerator CutsceneSusto(PlayerController player)
    {
        dandoSusto = true;

        // 1. Trava o jogador
        player.SetCanMove(false);
        player.animator.SetBool("isWalking", false);

        // 2. O FALSO TIRO (O Polimento que você pediu!)
        // Toca o som, treme a câmera e pisca o fogo, mas NÃO cria o projétil.
        if (player.somTiro != null) player.somTiro.Play();
        if (CameraShake.Instance != null) CameraShake.Instance.Shake(player.shakeDuracao, player.shakeIntensidade);
        if (player.MuzzleFlash != null) StartCoroutine(PiscarFlash(player.MuzzleFlash));

        // 3. Chama o diálogo IMEDIATAMENTE (Acaba com o delayzinho)
        bool terminouFala = false;
        if (DialogueManager.instance != null)
        {
            DialogueManager.instance.StartDialogue(falaDoSusto, () => { terminouFala = true; });
        }
        else { terminouFala = true; }

        yield return new WaitUntil(() => terminouFala);

        // 4. Destrava o jogador
        player.SetCanMove(true);
        dandoSusto = false;
    }

    // Mini-rotina só pra desligar o fogo da arma rápido
    IEnumerator PiscarFlash(GameObject flash)
    {
        flash.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        flash.SetActive(false);
    }
}