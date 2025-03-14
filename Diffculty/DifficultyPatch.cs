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
            if(GlobalGameManager.instance.gameMode == Harmony_Patch.rougeLike)
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
            if (GlobalGameManager.instance.gameMode == Harmony_Patch.rougeLike)
            {
                var NowDifficulty = DifficultyManager.GetNowDifficulty();
                int Adder = NowDifficulty.MaxEnergyAdder() + MemeManager.instance.MaxEnergyAdder();
                float Times = NowDifficulty.MaxEnergyTimes() * MemeManager.instance.MaxEnergyTimes();
                float energy = (__result * Times) + Adder;
                __result = energy;
            }
        }
        [HPHelper(typeof(WorkerModel), "TakeDamage", typeof(UnitModel), typeof(DamageInfo))]
        [HPPrefix]
        public static void PatchTakeDamage(UnitModel actor, ref DamageInfo dmg)
        {
            if (GlobalGameManager.instance.gameMode == Harmony_Patch.rougeLike)
            {
                var NowDifficulty = DifficultyManager.GetNowDifficulty();
                int Adder = NowDifficulty.AgentDamageAdder() + MemeManager.instance.AgentDamageAdder();
                float times = NowDifficulty.AgentDamageTimes() * MemeManager.instance.AgentDamageTimes();
                dmg.min = (int)(dmg.min * times) + Adder;
                dmg.max = (int)(dmg.max * times) + Adder;
            }
        }
        [HPHelper(typeof(CreatureModel), "GetWorkSuccessProb")]
        [HPPostfix]
        public static void PatchWorkSuccessAdder(AgentModel actor, SkillTypeInfo skill, ref float __result)
        {
            if (GlobalGameManager.instance.gameMode == Harmony_Patch.rougeLike)
            {
                var NowDifficulty = DifficultyManager.GetNowDifficulty();
                float adder = NowDifficulty.WorkSuccessAdder() + MemeManager.instance.WorkSuccessAdder();
                __result += adder;
            }
        }
        // 我知道这种代码简直蠢死了，但是反正本来就已经够屎山了，不差这一点
        //这蠢吗 不蠢吧 摸摸）

        [HPHelper(typeof(CreatureModel), "Escape")]
        [HPPrefix]
        public static void PatchCreatureMaxHP(ref CreatureModel __instance)
        {
            if (GlobalGameManager.instance.gameMode == Harmony_Patch.rougeLike)
            {
                var NowDifficulty = DifficultyManager.GetNowDifficulty();
                float CreatureMaxHPTimes = NowDifficulty.CreatureMaxHPTimes() * MemeManager.instance.CreatureMaxHPTimes() - 1;
                __instance.AddUnitBuf(new CreatureHpAdderBuf(__instance, CreatureMaxHPTimes));
            }
        }
        [HPHelper(typeof(ChildCreatureModel), "Escape")]
        [HPPrefix]
        public static void PatchChildCreatureMaxHP(ref ChildCreatureModel __instance)
        {
            if (GlobalGameManager.instance.gameMode == Harmony_Patch.rougeLike)
            {
                var NowDifficulty = DifficultyManager.GetNowDifficulty();
                float CreatureMaxHPTimes = NowDifficulty.CreatureMaxHPTimes() * MemeManager.instance.CreatureMaxHPTimes() - 1;
                __instance.AddUnitBuf(new CreatureHpAdderBuf(__instance, CreatureMaxHPTimes));
            }
        }
        [HPHelper(typeof(CreatureModel), "SetFeelingStateInWork")]
        [HPPostfix]
        public static void PatchCreatureTiredTime(CreatureFeelingState state, CreatureModel __instance)
        {
            var NowDifficulty = DifficultyManager.GetNowDifficulty();
            float tiredAdder = NowDifficulty.CreatureTiredTimeAdder() + MemeManager.instance.CreatureTiredTimeAdder();
            __instance.feelingStateRemainTime += tiredAdder;
        }

        public class CreatureHpAdderBuf : UnitBuf
        {
            public int originHp = 0;
            public float times = 0;
            public CreatureHpAdderBuf(CreatureModel creature, float times)
            {
                type = Harmony_Patch.rougeDifficultyBuf;
                duplicateType = BufDuplicateType.UNLIMIT;
                remainTime = float.MaxValue;
                model = creature;
                this.times = times;
            }

            public override void Init(UnitModel model)
            {
                if (model is CreatureModel || model is ChildCreatureModel)
                {
                    originHp = model.baseMaxHp;
                    model.baseMaxHp += (int)(originHp * times);
                }
            }

            public override void OnUnitDie()
            {
                base.OnUnitDie();
                model.baseMaxHp = originHp;
            }
        }
        
    }
    
}
