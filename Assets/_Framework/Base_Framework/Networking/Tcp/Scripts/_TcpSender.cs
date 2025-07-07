using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Linq;
using System.Text;

namespace _Base_Framework
{
    public class _TcpSender : TCPTestServer
    {
        private static string LOG_FORMAT = "<color=#00FF62><b>[_TcpSender]</b></color> {0}";

		[SerializeField]
		protected string localAddress = "127.0.0.1";
		[SerializeField]
		protected int port = 8052;

		protected override void Start()
		{
			// Start TcpServer background thread 		
			tcpListenerThread = new Thread(new ThreadStart(ListenForIncommingRequests));

			tcpListenerThread.IsBackground = true;
			tcpListenerThread.Start();
		}

		protected virtual void OnDestroy()
		{
			if (tcpListenerThread != null)
			{
				// tcpListenerThread.Join();
				tcpListenerThread.Abort();

				tcpListenerThread = null;
			}
		}

		// Update is called once per frame
		protected override void Update()
		{
#if DEBUG
			if (Input.GetKeyDown(KeyCode.Space))
			{
				SendData("This is a message from your server.");
			}
#endif
		}

		/// <summary> 	
		/// Runs in background TcpServerThread; Handles incomming TcpClient requests 	
		/// </summary> 	
		protected override void ListenForIncommingRequests()
		{
			try
			{
				IPAddress localaddr = IPAddress.Parse(localAddress);
				tcpListener = new TcpListener(localaddr, port);
				tcpListener.Start();

				Debug.LogFormat(LOG_FORMAT, "Server is listening, localaddr : <b><color=yellow>" + localaddr + "</color></b>, port : <b><color=yellow>" + port + "</color></b>");
				Byte[] bytes = new Byte[1024];
				while (true)
				{
					using (connectedTcpClient = tcpListener.AcceptTcpClient())
					{
						// Get a stream object for reading 					
						using (NetworkStream stream = connectedTcpClient.GetStream())
						{
							int length;
							// Read incomming stream into byte arrary. 						
							while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
							{
								byte[] incommingData = new byte[length];
								Array.Copy(bytes, 0, incommingData, 0, length);
								// Convert byte array to string data.
								string clientData = Encoding.ASCII.GetString(incommingData);
								Debug.LogFormat(LOG_FORMAT, "Received data FROM <b><color=magenta>Client</color></b> : <b><color=yellow>" + clientData + "</color></b>");
							}
						}
					}
				}
			}
			catch (SocketException socketException)
			{
				Debug.LogErrorFormat(LOG_FORMAT, "SocketException " + socketException.ToString());
			}
		}
		/// <summary> 	
		/// Send data to client using socket connection. 	
		/// </summary> 	
		protected virtual void SendData(string data)
		{
			Debug.LogFormat(LOG_FORMAT, "SendData(), data : <b><color=yellow>" + data + "</color></b>");

			if (connectedTcpClient == null)
			{
				Debug.LogWarningFormat(LOG_FORMAT, "Can not Send Data!!!!! cuz, <b>connectedTcpClient == null</b>");
				return;
			}

			try
			{
				// Get a stream object for writing.
				NetworkStream stream = connectedTcpClient.GetStream();
				if (stream.CanWrite)
				{
					// Convert string data to byte array.                 
					byte[] serverDataAsByteArray = Encoding.ASCII.GetBytes(data);
					// Write byte array to socketConnection stream.               
					stream.Write(serverDataAsByteArray, 0, serverDataAsByteArray.Length);
					Debug.LogFormat(LOG_FORMAT, "<b><color=yellow>Server</color></b> sent his data TO <b><color=magenta>Client</color></b> - should be received by client");
				}
			}
			catch (SocketException socketException)
			{
				Debug.LogErrorFormat(LOG_FORMAT, "Socket exception: " + socketException);
			}
		}
	}
}