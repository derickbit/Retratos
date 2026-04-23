using UnityEngine;
using UnityEngine.Events; // <-- É essa linha que faz o UnityEvent funcionar!

public class ItemEspingarda : MonoBehaviour
{
    [Header("Eventos Únicos")]
    public UnityEvent eventoAoColetar;

    public void Collect()
    {
        // 1. Adiciona a arma no inventário novo
        if (InventoryManager.instance != null)
        {
            InventoryManager.instance.AdicionarItem("espingarda");
            InventoryManager.instance.ForcarEquipar("espingarda");
        }

        // 2. Salva na memória do jogo que você tem a arma
        PlayerPrefs.SetInt("PossuiEspingarda", 1);
        PlayerPrefs.Save();

        // 3. Dispara a câmera da Cutscene (Se houver alguma configurada no Inspector)
        eventoAoColetar?.Invoke();

        // 4. Destrói o item do chão
        Destroy(gameObject);
    }
}