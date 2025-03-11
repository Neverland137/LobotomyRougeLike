using GlobalBullet;
using Harmony;
using HPHelper;
using NewGameMode.Diffculty;
using System;
using System.Collections.Generic;

namespace NewGameMode
{
    public class DifficultyPatch
    {
        [HPHelper(typeof(GlobalBulletManager), "SetMaxBullet")]
        [HPPrefix]
        public static void PatchMaxBullet(ref int max)
        {
            if (PlayerModel.instance.GetDay() == 30)
            {
                int Adder = DifficultyManager.GetNowDifficulty().BulletAdderOnFirstDay();
                max += Adder;
            }
            int Adder2 = MemeManager.instance.BulletAdder();
            max += Adder2;
        }
        [HPHelper(typeof(StageTypeInfo), "GetEnergyNeed")]
        [HPPostfix]
        public static void PatchMaxEnergy(int day, ref float __result)
        {
            var NowDifficulty = DifficultyManager.GetNowDifficulty();
            int Adder = NowDifficulty.MaxEnergyAdder() + MemeManager.instance.MaxEnergyAdder();
            float Times = NowDifficulty.MaxEnergyTimes() * MemeManager.instance.MaxEnergyTimes();
            float energy = (__result * Times) + Adder;
            __result = energy;
        }
        [HPHelper(typeof(WorkerModel), "TakeDamage", typeof(UnitModel), typeof(DamageInfo))]
        [HPPrefix]
        public static void PatchTakeDamage(UnitModel actor, ref DamageInfo dmg)
        {
            var NowDifficulty = DifficultyManager.GetNowDifficulty();
            int Adder = NowDifficulty.AgentDamageAdder() + MemeManager.instance.AgentDamageAdder();
            float times = NowDifficulty.AgentDamageTimes() * MemeManager.instance.AgentDamageTimes();
            dmg.min = (int)(dmg.min * times) + Adder;
            dmg.max = (int)(dmg.max * times) + Adder;
        }
        [HPHelper(typeof(CreatureModel), "GetWorkSuccessProb")]
        [HPPostfix]
        public static void PatchWorkSuccessAdder(AgentModel actor, SkillTypeInfo skill, ref float __result)
        {
            var NowDifficulty = DifficultyManager.GetNowDifficulty();
            float adder = NowDifficulty.WorkSuccessAdder() + MemeManager.instance.WorkSuccessAdder();
            __result += adder;
        }
        // 我知道这种代码简直蠢死了，但是反正本来就已经够屎山了，不差这一点
        //这蠢吗 不蠢吧 摸摸）
        private static Dictionary<CreatureTypeInfo, int> maxHPTable = new();
        [HPHelper(typeof(CreatureModel), "Escape")]
        [HPPrefix]
        public static void PatchCreatureMaxHP_1(ref CreatureModel __instance)
        {
            if (!maxHPTable.ContainsKey(__instance.metaInfo))
            {
                var NowDifficulty = DifficultyManager.GetNowDifficulty();
                float CreatureMaxHPTimes = NowDifficulty.CreatureMaxHPTimes() * MemeManager.instance.CreatureMaxHPTimes();
                __instance.metaInfo.maxHp = (int)(Math.Round(__instance.metaInfo.maxHp * CreatureMaxHPTimes));
                maxHPTable.Add(__instance.metaInfo, __instance.metaInfo.maxHp);
            }
            else
            {
                __instance.metaInfo.maxHp = maxHPTable[__instance.metaInfo];
            }
        }
        private static Dictionary<ChildCreatureTypeInfo, int> maxHPTable_Child = new();
        [HPHelper(typeof(ChildCreatureModel), "Escape")]
        [HPPrefix]
        public static void PatchCreatureMaxHP_2(ref ChildCreatureModel __instance)
        {
            if (!maxHPTable_Child.ContainsKey(__instance.childMetaInfo))
            {
                var NowDifficulty = DifficultyManager.GetNowDifficulty();
                float CreatureMaxHPTimes = NowDifficulty.CreatureMaxHPTimes() * MemeManager.instance.CreatureMaxHPTimes();
                __instance.childMetaInfo.maxHp = (int)(Math.Round(__instance.childMetaInfo.maxHp * CreatureMaxHPTimes));
                maxHPTable_Child.Add(__instance.childMetaInfo, __instance.childMetaInfo.maxHp);
            }
            else
            {
                __instance.childMetaInfo.maxHp = maxHPTable_Child[__instance.childMetaInfo];
            }
        }
        /*
        private static Dictionary<long, int> maxHPTable = new();
        public static void PatchCreatureMaxHP_1(long id, ref CreatureTypeInfo __result)
        {
            if (!maxHPTable.ContainsKey(id))
            {
                var NowDifficulty = DifficultyManager.GetNowDifficulty();
                float CreatureMaxHPTimes = NowDifficulty.CreatureMaxHPTimes();
                __result.maxHp = (int)(Math.Round(__result.maxHp * CreatureMaxHPTimes));
                maxHPTable.Add(id, __result.maxHp);
            }
            else
            {
                __result.maxHp = maxHPTable[id];
            }
            Harmony_Patch.LogDebug($"Creature ID: {id} maxHP: {__result.maxHp}");
        }
        private static Dictionary<LobotomyBaseMod.LcIdLong, int> maxHPTable_MOD = new();
        public static void PatchCreatureMaxHP_2(LobotomyBaseMod.LcIdLong lcid, ref CreatureTypeInfo __result)
        {
            if (!maxHPTable_MOD.ContainsKey(lcid))
            {
                var NowDifficulty = DifficultyManager.GetNowDifficulty();
                float CreatureMaxHPTimes = NowDifficulty.CreatureMaxHPTimes();
                __result.maxHp = (int)(Math.Round(__result.maxHp * CreatureMaxHPTimes));
                maxHPTable_MOD.Add(lcid, __result.maxHp);
            }
            else
            {
                __result.maxHp = maxHPTable_MOD[lcid];
            }
        }
        */
        [HPHelper(typeof(CreatureModel), "SetFeelingStateInWork")]
        [HPPostfix]
        public static void PatchCreatureTiredTime(CreatureFeelingState state, CreatureModel __instance)
        {
            var NowDifficulty = DifficultyManager.GetNowDifficulty();
            float tiredAdder = NowDifficulty.CreatureTiredTimeAdder() + MemeManager.instance.CreatureTiredTimeAdder();
            __instance.feelingStateRemainTime += tiredAdder;
        }
    }
}
