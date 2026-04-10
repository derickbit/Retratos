using UnityEngine;
using System.Collections;


public class PlayerController : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 5f;

    [Header("Interaction")] // <--- NOVO
    public float interactDistance = 1f; // Distância do "braço"
    public LayerMask interactLayer;     // O que é tocável? (A Layer que criamos)

    [Header("References")]
    public Rigidbody2D rb;
    public Animator animator;

    [Header("Torch System")]
    public GameObject torchLightObject; // Arraste a "Luz_Tocha" aqui no Inspector
    public bool hasTorch = false;
    public bool isTorchLit = false; // Sabe se o fogo está aceso ou não
    private bool isTransitioning = false; // NOVO: Trava para não metralhar o botão F

    [Header("Drop System")]
    public GameObject tochaApagadaPrefab; // Arrastar o prefab na Unity
    public GameObject tochaAcesaPrefab;   // Arrastar o prefab na Unity


    private Vector2 movement;
    private Vector2 facingDir = Vector2.down; // <--- NOVO: Lembra pra onde olhou por último
    private GameObject currentPrompt;
    public bool canMove = true; // Se for false, o boneco vira uma estátua

    [Header("Áudio")]
public AudioSource footstepSource;
public AudioSource sfxSource; // NOVO: Um AudioSource só para efeitos (tiros, fósforo, etc)
    public AudioClip somAcender;  // Arraste o mp3 do fósforo aqui
    public AudioClip somApagar;   // Arraste o mp3 do sopro aqui

[Header("Debug / Testes")]
    public bool testarComTocha = false; // Marque isso APENAS quando for testar direto na cabana!

