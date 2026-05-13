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

    private static volatile bool enrollReady = false;
    public static bool EnrollReady => enrollReady;
    public static void ClearEnrollReady() => enrollReady = false;

    private static volatile bool enrollRemove = false;
    public static bool EnrollRemove => enrollRemove;
    public static void ClearEnrollRemove() => enrollRemove = false;

    void Start()
    {
        newFingerprintReceived = false;
        authFailed = false;
        enrollReady = false;
        enrollRemove = false;
        LastFingerprintID = "";
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

    public void SendCommand(string cmd)
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.WriteLine(cmd);
            Debug.Log("Sent to Arduino: " + cmd);
        }
        else
        {
            Debug.LogWarning("Serial port not open — cannot send: " + cmd);
        }
    }

    void ReadSerialData()
    {
        while (isRunning && serialPort != null && serialPort.IsOpen)
        {
            try
            {
                string data = serialPort.ReadLine().Trim();
                if (data.StartsWith("AUTH_SUCCESS:"))
                {
                    LastFingerprintID = data.Substring("AUTH_SUCCESS:".Length);
                    newFingerprintReceived = true;
                }
                else if (data == "ENROLL_READY")
                {
                    enrollReady = true;
                }
                else if (data == "ENROLL_REMOVE")
                {
                    enrollRemove = true;
                }
                else if (data == "AUTH_FAIL")
                {
                    authFailed = true;
                }
            }
            catch { }
        }
    }

    void OnDestroy()
    {
        isRunning = false;
        if (serialPort != null && serialPort.IsOpen)
            serialPort.Close();
    }

    void OnApplicationQuit()
    {
        isRunning = false;
        if (serialPort != null && serialPort.IsOpen)
            serialPort.Close();
    }
}
