using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneradorNivel : MonoBehaviour
{
    [SerializeField] private GameObject[] partesNivel; // Partes del nivel para generar
    [SerializeField] private float distanciaMinima; // Distancia mínima para generar nueva parte
    [SerializeField] private Transform puntoFinal; // Punto de referencia para colocar nuevas partes
    [SerializeField] private int cantidadInicial; // Cantidad inicial de partes a generar
    private Transform jugador; // Transform del jugador

    private void Start()
    {
        // Buscar el jugador
       jugador = GameObject.FindGameObjectWithTag("Player").transform;

        // Generar partes iniciales del nivel
        for (int i = 0; i < cantidadInicial; i++)
        {
            GenerarParteNivel();
        }
    }

    private void Update()
    {  
        // Generar nueva parte del nivel si está cerca del punto final
        if (Vector2.Distance(jugador.position, puntoFinal.position) < distanciaMinima)
        {
            GenerarParteNivel();
        }
    }

    private void GenerarParteNivel()
    {
        // Elegir una parte de nivel aleatoria
        int numeroAleatorio = Random.Range(0, partesNivel.Length);
        GameObject nivel = Instantiate(partesNivel[numeroAleatorio], puntoFinal.position, Quaternion.identity);

        // Actualizar el punto final para la siguiente parte
        puntoFinal = BuscarPuntoFinal(nivel, "PuntoFinal");
    }

    private Transform BuscarPuntoFinal(GameObject parteNivel, string etiqueta)
    {
        Transform punto = null;

        // Buscar el transform con la etiqueta específica dentro de la parte del nivel
        foreach (Transform ubicacion in parteNivel.transform)
        {
            if (ubicacion.CompareTag(etiqueta))
            {
                punto = ubicacion;
                break;

            }
        }

        return punto; 
    }
}
