using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Zenject;
using Cysharp.Threading.Tasks;

namespace Game.Network
{
    public class ServerController : IInitializable, IDisposable, IRakServer
    {
        private const string START_IP = "192.168.31.149";
        private const ushort START_PORT = 30502;
        
        public IEnumerable<ClientData> Clients => _clients;
        
        private readonly List<ClientData> _clients = new();

        private CancellationTokenSource _loopToken;

        private Thread _loopThread;
        private bool _loopThreadActive;

        public void Initialize()
        {
            RakServer.RegisterInterface(this);
            RakServer.Init();
        }

        public void Dispose()
        {
            RakServer.Destroy();
            _loopToken.Dispose();
        }

        private async void Update()
        {
            while (!_loopToken.IsCancellationRequested)
            {
                RakServer.Update();
                await UniTask.Delay(100);
            }
        }

        public void Start()
        {
            _loopToken = new CancellationTokenSource();
            StartUpdate();
            RakServer.Start(START_IP, START_PORT);
        }

        public void Stop()
        {
            _loopToken.Cancel();
            _loopToken.Dispose();
            RakServer.Stop();
            _loopThreadActive = false;
        }

        private void StartUpdate()
        {
            if (_loopThreadActive)
                return;

            _ = UniTask.RunOnThreadPool(Update, false, _loopToken.Token);
        }

        public void OnConnected(ushort connectionIndex, ulong guid)
        {
            using var bitStream = PooledBitStream.GetBitStream();
            bitStream.Write((byte)GamePacketID.CLIENT_DATA_REQUEST);
            RakServer.SendToClient(bitStream, guid, PacketPriority.IMMEDIATE_PRIORITY, PacketReliability.RELIABLE);
        }

        public void OnDisconnected(ushort connectionIndex, ulong guid, DisconnectReason reason, string message)
        {
            if (_clients[connectionIndex] != null && _clients[connectionIndex].Guid == guid)
            {
                Debug.Log("[Server] Client " + _clients[connectionIndex].NickName + " disconnected! (" + reason + ")");
                _clients.RemoveAt(connectionIndex);
            }
            else
            {
                Debug.Log("[Server] Client " + RakServer.GetAddress(guid,true) + " disconnected! (" + reason + ")");
            }
        }

        public void OnReceived(GamePacketID packet_id, ushort connectionIndex, ulong guid, BitStream bitStream, ulong local_time)
        {
            switch (packet_id)
            {
                case GamePacketID.CLIENT_DATA_REPLY:
                    var playerName = bitStream.ReadString();

                    _clients.Add(new ClientData(guid, playerName));

                    using(var bsOut = PooledBitStream.GetBitStream())
                    {
                        bsOut.Write((byte)GamePacketID.CLIENT_DATA_ACCEPTED);
                        bsOut.Write("edited_"+playerName);
                        RakServer.SendToClient(bsOut, guid, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE);
                    }
                    break;
                case GamePacketID.CLIENT_CHAT_MESSAGE:
                    var text = bitStream.ReadString();
                    using(var bsOut = PooledBitStream.GetBitStream())
                    {
                        bsOut.Write((byte)GamePacketID.SERVER_CHAT_MESSAGE);
                        bsOut.Write(_clients.First(x => x.Guid == guid).NickName);
                        bsOut.Write(text);
                        RakServer.SendToAllIgnore(bsOut, guid, PacketPriority.LOW_PRIORITY, PacketReliability.RELIABLE);
                    }
                    break;
            }
        }
        
        public class ClientData
        {
            public ulong Guid;
            public string NickName;

            public ClientData(ulong guid, string username)
            {
                Guid = guid;
                NickName = username;
            }
        }
    }
}