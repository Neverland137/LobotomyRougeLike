using Harmony;
using NewGameMode.Diffculty;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewGameMode
{
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
                File.WriteAllText(Harmony_Patch.path + "/EAOError" + num.ToString() + ".txt", ex.Message + Environment.NewLine + ex.StackTrace);
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
                CreatureOverloadManager.instance.SetPrivateField("_qliphothOverloadMax", 4);
            }
        }

        public static void SetOverloadMultiply()
        {
            try
            {
                if (GlobalGameManager.instance.gameMode == rougeLike)
                {
                    int num = CreatureOverloadManager.instance.GetPrivateField<int>("qliphothOverloadIsolateNum");
                    var nowDifficulty = DifficultyManager.GetNowDifficulty();
                    int overloadAdder = nowDifficulty.OverloadAdder() + MemeManager.instance.OverloadAdder();
                    if (overloadlevel <= 2)
                    {
                        num = Mathf.RoundToInt(num * 0.3f);
                    }
                    else if (overloadlevel == 3)
                    {
                        num = 0;
                    }
                    else if (overloadlevel == 4)
                    {
                        num = Mathf.RoundToInt(num * 0.7f);
                    }
                    else if (overloadlevel == 5)
                    {
                    }
                    else if (overloadlevel >= 6)
                    {
                        num = Mathf.RoundToInt(num * 1.3f);
                    }
                    num += overloadAdder;
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
                    for (int i = 0; i < all_creature_list.Count; i++)
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
                        all_creature_list.Remove(remove_creature_list[i]);
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


                    ChildCreatureModel childCreatureModel = new ChildCreatureModel(creatureList.Count + 1);

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
                        Harmony_Patch.YKMTLogInstance.Error(ex);
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
                    childCreatureModel.GetMovableNode().SetDirection(UnitDirection.LEFT); num++;
                    childCreatureModel.SetActivatedState(false); num++;

                    childCreatureModel.sefira = sefira; num++;
                    childCreatureModel.sefiraNum = sefira.indexString; num++;
                    childCreatureModel.SetActivatedState(true); num++;
                    childCreatureModel.ClearCommand(); num++;
                    childCreatureModel.state = CreatureState.ESCAPE; num++;
                    childCreatureModel.baseMaxHp = childCreatureModel.metaInfo.maxHp; num++;
                    childCreatureModel.hp = (float)childCreatureModel.metaInfo.maxHp; num++;
                    childCreatureModel.SetFaction(FactionTypeList.StandardFaction.EscapedCreature); num++;
                    Notice.instance.Send(NoticeName.OnEscape, new object[]
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
            __instance.commandQueue.Execute(__instance.ForceTypeChange<CreatureModel>());
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
            bool manageStarted = GameManager.currentGameManager.ManageStarted;
            if (manageStarted)
            {
                __instance.script.OnFixedUpdate(__instance.ForceTypeChange<CreatureModel>());
            }
            bool flag6 = __instance.state == CreatureState.ESCAPE;
            if (flag6)
            {
                __instance.script.UniqueEscape();
            }
            else
            {
                bool flag7 = base.state == CreatureState.SUPPRESSED;
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
            __instance.SetFaction(FactionTypeList.StandardFaction.EscapedCreature);*/
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
                    Harmony_Patch.YKMTLogInstance.Error(ex);
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
                            Harmony_Patch.LogInfo(mission.count + "/" + mission.goal);

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
                                Harmony_Patch.LogInfo(mission.name + "  " + mission.type.ToString() + "Clear");
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

                                Harmony_Patch.LogInfo(mission.name + "  " + mission.type.ToString() + "NotClear");
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
                    Harmony_Patch.YKMTLogInstance.Error(ex);
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
                    Harmony_Patch.YKMTLogInstance.Error(ex);
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
                try
                {
                    rewardButton = UnityEngine.Object.Instantiate(bundle.LoadAsset<GameObject>("MissionRewardButton"));
                }
                catch
                {
                    Harmony_Patch.YKMTLogInstance.Error("MissionRewardButton not found");
                }
                bundle.Unload(false);
                AngelaConversationUI.instance.FadeOut = false;
                AngelaConversationUI.instance.FadeIn = false;
                rewardButton.transform.SetParent(GameStatusUI.GameStatusUI.Window.transform.Find("Canvas").transform);
                rewardButton.transform.localPosition = new Vector3(0, -330);
                rewardButton.transform.localScale = new Vector3(4, 4, 1);



                Texture2D texture2 = new Texture2D(1, 1);
                texture2.LoadImage(File.ReadAllBytes(Harmony_Patch.path + "/Sprite/AwardButton.png"));

                List<int> award_type = new List<int> { 0, 1, 2 };
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
                    Harmony_Patch.YKMTLogInstance.Error(ex);
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
                    Harmony_Patch.YKMTLogInstance.Error(ex);
                }
            }

            public static void CheckObserveMission()
            {
                try
                {
                    foreach (EXTRAMission mission in EXTRAMissionManager.instance.GetStartMission())
                    {
                        Harmony_Patch.LogInfo(mission.name + "  " + mission.type.ToString());
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
                    Harmony_Patch.YKMTLogInstance.Error(ex);
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
                this.duplicateType = BufDuplicateType.UNLIMIT;
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
}
