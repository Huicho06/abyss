using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering.Universal; // Necesario para usar Light2D


public class Player : MonoBehaviour
{
    [Header("Referencias")]
    private Rigidbody2D rb2D;
    // Nuevas variables para el Alma Especial
    private bool tieneAlmaEspecial = false; // Si el jugador tiene el alma especial
    private float tiempoAlmaEspecial = 5f; // Duración del modo especial
    private float tiempoRestanteAlmaEspecial; // Para controlar el tiempo restante
    [SerializeField] private GameObject particulasDestruccion; // Prefab de partículas de destrucción


    private float velocidadBaseGuardada;

    [Header("Movimiento")]
    [SerializeField] public float velocidadMovimiento;
    private float velocidadBase;

    [SerializeField] private float incrementoVelocidad;
    [SerializeField] private float velocidadMaxima;
    public GameObject almaRoja; // Si es un GameObject
    public GameObject almaVerde;  // Para referenciar el objeto Alma Verde
    [SerializeField] private float multiplicadorGravedadAlmaVerde = 1.5f; // El multiplicador de gravedad para cuando el jugador toca el alma verde
    private bool tieneAlmaVerde = false;  // Controla si el jugador está tocando el alma verde
    private float tiempoDeGravedadAlmaVerde = 2f; // Cuánto tiempo el efecto de gravedad se aplica
    private float tiempoRestanteGravedadAlmaVerde;  // Para controlar el tiempo restante

    [Header("Salto")]
    [SerializeField] private float fuerzaSalto;
    // Nueva variable para el aumento de velocidad
    private bool tieneBonusVelocidad = false;
    [SerializeField] private float incrementoVelocidadBonus = 2f; // Multiplicador de velocidad
    [SerializeField] private float duracionBonusVelocidad = 2f; // Duración del bonus en segundos
    private float tiempoBonusVelocidad = 0f; // Para controlar el tiempo del bonus

    [Header("Salto Regulable")]
    [Range(0, 1)][SerializeField] private float multiplicadorCancelarSalto = 0.5f;
    [SerializeField] private float multiplicadorGravedad = 2f;
    private float escalaGravedad;
    private bool botonSaltoArriba = true;
    private bool saltar;

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

    [Header("Detección de Obstáculos")]
    [SerializeField] private LayerMask capaObstaculo;
    [SerializeField] private Vector2 tamañoDetectorObstaculo = new Vector2(0.5f, 0.5f);
    [SerializeField] private Vector2 offsetDetectorObstaculo;
    private bool detectarObstaculo;

    private bool ralentizado = false;
    [SerializeField] private float multiplicadorRalentizar = 0.5f;
    [SerializeField] private float duracionRalentizacion = 2f;

    [Header("Animacion")]
    private Animator animator;

    public GameObject almaMorada;  // Referencia al alma morada
    public Light2D farolillo;  // Aquí cambiamos a Light2D

    private bool tieneAlmaMorada = false;  // Controla si el jugador está tocando el alma morada
    private float tiempoAlmaMorada = 2f; // Cuánto tiempo dura el aumento de luminosidad
    private float tiempoRestanteAlmaMorada;  // Para controlar el tiempo restante

    private void Start()
    {
        velocidadBase = velocidadMovimiento;
        rb2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();  // Asumimos que el jugador tiene un Animator
        tiempoRestanteAlmaEspecial = tiempoAlmaEspecial; // Inicializar el tiempo restante de Alma Especial
        if (rb2D == null)
        {
            Debug.LogError("No se encontró el componente Rigidbody2D en el jugador.");
        }

        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("No se encontró el componente Animator en el jugador.");
        }

