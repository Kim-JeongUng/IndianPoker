using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    private TcpClient client;
    private NetworkStream stream;
    private byte[] buffer = new byte[1024];

    public UIManager uiManager;

    void Start()
    {
        ConnectToServer("127.0.0.1", 5000);
    }

    void ConnectToServer(string ipAddress, int port)
    {
        try
        {
            client = new TcpClient(ipAddress, port);
            stream = client.GetStream();
            Debug.Log("Connected to server!");

            stream.BeginRead(buffer, 0, buffer.Length, OnRead, null);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to connect: {e.Message}");
        }
    }

    void OnRead(IAsyncResult ar)
    {
        try
        {
            int bytesRead = stream.EndRead(ar);
            if (bytesRead > 0)
            {
                string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Debug.Log($"Received from server: {message}");
                uiManager.HandleServerMessage(message);

                stream.BeginRead(buffer, 0, buffer.Length, OnRead, null);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Read error: {e.Message}");
        }
    }

    public void SendMessage(string message)
    {
        if (client == null || !client.Connected)
        {
            Debug.LogError("Not connected to server.");
            return;
        }

        byte[] data = Encoding.ASCII.GetBytes(message);
        stream.Write(data, 0, data.Length);
    }

    public void StartGame()
    {
        SendMessage("start game");
    }

    void OnApplicationQuit()
    {
        if (client != null)
        {
            stream.Close();
            client.Close();
        }
    }
}