void Start()
    {
        // 1. SINCRONIZA A MÃO DIRETO COM O INVENTÁRIO 
        string itemNaMao = "";
        if (InventoryManager.instance != null) itemNaMao = InventoryManager.instance.GetItemAtual();

        if (itemNaMao == "tocha")
        {
            hasTorch = true;
            isTorchLit = (PlayerPrefs.GetInt("TochaAcesa", 0) == 1);
            if (animator != null) animator.SetBool("isLit", isTorchLit);
            if (torchLightObject != null) torchLightObject.SetActive(isTorchLit);
        }
        else
        {
            hasTorch = false;
            isTorchLit = false;
            if (animator != null) animator.SetBool("isLit", false);
            if (torchLightObject != null) torchLightObject.SetActive(false);
        }

        if (animator != null) animator.SetBool("hasTorch", hasTorch);

        // --- 2. SISTEMA DE SPAWN DA TOCHA DROPADA NO CHÃO ---
        // Se a memória diz que a tocha tá no chão, e a cena salva é esta mesma em que estamos agora:
        if (PlayerPrefs.GetInt("TochaNoChao", 0) == 1)
        {
            if (PlayerPrefs.GetString("TochaCena", "") == UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)
            {
                float posX = PlayerPrefs.GetFloat("TochaChaoX", 0);
                float posY = PlayerPrefs.GetFloat("TochaChaoY", 0);
                bool acesa = (PlayerPrefs.GetInt("TochaChaoAcesa", 0) == 1);

                GameObject prefabCerto = acesa ? tochaAcesaPrefab : tochaApagadaPrefab;
                if (prefabCerto != null)
                {
                    GameObject tochaMundo = Instantiate(prefabCerto, new Vector3(posX, posY, 0), Quaternion.identity);
                    tochaMundo.name = prefabCerto.name + "(Clone)"; // Crachá de clone
                    TorchItem script = tochaMundo.GetComponent<TorchItem>();
                    if (script != null) script.estavaAcesa = acesa;
                }
            }
        }
        // ----------------------------------------------------

        // 3. SPAWN DO PLAYER (Portas)
        if (!string.IsNullOrEmpty(Door.nextSpawnID))
        {
            GameObject spawnPoint = GameObject.Find(Door.nextSpawnID);
            if (spawnPoint != null) transform.position = spawnPoint.transform.position;
        }
    }

    void Update()
    {

        // ---> NOVO: Atualiza a animação INDEPENDENTE do jogador estar travado ou não!
        animator.SetBool("hasTorch", hasTorch);

            // TRAVA DE MOVIMENTO
    if (!canMove)
    {
        // Força o boneco a parar fisicamente (senão ele desliza se entrou correndo)
        rb.linearVelocity = Vector2.zero; 

        // Força a animação a parar (senão ele fica "andando no lugar")
        animator.SetBool("isWalking", false);
        movement = Vector2.zero;

        // --- NOVO: Apaga o balãozinho quando o boneco trava ---
            if (currentPrompt != null)
            {
                currentPrompt.SetActive(false);
                currentPrompt = null;
            }
        return; // Encerra o Update aqui. O código abaixo não roda.
    }

    // ... (Todo o resto do seu código de Input/Move/Anim vem aqui embaixo) ...
    movement.x = Input.GetAxisRaw("Horizontal");
    // etc...


        // --- 1. Movimentação (Igual antes) ---
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        movement = movement.normalized;

        animator.SetBool("hasTorch", hasTorch);

        // --- 2. Animação (Igual antes) ---
        animator.SetBool("isWalking", movement.sqrMagnitude > 0);

        // --- LÓGICA DE PASSOS ---
if (movement.sqrMagnitude > 0 && canMove)
{
    // Se ele está se movendo e o som não está tocando, dá o play!
    if (!footstepSource.isPlaying)
    {
        // Opcional: Variar o tom (Pitch) levemente para não ficar robótico
        footstepSource.pitch = Random.Range(0.8f, 1.2f);
        footstepSource.Play();
    }
}
else
{
    // Se parou, pausa o som do passo
    footstepSource.Stop();
}

        // --- 3. Direção do Olhar (NOVO) ---
        // Se a gente se moveu, atualizamos a "Última Direção Vista"
        // Sem isso, se você parar, o vetor 'movement' vira zero e o Raycast não sabe pra onde atirar.
        if (movement.sqrMagnitude > 0)
        {
            facingDir = movement;
        }

        // --- 4. Flip Visual (Igual antes) ---
        if (movement.x > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (movement.x < 0) transform.localScale = new Vector3(-1, 1, 1);

// --- 5. Interação (Igual antes) ---
        if (Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }

// --- SISTEMA DE INVENTÁRIO CÍCLICO (NOVO) ---
        // Roda o Carrossel no botão Q (Poderia ser o Scroll também!)
        if (Input.GetKeyDown(KeyCode.Q)) 
        {
            if (!isTransitioning) 
            {
                if (isTorchLit) 
                {
                    // Se a tocha tá acesa, apaga ela primeiro de forma suave e DEPOIS gira o carrossel
                    StartCoroutine(ApagarETrocar());
                }
                else 
                {
                    // Se já tá apagada (ou de mão vazia), gira na hora
                    if (InventoryManager.instance != null) InventoryManager.instance.CiclarInventario();
                }
            }
        }

        // Faz o braço do boneco se atualizar todo frame baseado no centro do Carrossel
        ChecarItemNaMao();
        // Aperte "F" para acender/apagar o fogo (SOMENTE se estiver segurando a tocha)
        if (Input.GetKeyDown(KeyCode.F) && hasTorch)
        {
            ToggleFogo();
        }
        // Aperte "G" para Dropar a tocha no chão (SOMENTE se ela estiver na mão)
        if (Input.GetKeyDown(KeyCode.G) && hasTorch)
        {
            DroparTocha();
        }

        // --- 6. Checagem Visual (NOVO) ---
    CheckInteractionPrompt();

    // --- MODO DEBUG: Apaga o Save na hora para testar ---
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("⚠️ SAVE APAGADO! Reinicie a cena para testar do zero.");
        }

    }

    void FixedUpdate()
    {
        if (!canMove) return;
        rb.linearVelocity = movement * moveSpeed;
    }

    // --- FUNÇÃO NOVA: O Raio Laser ---
  void Interact()
    {
        Vector3 rayOrigin = transform.position + new Vector3(0, 0.1f, 0);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, facingDir, interactDistance, interactLayer);


                if (hit.collider != null)
        {
            // Verifica Porta
            Door door = hit.collider.GetComponent<Door>();
            if (door != null) { door.Enter(); return; }

// Verifica Tocha no Chão
            TorchItem item = hit.collider.GetComponent<TorchItem>();
         if (item != null) 
            { 
                bool eraAcesa = item.estavaAcesa; 
                item.Collect(); 
                
                if (InventoryManager.instance != null) InventoryManager.instance.ForcarEquipar("tocha");
                ChecarItemNaMao(); 

                if (eraAcesa)
                {
                    isTorchLit = true;
                    animator.SetBool("isLit", true);
                    animator.Play("Player_Torch_idle", 0, 0f);
                    if (torchLightObject != null) torchLightObject.SetActive(true);
                    PlayerPrefs.SetInt("TochaAcesa", 1); 
                }
                else
                {
                    // --- CORREÇÃO DO BUG AQUI ---
                    isTorchLit = false;
                    animator.SetBool("isLit", false);
                    if (torchLightObject != null) torchLightObject.SetActive(false);
                    PlayerPrefs.SetInt("TochaAcesa", 0); 
                }
                PlayerPrefs.Save();
                return; 
            }

            // --- BLOCO 3 NOVO ---
            Examinable examinable = hit.collider.GetComponent<Examinable>();
            if (examinable != null) 
            { 
                examinable.Examine(); 
                return; 
            }
            // ------------------

            BedInteract bed = hit.collider.GetComponent<BedInteract>();
        if (bed != null)
        {
            bed.TentarDormir();
            return;
        }
            
            Debug.Log("Interagi com: " + hit.collider.name);
        }
    }

    // --- DEBUG VISUAL (Pra gente ver a linha vermelha na Scene) ---
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 rayOrigin = transform.position + new Vector3(0, 0.1f, 0);
        // Desenha a linha na direção que ele está olhando
        Gizmos.DrawLine(rayOrigin, rayOrigin + (Vector3)facingDir * interactDistance);
    }


    void CheckInteractionPrompt()
{
    // 1. Lança o raio (igual ao Interact)
    Vector3 rayOrigin = transform.position + new Vector3(0, 0.1f, 0);
    RaycastHit2D hit = Physics2D.Raycast(rayOrigin, facingDir, interactDistance, interactLayer);

    // 2. Se bateu em algo...
    if (hit.collider != null)
    {
        // Tenta achar o filho chamado "Prompt" dentro do objeto que batemos
        Transform promptTransform = hit.collider.transform.Find("Prompt");

        if (promptTransform != null)
        {
            GameObject newPrompt = promptTransform.gameObject;

            // Se for um balão diferente do que já está aceso (ou se nenhum estiver aceso)
            if (currentPrompt != newPrompt)
            {
                // Apaga o anterior (se existir)
                if (currentPrompt != null) currentPrompt.SetActive(false);

                // Acende o novo
                newPrompt.SetActive(true);
                currentPrompt = newPrompt;
            }
            return; // Sai da função, trabalho feito
        }
    }

    // 3. Se o raio não bateu em nada (ou bateu em algo sem Prompt)
    if (currentPrompt != null)
    {
        // Apaga o que estava aceso
        currentPrompt.SetActive(false);
        currentPrompt = null;
    }
}

