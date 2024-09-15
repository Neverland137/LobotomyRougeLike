using Assets.Scripts.UI.Utils;
using Harmony;
using Steamworks;
using Steamworks.Data;
using Steamworks.Ugc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Policy;
using System.Xml;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.UIElements;
using UnityEngine.UI;
using static CreatureGenerate.CreatureGenerateData;

namespace NewGameMode
{
    public class Harmony_Patch
    {
        public static string path = Path.GetDirectoryName(Uri.UnescapeDataString(new UriBuilder(Assembly.GetExecutingAssembly().CodeBase).Path));
        public static string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/LOG";

        public static GameMode rougeLike = (GameMode)666666;

        public static GameObject newRougeButton = new GameObject();
        public static GameObject continueRougeButton = new GameObject();
        public Harmony_Patch()
        {
            try
            {
                HarmonyInstance harmony = HarmonyInstance.Create("ykmt.NewGameMode");
                File.WriteAllText(path + "/Log.txt", "");
                //存储数据
                harmony.Patch(typeof(GlobalGameManager).GetMethod("SaveGlobalData", AccessTools.all), new HarmonyMethod(typeof(Harmony_Patch).GetMethod("SaveGlobalData")), null, null);
                harmony.Patch(typeof(GlobalGameManager).GetMethod("SaveData", AccessTools.all), new HarmonyMethod(typeof(Harmony_Patch).GetMethod("SaveDayData")), null, null);
                harmony.Patch(typeof(GlobalGameManager).GetMethod("SaveData", AccessTools.all), new HarmonyMethod(typeof(Harmony_Patch).GetMethod("SaveRougeLikeDayData")), null, null);
                //结束这一天时存档
                harmony.Patch(typeof(GameManager).GetMethod("ClearStage", AccessTools.all), null, new HarmonyMethod(typeof(Harmony_Patch).GetMethod("OnClearStage")), null);
                
                //各种UI：主页UI，当天结算界面UI，
                harmony.Patch(typeof(AlterTitleController).GetMethod("Start", AccessTools.all), null, new HarmonyMethod(typeof(Harmony_Patch).GetMethod("NewGameModeButton_Start")), null);
                harmony.Patch(typeof(ResultScreen).GetMethod("OnSuccessManagement", AccessTools.all), null, new HarmonyMethod(typeof(Harmony_Patch).GetMethod("ResultScreen_Board")), null);


                //局内机制修改:禁止重开和回库
                harmony.Patch(typeof(GameManager).GetMethod("RestartGame", AccessTools.all), new HarmonyMethod(typeof(Harmony_Patch).GetMethod("NoRestartAndCheckPoint")), null, null);
                harmony.Patch(typeof(GameManager).GetMethod("ReturnToCheckPoint", AccessTools.all), new HarmonyMethod(typeof(Harmony_Patch).GetMethod("NoRestartAndCheckPoint")), null, null);
                harmony.Patch(typeof(EscapeUI).GetMethod("Start", AccessTools.all), null, new HarmonyMethod(typeof(Harmony_Patch).GetMethod("NoRestartAndCheckPointButton_EscapeUI")), null);
                harmony.Patch(typeof(ResultScreen).GetMethod("Awake", AccessTools.all), null, new HarmonyMethod(typeof(Harmony_Patch).GetMethod("NoRestartButton_ResultScreen")), null);
                //损失全部员工时删档并回到标题页
                harmony.Patch(typeof(Sefira).GetMethod("OnAgentCannotControll", AccessTools.all), null, new HarmonyMethod(typeof(Harmony_Patch).GetMethod("ReturnToTitleOnGameOver")), null);
                //屏蔽剧情
                harmony.Patch(typeof(StoryUI).GetMethod("LoadStory", AccessTools.all),null, new HarmonyMethod(typeof(Harmony_Patch).GetMethod("NoStory")),null);
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
            }
            catch (Exception ex)
            {
                File.WriteAllText(path + "/BaseHpError.txt", ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }
        public static void WorkSpeedBoost(ref float __result)
        {
            __result *= 1.5f;
        }
        public static void NewGameModeButton_Start(AlterTitleController __instance)//记得改按钮文字
        {
            try
            {
                //AssetBundle bundle = AssetBundle.Instantiate()
                //__instance._buttonRoot.transform.Find("ButtonLayout").transform.localPosition += 400 * Vector3.up;

                //以下为开始按钮的写法：复制原按钮加以修改。

                /*
                GameObject new_game_start_button = UnityEngine.Object.Instantiate(__instance._buttonRoot.transform.Find("ButtonLayout").GetChild(1).gameObject, __instance._buttonRoot.transform.Find("ButtonLayout").GetChild(2).gameObject.transform.parent);
                
                new_game_start_button.transform.SetParent(__instance._buttonRoot.transform.Find("ButtonLayout"));

                new_game_start_button.transform.localPosition = __instance._buttonRoot.transform.Find("ButtonLayout").GetChild(1).localPosition;
                new_game_start_button.transform.localScale = __instance._buttonRoot.transform.Find("ButtonLayout").GetChild(1).localScale;
                

                new_game_start_button.transform.localPosition += 400 * Vector3.up;

                UnityEngine.Object.Destroy(new_game_start_button.GetComponent<EventTrigger>());
                new_game_start_button.GetComponent<Button>().onClick.RemoveAllListeners();
                new_game_start_button.GetComponent<Button>().onClick.AddListener(delegate
                {
                    CallNewGame_Rougelike();
                });
                */

                AssetBundle bundle = AssetBundle.LoadFromFile(path + "/AssetsBundle/gamemodebutton");
                newRougeButton = UnityEngine.Object.Instantiate(bundle.LoadAsset<GameObject>("GameModeButton"));
                bundle.Unload(false);//关闭AB包，但是保留已加载的资源
                //感觉不如手搓按钮 by Plana
                //你不准感觉               

                newRougeButton.transform.SetParent(__instance._buttonRoot.transform.Find("ButtonLayout"));
                newRougeButton.transform.localScale = new Vector3(95, 95, 10);
                newRougeButton.transform.localPosition = new Vector3(-435, 330, 0);
                newRougeButton.AddComponent<ButtonInteraction>();

                UnityEngine.UI.Image image = newRougeButton.transform.GetChild(0).GetComponent<UnityEngine.UI.Image>();
                ///////////
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
                    CallNewGame_Rougelike();
                });


                //以下为继续按钮的写法：新建一个全新按钮。
                //在没有存档的情况下，该按钮不能点击。

                /*
                GameObject new_game_continue_button = new GameObject();
                new_game_continue_button.transform.SetParent(__instance._buttonRoot.transform);
                //new_game_continue_button.transform.localPosition -= 1300 * Vector3.up;
                //new_game_continue_button.transform.localPosition += 4100 * Vector3.right;
                new_game_continue_button.transform.localScale *= 1.5f;

                Button button = new_game_continue_button.AddComponent<Button>();
                
                GameObject new_game_continue_button_text = new GameObject();
                new_game_continue_button_text.transform.SetParent(new_game_continue_button.transform,false);
                UnityEngine.UI.Text text = new_game_continue_button_text.AddComponent<UnityEngine.UI.Text>();

                GameObject new_game_continue_button_image = new GameObject();
                new_game_continue_button_image.transform.SetParent(new_game_continue_button.transform, false);
                UnityEngine.UI.Image image = new_game_continue_button_image.AddComponent<UnityEngine.UI.Image>();

                Texture2D texture2 = new Texture2D(2, 1);
                texture2.LoadImage(File.ReadAllBytes(path + "/Sprite/StartButton.png"));

                text.transform.localScale *= 0.1f;
                //text.transform.localPosition = image.rectTransform.localPosition;
                text.text = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
                text.color = Color.white;

                image.sprite = Sprite.Create(texture2, new Rect(0f, 0f, texture2.width, texture2.height), new Vector2(0.5f, 0.5f));
                image.rectTransform.sizeDelta = new Vector2(200, 70);
                image.transform.localScale *= 0.11f;


                button.targetGraphic = image;
                button.onClick.AddListener(delegate
                {
                    CallContinueGame_Rougelike();
                });
                if (!File.Exists(path + "/Save/GlobalData.dat") || !File.Exists(path + "/Save/DayData.dat"))
                {
                    button.interactable = false;
                }
                */

                AssetBundle bundle0 = AssetBundle.LoadFromFile(path + "/AssetsBundle/gamemodebutton");
                continueRougeButton = UnityEngine.Object.Instantiate(bundle0.LoadAsset<GameObject>("GameModeButton"));
                bundle0.Unload(false);//关闭AB包，但是保留已加载的资源
                //感觉不如手搓按钮 by Plana
                //你不准感觉               

                continueRougeButton.transform.SetParent(__instance._buttonRoot.transform.Find("ButtonLayout"));
                continueRougeButton.transform.localScale = new Vector3(95, 95, 10);
                continueRougeButton.transform.localPosition = new Vector3(-435, 200, 0);
                continueRougeButton.AddComponent<ButtonInteraction>();
                
                UnityEngine.UI.Image image0 = continueRougeButton.transform.GetChild(0).GetComponent<UnityEngine.UI.Image>();
                ///////////
                LocalizeTextLoadScriptWithOutFontLoadScript script0 = continueRougeButton.transform.GetChild(1).gameObject.AddComponent<LocalizeTextLoadScriptWithOutFontLoadScript>();
                script0.id = "Rouge_Title_Continue_Button";

                //ContentSizeFitter fitter1 = continueRougeButton.transform.GetChild(0).gameObject.AddComponent<ContentSizeFitter>();
                //fitter1.verticalFit = ContentSizeFitter.FitMode.Unconstrained;//首选大小
                //fitter1.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;//不调整
                ContentSizeFitter fitter20 = continueRougeButton.transform.GetChild(1).gameObject.AddComponent<ContentSizeFitter>();
                fitter2.verticalFit = ContentSizeFitter.FitMode.PreferredSize;//首选大小
                fitter2.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;//不调整
                ContentSizeFitter fitter30 = continueRougeButton.transform.GetChild(2).gameObject.AddComponent<ContentSizeFitter>();
                //fitter3.verticalFit = ContentSizeFitter.FitMode.Unconstrained;//首选大小
                //fitter3.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;//不调整

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

                //text.text = LocalizeTextDataModel.instance.GetText("Rouge_Title_Continue_Button");
                text0.transform.localScale = new Vector3(0.02f, 0.02f, 1);
                //image.color = new Color(1f, 1f, 1f, 0f);
                GameObject.DestroyObject(continueRougeButton.transform.GetChild(2).GetComponent<UnityEngine.UI.Image>());
                GameObject.DestroyObject(continueRougeButton.transform.GetChild(2).GetChild(0));
                //.enabled = false;
                button0.targetGraphic = image0;
                //有办法把按钮的图像设成透明吗
                //我这个按钮好像没法加载image 是白不拉几的一片
                //好好好
                button0.transform.localScale *= 10f;
                continueRougeButton.transform.GetChild(0).transform.localPosition = image0.transform.localPosition + new Vector3(0,0,10);
                continueRougeButton.transform.GetChild(0).transform.localScale = image0.transform.localScale;
                button0.onClick.AddListener(delegate
                {
                    CallContinueGame_Rougelike();
                });
                if (!File.Exists(path + "/Save/GlobalData.dat") || !File.Exists(path + "/Save/DayData.dat"))
                {
                    button0.interactable = false;
                    text0.color = UnityEngine.Color.gray;
                    //button0.enabled = false;
                }
            }
            catch (Exception ex)
            {
                File.WriteAllText(path + "/ButtonStartError.txt", ex.Message + Environment.NewLine + ex.StackTrace);
            }

        }

        
        /*
        public static void NewGameModeButton_Awake()
        {
            //在动作库里添加id为666的动作类型，触发动作时启动CallNewGame_Rougelike
            Extension.GetPrivateField<Dictionary<global::AlterTitleController.TitleActionType, global::AlterTitleController.TitleCall>>(AlterTitleController.Controller, "_actionLibrary").Add((AlterTitleController.TitleActionType)666, new global::AlterTitleController.TitleCall(CallNewGame_Rougelike));
        }

        public static void NewGameModeButton_OnClick(int id)
        {
            //当按钮id超过原版时，才会触发。在动作库里搜索对应的动作，如果搜索到就call方法
            if (id >= 9)
            {
                global::AlterTitleController.TitleCall titleCall = null;
                Dictionary<global::AlterTitleController.TitleActionType, global::AlterTitleController.TitleCall> _actionLibrary = Extension.GetPrivateField<Dictionary<global::AlterTitleController.TitleActionType, global::AlterTitleController.TitleCall>>(AlterTitleController.Controller, "_actionLibrary");

                if (_actionLibrary.TryGetValue((global::AlterTitleController.TitleActionType)666, out titleCall))
                {
                    Debug.Log((global::AlterTitleController.TitleActionType)666);
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
                File.WriteAllText(path + "/ContinueRougeError.txt", message.Message + Environment.NewLine + message.StackTrace);
                Debug.LogError(message);
                global::GlobalGameManager.instance.ReleaseGame();
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
                    Dictionary<string, object> dic = global::SaveUtil.ReadSerializableFile(UnityEngine.Application.persistentDataPath + "/saveGlobal170808.dat");
                    Dictionary<string, object> dic2 = null;
                    Dictionary<string, object> dic3 = null;
                    Dictionary<string, object> dic4 = null;
                    Dictionary<string, object> dic5 = null;
                    Dictionary<string, object> dic6 = null;
                    Dictionary<string, object> dic7 = null;
                    if (global::GameUtil.TryGetValue<Dictionary<string, object>>(dic, "observe", ref dic2))
                    {
                        global::CreatureManager.instance.LoadObserveData(SetRandomObserve(dic2, 0, 0, 20, 50));
                    }
                    else
                    {
                        global::CreatureManager.instance.ResetObserveData();
                    }
                    if (global::GameUtil.TryGetValue<Dictionary<string, object>>(dic, "etcData", ref dic4))
                    {
                        global::GlobalEtcDataModel.instance.LoadGlobalData(dic4);
                    }
                    else
                    {
                        global::GlobalEtcDataModel.instance.ResetGlobalData();
                    }
                    if (global::GameUtil.TryGetValue<Dictionary<string, object>>(dic, "research", ref dic5))
                    {
                        global::GlobalEtcDataModel.instance.LoadGlobalData(dic5);
                        //随机部门科技暂时放弃。原因：部门科技与任务绑定
                        //global::ResearchDataModel.instance.LoadData(SetRandomResearch(dic5, 0.4f));
                    }
                    else
                    {
                        global::ResearchDataModel.instance.Init();
                    }
                    if (global::GameUtil.TryGetValue<Dictionary<string, object>>(dic, "missions", ref dic6))
                    {
                        global::MissionManager.instance.LoadData(dic6);
                    }
                    else
                    {
                        global::MissionManager.instance.Init();
                    }
                    if (global::GameUtil.TryGetValue<Dictionary<string, object>>(dic, "inventory", ref dic3))
                    {
                        //global::MissionManager.instance.LoadData(dic3);
                        
                        float[] rate = { 0.1f, 0.3f, 0.6f, 0.85f, 1 };
                        global::InventoryModel.Instance.LoadGlobalData(SetRandomEquipment(dic3, 40, 50, rate));
                    }
                    else
                    {
                        global::InventoryModel.Instance.Init();
                    }
                    if (global::GameUtil.TryGetValue<Dictionary<string, object>>(dic, "sefiraCharactes", ref dic7))
                    {
                        global::SefiraCharacterManager.instance.LoadData(dic7);
                    }
                    else
                    {
                        global::SefiraCharacterManager.instance.Init();
                    }
                }
            }
            catch (Exception ex)
            {
                File.WriteAllText(path + "/LoadGlobalDataError.txt", ex.Message + Environment.NewLine + ex.StackTrace);
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

            float[] rate = {0.1f, 0.3f, 0.6f, 0.9f, 1f};
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
                    Dictionary<string, object> dictionary = new Dictionary<string, object>();
                    dictionary.Add("observe", global::CreatureManager.instance.GetSaveObserveData());
                    dictionary.Add("etcData", global::GlobalEtcDataModel.instance.GetGlobalSaveData());
                    dictionary.Add("inventory", global::InventoryModel.Instance.GetGlobalSaveData());
                    dictionary.Add("research", global::ResearchDataModel.instance.GetSaveData());
                    dictionary.Add("missions", global::MissionManager.instance.GetSaveData());
                    dictionary.Add("sefiraCharactes", global::SefiraCharacterManager.instance.GetSaveData());
                    global::SaveUtil.WriteSerializableFile(path + "/Save/GlobalData.dat", dictionary);
                    return false;
                }

            }
            catch (Exception ex)
            {
                File.WriteAllText(path + "/SaveGlobalerror.txt", ex.Message + Environment.NewLine + ex.StackTrace);
            }

            return true;
        }

        public static bool SaveDayData()
        {
            try
            {
                if (GlobalGameManager.instance.gameMode == rougeLike)
                {
                    Dictionary<string, object> dictionary = new Dictionary<string, object>();
                    dictionary.Add("saveVer", "ver1");
                    dictionary.Add("playTime", GlobalGameManager.instance.playTime);
                    int day = global::PlayerModel.instance.GetDay();
                    dictionary.Add("lastDay", day);
                    Dictionary<int, Dictionary<string, object>> dictionary2 = new Dictionary<int, Dictionary<string, object>>();
                    Dictionary<string, object> saveDayData = GlobalGameManager.instance.GetSaveDayData();
                    dictionary2.Add(global::PlayerModel.instance.GetDay(), saveDayData);

                    if(File.Exists(path + "/Save/DayData.dat"))
                    {
                        Dictionary<string, object> dic = global::SaveUtil.ReadSerializableFile(path + "/Save/DayData.dat");
                        int num = 0;
                        Dictionary<string, object> value2 = null;
                        Dictionary<int, Dictionary<string, object>> dictionary3 = null;
                        if (global::GameUtil.TryGetValue<int>(dic, "checkPointDay", ref num) && global::GameUtil.TryGetValue<Dictionary<int, Dictionary<string, object>>>(dic, "dayList", ref dictionary3) && dictionary3.TryGetValue(num, out value2))
                        {
                            dictionary.Add("checkPointDay", 10036);
                            dictionary2.Add(num, value2);
                        }
                    }

                    dictionary.Add("dayList", dictionary2);
                    global::SaveUtil.WriteSerializableFile(path + "/Save/DayData.dat", dictionary);
                    return false;
                }
            }
            
            catch (Exception ex)
            {
                File.WriteAllText(path + "/SaveDataerror.txt", ex.Message + Environment.NewLine + ex.StackTrace);
            }
            return true;
        }

        public static void SaveRougeLikeDayData()//存储某天的肉鸽相关内容，例如D35的模因和奇思
        {
            //结构如下：dictionary的key表示存储的内容类型，例如wonder代表奇思，meme代表模因
            //dictionary的value里是具体存储的内容
            Dictionary<string, object> dictionary = new Dictionary<string, object>();

            dictionary.Add("wonder", WonderModel.instance.money);
            dictionary.Add("meme", MemeManager.instance.current_dic);

            SaveUtil.WriteSerializableFile(path + "/Save/RougeLikeDayData.dat", dictionary);
        }

        public static void LoadGlobalData()
        {
            try
            {
                if (!File.Exists(path + "/Save/GlobalData.dat"))
                {
                    global::CreatureManager.instance.ResetObserveData();
                    global::GlobalEtcDataModel.instance.ResetGlobalData();
                    global::ResearchDataModel.instance.Init();
                    global::InventoryModel.Instance.Init();
                    global::MissionManager.instance.Init();
                    global::SefiraCharacterManager.instance.Init();
                }
                else
                {
                    Dictionary<string, object> dic = global::SaveUtil.ReadSerializableFile(path + "/Save/GlobalData.dat");
                    Dictionary<string, object> dic2 = null;
                    Dictionary<string, object> dic3 = null;
                    Dictionary<string, object> dic4 = null;
                    Dictionary<string, object> dic5 = null;
                    Dictionary<string, object> dic6 = null;
                    Dictionary<string, object> dic7 = null;
                    if (global::GameUtil.TryGetValue<Dictionary<string, object>>(dic, "observe", ref dic2))
                    {
                        global::CreatureManager.instance.LoadObserveData(dic2);
                    }
                    else
                    {
                        global::CreatureManager.instance.ResetObserveData();
                    }
                    if (global::GameUtil.TryGetValue<Dictionary<string, object>>(dic, "etcData", ref dic4))
                    {
                        global::GlobalEtcDataModel.instance.LoadGlobalData(dic4);
                    }
                    else
                    {
                        global::GlobalEtcDataModel.instance.ResetGlobalData();
                    }
                    if (global::GameUtil.TryGetValue<Dictionary<string, object>>(dic, "research", ref dic5))
                    {
                        global::ResearchDataModel.instance.LoadData(dic5);
                    }
                    else
                    {
                        global::ResearchDataModel.instance.Init();
                    }
                    if (global::GameUtil.TryGetValue<Dictionary<string, object>>(dic, "missions", ref dic6))
                    {
                        global::MissionManager.instance.LoadData(dic6);
                    }
                    else
                    {
                        global::MissionManager.instance.Init();
                    }
                    if (global::GameUtil.TryGetValue<Dictionary<string, object>>(dic, "inventory", ref dic3))
                    {
                        global::InventoryModel.Instance.LoadGlobalData(dic3);
                    }
                    else
                    {
                        global::InventoryModel.Instance.Init();
                    }
                    if (global::GameUtil.TryGetValue<Dictionary<string, object>>(dic, "sefiraCharactes", ref dic7))
                    {
                        global::SefiraCharacterManager.instance.LoadData(dic7);
                    }
                    else
                    {
                        global::SefiraCharacterManager.instance.Init();
                    }
                }
            }
            catch (Exception ex)
            {
                File.WriteAllText(path + "/LoadGlobalerror.txt", ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }
        //?
        public static void LoadDayData(global::SaveType saveType)//加载原版天数存档
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
                    global::GameUtil.TryGetValue<string>(dic, "saveVer", ref text);
                    global::GameUtil.TryGetValue<float>(dic, "playTime", ref GlobalGameManager.instance.playTime);
                    int key = 0;
                    int key2 = 0;
                    Dictionary<int, Dictionary<string, object>> dictionary = null;
                    global::GameUtil.TryGetValue<Dictionary<int, Dictionary<string, object>>>(dic, "dayList", ref dictionary);
                    global::GameUtil.TryGetValue<int>(dic, "checkPointDay", ref key2);
                    global::GameUtil.TryGetValue<int>(dic, "lastDay", ref key);
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
                        if (saveType == global::SaveType.LASTDAY)
                        {
                            Extension.CallPrivateMethod<object>(GlobalGameManager.instance, "LoadDay", new object[] { data });
                            //this.LoadDay(data);
                            if (!global::GlobalGameManager.instance.dlcCreatureOn)
                            {
                                bool flag2 = global::CreatureManager.instance.ReplaceAllDlcCreature();
                                flag2 = (global::InventoryModel.Instance.RemoveAllDlcEquipment() || flag2);
                                flag2 = (global::AgentManager.instance.RemoveAllDlcEquipment() || flag2);
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
                            if (saveType != global::SaveType.CHECK_POINT)
                            {
                                throw new Exception("invalid SaveType");
                            }
                            Extension.CallPrivateMethod<object>(GlobalGameManager.instance, "LoadDay", new object[] { data2 });
                            //this.LoadDay(data2);
                            if (global::GlobalGameManager.instance.dlcCreatureOn)
                            {
                                SaveDayData();
                            }
                            else
                            {
                                bool flag3 = global::CreatureManager.instance.ReplaceAllDlcCreature();
                                flag3 = (global::InventoryModel.Instance.RemoveAllDlcEquipment() || flag3);
                                flag3 = (global::AgentManager.instance.RemoveAllDlcEquipment() || flag3);
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
                File.WriteAllText(path + "/LoadDayDataError.txt", ex.Message + Environment.NewLine + ex.StackTrace);
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
                global::GameUtil.TryGetValue<string>(data, "saveInnerVer", ref text);
                File.WriteAllText(path + "/LoadDayError1.txt", "1");
                //int dayFromSaveData = GlobalGameManager.instance.GetDayFromSaveData(data);
                global::GameUtil.TryGetValue<Dictionary<string, object>>(data, "money", ref dic);
                File.WriteAllText(path + "/LoadDayError1.txt", "2");
                global::GameUtil.TryGetValue<Dictionary<string, object>>(data, "agents", ref dic2);
                File.WriteAllText(path + "/LoadDayError1.txt", "3");
                global::GameUtil.TryGetValue<Dictionary<string, object>>(data, "creatures", ref dic3);
                File.WriteAllText(path + "/LoadDayError1.txt", "4");
                global::GameUtil.TryGetValue<Dictionary<string, object>>(data, "playerData", ref dic4);
                File.WriteAllText(path + "/LoadDayError1.txt", "5");
                global::GameUtil.TryGetValue<Dictionary<string, object>>(data, "sefiras", ref dic5);
                File.WriteAllText(path + "/LoadDayError1.txt", "6");
                global::GameUtil.TryGetValue<string>(data, "saveState", ref GlobalGameManager.instance.saveState);
                File.WriteAllText(path + "/LoadDayError1.txt", "7");
                if (global::GameUtil.TryGetValue<Dictionary<string, object>>(data, "agentName", ref dic6))
                {
                    global::AgentNameList.instance.LoadData(dic6);
                }
                global::MoneyModel.instance.LoadData(dic);
                File.WriteAllText(path + "/LoadDayError1.txt", "8");
                global::PlayerModel.instance.LoadData(dic4);
                File.WriteAllText(path + "/LoadDayError1.txt", "9");
                global::SefiraManager.instance.LoadData(dic5);
                File.WriteAllText(path + "/LoadDayError1.txt", "10");
                //global::AgentManager.instance.LoadCustomAgentData();
                File.WriteAllText(path + "/LoadDayError1.txt", "11");
                global::CreatureManager.instance.LoadData(dic3);
                File.WriteAllText(path + "/LoadDayError1.txt", "12");
                //global::AgentManager.instance.LoadDelAgentData();
                File.WriteAllText(path + "/LoadDayError1.txt", "13");
                global::AgentManager.instance.LoadData(dic2);
                File.WriteAllText(path + "/LoadDayError1.txt", "14");
                GlobalGameManager.instance.gameMode = rougeLike;
            }
            catch (Exception ex)
            {
                File.WriteAllText(path + "/LoadDayError.txt", ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }


        public static void OnClearStage()//只有在开始下一天的时候才会触发该函数
        {
            if (global::GlobalGameManager.instance.gameMode == rougeLike)
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
            Dictionary<string, object> result = new Dictionary<string, object>();

            List<Dictionary<string, object>> research_list = new List<Dictionary<string, object>>();
            global::GameUtil.TryGetValue<List<Dictionary<string, object>>>(origin_research, "research", ref research_list);
            int research_count = research_list.Count;
            research_list.Clear();

            List<int> research_id_list = new List<int>() { 1, 2, 103, 3, 4, 5, 6, 7, 203, 8, 9, 10, 501, 502, 701, 702, 703, 801, 802, 803, 901, 902, 903, 1001, 1002, 1003 };
            //你加10000干嘛 给你删了
            for (int i = 0; i < research_id_list.Count; i++)
            {
                Dictionary<string, object> dic = new Dictionary<string, object>();
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
        public static Dictionary<string, object> SetRandomObserve(Dictionary<string, object> observe,int stat_min,int stat_max,int cube_min,int cube_max)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();

            Dictionary<long, Dictionary<string, object>> all_observe_dic_with_id = new Dictionary<long, Dictionary<string, object>>();//观察列表
            Dictionary<string, Dictionary<long, Dictionary<string, object>>> dictionary2 = new Dictionary<string, Dictionary<long, Dictionary<string, object>>>();//模组观察列表
            
            //观察信息分成observeProgress,cubeNum,stat,defense,work,care。

            global::GameUtil.TryGetValue<Dictionary<long, Dictionary<string, object>>>(observe, "observeList", ref all_observe_dic_with_id);
            global::GameUtil.TryGetValue<Dictionary<string, Dictionary<long, Dictionary<string, object>>>>(observe, "observeListMod", ref dictionary2);


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
                Dictionary<string, object> single_observe_dic = new Dictionary<string, object>();//单个异想体的具体观察信息
                single_observe_dic.Add("stat", false);
                single_observe_dic.Add("defense", false);
                single_observe_dic.Add("work_r", false);
                single_observe_dic.Add("work_w", false);
                single_observe_dic.Add("work_b", false);
                single_observe_dic.Add("work_p", false);
                single_observe_dic.Add("care_0", false);
                single_observe_dic.Add("care_1", false);
                single_observe_dic.Add("care_2", false);
                single_observe_dic.Add("care_3", false);
                single_observe_dic.Add("care_4", false);
                single_observe_dic.Add("care_5", false);
                single_observe_dic.Add("care_6", false);

                List<string> stat_list = new List<string>();
                //stat_list.Add("stat");
                stat_list.Add("defense");
                stat_list.Add("work_r");
                stat_list.Add("work_w");
                stat_list.Add("work_b");
                stat_list.Add("work_p");
                stat_list.Add("care_0");
                stat_list.Add("care_1");
                stat_list.Add("care_2");
                stat_list.Add("care_3");
                stat_list.Add("care_4");
                stat_list.Add("care_5");
                stat_list.Add("care_6");

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
            Dictionary<string, object> result = new Dictionary<string, object>();
            try
            {
                List<global::InventoryModel.EquipmentSaveData> result_list = new List<global::InventoryModel.EquipmentSaveData>();
                List<string> result_mod_list = new List<string>();//没用，占位符
                //global::GameUtil.TryGetValue<List<global::InventoryModel.EquipmentSaveData>>(equipment, "equips", ref result_list);
                //global::GameUtil.TryGetValue<List<string>>(equipment, "equipsMod", ref result_mod_list);
                long new_instance_id = 10000;//存储实例id
                result_list.Clear();//随后清除原列表

                int equip_num = UnityEngine.Random.Range(equip_min, equip_max);

                List<int> equip_id = GetAllEquipmentidList();
                //移除失乐园和薄瞑
                equip_id.Remove(200015);
                equip_id.Remove(300015);
                equip_id.Remove(200038);
                equip_id.Remove(300038);

                List<int> z_equip_id = new List<int>();
                List<int> t_equip_id = new List<int>();
                List<int> h_equip_id = new List<int>();
                List<int> w_equip_id = new List<int>();
                List<int> a_equip_id = new List<int>();

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
                File.WriteAllText(path + "/RandomEquipError2.txt", ex.Message + Environment.NewLine + ex.StackTrace);
            }


            return result;
        }

        public static List<int> GetAllEquipmentidList()
        {
            List<int> list = new List<int>();
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
                int agent_count = UnityEngine.Random.Range(count_min, count_max + 1);
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
                File.WriteAllText(path + "/RandomAgentError.txt", ex.Message + Environment.NewLine + ex.StackTrace);
            }

            //AgentManager.instance.GetSaveData();

        }

        public static void SetRandomCreatures(float[] rate)
        {
            //File.WriteAllText(path + "/RandomCreatureError1.txt","1");
            CreatureManager.instance.Clear();
            //File.WriteAllText(path + "/RandomCreatureError1.txt", "2");
            try
            {
                bool dlcCreatureOn = GlobalGameManager.instance.dlcCreatureOn;//获取所有可用的异想体id列表，但不包含工具型
                //File.WriteAllText(path + "/RandomCreatureError1.txt", "3");
                List<long> all_creature_list;
                if (dlcCreatureOn)
                {
                    all_creature_list = new List<long>(CreatureGenerateInfo.all);
                }
                else
                {
                    all_creature_list = new List<long>(CreatureGenerateInfo.all_except_creditCreatures);
                }
                //File.WriteAllText(path + "/RandomCreatureError1.txt", "4");
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
                
                //File.WriteAllText(path + "/RandomCreatureError1.txt", "5");

                List<long> z_creature_list = new List<long>();
                List<long> t_creature_list = new List<long>();
                List<long> h_creature_list = new List<long>();
                List<long> w_creature_list = new List<long>();
                List<long> a_creature_list = new List<long>();

                foreach (long id in all_creature_list)
                {
                    if (CreatureTypeList.instance.GetData(id).GetRiskLevel() == RiskLevel.ZAYIN)
                    {
                        z_creature_list.Add(id);
                    }
                    else if(CreatureTypeList.instance.GetData(id).GetRiskLevel() == RiskLevel.TETH)
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
                //File.WriteAllText(path + "/RandomCreatureError1.txt", "6");
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
                        AddCreature(random_id[i],sefira);
                    }
                }
                //File.WriteAllText(path + "/RandomCreatureError1.txt", "7");

            }
            catch (Exception ex)
            {
                File.WriteAllText(path + "/RandomCreatureError.txt", ex.Message + Environment.NewLine + ex.StackTrace);
            }
            //CreatureManager.instance.AddCreatureInSefira()
        }
        
        public static void AddCreature(long id, global::Sefira sefira)
        {
            List<long> list2 = new List<long>(global::CreatureGenerateInfo.GetAll(false));
            foreach (global::CreatureModel creatureModel in global::CreatureManager.instance.GetCreatureList())
            {
                list2.Remove(creatureModel.metadataId);
            }

            long[] ary = {id};
            global::SefiraIsolate[] array = sefira.isolateManagement.GenIsolateByCreatureAryByOrder(ary);
            foreach (global::SefiraIsolate sefiraIsolate in array)
            {
                global::CreatureManager.instance.AddCreature(sefiraIsolate.creatureId, sefiraIsolate, sefira.indexString);
            }
        }

        public static bool NoRestartAndCheckPoint()
        {
            if (GlobalGameManager.instance.gameMode == rougeLike)
            {
                return false;
            }
            return true;
        }

        public static void NoRestartAndCheckPointButton_EscapeUI()
        {
            if (GlobalGameManager.instance.gameMode == rougeLike)
            {
                GameObject.Destroy(EscapeUI.instance.MiddleAreaControl.transform.GetChild(0).gameObject);
                GameObject.Destroy(EscapeUI.instance.MiddleAreaControl.transform.GetChild(1).gameObject);
                //EscapeUI.instance.MiddleAreaControl.transform.GetChild(0).gameObject.SetActive(false);
                //EscapeUI.instance.MiddleAreaControl.transform.GetChild(1).gameObject.SetActive(false);
            }
        }

        public static void NoRestartButton_ResultScreen()
        {
            if (GlobalGameManager.instance.gameMode == rougeLike)
            {
                //GameObject.Destroy(ResultScreen.instance.root.transform.GetChild(0).GetChild(4).GetChild(0).GetChild(0).gameObject);
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
                Dictionary<long, global::CreatureObserveInfoModel> dic = Extension.GetPrivateField<Dictionary<long, global::CreatureObserveInfoModel>>(CreatureManager.instance, "observeInfoList");
                foreach (KeyValuePair<long, global::CreatureObserveInfoModel> kvp in dic)//遍历所有观察信息
                {
                    CreatureObserveInfoModel creatureObserveInfoModel = kvp.Value;
                    if (creatureObserveInfoModel.GetObservationLevel() == 4)//如果观察等级达到4
                    {
                        CreatureTypeInfo creatureTypeInfo = CreatureTypeList.instance.GetData(creatureObserveInfoModel.creatureTypeId);
                        if (creatureTypeInfo.GetRiskLevel() == RiskLevel.WAW)//如果是W
                        {
                            UnitStatBuf unitStatBuf = new UnitStatBuf(float.MaxValue);
                            unitStatBuf.duplicateType = BufDuplicateType.UNLIMIT;
                            unitStatBuf.maxHp = 10;
                            unitStatBuf.maxMental = 10;
                            unitStatBuf.workProb = 10;
                            unitStatBuf.cubeSpeed = 10;
                            unitStatBuf.movementSpeed = 10;
                            unitStatBuf.attackSpeed = 10;

                            __instance.AddUnitBuf(unitStatBuf);
                        }
                        else if (creatureTypeInfo.GetRiskLevel() == RiskLevel.ALEPH)//如果是A
                        {
                            UnitStatBuf unitStatBuf = new UnitStatBuf(float.MaxValue);
                            unitStatBuf.duplicateType = BufDuplicateType.UNLIMIT;
                            unitStatBuf.maxHp = 20;
                            unitStatBuf.maxMental = 20;
                            unitStatBuf.workProb = 20;
                            unitStatBuf.cubeSpeed = 20;
                            unitStatBuf.movementSpeed = 20;
                            unitStatBuf.attackSpeed = 20;

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
            if (global::SefiraManager.instance.GameOverCheck())
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
                        LoadDayData(global::SaveType.LASTDAY);
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
            text.transform.localScale = Vector3.one*100;
        }
    }

    public class Challenge_Patch
    {
        public static GameMode rougeLike = (GameMode)666666;
        public Challenge_Patch(HarmonyInstance instance)
        {
            instance.Patch(typeof(DeployUI).GetMethod("Init"), null, new HarmonyMethod(typeof(Challenge_Patch).GetMethod("DeployUI_Init")));
            instance.Patch(typeof(GameManager).GetMethod("StartGame", AccessTools.all), null, new HarmonyMethod(typeof(Challenge_Patch).GetMethod("CallRandomChallenge")), null);
        }
        public static void DeployUI_Init()
        {
            if(GlobalGameManager.instance.gameMode==rougeLike)
            {
                if (PlayerModel.instance.GetDay() + 1 == 40)
                {
                    SefiraBossUI.Instance.OnEnterSefiraBossSession();
                }
            }
        }
        public static void CallRandomChallenge()
        {
            if (GlobalGameManager.instance.gameMode == rougeLike)
            {
                if (PlayerModel.instance.GetDay()+1== 40)
                {
                    ///////////
                    List<SefiraEnum> challenges= new List<SefiraEnum>(new SefiraEnum[] { SefiraEnum.GEBURAH, SefiraEnum.BINAH });
                    foreach (Assembly assembly in Add_On.instance.AssemList)
                    {
                        if(assembly.Location.ToLower().Contains("bluearchive"))
                        {
                            challenges.Add((SefiraEnum)1782);
                        }
                    }
                    SefiraBossManager.Instance.SetActivatedBoss(challenges[UnityEngine.Random.Range(0, challenges.Count)]);
                    ///////////
                    SefiraBossManager.Instance.OnStageStart();
                }
            }
        }
    }

    public class EnergyAndOverload_Patch
    {
        public static GameMode rougeLike = (GameMode)666666;

        public static List<AgentModel> panicAgentList = new List<AgentModel>();
        public static List<long> creatureList = new List<long>();

        public static GameObject rewardButton = new GameObject();
        public static GameObject rewardButton1 = new GameObject();
        public static GameObject rewardButton2 = new GameObject();
        public static GameObject rewardButton3 = new GameObject();
        public static List<GameObject> buttonList = new List<GameObject>() { rewardButton1, rewardButton2, rewardButton3 };
        public EnergyAndOverload_Patch(HarmonyInstance instance)
        {
            int num = 0;
            try
            {
                instance.Patch(typeof(StageTypeInfo).GetMethod("GetEnergyNeed"), null, new HarmonyMethod(typeof(EnergyAndOverload_Patch).GetMethod("EnergyNeedDecrease")), null);
                num++;

                //后面那三个null啥意思 好谢了 我build一下 好好

                //第三个和第五个不知道什么意思，第四个是参数 没有就null，空Type数组也行，你等会我去隔壁偷个随机考验
                instance.Patch(typeof(CreatureOverloadManager).GetMethod("ActivateOverload", BindingFlags.NonPublic | BindingFlags.Instance), new HarmonyMethod(typeof(EnergyAndOverload_Patch).GetMethod("SetOverloadMultiply")), null, null);

                instance.Patch(typeof(CreatureOverloadManager).GetMethod("ActivateOverload", BindingFlags.NonPublic | BindingFlags.Instance), null, new HarmonyMethod(typeof(EnergyAndOverload_Patch).GetMethod("CallRandomEvent")), null);
                num++;
                instance.Patch(typeof(CreatureOverloadManager).GetMethod("OnStageStart", AccessTools.all), null, new HarmonyMethod(typeof(EnergyAndOverload_Patch).GetMethod("SetQliphothOverloadMax")), null);
                num++;
                instance.Patch(typeof(OrdealGenInfo).GetMethod("GenerateOrdeals", AccessTools.all), null, new HarmonyMethod(typeof(EnergyAndOverload_Patch).GetMethod("BlockOriginOrdeal")), null);
                num++;
                instance.Patch(typeof(OrdealManager).GetMethod("InitAvailableFixers"), null, new HarmonyMethod(typeof(EnergyAndOverload_Patch).GetMethod("InitFixers")));

                instance.Patch(typeof(AgentModel).GetMethod("OnStageEnd"), null, new HarmonyMethod(typeof(EnergyAndOverload_Patch).GetMethod("CheckPanicAgentAlive")));

                instance.Patch(typeof(EventCreatureModel).GetMethod("OnFixedUpdate"), new HarmonyMethod(typeof(EnergyAndOverload_Patch).GetMethod("RandomCreatureOnFixedUpdate")), null);
                instance.Patch(typeof(CreatureModel).GetMethod("Suppressed"), null, new HarmonyMethod(typeof(EnergyAndOverload_Patch).GetMethod("CheckCreatureSuppress")));
            }
            catch (Exception ex)
            {
                File.WriteAllText(Harmony_Patch.path + "/EAOError"+num.ToString()+".txt",ex.Message+Environment.NewLine+ex.StackTrace);
            }
        }
        ///////////
        public static void BlockOriginOrdeal(ref List<OrdealBase> __result)
        {
            if (GlobalGameManager.instance.gameMode == rougeLike)
            {
                __result = new List<OrdealBase>();
            }
        }
        public static void InitFixers()
        {
            if (GlobalGameManager.instance.gameMode == rougeLike)
            {
                OrdealManager.instance.availableFixers.Add(RwbpType.P);
            }
        }
        ///////////
        public static void EnergyNeedDecrease(ref float __result)
        {
            if (GlobalGameManager.instance.gameMode == rougeLike)
            {
                __result *= 0.25f;
            }
        }

        public static void SetQliphothOverloadMax()
        {
            if (GlobalGameManager.instance.gameMode == rougeLike)
            {
                CreatureOverloadManager.instance.SetPrivateField("_qliphothOverloadMax",4);
            }
        }

        public static void SetOverloadMultiply()
        {
            try
            {
                if (GlobalGameManager.instance.gameMode == rougeLike)
                {
                    int num = CreatureOverloadManager.instance.GetPrivateField<int>("qliphothOverloadIsolateNum");
                    if (overloadlevel <= 2)
                    {
                        num=Mathf.RoundToInt(num * 0.3f);
                    }
                    else if (overloadlevel == 3)
                    {
                        num= 0;
                    }
                    else if (overloadlevel == 4)
                    {
                        num=Mathf.RoundToInt(num * 0.7f);
                    }
                    else if (overloadlevel == 5)
                    {
                    }
                    else if (overloadlevel >= 6)
                    {
                        num=Mathf.RoundToInt(num * 1.3f);
                    }
                    CreatureOverloadManager.instance.SetPrivateField("qliphothOverloadIsolateNum", num);
                }
            }
            catch (Exception ex)
            {
                File.WriteAllText(Harmony_Patch.path + "/OverloadMultiplyError.txt", ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }
        ///////////
        static int overloadlevel
        {
            get
            {
                return CreatureOverloadManager.instance.GetQliphothOverloadLevel() - 1;
            }
        }
        ///////////


        public static void CallRandomEvent()
        {
            try
            {
                if (GlobalGameManager.instance.gameMode == rougeLike)
                {
                    ///////////
                    int day = PlayerModel.instance.GetDay() + 1;
                    float random = UnityEngine.Random.value;
                    float random2 = UnityEngine.Random.value;
                    float mission_level_rate = 0.7f;

                    if (overloadlevel == 4)//生成考验
                    {
                        if (day <= 32)
                        {
                            new MachineNoonOrdeal().OnOrdealStart();
                        }
                        else if (day <= 34)
                        {
                            new MachineNoonOrdeal().OnOrdealStart();
                            new OutterGodNoonOrdeal().OnOrdealStart();
                        }
                        else if (day <= 36)
                        {
                            new MachineDuskOrdeal().OnOrdealStart();
                        }
                        else if (day <= 38)
                        {
                            new FixerOrdeal(OrdealLevel.NOON).OnOrdealStart();
                            OrdealManager.instance.InitAvailableFixers();
                        }
                        else
                        {
                            new FixerOrdeal(OrdealLevel.DUSK).OnOrdealStart();
                            OrdealManager.instance.InitAvailableFixers();
                        }
                        ///////////
                        //get level不是当前等级吗 还是说当前等级+1
                        //但是4级也没生效啊
                        //argument cant be null那个报错是咋回事
                        //原方法是null？没获取到原方法？
                    }
                    else if (random <= 1)
                    {
                        CreateRandomCreature();
                        float[] rate = { 0.5f, 0.8f, 1 };
                        //CreatePanicAgent(90, 110, rate);
                    }
                    else if (random <= 0.3f)
                    {
                        Award_GetLOB(2, 4);
                    }
                    else if (random <= 0.4f)
                    {
                        Award_GetAgent(50, 70);
                    }
                    else//60%概率生成各种任务，每种任务占15%
                    {
                        if (random <= 0.55f)//解锁图鉴
                        {
                            if (random2 <= mission_level_rate)
                            {
                                //写不太动 我先歇歇
                                RGRandomEventManager.StartMission(RGRandomEventManager.MissionRequire.OBSERVE, UnityEngine.Random.Range(4, 6), 1);
                            }
                            else
                            {
                                RGRandomEventManager.StartMission(RGRandomEventManager.MissionRequire.OBSERVE, UnityEngine.Random.Range(8, 11), 2);
                            }
                        }
                        else if (random <= 0.7f)//镇压异想体
                        {
                            if (random2 <= mission_level_rate)//一级任务
                            {
                                if (UnityEngine.Random.value <= 0.5f)//一半概率镇压H
                                {
                                    RGRandomEventManager.StartMission(RGRandomEventManager.MissionRequire.SUPPRESS, UnityEngine.Random.Range(3, 5), 1, 3);
                                }
                                else//一半概率镇压W
                                {
                                    RGRandomEventManager.StartMission(RGRandomEventManager.MissionRequire.SUPPRESS, UnityEngine.Random.Range(1, 3), 1, 4);
                                }
                            }
                            else//二级任务
                            {
                                if (UnityEngine.Random.value <= 0.5f)//一半概率镇压W
                                {
                                    RGRandomEventManager.StartMission(RGRandomEventManager.MissionRequire.SUPPRESS, UnityEngine.Random.Range(2, 4), 2, 4);
                                }
                                else//一半概率镇压A
                                {
                                    RGRandomEventManager.StartMission(RGRandomEventManager.MissionRequire.SUPPRESS, UnityEngine.Random.Range(1, 3), 2, 5);
                                }
                            }
                        }
                        else//融毁等级超过5级时，将“收集能源”和“进行工作”替换为随机异想体
                        {
                            
                            if (overloadlevel >= 5)
                            {
                                CreateRandomCreature();
                                return;
                            }
                            
                            if (random <= 0.85f)//收集能源
                            {
                                if (random2 <= mission_level_rate)
                                {
                                    RGRandomEventManager.StartMission(RGRandomEventManager.MissionRequire.ENERGY, UnityEngine.Random.Range(20, 51), 1);
                                }
                                else
                                {
                                    RGRandomEventManager.StartMission(RGRandomEventManager.MissionRequire.ENERGY, UnityEngine.Random.Range(40, 81), 2);
                                }
                            }
                            else//进行工作
                            {
                                if (random2 <= mission_level_rate)
                                {
                                    RGRandomEventManager.StartMission(RGRandomEventManager.MissionRequire.WORK, UnityEngine.Random.Range(3, 5), 1);
                                }
                                else
                                {
                                    RGRandomEventManager.StartMission(RGRandomEventManager.MissionRequire.WORK, UnityEngine.Random.Range(7, 10), 2);
                                }
                            }
                        }

                        string text = LocalizeTextDataModel.instance.GetText("RandomEvent_MissionStart");
                        AngelaConversationUI.instance.AddAngelaMessage(text);
                    }

                }
            }
            catch (Exception ex)
            {
                File.WriteAllText(Harmony_Patch.path + "/CallRandomEventError.txt", ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        public static void CreatePanicAgent(int stat_min, int stat_max, float[] rate)
        {
            try
            {
                //基础属性
                AgentModel agentModel = AgentManager.instance.AddSpareAgentModel();
                agentModel.primaryStat.hp = UnityEngine.Random.Range(stat_min, stat_max);
                agentModel.primaryStat.mental = UnityEngine.Random.Range(stat_min, stat_max);
                agentModel.primaryStat.work = UnityEngine.Random.Range(stat_min, stat_max);
                agentModel.primaryStat.battle = UnityEngine.Random.Range(stat_min, stat_max);

                //部门
                //string[] sefiraName = { "Malkut", "Yesod", "Hod", "Netzach", "Tiphereth1" };
                Sefira[] sefiras = PlayerModel.instance.GetOpenedAreaList();
                agentModel.SetCurrentSefira(sefiras[UnityEngine.Random.Range(0, PlayerModel.instance.GetOpenedAreaCount())].name);

                agentModel.SetCurrentNode(agentModel.GetCurrentSefira().GetDepartNodeByRandom(0));

                //装备
                List<int> equip_id = Harmony_Patch.GetAllEquipmentidList();
                //移除失乐园和薄瞑
                equip_id.Remove(200015);
                equip_id.Remove(300015);
                equip_id.Remove(200038);
                equip_id.Remove(300038);


                List<int> h_weapon_id = new List<int>();
                List<int> w_weapon_id = new List<int>();
                List<int> a_weapon_id = new List<int>();
                List<int> h_armor_id = new List<int>();
                List<int> w_armor_id = new List<int>();
                List<int> a_armor_id = new List<int>();

                foreach (int id in equip_id)//装备id分级
                {
                    if (EquipmentTypeList.instance.GetData(id) == null)
                    {
                        continue;
                    }
                    else if (EquipmentTypeList.instance.GetData(id).grade == "3")
                    {
                        if (InventoryModel.Instance.CheckEquipmentCount(id))//如果装备未超出自身上限
                        {
                            if (EquipmentTypeList.instance.GetData(id).type == EquipmentTypeInfo.EquipmentType.WEAPON)
                            {
                                h_weapon_id.Add(id);
                            }
                            else if (EquipmentTypeList.instance.GetData(id).type == EquipmentTypeInfo.EquipmentType.ARMOR)
                            {
                                h_armor_id.Add(id);
                            }
                        }
                    }
                    else if (EquipmentTypeList.instance.GetData(id).grade == "4")
                    {
                        if (EquipmentTypeList.instance.GetData(id).type == EquipmentTypeInfo.EquipmentType.WEAPON)
                        {
                            w_weapon_id.Add(id);
                        }
                        else if (EquipmentTypeList.instance.GetData(id).type == EquipmentTypeInfo.EquipmentType.ARMOR)
                        {
                            w_armor_id.Add(id);
                        }
                    }
                    else if (EquipmentTypeList.instance.GetData(id).grade == "5")
                    {
                        if (EquipmentTypeList.instance.GetData(id).type == EquipmentTypeInfo.EquipmentType.WEAPON)
                        {
                            a_weapon_id.Add(id);
                        }
                        else if (EquipmentTypeList.instance.GetData(id).type == EquipmentTypeInfo.EquipmentType.ARMOR)
                        {
                            a_armor_id.Add(id);
                        }
                    }
                }

                float random = UnityEngine.Random.value;
                if (random <= rate[0] && h_weapon_id.Count != 0)
                {
                    EquipmentModel weapon = InventoryModel.Instance.CreateEquipment(h_weapon_id[UnityEngine.Random.Range(0, h_weapon_id.Count)]);
                    agentModel.SetWeapon(weapon as WeaponModel);
                }
                else if (random <= rate[1] && w_weapon_id.Count != 0)
                {
                    EquipmentModel weapon = InventoryModel.Instance.CreateEquipment(w_weapon_id[UnityEngine.Random.Range(0, w_weapon_id.Count)]);
                    agentModel.SetWeapon(weapon as WeaponModel);
                }
                else if (random <= rate[2] && a_weapon_id.Count != 0)
                {
                    EquipmentModel weapon = InventoryModel.Instance.CreateEquipment(a_weapon_id[UnityEngine.Random.Range(0, a_weapon_id.Count)]);
                    agentModel.SetWeapon(weapon as WeaponModel);
                }

                random = UnityEngine.Random.value;
                if (random <= rate[0] && h_armor_id.Count != 0 && agentModel.Equipment.weapon.metaInfo.grade != "3")//只有在武器不是H的情况下才可能刷到H
                {
                    EquipmentModel armor = InventoryModel.Instance.CreateEquipment(h_armor_id[UnityEngine.Random.Range(0, h_armor_id.Count)]);
                    agentModel.SetArmor(armor as ArmorModel);
                }
                else if (random <= rate[1] && w_armor_id.Count != 0)
                {
                    EquipmentModel armor = InventoryModel.Instance.CreateEquipment(w_armor_id[UnityEngine.Random.Range(0, w_armor_id.Count)]);
                    agentModel.SetArmor(armor as ArmorModel);
                }
                else if (random <= rate[2] && a_armor_id.Count != 0)
                {
                    EquipmentModel armor = InventoryModel.Instance.CreateEquipment(a_armor_id[UnityEngine.Random.Range(0, a_armor_id.Count)]);
                    agentModel.SetArmor(armor as ArmorModel);
                }

                if (agentModel.Equipment.weapon.metaInfo.damageInfo.type == RwbpType.P)
                {
                    agentModel.ForcelyPanic(RwbpType.R);
                }
                else
                {
                    agentModel.ForcelyPanic(RwbpType.P);
                }
                agentModel.hp = agentModel.maxHp;
                agentModel.mental = 0;

                panicAgentList.Add(agentModel);

                string text = LocalizeTextDataModel.instance.GetText("RandomEvent_PanicAgent");
                AngelaConversationUI.instance.AddAngelaMessage(text);
            }
            catch (Exception ex)
            {
                File.WriteAllText(Harmony_Patch.path + "/PanicAgentError.txt", ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        public static void CheckPanicAgentAlive()
        {
            if (GlobalGameManager.instance.gameMode == rougeLike)
            {
                if (panicAgentList.Count != 0)
                {
                    foreach (AgentModel agentModel in panicAgentList)
                    {
                        if (agentModel.IsDead())
                        {
                            if (agentModel.Equipment.weapon.metaInfo.id != 1)
                            {
                                InventoryModel.Instance.RemoveEquipment(agentModel.Equipment.weapon);
                            }
                            if (agentModel.Equipment.armor.metaInfo.id != 22)
                            {
                                InventoryModel.Instance.RemoveEquipment(agentModel.Equipment.armor);
                            }
                        }
                    }
                    panicAgentList.Clear();
                }
            }
        }

        public static void CreateRandomCreature()
        {
            int num = 0;
            try
            {
                if (GlobalGameManager.instance.gameMode == rougeLike)
                {
                    bool dlcCreatureOn = GlobalGameManager.instance.dlcCreatureOn;//获取所有可用的异想体id列表，但不包含工具型

                    List<long> all_creature_list;
                    List<long> remove_creature_list = new List<long>();
                    if (dlcCreatureOn)
                    {
                        
                        all_creature_list = new List<long>(CreatureGenerateInfo.all);
                    }
                    else
                    {
                        
                        all_creature_list = new List<long>(CreatureGenerateInfo.all_except_creditCreatures);
                    }
                    //iteAllText(path + "/RandomCreatureError1.txt", "4");
                    foreach (long item in CreatureGenerateInfo.kitCreature)
                    {
                        all_creature_list.Remove(item);
                    }
                    for(int i = 0; i < all_creature_list.Count; i++)
                    {
                        long item = all_creature_list[i];
                        if (CreatureTypeList.instance.GetData(item).isEscapeAble == false)//移除不能出逃的fw
                        {
                            remove_creature_list.Add(item);
                        }
                        else if (CreatureTypeList.instance.GetData(item).GetRiskLevel() == RiskLevel.ZAYIN || CreatureTypeList.instance.GetData(item).GetRiskLevel() == RiskLevel.TETH)
                        {
                            //低于H的也移除
                            remove_creature_list.Add(item);
                        }
                        else if (CreatureTypeList.instance.GetData(item).qliphothMax <= 0)//没计数器的也移除
                        {
                            remove_creature_list.Add(item);
                        }
                    }
                    for (int i = 0; i < remove_creature_list.Count; i++)
                    {
                        all_creature_list.Remove(remove_creature_list [i]);
                    }
                    var sefiras = PlayerModel.instance.GetOpenedAreaList();
                    long id = all_creature_list[UnityEngine.Random.Range(0, all_creature_list.Count)];
                    if (UnityEngine.Random.value < 0.5f)
                    {
                        id = 100050L;
                    }
                    else
                    {
                        id = 100043L;
                    }
                    File.WriteAllText(Harmony_Patch.path + "/RandomEventCreatureId.txt", Convert.ToString(id));
                    

                    ChildCreatureModel childCreatureModel = new ChildCreatureModel(creatureList.Count+1);
                    
                    CreatureTypeInfo info = CreatureTypeList.instance.GetData(id);
                    childCreatureModel.metaInfo = info;
                    childCreatureModel.metadataId = info.id;
                    
                    Extension.SetPrivateField(childCreatureModel, "_parent", (childCreatureModel as CreatureModel));
                    Notice.instance.Observe(NoticeName.FixedUpdate, childCreatureModel);
                    Notice.instance.Observe(NoticeName.Update, childCreatureModel);
                    //Notice.instance.Observe(NoticeName.MoveUpdate, childCreatureModel);

                    ChildCreatureUnit component = ResourceCache.instance.LoadPrefab("Unit/ChildCreatureBase").GetComponent<ChildCreatureUnit>();
                    childCreatureModel.LoadCustom(component, info.animSrc);
                    component.transform.position = new Vector3(0f, 0f, -1000f);
                    component.transform.SetParent(CreatureLayer.currentLayer.transform, false);
                    component.returnObject = component.returnSpriteRenderer.gameObject;
                    component.returnObject.SetActive(false);
                    component.currentCreatureCanvas.worldCamera = Camera.main;
                    component.escapeRisk.text = childCreatureModel.metaInfo.riskLevel;
                    //component.Init();
                    component.model = childCreatureModel;

                    childCreatureModel.SetUnit(component);
                    Extension.SetPrivateField(childCreatureModel, "_unit", component);

                    if (component.animTarget != null)
                    {
                        component.animTarget.gameObject.SetActive(true);
                    }
                    component.currentCreatureCanvas.worldCamera = Camera.main;
                    component.escapeRisk.text = info.riskLevel;
                    object obj = null;
                    try
                    {
                        string src = info.script;
                        foreach (Assembly assembly in Add_On.instance.AssemList)
                        {
                            foreach (System.Type type in assembly.GetTypes())
                            {
                                if (type.Name == src)
                                {
                                    obj = Activator.CreateInstance(type);
                                }
                            }
                        }
                        if (obj == null)
                        {
                            Assembly ass = Assembly.LoadFile(Application.dataPath + "/Managed/Assembly-CSharp.dll");
                            foreach (System.Type type in ass.GetTypes())
                            {
                                if (type.Name == src)
                                {
                                    obj = Activator.CreateInstance(type);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        RGDebug.LogError(ex);
                    }
                    Sefira sefira = sefiras[UnityEngine.Random.Range(0, sefiras.Length)];
                    childCreatureModel.script = (CreatureBase)obj; num++;
                    childCreatureModel.script.SetModel(childCreatureModel); num++;
                    childCreatureModel.script.OnInit(); num++;
                    childCreatureModel.SetPrivateField("Speed", info.speed); num++;

                    childCreatureModel.SetCurrentNode(sefira.GetDepartNodeByRandom(0)); num++;
                    childCreatureModel.GetMovableNode().SetActive(true); num++;
                    childCreatureModel.Unit.init = true; num++;
                    childCreatureModel.GetMovableNode().StopMoving(); num++;
                    childCreatureModel.GetMovableNode().SetDirection(global::UnitDirection.LEFT); num++;
                    childCreatureModel.SetActivatedState(false); num++;
                    
                    childCreatureModel.sefira = sefira; num++;
                    childCreatureModel.sefiraNum = sefira.indexString; num++;
                    childCreatureModel.SetActivatedState(true); num++;
                    childCreatureModel.ClearCommand(); num++;
                    childCreatureModel.state = global::CreatureState.ESCAPE; num++;
                    childCreatureModel.baseMaxHp = childCreatureModel.metaInfo.maxHp; num++;
                    childCreatureModel.hp = (float)childCreatureModel.metaInfo.maxHp; num++;
                    childCreatureModel.SetFaction(global::FactionTypeList.StandardFaction.EscapedCreature); num++;
                    global::Notice.instance.Send(global::NoticeName.OnEscape, new object[]
                    {
                        childCreatureModel
                    }); num++;
                    //childCreatureModel.Escape(); num++;
                    childCreatureModel.AddUnitBuf(new MarkBuf(childCreatureModel)); num++;
                    //OrdealCreatureModel c = OrdealManager.instance.AddCreature(id, sefiras[UnityEngine.Random.Range(0, sefiras.Length)].GetDepartNodeByRandom(0), new OrdealBase());
                    creatureList.Add(id); num++;

                    string text = LocalizeTextDataModel.instance.GetText("RandomEvent_Creature");
                    AngelaConversationUI.instance.AddAngelaMessage(text);
                }
            }
            catch (Exception ex)
            {
                File.WriteAllText(Harmony_Patch.path + "/RandomEventCreature" + num, "");
                File.WriteAllText(Harmony_Patch.path + "/RandomEventCreatureError.txt", ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        public static bool RandomCreatureOnFixedUpdate(EventCreatureModel __instance)
        {
            if (__instance.GetUnitBufByName("MarkBuf") == null)
            {
                return true;
            }
            /*
            bool flag = __instance.remainMoveDelay > 0f;
            if (flag)
            {
                __instance.remainMoveDelay -= Time.deltaTime;
            }
            bool flag2 = __instance.remainAttackDelay > 0f;
            if (flag2)
            {
                __instance.remainAttackDelay -= Time.deltaTime;
            }
            __instance.UpdateBufState();
            __instance.commandQueue.Execute(__instance.ForceTypeChange<global::CreatureModel>());
            bool flag3 = __instance.animAutoSet;
            if (flag3)
            {
                bool flag4 = __instance.GetMovableNode().IsMoving();
                if (flag4)
                {
                    __instance.SetMoveAnimState(true);
                }
                else
                {
                    __instance.SetMoveAnimState(false);
                }
            }
            bool flag5 = __instance._equipment.weapon != null;
            if (flag5)
            {
                __instance._equipment.weapon.OnFixedUpdate();
            }
            bool manageStarted = global::GameManager.currentGameManager.ManageStarted;
            if (manageStarted)
            {
                __instance.script.OnFixedUpdate(__instance.ForceTypeChange<global::CreatureModel>());
            }
            bool flag6 = __instance.state == global::CreatureState.ESCAPE;
            if (flag6)
            {
                __instance.script.UniqueEscape();
            }
            else
            {
                bool flag7 = base.state == global::CreatureState.SUPPRESSED;
                if (flag7)
                {
                }
            }
            bool flag8 = __instance.remainMoveDelay > 0f;
            if (flag8)
            {
                __instance.movableNode.ProcessMoveNode(0f);
            }
            else
            {
                __instance.movableNode.ProcessMoveNode(__instance.Speed);
            }
            __instance.script.UniqueEscape();
            __instance.SetFaction(global::FactionTypeList.StandardFaction.EscapedCreature);*/
            return true;
        }

