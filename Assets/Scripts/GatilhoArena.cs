using UnityEngine;

public class GatilhoArena : MonoBehaviour
{
    public GameObject barreiraGalhos; 
    public AudioSource somMadeiraQuebrando; 
    
    // NOVO: Link para o Cérebro do Boss
    public BossManager cerebroBoss; 

    private bool arenaAtiva = false;

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!arenaAtiva && col.CompareTag("Player"))
        {
            arenaAtiva = true;
            if (barreiraGalhos != null) barreiraGalhos.SetActive(true); 
            if (somMadeiraQuebrando != null) somMadeiraQuebrando.Play();
            
            Debug.Log("A luta começou!");

            // APERTA O "PLAY" NA BOSS FIGHT!
            if (cerebroBoss != null)
            {
                cerebroBoss.ComecarLuta();
            }
        }
    }
}