using Game.Controllers.Gameplay;
using Game.Network;
using UnityEngine;
using Zenject;
using Game.Views.Player;

namespace Game.Installers
{
    public class ProjectInstaller : MonoInstaller
    {
        [SerializeField] private PlayerView _player;
        
        public override void InstallBindings()
        {
            SignalsInstaller.Install(Container);
            
            Container.Bind<PlayerView>().FromInstance(_player).AsSingle();

            Container.BindInterfacesAndSelfTo<InteractiveController>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PlayerMoveController>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<MouseLookController>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<ServerController>().AsSingle().MoveIntoAllSubContainers().NonLazy();
        }
    }
}