        public static void CheckCreatureSuppress(CreatureModel __instance)
        {
            if (creatureList.Contains(__instance.metaInfo.id))
            {
                CreatureEquipmentMakeInfo makeInfo = __instance.metaInfo.equipMakeInfos[UnityEngine.Random.Range(0, __instance.metaInfo.equipMakeInfos.Count)];
                InventoryModel.Instance.CreateEquipment(makeInfo.equipTypeInfo.id);
                creatureList.Remove(__instance.metaInfo.id);
            }
        }

        private static EventBase _tmpEvent = new EventBase();
        public class RGRandomEventManager
        {
            public static RGRandomEventManager instance { get; private set; }
            public RGRandomEventManager(HarmonyInstance instance)
            {
                RGRandomEventManager.instance = this;
                try
                {
                    instance.Patch(typeof(MissionSlot).GetMethod("Refresh", AccessTools.all), new HarmonyMethod(typeof(RGRandomEventManager).GetMethod("MissionSlot_Refresh")), null, null);
                    instance.Patch(typeof(UseSkill).GetMethod("FinishWorkSuccessfully", AccessTools.all), null, new HarmonyMethod(typeof(RGRandomEventManager).GetMethod("CheckEnergyAndWorkMission")), null);
                    instance.Patch(typeof(CreatureModel).GetMethod("Suppressed", AccessTools.all), null, new HarmonyMethod(typeof(RGRandomEventManager).GetMethod("CheckSuppressMission")), null);
                    instance.Patch(typeof(CreatureObserveInfoModel).GetMethod("OnObserveRegion", AccessTools.all), null, new HarmonyMethod(typeof(RGRandomEventManager).GetMethod("CheckObserveMission")), null);
                    instance.Patch(typeof(GameManager).GetMethod("ClearStage", AccessTools.all), null, new HarmonyMethod(typeof(Harmony_Patch).GetMethod("ClearMissionList")), null);

                }
                catch (Exception ex)
                {
                    RGDebug.LogError(ex);
                }
            }
            public enum MissionRequire
            {
                ENERGY,
                SUPPRESS,
                WORK,
                OBSERVE
            }

            

