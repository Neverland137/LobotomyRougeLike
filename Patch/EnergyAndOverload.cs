using Harmony;
using HPHelper;
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

        /*
        public static GameObject rewardButton = new GameObject();
        public static GameObject rewardButton1 = new GameObject();
        public static GameObject rewardButton2 = new GameObject();
        public static GameObject rewardButton3 = new GameObject();
        public static List<GameObject> buttonList = new List<GameObject>() { rewardButton1, rewardButton2, rewardButton3 };
        */

        /// <summary>
        /// 随机事件的概率，依次为：异想体，疯员工，任务。商店概率独立。
        /// </summary>
        public static int[] randomEventRate = { 0, 1000, 0 };
        /// <summary>
        /// 奖励依次为：装备，员工，LOB点
        /// </summary>
        public static int[] randomAwardTypeRate = {300, 400, 300 };
        /// <summary>
        /// 依次为：H，W，A
        /// </summary>
        public static int[] randomAwardEquipmentRate = {700, 300, 0 };
        public static int[] randomAwardAgentStat = {60, 70};
        public static int[] randomAwardLOB = {2, 4};

        public static Dictionary<CreatureModel, float> suppressCreatures = [];

        public static int[] suppressAwardMeme_LevelHE = { 1000, 0 };
        public static int[] suppressAwardMeme_LevelWAW = { 0, 1000 };
        public static int[] suppressAwardWonder_LevelHE = { 30, 50 };
        public static int[] suppressAwardWonder_LevelWAW = { 80, 100 };

        /// <summary>
        /// 疯员工装备，依次为：H，W，A
        /// </summary>
        public static int[] panicAgentEquipmentRate = { 500, 300, 200 };
        public static int[] panicAgentStat = {90, 110};
        /// <summary>
        /// 任务依次为：进行工作，获取能源，解锁观察信息，镇压异想体
        /// </summary>
        public static int[] missionTypeRate = {250, 250, 250, 250};//任务类型概率
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
        public static int[] missionAwardTypeRate = { 150, 250, 200, 300, 100 };

        public static int[] missionAwardEquipmentRate_Level1 = { 500, 500 };
        public static int[] missionAwardAgentStat_Level1 = { 70, 90 };
        public static int[] missionAwardLOB_Level1 = { 15, 20 };
        public static int[] missionAwardWonder_Level1 = { 100, 120 };
        public static int[] missionAwardMemeRate_Level1 = { 700, 300 };

        public static int[] missionAwardEquipmentRate_Level2 = { 500, 500 };
        public static int[] missionAwardAgentStat_Level2 = { 90, 110 };
        public static int[] missionAwardLOB_Level2 = { 40, 50 };
        public static int[] missionAwardWonder_Level2 = { 200, 250 };
        public static int[] missionAwardMemeRate_Level2 = { 300, 700 };

        public static int missionAwardAgentCnt = 1;
        public static int memeAwardAgentCnt = 1;
        public static int missionAwardEquipCnt = 1;
        public static int memeAwardEquipCnt = 1;
        public static int suppressAwardMemeCnt = 1;
        public static int missionAwardMemeCnt = 1;
        public static int memeAwardMemeCnt = 1;


        [HPHelper(typeof(OrdealGenInfo), "GenerateOrdeals")]
        [HPPostfix]
        public static void BlockOriginOrdeal(ref List<OrdealBase> __result)
        {
            if (GlobalGameManager.instance.gameMode == rougeLike)
            {
                int day = PlayerModel.instance.GetDay() + 1;
                __result = new List<OrdealBase>();
                /*
                if (day <= 32)
                {
                    __result.Add(new MachineNoonOrdeal());
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
                */
            }
        }
        [HPHelper(typeof(OrdealManager), "InitAvailableFixers")]
        [HPPostfix]
        public static void InitFixers()
        {
            if (GlobalGameManager.instance.gameMode == rougeLike)
            {
                OrdealManager.instance.availableFixers.Add(RwbpType.P);
            }
        }
        ///////////
        [HPHelper(typeof(StageTypeInfo), "GetEnergyNeed")]
        [HPPostfix]
        public static void EnergyNeedDecrease(ref float __result)
        {
            if (GlobalGameManager.instance.gameMode == rougeLike)
            {
                __result *= 0.25f;
            }
        }
        [HPHelper(typeof(CreatureOverloadManager), "OnStageStart")]
        [HPPostfix]
        public static void SetQliphothOverloadMax()
        {
            if (GlobalGameManager.instance.gameMode == rougeLike)
            {
                CreatureOverloadManager.instance.SetPrivateField("_qliphothOverloadMax", 4);
            }
        }

        [HPHelper(typeof(GameManager), "FixedUpdate")]
        [HPPostfix]
        public static void OnFixedUpdate()
        {
            foreach (var kvp in suppressCreatures)
            {
                suppressCreatures[kvp.Key] = kvp.Value - Time.deltaTime;
                if (kvp.Value <= 0)
                {
                    CreatureModel creature = kvp.Key;
                    suppressCreatures.Remove(kvp.Key);
                    creature.Suppressed();
                }
            }
        }

        [HPHelper(typeof(CreatureModel), "Suppressed")]
        [HPPostfix]
        public static void OnCreatureSuppressed(CreatureModel __instance)
        {
            if (suppressCreatures.ContainsKey(__instance))
            {
                suppressCreatures.Remove(__instance);
                if (__instance.metaInfo.GetRiskLevel() == RiskLevel.HE)
                {
                    Award_GetMeme(suppressAwardMeme_LevelHE, suppressAwardMemeCnt);
                    Award_GetWonder(suppressAwardWonder_LevelHE[0], suppressAwardWonder_LevelHE[1]);
                }
                else if (__instance.metaInfo.GetRiskLevel() == RiskLevel.WAW)
                {
                    Award_GetMeme(suppressAwardMeme_LevelWAW, suppressAwardMemeCnt);
                    Award_GetWonder(suppressAwardWonder_LevelWAW[0], suppressAwardWonder_LevelWAW[1]);
                }
            }
        }
        // Need Manual Patch
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
        // Need Manual Patch

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

                    float random3 = Harmony_Patch.customRandom.NextFloat();//如果是任务，则决定任务等级；如果是异想体出逃，则决定异想体等级

                    //【3级任务没做】
                    //【3级任务没做】
                    //【3级任务没做】

                    if (overloadlevel == 4)//生成考验
                    {
                        
                        if (day <= 32)
                        {
                            OrdealManager.instance.ActivateOrdeal(new MachineNoonOrdeal(), false);
                            //new MachineNoonOrdeal().OnOrdealStart();
                        }
                        else if (day <= 34)
                        {
                            OrdealManager.instance.ActivateOrdeal(new MachineNoonOrdeal(), false);
                            OrdealManager.instance.ActivateOrdeal(new OutterGodNoonOrdeal(), false);
                            //new MachineNoonOrdeal().OnOrdealStart();
                            //new OutterGodNoonOrdeal().OnOrdealStart();
                        }
                        else if (day <= 36)
                        {
                            OrdealManager.instance.ActivateOrdeal(new MachineDuskOrdeal(), false);
                            //new MachineDuskOrdeal().OnOrdealStart();
                        }
                        else if (day <= 38)
                        {
                            OrdealManager.instance.ActivateOrdeal(new FixerOrdeal(OrdealLevel.NOON), false);
                            //new FixerOrdeal(OrdealLevel.NOON).OnOrdealStart();
                            OrdealManager.instance.InitAvailableFixers();
                        }
                        else
                        {
                            OrdealManager.instance.ActivateOrdeal(new FixerOrdeal(OrdealLevel.DUSK), false);
                            //new FixerOrdeal(OrdealLevel.DUSK).OnOrdealStart();
                            OrdealManager.instance.InitAvailableFixers();
                        }
                    }
                    //事件
                    else
                    {
                        int index = Extension.WeightedRandomChoice(randomEventRate);
                        
                        switch (index)
                        {
                            case 0://H或W出逃，60秒内镇压H得一级模因+30-50奇思，120秒内镇压W得2级模因+80-100奇思
                                Harmony_Patch.logger.Info("Suppress");
                                if (random3 <= 0.5)
                                {
                                    List<CreatureModel> creatures = [];
                                    foreach (CreatureModel creature in CreatureManager.instance.GetCreatureList())
                                    {
                                        if (creature.metaInfo.GetRiskLevel() == RiskLevel.HE && creature.metaInfo.isEscapeAble)
                                        {
                                            creatures.Add(creature);
                                        }
                                    }
                                    if (creatures.Count != 0)
                                    {
                                        Harmony_Patch.logger.Info("SuppressHE");
                                        CreatureModel target = creatures[Harmony_Patch.customRandom.NextInt(0, creatures.Count)];
                                        target.SetQliphothCounter(0);
                                        suppressCreatures.Add(target, 60);
                                        AngelaConversationUI.instance.AddAngelaMessage(LocalizeTextDataModel.instance.GetText("RandomEvent_SuppressHE"));
                                    }
                                    else
                                    {
                                        AngelaConversationUI.instance.AddAngelaMessage(LocalizeTextDataModel.instance.GetText("RandomEvent_Fail"));
                                    }
                                }
                                else
                                {
                                    List<CreatureModel> creatures = [];
                                    foreach (CreatureModel creature in CreatureManager.instance.GetCreatureList())
                                    {
                                        if (creature.metaInfo.GetRiskLevel() == RiskLevel.WAW && creature.metaInfo.isEscapeAble)
                                        {
                                            creatures.Add(creature);
                                        }
                                    }
                                    if (creatures.Count != 0)
                                    {
                                        Harmony_Patch.logger.Info("SuppressWAW");
                                        CreatureModel target = creatures[Harmony_Patch.customRandom.NextInt(0, creatures.Count)];
                                        target.SetQliphothCounter(0);
                                        suppressCreatures.Add(target, 120);
                                        AngelaConversationUI.instance.AddAngelaMessage(LocalizeTextDataModel.instance.GetText("RandomEvent_SuppressWAW"));
                                    }
                                    else
                                    {
                                        AngelaConversationUI.instance.AddAngelaMessage(LocalizeTextDataModel.instance.GetText("RandomEvent_Fail"));
                                    }
                                }
                                break;
                            case 1://疯员工
                                Harmony_Patch.logger.Info("PanicAgent");
                                CreatePanicAgent(panicAgentStat[0], panicAgentStat[1], panicAgentEquipmentRate);
                                break;
                            case 2://任务
                                int index2_1 = Extension.WeightedRandomChoice(missionTypeRate);
                                switch (index2_1)//任务类型
                                {
                                    case 0://工作
                                        if (random3 <= missionLevel1Rate)
                                        {
                                            RGRandomEventManager.StartMission(RGRandomEventManager.MissionRequire.WORK, Harmony_Patch.customRandom.NextInt(missionRequireWork_Level1[0], missionRequireWork_Level1[1]), 1);
                                        }
                                        else
                                        {
                                            RGRandomEventManager.StartMission(RGRandomEventManager.MissionRequire.WORK, Harmony_Patch.customRandom.NextInt(missionRequireWork_Level2[0], missionRequireWork_Level2[1]), 2);
                                        }
                                        break;
                                    case 1:
                                        if (random3 <= missionLevel1Rate)
                                        {
                                            RGRandomEventManager.StartMission(RGRandomEventManager.MissionRequire.ENERGY, Harmony_Patch.customRandom.NextInt(missionRequireEnegry_Level1[0], missionRequireEnegry_Level1[1] + 1), 1);
                                        }
                                        else
                                        {
                                            RGRandomEventManager.StartMission(RGRandomEventManager.MissionRequire.ENERGY, Harmony_Patch.customRandom.NextInt(missionRequireEnegry_Level2[0], missionRequireEnegry_Level2[1] + 1), 2);
                                        }
                                        break;
                                    case 2:
                                        if (random3 <= missionLevel1Rate)
                                        {
                                            RGRandomEventManager.StartMission(RGRandomEventManager.MissionRequire.OBSERVE, Harmony_Patch.customRandom.NextInt(missionRequireObserve_Level1[0], missionRequireObserve_Level1[1] + 1), 1);
                                        }
                                        else
                                        {
                                            RGRandomEventManager.StartMission(RGRandomEventManager.MissionRequire.OBSERVE, Harmony_Patch.customRandom.NextInt(missionRequireObserve_Level2[0], missionRequireObserve_Level2[1] + 1), 2);
                                        }
                                        break;
                                    case 3:
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
                                        break;
                                    default://不生成任务
                                        AngelaConversationUI.instance.AddAngelaMessage(LocalizeTextDataModel.instance.GetText("RandomEvent_Fail"));
                                        break;
                                }
                                break;
                            default://不生成随机事件
                                AngelaConversationUI.instance.AddAngelaMessage(LocalizeTextDataModel.instance.GetText("RandomEvent_Fail"));
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Harmony_Patch.logger.Error(ex);
            }
        }

        public static void CreatePanicAgent(int stat_min, int stat_max, int[] rate)
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

                if (agentModel.Equipment.weapon.metaInfo.damageInfo.type == RwbpType.P)
                {
                    agentModel.forcelyPanicType = RwbpType.R;
                    agentModel.Panic();
                }
                else
                {
                    agentModel.forcelyPanicType = RwbpType.P;
                    agentModel.Panic();
                }
                agentModel.hp = agentModel.maxHp;
                agentModel.mental = 0;

                //装备
                int level = Extension.WeightedRandomChoice(rate);
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

                int weaponLoopCnt = 0;
                int armorLoopCnt = 0;
                while (agentModel.Equipment.weapon.metaInfo.id == 1)
                {
                    weaponLoopCnt++;
                    switch (level)
                    {
                        case 0:
                            if (h_weapon_id.Count != 0)
                            {
                                EquipmentModel weapon = InventoryModel.Instance.CreateEquipment(h_weapon_id[Harmony_Patch.customRandom.NextInt(0, h_weapon_id.Count)]);
                                agentModel.SetWeapon(weapon as WeaponModel);
                            }
                            break;
                        case 1:
                            if (w_weapon_id.Count != 0)
                            {
                                EquipmentModel weapon = InventoryModel.Instance.CreateEquipment(w_weapon_id[Harmony_Patch.customRandom.NextInt(0, w_weapon_id.Count)]);
                                agentModel.SetWeapon(weapon as WeaponModel);
                            }
                            break;
                        case 2:
                            if (a_weapon_id.Count != 0)
                            {
                                EquipmentModel weapon = InventoryModel.Instance.CreateEquipment(a_weapon_id[Harmony_Patch.customRandom.NextInt(0, a_weapon_id.Count)]);
                                agentModel.SetWeapon(weapon as WeaponModel);
                            }
                            break;
                    }
                    if (weaponLoopCnt >= 50)
                    {
                        break;
                    }
                }

                while (agentModel.Equipment.armor.metaInfo.id == 22)
                {
                    armorLoopCnt++;
                    switch (level)
                    {
                        case 0:
                            if (h_armor_id.Count != 0 && agentModel.Equipment.weapon.metaInfo.grade != "3")//武器非H才可能出H甲
                            {
                                EquipmentModel armor = InventoryModel.Instance.CreateEquipment(h_armor_id[Harmony_Patch.customRandom.NextInt(0, h_armor_id.Count)]);
                                agentModel.SetArmor(armor as ArmorModel);
                            }
                            break;
                        case 1:
                            if (w_armor_id.Count != 0)
                            {
                                EquipmentModel armor = InventoryModel.Instance.CreateEquipment(w_armor_id[Harmony_Patch.customRandom.NextInt(0, w_armor_id.Count)]);
                                agentModel.SetArmor(armor as ArmorModel);
                            }
                            break;
                        case 2:
                            if (a_armor_id.Count != 0)
                            {
                                EquipmentModel armor = InventoryModel.Instance.CreateEquipment(a_armor_id[Harmony_Patch.customRandom.NextInt(0, a_armor_id.Count)]);
                                agentModel.SetArmor(armor as ArmorModel);
                            }
                            break;
                    }
                    if (armorLoopCnt >= 50)
                    {
                        break;
                    }
                }
                

                panicAgentList.Add(agentModel);

                string text = LocalizeTextDataModel.instance.GetText("RandomEvent_PanicAgent");
                AngelaConversationUI.instance.AddAngelaMessage(text);
            }
            catch (Exception ex)
            {
                File.WriteAllText(Harmony_Patch.path + "/PanicAgentError.txt", ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }
        [HPHelper(typeof(AgentModel), "OnStageEnd")]
        [HPPostfix]
        public static void CheckPanicAgentAlive()
        {
            try
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
            catch (Exception ex)
            {
                Harmony_Patch.logger.Error(ex);
            }
        }



        private static EventBase _tmpEvent = new EventBase();
        public class RGRandomEventManager
        {
            public static RGRandomEventManager _instance;
            public static RGRandomEventManager instance
            {
                get
                {
                    if (_instance == null)
                    {
                        _instance = new RGRandomEventManager();
                    }
                    return _instance;
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
                    try
                    {
                        
                        mission.name = "E" + ((int)type + 1);
                        mission.type = EXTRAMissionType.Start;
                        Missionlist.Add(mission);
                        
                    }
                    catch (Exception ex)
                    {
                        Harmony_Patch.logger.Error(ex);
                    }
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
                [HPHelper(typeof(GameManager), "ClearStage")]
                [HPPostfix]
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
            [HPHelper(typeof(MissionSlot), "Refresh")]
            [HPPrefix]
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
                    Harmony_Patch.logger.Error(ex);
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
                    Harmony_Patch.logger.Error(ex);
                }
            }

            public static void EndMission(EXTRAMission mission)//启动对话窗口并给予奖励
            {
                try
                {
                    GameObject rewardButton = new GameObject();
                    GameObject rewardButton1 = new GameObject();
                    GameObject rewardButton2 = new GameObject();
                    GameObject rewardButton3 = new GameObject();
                    List<GameObject> buttonList = new List<GameObject>{rewardButton1,rewardButton2,rewardButton3};

                    mission.IsCleard = true;
                    mission.slot.Refresh();
                    mission.type = EXTRAMissionType.Cleard;
                    string text = LocalizeTextDataModel.instance.GetText("RandomEvent_MissionEnd");
                    AngelaConversationUI.instance.AddAngelaMessage(text);
                    //AngelaConversationUI.instance.FadeOut = false;
                    //AngelaConversationUI.instance.FadeIn = true;
                    AssetBundle bundle = AssetBundle.LoadFromFile(Harmony_Patch.path + "/AssetsBundle/missionrewardbutton");
                    try
                    {
                        rewardButton = UnityEngine.Object.Instantiate(bundle.LoadAsset<GameObject>("MissionRewardButton"));
                    }
                    catch
                    {
                        Harmony_Patch.logger.Error("MissionRewardButton not found");
                    }
                    bundle.Unload(false);
                    
                    rewardButton.transform.SetParent(GameStatusUI.GameStatusUI.Window.transform.Find("Canvas").transform);
                    rewardButton.transform.localPosition = new Vector3(0, -330);
                    rewardButton.transform.localScale = new Vector3(4, 4, 1);

                    Texture2D texture2 = new Texture2D(1, 1);
                    texture2.LoadImage(File.ReadAllBytes(Harmony_Patch.path + "/Sprite/AwardButton.png"));

                    //依次为：装备，员工，LOB，奇思，模因

                    List<int> award_type = new List<int>();

                    for (int i = 0; i < Math.Min(3, mission.level + 1); i++)
                    {
                        buttonList[i] = rewardButton.transform.GetChild(i).gameObject;
                        buttonList[i].AddComponent<AwardButtonInteraction>();
                        buttonList[i].GetComponent<UnityEngine.UI.Image>().sprite = Sprite.Create(texture2, new Rect(0f, 0f, texture2.width, texture2.height), new Vector2(0.5f, 0.5f));

                        float value = Harmony_Patch.customRandom.NextFloat();

                        int award = 0;
                        while (award_type.Count <= i)
                        {
                            award = Extension.WeightedRandomChoice(missionAwardTypeRate);
                            switch (award)
                            {
                                case 0:
                                    if (!award_type.Contains(0))
                                    {
                                        award_type.Add(0);
                                    }
                                    break;
                                case 1:
                                    if (!award_type.Contains(1))
                                    {
                                        award_type.Add(1);
                                    }
                                    break;
                                case 2:
                                    if (!award_type.Contains(2))
                                    {
                                        award_type.Add(2);
                                    }
                                    break;
                                case 3:
                                    if (!award_type.Contains(3))
                                    {
                                        award_type.Add(3);
                                    }
                                    break;
                                case 4:
                                    if (!award_type.Contains(4))
                                    {
                                        award_type.Add(4);
                                    }
                                    break;
                            }
                        }

                        switch (award_type[i])
                        {
                            case 0:
                                buttonList[i].transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = LocalizeTextDataModel.instance.GetText("RougeLikeAwardText_Equipment" + mission.level);
                                TooltipMouseOver toolTip = buttonList[i].AddComponent<TooltipMouseOver>();
                                toolTip.viewDefaultDesc = false;
                                toolTip.SetDynamicTooltip(LocalizeTextDataModel.instance.GetText("RougeLikeAwardTooltip_Equipment" + mission.level));
                                if (mission.level == 1)
                                {
                                    //按钮点击逻辑
                                    buttonList[i].GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate
                                    {
                                        Award_GetEquipment(missionAwardEquipmentRate_Level1, missionAwardEquipCnt);
                                        rewardButton.SetActive(false);
                                        AngelaConversationUI.instance.FadeOut = true;
                                        AngelaConversationUI.instance.FadeIn = true;
                                    });
                                }
                                else
                                {
                                    buttonList[i].GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate
                                    {
                                        Award_GetEquipment(missionAwardEquipmentRate_Level2, missionAwardEquipCnt);
                                        rewardButton.SetActive(false);
                                        AngelaConversationUI.instance.FadeOut = true;
                                        AngelaConversationUI.instance.FadeIn = true;
                                    });
                                }
                                break;
                            case 1:
                                buttonList[i].transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = LocalizeTextDataModel.instance.GetText("RougeLikeAwardText_Agent" + mission.level);
                                TooltipMouseOver toolTip2 = buttonList[i].AddComponent<TooltipMouseOver>();
                                toolTip2.viewDefaultDesc = false;
                                toolTip2.SetDynamicTooltip(LocalizeTextDataModel.instance.GetText("RougeLikeAwardTooltip_Agent" + mission.level));
                                if (mission.level == 1)
                                {
                                    buttonList[i].GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate
                                    {
                                        Award_GetAgent(missionAwardAgentStat_Level1[0], missionAwardAgentStat_Level1[1], missionAwardAgentCnt, set_sefira: false );
                                        rewardButton.SetActive(false);
                                        AngelaConversationUI.instance.FadeOut = true;
                                        AngelaConversationUI.instance.FadeIn = true;
                                    });
                                }
                                else
                                {
                                    buttonList[i].GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate
                                    {
                                        Award_GetAgent(missionAwardAgentStat_Level2[0], missionAwardAgentStat_Level2[1], missionAwardAgentCnt, set_sefira: false);
                                        rewardButton.SetActive(false);
                                        AngelaConversationUI.instance.FadeOut = true;
                                        AngelaConversationUI.instance.FadeIn = true;
                                    });
                                }
                                break;
                            case 2:
                                buttonList[i].transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = LocalizeTextDataModel.instance.GetText("RougeLikeAwardText_LOB" + mission.level);
                                TooltipMouseOver toolTip3 = buttonList[i].AddComponent<TooltipMouseOver>();
                                toolTip3.viewDefaultDesc = false;
                                toolTip3.SetDynamicTooltip(LocalizeTextDataModel.instance.GetText("RougeLikeAwardTooltip_LOB" + mission.level));
                                if (mission.level == 1)
                                {
                                    buttonList[i].GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate
                                    {
                                        Award_GetLOB(missionAwardLOB_Level1[0], missionAwardLOB_Level1[1]);
                                        rewardButton.SetActive(false);
                                        AngelaConversationUI.instance.FadeOut = true;
                                        AngelaConversationUI.instance.FadeIn = true;
                                    });
                                }
                                else
                                {
                                    buttonList[i].GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate
                                    {
                                        Award_GetLOB(missionAwardLOB_Level2[0], missionAwardLOB_Level2[1]);
                                        rewardButton.SetActive(false);
                                        AngelaConversationUI.instance.FadeOut = true;
                                        AngelaConversationUI.instance.FadeIn = true;
                                    });
                                }
                                break;
                            //奇思
                            case 3:
                                buttonList[i].transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = LocalizeTextDataModel.instance.GetText("RougeLikeAwardText_Wonder" + mission.level);
                                TooltipMouseOver toolTip4 = buttonList[i].AddComponent<TooltipMouseOver>();
                                toolTip4.viewDefaultDesc = false;
                                toolTip4.SetDynamicTooltip(LocalizeTextDataModel.instance.GetText("RougeLikeAwardTooltip_Wonder" + mission.level));

                                if (mission.level == 1)
                                {
                                    buttonList[i].GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate
                                    {
                                        Award_GetWonder(missionAwardWonder_Level1[0], missionAwardWonder_Level1[1]);
                                        rewardButton.SetActive(false);
                                        AngelaConversationUI.instance.FadeOut = true;
                                        AngelaConversationUI.instance.FadeIn = true;
                                    });
                                }
                                else
                                {
                                    buttonList[i].GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate
                                    {
                                        Award_GetWonder(missionAwardWonder_Level2[0], missionAwardWonder_Level2[1]);
                                        rewardButton.SetActive(false);
                                        AngelaConversationUI.instance.FadeOut = true;
                                        AngelaConversationUI.instance.FadeIn = true;
                                    });
                                }
                                break;
                            //模因
                            case 4:
                                buttonList[i].transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = LocalizeTextDataModel.instance.GetText("RougeLikeAwardText_Meme" + mission.level);
                                TooltipMouseOver toolTip5 = buttonList[i].AddComponent<TooltipMouseOver>();
                                toolTip5.viewDefaultDesc = false;
                                toolTip5.SetDynamicTooltip(LocalizeTextDataModel.instance.GetText("RougeLikeAwardTooltip_Meme" + mission.level));

                                if (mission.level == 1)
                                {
                                    buttonList[i].GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate
                                    {
                                        Award_GetMeme(missionAwardMemeRate_Level1);
                                        rewardButton.SetActive(false);
                                        AngelaConversationUI.instance.FadeOut = true;
                                        AngelaConversationUI.instance.FadeIn = true;
                                    });
                                }
                                else
                                {
                                    buttonList[i].GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate
                                    {
                                        Award_GetMeme(missionAwardMemeRate_Level2);
                                        rewardButton.SetActive(false);
                                        AngelaConversationUI.instance.FadeOut = true;
                                        AngelaConversationUI.instance.FadeIn = true;
                                    });
                                }
                                break;
                            default:
                                buttonList[i].transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = "ERROR";
                                buttonList[i].GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate
                                {
                                    GameObject.Destroy(rewardButton);
                                    AngelaConversationUI.instance.FadeOut = true;
                                    AngelaConversationUI.instance.FadeIn = true;
                                });
                                break;
                        }
                    }
                    if (mission.level == 1)
                    {
                        GameObject.Destroy(rewardButton.transform.GetChild(2).gameObject);
                    }

                    Harmony_Patch.dayResult[2]++;
                }
                catch (Exception ex)
                {
                    Harmony_Patch.logger.Error(ex);
                }

            }
            [HPHelper(typeof(UseSkill), "FinishWorkSuccessfully")]
            [HPPostfix]

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
                    Harmony_Patch.logger.Error(ex);
                }
            }
            [HPHelper(typeof(CreatureModel), "Suppressed")]
            [HPPostfix]
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
                            {
                                extraMission.count++;
                            }   
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
                    Harmony_Patch.logger.Error(ex);
                }
            }
            [HPHelper(typeof(CreatureObserveInfoModel), "OnObserveRegion")]
            [HPPostfix]
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
                    Harmony_Patch.logger.Error(ex);
                }
            }
        }

        public static void Award_GetEquipment(int[] rate, int equipCnt = 1, bool angela = true, bool specialEquipParadiseLost = false, bool specialEquipBossBird = false)
        {
            try
            {
                int level = -1;
                int index = Extension.WeightedRandomChoice(rate);
                switch (index)
                {
                    case 0:
                        level = 3; break;
                    case 1:
                        level = 4; break;
                    case 2:
                        level = 5; break;
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
                    string name = "";

                    for (int i = 0; i < equipCnt; i++)
                    {
                        Harmony_Patch.logger.Info("TotalCnt:" + equipCnt.ToString() + "  CurrentCnt:" + i.ToString());
                        EquipmentModel equip = InventoryModel.Instance.CreateEquipment(id_list[Harmony_Patch.customRandom.NextInt(0, id_list.Count)]);
                        name += equip.metaInfo.Name + "  ";
                        Harmony_Patch.logger.Info("Name:" + name);
                    }

                    if (angela)
                    {
                        string text = LocalizeTextDataModel.instance.GetText("RandomAward_GetEquipment");
                        AngelaConversationUI.instance.AddAngelaMessage(text + name);
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

                if (level == 5)
                {
                    Harmony_Patch.dayResult[1]++;
                }
            }
            catch (Exception ex)
            {
                Harmony_Patch.logger.Error(ex);
            }
        }

        public static void Award_GetAgent(int stat_min, int stat_max, int agentCnt = 1, bool angela = true, bool set_sefira = true)
        {
            try
            {
                string name = "";
                for (int i = 0; i < agentCnt; i++)
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

                    name += agentModel.name + "  ";
                    Harmony_Patch.dayResult[0]++;
                }

                if (angela)
                {
                    string text = LocalizeTextDataModel.instance.GetText("RandomAward_GetAgent");
                    AngelaConversationUI.instance.AddAngelaMessage(text + name);
                }
            }
            catch (Exception ex)
            {
                Harmony_Patch.logger.Error(ex);
            }
        }


        public static void Award_GetLOB(int lob_min, int lob_max, bool angela = true)
        {
            try
            {
                int random = Harmony_Patch.customRandom.NextInt(lob_min, lob_max);
                MoneyModel.instance.Add(random);
                if (angela)
                {
                    string text = LocalizeTextDataModel.instance.GetText("RandomAward_GetLOB") + random.ToString();
                    AngelaConversationUI.instance.AddAngelaMessage(text);
                }
            }
            catch (Exception ex)
            {
                Harmony_Patch.logger.Error(ex);
            }
        }

        public static void Award_GetWonder(int wonder_min, int wonder_max, bool angela = true)
        {
            try
            {
                int random = Harmony_Patch.customRandom.NextInt(wonder_min, wonder_max);
                WonderModel.instance.Add(random);
                if (angela)
                {
                    string text = LocalizeTextDataModel.instance.GetText("RandomAward_GetWonder") + random.ToString();
                    AngelaConversationUI.instance.AddAngelaMessage(text);
                }
            }
            catch (Exception ex)
            {
                Harmony_Patch.logger.Error(ex);
            }
        }

        public static void Award_GetMeme(int[] rate, int memeCnt = 1, bool angela = true, bool level3 = false)
        {
            try
            {
                int level = Extension.WeightedRandomChoice(rate);

                List<int> nowMemeInfo = [];
                foreach (var dic in MemeManager.instance.current_dic)
                {
                    nowMemeInfo.Add(dic.Value.metaInfo.id);
                }
                // 过滤掉已经拥有的模因或未满足require的模因，保留duplicate为true的模因或未获得模因
                List<int> tempMemeLevel1 = [];
                List<int> tempMemeLevel2 = [];
                List<int> tempMemeLevel3 = [];
                foreach (KeyValuePair<int, MemeInfo> kvp in MemeManager.instance.all_dic)
                {
                    if ((kvp.Value.duplicate || !nowMemeInfo.Contains(kvp.Key)) && kvp.Value.CheckRequire())
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

                List<int> target_id = new List<int>();
                bool success = true;
                for (int i = 0; i < memeCnt; i++)
                {
                    if (level3)//3级模因
                    {
                        target_id.Add(tempMemeLevel3[Harmony_Patch.customRandom.NextInt(0, tempMemeLevel3.Count)]);
                        MemeManager.instance.CreateMemeModel(target_id[i]);
                    }
                    else
                    {
                        switch (level)
                        {
                            case 0:
                                target_id.Add(tempMemeLevel1[Harmony_Patch.customRandom.NextInt(0, tempMemeLevel1.Count)]);
                                MemeManager.instance.CreateMemeModel(target_id[i]);
                                break;
                            case 1:
                                target_id.Add(tempMemeLevel2[Harmony_Patch.customRandom.NextInt(0, tempMemeLevel2.Count)]);
                                MemeManager.instance.CreateMemeModel(target_id[i]);
                                break;
                            default:
                                success = false;
                                break;
                        }
                    }
                }
                 
                
                if (angela)
                {
                    if (success)
                    {
                        string text = LocalizeTextDataModel.instance.GetText("RandomAward_GetMeme");
                        string name = "";
                        foreach (int id in target_id)
                        {
                            string tempName;
                            MemeManager.GetMemeInfo(id).GetLocalizedText("name", out tempName);
                            name += tempName + "  ";
                        }
                        AngelaConversationUI.instance.AddAngelaMessage(text + name);
                    }
                    else
                    {
                        string text = LocalizeTextDataModel.instance.GetText("RandomAward_GetMeme_Fail");
                        AngelaConversationUI.instance.AddAngelaMessage(text);
                    }
                }
            }
            catch (Exception ex)
            {
                Harmony_Patch.logger.Error(ex);
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
