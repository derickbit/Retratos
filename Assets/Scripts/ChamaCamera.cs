using UnityEngine;
using Unity.Cinemachine;
using System.Collections; // <-- Precisamos disso para a Coroutine

public class ChamaCamera : MonoBehaviour
{
    [Header("Configurações de Alvo")]
    public CinemachineCamera cameraAlvo;
    public float tempoFoco = 2.5f; 
    
    [Header("Configurações de Movimento")]
    public float velocidadeDeIda = 2.0f; 
    public float velocidadeDeVolta = 1.0f; 

    [Header("Fluxo")]
    public bool destravarPlayerAoFinal = false;

    public void DispararCamera()
    {
        Debug.Log("🎬 [ChamaCamera] Ação iniciada! Indo para: " + (cameraAlvo != null ? cameraAlvo.name : "NENHUMA"));
        StartCoroutine(RotinaCameraBlindada());
    }

    private IEnumerator RotinaCameraBlindada()
    {
        // 1. Ida
        if (Camera.main != null)
        {
            CinemachineBrain brain = Camera.main.GetComponent<CinemachineBrain>();
            if (brain != null)
            {
                CinemachineBlendDefinition blend = brain.DefaultBlend;
                blend.Time = velocidadeDeIda;
                brain.DefaultBlend = blend;
            }
        }

        if (CameraPriorityManager.Instance != null && cameraAlvo != null)
        {
            CameraPriorityManager.Instance.FocarCamera(cameraAlvo, tempoFoco);
        }

        // 2. O Trator espera o tempo exato (Ida + Foco)
        yield return new WaitForSeconds(velocidadeDeIda + tempoFoco);

        // 3. Volta
        Debug.Log("🎥 [ChamaCamera] Tempo de foco acabou. Preparando a volta...");
        if (Camera.main != null)
        {
            CinemachineBrain brain = Camera.main.GetComponent<CinemachineBrain>();
            if (brain != null) 
            {
                CinemachineBlendDefinition blend = brain.DefaultBlend;
                blend.Time = velocidadeDeVolta;
                brain.DefaultBlend = blend;
            }
        }

        // 4. O Trator espera a câmera pousar no player
        yield return new WaitForSeconds(velocidadeDeVolta);

        // 5. Destrava definitivo
        if (destravarPlayerAoFinal)
        {
            Debug.Log("🔓 [ChamaCamera] Tempo de volta acabou. Destravando o Player AGORA!");
            PlayerController p = FindAnyObjectByType<PlayerController>();
            if (p != null) p.SetCanMove(true);
        }
        else
        {
            Debug.Log("🛑 [ChamaCamera] Finalizado. (A caixinha de destravar estava desmarcada).");
        }
    }
}