using System.Collections;
using UnityEngine;

public class Enemigo : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform jugador; // Transform del jugador
    private Player playerController; // Referencia al script del jugador para obtener su velocidad
    private float velocidadEnemigo; // Velocidad del enemigo (acelerar� hasta la velocidad m�xima)

    [Header("Efectos de Muerte")]
    [SerializeField] private float tiempoOscurecer = 2f; // Tiempo para oscurecer la pantalla

    private GameManager gameManager; // Referencia al GameManager

    [Header("Velocidad del Enemigo")]
    [SerializeField] private float velocidadMaximaEnemigo = 10f; // Velocidad m�xima del enemigo
    [SerializeField] private float incrementoVelocidadEnemigo = 2f; // Incremento en la velocidad del enemigo
    [SerializeField] private float velocidadInicialEnemigo = 4f; // Velocidad inicial del enemigo

    private void Start()
    {
        // Buscar al jugador si no est� asignado manualmente
        if (jugador == null)
        {
            jugador = GameObject.FindGameObjectWithTag("Player").transform;
        }

        // Obtener el componente del jugador que contiene su velocidad
        if (jugador != null)
        {
            playerController = jugador.GetComponent<Player>();
            if (playerController == null)
            {
                Debug.LogError("El script PlayerController no se encuentra en el jugador.");
            }
        }

        // Obtener la referencia al GameManager
        gameManager = FindObjectOfType<GameManager>();

        // Establecer velocidad inicial
        velocidadEnemigo = velocidadInicialEnemigo;
    }

    private void Update()
    {
        // Verifica si el jugador y el controlador del jugador est�n asignados
        if (jugador != null && playerController != null)
        {
            // Aumenta gradualmente la velocidad del enemigo hasta la velocidad m�xima
            if (velocidadEnemigo < velocidadMaximaEnemigo)
            {
                velocidadEnemigo += incrementoVelocidadEnemigo * Time.deltaTime;
                velocidadEnemigo = Mathf.Min(velocidadEnemigo, velocidadMaximaEnemigo); // Limita la velocidad m�xima
            }

            // Mostrar velocidad del enemigo en la consola para depuraci�n

            // Movimiento del enemigo hacia el jugador
            Vector2 direccion = (jugador.position - transform.position).normalized; // Direcci�n hacia el jugador
            transform.position += (Vector3)direccion * velocidadEnemigo * Time.deltaTime; // Movimiento
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Verificar si colisiona con el jugador
        if (collision.gameObject.CompareTag("Player"))
        {
            // Detener el tiempo
            Time.timeScale = 0f;
            // Iniciar el efecto de muerte
            StartCoroutine(EfectoMuerte());
        }
    }

    private IEnumerator EfectoMuerte()
    {
        // Oscurecer la pantalla lentamente
        if (gameManager != null)
        {
            gameManager.JugadorMurio(); // Llamamos al GameManager para manejar la muerte
        }

        // Esperar un poco y mostrar el men� de muerte
        yield return new WaitForSeconds(1f);
        // La l�gica para mostrar el men� ya est� en el GameManager.
    }
}
