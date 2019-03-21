using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
// For Serial
using System.IO.Ports;
// For compare array
using System.Linq;

public class Arduino2 : MonoBehaviour { 
	// Serial
	public static HashSet<int> touchID = new HashSet<int>();
    public static Stack<string> touchShowID = new Stack<string>();
    public string portName;
	public int baudRate = 19200;
	SerialPort arduinoSerial;
	public string[] a;

	public int sensor;

	void Start () {
		// Open Serial port
		arduinoSerial = new SerialPort (portName, baudRate);
		// Set buffersize so read from Serial would be normal
		arduinoSerial.ReadTimeout = 50;
		arduinoSerial.ReadBufferSize = 8192;
		arduinoSerial.WriteBufferSize = 128;
		arduinoSerial.ReadBufferSize = 4096;
		arduinoSerial.Parity = Parity.None;
		arduinoSerial.StopBits = StopBits.One;
		arduinoSerial.DtrEnable = true;
		arduinoSerial.RtsEnable = true;;
		arduinoSerial.Open ();
	}

	void Update() {
		ReadFromArduino ();
	}

	public void ReadFromArduino () {
		string str = null;

		//int num;
		try {
            str = arduinoSerial.ReadLine();
            if (str.Length >= 3)
            {
                touchShowID.Push(str);
            }
        }
		catch (TimeoutException) {
		}
	}
}