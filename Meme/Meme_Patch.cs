using Assets.Scripts.UI.Utils;
using DG.Tweening;
using GameStatusUI;
using Harmony;
using NewGameMode.Diffculty;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static Mono.Security.X509.X520;
using static UnityEngine.Analytics.EnumCase;

namespace NewGameMode
{
    public class Meme_Patch
    {
        public static GameMode rougeLike = (GameMode)666666;
        public static GameObject memeScene = new GameObject();
        public Meme_Patch(HarmonyInstance instance)
        {
            int num = 0;
            try
            {
                //加载模因
                instance.Patch(typeof(GameStaticDataLoader).GetMethod("LoadStaticData"), null, new HarmonyMethod(typeof(Meme_Patch).GetMethod("LoadAllInfo"))); num++;
                //设置控制台，也写了一些奇思的指令
                instance.Patch(typeof(ConsoleScript).GetMethod("GetHmmCommand", AccessTools.all), new HarmonyMethod(typeof(Meme_Patch).GetMethod("GetAllCommand", AccessTools.all)), null, null); num++;
                //调整UI
                instance.Patch(typeof(GameManager).GetMethod("StartStage", AccessTools.all), null, new HarmonyMethod(typeof(Meme_Patch).GetMethod("InitMemeScene")), null);
                instance.Patch(typeof(GameManager).GetMethod("RestartGame", AccessTools.all), new HarmonyMethod(typeof(Meme_Patch).GetMethod("TurnRestartToMemeScene")), null, null);
                instance.Patch(typeof(EscapeUI).GetMethod("OpenWindow", AccessTools.all), null, new HarmonyMethod(typeof(Meme_Patch).GetMethod("TurnRestartToMemeSceneText")), null);

                // 扳机
                instance.Patch(typeof(GameManager).GetMethod("StartStage"), null, new HarmonyMethod(typeof(Meme_Patch).GetMethod("Meme_OnStageStart"))); num++;
                instance.Patch(typeof(GameManager).GetMethod("Release", AccessTools.all), null, new HarmonyMethod(typeof(Meme_Patch).GetMethod("Meme_OnStageRelease"))); num++;
                instance.Patch(typeof(EquipmentModel).GetMethod("OnPrepareWeapon"), new HarmonyMethod(typeof(Meme_Patch).GetMethod("Meme_OnPrepareWeapon")), null); num++;
                instance.Patch(typeof(EquipmentModel).GetMethod("OnCancelWeapon"), new HarmonyMethod(typeof(Meme_Patch).GetMethod("Meme_OnCancelWeapon")), null); num++;
                instance.Patch(typeof(WeaponModel).GetMethod("OnAttack"), new HarmonyMethod(typeof(Meme_Patch).GetMethod("Meme_OnAttack")), null); num++;
                instance.Patch(typeof(WeaponModel).GetMethod("OnEndAttackCycle"), new HarmonyMethod(typeof(Meme_Patch).GetMethod("Meme_OnEndAttackCycle")), null); num++;
                // FUCK YOU 1.0.9.1
                //trans生效吧求你了
                instance.Patch(typeof(EquipmentScriptBase).GetMethod(nameof(EquipmentScriptBase.OnKillMainTarget)),null, new HarmonyMethod(typeof(Meme_Patch).GetMethod("Meme_OnKillTargetWorker")), null); num++;
                instance.Patch(typeof(EquipmentScriptBase).GetMethod(nameof(EquipmentScriptBase.OnGiveDamage)), null, new HarmonyMethod(typeof(Meme_Patch).GetMethod("Meme_OnGiveDamage")), null); num++;
                instance.Patch(typeof(EquipmentScriptBase).GetMethod(nameof(EquipmentScriptBase.OnGiveDamageAfter)), null, new HarmonyMethod(typeof(Meme_Patch).GetMethod("Meme_OnGiveDamageAfter")), null); num++;

                //instance.Patch(typeof(WorkerModel).GetMethod(nameof(WorkerModel.TakeDamage)), new HarmonyMethod(typeof(Meme_Patch).GetMethod("Meme_WorkerTakeDamage")), new HarmonyMethod(typeof(Meme_Patch).GetMethod("Meme_WorkerTakeDamage_After")), null); num++;
                //instance.Patch(typeof(CreatureModel).GetMethod(nameof(CreatureModel.TakeDamage)), new HarmonyMethod(typeof(Meme_Patch).GetMethod("Meme_CreatureTakeDamage")), new HarmonyMethod(typeof(Meme_Patch).GetMethod("Meme_OnCreatureTakeDamage_After")), null); num++;

            }
            catch (Exception ex)
            {
                Harmony_Patch.LogError("MemePatch error: " + num + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }
        public static void LoadAllInfo()
        {
            try
            {
                MemeManager.LoadAllInfo();
            }
            catch (Exception ex)
            {
                Harmony_Patch.YKMTLogInstance.Error(ex);
            }
        }

        public static bool GetAllCommand(string cmd, ref string __result)
        {
            try
            {
                string[] array = cmd.Split(new char[]
                {
                    ' '
                });

                if (array.Length != 4 && array.Length != 3)
                {
                    return true;
                }

                string flag = array[0].ToLower();
                string type = array[1].ToLower().Trim();//指定操作目标，比如模因和奇思
                string type2 = array[2].ToLower().Trim();//指定具体要做的操作，比如添加和移除
                int value = 0;
                if (array.Length == 4)
                {
                    value = Convert.ToInt32(array[3].ToLower().Trim());
                }


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
                        else if (type2 == "list")
                        {
                            if (MemeManager.instance.current_list.Count == 0)
                            {
                                Harmony_Patch.LogWarning("HasNoMeme");
                                __result = "";
                                return false;
                            }
                            for (int i = 0; i < MemeManager.instance.current_list.Count; i++)
                            {
                                string name;
                                MemeManager.instance.current_list[i].metaInfo.localizeData.TryGetValue("name", out name);
                                Harmony_Patch.YKMTLogInstance.Info("name:" + name + "    " + "id:" + MemeManager.instance.current_list[i].metaInfo.id);
                            }

                            __result = "";
                            return false;
                        }
                    }
                    else if (type == "wonder")
                    {
                        if (type2 == "add")
                        {
                            WonderModel.instance.Add(value);
                            __result = "";
                            return false;
                        }
                        else if (type2 == "pay")
                        {
                            WonderModel.instance.Pay(value);
                            __result = "";
                            return false;
                        }
                        else if (type2 == "set")
                        {
                            WonderModel.instance.money = value;
                            __result = "";
                            return false;
                        }
                        else if (type2 == "show")
                        {
                            Harmony_Patch.LogInfo("Now wonder:" + WonderModel.instance.money.ToString());
                            __result = "";
                            return false;
                        }
                    }
                    else if (type == "difficult")
                    {
                        if (type2 == "set")
                        {
                            DifficultyManager.SetDifficulty(value);
                            Harmony_Patch.LogInfo("Now Difficulty: " + DifficultyManager.GetNowDifficultyIndex().ToString());
                            __result = "";
                            return false;
                        }
                        else if (type2 == "show")
                        {
                            Harmony_Patch.LogInfo("Now Difficulty: " + DifficultyManager.GetNowDifficultyIndex().ToString());
                            __result = "";
                            return false;
                        }
                    }
                    else if(type == "log")
                    {
                        if (type2 == "remove")
                        {
                            GameStatusUI.GameStatusUI.Window.logController.script.DeleteAll();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Harmony_Patch.YKMTLogInstance.Error(ex);
            }
            return true;
        }
        public static void InitMemeScene()
        {
            try
            {
                //对于memeScene的基础设置
                AssetBundle bundle = AssetBundle.LoadFromFile(Harmony_Patch.path + "/AssetsBundle/universal_meme_scene");
                memeScene = UnityEngine.Object.Instantiate(bundle.LoadAsset<GameObject>("UniversalMemeScene"));
                memeScene.transform.SetParent(EscapeUI.instance.MiddleAreaControl.transform.parent);
                memeScene.transform.localPosition = new Vector3(0, 0, 0);
                memeScene.transform.localScale = new Vector3(0, 0, 1);
                memeScene.SetActive(false);
                bundle.Unload(false);//关闭AB包，但是保留已加载的资源

                //对于子对象的详细设置，例如哪些按钮应当播放哪些音效，以及对于文字的设置
                //离开按钮
                GameObject exitButton = memeScene.transform.Find("ExitButton").gameObject;
                UniversalButtonIntereaction btIn1 = exitButton.AddComponent<UniversalButtonIntereaction>();
                btIn1.pointerEnterSound = false;
                exitButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => {
                    btIn1.Click(true, false, false);
                    memeScene.transform.DOScale(new Vector3(0, 0, 1), 1).SetEase(Ease.OutExpo).SetUpdate(true).OnComplete(() => {
                        memeScene.SetActive(false);
                    });
                });
                exitButton.transform.Find("Text").gameObject.AddComponent<LocalizeTextLoadScriptWithOutFontLoadScript>().id = "Meme_ExitButton";
                exitButton.transform.Find("Text").gameObject.AddComponent<FontLoadScript>();
                //单个模因按钮
                GameObject singleMemeButton = memeScene.transform.Find("MemeButtons").Find("Buttons").GetChild(0).gameObject;
                UniversalButtonIntereaction btIn2 = singleMemeButton.AddComponent<UniversalButtonIntereaction>();
                btIn2.pointerEnterSound = true;
                singleMemeButton.transform.Find("Text").gameObject.AddComponent<LocalizeTextLoadScriptWithOutFontLoadScript>();
                singleMemeButton.transform.Find("Text").gameObject.AddComponent<FontLoadScript>();
                //设置模因按钮的一部分点击逻辑，例如展示详情，详情的另一部分写在memeManager的CreateMeme里
                singleMemeButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => {
                    btIn2.Click(true, false, true);
                    memeScene.transform.Find("WonderandDetail").Find("Detail").gameObject.SetActive(true);
                    memeScene.transform.Find("WonderandDetail").Find("Detail").gameObject.transform.localScale = new Vector3(0, 0, 1);
                    memeScene.transform.Find("WonderandDetail").Find("Detail").gameObject.transform.DOScale(new Vector3(1, 1, 1), 0.5f).SetEase(Ease.OutExpo).SetUpdate(true);
                });
                //详情页的文本设置
                GameObject detail = memeScene.transform.Find("WonderandDetail").Find("Detail").gameObject;
                detail.transform.Find("Name").gameObject.AddComponent<LocalizeTextLoadScriptWithOutFontLoadScript>();
                detail.transform.Find("Name").gameObject.AddComponent<FontLoadScript>();
                detail.transform.Find("Desc").gameObject.AddComponent<LocalizeTextLoadScriptWithOutFontLoadScript>();
                detail.transform.Find("Desc").gameObject.AddComponent<FontLoadScript>();
                detail.transform.Find("ScrollSpecialDesc").Find("SpecialDesc").gameObject.AddComponent<LocalizeTextLoadScriptWithOutFontLoadScript>();
                detail.transform.Find("ScrollSpecialDesc").Find("SpecialDesc").gameObject.AddComponent<FontLoadScript>();
                //涉及价格的不能加那俩组件
                /*
                detail.transform.Find("BuyButton").Find("Text").gameObject.AddComponent<LocalizeTextLoadScriptWithOutFontLoadScript>();
                detail.transform.Find("BuyButton").Find("Text").gameObject.AddComponent<FontLoadScript>();
                */
                //奇思设置
                /*
                GameObject wonder = memeScene.transform.Find("WonderandDetail").Find("Wonder").gameObject;
                wonder.transform.Find("Text").gameObject.AddComponent<LocalizeTextLoadScriptWithOutFontLoadScript>();
                wonder.transform.Find("Text").gameObject.AddComponent<UpdateWonder>();
                
                //购买按钮设置
                GameObject buy = memeScene.transform.Find("WonderandDetail").Find("Detail").Find("BuyButton").gameObject;
                buy.transform.Find("Text").gameObject.AddComponent<LocalizeTextLoadScriptWithOutFontLoadScript>();
                buy.transform.Find("Text").gameObject.GetComponent<Text>().text = LocalizeTextDataModel.instance.GetText("Meme_Wonder");
                //刷新按钮设置
                GameObject refresh = memeScene.transform.Find("RefreshButton").gameObject;
                refresh.transform.Find("Text").gameObject.AddComponent<LocalizeTextLoadScriptWithOutFontLoadScript>();
                refresh.transform.Find("Text").gameObject.GetComponent<Text>().text = LocalizeTextDataModel.instance.GetText("Meme_Wonder");
                */

            }
            catch (Exception ex)
            {
                Harmony_Patch.YKMTLogInstance.Error(ex);
            }
        }

        public static void TurnRestartToMemeSceneText()
        {
            if (GlobalGameManager.instance.gameMode == rougeLike)
            {
                EscapeUI.instance.MiddleAreaControl.transform.GetChild(0).Find("ButtonText").GetComponent<Assets.Scripts.UI.Utils.LocalizeTextLoadScript>().id = "Rouge_EscapeUI_MemeScene";

            }
        }

        public static bool TurnRestartToMemeScene()
        {
            
            if (GlobalGameManager.instance.gameMode == rougeLike)
            {
                Harmony_Patch.YKMTLogInstance.Info("ShowMemeScene");
                try
                {
                    memeScene.SetActive(true);
                    memeScene.transform.localScale = new Vector3(0, 0, 1);
                    memeScene.transform.DOScale(new Vector3(1, 1, 1), 1).SetEase(Ease.OutExpo).SetUpdate(true);

                    memeScene.transform.Find("WonderandDetail").Find("Detail").Find("BuyButton").gameObject.SetActive(false);//查看模因界面时，隐藏购买按钮

                    if (MemeManager.instance.current_list.Count == 0)
                    {
                        Harmony_Patch.LogWarning("NoMeme");
                    }
                    if (MemeManager.instance.current_list.Count != 0)
                    {
                        /*
                        int need_change = memeScene.transform.Find("MemeButtons").Find("Buttons").childCount - MemeManager.instance.current_list.Count;
                        if (need_change > 0)//如果模因按钮数量过多
                        {
                            for (int i = 0; i < need_change; i++)
                            {
                                GameObject.Destroy(memeScene.transform.Find("MemeButtons").Find("Buttons").GetChild(memeScene.transform.Find("MemeButtons").Find("Buttons").childCount - 1).gameObject);
                            }
                        }
                        else if (need_change < 0)//如果模因按钮数量不足
                        {

                        }

                        for (int i = 0; i < MemeManager.instance.current_list.Count; i++)
                        {
                            //加载名称
                            string name_id;
                            MemeManager.instance.current_list[i].metaInfo.localizeData.TryGetValue("name", out name_id);
                            memeScene.transform.Find("MemeButtons").Find("Buttons").GetChild(i).Find("Text").gameObject.AddComponent<LocalizeTextLoadScriptWithOutFontLoadScript>().id = name_id;
                            memeScene.transform.Find("MemeButtons").Find("Buttons").GetChild(i).Find("Text").gameObject.AddComponent<FontLoadScript>();
                            //加载贴图
                            memeScene.transform.Find("MemeButtons").Find("Buttons").GetChild(i).Find("Image").gameObject.GetComponent<UnityEngineImage>().sprite = 
                        }
                        */
                        
                    }
                }
                catch (Exception ex)
                {
                    Harmony_Patch.YKMTLogInstance.Error(ex);
                }
                return false;
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
        public static void Meme_OnPrepareWeapon(UnitModel actor)
        {
            MemeManager.instance.OnPrepareWeapon(actor);
        }
        public static void Meme_OnCancelWeapon(UnitModel actor)
        {
            MemeManager.instance.OnCancelWeapon(actor);
        }
        public static void Meme_OnAttack(UnitModel actor, UnitModel target)
        {
            MemeManager.instance.OnAttackStart(actor, target);
        }
        public static void Meme_OnEndAttackCycle(WeaponModel __instance)
        {
            MemeManager.instance.OnAttackEnd(__instance.owner, __instance.currentTarget);
        }

        /*
        public static IEnumerable<CodeInstruction> Meme_OnKillTargetWorker(IEnumerable<CodeInstruction> instructions)
        {
            //要调用的方法和用于定位IL码位置的方法
            var methodInfo = MemeManager.instance.GetType().GetMethod(nameof(MemeManager.OnKillTargetWorker), BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(UnitModel), typeof(UnitModel) }, null);
            var rawMethodInfo = typeof(EquipmentScriptBase).GetMethod(nameof(EquipmentScriptBase.OnKillMainTarget), BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(UnitModel), typeof(UnitModel) }, null);

            //要调用的方法所属的实例，如果方法不是static那么这个是必需的
            FieldInfo instance = typeof(MemeManager).GetField("_instance", BindingFlags.NonPublic | BindingFlags.Static);
            FieldInfo currentTarget = typeof(EquipmentModel).GetField("currentTarget", BindingFlags.Public | BindingFlags.Instance);

            //原IL和新IL
            var codes = instructions.ToList();
            List<CodeInstruction> newCodes = new List<CodeInstruction>();
            IEnumerable<CodeInstruction> newCodes_copy;

            foreach (var code in codes)
            {
                newCodes.Add(code);
                if (code.opcode == OpCodes.Callvirt)
                {
                    if (code.operand is MethodInfo)
                    //if ((MethodInfo)code.operand == rawMethodInfo)
                    {
                        if (((MethodInfo)code.operand).Equals(rawMethodInfo))
                        {
                            newCodes.Add(new CodeInstruction(OpCodes.Ldsfld, instance));//在调用一个方法前，如果这个方法不是静态方法，那么必须先向栈内压入方法的实例
                            newCodes.Add(new CodeInstruction(OpCodes.Ldarg_1));//从被patch的方法中获取第一个参数并压入，按照从左至右，成为第一个参数
                            newCodes.Add(new CodeInstruction(OpCodes.Ldarg_0));//将WeaponModel实例压入栈内。由于不属于UnitModel，它不会成为方法的参数
                            newCodes.Add(new CodeInstruction(OpCodes.Ldfld, currentTarget));//从WeaponModel实例中获取target并压入，按照从左至右，成为第二个参数
                            newCodes.Add(new CodeInstruction(OpCodes.Call, methodInfo));//调用OnKillMainTarget(actor,target)

                        }
                    }
                }
            }

            newCodes_copy = newCodes;
            return newCodes_copy;

        }

        public static IEnumerable<CodeInstruction> Meme_OnGiveDamage(IEnumerable<CodeInstruction> instructions)
        {
            //要调用的方法和用于定位IL码位置的方法
            var methodInfo = MemeManager.instance.GetType().GetMethod(nameof(MemeManager.OnGiveDamage), BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(UnitModel), typeof(UnitModel) }, null);
            var rawMethodInfo = typeof(EquipmentScriptBase).GetMethod(nameof(EquipmentScriptBase.OnGiveDamage), BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(UnitModel), typeof(UnitModel), typeof(DamageInfo).MakeByRefType() }, null);

            //要调用的方法所属的实例，如果方法不是static那么这个是必需的
            FieldInfo instance = typeof(MemeManager).GetField("_instance", BindingFlags.NonPublic | BindingFlags.Static);
            FieldInfo currentTarget = typeof(EquipmentModel).GetField("currentTarget", BindingFlags.Public | BindingFlags.Instance);

            //原IL和新IL
            var codes = instructions.ToList();
            List<CodeInstruction> newCodes = new List<CodeInstruction>();
            IEnumerable<CodeInstruction> newCodes_copy;

            bool findRawMethod = false;//如果定位用方法被调用后没有立刻结束这一行，而是还有一句IL码，则借此判断，并在下一句IL码之后插入
            foreach (var code in codes)
            {
                newCodes.Add(code);
                if (code.opcode == OpCodes.Call)//记得检查用于定位IL码的方法到底是call还是callvirt
                {
                    if (code.operand is MethodInfo)
                    //if ((MethodInfo)code.operand == rawMethodInfo)
                    {
                        if (((MethodInfo)code.operand).Equals(rawMethodInfo))
                        {
                            
                            newCodes.Add(new CodeInstruction(OpCodes.Ldsfld, instance));//在调用一个方法前，如果这个方法不是静态方法，那么必须先向栈内压入方法的实例
                            newCodes.Add(new CodeInstruction(OpCodes.Ldarg_1));//从被patch的方法中获取第一个参数并压入，按照从左至右，成为第一个参数
                            newCodes.Add(new CodeInstruction(OpCodes.Ldarg_0));//将WeaponModel实例压入栈内。由于不属于UnitModel，它不会成为方法的参数
                            newCodes.Add(new CodeInstruction(OpCodes.Ldfld, currentTarget));//从WeaponModel实例中获取target并压入，按照从左至右，成为第二个参数
                            newCodes.Add(new CodeInstruction(OpCodes.Ldloca_S, 7));
                            newCodes.Add(new CodeInstruction(OpCodes.Call, methodInfo));//调用OnGiveDamage(actor,target,dmg)

                        }
                    }
                }
            }

            newCodes_copy = newCodes;
            return newCodes_copy;

        }
        */
        public static void Meme_OnKillTargetWorker(UnitModel actor, UnitModel target)
        {
            MemeManager.instance.OnKillTargetWorker(actor, target);
        }
        public static void Meme_OnGiveDamage(UnitModel actor, UnitModel target, ref DamageInfo dmg)
        {
            MemeManager.instance.OnGiveDamage(actor, target, ref dmg);
        }
        public static void Meme_OnGiveDamageAfter(UnitModel actor, UnitModel target, DamageInfo dmg)
        {
            MemeManager.instance.OnGiveDamageAfter(actor, target, dmg);
        }
        public static void Meme_WorkerTakeDamage(WorkerModel __instance, UnitModel actor, DamageInfo dmg)
        {
            MemeManager.instance.WorkerTakeDamage(actor, __instance, dmg);
        }
        public static void Meme_OnWorkerTakeDamage_After(WorkerModel __instance, UnitModel actor, float num, RwbpType type)//需要用trans
        {
            MemeManager.instance.OnWorkerTakeDamage_After(num, actor, __instance, type);
        }
        public static void Meme_CreatureTakeDamage(CreatureModel __instance, UnitModel actor, DamageInfo dmg)
        {
            MemeManager.instance.CreatureTakeDamage(actor, __instance, dmg);
        }
        public static void Meme_OnCreatureTakeDamage_After(CreatureModel __instance, UnitModel actor, float num, RwbpType type)//需要用trans
        {
            MemeManager.instance.OnCreatureTakeDamage_After(num, actor, __instance, type);
        }
    }
}
