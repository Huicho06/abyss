using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering.Universal;

public class Player : MonoBehaviour
{
    [Header("UI - Score")]
    [SerializeField] private GameObject panelScore;

    [Header("UI - Paneles de Almas")]
    [SerializeField] private GameObject panelAlmaRoja;
    [SerializeField] private GameObject panelAlmaVerde;
    [SerializeField] private GameObject panelAlmaMorada;
    [SerializeField] private GameObject panelAlmaMixta;

    [Header("Referencias")]
    private Rigidbody2D rb2D;
    private Collider2D miCollider;
    private Animator animator;
    [Header("Audio Muerte")]
    [SerializeField] private AudioClip gritoMuerte;
    private AudioSource audioSource;

    private GameManager gameManager;

    [Header("Movimiento")]
    [SerializeField] public float velocidadMovimiento;
    private float velocidadBase;
    [SerializeField] private float incrementoVelocidad;
    [SerializeField] private float velocidadMaxima;

    [Header("Descenso por plataformas")]
    [SerializeField] private LayerMask plataformaDescendible;
    [SerializeField] private float tiempoDesactivacionColision = 0.5f;
    private bool bajando = false;
    private Vector2 touchStartPos;
    private bool deslizandoHaciaAbajo = false;

    [Header("Salto")]
    [SerializeField] private float fuerzaSalto;
    public float fuerzaSaltoBase;
    [Range(0, 1)][SerializeField] private float multiplicadorCancelarSalto = 0.5f;
    [SerializeField] private float multiplicadorGravedad = 2f;
    private float escalaGravedad;
    private bool botonSaltoArriba = true;
    private bool saltar;

    [Header("Bonus de velocidad")]
    private bool tieneBonusVelocidad = false;
    [SerializeField] private float incrementoVelocidadBonus = 2f;
    [SerializeField] private float duracionBonusVelocidad = 2f;
    private float tiempoBonusVelocidad = 0f;
    private bool evitarSaltoPorGesto = false;

    [Header("Alma Roja, Verde, Morada, Especial")]
    public GameObject almaRoja;
    public GameObject almaVerde;
    public GameObject almaMorada;
    [SerializeField] private float multiplicadorGravedadAlmaVerde = 1.5f;
    private bool tieneAlmaVerde = false;
    private float tiempoDeGravedadAlmaVerde = 2f;
    private float tiempoRestanteGravedadAlmaVerde;

    private bool tieneAlmaMorada = false;
    private float tiempoAlmaMorada = 2f;
    private float tiempoRestanteAlmaMorada;

    private bool tieneAlmaEspecial = false;
    private float tiempoAlmaEspecial = 5f;
    private float tiempoRestanteAlmaEspecial;

    [Header("Luz")]
    public Light2D farolillo;

    [Header("Aceleración de Nivel")]
    [SerializeField] private float incrementoNivel = 0.1f;
    [SerializeField] private float velocidadMaxNivel = 10f;

    [Header("Detección de Plataforma")]
    [SerializeField] private LayerMask capaPlataforma;
    [SerializeField] private Vector2 tamañoDetectorPlataforma = new Vector2(0.5f, 0.1f);
    [SerializeField] private Vector2 offsetDetectorPlataforma;

    [Header("Detección de Pared")]
    [SerializeField] private LayerMask capaPared;
    [SerializeField] private Vector2 tamañoDetectorPared = new Vector2(0.1f, 1f);
    [SerializeField] private Vector2 offsetDetectorPared;
    private bool detectarPared;
    bool gesto= false;
    [Header("Detección de Obstáculos")]
    [SerializeField] private LayerMask capaObstaculo;
    [SerializeField] private Vector2 tamañoDetectorObstaculo = new Vector2(0.5f, 0.5f);
    [SerializeField] private Vector2 offsetDetectorObstaculo;
    private bool detectarObstaculo;

    [Header("Ralentización")]
    private bool ralentizado = false;
    [SerializeField] private float multiplicadorRalentizar = 0.5f;
    [SerializeField] private float duracionRalentizacion = 2f;

    [Header("Extras")]
    [SerializeField] private GameObject particulasDestruccion;


