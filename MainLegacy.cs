using Harmony;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

namespace NewGameMode
{
    public class Harmony_Patch
    {
        public static string path = Path.GetDirectoryName(Uri.UnescapeDataString(new UriBuilder(Assembly.GetExecutingAssembly().CodeBase).Path));
        public static string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/LOG";

        public static GameMode rougeLike = (GameMode)666666;

        public Harmony_Patch()
        {
            try
            {
                HarmonyInstance harmony = HarmonyInstance.Create("ykmt.NewGameMode");
                //加载数据
                harmony.Patch(typeof(GlobalGameManager).GetMethod("SaveGlobalData", AccessTools.all), new HarmonyMethod(typeof(Harmony_Patch).GetMethod("SaveGlobalData")), null, null);
                harmony.Patch(typeof(GlobalGameManager).GetMethod("SaveData", AccessTools.all), new HarmonyMethod(typeof(Harmony_Patch).GetMethod("SaveDayData")), null, null);
                //harmony.Patch(typeof(GlobalGameManager).GetMethod("LoadGlobalData", AccessTools.all), null, new HarmonyMethod(typeof(Harmony_Patch).GetMethod("WriteOriginGlobalData")), null);
                //harmony.Patch(typeof(GlobalGameManager).GetMethod("LoadData", AccessTools.all), null, new HarmonyMethod(typeof(Harmony_Patch).GetMethod("WriteOriginDayData")), null);
                //harmony.Patch(typeof(GlobalGameManager).GetMethod("LoadGlobalData", AccessTools.all), new HarmonyMethod(typeof(Harmony_Patch).GetMethod("LoadRougelikeGlobalData")), null, null);
                //harmony.Patch(typeof(GlobalGameManager).GetMethod("LoadData", AccessTools.all), null, new HarmonyMethod(typeof(Harmony_Patch).GetMethod("LoadOriginDayData")), null);
                //创建按钮
                harmony.Patch(typeof(AlterTitleController).GetMethod("Start", AccessTools.all), null, new HarmonyMethod(typeof(Harmony_Patch).GetMethod("NewGameModeButton_Start")), null);
                harmony.Patch(typeof(GameManager).GetMethod("ClearStage", AccessTools.all), null, new HarmonyMethod(typeof(Harmony_Patch).GetMethod("OnClearStage")), null);
                harmony.Patch(typeof(GameManager).GetMethod("RestartGame", AccessTools.all), new HarmonyMethod(typeof(Harmony_Patch).GetMethod("NoRestartAndCheckPoint")), null, null);
                harmony.Patch(typeof(GameManager).GetMethod("ReturnToCheckPoint", AccessTools.all), new HarmonyMethod(typeof(Harmony_Patch).GetMethod("NoRestartAndCheckPoint")), null, null);
                //harmony.Patch(typeof(AlterTitleController).GetMethod("Awake", AccessTools.all), null, new HarmonyMethod(typeof(Harmony_Patch).GetMethod("NewGameModeButton_Awake")), null);
                //harmony.Patch(typeof(AlterTitleController).GetMethod("OnClickButton", AccessTools.all), null, new HarmonyMethod(typeof(Harmony_Patch).GetMethod("NewGameModeButton_OnClick")), null);
            }
            catch (Exception ex)
            {
                File.WriteAllText(path + "/BaseHpError.txt", ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        public static void NewGameModeButton_Start(AlterTitleController __instance)//记得改按钮文字
        {
            try
            {
                //AssetBundle bundle = AssetBundle.Instantiate()
                __instance._buttonRoot.transform.Find("ButtonLayout").transform.localPosition += 400 * Vector3.up;

                //以下为开始按钮的两种写法：复制原按钮加以修改，和复制原按钮的组件并拼到一起。
                
                GameObject new_game_start_button = UnityEngine.Object.Instantiate(__instance._buttonRoot.transform.Find("ButtonLayout").GetChild(1).gameObject, __instance._buttonRoot.transform.Find("ButtonLayout").GetChild(2).gameObject.transform.parent);
                

                //GameObject new_game_start_button = new GameObject();
                //GameObject image = UnityEngine.Object.Instantiate(__instance._buttonRoot.transform.Find("ButtonLayout").GetChild(1).GetChild(0)).gameObject;
                //GameObject text = UnityEngine.Object.Instantiate(__instance._buttonRoot.transform.Find("ButtonLayout").GetChild(1).GetChild(1)).gameObject;
                //image.transform.SetParent(new_game_start_button.transform, false);
                //text.transform.SetParent(new_game_start_button.transform, false);

                //new_game_start_button.AddComponent<UnityEngine.UI.Button>();
                new_game_start_button.transform.SetParent(__instance._buttonRoot.transform.Find("ButtonLayout"));
                new_game_start_button.transform.GetChild(1).GetComponent<UnityEngine.UI.Text>().text = "探索支部";

                new_game_start_button.transform.localPosition = __instance._buttonRoot.transform.Find("ButtonLayout").GetChild(1).localPosition;
                new_game_start_button.transform.localScale = __instance._buttonRoot.transform.Find("ButtonLayout").GetChild(1).localScale;
                

                new_game_start_button.transform.localPosition += 400 * Vector3.up;

                UnityEngine.Object.Destroy(new_game_start_button.GetComponent<EventTrigger>());
                new_game_start_button.GetComponent<Button>().onClick.RemoveAllListeners();
                new_game_start_button.GetComponent<Button>().onClick.AddListener(delegate
                {
                    //CallContinueGame_Rougelike();
                    CallNewGame_Rougelike();
                });
                
                

                //以下为继续按钮的写法：复制并加以修改。
                /*
                GameObject new_game_continue_button = UnityEngine.Object.Instantiate(__instance._buttonRoot.transform.Find("ButtonLayout").GetChild(1).gameObject, __instance._buttonRoot.transform.Find("ButtonLayout").GetChild(2).gameObject.transform.parent);
                new_game_continue_button.transform.SetParent(__instance._buttonRoot.transform);
                new_game_continue_button.transform.GetChild(1).GetComponent<Text>().text = "继续探索";
                new_game_continue_button.transform.GetChild(0).localScale *= 100;
                new_game_continue_button.transform.GetChild(0).GetComponent<Image>().sprite = __instance._buttonRoot.transform.Find("ButtonLayout").GetChild(1).GetChild(0).GetComponent<Image>().sprite;
                new_game_continue_button.transform.GetChild(0).GetComponent<Image>().color = Color.white;
                new_game_continue_button.transform.localPosition += 400*Vector3.up;
                File.WriteAllText(path + "/ButtonStartError1.txt", "1");
                UnityEngine.Object.Destroy(new_game_continue_button.GetComponent<EventTrigger>());
                File.WriteAllText(path + "/ButtonStartError1.txt", "2");
                UnityEngine.Object.Destroy(new_game_continue_button.GetComponent<Button>());
                File.WriteAllText(path + "/ButtonStartError1.txt", "3");
                new_game_continue_button.AddComponent<Button>();
                File.WriteAllText(path + "/ButtonStartError1.txt", "4");
                //new_game_continue_button.GetComponent<Button>().
                new_game_continue_button.GetComponent<Button>().targetGraphic = new_game_continue_button.transform.GetChild(0).GetComponent<Image>();
                File.WriteAllText(path + "/ButtonStartError1.txt", "5");
                new_game_continue_button.GetComponent<Button>().onClick.AddListener(delegate
                {
                    CallContinueGame_Rougelike();
                });
                */

                //以下为复制按钮的写法：新建一个全新按钮。
                
                GameObject new_game_continue_button = new GameObject();
                new_game_continue_button.transform.SetParent(__instance._buttonRoot.transform);
                new_game_continue_button.transform.localPosition += 400 * Vector3.up;
                new_game_continue_button.transform.localPosition += 1200 * Vector3.right;
                new_game_continue_button.transform.localScale *= 0.1f;

                Button button = new_game_continue_button.AddComponent<Button>();
                UnityEngine.UI.Image image = new_game_continue_button.AddComponent<UnityEngine.UI.Image>();
                UnityEngine.UI.Text text = new_game_continue_button.AddComponent<UnityEngine.UI.Text>();

                Texture2D texture2 = new Texture2D(1, 1);
                texture2.LoadImage(File.ReadAllBytes(path + "/Sprite/begin.png"));


                image.sprite = Sprite.Create(texture2, new Rect(0f, 0f, texture2.width, texture2.height), new Vector2(0.5f, 0.5f));
                //text.text = "继续探索";
                button.targetGraphic = image;
                button.onClick.AddListener(delegate
                {
                    CallContinueGame_Rougelike();
                });
                
            }
            catch (Exception ex)
            {
                File.WriteAllText(path + "/ButtonStartError.txt", ex.Message + Environment.NewLine + ex.StackTrace);
            }

        }

        
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
        

        public static void CallNewGame_Rougelike()
        {
            File.AppendAllText(path + "/NewRouge.txt", "1");
            GlobalGameManager.instance.gameMode = rougeLike;
            global::GlobalGameManager.instance.isPlayingTutorial = false;
            CreatureGenerate.CreatureGenerateInfoManager.Instance.Init();
            global::GlobalGameManager.instance.InitStoryMode();
            GlobalGameManager.instance.gameMode = rougeLike;
            global::PlayerModel.instance.InitAddingCreatures();
            SetRougelikeGlobalData();
            SetRougelikeDayData();
            SaveGlobalData();
            SaveDayData();
            Extension.CallPrivateMethod<object>(AlterTitleController.Controller, "LoadUnlimitMode", null);
            GlobalGameManager.instance.gameMode = rougeLike;

        }

        public static void CallContinueGame_Rougelike()
        {
            try
            {
                File.WriteAllText(path + "/ContinueRouge.txt", "11");
                GlobalGameManager.instance.gameMode = rougeLike;
                //GlobalGameManager.instance.LoadGlobalData();
                //GlobalGameManager.instance.LoadData(SaveType.LASTDAY);
                LoadGlobalData();
                LoadDayData(global::SaveType.LASTDAY);
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
                if (!File.Exists(path + "/Save/GlobalData.dat"))
                {
                    return true;
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
                        global::CreatureManager.instance.LoadObserveData(SetRandomObserve(dic2, 2, 6, 20, 50));
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
                        float[] rate = { 0.1f, 0.3f, 0.6f, 0.9f, 1 };
                        global::InventoryModel.Instance.LoadGlobalData(SetRandomEquipment(dic3, 30, 40, rate));
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
            PlayerModel.instance.SetDay(35);
            for (int i = 0; i < 5; i++)
            {
                SefiraManager.instance.OpenSefira(SefiraEnum.MALKUT);
                SefiraManager.instance.OpenSefira(SefiraEnum.YESOD);
                SefiraManager.instance.OpenSefira(SefiraEnum.NETZACH);
                SefiraManager.instance.OpenSefira(SefiraEnum.HOD);
                SefiraManager.instance.OpenSefira(SefiraEnum.TIPERERTH1);
                SefiraManager.instance.OpenSefira(SefiraEnum.TIPERERTH2);
                SefiraManager.instance.OpenSefira(SefiraEnum.CHESED);
            }

            AgentManager.instance.customAgent.Clear();
            AgentManager.instance.Clear();

            SetRandomAgents(20, 30, 70, 90);

            //CreatureManager.instance.Clear();

            float[] rate = {0.1f, 0.3f, 0.6f, 0.9f, 1f};
            SetRandomCreatures(rate);
        }

        
        public static bool SaveGlobalData()
        {
            File.WriteAllText(path + "/SaveGlobal.txt", "1");
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
            File.WriteAllText(path + "/SaveData.txt", "1");
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

                    //else
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

        public static void LoadDayData(global::SaveType saveType)//加载天数存档
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
                        Extension.CallPrivateMethod<object>(GlobalGameManager.instance, "LoadDay", new object[] {dictionary2});
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
                }


            }
            catch (Exception ex)
            {
                File.WriteAllText(path + "/LoadDayDataError.txt", ex.Message + Environment.NewLine + ex.StackTrace);
            }
            return;
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
            File.WriteAllText(path + "/OnClearStage.txt", "1");
            if (global::GlobalGameManager.instance.gameMode == rougeLike)
            {
                SaveGlobalData();
                SaveDayData();
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


        /// <summary>
        /// 
        /// </summary>
        /// <param name="origin_research"></param>
        /// <param name="rate">这是获得部门科技的概率</param>
        /// <param ></param>
        /// <param 思考：要不要同时加入最大最小限制和概率？></param>
        /// <returns></returns>
        public static Dictionary<string, object> SetRandomResearch(Dictionary<string, object> origin_research, float rate)
        {
            //结构：origin_research字典套research_list列表套dic字典套id
            Dictionary<string, object> result = new Dictionary<string, object>();

            List<Dictionary<string, object>> research_list = new List<Dictionary<string, object>>();
            global::GameUtil.TryGetValue<List<Dictionary<string, object>>>(origin_research, "research", ref research_list);
            int research_count = research_list.Count;
            research_list.Clear();

            List<int> research_id_list = new List<int>() { 1, 2, 103, 3, 4, 5, 6, 7, 203, 8, 9, 10, 501, 502, 701, 702, 703, 801, 802, 803, 901, 902, 903, 1001, 1002, 1003, 10000 };

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
                stat_list.Add("stat");
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
                global::GameUtil.TryGetValue<List<global::InventoryModel.EquipmentSaveData>>(equipment, "equips", ref result_list);
                global::GameUtil.TryGetValue<List<string>>(equipment, "equipsMod", ref result_mod_list);
                long new_instance_id = 10000;//存储实例id
                result_list.Clear();//随后清除原列表

                int equip_num = UnityEngine.Random.Range(equip_min, equip_max);

                List<int> equip_id = GetAllEquipmentidList();

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
                    if (value <= rate[0])//刷到Z级
                    {
                        int random_id = UnityEngine.Random.Range(0, z_equip_id.Count);//随机选取一个id
                        equipmentSaveData.equipTypeId = z_equip_id[random_id];
                        equipmentSaveData.equipInstanceId = new_instance_id + i;
                        if (!InventoryModel.Instance.CheckEquipmentCount(z_equip_id[random_id]))//如果装备已超出自身上限
                        {
                            a_equip_id.RemoveAt(random_id);//列表中删除此id避免重复选中
                        }
                    }
                    else if (value <= rate[1])//刷到T级
                    {
                        int random_id = UnityEngine.Random.Range(0, t_equip_id.Count);//随机选取一个id
                        equipmentSaveData.equipTypeId = t_equip_id[random_id];
                        equipmentSaveData.equipInstanceId = new_instance_id + i;

                        if (!InventoryModel.Instance.CheckEquipmentCount(t_equip_id[random_id]))//如果装备已超出自身上限
                        {
                            a_equip_id.RemoveAt(random_id);//列表中删除此id避免重复选中
                        }
                    }
                    else if (value <= rate[2])//刷到H级
                    {
                        int random_id = UnityEngine.Random.Range(0, h_equip_id.Count);//随机选取一个id
                        equipmentSaveData.equipTypeId = h_equip_id[random_id];
                        equipmentSaveData.equipInstanceId = new_instance_id + i;
                        if (!InventoryModel.Instance.CheckEquipmentCount(h_equip_id[random_id]))//如果装备已超出自身上限
                        {
                            a_equip_id.RemoveAt(random_id);//列表中删除此id避免重复选中
                        }
                    }
                    else if (value <= rate[3])//刷到W级
                    {
                        int random_id = UnityEngine.Random.Range(0, w_equip_id.Count);//随机选取一个id
                        equipmentSaveData.equipTypeId = w_equip_id[random_id];
                        equipmentSaveData.equipInstanceId = new_instance_id + i;
                        if (!InventoryModel.Instance.CheckEquipmentCount(w_equip_id[random_id]))//如果装备已超出自身上限
                        {
                            a_equip_id.RemoveAt(random_id);//列表中删除此id避免重复选中
                        }
                    }
                    else if (value <= rate[4])//刷到A级
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
                result.Add("equipsMod", result_mod_list);
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
                for (int sefira_id = 1; sefira_id < 8; sefira_id++)//一直到起司全塞满
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
