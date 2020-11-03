﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using Mono.Nat;

public class Server
{
    public static int MaxPlayers { get; private set; }
    public static int Port { get; private set; }
    public static Dictionary<int, Client> clients = new Dictionary<int, Client>();
    public delegate void PacketHandler(int _fromClient, Packet _packet);
    public static Dictionary<int, PacketHandler> packetHandlers;

    private static TcpListener tcpListener;
    private static UdpClient udpListener;
    private static Dictionary<System.Net.IPAddress, INatDevice> globalDevice = new Dictionary<System.Net.IPAddress, INatDevice>();

    /// <summary>Starts the server.</summary>
    /// <param name="_maxPlayers">The maximum players that can be connected simultaneously.</param>
    /// <param name="_port">The port to start the server on.</param>
    public static void Start(int _maxPlayers, int _port)
    {
        MaxPlayers = _maxPlayers;
        Port = _port;

        Debug.Log("Starting server...");
        InitializeServerData();

        tcpListener = new TcpListener(IPAddress.Any, Port);
        tcpListener.Start();
        tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);

        udpListener = new UdpClient(Port);
        udpListener.BeginReceive(UDPReceiveCallback, null);

        NatUtility.DeviceFound += DeviceFound;
        NatUtility.StartDiscovery();

        Debug.Log($"Server started on port {Port}.");
    }

    private static void DeviceFound(object sender, DeviceEventArgs args)
    {
        if (args.Device.GetSpecificMapping(Protocol.Tcp, 27015).PublicPort == -1)
        {
            Debug.Log("UPnP device found trying to automatically port forward! " + args.Device.GetExternalIP());
            INatDevice device = args.Device;
            globalDevice.Add(args.Device.GetExternalIP(), device);
            device.CreatePortMap(new Mapping(Protocol.Tcp, 27015, 27015));
            device.CreatePortMap(new Mapping(Protocol.Udp, 27015, 27015));
        } else
        {
            Debug.Log("Device already port forwarded! " + args.Device.GetExternalIP());
        }
    }

    /// <summary>Handles new TCP connections.</summary>
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

    /// <summary>Receives incoming UDP data.</summary>
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
                    // If this is a new connection
                    clients[_clientId].udp.Connect(_clientEndPoint);
                    return;
                }

                if (clients[_clientId].udp.endPoint.ToString() == _clientEndPoint.ToString())
                {
                    // Ensures that the client is not being impersonated by another by sending a false clientID
                    clients[_clientId].udp.HandleData(_packet);
                }
            }
        }
        catch (Exception _ex)
        {
            Debug.Log($"Error receiving UDP data: {_ex}");
        }
    }

    /// <summary>Sends a packet to the specified endpoint via UDP.</summary>
    /// <param name="_clientEndPoint">The endpoint to send the packet to.</param>
    /// <param name="_packet">The packet to send.</param>
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

    /// <summary>Initializes all necessary server data.</summary>
    private static void InitializeServerData()
    {
        for (int i = 1; i <= MaxPlayers; i++)
        {
            clients.Add(i, new Client(i));
        }

        packetHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived },
                { (int)ClientPackets.playerMovement, ServerHandle.PlayerMovement },
                { (int)ClientPackets.playerShoot, ServerHandle.PlayerShoot },
                { (int)ClientPackets.playerVerticalRotation, ServerHandle.PlayerVerticalRotation },
            };
        Debug.Log("Initialized packets.");
    }

    public static void Stop()
    {
        tcpListener.Stop();
        udpListener.Close();

        foreach (KeyValuePair<System.Net.IPAddress, INatDevice> device in globalDevice)
        {
            if (device.Value.GetSpecificMapping(Protocol.Tcp, 27015).PublicPort != -1)
            {
                device.Value.DeletePortMap(new Mapping(Protocol.Tcp, 27015, 27015));

            }
            if (device.Value.GetSpecificMapping(Protocol.Udp, 27015).PublicPort != -1)
            {
                device.Value.DeletePortMap(new Mapping(Protocol.Udp, 27015, 27015));

            }
        }
    }
}
