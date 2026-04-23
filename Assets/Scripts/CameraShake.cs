using UnityEngine;
using Unity.Cinemachine; // Importante para Unity 6

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;
    private CinemachineBasicMultiChannelPerlin noise;
    private float timerTotal;
    private float timerPassado;
    private float intensidadeInicial;

void Awake()
    {
        Instance = this;
        noise = GetComponent<CinemachineBasicMultiChannelPerlin>();
        
        if (noise == null)
        {
            Debug.LogError("Tech Lead Avisa: O componente Noise não foi encontrado nesta câmera!");
        }
    }

    public void Shake(float duracao, float intensidade)
    {
        if (noise == null) return;
        
        noise.AmplitudeGain = intensidade;
        intensidadeInicial = intensidade;
        timerTotal = duracao;
        timerPassado = duracao;
    }

    void Update()
    {
        if (timerPassado > 0)
        {
            timerPassado -= Time.deltaTime;
            // Faz o tremor diminuir suavemente até parar
            noise.AmplitudeGain = Mathf.Lerp(0, intensidadeInicial, timerPassado / timerTotal);
        }
    }
}