void ChecarItemNaMao()
    {
        if (InventoryManager.instance == null) return;

        // O que está no centro do Carrossel agora?
        string itemAtual = InventoryManager.instance.GetItemAtual();

        if (itemAtual == "tocha")
        {
            // O jogador girou até a Tocha. Levanta o braço!
            hasTorch = true;
            animator.SetBool("hasTorch", true);
        }
else // O jogador está de "Mão Vazia"
        {
            hasTorch = false;
            animator.SetBool("hasTorch", false);

            if (isTorchLit)
            {
                isTorchLit = false;
                isTransitioning = false; 
                animator.SetBool("isLit", false);
                if (torchLightObject != null) torchLightObject.SetActive(false);

                // <--- SALVA QUE APAGOU FORÇADO AO GUARDAR NO BOLSO --->
                PlayerPrefs.SetInt("TochaAcesa", 0);
                PlayerPrefs.Save();
            }
        }
    }

public void ToggleFogo()
    {
        // Se a animação já estiver rolando, o botão 'F' é ignorado pra não bugar
        if (isTransitioning) return; 

        StartCoroutine(AnimacaoDoFogo());
    }

    IEnumerator AnimacaoDoFogo()
    {
        isTransitioning = true;

        // --- NOVO: TRAVA DE MOVIMENTO (Adiciona Tensão no Jogo!) ---
        canMove = false;
        rb.linearVelocity = Vector2.zero; // Freia o boneco na hora
        animator.SetBool("isWalking", false); // Força a animação de parado
        // -------------------------------------------------------------

        isTorchLit = !isTorchLit;

        // <--- SALVA NA MEMÓRIA SEMPRE QUE APERTAR F --->
        PlayerPrefs.SetInt("TochaAcesa", isTorchLit ? 1 : 0);
        PlayerPrefs.Save();

        if (isTorchLit)
        {
            // 1. Toca o som do fósforo
            if (sfxSource != null && somAcender != null) sfxSource.PlayOneShot(somAcender);
            
            // 2. Dispara a animação (os 3 frames)
            animator.SetBool("isLit", true);

            // 3. Espera 0.3 segundos (que é o tempo dos seus 3 frames rodarem)
            yield return new WaitForSeconds(0.3f);

            // 4. Acende a luz amarela da Unity só depois da faísca!
            if (torchLightObject != null) torchLightObject.SetActive(true);
        }
        else
        {
            // 1. Toca o som do sopro
            if (sfxSource != null && somApagar != null) sfxSource.PlayOneShot(somApagar, 0.3f);

            // 2. Apaga a luz amarela na HORA do sopro
            if (torchLightObject != null) torchLightObject.SetActive(false);

            // 3. Dispara a animação (os 3 frames voltando pro pau de madeira)
            animator.SetBool("isLit", false);

            // 4. Espera o tempo da animação terminar
            yield return new WaitForSeconds(0.3f);
        }
        // --- NOVO: DESTRAVA O MOVIMENTO APÓS A ANIMAÇÃO ---
        canMove = true;
        isTransitioning = false; // Destrava o botão 'F'
    }