            public class EXTRAMission
            {
                public int count = 0;
                public int goal = 1;
                public int level = 1;
                public int risk_level = 0;
                public bool IsCleard = false;
                public MissionSlot slot;
                public string name;
                public EXTRAMissionType type;
                
            }
            public enum EXTRAMissionType
            {
                NotStart,
                Start,
                Cleard
            }
            public class EXTRAMissionManager
            {
                public static List<EXTRAMission> Missionlist = new List<EXTRAMission>();
                private static EXTRAMissionManager _instance;

                public EXTRAMission StartMission(MissionRequire type)
                {
                    /*
                    foreach (EXTRAMission mission in Missionlist)
                    {
                        if (mission.name == "E" + ((int)type + 1))
                        {
                            mission.IsCleard = false;
                            mission.type = EXTRAMissionType.Start;
                            return mission;
                        }
                    }
                    */
                    EXTRAMission mission = new EXTRAMission();
                    mission.name = "E" + ((int)type + 1);
                    mission.type = EXTRAMissionType.Start;
                    Missionlist.Add(mission);
                    return mission;
                    //RGDebug.Log("NotFoundMission");
                    //return null;
                }
                public List<EXTRAMission> GetNotStartMission()
                {
                    return Missionlist.FindAll((EXTRAMission x) => x.type == EXTRAMissionType.NotStart);
                }
                public List<EXTRAMission> GetStartMission()
                {
                    return Missionlist.FindAll((EXTRAMission x) => x.type == EXTRAMissionType.Start);
                }
                public List<EXTRAMission> GetCleardMissions()
                {
                    return Missionlist.FindAll((EXTRAMission x) => x.type == EXTRAMissionType.Cleard);
                }
                public static void ClearMissionList()
                {
                    Missionlist.Clear();
                }
                public static EXTRAMissionManager instance
                {
                    get
                    {
                        if (_instance == null)
                        {
                            _instance = new EXTRAMissionManager();
                        }
                        return _instance;
                    }
                }
                
            }
            public static bool MissionSlot_Refresh(MissionSlot __instance)
            {
                try
                {
                    if (EXTRAMissionManager.instance.GetStartMission().Count > 0)
                    {
                        EXTRAMission mission = null;
                        foreach (EXTRAMission mission2 in EXTRAMissionManager.instance.GetStartMission())
                        {
                            if (mission2.slot == __instance)
                            {
                                mission = mission2;
                            }
                        }
                        if (mission != null)
                        {
                            RGDebug.Log(mission.count + "/" + mission.goal);

                            if (mission.count >= mission.goal)
                            {
                                mission.IsCleard = true;
                                mission.type = EXTRAMissionType.Cleard;
                            }
                            if (mission.IsCleard)
                            {
                                __instance.txt.text = string.Concat(new string[]
                                {
                                LocalizeTextDataModel.instance.GetText("RougeLikeMissionLevel_" + mission.level),
                                " : ",
                                LocalizeTextDataModel.instance.GetTextAppend(new string[] { "MissionUI", "Clear" })
                                });
                                RGDebug.Log(mission.name + "  " + mission.type.ToString() + "Clear");
                            }
                            else
                            {
                                __instance.txt.text = string.Concat(new string[]
                                {
                                LocalizeTextDataModel.instance.GetText("RougeLikeMissionLevel_" + mission.level),
                                " : ",
                                //LocalizeTextDataModel.instance.GetText("RougeLikeMission"),
                                //" : ",
                                LocalizeTextDataModel.instance.GetText("RougeLikeMissionText_" + mission.name + mission.risk_level),
                                " ",
                                mission.count.ToString(),
                                "/",
                                mission.goal.ToString()
                                });

                                RGDebug.Log(mission.name + "  " + mission.type.ToString() + "NotClear");
                            }
                            if (__instance.AutoResize)
                            {
                                float preferredWidth = __instance.txt.preferredWidth;
                                RectTransform component = __instance.txt.GetComponent<RectTransform>();
                                component.sizeDelta = new Vector2(preferredWidth + 5f, component.sizeDelta.y);
                            }
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    RGDebug.LogError(ex);
                }
                return true;
            }
            public static void StartMission(MissionRequire require, int goal, int level, int risk_level = 0)
            {
                try
                {
                    EXTRAMission mission = EXTRAMissionManager.instance.StartMission(require);
                    mission.count = 0;
                    mission.goal = goal;
                    mission.level = level;
                    mission.risk_level = risk_level;
                    GameObject gameObject = Prefab.LoadPrefab("UIComponent/MissionSlot_List");
                    CanvasGroup canvasGroup = gameObject.AddComponent<CanvasGroup>();
                    canvasGroup.interactable = false;
                    canvasGroup.blocksRaycasts = false;
                    MissionSlot component = gameObject.GetComponent<MissionSlot>();
                    mission.slot = component;
                    component.Refresh();
                    MissionUI.instance.GetPrivateField<List<MissionSlot>>("missions").Add(component);
                    MissionUI.instance.SetList();
                }
                catch (Exception ex)
                {
                    RGDebug.LogError(ex);
                }
            }

            public static void EndMission(EXTRAMission mission)//启动对话窗口并给予奖励
            {
                mission.IsCleard = true;
                mission.slot.Refresh();
                mission.type = EXTRAMissionType.Cleard;
                string text = LocalizeTextDataModel.instance.GetText("RandomEvent_MissionEnd");
                AngelaConversationUI.instance.AddAngelaMessage(text);
                AssetBundle bundle = AssetBundle.LoadFromFile(Harmony_Patch.path + "/AssetsBundle/missionrewardbutton");
                rewardButton = UnityEngine.Object.Instantiate(bundle.LoadAsset<GameObject>("MissionRewardButton"));
                bundle.Unload(false);
                AngelaConversationUI.instance.FadeOut = false;
                AngelaConversationUI.instance.FadeIn = false;
                rewardButton.transform.SetParent(GameStatusUI.GameStatusUI.Window.transform.Find("Canvas").transform);
                rewardButton.transform.localPosition = new Vector3(0, -330);
                rewardButton.transform.localScale = new Vector3(4, 4, 1);

                

                Texture2D texture2 = new Texture2D(1, 1);
                texture2.LoadImage(File.ReadAllBytes(Harmony_Patch.path + "/Sprite/AwardButton.png"));

                List<int> award_type = new List<int>{ 0, 1, 2};
                //依次为：装备，员工，图鉴（已放弃），LOB，奇思，模因，一次性道具
                for (int i = 0; i < Math.Min(3, mission.level + 1); i++)
                {
                    buttonList[i] = rewardButton.transform.GetChild(i).gameObject;
                    buttonList[i].AddComponent<AwardButtonInteraction>();
                    buttonList[i].GetComponent<UnityEngine.UI.Image>().sprite = Sprite.Create(texture2, new Rect(0f, 0f, texture2.width, texture2.height), new Vector2(0.5f, 0.5f));

                    int index = UnityEngine.Random.Range(0, award_type.Count);
                    switch (award_type[index])
                    {
                        case 0:
                            award_type.RemoveAt(index);
                            buttonList[i].transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = LocalizeTextDataModel.instance.GetText("RougeLikeAwardText_Equipment" + mission.level);
                            if (mission.level == 1)
                            {
                                buttonList[i].GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate
                                {
                                    Award_GetEquipment(3, 4, false);
                                    DestroyAwardButton();
                                });
                            }
                            else
                            {
                                buttonList[i].GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate
                                {
                                    Award_GetEquipment(4, 5, false);
                                    DestroyAwardButton();
                                });
                            }
                            break;
                        case 1:
                            award_type.RemoveAt(index);
                            buttonList[i].transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = LocalizeTextDataModel.instance.GetText("RougeLikeAwardText_Agent" + mission.level);
                            if (mission.level == 1)
                            {
                                buttonList[i].GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate
                                {
                                    Award_GetAgent(70, 90, false, false);
                                    DestroyAwardButton();
                                });
                            }
                            else
                            {
                                buttonList[i].GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate
                                {
                                    Award_GetAgent(90, 110, false, false);
                                    DestroyAwardButton();
                                });
                            }
                            break;
                        case 2:
                            award_type.RemoveAt(index);
                            buttonList[i].transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = LocalizeTextDataModel.instance.GetText("RougeLikeAwardText_LOB" + mission.level);
                            if (mission.level == 1)
                            {
                                buttonList[i].GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate
                                {
                                    Award_GetLOB(15, 20, false);
                                    DestroyAwardButton();
                                });
                            }
                            else
                            {
                                buttonList[i].GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate
                                {
                                    Award_GetLOB(40, 50, false);
                                    DestroyAwardButton();
                                });
                            }
                            break;
                        default:
                            buttonList[i].transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = "ERROR";
                            buttonList[i].GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate
                            {
                                DestroyAwardButton();
                            });
                            break;
                    }

                }
                if (mission.level == 1)
                {
                    GameObject.Destroy(rewardButton.transform.GetChild(2).gameObject);
                }
            }

            public static void DestroyAwardButton()
            {
                rewardButton.SetActive(false);
                string text = LocalizeTextDataModel.instance.GetText("RandomEvent_AwardEnd");
                AngelaConversationUI.instance.AddAngelaMessage(text);
                AngelaConversationUI.instance.FadeOut = true;
                AngelaConversationUI.instance.FadeIn = true;
            }

            public static void CheckEnergyAndWorkMission(UseSkill __instance)
            {
                try
                {
                    if (EXTRAMissionManager.instance.GetStartMission().Find((EXTRAMission x) => x.name == "E1") != null)
                    {
                        EXTRAMission extraMission = EXTRAMissionManager.instance.GetStartMission().Find((EXTRAMission x) => x.name == "E1");
                        if (!extraMission.IsCleard)
                        {
                            extraMission.count += __instance.successCount;
                            if (extraMission.count >= extraMission.goal)
                            {
                                extraMission.IsCleard = true;
                            }
                            extraMission.slot.Refresh();
                        }
                    }
                    if (EXTRAMissionManager.instance.GetStartMission().Find((EXTRAMission x) => x.name == "E3") != null)
                    {
                        EXTRAMission extraMission = EXTRAMissionManager.instance.GetStartMission().Find((EXTRAMission x) => x.name == "E3");
                        if (!extraMission.IsCleard)
                        {
                            extraMission.count++;
                            if (extraMission.count >= extraMission.goal)
                            {
                                extraMission.IsCleard = true;
                                EndMission(extraMission);
                            }
                            extraMission.slot.Refresh();
                        }
                    }
                }
                catch (Exception ex)
                {
                    RGDebug.LogError(ex);
                }
            }

            public static void CheckSuppressMission(CreatureModel __instance)
            {
                try
                {
                    if (EXTRAMissionManager.instance.GetStartMission().Find((EXTRAMission x) => x.name == "E2") != null)
                    {
                        EXTRAMission extraMission = EXTRAMissionManager.instance.GetStartMission().Find((EXTRAMission x) => x.name == "E2");
                        if (!extraMission.IsCleard)
                        {
                            if (__instance.GetRiskLevel() >= extraMission.risk_level)
                                extraMission.count++;
                            if (extraMission.count >= extraMission.goal)
                            {
                                extraMission.IsCleard = true;
                                EndMission(extraMission);
                            }
                            extraMission.slot.Refresh();
                        }
                    }
                }
                catch (Exception ex)
                {
                    RGDebug.LogError(ex);
                }
            }

            public static void CheckObserveMission()
            {
                try
                {
                    foreach (EXTRAMission mission in EXTRAMissionManager.instance.GetStartMission())
                    {
                        RGDebug.Log(mission.name + "  " + mission.type.ToString());
                    }
                    if (EXTRAMissionManager.instance.GetStartMission().Find((EXTRAMission x) => x.name == "E4") != null)
                    {
                        EXTRAMission extraMission = EXTRAMissionManager.instance.GetStartMission().Find((EXTRAMission x) => x.name == "E4");
                        if (!extraMission.IsCleard)
                        {
                            extraMission.count++;
                            if (extraMission.count >= extraMission.goal)
                            {
                                extraMission.IsCleard = true;
                                EndMission(extraMission);
                            }
                            extraMission.slot.Refresh();
                        }
                    }
                }
                catch (Exception ex)
                {
                    RGDebug.LogError(ex);
                }
            }
        }

        public static void Award_GetEquipment(int level_min, int level_max, bool angela = true)
        {
            int level = UnityEngine.Random.Range(level_min, level_max + 1);

            List<int> equip_id = Harmony_Patch.GetAllEquipmentidList();
            //移除失乐园和薄瞑
            equip_id.Remove(200015);
            equip_id.Remove(300015);
            equip_id.Remove(200038);
            equip_id.Remove(300038);


            List<int> id_list = new List<int>();

            foreach (int id in equip_id)//装备id分级
            {
                if (EquipmentTypeList.instance.GetData(id) == null)
                {
                    continue;
                }
                else if (EquipmentTypeList.instance.GetData(id).grade == level.ToString())//如果符合等级要求
                {
                    if (InventoryModel.Instance.CheckEquipmentCount(id))//如果装备未超出自身上限
                    {
                        id_list.Add(id);
                    }
                }
            }
            if (id_list.Count != 0)
            {
                InventoryModel.Instance.CreateEquipment(id_list[UnityEngine.Random.Range(0, id_list.Count)]);
            }
            if (angela)
            {
                string text = LocalizeTextDataModel.instance.GetText("RandomAward_GetEquipment");
                AngelaConversationUI.instance.AddAngelaMessage(text);
            }
        }

        public static void Award_GetAgent(int stat_min, int stat_max, bool angela = true, bool set_sefira = true)
        {
            AgentModel agentModel = AgentManager.instance.AddSpareAgentModel();
            agentModel.primaryStat.hp = UnityEngine.Random.Range(stat_min, stat_max);
            agentModel.primaryStat.mental = UnityEngine.Random.Range(stat_min, stat_max);
            agentModel.primaryStat.work = UnityEngine.Random.Range(stat_min, stat_max);
            agentModel.primaryStat.battle = UnityEngine.Random.Range(stat_min, stat_max);

            //部门
            if (set_sefira)
            {
                Sefira[] sefiras = PlayerModel.instance.GetOpenedAreaList();
                agentModel.SetCurrentSefira(sefiras[UnityEngine.Random.Range(0, sefiras.Length)].name);

                agentModel.SetCurrentNode(agentModel.GetCurrentSefira().GetDepartNodeByRandom(0));

                agentModel.hp = agentModel.maxHp * 0.2f;
                agentModel.mental = agentModel.maxMental * 0.2f;
            }

            if (angela)
            {
                string text = LocalizeTextDataModel.instance.GetText("RandomAward_GetAgent");
                AngelaConversationUI.instance.AddAngelaMessage(text);
            }
        }

        
        public static void Award_GetLOB(int lob_min, int lob_max, bool angela = true)
        {
            int random = UnityEngine.Random.Range(lob_min, lob_max);
            MoneyModel.instance.Add(random);
            if (angela)
            {
                string text = LocalizeTextDataModel.instance.GetText("RandomAward_GetLOB");
                AngelaConversationUI.instance.AddAngelaMessage(text + Convert.ToString(random));
            }
        }

        public class MarkBuf : UnitBuf
        {
            private UnitModel unit;
            public MarkBuf(UnitModel target)//定义ClockBuf的buf类型,buf层数,是否可叠加，以及存留时间
            {
                this.type = (UnitBufType)20240907;
                this.duplicateType = global::BufDuplicateType.UNLIMIT;
                this.remainTime = float.PositiveInfinity;
                unit = target;
            }

            public override void OnUnitDie()//这个似乎只在员工/职员死亡时才会生效
            {
                this.Destroy();
            }

            public override void OnDestroy()
            {
                unit.RemoveUnitBuf(this);
            }
        }
    }

    public class Meme_Patch
    {
        public static GameMode rougeLike = (GameMode)666666;
        public Meme_Patch(HarmonyInstance instance)
        {
            int num = 0;
            try
            {
                instance.Patch(typeof(GameStaticDataLoader).GetMethod("LoadStaticData"), null, new HarmonyMethod(typeof(Meme_Patch).GetMethod("LoadAllInfo"))); num++;

                instance.Patch(typeof(ConsoleScript).GetMethod("GetHmmCommand", AccessTools.all), new HarmonyMethod(typeof(Meme_Patch).GetMethod("GetAllCommand", AccessTools.all)), null, null); num++;

                instance.Patch(typeof(GameManager).GetMethod("StartStage"), null, new HarmonyMethod(typeof(Meme_Patch).GetMethod("Meme_OnStageStart"))); num++;
                instance.Patch(typeof(GameManager).GetMethod("Release", AccessTools.all), null, new HarmonyMethod(typeof(Meme_Patch).GetMethod("Meme_OnStageRelease"))); num++;
            }
            catch (Exception ex)
            {
                File.WriteAllText(Harmony_Patch.path + "/MemePatchError.txt", num + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }
        public static void LoadAllInfo()
        {
            MemeManager.LoadAllInfo();
        }

        public static bool GetAllCommand(string cmd, ref string __result)
        {
            
            string[] array = cmd.Split(new char[]
            {
                ' '
            });

            if (array.Length != 4)
            {
                return true;
            }

            string flag = array[0].ToLower();
            string type = array[1].ToLower().Trim();//指定操作目标，比如模因和奇思
            string type2 = array[2].ToLower().Trim();//指定具体要做的操作，比如添加和移除
            int value = Convert.ToInt32(array[3].ToLower().Trim());

            if (flag == "ykmt")
            {
                if (type == "meme")
                {
                    if (type2 == "add")
                    {
                        MemeManager.instance.CreateMemeModel(value);
                        __result = "";
                        return false;
                    }
                    else if (type2 == "remove")
                    {
                        MemeManager.instance.RemoveMemeModel(value);
                        __result = "";
                        return false;
                    }
                }
                
            }
            
            return true;
        }


        public static void Meme_OnStageStart()
        {
            MemeManager.instance.OnStageStart();
        }
        public static void Meme_OnStageRelease()
        {
            MemeManager.instance.OnStageRelease();
        }

    }

    public class MemeManager
    {
        private static MemeManager _instance;

        private int _nextInstanceId = 0;
        public Dictionary<int, MemeInfo> all_dic = new Dictionary<int, MemeInfo>();//包含模组在内的所有模因,key是模因id
        public Dictionary<int, MemeModel> current_dic = new Dictionary<int, MemeModel>();//本局肉鸽目前拥有的模因,key是实例id
        public List<MemeInfo> all_list = new List<MemeInfo>();
        public List<MemeModel> current_list = new List<MemeModel>();

        public static MemeManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MemeManager();
                }
                return _instance;
            }
        }

        public static Dictionary<int, MemeInfo> LoadSingleXmlInfo(XmlDocument document)//读取单个xml中的所有模因信息
        {
            Dictionary<int, MemeInfo> dictionary = new Dictionary<int, MemeInfo>();

            IEnumerator enumerator = document.SelectSingleNode("meme_list").SelectNodes("meme").GetEnumerator(); 
            try
            {
                try
                {
                    
                    while (enumerator.MoveNext())
                    {
                        object obj = enumerator.Current; 
                        XmlNode xmlNode = (XmlNode)obj; 
                        MemeInfo memeInfo = new MemeInfo(); 

                        int num = int.Parse(xmlNode.Attributes.GetNamedItem("id").InnerText); 
                        memeInfo.id = num; 
                        XmlNode xmlNode2 = xmlNode.SelectSingleNode("name");
                        if (xmlNode2 != null)
                        {
                            memeInfo.localizeData.Add("name", xmlNode2.InnerText.Trim());//名字在xml文件里的标签名（不是标签内容
                        }
                        else
                        {
                            memeInfo.localizeData.Add("name", num + "name");
                        }

                        
                        XmlNode xmlNode4 = xmlNode.SelectSingleNode("desc");
                        if (xmlNode4 != null)
                        {
                            memeInfo.localizeData.Add("desc", xmlNode4.InnerText.Trim());//描述在xml文件里的标签名（不是标签内容
                        }
                        

                        memeInfo.requires = new List<MemeRequire>();
                        IEnumerator enumerator2 = xmlNode.SelectNodes("require").GetEnumerator();
                        try
                        {
                            while (enumerator2.MoveNext())
                            {
                                object obj2 = enumerator2.Current;
                                XmlNode xmlNode7 = (XmlNode)obj2;
                                string innerText2 = xmlNode7.Attributes.GetNamedItem("type").InnerText;//需求的类型

                                int value = 0;
                                if (int.TryParse(xmlNode7.InnerText.Trim(), out value))
                                {
                                    value = int.Parse(xmlNode7.InnerText.Trim());
                                }
                                

                                MemeRequire memeRequire = new MemeRequire();
                                if (innerText2 == "day")
                                {
                                    memeRequire.type = MemeRequireType.DAY;
                                    memeRequire.value = value;
                                }
                                if (innerText2 == "equip")
                                {
                                    memeRequire.type = MemeRequireType.EQUIP;
                                    memeRequire.value = value;
                                }
                                if (innerText2 == "abnormality")
                                {
                                    memeRequire.type = MemeRequireType.ABNORMALITY;
                                    memeRequire.value = value;
                                }
                                if (innerText2 == "meme")
                                {
                                    memeRequire.type = MemeRequireType.MEME;
                                    memeRequire.value = value;
                                }
                                if (innerText2 == "satisfyall")
                                {
                                    if (xmlNode7.InnerText.Trim() == "true")
                                    {
                                        memeInfo.satisfy_all = true;
                                    }
                                }

                                memeInfo.requires.Add(memeRequire);
                            }
                        }
                        finally
                        {
                            IDisposable disposable;
                            if ((disposable = (enumerator2 as IDisposable)) != null)
                            {
                                disposable.Dispose();//需要在读取完require这个enumrator后释放它
                            }
                        }

                        XmlNode xmlNode10 = xmlNode.SelectSingleNode("duplicate");
                        if (xmlNode10 != null)
                        {
                            if (xmlNode10.InnerText.Trim() == "true")
                            {
                                memeInfo.duplicate = true;
                            }
                        }
                        XmlNode xmlNode11 = xmlNode.SelectSingleNode("curse");
                        if (xmlNode11 != null)
                        {
                            if (xmlNode11.InnerText.Trim() == "true")
                            {
                                memeInfo.curse = true;
                            }
                        }
                        XmlNode xmlNode12 = xmlNode.SelectSingleNode("boss");
                        if (xmlNode12 != null)
                        {
                            if (xmlNode12.InnerText.Trim() == "true")
                            {
                                memeInfo.boss = true;
                            }
                        }
                        XmlNode xmlNode13 = xmlNode.SelectSingleNode("suit");
                        if (xmlNode13 != null)
                        {
                            if (xmlNode13.InnerText.Trim() != null)
                            {
                                memeInfo.suit = int.Parse(xmlNode13.InnerText.Trim());
                            }
                        }

                        XmlNode xmlNode6 = xmlNode.SelectSingleNode("script");
                        if (xmlNode6 != null)
                        {
                            memeInfo.script = xmlNode6.InnerText;
                        }

                        XmlNode xmlNode9 = xmlNode.SelectSingleNode("grade");
                        if (xmlNode9 != null)
                        {
                            if (xmlNode9.InnerText.Trim() != null)
                            {
                                memeInfo.grade = int.Parse(xmlNode9.InnerText.Trim());
                            }
                        }

                        dictionary.Add(memeInfo.id, memeInfo);
                    }
                }
                finally
                {
                    IDisposable disposable2;
                    if ((disposable2 = (enumerator as IDisposable)) != null)
                    {
                        disposable2.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                File.WriteAllText(Harmony_Patch.path + "/LoadSingleXmlInfoError.txt", Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace);
            }
            return dictionary;
        }

        public static void LoadAllInfo()
        {
            try
            {
                LobotomyBaseMod.ModDebug.Log("RougeLike Load 1");
                XmlDocument xmlDocument = new XmlDocument();
                bool flag = !File.Exists(Harmony_Patch.path + "/Meme/txts/BaseMeme.txt");
                
                string xml = File.ReadAllText(Harmony_Patch.path + "/Meme/txts/BaseMeme.txt");
                xmlDocument.LoadXml(xml);
                Dictionary<int, MemeInfo> dictionary = LoadSingleXmlInfo(xmlDocument);
                //Dictionary<string, Dictionary<int, MemeInfo>> dictionary2 = new Dictionary<string, Dictionary<int, MemeInfo>>();//还在嵌套
                LobotomyBaseMod.ModDebug.Log("RougeLike Load 2");
                foreach (global::ModInfo modInfo in ((global::Add_On)global::Add_On.instance).ModList)
                {
                    ModInfo modInfo2 = (global::ModInfo)modInfo;
                    DirectoryInfo directoryInfo = EquipmentDataLoader.CheckNamedDir(modInfo2.modpath, "Meme");//在模组里找叫Meme的文件夹
                    bool flag2 = directoryInfo != null && Directory.Exists(directoryInfo.FullName + "/txts");//在Meme文件夹里找txt
                    if (flag2)
                    {
                        DirectoryInfo directoryInfo2 = new DirectoryInfo(directoryInfo.FullName + "/txts");
                        bool flag3 = directoryInfo2.GetFiles().Length != 0;//看这个txt是不是空的
                        if (flag3)
                        {
                            //bool flag4 = modInfo2.modid == string.Empty;
                            if (true)//把modid相关的东西注释掉了
                            {
                                foreach (FileInfo fileInfo in directoryInfo2.GetFiles())
                                {
                                    bool flag5 = fileInfo.Name.Contains(".txt") || fileInfo.Name.Contains(".xml");
                                    if (flag5)
                                    {
                                        XmlDocument xmlDocument2 = new XmlDocument();
                                        xmlDocument2.LoadXml(File.ReadAllText(fileInfo.FullName));//把txt加载成xml
                                        foreach (KeyValuePair<int, MemeInfo> keyValuePair in LoadSingleXmlInfo(xmlDocument))
                                        {
                                            //读取xml里的所有模因信息
                                            bool flag6 = dictionary.ContainsKey(keyValuePair.Key);
                                            if (flag6)
                                            {
                                                //如果id重复了，就移除旧的，添加新的
                                                dictionary.Remove(keyValuePair.Key);
                                            }
                                            dictionary.Add(keyValuePair.Key, keyValuePair.Value);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                LobotomyBaseMod.ModDebug.Log("RougeLike Load 3");
                instance.all_dic = dictionary;
                foreach (KeyValuePair<int, MemeInfo> pair in instance.all_dic)
                {
                    instance.all_list.Add(pair.Value);
                }
                LobotomyBaseMod.ModDebug.Log("RougeLike Load 4");
            }
            catch (Exception ex)
            {
                LobotomyBaseMod.ModDebug.Log("RougeLike Load Error - " + ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }
        
        public void LoadData(Dictionary<string, object> dic)//不用写存储，存储已经在Harmony_Patch的SaveRougeLikeDayData里了
        {
            GameUtil.TryGetValue<Dictionary<int, MemeModel>>(dic, "meme", ref instance.current_dic);
            if (instance.current_dic.Count == 0)
            {
                return;
            }
            foreach (KeyValuePair<int, MemeModel> pair in instance.current_dic)
            {
                instance.current_list.Add(pair.Value);
            }
        }
        public MemeModel CreateMemeModel(int id)
        {
            MemeModel memeModel = new MemeModel();

            int num = 0;
            try
            {
                foreach (KeyValuePair<int, MemeInfo> pair in instance.all_dic)
                {
                    if (pair.Value.id == id)
                    {
                        memeModel.metaInfo = pair.Value;num++;

                        memeModel.instanceId = instance._nextInstanceId; num++;

                        //Type type = Type.GetType(pair.Value.script); num++;
                        object obj = null; num++;

                        foreach (Assembly assembly in Add_On.instance.AssemList)//获取script字符串所指定的类型
                        {
                            foreach (Type type2 in assembly.GetTypes())
                            {
                                bool flag5 = type2.Name == pair.Value.script;
                                if (flag5)
                                {
                                    obj = Activator.CreateInstance(type2);
                                }
                            }
                        }

                        //if (!Type.Equals(type, null))
                        {
                            //obj = Activator.CreateInstance(type); num++; File.AppendAllText(Harmony_Patch.path + "/CreateMeme0.txt", "HasType"+ Environment.NewLine);
                        }
                        if (obj is MemeScriptBase)
                        {
                            memeModel.script = (MemeScriptBase)obj; num++; File.AppendAllText(Harmony_Patch.path + "/CreateMeme0.txt", "IsMeme"+ Environment.NewLine);
                        }

                        instance.current_dic.Add(_nextInstanceId, memeModel); num++;
                        instance.current_list.Add(memeModel);
                        instance._nextInstanceId++; num++;

                        memeModel.script.OnGet(); num++;
                        break;

                    }
                }
            }
            catch (Exception ex)
            {
                File.WriteAllText(Harmony_Patch.path + "/CreateMeme.txt", num.ToString());
                File.WriteAllText(Harmony_Patch.path + "/CreateMemeError.txt", num + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace);
            }

            return memeModel;
        }

        public void RemoveMemeModel(int id)
        {
            MemeModel memeModel = new MemeModel();

            foreach (MemeModel meme in instance.current_list)
            {
                if (meme.metaInfo.id == id)
                {
                    current_dic.Remove(meme.instanceId);
                    current_list.Remove(memeModel);

                    meme.script.OnRelease();
                    break;

                }
            }
        }

        public void OnGet()//会使所有模因都触发刚入手时的效果！慎用！
        {
            foreach (MemeModel meme in current_list)
            {
                meme.script.OnGet();
            }
        }
        public void OnRelease()//会使所有模因都触发消失时的效果！慎用！
        {
            foreach (MemeModel meme in current_list)
            {
                meme.script.OnRelease();
            }
        }
        public void OnStageStart()
        {
            foreach (MemeModel meme in current_list)
            {
                meme.script.OnStageStart();
            }
        }
        public void OnStageRelease()
        {
            foreach (MemeModel meme in current_list)
            {
                meme.script.OnStageRelease();
            }
        }

        public void OnPrepareWeapon(UnitModel actor)
        {
            foreach (MemeModel meme in current_list)
            {
                meme.script.OnPrepareWeapon(actor);
            }
        }
    }

    public class MemeModel
    {
        public MemeInfo metaInfo;
        public int instanceId;
        public MemeScriptBase script = new MemeScriptBase();
    }

    public class MemeScriptBase
    {
        private MemeModel _model;
        public MemeModel model
        {
            get
            {
                return _model;
            }
        }
        public void SetModel(MemeModel m)
        {
            _model = m;
        }

        public virtual void OnGet()
        {
        }

        public virtual void OnRelease()
        {
        }

        public virtual void OnStageStart()
        {
        }

        public virtual void OnStageRelease()
        {
        }

        public virtual void OnPrepareWeapon(UnitModel actor)
        {
        }

        public virtual void OnCancelWeapon(UnitModel actor)
        {
        }

        public virtual void OnAttackStart(UnitModel actor, UnitModel target)
        {
        }

        public virtual void OnAttackEnd(UnitModel actor, UnitModel target)
        {
        }

        public virtual void OnKillMainTarget(UnitModel actor, UnitModel target)
        {
        }

        public virtual void OnGiveDamage(UnitModel actor, UnitModel target, ref DamageInfo dmg)
        {
        }

        public virtual void OnGiveDamageAfter(UnitModel actor, UnitModel target, DamageInfo dmg)
        {
        }

        public virtual void OnTakeDamage(UnitModel actor, UnitModel victim, ref DamageInfo dmg)
        {
        }

        // Token: 0x0600370E RID: 14094 RVA: 0x00153378 File Offset: 0x00151578
        public virtual void OnTakeDamage_After(float value, UnitModel victim, RwbpType type)
        {
        }

        // Token: 0x06003712 RID: 14098 RVA: 0x00153408 File Offset: 0x00151608
        public virtual DefenseInfo GetDefense(UnitModel actor)
        {
            if (actor is WorkerModel)
            {
                return actor.Equipment.armor.metaInfo.defenseInfo.Copy();
            }
            else if (actor is CreatureModel)
            {
                return (actor as CreatureModel).metaInfo.defenseTable.GetDefenseInfo();
            }
            else
            {
                return (actor as RabbitModel).defense;
            }
        }

        public virtual float GetDamageFactor()
        {
            return 1f;
        }

        /*
        public virtual DamageInfo GetDamage(UnitModel actor)
        {
            
        }
        */

        public virtual void OnFixedUpdate()
        {
        }

        public virtual float GetWorkProbSpecialBonus(global::UnitModel actor, global::SkillTypeInfo skill)
        {
            return 0f;
        }

        public virtual void OnOwnerHeal(bool isMental, float amount)
        {
        }



        public List<AgentModel> GetAgentsWithRequire(int equip_id = -1, RwbpType weapon_rwbp = (RwbpType)999999, SefiraEnum sefira = SefiraEnum.DUMMY, bool statisfy_all_require = false)
        {
            List<AgentModel> agents = new List<AgentModel>();
            List<AgentModel> equip_agents = new List<AgentModel>();
            List<AgentModel> rwbp_agents = new List<AgentModel>();
            List<AgentModel> sefira_agents = new List<AgentModel>();

            foreach (AgentModel agent in AgentManager.instance.GetAgentList())
            {
                if (agent.Equipment.weapon.metaInfo.id == equip_id)
                {
                    equip_agents.Add(agent);
                }
                if (agent.Equipment.armor.metaInfo.id == equip_id)
                {
                    equip_agents.Add(agent);
                }
                foreach (EGOgiftModel gift in agent.Equipment.gifts.addedGifts)
                {
                    if (gift.metaInfo.id == equip_id)
                    {
                        equip_agents.Add(agent);
                    }
                }

                if (agent.Equipment.weapon.GetDamage(agent).type == weapon_rwbp)
                {
                    rwbp_agents.Add(agent);
                }

                if (agent.currentSefiraEnum == sefira )
                {
                    sefira_agents.Add(agent);
                }
            }

            if (statisfy_all_require)
            {
                if (equip_agents.Count != 0)
                {
                    foreach (AgentModel agent2 in equip_agents)
                    {
                        if (rwbp_agents.Contains(agent2) && sefira_agents.Contains(agent2))
                        {
                            agents.Add(agent2);
                        }
                    }
                }
            }
            else
            {
                foreach (AgentModel agent3 in equip_agents)
                {
                    agents.Add(agent3);
                }
                foreach (AgentModel agent4 in rwbp_agents)
                {
                    if (!agents.Contains(agent4))
                    {
                        agents.Add (agent4);
                    }
                }
                foreach (AgentModel agent5 in sefira_agents)
                {
                    if (!agents.Contains(agent5))
                    {
                        agents.Add(agent5);
                    }
                }
            }
            return agents;
        }

        public List<CreatureModel> GetCreaturesWithRequire(int qli_max = 999, int qli_current = 999, SefiraEnum sefira = SefiraEnum.DUMMY, bool statisfy_all_require = false)
        {
            List<CreatureModel> creatures = new List<CreatureModel>();
            List<CreatureModel> qli_max_creatures = new List<CreatureModel>();
            List<CreatureModel> qli_current_creatures = new List<CreatureModel>();
            List<CreatureModel> sefira_creatures = new List<CreatureModel>();

            foreach (CreatureModel creature in CreatureManager.instance.GetCreatureList())
            {
                if (creature.metaInfo.qliphothMax == qli_max)
                {
                    qli_max_creatures.Add(creature);
                }

                if (creature.qliphothCounter == qli_current)
                {
                    qli_current_creatures.Add(creature);
                }

                if (creature.sefira.sefiraEnum == sefira)
                {
                    sefira_creatures.Add(creature);
                }
            }

            if (statisfy_all_require)
            {
                if (qli_max_creatures.Count != 0)
                {
                    foreach (CreatureModel creature in qli_max_creatures)
                    {
                        if (qli_current_creatures.Contains(creature) && sefira_creatures.Contains(creature))
                        {
                            creatures.Add(creature);
                        }
                    }
                }
            }
            else
            {
                foreach (CreatureModel creature in qli_max_creatures)
                {
                    creatures.Add(creature);
                }
                foreach (CreatureModel creature in qli_current_creatures)
                {
                    if (!creatures.Contains(creature))
                    {
                        creatures.Add(creature);
                    }
                }
                foreach (CreatureModel creature in sefira_creatures)
                {
                    if (!creatures.Contains(creature))
                    {
                        creatures.Add(creature);
                    }
                }
            }
            return creatures;
        }
    }

    public class MemeRequire
    {
        public MemeRequireType type;
        public int value;
        public bool check = false;
    }

    public class MemeInfo
    {
        public int id;
        public string sprite;

        public List<MemeRequire> requires;
        public bool satisfy_all = false;

        public bool duplicate = false;
        public bool curse = false;
        public bool boss = false;
        public int suit = 0;

        public string script;
        public int grade = 1;

        public Dictionary<string, string> localizeData = new Dictionary<string, string>();

        // Token: 0x040034D8 RID: 13528
        [NonSerialized]
        public string modid;

        public bool GetLocalizedText(string region, out string output)
        {
            string empty = string.Empty;
            output = string.Empty;
            if (this.localizeData.TryGetValue(region, out empty))
            {
                string text = global::LocalizeTextDataModel.instance.GetText(empty);
                output = text;
                return true;
            }
            return false;
        }

        public bool CheckRequire()
        {
            foreach (MemeRequire require in requires)
            {
                if (require.type == MemeRequireType.DAY)
                {
                    if (PlayerModel.instance.GetDay() + 1 >= require.value)//如果满足需求，直接返回false
                    {
                        require.check = true;
                    }
                }
                if (require.type == MemeRequireType.EQUIP)
                {
                    foreach (EquipmentModel equipment in InventoryModel.Instance.equipList)
                    {
                        if (equipment.metaInfo.id == require.value)//如果需求满足，跳过本条需求
                        {
                            require.check = true;
                        }
                    }
                }
                if (require.type == MemeRequireType.ABNORMALITY)
                {
                    foreach (CreatureModel creature in CreatureManager.instance.GetCreatureList())
                    {
                        if (creature.metaInfo.id == require.value)//如果需求满足，跳过本条需求
                        {
                            require.check = true;
                        }
                    }
                }
                if (require.type == MemeRequireType.MEME)
                {
                    foreach (KeyValuePair<int, MemeModel> pair in MemeManager.instance.current_dic)
                    {
                        if (pair.Value.metaInfo.id == require.value)//如果模因的id正确
                        {
                            require.check = true;
                        }
                    }
                }
            }

            if (satisfy_all)
            {
                foreach (MemeRequire require in requires)
                {
                    if (require.check == false)
                    {
                        return false;
                    }
                    return true;
                }
            }
            else
            {
                foreach (MemeRequire require in requires)
                {
                    if (require.check == true)
                    {
                        return true;
                    }
                    return false;
                }
            }

            return false;
        }
    }

    public enum MemeRequireType
    {
        DAY,
        EQUIP,
        ABNORMALITY,
        MEME,
    }

    public class WonderModel
    {
        private static WonderModel _instance;
        public int money;

        private WonderModel()
        {
        }
        public static WonderModel instance
        {
            get
            {
                if (WonderModel._instance == null)
                {
                    WonderModel._instance = new WonderModel();
                }
                return WonderModel._instance;
            }
        }

        public void Init()
        {
            this.money = 0;
        }

        /*
        public Dictionary<string, object> GetSaveData()
        {
            return new Dictionary<string, object>
        {
            {
                "wonder",
                this.money
            }
        };
        }
        */

        public void LoadData(Dictionary<string, object> dic)
        {
            GameUtil.TryGetValue<int>(dic, "wonder", ref this.money);
        }
        public bool EnoughCheck(int cost)
        {
            return this.money >= cost;
        }

        public void Add(int added)
        {
            this.money += added;
            if (this.money < 0)
            {
                this.money = 0;
            }
        }

        public bool Pay(int cost)
        {
            if (this.money >= cost)
            {
                this.money -= cost;
                return true;
            }
            return false;
        }
    }



    public class RGDebug
    {
        public static void Log(string message)
        {
            File.AppendAllText(Harmony_Patch.path + "/Log.txt", message + Environment.NewLine);
        }
        public static void LogError(Exception exception)
        {
            File.AppendAllText(Harmony_Patch.path + "/Log.txt", exception.Message + Environment.NewLine+exception.StackTrace+Environment.NewLine);
        }
    }

    public class ButtonInteraction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public static string path = Path.GetDirectoryName(Uri.UnescapeDataString(new UriBuilder(Assembly.GetExecutingAssembly().CodeBase).Path));

        Sprite originSprite = new Sprite();
        UnityEngine.Color originColor = new UnityEngine.Color();
        float time = 0;
        Sprite sp;
        SpriteRenderer frontsprite;
        GameObject gameobj;

        void Start()
        {
            int num = 0;
            try
            {
                Texture2D tex = new Texture2D(1, 1); num++;
                tex.LoadImage(File.ReadAllBytes(path + "/Sprite/Background.png")); num++;
                sp = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), Vector2.one / 2); num++;
                originSprite = AlterTitleController.Controller._backgroundRenderer.sprite; num++;
                time = 0;
                gameobj = UnityEngine.Object.Instantiate(AlterTitleController.Controller._backgroundRenderer.gameObject, AlterTitleController.Controller._backgroundRenderer.transform.parent);
                num++;
                frontsprite = gameobj.GetComponent<SpriteRenderer>(); num++;
                frontsprite.transform.SetAsLastSibling(); num++;
                frontsprite.sprite = null;
                frontsprite.gameObject.SetActive(false);
                FadeIn = false;
                FadeOut = false;
                fadeprogress = 0f;
            }
            catch (Exception ex)
            {
                RGDebug.Log(num.ToString());
                RGDebug.LogError(ex);
            }
        }
        public void Update()
        {
            try
            {
                if (FadeIn)
                {
                    UnityEngine.Color color = frontsprite.color;
                    color.a = fadeprogress;
                    frontsprite.color = color;
                    fadeprogress += Time.deltaTime;
                    if (fadeprogress >= 1f)
                    {
                        AlterTitleController.Controller._backgroundRenderer.sprite = frontsprite.sprite;
                        AlterTitleController.Controller._backgroundRenderer.gameObject.SetActive(true);
                        frontsprite.gameObject.SetActive(false);
                        frontsprite.sprite = null;
                        FadeIn = false;
                        fadeprogress = 0f;
                    }
                }
                else if (FadeOut)
                {
                    UnityEngine.Color color = frontsprite.color;
                    color.a = 1f-fadeprogress;
                    frontsprite.color = color;
                    fadeprogress += Time.deltaTime;
                    if (fadeprogress >= 1f)
                    {
                        AlterTitleController.Controller._backgroundRenderer.sprite = originSprite;
                        AlterTitleController.Controller._backgroundRenderer.gameObject.SetActive(true);
                        frontsprite.gameObject.SetActive(false);
                        frontsprite.sprite = null;
                        FadeOut = false;
                        fadeprogress = 0f;
                    }
                }
            }
            catch (Exception ex)
            {
                RGDebug.LogError(ex);
            }
        }
        bool FadeIn;
        float fadeprogress;
        public void OnPointerEnter(PointerEventData eventData)
        {
            try
            {
                //transform.parent.GetChild(1).GetComponent<UnityEngine.UI.Text>().color = Color.white;
                frontsprite.sprite = sp;
                frontsprite.color = new UnityEngine.Color(1f, 1f, 1f, 0f);
                frontsprite.gameObject.SetActive(true);
                FadeIn = true;
                FadeOut = false;
                fadeprogress = 0f;
            }
            catch (Exception ex)
            {
                RGDebug.LogError(ex);
            }
        }
        bool FadeOut;
        public void OnPointerExit(PointerEventData eventData)
        {
            try
            {
                //transform.parent.GetChild(1).GetComponent<UnityEngine.UI.Text>().color = originColor;
                AlterTitleController.Controller._backgroundRenderer.sprite = originSprite;
                FadeIn = false;
                FadeOut = true;
                fadeprogress = 0f;
                frontsprite.sprite = sp;
                frontsprite.color = new UnityEngine.Color(1f, 1f, 1f, 1f);
                frontsprite.gameObject.SetActive(true);
            }
            catch (Exception ex)
            {
                RGDebug.LogError(ex);
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
                RGDebug.LogError(ex);
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
                RGDebug.LogError(ex);
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
        AudioClipPlayer.PlayerData Cancel= new AudioClipPlayer.PlayerData() { globalType = AudioType.CANCEL, region = AudioRegion.GLOBAL, localPlayIndex=0};
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
                RGDebug.LogError(ex);
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
                RGDebug.LogError(ex);
            }
        }

    }

    public static class Extension
    {
        /// <summary>
        /// 获取一个私密词条
        /// </summary>
        /// <typeparam name="T">词条的类型 int bool这种类型</typeparam>
        /// <param name="instance">词条所在类型</param>
        /// <param name="fieldname">词条名</param>
        public static T GetPrivateField<T>(this object instance, string fieldname)
        {
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
            return (T)((object)instance.GetType().GetField(fieldname, bindingFlags).GetValue(instance));
        }

        /// <summary>
        /// 修改一个私密词条
        /// </summary>
        /// <param name="instance">词条所在类型</param>
        /// <param name="fieldname">词条名</param>
        /// <param name="value">修改值</param>
        public static void SetPrivateField(this object instance, string fieldname, object value)
        {
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
            instance.GetType().GetField(fieldname, bindingFlags).SetValue(instance, value);
        }

        /// <summary>
        /// 使用一个私密方法
        /// </summary>
        /// <typeparam name="T">原方法的返回类型（应该） void填object</typeparam>
        /// <param name="instance">方法所在类型</param>
        /// <param name="name">方法名称</param>
        /// <param name="param">方法所有参数 没有参数填null</param>
        public static T CallPrivateMethod<T>(this object instance, string name, params object[] param)
        {
            try
            {
                BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
                return (T)((object)instance.GetType().GetMethod(name, bindingFlags).Invoke(instance, param));
            }
            catch (Exception ex)
            {
                //BlueArchiveDebug.WriteError(ex, name);
                return default(T);
            }
        }

        /// <summary>
        /// 使用一个私密静态方法
        /// </summary>
        /// <typeparam name="T">原方法的返回类型（应该） void填object</typeparam>
        /// <param name="instance">方法所在类型</param>
        /// <param name="name">方法名称</param>
        /// <param name="param">方法所有参数 没有参数填null</param>
        public static T CallPrivateStaticMethod<T>(this object instance, string name, params object[] param)
        {
            BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.NonPublic;
            return (T)((object)instance.GetType().GetMethod(name, bindingFlags).Invoke(null, param));
        }
    }
}
///////////
public class LocalizeTextLoadScriptWithOutFontLoadScript : MonoBehaviour, global::ILanguageLinkedData, global::IObserver
{
    public UnityEngine.UI.Text Text
    {
        get
        {
            return base.gameObject.GetComponent<UnityEngine.UI.Text>();
        }
    }

    // Token: 0x06005296 RID: 21142 RVA: 0x00042AFF File Offset: 0x00040CFF
    private void Start()
    {
        if (!this.init)
        {
            this.init = true;
            this.SetText();
        }
    }

    // Token: 0x06005297 RID: 21143 RVA: 0x001E05BC File Offset: 0x001DE7BC
    public void SetText()
    {
        if (this.id == string.Empty)
        {
            return;
        }
        string text = global::LocalizeTextDataModel.instance.GetText(this.id);
        this.Text.text = text;
    }

    // Token: 0x06005298 RID: 21144 RVA: 0x00042B19 File Offset: 0x00040D19
    public void SetText(string id)
    {
        this.id = id;
        this.SetText();
    }

    // Token: 0x06005299 RID: 21145 RVA: 0x00042B28 File Offset: 0x00040D28
    public void SetTextForcely(string text)
    {
        this.Text.text = text;
        this.init = true;
    }

    // Token: 0x0600529B RID: 21147 RVA: 0x00040E16 File Offset: 0x0003F016
    private void OnEnable()
    {
        global::Notice.instance.Observe(global::NoticeName.LanaguageChange, this);
    }

    // Token: 0x0600529C RID: 21148 RVA: 0x0002CE41 File Offset: 0x0002B041
    private void OnDisable()
    {
        global::Notice.instance.Remove(global::NoticeName.LanaguageChange, this);
    }

    // Token: 0x0600529D RID: 21149 RVA: 0x00042B46 File Offset: 0x00040D46
    public void OnLanguageChanged()
    {
        this.SetText();
    }

    // Token: 0x0600529E RID: 21150 RVA: 0x00042B4E File Offset: 0x00040D4E
    public void OnNotice(string notice, params object[] param)
    {
        if (notice == global::NoticeName.LanaguageChange)
        {
            this.OnLanguageChanged();
        }
    }

    public string id = string.Empty;
    private bool init;
}
///////////