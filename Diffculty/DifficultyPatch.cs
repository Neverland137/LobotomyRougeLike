using GlobalBullet;
using Harmony;
using NewGameMode.Diffculty;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NewGameMode
{
    public class DifficultyPatch
    {
        public DifficultyPatch(HarmonyInstance instance)
        {
            try
            {
                instance.Patch(typeof(GlobalBulletManager).GetMethod("SetMaxBullet"), new HarmonyMethod(typeof(DifficultyPatch).GetMethod("PatchMaxBullet")), null);
                instance.Patch(typeof(StageTypeInfo).GetMethod("GetEnergyNeed"), null, new HarmonyMethod(typeof(DifficultyPatch).GetMethod("PatchMaxEnergy")));
                instance.Patch(typeof(WorkerModel).GetMethod("TakeDamage",new Type[] { typeof(UnitModel), typeof(DamageInfo) }), new HarmonyMethod(typeof(DifficultyPatch).GetMethod("PatchTakeDamage")), null);
            }
            catch (Exception ex)
            {
                Harmony_Patch.LogError("Difficulty Patch Error.\n" + ex.ToString());
            }
        }
        public static void PatchMaxBullet(ref int max)
        {
            if (PlayerModel.instance.GetDay() == 30)
            {
                int Adder = DifficultyManager.GetNowDifficulty().BulletAdderOnFirstDay();
                max += Adder;
            }
        }
        public static void PatchMaxEnergy(int day, ref float __result)
        {
            var NowDifficulty = DifficultyManager.GetNowDifficulty();
            int Adder = NowDifficulty.MaxEnergyAdder();
            float Times = NowDifficulty.MaxEnergyTimes();
            float energy = (__result * Times) + Adder;
            __result = energy;
        }
        public static void PatchTakeDamage(UnitModel actor, ref DamageInfo dmg)
        {
            var NowDifficulty = DifficultyManager.GetNowDifficulty();
            int Adder = NowDifficulty.AgentDamageAdder();
            float times = NowDifficulty.AgentDamageTimes();
            dmg.min = (int)(dmg.min * times) + Adder;
            dmg.max = (int)(dmg.max * times) + Adder;
        }
    }
}
