using CrashMissileCrash.Ads;
using CrashMissileCrash.Analytics;
using CrashMissileCrash.Audio;
using CrashMissileCrash.Backend;
using CrashMissileCrash.Base;
using CrashMissileCrash.Battle;
using CrashMissileCrash.Battle.Gates;
using CrashMissileCrash.Battle.SpecialMechanics;
using CrashMissileCrash.Cannons;
using CrashMissileCrash.Cards;
using CrashMissileCrash.Champions;
using CrashMissileCrash.Data;
using CrashMissileCrash.Economy;
using CrashMissileCrash.Events;
using CrashMissileCrash.IAP;
using CrashMissileCrash.Progression;
using CrashMissileCrash.Raids;
using CrashMissileCrash.RemoteConfig;
using CrashMissileCrash.Tutorial;
using CrashMissileCrash.UI;
using CrashMissileCrash.UI.Screens;
using CrashMissileCrash.Utils;
using UnityEngine;

namespace CrashMissileCrash.Core
{
    /// <summary>
    /// Single entry point for the whole game. Attach this to one empty GameObject in an otherwise
    /// empty scene (see docs/SETUP.md) and it builds every manager, the battlefield, the camera
    /// and the entire UI procedurally at runtime - nothing else needs to exist in the scene.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            BuildCoreSystems();
            BuildBattleSystems();
            BuildMetaSystems();
            BuildLiveOpsSystems();
            var ui = BuildUI();
            BuildAudio();
            BuildWorldObjects();
            BuildFlowControllers();

            ui.Show<HomeScreen>(false);
        }

        private void Start()
        {
            if (TutorialManager.Instance != null && !TutorialManager.Instance.IsTutorialComplete)
            {
                TutorialManager.Instance.RunIfNeeded();
            }
        }

        private static T Spawn<T>(string name) where T : Component => new GameObject(name).AddComponent<T>();

        private void BuildCoreSystems()
        {
            Spawn<GameManager>("GameManager");
            Spawn<GameDatabase>("GameDatabase");
            Spawn<VisualRegistry>("VisualRegistry");
            ServiceLocator.Register<IBackendService>(new LocalMockBackendService());
        }

        private void BuildBattleSystems()
        {
            Spawn<BattlefieldBounds>("BattlefieldBounds");
            Spawn<CrowdManager>("CrowdManager");
            Spawn<EnemyManager>("EnemyManager");
            Spawn<EnemyBase>("EnemyBase");
            Spawn<GateManager>("GateManager");
            Spawn<ObstacleManager>("ObstacleManager");
            Spawn<CombatSystem>("CombatSystem");
            Spawn<LevelManager>("LevelManager");
            Spawn<VFXManager>("VFXManager");
            Spawn<BattleManager>("BattleManager");
        }

        private void BuildMetaSystems()
        {
            Spawn<EconomyManager>("EconomyManager");
            Spawn<InventoryManager>("InventoryManager");
            Spawn<CardManager>("CardManager");
            Spawn<CardPackManager>("CardPackManager");
            Spawn<CannonManager>("CannonManager");
            Spawn<ChampionManager>("ChampionManager");
            Spawn<ProgressionManager>("ProgressionManager");
            Spawn<MissionManager>("MissionManager");
            Spawn<LeagueManager>("LeagueManager");
            Spawn<SeasonManager>("SeasonManager");
            Spawn<BaseManager>("BaseManager");
        }

        private void BuildLiveOpsSystems()
        {
            Spawn<RaidManager>("RaidManager");
            Spawn<RevengeManager>("RevengeManager");
            Spawn<EventManager>("EventManager");
            Spawn<AdManager>("AdManager");
            Spawn<IAPManager>("IAPManager");
            Spawn<AnalyticsManager>("AnalyticsManager");
            Spawn<RemoteConfigManager>("RemoteConfigManager");
            Spawn<SaveManager>("SaveManager");
        }

        private UIManager BuildUI()
        {
            var uiManager = Spawn<UIManager>("UIManager");

            uiManager.RegisterScreen(CreateScreen<HomeScreen>(uiManager, "Screen_Home"));
            uiManager.RegisterScreen(CreateScreen<BattleHUD>(uiManager, "Screen_BattleHUD"));
            uiManager.RegisterScreen(CreateScreen<VictoryScreen>(uiManager, "Screen_Victory"));
            uiManager.RegisterScreen(CreateScreen<DefeatScreen>(uiManager, "Screen_Defeat"));
            uiManager.RegisterScreen(CreateScreen<CollectionScreen>(uiManager, "Screen_Collection"));
            uiManager.RegisterScreen(CreateScreen<ShopScreen>(uiManager, "Screen_Shop"));
            uiManager.RegisterScreen(CreateScreen<MissionsScreen>(uiManager, "Screen_Missions"));
            uiManager.RegisterScreen(CreateScreen<SeasonScreen>(uiManager, "Screen_Season"));
            uiManager.RegisterScreen(CreateScreen<EventsScreen>(uiManager, "Screen_Events"));
            uiManager.RegisterScreen(CreateScreen<BaseScreen>(uiManager, "Screen_Base"));
            uiManager.RegisterScreen(CreateScreen<WorldMapScreen>(uiManager, "Screen_WorldMap"));
            uiManager.RegisterScreen(CreateScreen<SettingsScreen>(uiManager, "Screen_Settings"));
            uiManager.RegisterScreen(CreateScreen<TutorialOverlay>(uiManager, "Screen_Tutorial"));

            return uiManager;
        }

        private static T CreateScreen<T>(UIManager uiManager, string name) where T : UIScreen
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(uiManager.RootRect, false);
            UIBuilder.Stretch(rect);
            return go.AddComponent<T>();
        }

        private void BuildAudio()
        {
            Spawn<AudioManager>("AudioManager");
        }

        private void BuildWorldObjects()
        {
            var cameraGo = new GameObject("BattleCamera", typeof(Camera), typeof(AudioListener), typeof(CameraController));
            cameraGo.tag = "MainCamera";
            var camera = cameraGo.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.05f, 0.06f, 0.09f);
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 200f;
            cameraGo.transform.position = new Vector3(0f, 9f, -6f);
            cameraGo.transform.rotation = Quaternion.Euler(55f, 0f, 0f);

            var lightGo = new GameObject("SunLight", typeof(Light));
            var light = lightGo.GetComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            light.color = new Color(1f, 0.97f, 0.9f);
            lightGo.transform.rotation = Quaternion.Euler(55f, 30f, 0f);

            var cannonGo = new GameObject("Cannon", typeof(CannonController));
            var cannonController = cannonGo.GetComponent<CannonController>();
            var cannonBody = PrimitiveFactory.CreatePlaceholder("CannonBody", PlaceholderShape.Cylinder, new Color(0.2f, 0.25f, 0.3f), new Vector3(1.2f, 0.8f, 1.2f));
            cannonBody.transform.SetParent(cannonGo.transform, false);
            cannonGo.transform.position = new Vector3(0f, 0.5f, 0.5f);

            Spawn<CannonFireController>("CannonFireController");
        }

        private void BuildFlowControllers()
        {
            Spawn<GameFlowController>("GameFlowController");
            Spawn<TutorialManager>("TutorialManager");
            GameManager.Instance.ChangeState(GameState.Home);
        }
    }
}
