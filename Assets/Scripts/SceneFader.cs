using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneFader : MonoBehaviour
{
    public static SceneFader instance; 

    [Header("Referência")]
    public CanvasGroup fadeGroup; // Arraste o objeto FadeScreen aqui
    public float fadeSpeed = 3f;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            // O Canvas nasce preto para esconder o carregamento da cena
            fadeGroup.alpha = 1f; 
        }
        else
        {
            Destroy(gameObject); 
        }
    }

    private void Start()
    {
        // Começa o jogo fazendo o Fade In (Preto -> Transparente)
        StartCoroutine(FadeIn());
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(FadeOutAndLoad(sceneName));
    }

    IEnumerator FadeIn()
    {
        fadeGroup.blocksRaycasts = true; 
        float alpha = 1f;

        while (alpha > 0f)
        {
            alpha -= Time.deltaTime * fadeSpeed;
            fadeGroup.alpha = alpha;
            yield return null; 
        }

        fadeGroup.alpha = 0f;
        fadeGroup.blocksRaycasts = false; 
    }

    IEnumerator FadeOutAndLoad(string sceneName)
    {
        fadeGroup.blocksRaycasts = true; 
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
    }
}