using Common;
using JetBrains.Annotations;
using UnityEngine;
using System.IO;

namespace Core.Scenario
{
    public class SpawnPlayerAI : ScenarioAction
    {
        [SerializeField, UsedImplicitly] private CustomSpawnSettings customSpawnSettings;

        internal override void Initialize(Map map)
        {
            base.Initialize(map);

            EventHandler.RegisterEvent(World, GameEvents.ServerLaunched, OnServerLaunched);
        }

        internal override void DeInitialize()
        {
            EventHandler.UnregisterEvent(World, GameEvents.ServerLaunched, OnServerLaunched);

            base.DeInitialize();
        }

        private void OnServerLaunched()
        {
            var playerCreateToken = new Player.CreateToken
            {
                Position = customSpawnSettings.SpawnPoint.position,
                Rotation = customSpawnSettings.SpawnPoint.rotation,
                OriginalAIInfoId = customSpawnSettings.UnitInfoAI?.Id ?? 0,
                DeathState = DeathState.Alive,
                FreeForAll = true,
                ModelId = 1,
                ClassType = ClassType.Mage,
                OriginalModelId = 1,
                FactionId = Balance.DefaultFaction.FactionId,
                PlayerName = customSpawnSettings.CustomNameId,
                Scale = customSpawnSettings.CustomScale
            };

            // HEHE
            //Debug.Log("Spawning player: " + customSpawnSettings.CustomNameId + ", pos: " + customSpawnSettings.SpawnPoint.position);
            //int playerAIsToSpawn = 25; // Max
            int playerAIsToSpawn = PlayerAIsToSpawn();
            if (World.UnitManager.AmountOfEntities() < (playerAIsToSpawn+2))
                World.UnitManager.Create<Player>(BoltPrefabs.Player, playerCreateToken);
        }

        private static int PlayerAIsToSpawn()
        {
            //TextAsset txt = (TextAsset)Resources.Load("player_ai_to_spawn", typeof(TextAsset));
            //string content = txt.text;
            StreamReader sr = new StreamReader(System.Environment.GetEnvironmentVariable("USERPROFILE") + "/player_ai_to_spawn.txt");
            string content = sr.ReadLine();
            sr.Close();
            int result = System.Int32.Parse(content);
            if (result < 0)
                return 25;
            return result;
        }
    }
}
