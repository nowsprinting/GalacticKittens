using System.Linq;
using System.Threading;
using Autopilot.Scripts.TestDoubles;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Agents;
using UnityEngine;

namespace Autopilot.Scripts
{
    [CreateAssetMenu(fileName = "New GameplayAgent", menuName = "Anjin/GalacticKittens/Gameplay Agent", order = 41)]
    public class GameplayAgent : AbstractAgent
    {
        public override async UniTask Run(CancellationToken token)
        {
            try
            {
                Logger.Log($"Enter {this.name}.Run()");

                PlayerShipMovement movement = null;
                await UniTask.WaitWhile(() =>
                {
                    movement = FindObjectsByType<PlayerShipMovement>(FindObjectsSortMode.None)
                        .FirstOrDefault(x => x.IsOwner);
                    return movement == null;
                }, cancellationToken: token);
                movement.Input = new StubInputMonkey(Random);
                Logger.Log($"Inject StubInputMonkey to {movement.gameObject.name}");

                PlayerShipShootBullet shoot = null;
                await UniTask.WaitWhile(() =>
                {
                    shoot = FindObjectsByType<PlayerShipShootBullet>(FindObjectsSortMode.None)
                        .FirstOrDefault(x => x.IsOwner);
                    return shoot == null;
                }, cancellationToken: token);
                shoot.Input = new StubInputSpaceKey();
                Logger.Log($"Inject StubInputSpaceKey to {shoot.gameObject.name}");

                // not wait
            }
            finally
            {
                Logger.Log($"Exit {this.name}.Run()");
            }
        }
    }
}