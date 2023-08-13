﻿using Game.Network;
using UnityEngine;
using Zenject;

namespace Game.Utils
{
    public class ServerGui : MonoBehaviour
    {
        private ServerController _serverController;

        [Inject]
        private void Constructor(ServerController serverController)
        {
            _serverController = serverController;
        }
        
        private void OnGUI()
        {
            if (RakServer.State is ServerState.NOT_STARTED or ServerState.STOPPED)
            {
                if (GUILayout.Button("Start Server"))
                {
                    _serverController.Start();
                }
            }
            else
            {
                if (GUILayout.Button("Stop Server"))
                {
                    _serverController.Stop();
                }

                GUILayout.Box("Connected clients");

                foreach (var data in _serverController.Clients)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Box(data.NickName);
                    if (GUILayout.Button("Kick"))
                    {
                        RakServer.CloseConnection(data.Guid);
                    }

                    if (GUILayout.Button("Ban"))
                    {
                        RakServer.AddBanIP(RakServer.GetAddress(data.Guid));
                        RakServer.CloseConnection(data.Guid);
                    }

                    GUILayout.EndHorizontal();
                }
            }
        }
    }
}