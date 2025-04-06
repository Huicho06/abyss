using UnityEngine;
using UnityEngine.UI;

public class CoinCollector : MonoBehaviour
{
    public float coins; // Monedas del jugador
    public Text txtCoins; // UI de monedas
    public GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        txtCoins.text = "Coins: " + coins.ToString();
    }

    public void AddCoins(float amount)
    {
    
        

        coins += amount;
        txtCoins.text = "Coins: " + coins.ToString();
    }
}
