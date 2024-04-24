using System;
using System.IO;
using System.Net.Sockets;
using TMPro;
using UnityEngine;

public class Client : MonoBehaviour
{
    [SerializeField] Chat chat;

    [SerializeField] TMP_InputField nameField;
    [SerializeField] TMP_InputField ipField;
    [SerializeField] TMP_InputField portField;

    TcpClient client;
    NetworkStream stream;
    StreamWriter writer;
    StreamReader reader;

    string clientName;
    string ip;
    int port;

    bool isConnected;
    public bool IsConnected { get { return isConnected; } }

    private void Update()
    {
        if (!IsConnected)
            return;

        if (!stream.DataAvailable)
            return;

        string text = reader.ReadLine();
        ReceiveChat(text);
    }

    public void Connect()
    {
        if (isConnected)
            return;

        clientName = nameField.text;
        ip = ipField.text;
        port = int.Parse(portField.text);

        try
        {
            client = new TcpClient(ip, port);
            stream = client.GetStream();
            writer = new StreamWriter(stream);
            reader = new StreamReader(stream);

            Debug.Log("Connect Success");
            isConnected = true;
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    public void DisConnect()
    {
        writer?.Close();
        writer = null;
        reader?.Close();
        reader = null;
        stream?.Close();
        stream = null;
        client?.Close();
        client = null;

        isConnected = false;
    }

    public void SendChat(string chatText)
    {
        if (!isConnected)
            return;

        Debug.Log($"Client SendMassage : {chatText}");

        try
        {
            writer.WriteLine($"{clientName} : {chatText}");
            writer.Flush();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    public void ReceiveChat(string chatText)
    {
        chat.AddMessage(chatText);
    }

    private void AddMessage(string message)
    {
        Debug.Log($"[Client] {message}");
        chat.AddMessage($"[Client] {message}");
    }
}
