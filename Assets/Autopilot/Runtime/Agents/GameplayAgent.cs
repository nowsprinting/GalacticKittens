using System.Linq;
using System.Threading;
using Autopilot.Agents.TestDoubles;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Agents;
using TestHelper.Input;
using UnityEngine;

namespace Autopilot.Agents
{
    /// <summary>
    /// Agent for Gameplay scene.
    /// Using stub input w, s, and space key.
    /// </summary>
    [CreateAssetMenu(fileName = "New GameplayAgent", menuName = "Anjin/GalacticKittens/Gameplay Agent", order = 41)]
    public class GameplayAgent : AbstractAgent
    {
        [Tooltip("Shoot bullet interval frame count")]
        public int shootBulletInterval = 20;

        public override async UniTask Run(CancellationToken token)
        {
            PlayerShipMovement movement = null;
            PlayerShipShootBullet shoot = null;

            try
            {
                Logger.Log($"Enter {this.name}.Run()");

                await UniTask.WaitWhile(() =>
                {
                    movement = FindObjectsByType<PlayerShipMovement>(FindObjectsSortMode.None)
                        .FirstOrDefault(x => x.IsOwner);
                    return movement == null;
                }, cancellationToken: token);
                movement.Input = new StubInputMonkey(Random);
                Logger.Log($"Inject StubInputMonkey to {movement.gameObject.name}");

                await UniTask.WaitWhile(() =>
                {
                    shoot = FindObjectsByType<PlayerShipShootBullet>(FindObjectsSortMode.None)
                        .FirstOrDefault(x => x.IsOwner);
                    return shoot == null;
                }, cancellationToken: token);
                shoot.Input = new StubInputSpaceKey() { Interval = this.shootBulletInterval };
                Logger.Log($"Inject StubInputSpaceKey to {shoot.gameObject.name}");

                await UniTask.WaitWhile(() => true, cancellationToken: token);
            }
            finally
            {
                if (movement != null)
                {
                    movement.Input = new InputWrapper();
                }

                if (shoot != null)
                {
                    shoot.Input = new InputWrapper();
                }

                Logger.Log($"Exit {this.name}.Run()");
            }
        }
    }
}