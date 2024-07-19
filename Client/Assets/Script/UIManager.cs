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
            // 카드 정보 업데이트 처리
            UpdateCardImage(message);
        }
        else if (message.Contains("wins and takes the pot"))
        {
            // 게임 종료 처리
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
        // 카드 이미지 업데이트 로직
        string cardValue = message.Split(':')[1].Trim();
        // 예시 카드 이미지를 로드합니다. 실제 카드 이미지는 Resources 폴더에 있어야 합니다.
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
