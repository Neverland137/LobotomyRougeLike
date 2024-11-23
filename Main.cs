using Assets.Scripts.UI.Utils;
using DG.Tweening;
using Harmony;
using NewGameMode.Diffculty;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NewGameMode
{
    public class Harmony_Patch
    {
        public const string VERSION = "1.0.0";
        public static string path = Path.GetDirectoryName(Uri.UnescapeDataString(new UriBuilder(Assembly.GetExecutingAssembly().CodeBase).Path));
        public static string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/LOG";

        public static GameMode rougeLike = (GameMode)666666;

        public static GameObject newRougeButton = new GameObject();
        public static GameObject continueRougeButton = new GameObject();
        public static YKMTLog YKMTLogInstance;
        public static Action<string> LogInfo = (message) => YKMTLogInstance.Info(message);
        public static Action<string> LogError = (message) => YKMTLogInstance.Error(message);
        public static Action<Exception> LogErrorEx = (message) => YKMTLogInstance.Error(message);
        public static Action<string> LogWarning = (message) => YKMTLogInstance.Warn(message);
        public static Action<string> LogDebug = (message) => YKMTLogInstance.Debug(message);
        public Harmony_Patch()
        {
            try
            {
                YKMTLogInstance = new YKMTLog(path + "/Logs", true);
                LogInfo("NewGameMode by YKMT TEAM. Version " + VERSION);
                LogInfo("ModPath: " + path);
                HarmonyInstance harmony = HarmonyInstance.Create("ykmt.NewGameMode");
                // File.WriteAllText(path + "/Log.txt", "");
                //复制dll文件
                harmony.Patch(typeof(GlobalGameManager).GetMethod("Awake", AccessTools.all), null, new HarmonyMethod(typeof(Harmony_Patch).GetMethod("CopyDll")), null);
                //存储数据
                harmony.Patch(typeof(GlobalGameManager).GetMethod("SaveGlobalData", AccessTools.all), new HarmonyMethod(typeof(Harmony_Patch).GetMethod("SaveGlobalData")), null, null);
                harmony.Patch(typeof(GlobalGameManager).GetMethod("SaveData", AccessTools.all), new HarmonyMethod(typeof(Harmony_Patch).GetMethod("SaveDayData")), null, null);
                harmony.Patch(typeof(GlobalGameManager).GetMethod("SaveData", AccessTools.all), new HarmonyMethod(typeof(Harmony_Patch).GetMethod("SaveRougeLikeDayData")), null, null);
                //结束这一天时存档
                harmony.Patch(typeof(GameManager).GetMethod("ClearStage", AccessTools.all), null, new HarmonyMethod(typeof(Harmony_Patch).GetMethod("OnClearStage")), null);

                //各种UI：主页UI，当天结算界面UI，
                harmony.Patch(typeof(AlterTitleController).GetMethod("Start", AccessTools.all), null, new HarmonyMethod(typeof(Harmony_Patch).GetMethod("NewGameModeButton_Start")), null);
                harmony.Patch(typeof(ResultScreen).GetMethod("OnSuccessManagement", AccessTools.all), null, new HarmonyMethod(typeof(Harmony_Patch).GetMethod("ResultScreen_Board")), null);

                //局内机制修改:禁止重开和回库，禁止重开已在模因处完成修改

                //harmony.Patch(typeof(GameManager).GetMethod("ReturnToCheckPoint", AccessTools.all), new HarmonyMethod(typeof(Harmony_Patch).GetMethod("NoRestartAndCheckPoint")), null, null);
                //harmony.Patch(typeof(EscapeUI).GetMethod("Start", AccessTools.all), null, new HarmonyMethod(typeof(Harmony_Patch).GetMethod("NoCheckPointButton_EscapeUI")), null);
                //harmony.Patch(typeof(ResultScreen).GetMethod("Awake", AccessTools.all), null, new HarmonyMethod(typeof(Harmony_Patch).GetMethod("NoRestartButton_ResultScreen")), null);
                //损失全部员工时删档并回到标题页
                harmony.Patch(typeof(Sefira).GetMethod("OnAgentCannotControll", AccessTools.all), null, new HarmonyMethod(typeof(Harmony_Patch).GetMethod("ReturnToTitleOnGameOver")), null);
                //屏蔽剧情
                harmony.Patch(typeof(StoryUI).GetMethod("LoadStory", AccessTools.all), null, new HarmonyMethod(typeof(Harmony_Patch).GetMethod("NoStory")), null);
                //第40天（即肉鸽模式的第五天）开启挑战

                //harmony.Patch(typeof(AlterTitleController).GetMethod("Awake", AccessTools.all), null, new HarmonyMethod(typeof(Harmony_Patch).GetMethod("NewGameModeButton_Awake")), null);
                //harmony.Patch(typeof(AlterTitleController).GetMethod("OnClickButton", AccessTools.all), null, new HarmonyMethod(typeof(Harmony_Patch).GetMethod("NewGameModeButton_OnClick")), null);
                //图鉴相关
                //harmony.Patch(typeof(CreatureObserveInfoModel).GetMethod("GetObserveCost", AccessTools.all), null, new HarmonyMethod(typeof(Harmony_Patch).GetMethod("ObserveCostBoost")), null);
                harmony.Patch(typeof(CreatureModel).GetMethod("AddObservationLevel", AccessTools.all), null, new HarmonyMethod(typeof(Harmony_Patch).GetMethod("ObserveGetLOB")), null);
                harmony.Patch(typeof(AgentModel).GetMethod("OnStageStart", AccessTools.all), null, new HarmonyMethod(typeof(Harmony_Patch).GetMethod("ObserveGetBouns")), null);
                harmony.Patch(typeof(CreatureModel).GetMethod("GetCubeSpeed"), null, new HarmonyMethod(typeof(Harmony_Patch).GetMethod("WorkSpeedBoost")));
                harmony.Patch(typeof(CreatureEquipmentMakeInfo).GetMethod("GetCostAfterUpgrade", AccessTools.all), null, new HarmonyMethod(typeof(Harmony_Patch).GetMethod("EquipmentCostDecrease")), null);

                new Challenge_Patch(harmony);
                new EnergyAndOverload_Patch(harmony);
                new EnergyAndOverload_Patch.RGRandomEventManager(harmony);
                new Meme_Patch(harmony);

                // 初始化商店
                ShopManager.InitShopMeme();

                // 初始化难度系统
                DifficultyManager.Init();
                // HarmonyPatch
                new DifficultyPatch(harmony);

                // Technology
                ZoharModel.LoadZoharData();
            }
            catch (Exception ex)
            {
                YKMTLogInstance.Error("Error While Patching. Exception Message: " + ex.Message + "\nStack Trace: " + ex.StackTrace);
            }
        }

        public static void WorkSpeedBoost(ref float __result)
        {
            __result *= 1.5f;
        }

        public static void CopyDll()
        {
            string[] dllFiles = Directory.GetFiles(path, "*.dll");

            foreach (string file in dllFiles)
            {
                string fileName = Path.GetFileName(file);
                string destFilePath = Path.Combine(Application.dataPath + "/Managed/", fileName);
                if (fileName == "BaseMeme.dll" || fileName == "NewGameMode.dll")
                {
                    continue;
                }


                // 如果目标文件夹中没有同名文件，则复制后关闭游戏
                if (!File.Exists(destFilePath))
                {
                    try
                    {
                        File.Copy(file, destFilePath, true); // true 表示如果文件存在则覆盖
                        //SceneManager.LoadScene("ForceExitScene");这是别碰我那个强制退出的页面，吓人，别用
                        Application.Quit();
                    }
                    catch (Exception ex)
                    {
                        YKMTLogInstance.Error("Could not copy " + fileName + ". Reason: " + ex.ToString());
                    }
                }
                else
                {
                    try
                    {
                        File.Copy(file, destFilePath, true); // true 表示如果文件存在则覆盖
                    }
                    catch (Exception ex)
                    {
                        YKMTLogInstance.Error("Could not copy " + fileName + ". Reason: " + ex.ToString());
                    }
                }
            }


        }
        public static void NewGameModeButton_Start(AlterTitleController __instance)//记得改按钮文字
        {
            try
            {
                AssetBundle bundle = AssetBundle.LoadFromFile(path + "/AssetsBundle/gamemodebutton");
                newRougeButton = UnityEngine.Object.Instantiate(bundle.LoadAsset<GameObject>("GameModeButton"));
                bundle.Unload(false);
                //关闭AB包，但是保留已加载的资源
                //感觉不如手搓按钮 by Plana
                //你不准感觉               

                newRougeButton.transform.SetParent(__instance._buttonRoot.transform.Find("ButtonLayout"));
                newRougeButton.transform.localScale = new Vector3(95, 95, 10);
                newRougeButton.transform.localPosition = new Vector3(-435, 330, 0);
                newRougeButton.AddComponent<ButtonInteraction>();

                UnityEngine.UI.Image image = newRougeButton.transform.GetChild(0).GetComponent<UnityEngine.UI.Image>();
                ///////////
                newRougeButton.transform.GetChild(1).gameObject.AddComponent<FontLoadScript>();
                LocalizeTextLoadScriptWithOutFontLoadScript script = newRougeButton.transform.GetChild(1).gameObject.AddComponent<LocalizeTextLoadScriptWithOutFontLoadScript>();
                script.id = "Rouge_Title_Start_Button";

                //ContentSizeFitter fitter1 = continueRougeButton.transform.GetChild(0).gameObject.AddComponent<ContentSizeFitter>();
                //fitter1.verticalFit = ContentSizeFitter.FitMode.Unconstrained;//首选大小
                //fitter1.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;//不调整
                ContentSizeFitter fitter2 = newRougeButton.transform.GetChild(1).gameObject.AddComponent<ContentSizeFitter>();
                fitter2.verticalFit = ContentSizeFitter.FitMode.PreferredSize;//首选大小
                fitter2.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;//不调整
                ContentSizeFitter fitter3 = newRougeButton.transform.GetChild(2).gameObject.AddComponent<ContentSizeFitter>();
                //fitter3.verticalFit = ContentSizeFitter.FitMode.Unconstrained;//首选大小
                //fitter3.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;//不调整

                UnityEngine.UI.Text text = newRougeButton.transform.GetChild(1).GetComponent<UnityEngine.UI.Text>();
                text.alignment = TextAnchor.MiddleCenter;
                text.horizontalOverflow = HorizontalWrapMode.Wrap;
                text.verticalOverflow = VerticalWrapMode.Truncate;
                ///////////
                UnityEngine.UI.Button button = newRougeButton.transform.GetChild(2).GetComponent<UnityEngine.UI.Button>();

                Texture2D texture2 = new Texture2D(1, 1);
                texture2.LoadImage(File.ReadAllBytes(path + "/Sprite/StartButton.png"));
                image.sprite = Sprite.Create(texture2, new Rect(0f, 0f, texture2.width, texture2.height), new Vector2(0.5f, 0.5f));
                image.transform.localScale *= 10f;
                image.color = new UnityEngine.Color(1, 1, 1, 0);

                //text.text = LocalizeTextDataModel.instance.GetText("Rouge_Title_Continue_Button");
                text.transform.localScale = new Vector3(0.02f, 0.02f, 1);
                //image.color = new Color(1f, 1f, 1f, 0f);
                GameObject.DestroyObject(newRougeButton.transform.GetChild(2).GetComponent<UnityEngine.UI.Image>());
                GameObject.DestroyObject(newRougeButton.transform.GetChild(2).GetChild(0));
                //.enabled = false;
                button.targetGraphic = image;
                //有办法把按钮的图像设成透明吗
                //我这个按钮好像没法加载image 是白不拉几的一片
                //好好好
                button.transform.localScale *= 10f;
                newRougeButton.transform.GetChild(0).transform.localPosition = image.transform.localPosition + new Vector3(0, 0, 10);
                newRougeButton.transform.GetChild(0).transform.localScale = image.transform.localScale;
                button.onClick.AddListener(delegate
                {
                    LogInfo("Click GameStartButton.");
                    CallNewGame_Rougelike();
                });


                //以下为继续按钮的写法：新建一个全新按钮。
                //在没有存档的情况下，该按钮不能点击。

                AssetBundle bundle0 = AssetBundle.LoadFromFile(path + "/AssetsBundle/gamemodebutton");
                continueRougeButton = UnityEngine.Object.Instantiate(bundle0.LoadAsset<GameObject>("GameModeButton"));
                bundle0.Unload(false);

                continueRougeButton.transform.SetParent(__instance._buttonRoot.transform.Find("ButtonLayout"));
                continueRougeButton.transform.localScale = new Vector3(95, 95, 10);
                continueRougeButton.transform.localPosition = new Vector3(-435, 200, 0);
                continueRougeButton.AddComponent<ButtonInteraction>();

                UnityEngine.UI.Image image0 = continueRougeButton.transform.GetChild(0).GetComponent<UnityEngine.UI.Image>();
                ///////////
                //
                continueRougeButton.transform.GetChild(1).gameObject.AddComponent<FontLoadScript>();
                LocalizeTextLoadScriptWithOutFontLoadScript script0 = continueRougeButton.transform.GetChild(1).gameObject.AddComponent<LocalizeTextLoadScriptWithOutFontLoadScript>();
                script0.id = "Rouge_Title_Continue_Button";

                ContentSizeFitter fitter20 = continueRougeButton.transform.GetChild(1).gameObject.AddComponent<ContentSizeFitter>();
                fitter2.verticalFit = ContentSizeFitter.FitMode.PreferredSize;//首选大小
                fitter2.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;//不调整
                ContentSizeFitter fitter30 = continueRougeButton.transform.GetChild(2).gameObject.AddComponent<ContentSizeFitter>();

                UnityEngine.UI.Text text0 = continueRougeButton.transform.GetChild(1).GetComponent<UnityEngine.UI.Text>();
                text0.alignment = TextAnchor.MiddleCenter;
                text0.horizontalOverflow = HorizontalWrapMode.Wrap;
                text0.verticalOverflow = VerticalWrapMode.Truncate;
                ///////////
                UnityEngine.UI.Button button0 = continueRougeButton.transform.GetChild(2).GetComponent<UnityEngine.UI.Button>();

                Texture2D texture20 = new Texture2D(1, 1);
                texture20.LoadImage(File.ReadAllBytes(path + "/Sprite/StartButton.png"));
                image0.sprite = Sprite.Create(texture2, new Rect(0f, 0f, texture20.width, texture20.height), new Vector2(0.5f, 0.5f));
                image0.transform.localScale *= 10f;
                image0.color = new UnityEngine.Color(1, 1, 1, 0);

                text0.transform.localScale = new Vector3(0.02f, 0.02f, 1);
                GameObject.DestroyObject(continueRougeButton.transform.GetChild(2).GetComponent<UnityEngine.UI.Image>());
                GameObject.DestroyObject(continueRougeButton.transform.GetChild(2).GetChild(0));
                button0.targetGraphic = image0;
                button0.transform.localScale *= 10f;
                continueRougeButton.transform.GetChild(0).transform.localPosition = image0.transform.localPosition + new Vector3(0, 0, 10);
                continueRougeButton.transform.GetChild(0).transform.localScale = image0.transform.localScale;
                button0.onClick.AddListener(delegate
                {
                    LogInfo("Click Continue Button");
                    CallContinueGame_Rougelike();
                });
                if (!File.Exists(path + "/Save/GlobalData.dat") || !File.Exists(path + "/Save/DayData.dat"))
                {
                    button0.interactable = false;
                    text0.color = UnityEngine.Color.gray;
                }
            }
            catch (Exception ex)
            {
                YKMTLogInstance.Error(ex);
            }

        }


        /*
        public static void NewGameModeButton_Awake()
        {
            //在动作库里添加id为666的动作类型，触发动作时启动CallNewGame_Rougelike
            Extension.GetPrivateField<Dictionary<AlterTitleController.TitleActionType, AlterTitleController.TitleCall>>(AlterTitleController.Controller, "_actionLibrary").Add((AlterTitleController.TitleActionType)666, new AlterTitleController.TitleCall(CallNewGame_Rougelike));
        }

        public static void NewGameModeButton_OnClick(int id)
        {
            //当按钮id超过原版时，才会触发。在动作库里搜索对应的动作，如果搜索到就call方法
            if (id >= 9)
            {
                AlterTitleController.TitleCall titleCall = null;
                Dictionary<AlterTitleController.TitleActionType, AlterTitleController.TitleCall> _actionLibrary = Extension.GetPrivateField<Dictionary<AlterTitleController.TitleActionType, AlterTitleController.TitleCall>>(AlterTitleController.Controller, "_actionLibrary");

                if (_actionLibrary.TryGetValue((AlterTitleController.TitleActionType)666, out titleCall))
                {
                    Debug.Log((AlterTitleController.TitleActionType)666);
                    titleCall();
                }
            }
        }
        */

        public static void CallNewGame_Rougelike()
        {
            GlobalGameManager.instance.gameMode = rougeLike;
            GlobalGameManager.instance.isPlayingTutorial = false;
            CreatureGenerate.CreatureGenerateInfoManager.Instance.Init();
            GlobalGameManager.instance.InitStoryMode();
            GlobalGameManager.instance.gameMode = rougeLike;
            PlayerModel.instance.InitAddingCreatures();


            MemeManager.instance.current_dic.Clear();
            MemeManager.instance.current_list.Clear();
            WonderModel.instance.money = 0;
            SetRougelikeGlobalData();
            SetRougelikeDayData();
            SaveGlobalData();
            SaveDayData();
            SaveRougeLikeDayData();
            GlobalGameManager.instance.gameMode = rougeLike;
            if (GlobalGameManager.instance.loadingScreen.isLoading)
            {
                return;
            }
            GlobalGameManager.instance.loadingScene = "TitleEndScene";
            GlobalGameManager.instance.loadingScreen.LoadScene("StoryV2");
        }

        public static void CallContinueGame_Rougelike()
        {
            try
            {
                GlobalGameManager.instance.gameMode = rougeLike;
                LoadGlobalData();
                LoadDayData(SaveType.LASTDAY);
                LoadRougeLikeDayData();
                Extension.CallPrivateMethod<object>(AlterTitleController.Controller, "LoadUnlimitMode", null);
                GlobalGameManager.instance.gameMode = rougeLike;
            }
            catch (Exception message)
            {
                YKMTLogInstance.Error(message);
                Debug.LogError(message);
                GlobalGameManager.instance.ReleaseGame();
            }
        }


        public static bool SetRougelikeGlobalData()//设置新一局肉鸽的全局存档
        {

            try
            {
                //if (!File.Exists(path + "/Save/GlobalData.dat"))
                {
                    //return true;
                }
                //else
                {
                    Dictionary<string, object> dic = SaveUtil.ReadSerializableFile(UnityEngine.Application.persistentDataPath + "/saveGlobal170808.dat");
                    Dictionary<string, object> dic2 = null;
                    Dictionary<string, object> dic3 = null;
                    Dictionary<string, object> dic4 = null;
                    Dictionary<string, object> dic5 = null;
                    Dictionary<string, object> dic6 = null;
                    Dictionary<string, object> dic7 = null;
                    if (GameUtil.TryGetValue<Dictionary<string, object>>(dic, "observe", ref dic2))
                    {
                        CreatureManager.instance.LoadObserveData(SetRandomObserve(dic2, 0, 0, 20, 50));
                    }
                    else
                    {
                        CreatureManager.instance.ResetObserveData();
                    }
                    if (GameUtil.TryGetValue<Dictionary<string, object>>(dic, "etcData", ref dic4))
                    {
                        GlobalEtcDataModel.instance.LoadGlobalData(dic4);
                    }
                    else
                    {
                        GlobalEtcDataModel.instance.ResetGlobalData();
                    }
                    if (GameUtil.TryGetValue<Dictionary<string, object>>(dic, "research", ref dic5))
                    {
                        GlobalEtcDataModel.instance.LoadGlobalData(dic5);
                        //随机部门科技暂时放弃。原因：部门科技与任务绑定
                        //ResearchDataModel.instance.LoadData(SetRandomResearch(dic5, 0.4f));
                    }
                    else
                    {
                        ResearchDataModel.instance.Init();
                    }
                    if (GameUtil.TryGetValue<Dictionary<string, object>>(dic, "missions", ref dic6))
                    {
                        MissionManager.instance.LoadData(dic6);
                    }
                    else
                    {
                        MissionManager.instance.Init();
                    }
                    if (GameUtil.TryGetValue<Dictionary<string, object>>(dic, "inventory", ref dic3))
                    {
                        //MissionManager.instance.LoadData(dic3);

                        float[] rate = { 0.1f, 0.3f, 0.6f, 0.85f, 1 };
                        InventoryModel.Instance.LoadGlobalData(SetRandomEquipment(dic3, 40, 50, rate));
                    }
                    else
                    {
                        InventoryModel.Instance.Init();
                    }
                    if (GameUtil.TryGetValue<Dictionary<string, object>>(dic, "sefiraCharactes", ref dic7))
                    {
                        SefiraCharacterManager.instance.LoadData(dic7);
                    }
                    else
                    {
                        SefiraCharacterManager.instance.Init();
                    }
                }
            }
            catch (Exception ex)
            {
                YKMTLogInstance.Error(ex);
            }

            return false;
        }

        public static void SetRougelikeDayData()//设置新一局肉鸽的天数存档
        {
            PlayerModel.instance.SetDay(30);
            for (int i = 0; i < 5; i++)
            {
                SefiraManager.instance.OpenSefira(SefiraEnum.MALKUT);
                SefiraManager.instance.OpenSefira(SefiraEnum.YESOD);
                SefiraManager.instance.OpenSefira(SefiraEnum.NETZACH);
                SefiraManager.instance.OpenSefira(SefiraEnum.HOD);
                SefiraManager.instance.OpenSefira(SefiraEnum.TIPERERTH1);
                SefiraManager.instance.OpenSefira(SefiraEnum.TIPERERTH2);
            }
            AgentManager.instance.customAgent.Clear();
            AgentManager.instance.Clear();

            SetRandomAgents(15, 25, 60, 90);

            //CreatureManager.instance.Clear();

            float[] rate = { 0.1f, 0.3f, 0.6f, 0.9f, 1f };
            SetRandomCreatures(rate);
            MoneyModel.instance.Add(80);
            //以下为肉鸽新增的内容初始化
            WonderModel.instance.Init();
        }


        public static bool SaveGlobalData()
        {
            try
            {
                if (GlobalGameManager.instance.gameMode == rougeLike)
                {
                    Dictionary<string, object> dictionary = new Dictionary<string, object>
                    {
                        { "observe", CreatureManager.instance.GetSaveObserveData() },
                        { "etcData", GlobalEtcDataModel.instance.GetGlobalSaveData() },
                        { "inventory", InventoryModel.Instance.GetGlobalSaveData() },
                        { "research", ResearchDataModel.instance.GetSaveData() },
                        { "missions", MissionManager.instance.GetSaveData() },
                        { "sefiraCharactes", SefiraCharacterManager.instance.GetSaveData() }
                    };
                    SaveUtil.WriteSerializableFile(path + "/Save/GlobalData.dat", dictionary);
                    return false;
                }

            }
            catch (Exception ex)
            {
                YKMTLogInstance.Error(ex);
            }

            return true;
        }

        public static bool SaveDayData()
        {
            try
            {
                if (GlobalGameManager.instance.gameMode == rougeLike)
                {
                    Dictionary<string, object> dictionary = new Dictionary<string, object>
                    {
                        { "saveVer", "ver1" },
                        { "playTime", GlobalGameManager.instance.playTime }
                    };
                    int day = PlayerModel.instance.GetDay();
                    dictionary.Add("lastDay", day);
                    Dictionary<int, Dictionary<string, object>> dictionary2 = [];
                    Dictionary<string, object> saveDayData = GlobalGameManager.instance.GetSaveDayData();
                    dictionary2.Add(PlayerModel.instance.GetDay(), saveDayData);

                    if (File.Exists(path + "/Save/DayData.dat"))
                    {
                        Dictionary<string, object> dic = SaveUtil.ReadSerializableFile(path + "/Save/DayData.dat");
                        int num = 0;
                        Dictionary<string, object> value2 = null;
                        Dictionary<int, Dictionary<string, object>> dictionary3 = null;
                        if (GameUtil.TryGetValue<int>(dic, "checkPointDay", ref num) && GameUtil.TryGetValue<Dictionary<int, Dictionary<string, object>>>(dic, "dayList", ref dictionary3) && dictionary3.TryGetValue(num, out value2))
                        {
                            dictionary.Add("checkPointDay", 10036);
                            dictionary2.Add(num, value2);
                        }
                    }

                    dictionary.Add("dayList", dictionary2);
                    SaveUtil.WriteSerializableFile(path + "/Save/DayData.dat", dictionary);
                    return false;
                }
            }

            catch (Exception ex)
            {
                YKMTLogInstance.Error(ex);
            }
            return true;
        }

        public static void SaveRougeLikeDayData()//存储某天的肉鸽相关内容，例如D35的模因和奇思
        {
            //结构如下：dictionary的key表示存储的内容类型，例如wonder代表奇思，meme代表模因
            //dictionary的value里是具体存储的内容
            Dictionary<string, object> dictionary = new Dictionary<string, object>
            {
                { "wonder", WonderModel.instance.money },
                { "meme", MemeManager.instance.current_dic }
            };

            SaveUtil.WriteSerializableFile(path + "/Save/RougeLikeDayData.dat", dictionary);
        }

        public static void LoadGlobalData()
        {
            try
            {
                if (!File.Exists(path + "/Save/GlobalData.dat"))
                {
                    CreatureManager.instance.ResetObserveData();
                    GlobalEtcDataModel.instance.ResetGlobalData();
                    ResearchDataModel.instance.Init();
                    InventoryModel.Instance.Init();
                    MissionManager.instance.Init();
                    SefiraCharacterManager.instance.Init();
                }
                else
                {
                    Dictionary<string, object> dic = SaveUtil.ReadSerializableFile(path + "/Save/GlobalData.dat");
                    Dictionary<string, object> dic2 = null;
                    Dictionary<string, object> dic3 = null;
                    Dictionary<string, object> dic4 = null;
                    Dictionary<string, object> dic5 = null;
                    Dictionary<string, object> dic6 = null;
                    Dictionary<string, object> dic7 = null;
                    if (GameUtil.TryGetValue<Dictionary<string, object>>(dic, "observe", ref dic2))
                    {
                        CreatureManager.instance.LoadObserveData(dic2);
                    }
                    else
                    {
                        CreatureManager.instance.ResetObserveData();
                    }
                    if (GameUtil.TryGetValue<Dictionary<string, object>>(dic, "etcData", ref dic4))
                    {
                        GlobalEtcDataModel.instance.LoadGlobalData(dic4);
                    }
                    else
                    {
                        GlobalEtcDataModel.instance.ResetGlobalData();
                    }
                    if (GameUtil.TryGetValue<Dictionary<string, object>>(dic, "research", ref dic5))
                    {
                        ResearchDataModel.instance.LoadData(dic5);
                    }
                    else
                    {
                        ResearchDataModel.instance.Init();
                    }
                    if (GameUtil.TryGetValue<Dictionary<string, object>>(dic, "missions", ref dic6))
                    {
                        MissionManager.instance.LoadData(dic6);
                    }
                    else
                    {
                        MissionManager.instance.Init();
                    }
                    if (GameUtil.TryGetValue<Dictionary<string, object>>(dic, "inventory", ref dic3))
                    {
                        InventoryModel.Instance.LoadGlobalData(dic3);
                    }
                    else
                    {
                        InventoryModel.Instance.Init();
                    }
                    if (GameUtil.TryGetValue<Dictionary<string, object>>(dic, "sefiraCharactes", ref dic7))
                    {
                        SefiraCharacterManager.instance.LoadData(dic7);
                    }
                    else
                    {
                        SefiraCharacterManager.instance.Init();
                    }
                }
            }
            catch (Exception ex)
            {
                YKMTLogInstance.Error(ex);
            }
        }
        //?
        public static void LoadDayData(SaveType saveType)//加载原版天数存档
        {
            try
            {

                if (!File.Exists(path + "/Save/DayData.dat"))
                {
                    return;
                }
                else
                {
                    Extension.CallPrivateMethod<object>(GlobalGameManager.instance, "LoadData_preprocess", null);
                    //GlobalGameManager.instance.LoadData_preprocess();
                    Dictionary<string, object> dic = SaveUtil.ReadSerializableFile(path + "/Save/DayData.dat");
                    //Dictionary<string, object> dic = GlobalGameManager.instance.LoadSaveFile();
                    string text = "old";
                    GameUtil.TryGetValue<string>(dic, "saveVer", ref text);
                    GameUtil.TryGetValue<float>(dic, "playTime", ref GlobalGameManager.instance.playTime);
                    int key = 0;
                    int key2 = 0;
                    Dictionary<int, Dictionary<string, object>> dictionary = null;
                    GameUtil.TryGetValue<Dictionary<int, Dictionary<string, object>>>(dic, "dayList", ref dictionary);
                    GameUtil.TryGetValue<int>(dic, "checkPointDay", ref key2);
                    GameUtil.TryGetValue<int>(dic, "lastDay", ref key);
                    Dictionary<string, object> dictionary2 = null;
                    if (!dictionary.TryGetValue(key, out dictionary2))
                    {
                        throw new Exception("lastDay not found (saveVer : " + text + ")");
                    }
                    Dictionary<string, object> data = dictionary2;
                    if (text == "old")
                    {
                        Extension.CallPrivateMethod<object>(GlobalGameManager.instance, "LoadDay", new object[] { dictionary2 });
                        //GlobalGameManager.instance.LoadDay(dictionary2);
                        GlobalGameManager.instance.SaveData(true);
                    }
                    else
                    {
                        Dictionary<string, object> data2 = null;
                        bool flag = dictionary.TryGetValue(key2, out data2);
                        if (saveType == SaveType.LASTDAY)
                        {
                            Extension.CallPrivateMethod<object>(GlobalGameManager.instance, "LoadDay", new object[] { data });
                            //this.LoadDay(data);
                            if (!GlobalGameManager.instance.dlcCreatureOn)
                            {
                                bool flag2 = CreatureManager.instance.ReplaceAllDlcCreature();
                                flag2 = (InventoryModel.Instance.RemoveAllDlcEquipment() || flag2);
                                flag2 = (AgentManager.instance.RemoveAllDlcEquipment() || flag2);
                                if (flag2)
                                {
                                    Debug.Log("exists removed DLC data");
                                    SaveDayData();
                                    SaveGlobalData();
                                }
                            }
                        }
                        else
                        {
                            if (saveType != SaveType.CHECK_POINT)
                            {
                                throw new Exception("invalid SaveType");
                            }
                            Extension.CallPrivateMethod<object>(GlobalGameManager.instance, "LoadDay", new object[] { data2 });
                            //this.LoadDay(data2);
                            if (GlobalGameManager.instance.dlcCreatureOn)
                            {
                                SaveDayData();
                            }
                            else
                            {
                                bool flag3 = CreatureManager.instance.ReplaceAllDlcCreature();
                                flag3 = (InventoryModel.Instance.RemoveAllDlcEquipment() || flag3);
                                flag3 = (AgentManager.instance.RemoveAllDlcEquipment() || flag3);
                                if (flag3)
                                {
                                    Debug.Log("exists removed DLC data");
                                    SaveDayData();
                                    SaveGlobalData();
                                }
                                else
                                {
                                    SaveDayData();
                                }
                            }
                        }
                    }

                    WonderModel.instance.LoadData(data);
                }

            }
            catch (Exception ex)
            {
                YKMTLogInstance.Error(ex);
            }
            return;
        }

        public static void LoadRougeLikeDayData()//加载某天的肉鸽相关内容，例如D35的模因和奇思
        {
            if (!File.Exists(path + "/Save/RougeLikeDayData.dat"))
            {
                return;
            }
            else
            {
                Dictionary<string, object> dic = SaveUtil.ReadSerializableFile(path + "/Save/RougeLikeDayData.dat");

                WonderModel.instance.LoadData(dic);
                MemeManager.instance.LoadData(dic);
            }
        }
        public static void LoadDay(Dictionary<string, object> data)
        {
            try
            {
                Dictionary<string, object> dic = null;
                Dictionary<string, object> dic2 = null;
                Dictionary<string, object> dic3 = null;
                Dictionary<string, object> dic4 = null;
                Dictionary<string, object> dic5 = null;
                Dictionary<string, object> dic6 = null;
                string text = "old";
                GameUtil.TryGetValue<string>(data, "saveInnerVer", ref text);
                YKMTLogInstance.Debug("LoadDay Debugnum 1");
                //int dayFromSaveData = GlobalGameManager.instance.GetDayFromSaveData(data);
                GameUtil.TryGetValue<Dictionary<string, object>>(data, "money", ref dic);
                YKMTLogInstance.Debug("LoadDay Debugnum 2");
                GameUtil.TryGetValue<Dictionary<string, object>>(data, "agents", ref dic2);
                YKMTLogInstance.Debug("LoadDay Debugnum 3");
                GameUtil.TryGetValue<Dictionary<string, object>>(data, "creatures", ref dic3);
                YKMTLogInstance.Debug("LoadDay Debugnum 4");
                GameUtil.TryGetValue<Dictionary<string, object>>(data, "playerData", ref dic4);
                YKMTLogInstance.Debug("LoadDay Debugnum 5");
                GameUtil.TryGetValue<Dictionary<string, object>>(data, "sefiras", ref dic5);
                YKMTLogInstance.Debug("LoadDay Debugnum 6");
                GameUtil.TryGetValue<string>(data, "saveState", ref GlobalGameManager.instance.saveState);
                YKMTLogInstance.Debug("LoadDay Debugnum 7");
                if (GameUtil.TryGetValue<Dictionary<string, object>>(data, "agentName", ref dic6))
                {
                    AgentNameList.instance.LoadData(dic6);
                }
                MoneyModel.instance.LoadData(dic);
                YKMTLogInstance.Debug("LoadDay Debugnum 8");
                PlayerModel.instance.LoadData(dic4);
                YKMTLogInstance.Debug("LoadDay Debugnum 9");
                SefiraManager.instance.LoadData(dic5);
                YKMTLogInstance.Debug("LoadDay Debugnum 10");
                //AgentManager.instance.LoadCustomAgentData();
                YKMTLogInstance.Debug("LoadDay Debugnum 11");
                CreatureManager.instance.LoadData(dic3);
                YKMTLogInstance.Debug("LoadDay Debugnum 12");
                //AgentManager.instance.LoadDelAgentData();
                YKMTLogInstance.Debug("LoadDay Debugnum 13");
                AgentManager.instance.LoadData(dic2);
                YKMTLogInstance.Debug("LoadDay Debugnum 14");
                GlobalGameManager.instance.gameMode = rougeLike;
            }
            catch (Exception ex)
            {
                YKMTLogInstance.Error(ex);
            }
        }


        public static void OnClearStage()//只有在开始下一天的时候才会触发该函数
        {
            if (GlobalGameManager.instance.gameMode == rougeLike)
            {
                SaveGlobalData();
                SaveDayData();
                SaveRougeLikeDayData();
                if (PlayerModel.instance.GetDay() + 1 == 40)
                {
                    ReturnToTitleOnGameOver();
                }
            }
        }



        /// <summary>
        /// 思考：要不要同时加入最大最小限制和概率？
        /// </summary>
        /// <param name="origin_research"></param>
        /// <param name="rate">这是获得部门科技的概率</param>
        public static Dictionary<string, object> SetRandomResearch(Dictionary<string, object> origin_research, float rate)
        {
            //结构：origin_research字典套research_list列表套dic字典套id
            Dictionary<string, object> result = [];

            List<Dictionary<string, object>> research_list = [];
            GameUtil.TryGetValue<List<Dictionary<string, object>>>(origin_research, "research", ref research_list);
            int research_count = research_list.Count;
            research_list.Clear();

            List<int> research_id_list = [1, 2, 103, 3, 4, 5, 6, 7, 203, 8, 9, 10, 501, 502, 701, 702, 703, 801, 802, 803, 901, 902, 903, 1001, 1002, 1003];
            //你加10000干嘛 给你删了
            for (int i = 0; i < research_id_list.Count; i++)
            {
                Dictionary<string, object> dic = [];
                if (UnityEngine.Random.value <= rate)
                {
                    dic.Add("researchItemTypeId", research_id_list[i]);
                    dic.Add("curLevel", 1);
                }
                else
                {
                    dic.Add("researchItemTypeId", research_id_list[i]);
                    dic.Add("curLevel", 0);
                }

                //research_id_list.RemoveAt(research_id_list.Count - 1);

                research_list.Add(dic);
            }


            result.Add("research", research_list);
            return result;
        }
        public static Dictionary<string, object> SetRandomObserve(Dictionary<string, object> observe, int stat_min, int stat_max, int cube_min, int cube_max)
        {
            Dictionary<string, object> result = [];

            Dictionary<long, Dictionary<string, object>> all_observe_dic_with_id = [];//观察列表
            Dictionary<string, Dictionary<long, Dictionary<string, object>>> dictionary2 = [];//模组观察列表

            //观察信息分成observeProgress,cubeNum,stat,defense,work,care。

            GameUtil.TryGetValue<Dictionary<long, Dictionary<string, object>>>(observe, "observeList", ref all_observe_dic_with_id);
            GameUtil.TryGetValue<Dictionary<string, Dictionary<long, Dictionary<string, object>>>>(observe, "observeListMod", ref dictionary2);


            bool dlcCreatureOn = GlobalGameManager.instance.dlcCreatureOn;//获取所有可用的异想体id列表，但不包含工具型
            List<long> all_creature_list;
            if (dlcCreatureOn)
            {
                all_creature_list = new List<long>(CreatureGenerateInfo.all);
            }
            else
            {
                all_creature_list = new List<long>(CreatureGenerateInfo.all_except_creditCreatures);
            }
            foreach (long item in CreatureGenerateInfo.kitCreature)
            {
                all_creature_list.Remove(item);
            }

            all_observe_dic_with_id.Clear();

            //Dictionary<long, Dictionary<string, object>> all_observe_dic_with_id = new Dictionary<long, Dictionary<string, object>>();

            for (int i = 0; i < all_creature_list.Count; i++)
            {
                Dictionary<string, object> single_observe_dic = new Dictionary<string, object>
                {
                    { "stat", false },
                    { "defense", false },
                    { "work_r", false },
                    { "work_w", false },
                    { "work_b", false },
                    { "work_p", false },
                    { "care_0", false },
                    { "care_1", false },
                    { "care_2", false },
                    { "care_3", false },
                    { "care_4", false },
                    { "care_5", false },
                    { "care_6", false }
                };//单个异想体的具体观察信息

                List<string> stat_list =
                [
                    //stat_list.Add("stat");
                    "defense",
                    "work_r",
                    "work_w",
                    "work_b",
                    "work_p",
                    "care_0",
                    "care_1",
                    "care_2",
                    "care_3",
                    "care_4",
                    "care_5",
                    "care_6",
                ];

                int stat_num = UnityEngine.Random.Range(stat_min, stat_max);//单个异想体图鉴中随机解锁的条目数量
                for (int i1 = 0; i1 < stat_num; i1++)
                {
                    // 随机选择一个键
                    int stat_index = UnityEngine.Random.Range(0, stat_list.Count);

                    string keyToChange = stat_list[stat_index];

                    // 将选中键的值设置为true
                    single_observe_dic[keyToChange] = true;

                    //删除已选择过的键
                    stat_list.RemoveAt(stat_index);
                }
                single_observe_dic["stat"] = true;
                single_observe_dic.Add("observeProgress", 0);
                single_observe_dic.Add("cubeNum", UnityEngine.Random.Range(cube_min, cube_max));
                single_observe_dic.Add("totalKitUseCount", 0);
                single_observe_dic.Add("totalKitUseTime", 0);


                all_observe_dic_with_id.Add(all_creature_list[i], single_observe_dic);
            }
            result.Add("observeList", all_observe_dic_with_id);
            result.Add("observeListMod", dictionary2);

            return result;
        }

        public static Dictionary<string, object> SetRandomEquipment(Dictionary<string, object> equipment, int equip_min, int equip_max, float[] rate)
        {
            Dictionary<string, object> result = [];
            try
            {
                List<InventoryModel.EquipmentSaveData> result_list = [];
                List<string> result_mod_list = [];//没用，占位符
                //GameUtil.TryGetValue<List<InventoryModel.EquipmentSaveData>>(equipment, "equips", ref result_list);
                //GameUtil.TryGetValue<List<string>>(equipment, "equipsMod", ref result_mod_list);
                long new_instance_id = 10000;//存储实例id
                result_list.Clear();//随后清除原列表

                int equip_num = UnityEngine.Random.Range(equip_min, equip_max);

                List<int> equip_id = GetAllEquipmentidList();
                //移除失乐园和薄瞑
                equip_id.Remove(200015);
                equip_id.Remove(300015);
                equip_id.Remove(200038);
                equip_id.Remove(300038);

                List<int> z_equip_id = [];
                List<int> t_equip_id = [];
                List<int> h_equip_id = [];
                List<int> w_equip_id = [];
                List<int> a_equip_id = [];

                foreach (int id in equip_id)//装备id分级
                {
                    if (EquipmentTypeList.instance.GetData(id) == null)
                    {
                        continue;
                    }
                    if (EquipmentTypeList.instance.GetData(id).grade == "1")
                    {
                        z_equip_id.Add(id);
                    }
                    else if (EquipmentTypeList.instance.GetData(id).grade == "2")
                    {
                        t_equip_id.Add(id);
                    }
                    else if (EquipmentTypeList.instance.GetData(id).grade == "3")
                    {
                        h_equip_id.Add(id);
                    }
                    else if (EquipmentTypeList.instance.GetData(id).grade == "4")
                    {
                        w_equip_id.Add(id);
                    }
                    else if (EquipmentTypeList.instance.GetData(id).grade == "5")
                    {
                        a_equip_id.Add(id);
                    }

                }
                for (int i = 0; i < equip_num; i++)
                {
                    InventoryModel.EquipmentSaveData equipmentSaveData = new InventoryModel.EquipmentSaveData();

                    float value = UnityEngine.Random.value;
                    if (value <= rate[0] && z_equip_id.Count != 0)//刷到Z级
                    {
                        int random_id = UnityEngine.Random.Range(0, z_equip_id.Count);//随机选取一个id
                        equipmentSaveData.equipTypeId = z_equip_id[random_id];
                        equipmentSaveData.equipInstanceId = new_instance_id + i;
                        if (!InventoryModel.Instance.CheckEquipmentCount(z_equip_id[random_id]))//如果装备已超出自身上限
                        {
                            z_equip_id.RemoveAt(random_id);//列表中删除此id避免重复选中
                        }
                    }
                    else if (value <= rate[1] && t_equip_id.Count != 0)//刷到T级
                    {
                        int random_id = UnityEngine.Random.Range(0, t_equip_id.Count);//随机选取一个id
                        equipmentSaveData.equipTypeId = t_equip_id[random_id];
                        equipmentSaveData.equipInstanceId = new_instance_id + i;

                        if (!InventoryModel.Instance.CheckEquipmentCount(t_equip_id[random_id]))//如果装备已超出自身上限
                        {
                            t_equip_id.RemoveAt(random_id);//列表中删除此id避免重复选中
                        }
                    }
                    else if (value <= rate[2] && h_equip_id.Count != 0)//刷到H级
                    {
                        int random_id = UnityEngine.Random.Range(0, h_equip_id.Count);//随机选取一个id
                        equipmentSaveData.equipTypeId = h_equip_id[random_id];
                        equipmentSaveData.equipInstanceId = new_instance_id + i;
                        if (!InventoryModel.Instance.CheckEquipmentCount(h_equip_id[random_id]))//如果装备已超出自身上限
                        {
                            h_equip_id.RemoveAt(random_id);//列表中删除此id避免重复选中
                        }
                    }
                    else if (value <= rate[3] && w_equip_id.Count != 0)//刷到W级
                    {
                        int random_id = UnityEngine.Random.Range(0, w_equip_id.Count);//随机选取一个id
                        equipmentSaveData.equipTypeId = w_equip_id[random_id];
                        equipmentSaveData.equipInstanceId = new_instance_id + i;
                        if (!InventoryModel.Instance.CheckEquipmentCount(w_equip_id[random_id]))//如果装备已超出自身上限
                        {
                            w_equip_id.RemoveAt(random_id);//列表中删除此id避免重复选中
                        }
                    }
                    else if (value <= rate[4] && a_equip_id.Count != 0)//刷到A级
                    {
                        int random_id = UnityEngine.Random.Range(0, a_equip_id.Count);//随机选取一个id
                        equipmentSaveData.equipTypeId = a_equip_id[random_id];
                        equipmentSaveData.equipInstanceId = new_instance_id + i;
                        if (!InventoryModel.Instance.CheckEquipmentCount(a_equip_id[random_id]))//如果装备已超出自身上限
                        {
                            a_equip_id.RemoveAt(random_id);//列表中删除此id避免重复选中
                        }

                    }
                    if (equipmentSaveData == null)
                    {
                        File.AppendAllText(path + "/RandomEquipError0.txt", "NoSaveData");
                    }

                    result_list.Add(equipmentSaveData);

                }
                result.Add("equips", result_list);
                //不要加下面这一条，会导致随机装备爆炸
                //result.Add("equipsMod", result_mod_list);
                result.Add("nextInstanceId", 20000L);
            }

            catch (Exception ex)
            {
                YKMTLogInstance.Error(ex);
            }


            return result;
        }

        /// <summary>
        /// 获得全部装备ID列表
        /// </summary>
        /// <returns></returns>
        public static List<int> GetAllEquipmentidList()
        {
            List<int> list = [];
            //List<int> list2 = new List<int>();
            string text = string.Empty;
            bool flag = File.Exists(UnityEngine.Application.dataPath + "/Managed/BaseMod/BaseEquipment.txt");
            if (flag)
            {
                XmlDocument xmlDocument = new XmlDocument();
                string xml = File.ReadAllText(UnityEngine.Application.dataPath + "/Managed/BaseMod/BaseEquipment.txt");
                xmlDocument.LoadXml(xml);
                foreach (object obj in xmlDocument.SelectSingleNode("equipment_list").SelectNodes("equipment"))
                {
                    XmlNode xmlNode = (XmlNode)obj;
                    bool flag2 = xmlNode.Attributes.GetNamedItem("id") != null;
                    if (flag2)
                    {
                        int num = int.Parse(xmlNode.Attributes.GetNamedItem("id").InnerText);
                        bool flag3 = xmlNode.Attributes.GetNamedItem("type").InnerText == "weapon" && num != 177777 && num >= 200001 && num <= 200106;
                        if (flag3)
                        {
                            text = text + num.ToString() + "\n";
                            list.Add(num);
                        }
                        else
                        {
                            bool flag4 = xmlNode.Attributes.GetNamedItem("type").InnerText == "armor" && num >= 300001;//这段是自己加的，屏蔽一些奇怪装备，可能导致BUG
                            if (flag4)
                            {
                                list.Add(num);
                            }
                        }
                    }
                }
            }
            DirectoryInfo[] directories = new DirectoryInfo(UnityEngine.Application.dataPath + "/BaseMods").GetDirectories();
            foreach (DirectoryInfo directoryInfo in directories)
            {
                bool flag5 = Directory.Exists(directoryInfo.FullName + "/Equipment/txts");
                if (flag5)
                {
                    DirectoryInfo directoryInfo2 = new DirectoryInfo(directoryInfo.FullName + "/Equipment/txts");
                    bool flag6 = directoryInfo2 != null;
                    if (flag6)
                    {
                        bool flag7 = directoryInfo2.GetFiles().Length != 0;
                        if (flag7)
                        {
                            foreach (FileInfo fileInfo in directoryInfo2.GetFiles())
                            {
                                bool flag8 = fileInfo.Name.Contains(".txt");
                                if (flag8)
                                {
                                    XmlDocument xmlDocument2 = new XmlDocument();
                                    string xml2 = File.ReadAllText(fileInfo.FullName);
                                    xmlDocument2.LoadXml(xml2);
                                    foreach (object obj2 in xmlDocument2.SelectSingleNode("equipment_list").SelectNodes("equipment"))
                                    {
                                        XmlNode xmlNode2 = (XmlNode)obj2;
                                        bool flag9 = xmlNode2.Attributes.GetNamedItem("id") != null;
                                        if (flag9)
                                        {
                                            int item = int.Parse(xmlNode2.Attributes.GetNamedItem("id").InnerText);
                                            bool flag10 = xmlNode2.Attributes.GetNamedItem("type").InnerText == "weapon";
                                            if (flag10)
                                            {
                                                text = text + item.ToString() + "\n";
                                                list.Add(item);
                                            }
                                            else
                                            {
                                                bool flag11 = xmlNode2.Attributes.GetNamedItem("type").InnerText == "armor";
                                                if (flag11)
                                                {
                                                    list.Add(item);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            //Harmony_Patch.weapon = list;
            //Harmony_Patch.armor = list2;
            return list;
        }

        public static void SetRandomAgents(int count_min, int count_max, int stat_min, int stat_max)
        {
            try
            {
                //AgentManager.instance.customAgent.Clear();
                //AgentManager.instance.Clear();
                //Dictionary<string, object> result = new Dictionary<string, object>();

                //File.WriteAllText(path + "/RandomAgentError1.txt", "Start");
                var nowDifficulty = DifficultyManager.GetNowDifficulty();
                int agentAdder = nowDifficulty.AgentAdder();
                int agentReplacer = nowDifficulty.AgentReplacer();
                int agent_count = 0;
                if (agentReplacer != -1)
                {
                    agent_count = agentReplacer;
                }
                else
                {
                    agent_count = UnityEngine.Random.Range(count_min, count_max + 1) + agentAdder;
                }
                for (int i = 0; i < agent_count; i++)
                {
                    AgentModel agentModel = AgentManager.instance.AddSpareAgentModel();
                    agentModel.primaryStat.hp = UnityEngine.Random.Range(stat_min, stat_max + 1);
                    agentModel.primaryStat.mental = UnityEngine.Random.Range(stat_min, stat_max + 1);
                    agentModel.primaryStat.work = UnityEngine.Random.Range(stat_min, stat_max + 1);
                    agentModel.primaryStat.battle = UnityEngine.Random.Range(stat_min, stat_max + 1);

                    //自动分配部门
                    foreach (Sefira sefira in SefiraManager.instance.GetOpendSefiraList())
                    {
                        if (sefira.agentList.Count >= 3)
                        {
                            continue;
                        }
                        agentModel.SetCurrentSefira(sefira.indexString);
                    }
                    //自动穿装备
                    foreach (EquipmentModel equipment in InventoryModel.Instance.GetWaitingEquipmentList())
                    {
                        if (agentModel.Equipment.weapon.metaInfo.id != 1 && agentModel.Equipment.armor.metaInfo.id != 22)
                        {
                            break;
                        }
                        if (equipment is WeaponModel)
                        {
                            agentModel.SetWeapon(equipment as WeaponModel);
                        }
                        else if (equipment is ArmorModel)
                        {
                            agentModel.SetArmor(equipment as ArmorModel);
                        }
                    }

                    //result.Add(agentModel);
                }
                //File.WriteAllText(path + "/RandomAgentError1.txt", "End");
            }
            catch (Exception ex)
            {
                YKMTLogInstance.Error(ex);
            }

            //AgentManager.instance.GetSaveData();

        }

        public static void SetRandomCreatures(float[] rate)
        {
            CreatureManager.instance.Clear();
            try
            {
                bool dlcCreatureOn = GlobalGameManager.instance.dlcCreatureOn;//获取所有可用的异想体id列表，但不包含工具型
                List<long> all_creature_list;
                if (dlcCreatureOn)
                {
                    all_creature_list = new List<long>(CreatureGenerateInfo.all);
                }
                else
                {
                    all_creature_list = new List<long>(CreatureGenerateInfo.all_except_creditCreatures);
                }
                foreach (long item in CreatureGenerateInfo.kitCreature)
                {
                    all_creature_list.Remove(item);
                }
                //all_creature_list.Remove(100014L);
                //all_creature_list.Remove(100015L);
                //all_creature_list.Remove(100104L);

                using (StreamReader sr = new StreamReader(path + "/Config/RandomCreatureBlackList.txt"))
                {
                    while (sr.ReadLine() != "BlackList")
                    {
                    }
                    string line;
                    while ((line = sr.ReadLine()) != "BlackListEnd")
                    {
                        if (all_creature_list.Contains(Convert.ToInt64(line)))
                        {
                            all_creature_list.Remove(Convert.ToInt64(line));
                        }
                    }
                }

                List<long> z_creature_list = [];
                List<long> t_creature_list = [];
                List<long> h_creature_list = [];
                List<long> w_creature_list = [];
                List<long> a_creature_list = [];

                foreach (long id in all_creature_list)
                {
                    if (CreatureTypeList.instance.GetData(id).GetRiskLevel() == RiskLevel.ZAYIN)
                    {
                        z_creature_list.Add(id);
                    }
                    else if (CreatureTypeList.instance.GetData(id).GetRiskLevel() == RiskLevel.TETH)
                    {
                        t_creature_list.Add(id);
                    }
                    else if (CreatureTypeList.instance.GetData(id).GetRiskLevel() == RiskLevel.HE)
                    {
                        h_creature_list.Add(id);
                    }
                    else if (CreatureTypeList.instance.GetData(id).GetRiskLevel() == RiskLevel.WAW)
                    {
                        w_creature_list.Add(id);
                    }
                    else if (CreatureTypeList.instance.GetData(id).GetRiskLevel() == RiskLevel.ALEPH)
                    {
                        a_creature_list.Add(id);
                    }
                }
                for (int sefira_id = 1; sefira_id < 7; sefira_id++)//一直到中本2全塞满
                {
                    long[] random_id = new long[4];
                    for (int i = 0; i < 4; i++)
                    {
                        float value = UnityEngine.Random.value;
                        int random = 0;
                        if (value <= rate[0])
                        {
                            random = UnityEngine.Random.Range(0, z_creature_list.Count);
                            random_id[i] = z_creature_list[random];
                            z_creature_list.RemoveAt(random);//删除异想体避免重复选中
                        }
                        else if (value <= rate[1])
                        {
                            random = UnityEngine.Random.Range(0, z_creature_list.Count);
                            random_id[i] = t_creature_list[random];
                            t_creature_list.RemoveAt(random);//删除异想体避免重复选中
                        }
                        else if (value <= rate[2])
                        {
                            random = UnityEngine.Random.Range(0, h_creature_list.Count);
                            random_id[i] = h_creature_list[random];
                            h_creature_list.RemoveAt(random);//删除异想体避免重复选中
                        }
                        else if (value <= rate[3])
                        {
                            random = UnityEngine.Random.Range(0, w_creature_list.Count);
                            random_id[i] = w_creature_list[random];
                            w_creature_list.RemoveAt(random);//删除异想体避免重复选中
                        }
                        else if (value <= rate[4])
                        {
                            random = UnityEngine.Random.Range(0, a_creature_list.Count);
                            random_id[i] = a_creature_list[random];
                            a_creature_list.RemoveAt(random);//删除异想体避免重复选中
                        }
                        Sefira sefira = SefiraManager.instance.GetSefira(sefira_id);
                        AddCreature(random_id[i], sefira);
                    }
                }

            }
            catch (Exception ex)
            {
                YKMTLogInstance.Error(ex);
            }
            //CreatureManager.instance.AddCreatureInSefira()
        }

        public static void AddCreature(long id, Sefira sefira)
        {
            List<long> list2 = new List<long>(CreatureGenerateInfo.GetAll(false));
            foreach (CreatureModel creatureModel in CreatureManager.instance.GetCreatureList())
            {
                list2.Remove(creatureModel.metadataId);
            }

            long[] ary = { id };
            SefiraIsolate[] array = sefira.isolateManagement.GenIsolateByCreatureAryByOrder(ary);
            foreach (SefiraIsolate sefiraIsolate in array)
            {
                CreatureManager.instance.AddCreature(sefiraIsolate.creatureId, sefiraIsolate, sefira.indexString);
            }
        }

        public static void NoCheckPointButton_EscapeUI()
        {
            if (GlobalGameManager.instance.gameMode == rougeLike)
            {
                //EscapeUI.instance.MiddleAreaControl.transform.GetChild(0).gameObject.SetActive(false);
                EscapeUI.instance.MiddleAreaControl.transform.GetChild(1).gameObject.SetActive(false);
            }
        }

        public static void NoRestartButton_ResultScreen()
        {
            if (GlobalGameManager.instance.gameMode == rougeLike)
            {
                ResultScreen.instance.root.transform.GetChild(0).GetChild(4).GetChild(0).GetChild(0).gameObject.SetActive(false);
            }
        }

        public static void NoStory(StoryUI __instance)
        {
            if (GlobalGameManager.instance.gameMode == rougeLike)
            {
                __instance.Clear();
                ExpandUI.instance.Init(new ExpandUI.OnOpenEvent(delegate { }));
            }
        }

        public static void ObserveCostBoost(ref int __result)
        {
            if (GlobalGameManager.instance.gameMode == rougeLike)
            {
                __result = Convert.ToInt32(__result * 1.5f);
            }
        }

        public static void ObserveGetLOB(CreatureModel __instance)
        {
            if (GlobalGameManager.instance.gameMode == rougeLike)
            {
                if (__instance.GetObservationLevel() == 4)
                {
                    if (__instance.GetRiskLevel() == 1)
                    {
                        MoneyModel.instance.Add(2);
                    }
                    else if (__instance.GetRiskLevel() == 2)
                    {
                        MoneyModel.instance.Add(3);
                    }
                    else if (__instance.GetRiskLevel() == 3)
                    {
                        MoneyModel.instance.Add(7);
                    }
                }
            }
        }

        public static void ObserveGetBouns(AgentModel __instance)
        {
            if (GlobalGameManager.instance.gameMode == rougeLike)
            {
                Dictionary<long, CreatureObserveInfoModel> dic = Extension.GetPrivateField<Dictionary<long, CreatureObserveInfoModel>>(CreatureManager.instance, "observeInfoList");
                foreach (KeyValuePair<long, CreatureObserveInfoModel> kvp in dic)//遍历所有观察信息
                {
                    CreatureObserveInfoModel creatureObserveInfoModel = kvp.Value;
                    if (creatureObserveInfoModel.GetObservationLevel() == 4)//如果观察等级达到4
                    {
                        CreatureTypeInfo creatureTypeInfo = CreatureTypeList.instance.GetData(creatureObserveInfoModel.creatureTypeId);
                        if (creatureTypeInfo.GetRiskLevel() == RiskLevel.WAW)//如果是W
                        {
                            UnitStatBuf unitStatBuf = new UnitStatBuf(float.MaxValue)
                            {
                                duplicateType = BufDuplicateType.UNLIMIT,
                                maxHp = 10,
                                maxMental = 10,
                                workProb = 10,
                                cubeSpeed = 10,
                                movementSpeed = 10,
                                attackSpeed = 10
                            };

                            __instance.AddUnitBuf(unitStatBuf);
                        }
                        else if (creatureTypeInfo.GetRiskLevel() == RiskLevel.ALEPH)//如果是A
                        {
                            UnitStatBuf unitStatBuf = new UnitStatBuf(float.MaxValue)
                            {
                                duplicateType = BufDuplicateType.UNLIMIT,
                                maxHp = 20,
                                maxMental = 20,
                                workProb = 20,
                                cubeSpeed = 20,
                                movementSpeed = 20,
                                attackSpeed = 20
                            };

                            __instance.AddUnitBuf(unitStatBuf);
                        }
                    }
                }
                __instance.hp = __instance.maxHp;
                __instance.mental = __instance.maxMental;
            }
        }

        public static void EquipmentCostDecrease(ref int __result)
        {
            if (GlobalGameManager.instance.gameMode == rougeLike)
            {
                __result = Convert.ToInt32(__result * 0.75f);
            }
        }

        public static void ReturnToTitleOnGameOver()
        {
            if (SefiraManager.instance.GameOverCheck())
            {
                if (GlobalGameManager.instance.gameMode == rougeLike)
                {
                    //如果未部署员工数少于当前开放的部门数
                    if (AgentManager.instance.agentListSpare.Count < SefiraManager.instance.GetOpendSefiraList().Length)
                    {
                        GameManager.currentGameManager.ReturnToTitle();
                        File.Delete(path + "/Save/DayData.dat");
                        File.Delete(path + "/Save/DayData.dat.backup");
                        File.Delete(path + "/Save/GlobalData.dat");
                        File.Delete(path + "/Save/GlobalData.backup");
                    }
                    else
                    {
                        SaveGlobalData();
                        SaveDayData();
                        LoadGlobalData();
                        LoadDayData(SaveType.LASTDAY);
                        Extension.CallPrivateMethod<object>(AlterTitleController.Controller, "LoadUnlimitMode", null);
                        GlobalGameManager.instance.gameMode = rougeLike;
                    }
                }
            }

        }

        public static void ResultScreen_Board()
        {
            GameObject.Destroy(ResultScreen.instance.root.transform.GetChild(0).GetChild(6).GetChild(0).gameObject.GetComponent<FontLoadScript>());
            GameObject.Destroy(ResultScreen.instance.root.transform.GetChild(0).GetChild(6).GetChild(0).gameObject.GetComponent<LocalizeTextLoadScript>());

            UnityEngine.UI.Text title = ResultScreen.instance.root.transform.GetChild(0).GetChild(6).GetChild(0).GetComponent<UnityEngine.UI.Text>();
            title.text = LocalizeTextDataModel.instance.GetText("ResultScreen_Title");

            UnityEngine.UI.Text text = ResultScreen.instance.root.transform.GetChild(0).GetChild(6).gameObject.AddComponent<UnityEngine.UI.Text>();
            text.text = "AAAAAAAA";
            text.color = UnityEngine.Color.white;
            text.transform.localPosition = Vector3.zero;
            text.transform.localScale = Vector3.one * 100;
        }
    }

    public class ButtonInteraction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {

        void Start()
        {
            int num = 0;
            try
            {
                Texture2D tex = new Texture2D(1, 1); num++;
                tex.LoadImage(File.ReadAllBytes(ButtonInteraction_Data.path + "/Sprite/Background.png")); num++;
                ButtonInteraction_Data.sp = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), Vector2.one / 2); num++;
                ButtonInteraction_Data.originSprite = AlterTitleController.Controller._backgroundRenderer.sprite; num++;
                ButtonInteraction_Data.time = 0;
                ButtonInteraction_Data.gameobj = UnityEngine.Object.Instantiate(AlterTitleController.Controller._backgroundRenderer.gameObject, AlterTitleController.Controller._backgroundRenderer.transform.parent);
                num++;
                ButtonInteraction_Data.frontsprite = ButtonInteraction_Data.gameobj.GetComponent<SpriteRenderer>(); num++;
                ButtonInteraction_Data.frontsprite.transform.SetAsLastSibling(); num++;
                ButtonInteraction_Data.frontsprite.sprite = null;
                ButtonInteraction_Data.frontsprite.gameObject.SetActive(false);
                ButtonInteraction_Data.fadeprogress = 0f;
            }
            catch (Exception ex)
            {
                Harmony_Patch.YKMTLogInstance.Debug(num.ToString());
                Harmony_Patch.YKMTLogInstance.Error(ex);
            }
        }
        public void Update()
        {
            try
            {
                if (ButtonInteraction_Data.currentState == ButtonState.FadingIn)
                {
                    UnityEngine.Color color = ButtonInteraction_Data.frontsprite.color;
                    color.a = ButtonInteraction_Data.fadeprogress;
                    ButtonInteraction_Data.frontsprite.color = color;
                    ButtonInteraction_Data.fadeprogress += Time.deltaTime;
                    if (ButtonInteraction_Data.fadeprogress >= 1f)
                    {
                        AlterTitleController.Controller._backgroundRenderer.sprite = ButtonInteraction_Data.frontsprite.sprite;
                        AlterTitleController.Controller._backgroundRenderer.gameObject.SetActive(true);
                        ButtonInteraction_Data.frontsprite.gameObject.SetActive(false);
                        ButtonInteraction_Data.frontsprite.sprite = null;
                        ButtonInteraction_Data.fadeprogress = 0f;
                        ButtonInteraction_Data.currentState = ButtonState.Idle;
                    }
                }
                else if (ButtonInteraction_Data.currentState == ButtonState.FadingOut)
                {
                    UnityEngine.Color color = ButtonInteraction_Data.frontsprite.color;
                    color.a = ButtonInteraction_Data.fadeprogress;
                    ButtonInteraction_Data.frontsprite.color = color;
                    ButtonInteraction_Data.fadeprogress -= Time.deltaTime;
                    if (ButtonInteraction_Data.fadeprogress <= 0f)
                    {
                        AlterTitleController.Controller._backgroundRenderer.sprite = ButtonInteraction_Data.originSprite;
                        AlterTitleController.Controller._backgroundRenderer.gameObject.SetActive(true);
                        ButtonInteraction_Data.frontsprite.gameObject.SetActive(false);
                        ButtonInteraction_Data.frontsprite.sprite = null;
                        ButtonInteraction_Data.fadeprogress = 0f;
                        ButtonInteraction_Data.currentState = ButtonState.Idle;
                    }
                }
            }
            catch (Exception ex)
            {
                Harmony_Patch.YKMTLogInstance.Error(ex);
            }
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            try
            {
                //transform.parent.GetChild(1).GetComponent<UnityEngine.UI.Text>().color = Color.white;
                ButtonInteraction_Data.frontsprite.sprite = ButtonInteraction_Data.sp;
                ButtonInteraction_Data.currentState = ButtonState.FadingIn;
                ButtonInteraction_Data.frontsprite.color = new UnityEngine.Color(1f, 1f, 1f, 0f);
                ButtonInteraction_Data.frontsprite.gameObject.SetActive(true);
                ButtonInteraction_Data.fadeprogress = ButtonInteraction_Data.frontsprite.color.a;
            }
            catch (Exception ex)
            {
                Harmony_Patch.YKMTLogInstance.Error(ex);
            }
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            try
            {
                //transform.parent.GetChild(1).GetComponent<UnityEngine.UI.Text>().color = originColor;
                AlterTitleController.Controller._backgroundRenderer.sprite = ButtonInteraction_Data.originSprite;
                ButtonInteraction_Data.currentState = ButtonState.FadingOut;
                ButtonInteraction_Data.fadeprogress = ButtonInteraction_Data.frontsprite.color.a;
                ButtonInteraction_Data.frontsprite.sprite = ButtonInteraction_Data.sp;
                ButtonInteraction_Data.frontsprite.color = new UnityEngine.Color(1f, 1f, 1f, 1f);
                ButtonInteraction_Data.frontsprite.gameObject.SetActive(true);
            }
            catch (Exception ex)
            {
                Harmony_Patch.YKMTLogInstance.Error(ex);
            }
        }
    }

    public class AwardButtonInteraction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public static string path = Path.GetDirectoryName(Uri.UnescapeDataString(new UriBuilder(Assembly.GetExecutingAssembly().CodeBase).Path));

        Sprite originSprite = new Sprite();
        UnityEngine.Color originColor = new UnityEngine.Color();
        float time = 0;
        Sprite sp;
        SpriteRenderer frontsprite;
        GameObject gameobj;

        bool fadeBig;
        bool fadeSmall;
        float fadeProgress;
        Vector3 originalScale = Vector3.one;

        void Start()
        {
            try
            {
                fadeBig = false;
                fadeSmall = false;
                fadeProgress = 0f;

                originalScale = transform.localScale;
            }
            catch (Exception ex)
            {
                Harmony_Patch.YKMTLogInstance.Error(ex);
            }
        }
        public void Update()
        {
            try
            {
                if (fadeBig)
                {
                    fadeProgress += Time.deltaTime;
                    transform.localScale = originalScale * (1 + fadeProgress);
                    if (fadeProgress >= 0.2f)
                    {
                        fadeBig = false;
                        fadeProgress = 0f;
                    }
                }
                else if (fadeSmall)
                {
                    fadeProgress += Time.deltaTime;
                    transform.localScale = originalScale * (1.2f - fadeProgress);
                    if (fadeProgress >= 0.2f)
                    {
                        fadeSmall = false;
                        fadeProgress = 0f;
                    }
                }
            }
            catch (Exception ex)
            {
                Harmony_Patch.YKMTLogInstance.Error(ex);
            }
        }
        public void PlayClip(AudioClipPlayer.PlayerData data)
        {
            if (data.region == AudioRegion.GLOBAL)
            {
                GlobalAudioManager.instance.PlayGlobalClip(data.globalType);
            }
            else
            {
                if (data.localPlayIndex != -1)
                {
                    LocalAudioManager.instance.PlayClip(data.localPlayIndex);
                    return;
                }
                if (data.localName != string.Empty)
                {
                    LocalAudioManager.instance.PlayClip(data.localName);
                    return;
                }
            }
        }
        AudioClipPlayer.PlayerData PointOver = new AudioClipPlayer.PlayerData() { globalType = AudioType.POINTER_OVER, region = AudioRegion.LOCAL, localName = "Over" };
        AudioClipPlayer.PlayerData Click = new AudioClipPlayer.PlayerData() { globalType = AudioType.CLICK, region = AudioRegion.LOCAL, localName = "Click" };
        AudioClipPlayer.PlayerData Cancel = new AudioClipPlayer.PlayerData() { globalType = AudioType.CANCEL, region = AudioRegion.GLOBAL, localPlayIndex = 0 };
        public void OnPointerEnter(PointerEventData eventData)
        {
            try
            {
                fadeBig = true;
                fadeSmall = false;
                //transform.localScale = originalScale * 1.2f;

                fadeProgress = 0f;
            }
            catch (Exception ex)
            {
                Harmony_Patch.YKMTLogInstance.Error(ex);
            }
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            try
            {
                fadeBig = false;
                fadeSmall = true;
                fadeProgress = 0f;
                //transform.localScale = originalScale;
            }
            catch (Exception ex)
            {
                Harmony_Patch.YKMTLogInstance.Error(ex);
            }
        }

    }

    //该组件的作用：鼠标放在按钮上时若pointerEnterSound为true则播放第一个音效，
    //点击时（需手动调用）如果successSound为true则播放第二个音效，failSound是第三个音效，如果shrink为true则使按钮缩放
    public class UniversalButtonIntereaction : MonoBehaviour, IPointerEnterHandler
    {
        public static string path = Path.GetDirectoryName(Uri.UnescapeDataString(new UriBuilder(Assembly.GetExecutingAssembly().CodeBase).Path));

        AudioSource[] audios;

        public bool pointerEnterSound = true;
        void Start()
        {
            try
            {
                audios = transform.gameObject.GetComponents<AudioSource>();
            }
            catch (Exception ex)
            {
                Harmony_Patch.YKMTLogInstance.Error(ex);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            try
            {
                if (pointerEnterSound)
                {
                    audios[0].Play();
                }
            }
            catch (Exception ex)
            {
                Harmony_Patch.YKMTLogInstance.Error(ex);
            }
        }

        public void Click(bool successSound = true, bool failSound = true, bool shrink = true)
        {
            try
            {
                if (pointerEnterSound)
                {
                    if (successSound)
                    {
                        audios[1].Play();
                    }
                    if (failSound)
                    {
                        audios[2].Play();
                    }
                }
                else
                {
                    if (successSound)
                    {
                        audios[0].Play();
                    }
                    if (failSound)
                    {
                        audios[1].Play();
                    }
                }

                if (shrink)
                {
                    transform.DOScale(new Vector3(0.85f, 0.85f, 1f), 0.1f).SetEase(Ease.InOutCirc).SetUpdate(true).OnComplete(() =>
                    {
                        transform.DOScale(new Vector3(1, 1, 1), 0.1f).SetEase(Ease.InOutCirc).SetUpdate(true);
                    });
                }

            }
            catch (Exception ex)
            {
                Harmony_Patch.YKMTLogInstance.Error(ex);
            }
        }
    }

    public class ShowScene : MonoBehaviour
    {
        public static string path = Path.GetDirectoryName(Uri.UnescapeDataString(new UriBuilder(Assembly.GetExecutingAssembly().CodeBase).Path));

        AudioSource[] audios;
        bool pointerEnterSound = true;
        void Awake()
        {
            try
            {
                transform.localScale = new Vector3(0, 0, 1);
                transform.DOScale(new Vector3(1, 1, 1), 1).SetEase(Ease.OutExpo).SetUpdate(true);
            }
            catch (Exception ex)
            {
                Harmony_Patch.YKMTLogInstance.Error(ex);
            }
        }
    }

    public class UpdateWonder : MonoBehaviour
    {
        void Update()
        {
            base.gameObject.GetComponent<Text>().text = LocalizeTextDataModel.instance.GetText("Wonder") + ":" + WonderModel.instance.money;
        }
    }
}
///////////
public class LocalizeTextLoadScriptWithOutFontLoadScript : MonoBehaviour, ILanguageLinkedData, IObserver
{
    public UnityEngine.UI.Text Text
    {
        get
        {
            return transform.gameObject.GetComponent<UnityEngine.UI.Text>();
        }
    }

    private void Start()
    {
        this.SetText();
        if (!this.init)
        {
            this.init = true;

        }
    }

    public void SetText()
    {
        if (this.id == string.Empty)
        {
            return;
        }
        string text = LocalizeTextDataModel.instance.GetText(this.id);
        this.Text.text = text;
    }

    public void SetText(string id)
    {
        this.id = id;
        this.SetText();
    }

    public void SetTextForcely(string text)
    {
        this.Text.text = text;
        this.init = true;
    }

    private void OnEnable()
    {
        Notice.instance.Observe(NoticeName.LanaguageChange, this);
    }

    private void OnDisable()
    {
        Notice.instance.Remove(NoticeName.LanaguageChange, this);
    }

    public void OnLanguageChanged()
    {
        this.SetText();
    }

    public void OnNotice(string notice, params object[] param)
    {
        if (notice == NoticeName.LanaguageChange)
        {
            this.OnLanguageChanged();
        }
    }

    public string id = string.Empty;
    private bool init;
}
///////////