    private void Start()
    {
        if (panelScore != null)
        {
            panelScore.SetActive(false);
        }

        rb2D = GetComponent<Rigidbody2D>();
        miCollider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();

        velocidadBase = velocidadMovimiento;
        escalaGravedad = rb2D.gravityScale;
        tiempoRestanteAlmaEspecial = tiempoAlmaEspecial;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        gameManager = FindObjectOfType<GameManager>();

        // Ocultar paneles al inicio
        panelAlmaRoja?.SetActive(false);
        panelAlmaVerde?.SetActive(false);
        panelAlmaMorada?.SetActive(false);
        panelAlmaMixta?.SetActive(false);
    }

    private void Update()
    {
        if (DetectarGestos()) return;
        Debug.Log("Gesto : " +  gesto);
        // BAJAR por plataforma
        if ((Input.GetKeyDown(KeyCode.S) || deslizandoHaciaAbajo) && !bajando)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1f, plataformaDescendible);
            if (hit.collider != null)
            {
                Debug.Log("Bajando plataforma...");
                StartCoroutine(DesactivarColisionTemporal(hit.collider));
            }
        }

        // SALTO
        // SALTO (solo si no hay gesto descendente activo)
        if (!evitarSaltoPorGesto && (Input.GetButton("Jump") || DetectarToquePantalla()) && PuedeSaltar())
        {
            saltar = true;
        }

        if (Input.GetButtonUp("Jump") || LiberarToquePantalla())
        {
            BotonSaltoArriba();
        }

        // BONUS DE VELOCIDAD (ALMA ROJA)
        if (tieneBonusVelocidad)
        {
            tiempoBonusVelocidad -= Time.deltaTime;
            if (tiempoBonusVelocidad <= 0)
            {
                tieneBonusVelocidad = false;
                velocidadMovimiento = velocidadBase;
                animator.SetBool("esCorriendoFuerte", false);
            }
            else
            {
                velocidadMovimiento = velocidadBase * incrementoVelocidadBonus;
                animator.SetBool("esCorriendoFuerte", true);
            }
        }
        else
        {
            if (velocidadMovimiento < velocidadMaxima)
            {
                velocidadMovimiento += incrementoVelocidad * Time.deltaTime;
                velocidadMovimiento = Mathf.Min(velocidadMovimiento, velocidadMaxima);
            }
            animator.SetBool("esCorriendoFuerte", false);
        }

        // ALMA VERDE
        if (tieneAlmaVerde)
        {
            tiempoRestanteGravedadAlmaVerde -= Time.deltaTime;
            if (tiempoRestanteGravedadAlmaVerde <= 0)
            {
                tieneAlmaVerde = false;
                fuerzaSalto = fuerzaSaltoBase;
            }
        }

        // ALMA MORADA (luz)
        if (tieneAlmaMorada)
        {
            tiempoRestanteAlmaMorada -= Time.deltaTime;
            if (tiempoRestanteAlmaMorada <= 0)
            {
                tieneAlmaMorada = false;
                farolillo.intensity = 1f;
                farolillo.pointLightOuterRadius = 5f;
            }
            else
            {
                farolillo.intensity = Mathf.Lerp(farolillo.intensity, 2f, Time.deltaTime * 2f);
                farolillo.pointLightOuterRadius = Mathf.Lerp(farolillo.pointLightOuterRadius, 13f, Time.deltaTime * 2f);
            }
        }

        // ALMA ESPECIAL
        if (tieneAlmaEspecial)
        {
            tiempoRestanteAlmaEspecial -= Time.deltaTime;
            if (tiempoRestanteAlmaEspecial <= 0)
            {
                tieneAlmaEspecial = false;
                velocidadMovimiento = velocidadBase;
                animator.SetBool("esCorriendoFuerte", false);
                farolillo.intensity = 1f;
            }
            else
            {
                velocidadMovimiento = velocidadBase * incrementoVelocidadBonus;
                animator.SetBool("esCorriendoFuerte", true);
                farolillo.intensity = 2f;
            }
        }

        detectarObstaculo = DetectarColisionConObstaculo();
        animator.SetBool("esObstaculo", detectarObstaculo);

        detectarPared = DetectarColisionConPared();
        animator.SetBool("esPared", detectarPared);
    }

    public void MostrarPanelScore()
    {
        StartCoroutine(MostrarPanelScoreConDelay(1.6f)); // Espera 1 segundo
    }

    private IEnumerator MostrarPanelScoreConDelay(float segundos)
    {
        yield return new WaitForSeconds(segundos);

        if (panelScore != null)
        {
            panelScore.SetActive(true);
        }
    }

    private bool DetectarGestos()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                touchStartPos = touch.position;
                evitarSaltoPorGesto = false;
            }
            else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
            {
                float deltaY = touch.position.y - touchStartPos.y;
                float deltaX = Mathf.Abs(touch.position.x - touchStartPos.x);

                if (deltaY < -15f && deltaX < 80f && !bajando)
                {
                    RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1f, plataformaDescendible);
                    if (hit.collider != null)
                    {
                        evitarSaltoPorGesto = true;
                        StartCoroutine(DesactivarColisionTemporal(hit.collider));
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private IEnumerator ForzarDescensoPorUnInstante()
    {
        deslizandoHaciaAbajo = true;
        yield return new WaitForSeconds(0.2f); // Evita que salte por 0.2s
        deslizandoHaciaAbajo = false;
    }

    private void FixedUpdate()
    {
        bool enSuelo = PuedeSaltar();
        animator.SetBool("esSuelo", enSuelo);

        float velocidadFinal = velocidadMovimiento;
        if (ralentizado) velocidadFinal *= multiplicadorRalentizar;
        rb2D.velocity = new Vector2(velocidadFinal, rb2D.velocity.y);

        if (saltar && botonSaltoArriba && enSuelo)
        {
            Saltar();
        }

        if (rb2D.velocity.y < 0)
        {
            rb2D.gravityScale = escalaGravedad * multiplicadorGravedad;
        }
        else
        {
            rb2D.gravityScale = escalaGravedad;
        }

        if (DetectarColisionConPared() && rb2D.velocity.y > 0)
        {
            rb2D.velocity = new Vector2(rb2D.velocity.x, -Mathf.Abs(rb2D.velocity.y));
        }

        if (detectarObstaculo)
        {
            Debug.Log("Obstáculo detectado. Animación aplicada.");
        }

        saltar = false;
    }

    private void Saltar()
    {
        rb2D.AddForce(Vector2.up * fuerzaSalto, ForceMode2D.Impulse);
        saltar = false;
        botonSaltoArriba = false;
    }

    private void BotonSaltoArriba()
    {
        if (rb2D.velocity.y > 0)
        {
            rb2D.velocity = new Vector2(rb2D.velocity.x, rb2D.velocity.y * multiplicadorCancelarSalto);
        }
        botonSaltoArriba = true;
        saltar = false;
    }

    private bool PuedeSaltar()
    {
        Vector2 posicionDetector = (Vector2)transform.position + offsetDetectorPlataforma;
        Collider2D colision = Physics2D.OverlapBox(posicionDetector, tamañoDetectorPlataforma, 0, capaPlataforma);
        return colision != null;
    }

    private bool DetectarToquePantalla()
    {
        if(DetectarGestos()== false)
        {
            return Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;

        }
        return false;
    }

    private bool LiberarToquePantalla()
    {
        if (DetectarGestos() == false)
        {
            return Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended;

        }
        return false;
    }

    private bool DetectarColisionConPared()
    {
        Vector2 posicionDetector = (Vector2)transform.position + offsetDetectorPared;
        Collider2D colision = Physics2D.OverlapBox(posicionDetector, tamañoDetectorPared, 0, capaPared);
        return colision != null;
    }

    private bool DetectarColisionConObstaculo()
    {
        Vector2 posicionDetector = (Vector2)transform.position + offsetDetectorObstaculo;
        Collider2D colision = Physics2D.OverlapBox(posicionDetector, tamañoDetectorObstaculo, 0, capaObstaculo);
        return colision != null;
    }

    private IEnumerator DesactivarColisionTemporal(Collider2D plataforma)
    {
        bajando = true;
        Physics2D.IgnoreCollision(miCollider, plataforma, true);
        yield return new WaitForSeconds(tiempoDesactivacionColision);
        Physics2D.IgnoreCollision(miCollider, plataforma, false);
        bajando = false;
    }

    public void Ralentizar(float factor, float duracion)
    {
        if (!ralentizado)
        {
            StartCoroutine(RalentizarCoroutine(factor, duracion));
        }
    }

    private IEnumerator RalentizarCoroutine(float factor, float duracion)
    {
        ralentizado = true;
        multiplicadorRalentizar = factor;
        yield return new WaitForSeconds(duracion);
        ralentizado = false;
        multiplicadorRalentizar = 0.5f;
    }

    private IEnumerator RevertirAnimacion(string parametro)
    {
        yield return new WaitForSeconds(1f);
        animator.SetBool(parametro, false);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Pared"))
        {
            StartCoroutine(RevertirAnimacion("esPared"));
            Debug.Log("Pared detectada. Animación aplicada.");

        }
        if (col.CompareTag("Obstaculo"))
        {
            Debug.Log("Obstáculo detectado. Animación aplicada.");
            StartCoroutine(RevertirAnimacion("esObstaculo"));
        }

        if (col.CompareTag("AlmaRoja"))
        {
            tieneBonusVelocidad = true;
            tiempoBonusVelocidad = duracionBonusVelocidad;
            Destroy(col.gameObject);
        }

        if (col.CompareTag("AlmaVerde"))
        {
            tieneAlmaVerde = true;
            tiempoRestanteGravedadAlmaVerde = 2;
            fuerzaSaltoBase = fuerzaSalto;
            fuerzaSalto *= 1.3f;
            Destroy(col.gameObject);
        }

        if (col.CompareTag("AlmaMorada"))
        {
            tieneAlmaMorada = true;
            tiempoRestanteAlmaMorada = tiempoAlmaMorada;
            Destroy(col.gameObject);
        }

        if (col.CompareTag("AlmaEspecial"))
        {
            tieneAlmaEspecial = true;
            tiempoRestanteAlmaEspecial = tiempoAlmaEspecial;
            Destroy(col.gameObject);
        }
        if (col.CompareTag("PisoMuerte"))
        {
            Debug.Log("¡Pisaste el piso de la muerte!");
            if (gritoMuerte != null)
            {
                audioSource.PlayOneShot(gritoMuerte);
            }

            velocidadMovimiento = 0;
            rb2D.velocity = Vector2.zero;

            StartCoroutine(MorirTrasRetraso());
        }
        if (col.CompareTag("AlmaRoja"))
        {
            tieneBonusVelocidad = true;
            tiempoBonusVelocidad = duracionBonusVelocidad;
            Destroy(col.gameObject);
            if (panelAlmaRoja != null)
            {
                panelAlmaRoja.SetActive(true);
                StartCoroutine(DesactivarPanelTrasTiempo(panelAlmaRoja, duracionBonusVelocidad));
            }
        }

        if (col.CompareTag("AlmaVerde"))
        {
            tieneAlmaVerde = true;
            tiempoRestanteGravedadAlmaVerde = 2;
            fuerzaSaltoBase = fuerzaSalto;
            fuerzaSalto *= 1.3f;
            Destroy(col.gameObject);
            if (panelAlmaVerde != null)
            {
                panelAlmaVerde.SetActive(true);
                StartCoroutine(DesactivarPanelTrasTiempo(panelAlmaVerde, tiempoDeGravedadAlmaVerde));
            }
        }

        if (col.CompareTag("AlmaMorada"))
        {
            tieneAlmaMorada = true;
            tiempoRestanteAlmaMorada = tiempoAlmaMorada;
            Destroy(col.gameObject);
            if (panelAlmaMorada != null)
            {
                panelAlmaMorada.SetActive(true);
                StartCoroutine(DesactivarPanelTrasTiempo(panelAlmaMorada, tiempoAlmaMorada));
            }
        }

        if (col.CompareTag("AlmaEspecial"))
        {
            tieneAlmaEspecial = true;
            tiempoRestanteAlmaEspecial = tiempoAlmaEspecial;
            Destroy(col.gameObject);
            if (panelAlmaMixta != null)
            {
                panelAlmaMixta.SetActive(true);
                StartCoroutine(DesactivarPanelTrasTiempo(panelAlmaMixta, tiempoAlmaEspecial));
            }
        }
    }
    private IEnumerator DesactivarPanelTrasTiempo(GameObject panel, float segundos)
    {
        yield return new WaitForSeconds(segundos);
        panel.SetActive(false);
    }

    private IEnumerator MorirTrasRetraso()
    {
        yield return new WaitForSeconds(2f); // Espera antes de mostrar menú

        if (gameManager != null)
        {
            gameManager.JugadorMurio(); // Muestra menú de muerte
        }
        else
        {
            Debug.LogWarning("No se encontró el GameManager para mostrar el menú de muerte.");
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube((Vector2)transform.position + offsetDetectorObstaculo, tamañoDetectorObstaculo);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube((Vector2)transform.position + offsetDetectorPlataforma, tamañoDetectorPlataforma);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube((Vector2)transform.position + offsetDetectorPared, tamañoDetectorPared);
    }
}
