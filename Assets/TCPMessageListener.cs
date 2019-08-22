using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Net.Sockets;

public class TCPMessageListener : MonoBehaviour
{
    private TcpClient client;

    private int port;
    // Start is called before the first frame update
    void Start()
    {
        port = 8000;
        Connect("127.0.0.1", "Client Saying Hello");
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void Connect(string server, string message)
    {
        client = new TcpClient(server, port);
        byte[] data = System.Text.Encoding.ASCII.GetBytes(message);
        NetworkStream stream = client.GetStream();
        stream.Write(data, 0, data.Length);
        Debug.Log("Sent: " +  message);

        // Buffer to store the response bytes.
        data = new byte[256];

        // String to store the response ASCII representation.
        string responseData = string.Empty;

        // Read the first batch of the TcpServer response bytes.
        int bytes = stream.Read(data, 0, data.Length);
        responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
        Debug.Log("Received: " + responseData);

        // Close everything.
        stream.Close();
        client.Close();
    }
}
