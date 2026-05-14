using UnityEngine;

public class GatilhoVoltaAcampamento : MonoBehaviour
{
    public string nomeAcampamento = "Acampamento";
    public string spawnNoAcampamento = "Spawn_SaidaTrilha"; // O ID de onde ele aparece ao voltar
    
    // Referência ao gatilho do urso para saber se a perseguição já começou
    public GatilhoFuga gatilhoDoUrso; 

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Só permite voltar se o urso ainda não foi ativado (jaAtivou é do seu GatilhoFuga)
            // Se o urso já estiver correndo, o player não pode simplesmente sair da cena!
            if (gatilhoDoUrso != null && !gatilhoDoUrso.jaAtivou)
            {
                // Marca que o player está "desistindo" da caça temporariamente
                PlayerPrefs.SetInt("PlayerDesistiu", 1);
                PlayerPrefs.Save();

                // Define onde ele vai aparecer no acampamento
                Door.nextSpawnID = spawnNoAcampamento;

                if (SceneFader.instance != null)
                {
                    SceneFader.instance.LoadScene(nomeAcampamento);
                }
                else
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene(nomeAcampamento);
                }
            }
        }
    }
}