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
using static Mono.Security.X509.X520;

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

        /// <summary>
        /// 随机事件的概率，依次为：奖励，疯员工，任务。商店概率独立。
        /// </summary>
        public static float[] randomEventRate = { 0.25f, 0.35f, 1 };
        /// <summary>
        /// 奖励依次为：装备，员工，LOB点
        /// </summary>
        public static float[] randomAwardTypeRate = {0.3f, 0.7f, 1 };
        /// <summary>
        /// 依次为：H，W，A
        /// </summary>
        public static float[] randomAwardEquipmentRate = {0.7f, 1, 1 };
        public static int[] randomAwardAgentStat = {60, 70};
        public static int[] randomAwardLOB = {2, 4};

        /// <summary>
        /// 疯员工装备，依次为：H，W，A
        /// </summary>
        public static float[] panicAgentEquipmentRate = { 0.5f, 0.8f, 1 };
        public static int[] panicAgentStat = {90, 110};
        /// <summary>
        /// 任务依次为：进行工作，获取能源，解锁观察信息，镇压异想体
        /// </summary>
        public static float[] missionTypeRate = {0.25f, 0.5f, 0.75f, 1};//任务类型概率
        /// <summary>
        /// 出现1级任务的概率，剩下的是2级任务
        /// </summary>
        public static float missionLevel1Rate = 0.7f;//任务等级

        public static int[] missionRequireWork_Level1 = { 3, 4 };
        public static int[] missionRequireEnegry_Level1 = { 30, 50 };
        public static int[] missionRequireObserve_Level1 = { 4, 5 };
        public static int[] missionRequireSuppressHE_Level1 = { 3, 4 };
        public static int[] missionRequireSuppressWAW_Level1 = { 1, 2 };
        public static float missionRequireSuppressRate_Level1 = 0.5f;//决定镇压H还是W

        public static int[] missionRequireWork_Level2 = { 7, 9 };
        public static int[] missionRequireEnegry_Level2 = { 40, 80 };
        public static int[] missionRequireObserve_Level2 = { 8, 10 };
        public static int[] missionRequireSuppressWAW_Level2 = { 2, 3 };
        public static int[] missionRequireSuppressALEPH_Level2 = { 1, 2 };
        public static float missionRequireSuppressRate_Level2 = 0.5f;//决定镇压W还是A
        /// <summary>
        /// 奖励依次为：装备15%，员工25%，LOB点20%，奇思30%，模因10%
        /// </summary>
        public static float[] missionAwardTypeRate = { 0.15f, 0.4f, 0.6f, 0.9f, 1 };

        public static float[] missionAwardEquipmentRate_Level1 = { 0.5f, 1 };
        public static int[] missionAwardAgentStat_Level1 = { 70, 90 };
        public static int[] missionAwardLOB_Level1 = { 15, 20 };
        public static int[] missionAwardWonder_Level1 = { 100, 120 };
        public static float[] missionAwardMemeRate_Level1 = { 0.7f, 1 };

        public static float[] missionAwardEquipmentRate_Level2 = { 0.5f, 1 };
        public static int[] missionAwardAgentStat_Level2 = { 90, 110 };
        public static int[] missionAwardLOB_Level2 = { 40, 50 };
        public static int[] missionAwardWonder_Level2 = { 200, 250 };
        public static float[] missionAwardMemeRate_Level2 = { 0.3f, 1 };


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


        /// <summary>
        /// 【没做3级任务的逻辑！！！3级任务会顶掉考验】
        /// </summary>
        public static void CallRandomEvent()
        {
            try
            {
                if (GlobalGameManager.instance.gameMode == rougeLike)
                {
                    ///////////
                    int day = PlayerModel.instance.GetDay() + 1;
                    float random = Harmony_Patch.customRandom.NextFloat();//决定事件类型：奖励，疯员工，任务
                    float random2 = Harmony_Patch.customRandom.NextFloat();//决定细分类型：奖励类型，任务类型
                    float random3 = Harmony_Patch.customRandom.NextFloat();//如果是任务，则决定任务等级

                    //【3级任务没做】
                    //【3级任务没做】
                    //【3级任务没做】

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
                    //奖励事件
                    else if (random <= randomEventRate[0])
                    {
                        if (random <= randomAwardTypeRate[0])
                        {
                            Award_GetEquipment(randomAwardEquipmentRate);
                        }
                        else if (random <= randomAwardTypeRate[1])
                        {
                            Award_GetAgent(randomAwardAgentStat[0], randomAwardAgentStat[1]);
                        }
                        else if (random <= randomAwardTypeRate[2])
                        {
                            Award_GetLOB(randomAwardLOB[0], randomAwardLOB[1]);
                        }
                        else//选中奖励事件但是没有生成
                        {
                            string text = LocalizeTextDataModel.instance.GetText("RandomEvent_Fail");
                            AngelaConversationUI.instance.AddAngelaMessage(text);
                        }
                    }
                    //疯员工
                    else if (random <= randomEventRate[1]) 
                    {
                        CreatePanicAgent(panicAgentStat[0], panicAgentStat[1], panicAgentEquipmentRate);
                    }
                    //任务
                    else if(random <= randomEventRate[2])
                    {
                        //任务类型：依次为进行工作，获取能源，解锁观察信息，镇压异想体
                        if (random2 <= missionTypeRate[0])
                        {
                            //任务等级
                            if (random3 <= missionLevel1Rate)
                            {
                                RGRandomEventManager.StartMission(RGRandomEventManager.MissionRequire.WORK, Harmony_Patch.customRandom.NextInt(missionRequireWork_Level1[0], missionRequireWork_Level1[1]), 1);
                            }
                            else
                            {
                                RGRandomEventManager.StartMission(RGRandomEventManager.MissionRequire.WORK, Harmony_Patch.customRandom.NextInt(missionRequireWork_Level2[0], missionRequireWork_Level2[1]), 2);
                            }
                        }
                        else if(random2 <= missionTypeRate[1]) 
                        {
                            if (random3 <= missionLevel1Rate)
                            {
                                RGRandomEventManager.StartMission(RGRandomEventManager.MissionRequire.ENERGY, Harmony_Patch.customRandom.NextInt(missionRequireEnegry_Level1[0], missionRequireEnegry_Level1[1] + 1), 1);
                            }
                            else
                            {
                                RGRandomEventManager.StartMission(RGRandomEventManager.MissionRequire.ENERGY, Harmony_Patch.customRandom.NextInt(missionRequireEnegry_Level2[0], missionRequireEnegry_Level2[1] + 1), 2);
                            }
                        }
                        else if(random2 <= missionTypeRate[2]) 
                        {
                            if (random3 <= missionLevel1Rate)
                            {
                                RGRandomEventManager.StartMission(RGRandomEventManager.MissionRequire.OBSERVE, Harmony_Patch.customRandom.NextInt(missionRequireObserve_Level1[0], missionRequireObserve_Level1[1] + 1), 1);
                            }
                            else
                            {
                                RGRandomEventManager.StartMission(RGRandomEventManager.MissionRequire.OBSERVE, Harmony_Patch.customRandom.NextInt(missionRequireObserve_Level2[0], missionRequireObserve_Level2[1] + 1), 2);
                            }
                        }

                        else if (random2 <= missionTypeRate[3])//镇压异想体
                        {
                            if (random3 <= missionLevel1Rate)//一级任务
                            {
                                if (Harmony_Patch.customRandom.NextFloat() <= missionRequireSuppressRate_Level1)//镇压H
                                {
                                    RGRandomEventManager.StartMission(RGRandomEventManager.MissionRequire.SUPPRESS, Harmony_Patch.customRandom.NextInt(missionRequireSuppressHE_Level1[0], missionRequireSuppressHE_Level1[1] + 1), 1, 3);
                                }
                                else//镇压W
                                {
                                    RGRandomEventManager.StartMission(RGRandomEventManager.MissionRequire.SUPPRESS, Harmony_Patch.customRandom.NextInt(missionRequireSuppressWAW_Level1[0], missionRequireSuppressWAW_Level1[1] + 1), 1, 4);
                                }
                            }
                            else//二级任务
                            {
                                if (Harmony_Patch.customRandom.NextFloat() <= missionRequireSuppressRate_Level2)//镇压W
                                {
                                    RGRandomEventManager.StartMission(RGRandomEventManager.MissionRequire.SUPPRESS, Harmony_Patch.customRandom.NextInt(missionRequireSuppressWAW_Level2[0], missionRequireSuppressWAW_Level2[1] + 1), 2, 4);
                                }
                                else//镇压A
                                {
                                    RGRandomEventManager.StartMission(RGRandomEventManager.MissionRequire.SUPPRESS, Harmony_Patch.customRandom.NextInt(missionRequireSuppressALEPH_Level2[0], missionRequireSuppressALEPH_Level2[1] + 1), 2, 5);
                                }
                            }
                        }
                        else//选中任务事件但是没有生成
                        {
                            string text = LocalizeTextDataModel.instance.GetText("RandomEvent_Fail");
                            AngelaConversationUI.instance.AddAngelaMessage(text);
                        }
                    }
                    //不生成任何事件
                    else
                    {
                        string text = LocalizeTextDataModel.instance.GetText("RandomEvent_Fail");
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
                agentModel.primaryStat.hp = Harmony_Patch.customRandom.NextInt(stat_min, stat_max);
                agentModel.primaryStat.mental = Harmony_Patch.customRandom.NextInt(stat_min, stat_max);
                agentModel.primaryStat.work = Harmony_Patch.customRandom.NextInt(stat_min, stat_max);
                agentModel.primaryStat.battle = Harmony_Patch.customRandom.NextInt(stat_min, stat_max);

                //部门
                //string[] sefiraName = { "Malkut", "Yesod", "Hod", "Netzach", "Tiphereth1" };
                Sefira[] sefiras = PlayerModel.instance.GetOpenedAreaList();
                agentModel.SetCurrentSefira(sefiras[Harmony_Patch.customRandom.NextInt(0, PlayerModel.instance.GetOpenedAreaCount())].name);

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
                        if (InventoryModel.Instance.CheckEquipmentCount(id))//如果装备未超出自身上限
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
                            
                    }
                    else if (EquipmentTypeList.instance.GetData(id).grade == "5")
                    {
                        if (InventoryModel.Instance.CheckEquipmentCount(id))//如果装备未超出自身上限
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
                }

                float random = Harmony_Patch.customRandom.NextFloat();
                if (random <= rate[0] && h_weapon_id.Count != 0)
                {
                    EquipmentModel weapon = InventoryModel.Instance.CreateEquipment(h_weapon_id[Harmony_Patch.customRandom.NextInt(0, h_weapon_id.Count)]);
                    agentModel.SetWeapon(weapon as WeaponModel);
                }
                else if (random <= rate[1] && w_weapon_id.Count != 0)
                {
                    EquipmentModel weapon = InventoryModel.Instance.CreateEquipment(w_weapon_id[Harmony_Patch.customRandom.NextInt(0, w_weapon_id.Count)]);
                    agentModel.SetWeapon(weapon as WeaponModel);
                }
                else if (random <= rate[2] && a_weapon_id.Count != 0)
                {
                    EquipmentModel weapon = InventoryModel.Instance.CreateEquipment(a_weapon_id[Harmony_Patch.customRandom.NextInt(0, a_weapon_id.Count)]);
                    agentModel.SetWeapon(weapon as WeaponModel);
                }

                random = Harmony_Patch.customRandom.NextFloat();
                if (random <= rate[0] && h_armor_id.Count != 0 && agentModel.Equipment.weapon.metaInfo.grade != "3")//只有在武器不是H的情况下才可能刷到H
                {
                    EquipmentModel armor = InventoryModel.Instance.CreateEquipment(h_armor_id[Harmony_Patch.customRandom.NextInt(0, h_armor_id.Count)]);
                    agentModel.SetArmor(armor as ArmorModel);
                }
                else if (random <= rate[1] && w_armor_id.Count != 0)
                {
                    EquipmentModel armor = InventoryModel.Instance.CreateEquipment(w_armor_id[Harmony_Patch.customRandom.NextInt(0, w_armor_id.Count)]);
                    agentModel.SetArmor(armor as ArmorModel);
                }
                else if (random <= rate[2] && a_armor_id.Count != 0)
                {
                    EquipmentModel armor = InventoryModel.Instance.CreateEquipment(a_armor_id[Harmony_Patch.customRandom.NextInt(0, a_armor_id.Count)]);
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

                    if (level >= 3)
                    {
                        string text = LocalizeTextDataModel.instance.GetText("RandomEvent_MissionStart_Level3");
                        AngelaConversationUI.instance.AddAngelaMessage(text);
                    }
                    else
                    {
                        string text = LocalizeTextDataModel.instance.GetText("RandomEvent_MissionStart");
                        AngelaConversationUI.instance.AddAngelaMessage(text);
                    }
                    
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

                //依次为：装备，员工，LOB，奇思，模因

                int[] award_type = { };

                for (int i = 0; i < Math.Min(3, mission.level + 1); i++)
                {
                    buttonList[i] = rewardButton.transform.GetChild(i).gameObject;
                    buttonList[i].AddComponent<AwardButtonInteraction>();
                    buttonList[i].GetComponent<UnityEngine.UI.Image>().sprite = Sprite.Create(texture2, new Rect(0f, 0f, texture2.width, texture2.height), new Vector2(0.5f, 0.5f));

                    float value = Harmony_Patch.customRandom.NextFloat();


                    int award = 0;
                    while (award_type.Count() <= i)
                    {
                        if (value <= missionAwardTypeRate[0] && !award_type.Contains(0))
                        {
                            award_type.Add(0);
                            award = 0;
                        }
                        else if (value <= missionAwardTypeRate[1] && !award_type.Contains(1))
                        {
                            award_type.Add(1);
                            award = 1;
                        }
                        else if (value <= missionAwardTypeRate[2] && !award_type.Contains(2))
                        {
                            award_type.Add(2);
                            award = 2;
                        }
                        else if (value <= missionAwardTypeRate[3] && !award_type.Contains(3))
                        {
                            award_type.Add(3);
                            award = 3;
                        }
                        else
                        {
                            award_type.Add(4);
                            award = 4;
                        }
                    }
                    
                    switch (award)
                    {
                        case 0:
                            buttonList[i].transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = LocalizeTextDataModel.instance.GetText("RougeLikeAwardText_Equipment" + mission.level);
                            if (mission.level == 1)
                            {
                                buttonList[i].GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate
                                {
                                    Award_GetEquipment(missionAwardEquipmentRate_Level1);
                                    DestroyAwardButton();
                                });
                            }
                            else
                            {
                                buttonList[i].GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate
                                {
                                    Award_GetEquipment(missionAwardEquipmentRate_Level2);
                                    DestroyAwardButton();
                                });
                            }
                            break;
                        case 1:
                            buttonList[i].transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = LocalizeTextDataModel.instance.GetText("RougeLikeAwardText_Agent" + mission.level);
                            if (mission.level == 1)
                            {
                                buttonList[i].GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate
                                {
                                    Award_GetAgent(missionAwardAgentStat_Level1[0], missionAwardAgentStat_Level1[1], set_sefira: false);
                                    DestroyAwardButton();
                                });
                            }
                            else
                            {
                                buttonList[i].GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate
                                {
                                    Award_GetAgent(missionAwardAgentStat_Level2[0], missionAwardAgentStat_Level2[1], set_sefira: false);
                                    DestroyAwardButton();
                                });
                            }
                            break;
                        case 2:
                            buttonList[i].transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = LocalizeTextDataModel.instance.GetText("RougeLikeAwardText_LOB" + mission.level);
                            if (mission.level == 1)
                            {
                                buttonList[i].GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate
                                {
                                    Award_GetLOB(missionAwardLOB_Level1[0], missionAwardLOB_Level1[1]);
                                    DestroyAwardButton();
                                });
                            }
                            else
                            {
                                buttonList[i].GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate
                                {
                                    Award_GetLOB(missionAwardLOB_Level2[0], missionAwardLOB_Level2[1]);
                                    DestroyAwardButton();
                                });
                            }
                            break;
                        //奇思
                        case 3:
                            buttonList[i].transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = LocalizeTextDataModel.instance.GetText("RougeLikeAwardText_Wonder" + mission.level);
                            if (mission.level == 1)
                            {
                                buttonList[i].GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate
                                {
                                    Award_GetWonder(missionAwardWonder_Level1[0], missionAwardWonder_Level1[1]);
                                    DestroyAwardButton();
                                });
                            }
                            else
                            {
                                buttonList[i].GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate
                                {
                                    Award_GetWonder(missionAwardWonder_Level2[0], missionAwardWonder_Level2[1]);
                                    DestroyAwardButton();
                                });
                            }
                            break;
                        //模因
                        case 4:
                            buttonList[i].transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = LocalizeTextDataModel.instance.GetText("RougeLikeAwardText_Meme" + mission.level);
                            if (mission.level == 1)
                            {
                                buttonList[i].GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate
                                {
                                    Award_GetMeme(missionAwardMemeRate_Level1);
                                    DestroyAwardButton();
                                });
                            }
                            else
                            {
                                buttonList[i].GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate
                                {
                                    Award_GetMeme(missionAwardMemeRate_Level2);
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
                //string text = LocalizeTextDataModel.instance.GetText("RandomEvent_AwardEnd");
                //AngelaConversationUI.instance.AddAngelaMessage(text);
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

        public static void Award_GetEquipment(float[] rate, bool angela = true, int equipCnt = 1, bool specialEquipParadiseLost = false, bool specialEquipBossBird = false)
        {
            float random = Harmony_Patch.customRandom.NextFloat();
            int level = -1;
            if (random <= rate[0])
            {
                level = 3;
            }
            else if (random <= rate[1])
            {
                level = 4;
            }
            else if (random <= rate[2])
            {
                level = 5;
            }

            List<int> id_list = new List<int>();

            if (specialEquipParadiseLost)
            {
                id_list.Add(200015);
                id_list.Add(300015);
            }
            else if (specialEquipBossBird)
            {
                id_list.Add(200038);
                id_list.Add(300038);
            }
            else
            {
                List<int> equip_id = Harmony_Patch.GetAllEquipmentidList();
                //移除失乐园和薄瞑
                equip_id.Remove(200015);
                equip_id.Remove(300015);
                equip_id.Remove(200038);
                equip_id.Remove(300038);


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
            }
            
            if (id_list.Count != 0)
            {
                for (int i = 0; i < equipCnt; i++)
                {
                    EquipmentModel equip = InventoryModel.Instance.CreateEquipment(id_list[Harmony_Patch.customRandom.NextInt(0, id_list.Count)]);
                    if (angela)
                    {
                        string text = LocalizeTextDataModel.instance.GetText("RandomAward_GetEquipment" + equip.metaInfo.Name);
                        AngelaConversationUI.instance.AddAngelaMessage(text);
                    }
                }
            }
            else 
            {
                if (angela)
                {
                    string text = LocalizeTextDataModel.instance.GetText("RandomAward_GetEquipment_Fail");
                    AngelaConversationUI.instance.AddAngelaMessage(text);
                }
            }
            
        }

        public static void Award_GetAgent(int stat_min, int stat_max, bool angela = true, bool set_sefira = true)
        {
            AgentModel agentModel = AgentManager.instance.AddSpareAgentModel();
            agentModel.primaryStat.hp = Harmony_Patch.customRandom.NextInt(stat_min, stat_max);
            agentModel.primaryStat.mental = Harmony_Patch.customRandom.NextInt(stat_min, stat_max);
            agentModel.primaryStat.work = Harmony_Patch.customRandom.NextInt(stat_min, stat_max);
            agentModel.primaryStat.battle = Harmony_Patch.customRandom.NextInt(stat_min, stat_max);

            //部门
            if (set_sefira)
            {
                Sefira[] sefiras = PlayerModel.instance.GetOpenedAreaList();
                agentModel.SetCurrentSefira(sefiras[Harmony_Patch.customRandom.NextInt(0, sefiras.Length)].name);

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
            int random = Harmony_Patch.customRandom.NextInt(lob_min, lob_max);
            MoneyModel.instance.Add(random);
            if (angela)
            {
                string text = LocalizeTextDataModel.instance.GetText("RandomAward_GetLOB");
                AngelaConversationUI.instance.AddAngelaMessage(text + Convert.ToString(random));
            }
        }

        public static void Award_GetWonder(int wonder_min, int wonder_max, bool angela = true)
        {
            int random = Harmony_Patch.customRandom.NextInt(wonder_min, wonder_max);
            WonderModel.instance.Add(random);
            if (angela)
            {
                string text = LocalizeTextDataModel.instance.GetText("RandomAward_GetWonder");
                AngelaConversationUI.instance.AddAngelaMessage(text + Convert.ToString(random));
            }
        }

        public static void Award_GetMeme(float[] rate, bool angela = true, bool level3 = false)
        {
            float random = Harmony_Patch.customRandom.NextFloat();

            List<int> nowMemeInfo = [];
            foreach (var dic in MemeManager.instance.current_dic)
            {
                nowMemeInfo.Add(dic.Value.metaInfo.id);
            }
            // 过滤掉已经拥有的模因，保留duplicate为true的模因或未获得模因
            List<int> tempMemeLevel1 = [];
            List<int> tempMemeLevel2 = [];
            List<int> tempMemeLevel3 = [];
            foreach (KeyValuePair<int, MemeInfo> kvp in MemeManager.instance.all_dic)
            {
                if (kvp.Value.duplicate || !nowMemeInfo.Contains(kvp.Key))
                {
                    if (kvp.Value.grade == 1)
                    {
                        tempMemeLevel1.Add(kvp.Key);
                    }
                    if (kvp.Value.grade == 2)
                    {
                        tempMemeLevel2.Add(kvp.Key);
                    }
                    if (kvp.Value.grade == 3)
                    {
                        tempMemeLevel3.Add(kvp.Key);
                    }
                }
            }

            int target_id = -1;
            bool success = true;
            if (level3)//3级模因
            {
                target_id = tempMemeLevel3[Harmony_Patch.customRandom.NextInt(0, tempMemeLevel3.Count)];
                MemeManager.instance.CreateMemeModel(target_id);
            }
            else if (random <= rate[0])//1级模因
            {
                target_id = tempMemeLevel1[Harmony_Patch.customRandom.NextInt(0, tempMemeLevel1.Count)];
                MemeManager.instance.CreateMemeModel(target_id);
            }
            else if (random <= rate[1])//2级
            {
                target_id = tempMemeLevel2[Harmony_Patch.customRandom.NextInt(0, tempMemeLevel2.Count)];
                MemeManager.instance.CreateMemeModel(target_id);
            }
            else //啥都没有
            {
                success = false;
            }
            if (angela)
            {
                if (success)
                {
                    string text = LocalizeTextDataModel.instance.GetText("RandomAward_GetMeme");
                    string name = "UNKNOWN";
                    MemeManager.GetMemeInfo(target_id).GetLocalizedText("name", out name);
                    AngelaConversationUI.instance.AddAngelaMessage(text + name);
                }
                else
                {
                    string text = LocalizeTextDataModel.instance.GetText("RandomAward_GetMeme_Fail");
                    AngelaConversationUI.instance.AddAngelaMessage(text);
                }
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
