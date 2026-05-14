using UnityEngine;
using System.Collections;

public class ArbustoBoss : MonoBehaviour
{
    public Animator anim;
    public AudioSource somMato;
    private bool tremendo = false;

    public void IniciarBait()
    {
        tremendo = true;
        if (somMato != null) somMato.Play();
        StartCoroutine(LoopDeTremer());
    }

    public void PararBait()
    {
        tremendo = false;
        if (somMato != null) somMato.Stop();
    }

    IEnumerator LoopDeTremer()
    {
        // Fica mandando a árvore tremer repetidas vezes até mandarem parar
        while (tremendo)
        {
            if (anim != null) anim.SetTrigger("tremer");
            yield return new WaitForSeconds(0.3f); 
        }
    }
}