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

    // Fix: use a private setter accessed via method
    private static bool newFingerprintReceived = false;
    public static bool NewFingerprintReceived
    {
        get { return newFingerprintReceived; }
    }
    public static void ClearFingerprint()
    {
        newFingerprintReceived = false;
    }

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
                if (!string.IsNullOrEmpty(data))
                {
                    LastFingerprintID = data;
                    newFingerprintReceived = true;
                }
            }
            catch { }
        }
    }

    void OnApplicationQuit()
    {
        isRunning = false;
        if (serialPort != null && serialPort.IsOpen)
            serialPort.Close();
    }
}