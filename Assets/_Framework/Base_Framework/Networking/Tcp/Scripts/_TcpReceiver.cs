using System.Collections.Generic;
using System;
using UnityEngine;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using _Base_Framework;
using System.Text;
using System.Threading;

namespace _Base_Framework
{
    public class _TcpReceiver : TCPTestClient
    {
        private static string LOG_FORMAT = "<color=#00FF62><b>[_TcpReceiver]</b></color> {0}";

		[SerializeField]
		protected string hostname = "localhost";
		[SerializeField]
		protected int port = 8052;

		protected virtual void OnDestroy()
		{
			if (clientReceiveThread != null)
			{
				// clientReceiveThread.Join();
				clientReceiveThread.Abort();

				clientReceiveThread = null;
			}
		}

		protected override void Start()
		{
			Debug.LogFormat(LOG_FORMAT, "Start()");

			ConnectToTcpServer();
		}

		// Update is called once per frame
		protected override void Update()
		{
#if DEBUG
			if (Input.GetKeyDown(KeyCode.Space))
			{
				SendData("This is a message from one of your clients.");
			}
#endif
		}

		/// <summary> 	
		/// Setup socket connection.
		/// </summary> 	
		protected override void ConnectToTcpServer()
		{
			try
			{
				clientReceiveThread = new Thread(new ThreadStart(ListenForData));
				clientReceiveThread.IsBackground = true;
				clientReceiveThread.Start();
			}
			catch (Exception e)
			{
				Debug.LogErrorFormat(LOG_FORMAT, "On client connect exception " + e);
			}
		}
		/// <summary> 	
		/// Runs in background clientReceiveThread; Listens for incomming data.
		/// </summary>     
		protected override void ListenForData()
		{
			Debug.LogWarningFormat(LOG_FORMAT, "ListenForData(), hostname : <b><color=yellow>" + hostname + "</color></b>, port : <b><color=yellow>" + port + "</color></b>");
			try
			{
				socketConnection = new TcpClient(hostname, port);
				Byte[] bytes = new Byte[1024];
				while (true)
				{
					// Get a stream object for reading
					using (NetworkStream stream = socketConnection.GetStream())
					{
						int length;
						// Read incomming stream into byte arrary. 					
						while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
						{
							var incommingData = new byte[length];
							Array.Copy(bytes, 0, incommingData, 0, length);
							// Convert byte array to string message. 						
							string serverData = Encoding.ASCII.GetString(incommingData);
							Debug.LogFormat(LOG_FORMAT, "Received data FROM <b><color=magenta>Server</color></b> : <b><color=yellow>" + serverData + "</color></b>");
						}
					}
				}
			}
			catch (SocketException socketException)
			{
				Debug.LogErrorFormat(LOG_FORMAT, "hostname : " + hostname + ", port : " + port + ", Socket exception: " + socketException);
			}
		}

		protected virtual void SendData(string data)
		{
			Debug.LogWarningFormat(LOG_FORMAT, "SendData(), data : <b><color=yellow>" + data + "</color></b>");
			if (socketConnection == null)
			{
				Debug.LogWarningFormat(LOG_FORMAT, "Can not Send Data!!!!! cuz, <b>socketConnection == null</b>");
				return;
			}

			try
			{
				// Get a stream object for writing.
				NetworkStream stream = socketConnection.GetStream();
				if (stream.CanWrite)
				{
					// Convert string data to byte array.                 
					byte[] clientDataAsByteArray = Encoding.ASCII.GetBytes(data);
					// Write byte array to socketConnection stream.                 
					stream.Write(clientDataAsByteArray, 0, clientDataAsByteArray.Length);
					Debug.LogFormat(LOG_FORMAT, "<b><color=magenta>Client</color></b> sent his data TO <b><color=yellow>Server</color></b> - should be received by server");
				}
			}
			catch (SocketException socketException)
			{
				Debug.LogErrorFormat(LOG_FORMAT, "Socket exception: " + socketException);
			}
		}
	}
}