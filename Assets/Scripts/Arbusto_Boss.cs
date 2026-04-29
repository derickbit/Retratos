using UnityEngine;

public class ArbustoBoss : MonoBehaviour
{
    public Animator anim;
    public AudioSource somMato;

    public void IniciarBait()
    {
        if (anim != null) anim.SetTrigger("tremer");
        if (somMato != null) somMato.Play();
    }

    public void PararBait()
    {
        // Se você usou um trigger/bool pra ele voltar pro Idle, ative aqui
        // Ex: if (anim != null) anim.SetTrigger("parar");
        if (somMato != null) somMato.Stop();
    }
}