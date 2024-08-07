using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public InputField betInputField;
    public InputField messageInputField;
    public Text messageBox;
    public Text coinText;
    public Text turnText;
    public Image cardImage;
    public Transform chatContentParent;
    public GameObject chatPrefab;
    private int coins = 30;
    private bool isMyTurn = false;

    public NetworkManager networkManager;

    public void HandleServerMessage(string message)
    {
        messageBox.text += "\n" + message;

        if (message.StartsWith("Your card:"))
        {
            // ī�� ���� ������Ʈ ó��
            UpdateCardImage(message);
        }
        else if (message.StartsWith("wins and takes the pot"))
        {
            // ���� ���� ó��
            isMyTurn = false;
        }
        else if (message.StartsWith("Your turn to bet"))
        {
            isMyTurn = true;
            turnText.text = "It's your turn to bet.";
        }
        else if (message.StartsWith("turn to bet"))
        {
            isMyTurn = false;
            turnText.text = "";
        }
        else if (message.StartsWith("Chat"))
        {
            string trimmedMessage = message.Substring(5); // "Chat "
            ProcessMessage(trimmedMessage);
        }

    }

    void ProcessMessage(string message)
    {
        Debug.Log(message);
        // Instantiate chat object from the prefab
        GameObject chatObj = Instantiate(chatPrefab, chatContentParent);

        // Change the text of the chat object to the message
        TextMeshProUGUI chatText = chatObj.GetComponent<TextMeshProUGUI>();
        if (chatText != null)
        {
            chatText.text = message;
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
            Debug.Log("It's not your turn to bet.");
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
        string message = messageInputField.text;
        networkManager.SendMessage(message);
        messageInputField.text = string.Empty;
    }

    public void StartGame()
    {
        networkManager.StartGame();
    }
}
