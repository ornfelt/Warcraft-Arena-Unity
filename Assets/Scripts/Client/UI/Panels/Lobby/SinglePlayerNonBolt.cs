using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Localization;
using Client.UI;
using Common;
using Core;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using UdpKit;

namespace Client
{
    internal class SinglePlayerNonBolt
    {
        private enum State
        {
            Inactive,
            Active,
            Starting,
            Connecting
        }

        private const float MaxConnectionAttemptTime = 50.0f;

        private readonly ConnectionAttemptInfo connectionAttemptInfo = new ConnectionAttemptInfo();
        private NetworkingMode networkingMode;
        private World world;
        private State state;

        public Map<Guid, UdpSession> Sessions => BoltNetwork.SessionList;
        public string Version => "1.0.92";

        [SerializeField, UsedImplicitly] private BalanceReference balance;

        //private new WorldServer World { get; set; }
        //private ServerLaunchState LaunchState { get; set; }
        private ServerRoomToken ServerToken { get; set; }

        public void StartSinglePlayerNonBolt(ServerRoomToken serverToken, Action onStartSuccess, Action onStartFail)
        {
            //networkingMode = NetworkingMode.Both;
            //serverToken.Version = Version;
            //StartCoroutine(StartServerRoutineNonBolt(serverToken, true, onStartSuccess, onStartFail));
            StartServerRoutineNonBolt(serverToken, true, onStartSuccess, onStartFail);
        }

        private void StartServerRoutineNonBolt(ServerRoomToken serverToken, bool singlePlayer, Action onStartSuccess, Action onStartFail)
        {
            //if (BoltNetwork.IsRunning && !BoltNetwork.IsServer)
            //{
            //    BoltLauncher.Shutdown();
            //    yield return new WaitUntil(NetworkIsInactive);
            //}

            state = State.Starting;

            //if (singlePlayer)
            //    BoltLauncher.StartSinglePlayer(config);
            //else
            //    BoltLauncher.StartServer(config);

            //yield return new WaitUntil(NetworkIsIdle);

            //for (int i = 0; i < 3; i++)
            //    yield return new WaitForEndOfFrame();

            //if (BoltNetwork.IsServer)
            //{
            //    onStartSuccess?.Invoke();

            //    if (!singlePlayer)
            //        BoltMatchmaking.CreateSession(Guid.NewGuid().ToString(), serverToken);

            //    BoltNetwork.LoadScene(serverToken.Map, serverToken);
            //}
            //else
            //{
            //    onStartFail?.Invoke();
            //}

            //SceneManager.LoadScene(serverToken.Map, serverToken);
            SceneManager.LoadScene(serverToken.Map);
            onStartSuccess?.Invoke();
        }
    }
}
