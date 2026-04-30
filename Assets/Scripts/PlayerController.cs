using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Componentes Base")]
    public Rigidbody2D rb;
    public Animator animator;
    public bool canMove = true; 

    [Header("Movimentação e Peso")]
    public float moveSpeedNormal = 5f; 
    public float moveSpeedArmado = 2.5f; 
    private Vector2 movement;
    private Vector2 facingDir = Vector2.down; 

    [Header("Interação (Raio Laser)")]
    public float interactDistance = 1f; 
    public LayerMask interactLayer;     
    private GameObject currentPrompt;

    [Header("Inventário de Estado")]
    public bool hasTorch = false;
    public bool isTorchLit = false; 
    public bool hasEspingarda = false;
    public bool hasGunGuardada = false;
    private bool isTransitioning = false; 

    [Header("Sistema da Tocha")]
    public GameObject torchLightObject; 
    public GameObject tochaApagadaPrefab; 
    public GameObject tochaAcesaPrefab;   

    [Header("Sistema da Espingarda")]
    public GameObject espingardaChaoPrefab;
    public GameObject projetilPrefab; 
    public Transform pontoDeTiro;     
    public float fireRate = 0.8f; 
    public float distanciaTiro = 15f; 
    public LayerMask camadaInimigo;   
    private float nextFireTime = 0f;

    [Header("Efeitos Visuais e Sonoros")]
    public AudioSource footstepSource;
    public AudioSource sfxSource; 
    public AudioSource somTiro; 
    public AudioClip somAcender;  
    public AudioClip somApagar;   
    public GameObject MuzzleFlash;
    public CameraShake cameraShake; 
    public float shakeDuracao = 0.1f;
    public float shakeIntensidade = 0.2f;

    [Header("Debug Tools")]
    public bool debugForceTorch = false;
    public bool debugForceEspingarda = false;
    public bool testarComTocha = false;

    [Header("Sistema de Zona Segura")]
    public bool inSafeZone = false;
    public AmigoSafeZone zonaAtual;

    void Start()
    {
        // Puxa o estado inicial das luzes e spawn
        isTorchLit = (PlayerPrefs.GetInt("TochaAcesa", 0) == 1);
        if (torchLightObject != null) torchLightObject.SetActive(isTorchLit);

        RestaurarTochaDoChao();
        
        if (!string.IsNullOrEmpty(Door.nextSpawnID))
        {
            GameObject spawnPoint = GameObject.Find(Door.nextSpawnID);
            if (spawnPoint != null) transform.position = spawnPoint.transform.position;
        }

        SincronizarInventarioComAnimator();
    }

    void Update()
    {
        SincronizarInventarioComAnimator(); // O ÚNICO responsável por atualizar variáveis de mão!

        if (!canMove)
        {
            rb.linearVelocity = Vector2.zero; 
            animator.SetBool("isWalking", false);
            movement = Vector2.zero;
            EsconderPromptVisual();
            return; 
        }

        // 1. Movimentação
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        movement = movement.normalized;

        animator.SetBool("isWalking", movement.sqrMagnitude > 0);

        if (movement.sqrMagnitude > 0)
        {
            facingDir = movement;
            if (!footstepSource.isPlaying)
            {
                footstepSource.pitch = Random.Range(0.8f, 1.2f);
                footstepSource.Play();
            }
        }
        else footstepSource.Stop();

        // 2. Flip Visual
        if (movement.x > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (movement.x < 0) transform.localScale = new Vector3(-1, 1, 1);

        // 3. Inputs de Ação
        if (Input.GetKeyDown(KeyCode.E)) Interact();
        
        if (Input.GetKeyDown(KeyCode.Q) && !isTransitioning) 
        {
            if (isTorchLit) StartCoroutine(ApagarETrocar());
            else if (InventoryManager.instance != null) InventoryManager.instance.CiclarInventario();
        }

        if (Input.GetKeyDown(KeyCode.F) && hasTorch) ToggleFogo();
        
        if (Input.GetKeyDown(KeyCode.G))
        {
            if (hasTorch) DroparTocha();
            else if (hasEspingarda) DroparEspingarda();
        }

        if ((Input.GetKeyDown(KeyCode.F) || Input.GetMouseButtonDown(0)) && hasEspingarda)
        {
            if (Time.time >= nextFireTime && !isTransitioning)
            {
                // SE ELE TENTAR ATIRAR NO ACAMPAMENTO, CHAMA A CUTSCENE!
                if (inSafeZone && zonaAtual != null)
                {
                    zonaAtual.DarEsporro(this);
                    nextFireTime = Time.time + fireRate; 
                }
                // SE ESTIVER NA MATA, ATIRA NORMAL
                else
                {
                    Atirar();
                    nextFireTime = Time.time + fireRate; 
                }
            }
        }

        CheckInteractionPrompt();

        // Debug Clean Save
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("⚠️ SAVE APAGADO! Reinicie a cena.");
        }
    }

    void FixedUpdate()
    {
        if (!canMove) return;
        float velocidadeAtual = hasEspingarda ? moveSpeedArmado : moveSpeedNormal;
        rb.linearVelocity = movement * velocidadeAtual;
    }

    // ==========================================
    // SISTEMA DE SINCRONIZAÇÃO (A REFATORAÇÃO)
    // ==========================================
    void SincronizarInventarioComAnimator()
    {
        if (InventoryManager.instance == null) return;

        string itemAtual = InventoryManager.instance.GetItemAtual();
        bool possuiArmaNaMochila = InventoryManager.instance.VerificarSePossuiItem("espingarda");

        // Overrides de Debug
        if (debugForceTorch) itemAtual = "tocha";
        if (debugForceEspingarda) { itemAtual = "espingarda"; possuiArmaNaMochila = true; }

        // Define a verdade absoluta
        hasTorch = (itemAtual == "tocha");
        hasEspingarda = (itemAtual == "espingarda");
        hasGunGuardada = (possuiArmaNaMochila && !hasEspingarda);

        // Trava de Segurança: Se não estou com a tocha na mão, ela NÃO pode estar acesa
        if (!hasTorch && isTorchLit)
        {
            isTorchLit = false;
            if (torchLightObject != null) torchLightObject.SetActive(false);
            PlayerPrefs.SetInt("TochaAcesa", 0);
            PlayerPrefs.Save();
        }

        // Manda pro Animator
        animator.SetBool("hasTorch", hasTorch);
        animator.SetBool("hasGun", hasEspingarda); 
        animator.SetBool("hasGunGuardada", hasGunGuardada);
        animator.SetBool("isLit", isTorchLit);
    }

    // ==========================================
    // INTERAÇÃO E AMBIENTE
    // ==========================================
    void Interact()
    {
        Vector3 rayOrigin = transform.position + new Vector3(0, 0.1f, 0);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, facingDir, interactDistance, interactLayer);

        if (hit.collider != null)
        {
            Door door = hit.collider.GetComponent<Door>();
            if (door != null) { door.Enter(); return; }

            ArbustoSaida saidaMatagal = hit.collider.GetComponent<ArbustoSaida>();
            if (saidaMatagal != null) { saidaMatagal.PularNoMato(this); return; }

            ItemEspingarda espingarda = hit.collider.GetComponent<ItemEspingarda>();
            if (espingarda != null) { espingarda.Collect(); return; }

            Examinable examinable = hit.collider.GetComponent<Examinable>();
            if (examinable != null) { examinable.Examine(); return; }

            BedInteract bed = hit.collider.GetComponent<BedInteract>();
            if (bed != null) { bed.TentarDormir(); return; }

            // Lógica isolada da Tocha no Chão
            TorchItem itemTocha = hit.collider.GetComponent<TorchItem>();
            if (itemTocha != null) 
            { 
                bool eraAcesa = itemTocha.estavaAcesa; 
                itemTocha.Collect(); 
                
                if (InventoryManager.instance != null) InventoryManager.instance.ForcarEquipar("tocha");
                SincronizarInventarioComAnimator(); // Sincroniza a mão antes de pular a animação!

                if (eraAcesa)
                {
                    isTorchLit = true;
                    if (torchLightObject != null) torchLightObject.SetActive(true);
                    PlayerPrefs.SetInt("TochaAcesa", 1); 
                    
                    // O TELEPORTE VISUAL (Evita o boneco riscar o fósforo do nada)
                    if (hasGunGuardada) animator.Play("Player_idle_Arma_Tocha", 0, 0f); 
                    else animator.Play("Player_Torch_idle", 0, 0f);
                }
                else
                {
                    isTorchLit = false;
                    if (torchLightObject != null) torchLightObject.SetActive(false);
                    PlayerPrefs.SetInt("TochaAcesa", 0); 
                }
                PlayerPrefs.Save();
                return; 
            }
        }
    }

    void CheckInteractionPrompt()
    {
        Vector3 rayOrigin = transform.position + new Vector3(0, 0.1f, 0);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, facingDir, interactDistance, interactLayer);

        if (hit.collider != null)
        {
            Transform promptTransform = hit.collider.transform.Find("BotãoInteração");
            if (promptTransform != null)
            {
                GameObject newPrompt = promptTransform.gameObject;
                if (currentPrompt != newPrompt)
                {
                    EsconderPromptVisual();
                    newPrompt.SetActive(true);
                    currentPrompt = newPrompt;
                }
                return; 
            }
        }
        EsconderPromptVisual();
    }

    void EsconderPromptVisual()
    {
        if (currentPrompt != null)
        {
            currentPrompt.SetActive(false);
            currentPrompt = null;
        }
    }

    // ==========================================
    // AÇÕES DE ITENS (Atirar, Dropar, Acender)
    // ==========================================
    public void ToggleFogo()
    {
        if (isTransitioning) return; 
        StartCoroutine(AnimacaoDoFogo());
    }

    IEnumerator AnimacaoDoFogo()
    {
        isTransitioning = true;
        SetCanMove(false);
        animator.SetBool("isWalking", false); 

        isTorchLit = !isTorchLit;
        PlayerPrefs.SetInt("TochaAcesa", isTorchLit ? 1 : 0);
        PlayerPrefs.Save();

        if (isTorchLit)
        {
            if (sfxSource != null && somAcender != null) sfxSource.PlayOneShot(somAcender);
            animator.SetBool("isLit", true);
            yield return new WaitForSeconds(0.3f);
            if (torchLightObject != null) torchLightObject.SetActive(true);
        }
        else
        {
            if (sfxSource != null && somApagar != null) sfxSource.PlayOneShot(somApagar, 0.3f);
            if (torchLightObject != null) torchLightObject.SetActive(false);
            animator.SetBool("isLit", false);
            yield return new WaitForSeconds(0.3f);
        }
        
        SetCanMove(true);
        isTransitioning = false; 
    }

    public void DroparTocha(bool jogarPraTras = false)
    {
        if (isTransitioning) return; 

        GameObject tochaCerta = isTorchLit ? tochaAcesaPrefab : tochaApagadaPrefab;
        if (tochaCerta != null)
        {
            Vector3 posicaoDrop = transform.position + ((Vector3)(jogarPraTras ? -facingDir : facingDir) * 0.5f);
            posicaoDrop.z = 0f; 

            GameObject novaTocha = Instantiate(tochaCerta, posicaoDrop, Quaternion.identity);
            novaTocha.name = tochaCerta.name + "(Clone)"; 
            
            TorchItem script = novaTocha.GetComponent<TorchItem>();
            if (script != null) script.estavaAcesa = isTorchLit; 

            PlayerPrefs.SetInt("TochaNoChao", 1);
            PlayerPrefs.SetFloat("TochaChaoX", posicaoDrop.x);
            PlayerPrefs.SetFloat("TochaChaoY", posicaoDrop.y);
            PlayerPrefs.SetInt("TochaChaoAcesa", isTorchLit ? 1 : 0);
            PlayerPrefs.SetString("TochaCena", UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        if (InventoryManager.instance != null) InventoryManager.instance.RemoverItem("tocha");
        
        isTorchLit = false;
        if (torchLightObject != null) torchLightObject.SetActive(false);
        PlayerPrefs.SetInt("TochaAcesa", 0);
        PlayerPrefs.Save();

        SincronizarInventarioComAnimator(); // Avisa o sistema que estamos sem tocha

        // TELEPORTE VISUAL (Para evitar o bug do Animator tocar a animação de soprar o fogo)
        if (hasEspingarda) animator.Play("Player_ArmaGuardada_Idle", 0, 0f);
        else if (hasGunGuardada) animator.Play("Player_ArmaGuardada_Idle", 0, 0f); 
        else animator.Play("Player_idle", 0, 0f);
    }

    public void DroparEspingarda(bool jogarPraTras = false)
    {
        if (isTransitioning) return; 

        if (espingardaChaoPrefab != null)
        {
            Vector3 posicaoDrop = transform.position + ((Vector3)(jogarPraTras ? -facingDir : facingDir) * 0.5f);
            posicaoDrop.z = 0f; 

            GameObject armaNoChao = Instantiate(espingardaChaoPrefab, posicaoDrop, Quaternion.identity);
            armaNoChao.name = espingardaChaoPrefab.name + "(Clone)"; 
        }

        if (InventoryManager.instance != null) InventoryManager.instance.RemoverItem("espingarda");
        PlayerPrefs.SetInt("PossuiEspingarda", 0);
        PlayerPrefs.Save();

        SincronizarInventarioComAnimator();
    }

    IEnumerator ApagarETrocar()
    {
        yield return StartCoroutine(AnimacaoDoFogo());
        if (InventoryManager.instance != null) InventoryManager.instance.CiclarInventario();
    }

    public void Atirar()
    {
        isTransitioning = true;
        SetCanMove(false);
        animator.SetBool("isWalking", false);

        if (MuzzleFlash != null)
        {
            MuzzleFlash.SetActive(true);
            StartCoroutine(ApagarClarao());
        }
        if (somTiro != null) somTiro.Play();
        if (CameraShake.Instance != null) CameraShake.Instance.Shake(shakeDuracao, shakeIntensidade);

        if (projetilPrefab != null && pontoDeTiro != null)
        {
            Vector2 direcaoTiro = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
            GameObject balaNova = Instantiate(projetilPrefab, pontoDeTiro.position, Quaternion.identity);
            
            Projetil scriptBala = balaNova.GetComponent<Projetil>();
            if (scriptBala != null) scriptBala.Configurar(direcaoTiro);
        }

        Invoke("DestravarTiro", 0.3f);
    }

    IEnumerator ApagarClarao()
    {
        yield return new WaitForSeconds(0.1f);
        if (MuzzleFlash != null) MuzzleFlash.SetActive(false);
    }

    void DestravarTiro()
    {
        SetCanMove(true);
        isTransitioning = false;
    }

    // ==========================================
    // UTILITÁRIOS
    // ==========================================
    public void SetCanMove(bool status) 
    {
        canMove = status;
        if (!status) rb.linearVelocity = Vector2.zero; 
    }

    void RestaurarTochaDoChao()
    {
        if (PlayerPrefs.GetInt("TochaNoChao", 0) == 1 && PlayerPrefs.GetString("TochaCena", "") == UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)
        {
            float posX = PlayerPrefs.GetFloat("TochaChaoX", 0);
            float posY = PlayerPrefs.GetFloat("TochaChaoY", 0);
            bool acesa = (PlayerPrefs.GetInt("TochaChaoAcesa", 0) == 1);

            GameObject prefabCerto = acesa ? tochaAcesaPrefab : tochaApagadaPrefab;
            if (prefabCerto != null)
            {
                GameObject tochaMundo = Instantiate(prefabCerto, new Vector3(posX, posY, 0), Quaternion.identity);
                tochaMundo.name = prefabCerto.name + "(Clone)"; 
                TorchItem script = tochaMundo.GetComponent<TorchItem>();
                if (script != null) script.estavaAcesa = acesa;
            }
        }
    }

    void OnValidate() 
    {
        if (Application.isPlaying && debugForceEspingarda) 
        {
            debugForceEspingarda = false; 
            if (InventoryManager.instance != null) 
            {
                InventoryManager.instance.AdicionarItem("espingarda"); 
                InventoryManager.instance.ForcarEquipar("espingarda");
            }
        }
    }
}