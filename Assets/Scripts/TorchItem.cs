using UnityEngine;

public class TorchItem : MonoBehaviour
{
    public bool estavaAcesa = false; 

    void Start()
    {
        // 1. Se essa tocha é a ORIGINAL do mapa (não tem "Clone" no nome)
        // e nós já pegamos a tocha alguma vez na vida, ela se autodestrói.
        if (!gameObject.name.Contains("Clone") && PlayerPrefs.GetInt("TochaJaFoiPega", 0) == 1)
        {
            Destroy(gameObject);
            return;
        }

        // 2. Se ela já nasceu com o prefab de Acesa, garante que a memória saiba
        if (gameObject.name.Contains("Acesa")) estavaAcesa = true;
    }

    public void Collect()
    {
        // MANTÉM A SUA LÓGICA INTACTA
        if (InventoryManager.instance != null)
        {
            InventoryManager.instance.AdicionarTocha();
        }

        PlayerPrefs.SetInt("PossuiTocha", 1);
        PlayerPrefs.SetInt("TochaAcesa", estavaAcesa ? 1 : 0);
        
        // --- AS DUAS LINHAS NOVAS DO GPS ---
        PlayerPrefs.SetInt("TochaJaFoiPega", 1); // Avisa que a do mapa já foi encontrada
        PlayerPrefs.SetInt("TochaNoChao", 0);    // Limpa o GPS (ela saiu do chão pra mochila)
        // -----------------------------------

        PlayerPrefs.Save();

        Destroy(gameObject);
    }
}