using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using BrutalCompany.Component;
using System.Collections.Generic;
using System.Security.Policy;
using UnityEngine;
using static UnityEngine.UIElements.UIR.Implementation.UIRStylePainter;
using Project1;
using System.Linq;
using Project1.Data;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

namespace BrutalCompany
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static bool loaded;
        Harmony _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        public static List<SelectableLevel> levelsModified = new List<SelectableLevel>();
        public static Dictionary<SelectableLevel, float> levelHeatVal;
        public static Dictionary<SelectableLevel, List<SpawnableEnemyWithRarity>> levelEnemySpawns;
        public static Dictionary<SpawnableEnemyWithRarity, int> enemyRaritys;
        public static Dictionary<SpawnableEnemyWithRarity, AnimationCurve> enemyPropCurves;
        public static ManualLogSource mls;
        private void Awake()
        {
            mls = BepInEx.Logging.Logger.CreateLogSource("BrutalCompany");
            // Plugin startup logic
            mls.LogInfo("Loaded Brutal Company and applying patches.");
            _harmony.PatchAll(typeof(Plugin));
            mls = Logger;
            levelHeatVal = new Dictionary<SelectableLevel, float>();
            enemyRaritys = new Dictionary<SpawnableEnemyWithRarity, int>();
            levelEnemySpawns = new Dictionary<SelectableLevel, List<SpawnableEnemyWithRarity>>();
            enemyPropCurves = new Dictionary<SpawnableEnemyWithRarity, AnimationCurve>();


        }

        public void OnDestroy()
        {
            //mls.LogMessage("ugh");
            if (!loaded)
            {
                GameObject gameObject = new GameObject("QuotaChanger");
                DontDestroyOnLoad(gameObject);
                gameObject.AddComponent<QuotaAjuster>();
                //mls.LogMessage("Created the gameobject!");
                loaded = true;
                //LC_API.ServerAPI.ModdedServer.SetServerModdedOnly();
            }
        }


        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.LoadNewLevel))]
        [HarmonyPrefix]
        static bool ModifyLevel(ref SelectableLevel newLevel)
        {
            
            if (!levelHeatVal.ContainsKey(newLevel))
            {
                levelHeatVal.Add(newLevel, 0);
            }
            if (!levelEnemySpawns.ContainsKey(newLevel))
            {
                List<SpawnableEnemyWithRarity> spawns = new List<SpawnableEnemyWithRarity>();
                foreach (var item in newLevel.Enemies)
                {
                    spawns.Add(item);
                }
                levelEnemySpawns.Add(newLevel, spawns);
            }
            List<SpawnableEnemyWithRarity> spawnableEnemies;
            levelEnemySpawns.TryGetValue(newLevel, out spawnableEnemies);
            newLevel.Enemies = spawnableEnemies;
            foreach (var level in levelHeatVal.Keys.ToList())
            {
                float hl;
                levelHeatVal.TryGetValue(level, out hl);
                levelHeatVal[level] = Mathf.Clamp(hl - 5f, 0, 100f);
            }
            float hl2;
            levelHeatVal.TryGetValue(newLevel, out hl2);
            foreach (var enemy in newLevel.Enemies)
            {
                if (!enemyRaritys.ContainsKey(enemy))
                {
                    enemyRaritys.Add(enemy, enemy.rarity);
                }
                int rare = 0;
                enemyRaritys.TryGetValue(enemy, out rare);
                enemy.rarity = rare;
            }
            foreach (var enemy in newLevel.Enemies)
            {
                if (!enemyPropCurves.ContainsKey(enemy))
                {
                    enemyPropCurves.Add(enemy, enemy.enemyType.probabilityCurve);
                }
                AnimationCurve prob = new AnimationCurve();
                enemyPropCurves.TryGetValue(enemy, out prob);
                enemy.enemyType.probabilityCurve = prob;
            }
            HUDManager.Instance.AddTextToChatOnServer("\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n");
            HUDManager.Instance.AddTextToChatOnServer("<color=orange>MOON IS AT " + hl2.ToString() + "% HEAT</color>");
            if (hl2 > 49f)
            {
                HUDManager.Instance.AddTextToChatOnServer("<color=red>HEAT LEVEL IS DANGEROUSLY HIGH. <color=white>\nVISIT OTHER MOONS TO LOWER HEAT LEVEL.</color>");
            }
            int chanceForTurretHell = Random.Range(-5, 3);
            int chanceforLandmineHell = Random.Range(-5, 3);
            EventEnum eventRand = (EventEnum)Mathf.Clamp(Random.Range(-1, 13), 0, 99);
            if (newLevel.sceneName == "CompanyBuilding")
            {
                eventRand = 0;
            }
            switch (eventRand)
            {
                default:
                    HUDManager.Instance.AddTextToChatOnServer("<color=green>Level event: NONE</color>");
                    break;

                case EventEnum.None:
                    HUDManager.Instance.AddTextToChatOnServer("<color=green>Level event: NONE</color>");
                    break;

                case EventEnum.Turret:
                    HUDManager.Instance.AddTextToChatOnServer("<color=red>Level event: TURRET HELL</color>");
                    break;

                case EventEnum.Landmine:
                    HUDManager.Instance.AddTextToChatOnServer("<color=red>Level event: LANDMINE HELL</color>");
                    break;

                case EventEnum.Hoarding:
                    HUDManager.Instance.AddTextToChatOnServer("<color=red>Level event: HOARDER TOWN</color>");
                    foreach (var item in newLevel.Enemies)
                    {
                        item.rarity = 0;
                        if (item.enemyType.enemyPrefab.GetComponent<HoarderBugAI>() != null)
                        {
                            item.rarity = 999;
                        }
                    }
                    break;

                case EventEnum.Lasso:
                    HUDManager.Instance.AddTextToChatOnServer("<color=red>Level event: LASSO MAN IS REAL</color>");
                    bool gotLasso = false;
                    bool addedLasso = false;
                    foreach (var item in newLevel.Enemies)
                    {
                        if (item.enemyType.enemyPrefab.GetComponent<LassoManAI>() != null)
                        {
                            gotLasso = true;
                        }
                    }
                    if (!gotLasso)
                    {
                        foreach (var level in StartOfRound.Instance.levels)
                        {
                            foreach (var enemy in level.Enemies)
                            {
                                if (enemy.enemyType.enemyPrefab.GetComponent<LassoManAI>() != null)
                                {
                                    if (!addedLasso)
                                    {
                                        addedLasso = true;
                                        newLevel.Enemies.Add(enemy);
                                    }
                                }
                            }
                        }

                    }
                    foreach (var item in newLevel.Enemies)
                    {
                        if (item.enemyType.enemyPrefab.GetComponent<LassoManAI>() != null)
                        {
                            item.rarity = 999;
                            item.enemyType.probabilityCurve = new AnimationCurve(new Keyframe(0, 10000));
                        }
                    }
                    break;

                case EventEnum.Unfair:
                    HUDManager.Instance.AddTextToChatOnServer("<color=red>Level event: UNFAIR COMPANY</color>");
                    break;

                case EventEnum.OopsAllSnareFlea:
                    HUDManager.Instance.AddTextToChatOnServer("<color=red>Level event: OOPS, ALL SNARE FLEAS!</color>");
                    foreach (var item in newLevel.Enemies)
                    {
                        item.rarity = 0;
                        if (item.enemyType.enemyPrefab.GetComponent<CentipedeAI>() != null)
                        {
                            item.rarity = 999;
                        }
                    }
                    break;

                case EventEnum.BrackenAndCoil:
                    HUDManager.Instance.AddTextToChatOnServer("<color=red>Level event: THE WORST COMBO OF ALL TIME</color>");
                    foreach (var item in newLevel.Enemies)
                    {
                        item.rarity = 0;
                        if (item.enemyType.enemyPrefab.GetComponent<FlowermanAI>() != null)
                        {
                            item.rarity = 999;
                        }
                        if (item.enemyType.enemyPrefab.GetComponent<SpringManAI>() != null)
                        {
                            item.rarity = 999;
                        }
                    }
                    break;

                case EventEnum.Chaos:
                    HUDManager.Instance.AddTextToChatOnServer("<color=red>Level event: CHAOS COMPANY</color>");
                    foreach (var item in newLevel.Enemies)
                    {
                        item.enemyType.probabilityCurve = new AnimationCurve(new Keyframe(0, 1000));
                    }
                    break;

                case EventEnum.All:
                    HUDManager.Instance.AddTextToChatOnServer("<color=red>Level event: ALL</color>");
                    foreach (var item in newLevel.Enemies)
                    {
                        item.enemyType.probabilityCurve = new AnimationCurve(new Keyframe(0, 1000));
                    }
                    foreach (var item in newLevel.Enemies)
                    {
                        item.rarity = 0;
                        if (item.enemyType.enemyPrefab.GetComponent<FlowermanAI>() != null)
                        {
                            item.rarity = 999;
                        }
                        if (item.enemyType.enemyPrefab.GetComponent<SpringManAI>() != null)
                        {
                            item.rarity = 999;
                        }
                    }
                    foreach (var item in newLevel.Enemies)
                    {
                        item.rarity = 0;
                        if (item.enemyType.enemyPrefab.GetComponent<CentipedeAI>() != null)
                        {
                            item.rarity = 999;
                        }
                    }
                    foreach (var item in newLevel.Enemies)
                    {
                        if (item.enemyType.enemyPrefab.GetComponent<LassoManAI>() != null)
                        {
                            item.rarity = 999;
                        }
                    }
                    foreach (var item in newLevel.Enemies)
                    {
                        item.rarity = 0;
                        if (item.enemyType.enemyPrefab.GetComponent<HoarderBugAI>() != null)
                        {
                            item.rarity = 999;
                        }
                    }
                    int randItemCount2 = Random.Range(2, 9);
                    for (int i = 0; i < randItemCount2; i++)
                    {
                        int randomItemID = Random.Range(0, 3);
                        Terminal terminal = UnityEngine.Object.FindObjectOfType<Terminal>();
                        terminal.orderedItemsFromTerminal.Add(randomItemID);
                    }
                    Terminal terminal3 = UnityEngine.Object.FindObjectOfType<Terminal>();
                    int itemCount2 = terminal3.orderedItemsFromTerminal.Count;
                    terminal3.orderedItemsFromTerminal.Clear();
                    for (int i = 0; i < itemCount2; i++)
                    {
                        terminal3.orderedItemsFromTerminal.Add(0);
                    }
                    break;

                case EventEnum.Delivery:
                    HUDManager.Instance.AddTextToChatOnServer("<color=green>Level event: DELIVERY!</color>");
                    int randItemCount = Random.Range(2, 9);
                    for (int i = 0; i < randItemCount; i++)
                    {
                        int randomItemID = Random.Range(0, 6);
                        Terminal terminal = UnityEngine.Object.FindObjectOfType<Terminal>();
                        terminal.orderedItemsFromTerminal.Add(randomItemID);
                    }
                    break;

                case EventEnum.ReplaceItems:
                    HUDManager.Instance.AddTextToChatOnServer("<color=red>Level event: YOU BOUGHT WALKIES, RIGHT?</color>");
                    Terminal terminal2 = UnityEngine.Object.FindObjectOfType<Terminal>();
                    int itemCount = terminal2.orderedItemsFromTerminal.Count;
                    if (itemCount == 0)
                    {
                        itemCount = 1;
                    }
                    
                    terminal2.orderedItemsFromTerminal.Clear();
                    for (int i = 0; i < itemCount; i++)
                    {
                        terminal2.orderedItemsFromTerminal.Add(0);
                    }
                    break;
            }
            SelectableLevel n = newLevel;
            mls.LogWarning("Map Objects");
            foreach (var item in n.spawnableMapObjects)
            {
                
                if (item.prefabToSpawn.GetComponentInChildren<Turret>() != null)
                {
                    //mls.LogWarning("Got turret prefab in MapObjects. Changing number to spawn curve!");
                    if (eventRand == EventEnum.Turret | eventRand == EventEnum.All)
                    {
                        item.numberToSpawn = new UnityEngine.AnimationCurve(new UnityEngine.Keyframe(0f, 200f), new UnityEngine.Keyframe(1f, 25));
                        //mls.LogWarning("TURRET HELL ACTIVATED :3");

                    }
                    else
                    {
                        item.numberToSpawn = new UnityEngine.AnimationCurve(new UnityEngine.Keyframe(0f, 0f), new UnityEngine.Keyframe(1f, 10));
                        //mls.LogWarning("Got turret prefab in MapObjects. Changing number to spawn curve!");
                    }
                }
                else if (item.prefabToSpawn.GetComponentInChildren<Landmine>() != null)
                {
                    //mls.LogWarning("Got landmine prefab in MapObjects. Changing number to spawn curve!");
                    if (eventRand == EventEnum.Landmine | eventRand == EventEnum.All)
                    {
                        item.numberToSpawn = new UnityEngine.AnimationCurve(new UnityEngine.Keyframe(0f, 300f), new UnityEngine.Keyframe(1f, 170f));
                        //mls.LogWarning("LANDMINE HELL ACTIVATED :3");

                    }
                    else
                    {
                        item.numberToSpawn = new UnityEngine.AnimationCurve(new UnityEngine.Keyframe(0f, 0f), new UnityEngine.Keyframe(1f, 70));
                        //mls.LogWarning("Got landmine prefab in MapObjects. Changing number to spawn curve!");
                    }
                }
                mls.LogInfo(item.prefabToSpawn.ToString());
            }
            mls.LogWarning("Enemies");
            foreach (var item in n.Enemies)
            {
                
                mls.LogInfo(item.enemyType.enemyName + "--rarity = " + item.rarity.ToString());
            }
            mls.LogWarning("Daytime Enemies");
            foreach (var item in n.DaytimeEnemies)
            {
                
                mls.LogInfo(item.enemyType.enemyName);
            }
            if (levelsModified.Contains(newLevel))
            {
                //mls.LogInfo("We already hellified this level, skipping hellifier!!!");
                //HUDManager.Instance.AddTextToChatOnServer("<color=red>Brutal Company: Skipping level data change as we already did this one!</color>");
            }
            else
            {
                levelsModified.Add(newLevel);
                n.minScrap += 0;
                n.maxScrap += 45;
                n.minTotalScrapValue += 0;
                n.maxTotalScrapValue += 800;
                
                n.daytimeEnemySpawnChanceThroughDay = new UnityEngine.AnimationCurve(new UnityEngine.Keyframe(0, 7f), new Keyframe(0.5f, 7));
                
                n.maxEnemyPowerCount += 2000;
                n.maxOutsideEnemyPowerCount += 20;
                n.maxDaytimeEnemyPowerCount += 200;
                newLevel = n;
               // mls.LogInfo("New level values: Factory size - " + n.factorySizeMultiplier + " MinScrap - " + n.minScrap + " MaxScrap - " + n.maxScrap + " MinTotalScrapValue - " + n.minTotalScrapValue + " MaxTotalScrapValue - " + n.maxTotalScrapValue + " SpawnChance - " + n.enemySpawnChanceThroughoutDay);
                //HUDManager.Instance.AddTextToChatOnServer("<color=red>Brutal Company: Done generating this levels data.</color>");
            }
            n.enemySpawnChanceThroughoutDay = new UnityEngine.AnimationCurve(new UnityEngine.Keyframe(0, 0.1f + hl2), new Keyframe(0.5f, 500 + hl2));
            n.outsideEnemySpawnChanceThroughDay = new UnityEngine.AnimationCurve(new UnityEngine.Keyframe(0, -30f + hl2), new Keyframe(20f, -30 + hl2), new Keyframe(21f, 10 + hl2));
            if (eventRand == EventEnum.Unfair | eventRand == EventEnum.All)
            {
                n.outsideEnemySpawnChanceThroughDay = new UnityEngine.AnimationCurve(new UnityEngine.Keyframe(0, 10 + hl2), new Keyframe(20f, 10 + hl2), new Keyframe(21f, 10 + hl2));
            }
            if (eventRand == EventEnum.Hoarding | eventRand == EventEnum.All)
            {
                n.enemySpawnChanceThroughoutDay = new UnityEngine.AnimationCurve(new UnityEngine.Keyframe(0, 500f + hl2));
            }
            if (eventRand == EventEnum.Chaos | eventRand == EventEnum.All)
            {
                n.enemySpawnChanceThroughoutDay = new UnityEngine.AnimationCurve(new UnityEngine.Keyframe(0, 500f + hl2));
            }
            levelHeatVal.TryGetValue(newLevel, out hl2);
            levelHeatVal[newLevel] = Mathf.Clamp(hl2 + 30f, 0, 100f);
            //HUDManager.Instance.AddTextToChatOnServer("<color=red>Brutal Company: Done with level generation!</color>");
            return true;
        }
    }
}