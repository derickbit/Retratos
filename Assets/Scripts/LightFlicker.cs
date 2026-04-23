using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightFlicker : MonoBehaviour
{
    public Light2D luzFogueira;
    public float minIntensity = 0.3f;
    public float maxIntensity = 1.8f;
    [Range(1, 30)] public float velocidadeTremor = 5f; 
    [Range(0f, 1f)] public float tempoDePausa = 0.1f; // NOVO: Tempo que ele segura a luz antes de piscar de novo

    private float intensidadeAlvo;
    private float intensidadeAtual;
    private float timerPausa;

    void Start()
    {
        if (luzFogueira == null) luzFogueira = GetComponent<Light2D>();
        if (luzFogueira != null) intensidadeAtual = luzFogueira.intensity;
        SortearNovoAlvo();
    }

    void Update()
    {
        if (luzFogueira == null) return;

        // Se está no tempo de pausa, só desconta o relógio e não faz nada
        if (timerPausa > 0)
        {
            timerPausa -= Time.deltaTime;
            return;
        }

        intensidadeAtual = Mathf.Lerp(intensidadeAtual, intensidadeAlvo, Time.deltaTime * velocidadeTremor);
        luzFogueira.intensity = intensidadeAtual;

        if (Mathf.Abs(intensidadeAtual - intensidadeAlvo) < 0.05f)
        {
            SortearNovoAlvo();
            timerPausa = tempoDePausa; // Inicia a pausa
        }
    }

    void SortearNovoAlvo()
    {
        intensidadeAlvo = Random.Range(minIntensity, maxIntensity);
    }
}