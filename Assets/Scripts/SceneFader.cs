using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneFader : MonoBehaviour
{
    public static SceneFader instance; // Para acessar de qualquer lugar (Singleton)

    [Header("Referência")]
    public CanvasGroup fadeGroup; // Arraste o objeto FadeScreen aqui
    public float fadeSpeed = 3f;

    private void Awake()
    {
        // Garante que só existe UM fader no jogo e ele sobrevive entre cenas
        if (instance == null)
        {
            instance = this;
           // DontDestroyOnLoad(gameObject);  O Canvas não morre ao trocar de cena
            fadeGroup.alpha = 1f; // <--- FORÇA FICAR PRETO ASSIM QUE O JOGO NASCE
        }
        else
        {
            Destroy(gameObject); // Se já tem um, destroi o impostor
        }
    }

    private void Start()
    {
        // Começa o jogo fazendo o Fade In (Preto -> Transparente)
        StartCoroutine(FadeIn());
    }

    // Função para chamar quando quiser trocar de cena
    public void LoadScene(string sceneName)
    {
        StartCoroutine(FadeOutAndLoad(sceneName));
    }

    IEnumerator FadeIn()
    {
        fadeGroup.blocksRaycasts = true; // Bloqueia cliques enquanto clareia
        float alpha = 1f;

        while (alpha > 0f)
        {
            alpha -= Time.deltaTime * fadeSpeed;
            fadeGroup.alpha = alpha;
            yield return null; // Espera um frame
        }

        fadeGroup.alpha = 0f;
        fadeGroup.blocksRaycasts = false; // Libera o jogo
    }

    IEnumerator FadeOutAndLoad(string sceneName)
    {
        fadeGroup.blocksRaycasts = true; // Bloqueia cliques
        float alpha = 0f;

        while (alpha < 1f)
        {
            alpha += Time.deltaTime * fadeSpeed;
            fadeGroup.alpha = alpha;
            yield return null;
        }

        fadeGroup.alpha = 1f;
        
        // Agora que a tela está preta, carrega a cena
        SceneManager.LoadScene(sceneName);
        
        // Espera um pouquinho e faz o Fade In na cena nova
       // yield return new WaitForSeconds(0.5f);
        StartCoroutine(FadeIn());
    }
}