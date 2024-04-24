using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using TMPro;
using UnityEngine;

public class Server : MonoBehaviour
{
    [SerializeField] RectTransform logContent;
    [SerializeField] TMP_Text logTextPrefab;
    [SerializeField] TMP_InputField ipField;
    [SerializeField] TMP_InputField portField;

    TcpListener listener;
    List<TcpClient> clients = new List<TcpClient>();
    List<TcpClient> disConnects = new List<TcpClient>();

    IPAddress ip;
    int port;

    bool isOpened;
    public bool IsOpended { get { return isOpened; } }

    private void Start()
    {
        IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
        ip = host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
        ipField.text = ip.ToString();
    }

    private void Update()
    {
        if (!IsOpended)
            return;

        foreach (TcpClient client in clients)
        {
            if (!CheckClient(client))
            {
                client.Close();
                disConnects.Add(client);
                continue;
            }

            NetworkStream stream = client.GetStream();
            if (stream.DataAvailable)
            {
                StreamReader reader = new StreamReader(stream);
                string text = reader.ReadLine();
                AddLog(text);
                SendAll(text);
            }
        }

        foreach (TcpClient client in disConnects)
        {
            clients.Remove(client);
        }
        disConnects.Clear();
    }

    private void OnDestroy()
    {
        if (isOpened)
            Close();
    }

    public void Open()
    {
        if (isOpened)
            return;

        Debug.Log("Try to Open");
        port = int.Parse(portField.text);

        try
        {
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();

            isOpened = true;
            listener.BeginAcceptTcpClient(AcceptCallback, listener);
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    public void Close()
    {
        listener?.Stop();
        listener = null;

        isOpened = false;
    }

    public void SendAll(string chat)
    {
        foreach (TcpClient client in clients)
        {
            NetworkStream stream = client.GetStream();
            StreamWriter writer = new StreamWriter(stream);

            try
            {
                writer.WriteLine(chat);
                writer.Flush();
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
            }
        }
    }

    void AcceptCallback(IAsyncResult ar)
    {
        if (!isOpened)
            return;

        TcpClient client = listener.EndAcceptTcpClient(ar);
        clients.Add(client);
        Debug.Log("Client Connected");
        listener.BeginAcceptTcpClient(AcceptCallback, listener);
    }

    private void AddLog(string message)
    {
        Debug.Log($"[Server] {message}");
        TMP_Text newLog = Instantiate(logTextPrefab, logContent);
        newLog.text = message;
    }

    bool CheckClient(TcpClient client)
    {
        try
        {
            if (client == null)
                return false;

            if (!client.Connected)
                return false;

            bool check = client.Client.Poll(0, SelectMode.SelectRead);
            if (!check)
                return false;

            int size = client.Client.Receive(new byte[1], SocketFlags.Peek);
            if(size == 0) 
                return false;

            return true;

            //if (client != null && client.Client != null && client.Connected)
            //{
            //    if (client.Client.Poll(0, SelectMode.SelectRead))
            //        return !(client.Client.Receive(new byte[1], SocketFlags.Peek) == 0);

            //    return true;
            //}
            //else
            //    return false;
        }
        catch (Exception ex)
        {            
            AddLog(ex.Message);
            return false;
        }
    }
}
