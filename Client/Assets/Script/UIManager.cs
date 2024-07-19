using System;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public InputField betInputField;
    public Text messageBox;
    public Text coinText;
    public Text turnText;
    public Image cardImage;
    private int coins = 30;
    private bool isMyTurn = false;

    public NetworkManager networkManager;

    public void HandleServerMessage(string message)
    {
        messageBox.text += "\n" + message;

        if (message.Contains("Your card:"))
        {
            // ī�� ���� ������Ʈ ó��
            UpdateCardImage(message);
        }
        else if (message.Contains("wins and takes the pot"))
        {
            // ���� ���� ó��
            isMyTurn = false;
        }
        else if (message.Contains("Your turn to bet"))
        {
            isMyTurn = true;
            turnText.text = "It's your turn to bet.";
        }
        else if (message.Contains("turn to bet"))
        {
            isMyTurn = false;
            turnText.text = "";
        }
    }

    void UpdateCardImage(string message)
    {
        Debug.Log("myCard : "+message);
        // ī�� �̹��� ������Ʈ ����
        string cardValue = message.Split(':')[1].Trim();
        // ���� ī�� �̹����� �ε��մϴ�. ���� ī�� �̹����� Resources ������ �־�� �մϴ�.
        cardImage.sprite = Resources.Load<Sprite>($"Cards/{cardValue}");
    }

    public void PlaceBet()
    {
        if (!isMyTurn)
        {
            Debug.LogError("It's not your turn to bet.");
            return;
        }

        if (!int.TryParse(betInputField.text, out int bet))
        {
            Debug.LogError("Invalid bet amount.");
            return;
        }

        if (bet > coins)
        {
            Debug.LogError("Not enough coins.");
            return;
        }

        coins -= bet;
        coinText.text = $"Coins: {coins}";

        string message = $"bet {bet}";
        networkManager.SendMessage(message);
        betInputField.text = string.Empty;

        isMyTurn = false;  // Turn is now over
    }

    public void SendMessage()
    {
        string message = betInputField.text;
        networkManager.SendMessage(message);
        betInputField.text = string.Empty;
    }

    public void StartGame()
    {
        networkManager.StartGame();
    }
}
