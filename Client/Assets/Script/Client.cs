using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class Client : MonoBehaviour
{
    private TcpClient client;
    private NetworkStream stream;

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
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to connect: {e.Message}");
        }
    }

    void SendMessage(string message)
    {
        if (client == null || !client.Connected)
        {
            Debug.LogError("Not connected to server.");
            return;
        }

        byte[] data = Encoding.ASCII.GetBytes(message);
        stream.Write(data, 0, data.Length);

        byte[] responseData = new byte[1024];
        int bytesRead = stream.Read(responseData, 0, responseData.Length);
        string responseMessage = Encoding.ASCII.GetString(responseData, 0, bytesRead);
        Debug.Log($"Received from server: {responseMessage}");
    }

    void OnApplicationQuit()
    {
        if (client != null)
        {
            stream.Close();
            client.Close();
        }
    }

    // Example usage: Call this function to send a message
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SendMessage("Hello from Unity client!");
        }
    }
}