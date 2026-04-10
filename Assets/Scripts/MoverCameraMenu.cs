using UnityEngine;

public class MoverCameraMenu : MonoBehaviour
{
    [Header("Configurações da Câmera")]
    public float velocidade = 1f;    // Quão rápido ela sobe e desce
    public float distancia = 15f;   // Quantos metros ela vai subir antes de voltar

    private float yInicial;

    void Start()
    {
        // Grava a posição de onde a câmera começou
        yInicial = transform.position.y;
    }

    void Update()
    {
        // A mágica: Mathf.PingPong vai de 0 até a 'distancia', e depois volta pra 0, num loop infinito
        float novoY = yInicial + Mathf.PingPong(Time.time * velocidade, distancia);
        
        // Aplica a nova altura na câmera, mantendo o X e o Z originais
        transform.position = new Vector3(transform.position.x, novoY, transform.position.z);
    }
}