public void DroparTocha(bool jogarPraTras = false)
    {
       if (isTransitioning) return; 

        GameObject tochaCerta = isTorchLit ? tochaAcesaPrefab : tochaApagadaPrefab;
        
        if (tochaCerta != null)
        {
            Vector2 direcao = jogarPraTras ? -facingDir : facingDir;
            Vector3 distArremesso = (Vector3)direcao * 0.5f; 
            Vector3 posicaoDrop = transform.position + distArremesso;
            posicaoDrop.z = 0f; 

            // CRIAMOS O OBJETO E COLOCAMOS O CRACHÁ DE "CLONE"
            GameObject novaTochaNoChao = Instantiate(tochaCerta, posicaoDrop, Quaternion.identity);
            novaTochaNoChao.name = tochaCerta.name + "(Clone)"; 
            
            TorchItem scriptTocha = novaTochaNoChao.GetComponent<TorchItem>();
            if (scriptTocha != null)
            {
                scriptTocha.estavaAcesa = isTorchLit; 
            }

            // --- NOVO: SALVA O GPS E O ESTADO DA TOCHA NO CHÃO ---
            PlayerPrefs.SetInt("TochaNoChao", 1);
            PlayerPrefs.SetFloat("TochaChaoX", posicaoDrop.x);
            PlayerPrefs.SetFloat("TochaChaoY", posicaoDrop.y);
            PlayerPrefs.SetInt("TochaChaoAcesa", isTorchLit ? 1 : 0);
            PlayerPrefs.SetString("TochaCena", UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        if (InventoryManager.instance != null) InventoryManager.instance.RemoverTocha();
        
        ChecarItemNaMao();

        PlayerPrefs.SetInt("TochaAcesa", 0);
        PlayerPrefs.Save();
    }

    IEnumerator ApagarETrocar()
    {
        // Começa a animação de apagar (o próprio script AnimacaoDoFogo já muda o isTorchLit pra false)
        yield return StartCoroutine(AnimacaoDoFogo());
        
        // Depois que a animação acabou, ele finalmente gira o cilindro do inventário
        if (InventoryManager.instance != null) InventoryManager.instance.CiclarInventario();
    }

}