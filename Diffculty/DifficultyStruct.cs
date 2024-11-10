using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGameMode
{
    public class DifficultyStruct
    {
        private int _WonderAdder = 0;
        private float _WonderTimes = 1f;
        private int _AgentAdder = 0;
        private int _AgentReplacer = -1;
        private int _BulletAdderOnFirstDay = 0;
        private int _MaxEnergyAdder = 0;
        private float _MaxEnergyTimes = 1f;
        private int _AgentDamageAdder = 0;
        private float _AgentDamageTimes = 1f;
        private int _OverloadAdder = 0;
        private float _WorkSuccessAdder = 0f;
        private float _CreatureMaxHPTimes = 1f;
        private float _AgentMaxHPTimes = 1f;
        private int _CreatureTiredTimeAdder = 0;
        private float _FurnaceBoomAdder = 0f;
        private float _UpLevel1RecipeProbAdder = 0f;

        public DifficultyStruct(int wonderAdder, float wonderTimes, int AgentAdder, int AgentReplacer, int BulletAdderOnFirstDay, int MaxEnergyAdder, float MaxEnergyTimes, int AgentDamageAdder, float AgentDamageTimes, int OverloadAdder, float WorkSuccessAdder, float CreatureMaxHPTimes, int CreatureTiredTimeAdder, float FurnaceBoomAdder, float UpLevel1RecipeProbAdder)
        {
            _WonderAdder = wonderAdder;
            _WonderTimes = wonderTimes;
            _AgentAdder = AgentAdder;
            _AgentReplacer = AgentReplacer;
            _BulletAdderOnFirstDay = BulletAdderOnFirstDay;
            _MaxEnergyAdder = MaxEnergyAdder;
            _MaxEnergyTimes = MaxEnergyTimes;
            _AgentDamageAdder = AgentDamageAdder;
            _AgentDamageTimes = AgentDamageTimes;
            _OverloadAdder = OverloadAdder;
            _WorkSuccessAdder = WorkSuccessAdder;
            _CreatureMaxHPTimes = CreatureMaxHPTimes;
            _CreatureTiredTimeAdder = CreatureTiredTimeAdder;
            _FurnaceBoomAdder = FurnaceBoomAdder;
            _UpLevel1RecipeProbAdder = UpLevel1RecipeProbAdder;
        }
        public int WonderAdder()
        {
            return _WonderAdder;
        }
        public float WonderTimes()
        {
            return _WonderTimes;
        }
        public int AgentAdder()
        {
            return _AgentAdder;
        }
        public int AgentReplacer()
        {
            return _AgentReplacer;
        }
        public int BulletAdderOnFirstDay()
        {
            return _BulletAdderOnFirstDay;
        }
        public int MaxEnergyAdder()
        {
            return _MaxEnergyAdder;
        }
        public float MaxEnergyTimes()
        {
            return _MaxEnergyTimes;
        }
        public int AgentDamageAdder()
        {
            return _AgentDamageAdder;
        }
        public float AgentDamageTimes()
        {
            return _AgentDamageTimes;
        }
        public int OverloadAdder()
        {
            return _OverloadAdder;
        }
        public float WorkSuccessAdder()
        {
            return _WorkSuccessAdder;
        }
        public float CreatureMaxHPTimes()
        {
            return _CreatureMaxHPTimes;
        }
        public int CreatureTiredTimeAdder()
        {
            return _CreatureTiredTimeAdder;
        }
        public float FurnaceBoomAdder()
        {
            return _FurnaceBoomAdder;
        }
        public float UpLevel1RecipeProbAdder()
        {
            return _UpLevel1RecipeProbAdder;
        }
    }
}
