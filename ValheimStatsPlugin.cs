
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System;
using System.IO;
using static PlayerProfile;

namespace ValheimStatsToDiscord
{
    [BepInPlugin("com.gamemaster.valheimonlinetracker", "Valheim Online Tracker", "1.6.0")]
    public class ValheimOnlineTrackerPlugin : BaseUnityPlugin
    {
        public static ManualLogSource Log;
        private static readonly HttpClient http = new HttpClient();
        public static ValheimOnlineTrackerPlugin _instance;

        private float postTimer = 0f;
        private float trackTimer = 0f;
        private float loginStatsTimer = 0f;

        private ConfigEntry<string> WebhookURL;
        private ConfigEntry<string> StatsWebhookURL;
        private ConfigEntry<int> PostInterval;
        private ConfigEntry<string> StartupMessages;
        private ConfigEntry<string> ShutdownMessages;
        private ConfigEntry<string> OnlineTitle;
        private ConfigEntry<string> NoPlayersMessage;
        private ConfigEntry<string> BotName;

        private ConfigEntry<bool> EnableLoginLogout;
        private ConfigEntry<string> LoginMessages;
        private ConfigEntry<string> LogoutMessages;

        private ConfigEntry<bool> EnableLoginStatsPost;
        private ConfigEntry<int> LoginStatsInterval;
        private ConfigEntry<string> LoginStatsMessages;

        private ConfigEntry<string> LoginMilestoneMessage;
        private ConfigEntry<string> TimeMilestoneMessage;

        private string statsPath;
        private Dictionary<string, PlayerStats> playerStats = new();
        private Dictionary<string, HashSet<string>> milestonesHit = new();
        private HashSet<string> currentPlayers = new();
        private HashSet<string> loginsToday = new();
        private HashSet<string> loginsWeek = new();
        private HashSet<string> loginsAllTime = new();

        public void Awake()
        {
            Log = Logger;
            _instance = this;

            string configFolder = Path.Combine(Paths.ConfigPath, "OnlineTracker");
            Directory.CreateDirectory(configFolder);
            var customConfig = new ConfigFile(Path.Combine(configFolder, "ValheimOnlineTracker.cfg"), true);
            statsPath = Path.Combine(configFolder, "Viking_Stats.json");

            SetupConfig(customConfig);
            LoadStats();

            Harmony.CreateAndPatchAll(typeof(ValheimOnlineTrackerPlugin));

            Task.Run(async () =>
            {
                if (!string.IsNullOrWhiteSpace(WebhookURL.Value))
                    await SendDiscord(PickRandom(StartupMessages.Value), useStatsWebhook: false);
            });
        }

        public async Task SendDiscord(string message, bool useStatsWebhook)
        {
            string url = useStatsWebhook && !string.IsNullOrWhiteSpace(StatsWebhookURL.Value)
                ? StatsWebhookURL.Value
                : WebhookURL.Value;

            if (string.IsNullOrWhiteSpace(url)) return;

            var payload = new
            {
                username = BotName.Value,
                content = message
            };

            string json = JsonConvert.SerializeObject(payload);

            try
            {
                var response = await http.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));
                if (!response.IsSuccessStatusCode)
                {
                    Log.LogWarning("[OnlineTracker] Discord response: " + response.StatusCode + " - " + await response.Content.ReadAsStringAsync());
                }
            }
            catch (Exception ex)
            {
                Log.LogError("[OnlineTracker] Failed to send Discord message: " + ex.Message);
            }
        }
    }

    [HarmonyPatch(typeof(Character), "OnDeath")]
    public class MobKillPatch
    {
        static void Postfix(Character __instance)
        {
            try
            {
                if (__instance == null || !__instance.IsDead()) return;
                if (__instance.IsPlayer()) return;

                string prefabName = Utils.GetPrefabName(__instance.gameObject);
                var bossList = new List<string> { "Eikthyr", "gd_king", "Bonemass", "Dragon", "GoblinKing", "SeekerQueen" };
                if (bossList.Contains(prefabName)) return;

                var hit = (HitData)AccessTools.Field(typeof(Character), "m_lastHit").GetValue(__instance);
                var attacker = hit.GetAttacker();

                if (attacker == null) return;

                var player = attacker.GetComponent<Player>();
                if (player == null || string.IsNullOrWhiteSpace(player.GetPlayerName())) return;

                string victim = __instance.m_name;
                if (string.IsNullOrWhiteSpace(victim))
                    victim = prefabName;

                string killer = player.GetPlayerName();
                string msg = $"⚔️ {killer} just slayed a {victim}!";

                ValheimOnlineTrackerPlugin._instance?.SendDiscord(msg, useStatsWebhook: true);
            }
            catch (Exception ex)
            {
                ValheimOnlineTrackerPlugin.Log.LogError("[MobKillPatch] Error handling kill: " + ex.Message);
            }
        }
    }
}
