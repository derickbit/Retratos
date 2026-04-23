using UnityEngine;
using Unity.Cinemachine; 

public class CameraPriorityManager : MonoBehaviour
{
    public static CameraPriorityManager Instance;

    void Awake() 
    { 
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Função Genérica que o seu script ChamaCamera vai usar
    public void FocarCamera(CinemachineCamera novaCamera, float duracao)
    {
        StartCoroutine(RotinaFoco(novaCamera, duracao));
    }

    private System.Collections.IEnumerator RotinaFoco(CinemachineCamera cam, float tempo)
    {
        cam.Priority = 20; // Assume o controle
        yield return new WaitForSeconds(tempo);
        cam.Priority = 0;  // Devolve o controle para o Player
    }
}