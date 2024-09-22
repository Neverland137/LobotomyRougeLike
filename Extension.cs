using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NewGameMode
{
    public static class Extension
    {
        /// <summary>
        /// 获取一个私密词条
        /// </summary>
        /// <typeparam name="T">词条的类型 int bool这种类型</typeparam>
        /// <param name="instance">词条所在类型</param>
        /// <param name="fieldname">词条名</param>
        public static T GetPrivateField<T>(this object instance, string fieldname)
        {
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
            return (T)((object)instance.GetType().GetField(fieldname, bindingFlags).GetValue(instance));
        }

        /// <summary>
        /// 修改一个私密词条
        /// </summary>
        /// <param name="instance">词条所在类型</param>
        /// <param name="fieldname">词条名</param>
        /// <param name="value">修改值</param>
        public static void SetPrivateField(this object instance, string fieldname, object value)
        {
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
            instance.GetType().GetField(fieldname, bindingFlags).SetValue(instance, value);
        }

        /// <summary>
        /// 使用一个私密方法
        /// </summary>
        /// <typeparam name="T">原方法的返回类型（应该） void填object</typeparam>
        /// <param name="instance">方法所在类型</param>
        /// <param name="name">方法名称</param>
        /// <param name="param">方法所有参数 没有参数填null</param>
        public static T CallPrivateMethod<T>(this object instance, string name, params object[] param)
        {
            try
            {
                BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
                return (T)((object)instance.GetType().GetMethod(name, bindingFlags).Invoke(instance, param));
            }
            catch (Exception ex)
            {
                //BlueArchiveDebug.WriteError(ex, name);
                return default(T);
            }
        }

        /// <summary>
        /// 使用一个私密静态方法
        /// </summary>
        /// <typeparam name="T">原方法的返回类型（应该） void填object</typeparam>
        /// <param name="instance">方法所在类型</param>
        /// <param name="name">方法名称</param>
        /// <param name="param">方法所有参数 没有参数填null</param>
        public static T CallPrivateStaticMethod<T>(this object instance, string name, params object[] param)
        {
            BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.NonPublic;
            return (T)((object)instance.GetType().GetMethod(name, bindingFlags).Invoke(null, param));
        }
        /// <summary>
        /// 根据权重，随机返回一个索引
        /// </summary>
        /// <param name="weights">权重数组</param>
        /// <returns></returns>
        public static int WeightedRandomChoice(int[] weights)
        {
            Random random = new Random();
            int totalWeight = 0;
            foreach (var weight in weights)
            {
                totalWeight += weight;
            }
            double randValue = random.NextDouble() * totalWeight;
            double cumulativeWeight = 0;
            for (int i = 0; i < weights.Length; i++)
            {
                cumulativeWeight += weights[i];
                if (randValue < cumulativeWeight)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