        escalaGravedad = rb2D.gravityScale;
    }

    public void ActivarAlmaEspecial()
    {
        tieneAlmaEspecial = true;
        tiempoRestanteAlmaEspecial = tiempoAlmaEspecial; // Reiniciamos el tiempo
    }
    private void Update()
    {
        // Si el jugador tiene el alma especial
        // Si el jugador tiene el alma especial
        if (tieneAlmaEspecial)
        {
            // Reducimos el tiempo restante de la alma especial
            tiempoRestanteAlmaEspecial -= Time.deltaTime;

            // Mientras dure el modo especial
            if (tiempoRestanteAlmaEspecial <= 0)
            {
                tieneAlmaEspecial = false; // Se termina el efecto
                velocidadMovimiento = velocidadBase; // Restauramos la velocidad original
                animator.SetBool("esCorriendoFuerte", false); // Desactiva animación de correr rápido
                farolillo.intensity = 1f; // Restauramos la iluminación base (alma morada)
            }
            else
            {
                // Aquí activamos el modo loco (combinando los efectos)
                velocidadMovimiento = velocidadBase * incrementoVelocidadBonus; // Aumento de velocidad
                animator.SetBool("esCorriendoFuerte", true); // Activa animación de correr rápido

                // Aumentar la intensidad de la luz (alma morada)
                farolillo.intensity = 2f; // Intensidad de luz incrementada (ejemplo)
            }

            // Aquí puedes manejar el salto especial de Alma Verde
            if (tieneAlmaVerde)
            {
                // Si el jugador tiene el alma verde, aplicamos el aumento de gravedad para el salto
                if (tiempoRestanteGravedadAlmaVerde > 0)
                {
                    // Aumentamos la gravedad para simular un salto más fuerte
                    rb2D.gravityScale = multiplicadorGravedadAlmaVerde;
                }
                else
                {
                    rb2D.gravityScale = 1f; // Restauramos la gravedad normal
                }
            }
        }



        // Otros comportamientos del jugador...
        // Manejo del bonus de velocidad (alma roja)
        if (tieneBonusVelocidad)
        {
            tiempoBonusVelocidad -= Time.deltaTime;

            if (tiempoBonusVelocidad <= 0)
            {
                tieneBonusVelocidad = false; // Termina el bonus de velocidad
                velocidadMovimiento = velocidadBase; // Restauramos la velocidad original
                animator.SetBool("esCorriendoFuerte", false); // Desactiva la animación de correr rápido
            }
            else
            {
                // Aplicamos el bonus de velocidad
                velocidadMovimiento = velocidadBase * incrementoVelocidadBonus;
                animator.SetBool("esCorriendoFuerte", true); // Activa la animación de correr rápido
            }
        }
        else
        {
            // Si no tiene el bonus, aplicamos la aceleración
            if (velocidadMovimiento < velocidadMaxima)
            {
                velocidadMovimiento += incrementoVelocidad * Time.deltaTime;
                velocidadMovimiento = Mathf.Min(velocidadMovimiento, velocidadMaxima); // Limita a la velocidad máxima
            }

            animator.SetBool("esCorriendoFuerte", false); // Desactiva la animación si no tiene bono
        }



        // Manejo del bonus de velocidad (alma roja)
        if (tieneBonusVelocidad)
        {
            tiempoBonusVelocidad -= Time.deltaTime;

            if (tiempoBonusVelocidad <= 0)
            {
                tieneBonusVelocidad = false; // Termina el bonus de velocidad
                animator.SetBool("esCorriendoFuerte", false); // Desactiva la animación de correr rápido

                velocidadMovimiento = velocidadBase; // Restauramos la velocidad original
            }
            else
            {
                // Aplicamos el bonus de velocidad
                velocidadMovimiento = velocidadBase * incrementoVelocidadBonus;
                animator.SetBool("esCorriendoFuerte", true); // Activa la animación de correr rápido
            }
        }
        else
        {
            // Si no tiene el bonus, aplicamos la aceleración
            if (velocidadMovimiento < velocidadMaxima)
            {
                velocidadMovimiento += incrementoVelocidad * Time.deltaTime;
                velocidadMovimiento = Mathf.Min(velocidadMovimiento, velocidadMaxima); // Limita a la velocidad máxima
            }

            animator.SetBool("esCorriendoFuerte", false); // Desactiva la animación si no tiene bono
        }

        // Actualizar animaciones basadas en detección
        detectarObstaculo = DetectarColisionConObstaculo();
        animator.SetBool("esObstaculo", detectarObstaculo);

        detectarPared = DetectarColisionConPared();
        animator.SetBool("esPared", detectarPared);


        float velocidadFinal = velocidadMovimiento;
        if (ralentizado)
        {
            velocidadFinal *= multiplicadorRalentizar;
        }

        // Asignar la velocidad al Rigidbody2D
        rb2D.velocity = new Vector2(velocidadFinal, rb2D.velocity.y);

        // Comprobar si el jugador presiona el botón de salto
        if (Input.GetButton("Jump") || DetectarToquePantalla())
        {
            if (PuedeSaltar())
            {
                saltar = true;
            }
        }

        // Liberar el salto cuando el botón es soltado
        if (Input.GetButtonUp("Jump") || LiberarToquePantalla())
        {
            BotonSaltoArriba();
        }

        if (tieneAlmaVerde)
        {
            tiempoRestanteGravedadAlmaVerde -= Time.deltaTime;
            if (tiempoRestanteGravedadAlmaVerde <= 0)
            {
                tieneAlmaVerde = false;  // Desactivamos el efecto
                fuerzaSalto = 20;  // Restauramos la fuerza del salto a su valor original
            }
        }
        // Lógica del alma morada para aumentar la luminocidad del farolillo
        if (tieneAlmaMorada)
        {
            // Actualiza el efecto del alma morada
            Debug.Log("Alma Morada activada");
            tiempoRestanteAlmaMorada -= Time.deltaTime;

            if (tiempoRestanteAlmaMorada <= 0)
            {
                tieneAlmaMorada = false; // Termina el efecto del alma morada
                farolillo.intensity = 1f; // Restablece la luminosidad original
                farolillo.pointLightOuterRadius = 5f; // Restablece el rango del farolillo
            }
            else
            {
                // Aumenta la luminosidad del farolillo mientras tiene el alma morada
                farolillo.intensity = Mathf.Lerp(farolillo.intensity, 2f, Time.deltaTime * 2f);  // Aumenta la intensidad más rápido
                                                                                                 // Aumenta el radio de la luz
                farolillo.pointLightOuterRadius = Mathf.Lerp(farolillo.pointLightOuterRadius, 13f, Time.deltaTime * 2f);  // Aumenta el rango de la luz gradualmente
            }

        }
    }

    private void FixedUpdate()
    {
        // Si el jugador ya no tiene el alma verde, restablece la gravedad
        if (tiempoRestanteGravedadAlmaVerde <= 0)
        {
            tieneAlmaVerde = false;  // Resetea el estado para que ya no se aplique el efecto
        }
        // Actualiza el estado del Animator basado en si está en el suelo o no
        bool enSuelo = PuedeSaltar();
        animator.SetBool("esSuelo", enSuelo);

        // Manejo del salto
        if (saltar && botonSaltoArriba && enSuelo)
        {
            Saltar();
        }

        // Aumenta la gravedad cuando está descendiendo
        if (rb2D.velocity.y < 0)
        {
            rb2D.gravityScale = escalaGravedad * multiplicadorGravedad;
        }
        else
        {
            rb2D.gravityScale = escalaGravedad;
        }

        // Detecta colisión con paredes para manejar la velocidad
        if (DetectarColisionConPared() && rb2D.velocity.y > 0)
        {
            rb2D.velocity = new Vector2(rb2D.velocity.x, -Mathf.Abs(rb2D.velocity.y)); // Invertir la velocidad vertical
        }

        if (detectarObstaculo)
        {
            // Por ejemplo, reducir velocidad o iniciar un evento especial
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

        if (colision == null && Mathf.Abs(rb2D.velocity.y) < 0.01f)
        {
            // Permite al personaje estabilizarse
            rb2D.velocity = new Vector2(rb2D.velocity.x, 0);
        }

        return colision != null;
    }

    private bool DetectarToquePantalla()
    {
        if (Input.touchCount > 0)
        {
            Touch toque = Input.GetTouch(0);
            if (toque.phase == TouchPhase.Began)
            {
                return true;
            }
        }
        return false;
    }

    private bool LiberarToquePantalla()
    {
        if (Input.touchCount > 0)
        {
            Touch toque = Input.GetTouch(0);
            if (toque.phase == TouchPhase.Ended)
            {
                return true;
            }
        }
        return false;
    }

    private bool DetectarColisionConPared()
    {
        Vector2 posicionDetector = (Vector2)transform.position + offsetDetectorPared;
        Collider2D colision = Physics2D.OverlapBox(posicionDetector, tamañoDetectorPared, 0, capaPared);
        return colision != null;
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

    private bool DetectarColisionConObstaculo()
    {
        Vector2 posicionDetector = (Vector2)transform.position + offsetDetectorObstaculo;
        Collider2D colision = Physics2D.OverlapBox(posicionDetector, tamañoDetectorObstaculo, 0, capaObstaculo);
        return colision != null;
    }



    private void OnDrawGizmos()
    {


        // Visualización del detector de obstáculos
        Gizmos.color = Color.blue;
        Vector2 posicionDetectorObstaculo = (Vector2)transform.position + offsetDetectorObstaculo;
        Gizmos.DrawWireCube(posicionDetectorObstaculo, tamañoDetectorObstaculo);


        Gizmos.color = Color.green;
        Vector2 posicionDetectorPlataforma = (Vector2)transform.position + offsetDetectorPlataforma;
        Gizmos.DrawWireCube(posicionDetectorPlataforma, tamañoDetectorPlataforma);

        Gizmos.color = Color.red;
        Vector2 posicionDetectorPared = (Vector2)transform.position + offsetDetectorPared;
        Gizmos.DrawWireCube(posicionDetectorPared, tamañoDetectorPared);
    }
    private IEnumerator RevertirAnimacion(string parametro)
    {
        yield return new WaitForSeconds(1f); // Espera 1 segundo o el tiempo de la animación de choque.
        animator.SetBool(parametro, false);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Pared"))
        {
            animator.SetBool("esPared", true);
            StartCoroutine(RevertirAnimacion("esPared"));
        }
        else if (col.CompareTag("Obstaculo"))
        {
            animator.SetBool("esObstaculo", true);
            StartCoroutine(RevertirAnimacion("esObstaculo"));
        }
        if (col.CompareTag("AlmaRoja"))
        {
            if (!tieneBonusVelocidad)
            {
                Debug.Log("¡Bonus de velocidad activado!");
                tieneBonusVelocidad = true;  // Activa el bonus de velocidad
                tiempoBonusVelocidad = duracionBonusVelocidad;  // Reinicia el tiempo del bonus
            }
            Destroy(col.gameObject);  // Destruimos el alma verde al recogerla

        }

        if (col.CompareTag("AlmaVerde"))
        {
            if (!tieneAlmaVerde)
            {
                Debug.Log("Tiene su almita");
                tieneAlmaVerde = true;  // Activamos el efecto de alma verde
                tiempoRestanteGravedadAlmaVerde = 2;  // Reiniciamos el temporizador
                fuerzaSaltoBase = fuerzaSalto;
                // Aquí, aumentamos temporalmente la fuerza del salto
                fuerzaSalto *= 1.3f;  // Aumentamos la fuerza del salto (ajusta el valor según lo necesites)

            }
            Destroy(col.gameObject);  // Destruimos el alma verde al recogerla
        }
        if (col.CompareTag("AlmaMorada"))
        {

            Debug.Log("¡Has recogido el alma morada!");
            tieneAlmaMorada = true; // Activa el efecto del alma morada
            tiempoRestanteAlmaMorada = tiempoAlmaMorada;  // Restablece el tiempo de efecto
            Destroy(col.gameObject);  // Destruye el alma morada recogida
        }
        if (col.CompareTag("AlmaEspecial"))
        {
            tieneAlmaEspecial = true; // Activa el alma especial
            tiempoRestanteAlmaEspecial = tiempoAlmaEspecial; // Reinicia el tiempo restante
            Destroy(col.gameObject); // Destruye el alma especial
        }
    }
    public float fuerzaSaltoBase;
    private IEnumerator RestaurarVelocidad()
    {
        yield return new WaitForSeconds(2f); // Espera 2 segundos
        velocidadMovimiento /= 2; // Restaurar la velocidad original
    }

}