using UnityEngine;
using System.Collections;

public class ArbustoSaida : MonoBehaviour
{
    public string cenaParaCarregar = "Floresta"; 
    public string idPontoDeSpawn = "Spawn_SaidaCombate";
    public float tempoDeCaminhada = 0.8f;   
    public int ordemDeCamadaAtrasDaMoita = -10; 

    public void PularNoMato(PlayerController player)
    {
        StartCoroutine(EntrarNoMatoECarregar(player));
    }

    IEnumerator EntrarNoMatoECarregar(PlayerController player)
    {
        player.SetCanMove(false);
        SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
        
        // Liga a animação de andar
        if (player.animator != null) 
        {
            player.animator.SetBool("isWalking", true);
        }

        if (sr != null) 
        {
            sr.flipX = false; // Garante que ele olhe pra moita
            sr.sortingOrder = ordemDeCamadaAtrasDaMoita; // Joga pra trás da folhagem
        }

        Vector3 posInicial = player.transform.position;
        Vector3 posFinal = transform.position; 
        posFinal.z = posInicial.z; // Trava o eixo Z pra evitar bugs
        
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / tempoDeCaminhada;
            player.transform.position = Vector3.Lerp(posInicial, posFinal, t);
            yield return null;
        }

        // Desliga a animação e SOME com o boneco para não piscar
        if (player.animator != null) player.animator.SetBool("isWalking", false);
        player.gameObject.SetActive(false);
        Door.nextSpawnID = idPontoDeSpawn;

        // Chama a tela preta
        if (SceneFader.instance != null) 
        {
            SceneFader.instance.LoadScene(cenaParaCarregar);
        } 
        else 
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(cenaParaCarregar);
        }
    }
}