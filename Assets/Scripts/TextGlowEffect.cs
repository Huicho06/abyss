using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextGlowEffect : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro; // Asigna el texto en el inspector
    private float glowPower = 1.0f;    // Brillo inicial
    private bool increasing = true;    // Dirección del brillo

    void Update()
    {
        // Oscila el brillo entre 0.5 y 2.0
        if (increasing)
        {
            glowPower += Time.deltaTime;
            if (glowPower >= 2.0f) increasing = false;
        }
        else
        {
            glowPower -= Time.deltaTime;
            if (glowPower <= 0.5f) increasing = true;
        }

        // Ajusta el brillo del texto
        textMeshPro.material.SetFloat("_GlowPower", glowPower);
    }
}