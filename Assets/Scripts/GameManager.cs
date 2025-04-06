using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Para manejar la UI

public class GameManager : MonoBehaviour
{
    [Header("Referencias de UI")]
    [SerializeField] private CanvasGroup pantallaOscura; // Canvas para la transici�n a negro
    [SerializeField] private GameObject menuMuerte; // Panel del men� de muerte
    [SerializeField] private GameObject menuPausa; // Panel del men� de pausa
    [SerializeField] private Text textoPuntuacionActual; // Texto para la puntuaci�n actual
    [SerializeField] private Text textoMejorPuntuacion; // Texto para la mejor puntuaci�n
    [SerializeField] private Text textoPuntuacionTiempoReal; // Texto para la puntuaci�n en tiempo real
    [SerializeField] private Slider sliderVolumen; // Slider para ajustar el volumen de la m�sica

    [Header("Par�metros")]
    [SerializeField] private float tiempoOscurecimiento = 2f; // Duraci�n total del oscurecimiento
    [SerializeField] private AudioClip sonidoMuerte; // Sonido al morir
    [SerializeField] private Transform jugador; // Referencia al transform del jugador



    private AudioSource audioSource;
    private int puntuacionActual = 0; // Puntuaci�n actual
    private int mejorPuntuacion; // Mejor puntuaci�n guardada
    private float posicionInicialX; // Posici�n inicial del jugador en el eje X
    private bool jugadorMuerto = false;
    private bool juegoPausado = false;

    private static GameManager instancia; // Instancia �nica de GameManager

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        menuMuerte.SetActive(false);
        menuPausa.SetActive(false);
        pantallaOscura.alpha = 0;
        if (instancia == null)
        {
            instancia = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instancia != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
     
        // Inicializar la UI y cargar el mejor puntaje
        if (pantallaOscura != null)
        {
            pantallaOscura.alpha = 0;
            pantallaOscura.gameObject.SetActive(false);
        }
        if (menuMuerte != null)
        {
            menuMuerte.SetActive(false);
        }
        if (menuPausa != null)
        {
            menuPausa.SetActive(false);
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Intenta encontrar al jugador si no se asigna en el inspector
        if (jugador == null)
        {
            jugador = GameObject.FindWithTag("Player")?.transform; // Aseg�rate de que el jugador tenga la etiqueta "Player"
        }

        if (jugador != null)
        {
            posicionInicialX = jugador.position.x;
        }
        else
        {
            Debug.LogError("El transform del jugador no se ha asignado correctamente.");
        }
        mejorPuntuacion = PlayerPrefs.GetInt("MejorPuntuacion", 0);
        ActualizarTextoPuntuacion();
    }

    private void Update()
    {
        if (!jugadorMuerto && jugador != null)
        {
            puntuacionActual = Mathf.FloorToInt(jugador.position.x - posicionInicialX);
            ActualizarTextoPuntuacionTiempoReal();
        }

        // Comprobar si se presiona la tecla ESC para mostrar el men� de pausa
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!juegoPausado)
            {
                PausarJuego();
            }
            else
            {
                ReanudarJuego();
            }
        }
    }

    public void JugadorMurio()
    {
        if (jugadorMuerto) return;
        jugadorMuerto = true;

        if (puntuacionActual > mejorPuntuacion)
        {
            mejorPuntuacion = puntuacionActual;
            PlayerPrefs.SetInt("MejorPuntuacion", mejorPuntuacion);
        }

        if (pantallaOscura != null)
        {
            pantallaOscura.gameObject.SetActive(true);
            StartCoroutine(OscurecerPantallaYMostrarMenu());
        }

        if (sonidoMuerte != null)
        {
            audioSource.PlayOneShot(sonidoMuerte);
        }

    }

    private IEnumerator OscurecerPantallaYMostrarMenu()
    {
        float tiempoTranscurrido = 0f;
        float velocidadOscurecimiento = 1f / tiempoOscurecimiento;

        while (pantallaOscura.alpha < 1)
        {
            pantallaOscura.alpha += Time.unscaledDeltaTime * velocidadOscurecimiento;
            tiempoTranscurrido += Time.unscaledDeltaTime;
            yield return null;
        }

        pantallaOscura.alpha = 1;
        MostrarMenuMuerte();
    }

    private void MostrarMenuMuerte()
    {
        if (menuMuerte != null)
        {
            menuMuerte.SetActive(true);
        }

        if (textoPuntuacionActual != null)
        {
            textoPuntuacionActual.text = $"Puntuaci�n: {puntuacionActual}";
        }
        if (textoMejorPuntuacion != null)
        {
            textoMejorPuntuacion.text = $"Mejor Puntuaci�n: {mejorPuntuacion}";
        }
    }

    public void Reintentar()
    {
        menuMuerte.SetActive(false);
        menuPausa.SetActive(false);
        pantallaOscura.alpha = 0;
        jugadorMuerto = false;
        Time.timeScale = 1f;
        // Ocultar el Canvas Oscuro y el men� de muerte antes de cargar la escena
        if (pantallaOscura != null)
        {
            pantallaOscura.alpha = 0;
            pantallaOscura.gameObject.SetActive(false);
        }
        if (menuMuerte != null)
        {
            menuMuerte.SetActive(false);
        }
        if (menuPausa != null)
        {
            menuPausa.SetActive(false);
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void VolverAlMenu()
    {
       
        Time.timeScale = 1f;   
        SceneManager.LoadScene("Menu");
        jugadorMuerto = false;
        menuMuerte.SetActive(false);
        menuPausa.SetActive(false);
        pantallaOscura.alpha = 0;
    }

    private void ActualizarTextoPuntuacionTiempoReal()
    {
        if (textoPuntuacionTiempoReal != null)
        {
            textoPuntuacionTiempoReal.text = $"Puntuaci�n: {puntuacionActual}";
        }
    }

    private void ActualizarTextoPuntuacion()
    {
        if (textoMejorPuntuacion != null)
        {
            textoMejorPuntuacion.text = $"Mejor Puntuaci�n: {mejorPuntuacion}";
        }
    }

    

    public void PausarJuego()
    {
        Time.timeScale = 0f;
        juegoPausado = true;

        if (menuPausa != null)
        {
            menuPausa.SetActive(true);
        }
    }

    public void ReanudarJuego()
    {
        Time.timeScale = 1f;
        juegoPausado = false;

        if (menuPausa != null)
        {
            menuPausa.SetActive(false);
        }
    }

    public void AjustarVolumen(float volumen)
    {
        if (audioSource != null)
        {
            audioSource.volume = volumen;
        }
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reasignar referencias a UI reales de la escena recargada
        if (textoPuntuacionTiempoReal == null)
            textoPuntuacionTiempoReal = GameObject.Find("scoreText")?.GetComponent<Text>();

        if (jugador == null)
            jugador = GameObject.FindWithTag("Player")?.transform;

        ActualizarTextoPuntuacion();
    }


}
