using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using GameServer;

class Server
{
	public static int MaxPlayers { get; private set; }
	public static int Port { get; private set; }
	public static Dictionary<int, Client> clients = new Dictionary<int, Client>();
	public delegate void PacketHandler(int _fromClient, Packet _packet);
	public static Dictionary<int, PacketHandler> packetHandlers;

	private static TcpListener tcpListener;
	private static UdpClient udpListener;

	public static Troops[] allTroops;
	public static List<FormationTable> formationTable;

	public static MySQLSettings mySQLSettings;

	public static void Start(int _maxPlayers, int _port)
	{
		//ipconfig
		MaxPlayers = _maxPlayers;
		Port = _port;

		InitializeMySQLServer();
#if graphic
		Debug.Log("Starting server...");
#endif
		InitializeServerData();
		tcpListener = new TcpListener(IPAddress.Any, Port);
		tcpListener.Start();
		tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);

		udpListener = new UdpClient(Port);
		udpListener.BeginReceive(UDPReceiveCallback, null);

		Debug.Log($"Server started on port {Port}.");

		//allTroops = Database.InizializeTroops();

		Debug.Log("Troops inizialized");

		//formationTable = Database.InizializeFormationTable();

		Debug.Log("FormationTable inizialized");

	}


	private static void TCPConnectCallback(IAsyncResult _result)
	{
		TcpClient _client = tcpListener.EndAcceptTcpClient(_result);
		tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);
		Debug.Log($"Incoming connection from {_client.Client.RemoteEndPoint}...");

		for (int i = 1; i <= MaxPlayers; i++)
		{
			if (clients[i].tcp.socket == null)
			{
				clients[i].tcp.Connect(_client);
				return;
			}
		}

		Debug.Log($"{_client.Client.RemoteEndPoint} failed to connect: Server full!");
	}

	private static void UDPReceiveCallback(IAsyncResult _result)
	{
		try
		{
			IPEndPoint _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
			byte[] _data = udpListener.EndReceive(_result, ref _clientEndPoint);
			udpListener.BeginReceive(UDPReceiveCallback, null);

			if (_data.Length < 4)
			{
				return;
			}

			using (Packet _packet = new Packet(_data))
			{
				int _clientId = _packet.ReadInt();

				if (_clientId == 0)
				{
					return;
				}

				if (clients[_clientId].udp.endPoint == null)
				{
					clients[_clientId].udp.Connect(_clientEndPoint);
					return;
				}

				if (clients[_clientId].udp.endPoint.ToString() == _clientEndPoint.ToString())
				{
					clients[_clientId].udp.HandleData(_packet);
				}
			}
		}
		catch (Exception _ex)
		{
			Debug.Log($"Error receiving UDP data: {_ex}");
		}
	}

	public static void SendUDPData(IPEndPoint _clientEndPoint, Packet _packet)
	{
		try
		{
			if (_clientEndPoint != null)
			{
				udpListener.BeginSend(_packet.ToArray(), _packet.Length(), _clientEndPoint, null, null);
			}
		}
		catch (Exception _ex)
		{
			Debug.Log($"Error sending data to {_clientEndPoint} via UDP: {_ex}");
		}
	}

	private static void InitializeServerData()
	{
		for (int i = 1; i <= MaxPlayers; i++)
		{
			clients.Add(i, new Client(i));
		}

		packetHandlers = new Dictionary<int, PacketHandler>()
		{
			{ (int)ClientPackets.Login, ServerHandle.Login },
			{ (int)ClientPackets.Register, ServerHandle.Register },
			{ (int)ClientPackets.placebelTroops, ServerHandle.PlacebelTroops},
			{ (int)ClientPackets.PlaceTroop, ServerHandle.PlaceTroop},
			{ (int)ClientPackets.TroopMove, ServerHandle.TroopMove},
			{ (int)ClientPackets.SearchforMatch, ServerHandle.ClientSearchForMatch},
			{ (int)ClientPackets.CommanderChild, ServerHandle.SetCommanderChild},
			{ (int)ClientPackets.setPositionFromTroop, ServerHandle.setPositionFromTroop },
			{ (int)ClientPackets.setAttack, ServerHandle.setAttack },
			{ (int)ClientPackets.PlaceArmy, ServerHandle.PlaceArmy },
			{ (int)ClientPackets.Exception, ServerHandle.ExceptionFromClient },
			{ (int)ClientPackets.moveToNewGrid, ServerHandle.MoveToNewGrid}
		};
		Debug.Log("Initialized packets.");
	}

	private static void InitializeMySQLServer()
	{
		mySQLSettings.user = "root";
		mySQLSettings.password = "";
		mySQLSettings.server = "127.0.0.1";
		mySQLSettings.database = "unitydatabase";

		mySQLSettings = MySQL.ConnectToMySQL(mySQLSettings);
		Debug.Log(mySQLSettings.server);
	}

	public static void Stop()
	{
		tcpListener.Stop();
		udpListener.Close();
	}
}
