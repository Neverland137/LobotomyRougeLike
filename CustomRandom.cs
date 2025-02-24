using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGameMode
{
    public class CustomRandom
    {
        private uint state; // 当前随机数状态
        private const ulong Multiplier = 1664525; // 乘数
        private const ulong Increment = 1013904223; // 增量
        private const ulong Modulus = uint.MaxValue; // 模数（2^32 - 1）
        // 构造函数，通过种子初始化
        public CustomRandom(uint seed)
        {
            state = seed;
        }
        public void SetSeed(uint seed)
        {
            state = seed;
        }
        // 生成下一个随机数
        public long Next()
        {
            /*
            state ^= state << 13;
            state ^= state >> 7;
            state ^= state << 17;
            */
            state = (uint)((Multiplier * state + Increment) % Modulus);
            return state;
        }

        /// <summary>
        /// 生成指定范围内的随机数，包含min不包含max
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public int NextInt(int min, int max)
        {
            uint range = (uint)(max - min);
            return (int)(min + Next() % range);
        }

        /// <summary>
        /// 生成 0 到 1 之间的随机浮点数
        /// </summary>
        /// <returns></returns>
        public float NextFloat()
        {
            return Next() / (float)(uint.MaxValue);
        }

        /// <summary>
        /// 生成 0 到 100 之间的随机浮点数
        /// </summary>
        /// <returns></returns>
        public float NextFloatPercent()
        {
            return Next() / (float)(uint.MaxValue) * 100;
        }
    }
}
