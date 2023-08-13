using Game.Common;
using Game.Network;
using Game.Player;
using Game.PrefabsActions;
using Game.Utils;
using UnityEngine;
using Zenject;

namespace Game.Installers
{
    public class SceneInstaller : MonoInstaller
    {
        
        public override void InstallBindings()
        {
            Container.Bind<PrefabCreator>()
                .AsSingle()
                .NonLazy();
            
            Container.Bind<PlayerCreator>().AsSingle().NonLazy();

            Container.BindInterfacesAndSelfTo<CannonController>().AsSingle().NonLazy();
            
            Container.BindSignal<NetworkSignals.ClientPacketReceived>().ToMethod(@object =>
            {
                Container.ResolveAll<IClientPacketReader>().ForEach(reader => reader.ReceivePacket(@object.Packet));
            });
        }
    }
}