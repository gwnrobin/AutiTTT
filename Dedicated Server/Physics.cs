using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace GameServer
{
    class Physics
    {
        //public static Dictionary<int, Client> objects = new Dictionary<int, Client>();
        //Dictonary for items
        public static void Update()
        {
            foreach (Client _client in Server.clients.Values)
            {
                if (_client.player != null)
                {
                    _client.player.Update();
                    Vector3 _up = Vector3.Normalize(new Vector3(0, 1, 0));
                    _client.player.position +=  _up * _client.player.vMomentum / 100;
                    if (_client.player.position.Y > 0)
                    {
                        _client.player.vMomentum -= _client.player.vMomentum * 1/4 + 2;
                    } else if (_client.player.position.Y < 0)
                    {
                        _client.player.position.Y = 0;
                        _client.player.vMomentum = 0;
                    }
                    ServerSend.PlayerPosition(_client.player);


                }
            }
        }
    }
}
