﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGameMode
{
    /// <summary>
    /// 难度说明基本都在DifficultyInfo中，因此本class不再赘述
    /// </summary>
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
        public DifficultyStruct(int wonderAdder, float wonderTimes, int AgentAdder, int AgentReplacer, int BulletAdderOnFirstDay, int MaxEnergyAdder, float MaxEnergyTimes, int AgentDamageAdder, float AgentDamageTimes)
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
    }
}
