using UnityEngine;
using System.IO.Ports;
using System.Threading;

public class FingerprintReader : MonoBehaviour
{
    [Header("Serial Settings")]
    public string portName = "COM3";
    public int baudRate = 9600;

    private SerialPort serialPort;
    private Thread readThread;
    private bool isRunning = false;

    public static string LastFingerprintID { get; private set; } = "";

    private static volatile bool newFingerprintReceived = false;
    public static bool NewFingerprintReceived => newFingerprintReceived;
    public static void ClearFingerprint() => newFingerprintReceived = false;

    private static volatile bool authFailed = false;
    public static bool AuthFailed => authFailed;
    public static void ClearAuthFailed() => authFailed = false;

    void Start()
    {
        OpenSerialPort();
    }

    void OpenSerialPort()
    {
        try
        {
            serialPort = new SerialPort(portName, baudRate);
            serialPort.Open();
            isRunning = true;
            readThread = new Thread(ReadSerialData);
            readThread.IsBackground = true;
            readThread.Start();
            Debug.Log("Fingerprint reader connected on " + portName);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Could not open serial port: " + e.Message);
        }
    }

    void ReadSerialData()
    {
        while (isRunning && serialPort != null && serialPort.IsOpen)
        {
            try
            {
                string data = serialPort.ReadLine().Trim();
                Debug.Log("[FingerprintReader] Received: " + data);
                if (data.StartsWith("AUTH_SUCCESS:"))
                {
                    LastFingerprintID = data.Substring("AUTH_SUCCESS:".Length).Trim();
                    newFingerprintReceived = true;
                }
                else if (data == "AUTH_FAIL")
                {
                    authFailed = true;
                }
            }
            catch (System.Exception e)
            {
                if (isRunning)
                    Debug.LogWarning("[FingerprintReader] Read error: " + e.Message);
            }
        }
    }

    void OnApplicationQuit()
    {
        isRunning = false;
        if (serialPort != null && serialPort.IsOpen)
            serialPort.Close();
    }
}