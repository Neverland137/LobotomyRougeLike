﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGameMode.Meme
{
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
}
