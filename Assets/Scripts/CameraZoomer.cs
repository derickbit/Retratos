using UnityEngine;
using Unity.Cinemachine; 

public class CameraZoomer : MonoBehaviour {
    public CinemachineCamera vcam; // Seu ajuste estava certíssimo aqui!
    public float zoomOut = 8f; 
    public float zoomNormal = 4.5f; 
    public float velocidade = 2f;

    private float alvoZoom;

    void Start() { alvoZoom = zoomNormal; }

    void Update() {
        // A MUDANÇA: Substituímos 'm_Lens' por 'Lens' apenas.
        vcam.Lens.OrthographicSize = Mathf.Lerp(vcam.Lens.OrthographicSize, alvoZoom, Time.deltaTime * velocidade);
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) alvoZoom = zoomOut;
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.CompareTag("Player")) alvoZoom = zoomNormal;
    }
}