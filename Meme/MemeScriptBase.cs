﻿using System;
using System.Collections.Generic;

namespace NewGameMode
{
    [Serializable]
    public class MemeScriptBase
    {
        
        public MemeModel model;
        public bool init = false;
        public static Dictionary<int, object> allMemeGameDataDic = new Dictionary<int, object>();

        

        public void SetModel(MemeModel m)
        {
            model = m;
        }

        public void Init()
        {
            if (!init)
            {
                OnInit();
                init = true;
            }
        }

        public virtual void OnInit()
        {

        }
        public virtual void OnGet()
        {
            Harmony_Patch.logger.Info("GetMeme:" + model.metaInfo.sprite_name);
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

        public virtual void OnKillTargetWorker(UnitModel actor, UnitModel target)
        {
        }

        public virtual void OnGiveDamage(UnitModel actor, UnitModel target, ref DamageInfo dmg)
        {
            
        }

        public virtual void OnGiveDamageAfter(UnitModel actor, UnitModel target, DamageInfo dmg)
        {
        }

        public virtual void WorkerTakeDamage(UnitModel actor, UnitModel victim, ref DamageInfo dmg)
        {
        }

        public virtual void OnWorkerTakeDamage_After(float value, UnitModel actor, UnitModel victim, RwbpType type)
        {
        }

        public virtual void CreatureTakeDamage(UnitModel actor, UnitModel victim, ref DamageInfo dmg)//在异想体被扣减血量后才生效
        {
        }

        public virtual void OnCreatureTakeDamage_After(float value, UnitModel actor, UnitModel victim, RwbpType type)
        {
        }

        public virtual void OnCreatureSuppressed(CreatureModel creature)
        {
        }

        public virtual void OnWorkerDie(WorkerModel worker)
        {
        }

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

        
        public virtual DamageInfo GetWorkerDamage(WorkerModel actor)
        {
            return actor.Equipment.weapon.metaInfo.damageInfo.Copy();
        }
        

        public virtual void OnFixedUpdate()
        {
        }

        public virtual float GetWorkProbSpecialBonus(UnitModel actor, SkillTypeInfo skill)
        {
            return 0f;
        }

        public virtual void OnHeal(bool isMental, float amount)
        {
        }

        public virtual int WonderAdder()
        {
            return 0;
        }

        public virtual float WonderTimes()//与难度系统的奇思倍率成加算
        {
            return 0f;
        }

        public virtual int BulletAdder()
        {
            return 0;
        }
        public virtual int MaxEnergyAdder()
        {
            return 0;
        }
        public virtual float MaxEnergyTimes()
        {
            return 1f;
        }
        /// <summary>
        /// 员工受到的伤害增加
        /// </summary>
        /// <returns></returns>
        public virtual int AgentDamageAdder()
        {
            return 0;
        }
        /// <summary>
        /// 员工受到的伤害倍率
        /// </summary>
        /// <returns></returns>
        public virtual float AgentDamageTimes()
        {
            return 1f;
        }
        public virtual int OverloadAdder()
        {
            return 0;
        }
        public virtual float WorkSuccessAdder()
        {
            return 0f;
        }
        public virtual float CreatureMaxHPTimes()
        {
            return 1f;
        }
        public virtual int CreatureTiredTimeAdder()
        {
            return 0;
        }
        public virtual float FurnaceBoomAdder()
        {
            return 0f;
        }
        public virtual float UpLevel1RecipeProbAdder()
        {
            return 0f;
        }


        public static void LoadData(Dictionary<string, object> dic) //不用写存储，存储已经在Harmony_Patch的SaveRougeLikeDayData里了
        {
            GameUtil.TryGetValue<Dictionary<int, object>>(dic, "memeData", ref allMemeGameDataDic);
            foreach (KeyValuePair<int, MemeModel> pair in MemeManager.instance.current_dic)//部分模因数据初始化，例如基础暴击，以及无法存储的模因按钮
            {
                pair.Value.script.Init();
            }
        }


        /// <summary>
        /// 用于存储单个模因的“当局数据”，例如“本局游戏中，员工死亡数”。
        /// 对于不同模因，该数据不同，所以不能使用static。。
        /// </summary>
        /// <param name="data"></param>
        public void Meme_GameDataSave(object data, int instanceId = -1)
        {
            try
            {
                if (instanceId != -1)
                {
                    if (allMemeGameDataDic.ContainsKey(instanceId))
                    {
                        allMemeGameDataDic[instanceId] = data;
                    }
                    else
                    {
                        allMemeGameDataDic.Add(instanceId, data);
                    }
                }
                else
                {
                    if (allMemeGameDataDic.ContainsKey(model.instanceId))
                    {
                        allMemeGameDataDic[model.instanceId] = data;
                    }
                    else
                    {
                        allMemeGameDataDic.Add(model.instanceId, data);
                    }
                }
            }
            catch (Exception e)
            {
                Harmony_Patch.logger.Error(e);
            }
        }


        /// <summary>
        /// 用于读取模因的“当局数据”，例如“本局游戏中，员工死亡数”
        /// </summary>
        public object Meme_GameDataRead(int instanceId = -1)
        {
            object result = null;
            if (allMemeGameDataDic.Count != 0)
            {
                if (instanceId != -1)
                {
                    allMemeGameDataDic.TryGetValue(instanceId, out result);
                }
                else
                {
                    allMemeGameDataDic.TryGetValue(model.instanceId, out result);
                }
            }
            
            return null;
        }

        public List<AgentModel> GetAgentsWithRequire(int equip_id = -1, RwbpType weapon_rwbp = (RwbpType)999999, SefiraEnum sefira = SefiraEnum.DUMMY, bool statisfy_all_require = false)
        {
            List<AgentModel> agents = [];
            List<AgentModel> equip_agents = [];
            List<AgentModel> rwbp_agents = [];
            List<AgentModel> sefira_agents = [];

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

                if (agent.currentSefiraEnum == sefira)
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
                        agents.Add(agent4);
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
            List<CreatureModel> creatures = [];
            List<CreatureModel> qli_max_creatures = [];
            List<CreatureModel> qli_current_creatures = [];
            List<CreatureModel> sefira_creatures = [];

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

        public static RwbpType RandomDmgType()
        {
            float random = UnityEngine.Random.value;
            RwbpType type;
            if (random <= 0.25f)
            {
                type = RwbpType.R;
            }
            else if (random <= 0.5f)
            {
                type = RwbpType.W;
            }
            else if (random <= 0.75f)
            {
                type = RwbpType.B;
            }
            else
            {
                type = RwbpType.P;
            }
            return type;
        }

        public static RwbpType WeakDmgType(UnitModel target)
        {
            RwbpType result = RwbpType.R;

            if (target.defense != null)
            {
                float weak = target.defense.R;
                if (target.defense.W > weak)
                {
                    result = RwbpType.W;
                    weak = target.defense.W;
                }
                if (target.defense.B > weak)
                {
                    result = RwbpType.B;
                    weak = target.defense.B;
                }
                if (target.defense.P > weak)
                {
                    result = RwbpType.P;
                    weak = target.defense.P;
                }
            }

            return result;
        }

    }

    public enum DamageType_RougeLike
    {
        ADDITIONAL,
        CRITICAL,
        NULL
    }

    public class RougeLikeDamageInfo : DamageInfo
    {
        public DamageType_RougeLike rougeType;
        public bool realDmg = false;
        // 必须调用基类的构造函数
        public RougeLikeDamageInfo(RwbpType type, int min, int max, DamageType_RougeLike rougeType = DamageType_RougeLike.NULL, bool realDmg = false)
            : base(type, min, max)
        {
            this.rougeType = rougeType;
            if (realDmg)
            {
                realDmg = true;
            }
        }

        public RougeLikeDamageInfo(RwbpType type, float dmg, DamageType_RougeLike rougeType = DamageType_RougeLike.NULL, bool realDmg = false)
            : base(type, dmg)
        {
            this.rougeType = rougeType;
            if (realDmg)
            {
                realDmg = true;
            }
        }
    }

    
}
