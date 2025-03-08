// 一个方便的Harmony补丁框架，可以自动Patch所有由[HPHelper]与[HPPrefix]或[HPPostfix]标记的方法。
// 作者：曾小皮-ZengXiaoPi
// 在MIT许可证下发布，允许自由分享、修改、分发。
// 用法：
// 0. 将HPPatcher.LogError改为你的日志记录方法。
// 1. 在你需要Patch的类中添加[HPHelper]属性，并设置需要Patch的类型、方法名、参数类型（可选）、是否无参数（可选）。
// 注意，如果你需要Patch的方法有多个重载，而你刚好需要Patch没有参数的方法，那么你需要在[HPHelper]属性中设置TargetUseNoParameter为true。
// 2. 在你需要Patch的方法中添加[HPPrefix]或[HPPostfix]属性。
// 3. 在你需要Patch的入口处调用HPPatcher.PatchAll(HarmonyInstance.Create("your.mod.name"), typeof(YourPatchClass));


using Harmony;
using NewGameMode;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace HPHelper
{
    [AttributeUsage(AttributeTargets.Method)]
    public class HPPrefixAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class HPPostfixAttribute : Attribute { }
    [AttributeUsage(AttributeTargets.Method)]
    public class HPTranspilerAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class HPHelperAttribute : Attribute
    {
        public Type TargetType { get; }
        public string TargetTypeName { get; }
        public string MethodName { get; }
        public Type[] ParameterTypes { get; }
        public bool TargetUseNoParameter { get; }

        public HPHelperAttribute(Type targetType, string methodName, params Type[] parameterTypes)
        {
            TargetType = targetType;
            MethodName = methodName;
            ParameterTypes = parameterTypes;
            TargetUseNoParameter = false;
        }

        public HPHelperAttribute(string targetTypeName, string methodName, params Type[] parameterTypes)
        {
            TargetTypeName = targetTypeName;
            MethodName = methodName;
            ParameterTypes = parameterTypes;
            TargetUseNoParameter = false;
        }
        public HPHelperAttribute(Type targetType, string methodName)
        {
            TargetType = targetType;
            MethodName = methodName;
            ParameterTypes = null;
            TargetUseNoParameter = false;
        }

        public HPHelperAttribute(string targetTypeName, string methodName)
        {
            TargetTypeName = targetTypeName;
            MethodName = methodName;
            ParameterTypes = null;
            TargetUseNoParameter = false;
        }
        public HPHelperAttribute(Type targetType, string methodName, bool targetUseNoParameter)
        {
            TargetType = targetType;
            MethodName = methodName;
            ParameterTypes = null;
            TargetUseNoParameter = true;
        }

        public HPHelperAttribute(string targetTypeName, string methodName, bool targetUseNoParameter)
        {
            TargetTypeName = targetTypeName;
            MethodName = methodName;
            ParameterTypes = null;
            TargetUseNoParameter = true;
        }
    }
    public static class HPPatcher
    {
        /// <summary>
        /// 日志记录。
        /// </summary>
        /// <param name="message">消息</param>
        public static void LogError(string message)
        {
            // 请修改此处为你自己的日志记录方法
            Harmony_Patch.logger.Error(message);
        }
        /// <summary>
        /// 自动Patch所有由[HPHelper]与[HPPrefix]或[HPPostfix]或[Transpiler]标记的方法
        /// </summary>
        /// <param name="harmony">HarmonyInstance实例</param>
        /// <param name="patchClass">你需要Patch的类</param>
        public static void PatchAll(HarmonyInstance harmony, Type patchClass)
        {
            foreach (var method in patchClass.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
            {
                try
                {
                    ProcessMethod(harmony, method);
                }
                catch (Exception ex)
                {
                    LogError($"Error patching {method.Name}: {ex}");
                }
            }
        }

        private static void ProcessMethod(HarmonyInstance harmony, MethodInfo patchMethod)
        {
            try
            {
                var hpHelper = (HPHelperAttribute)Attribute.GetCustomAttribute(
                    patchMethod,
                    typeof(HPHelperAttribute)
                );
                if (hpHelper == null) return;

                int fixTypeCount = 0;
                bool isPrefix = patchMethod.IsDefined(typeof(HPPrefixAttribute), false);
                bool isPostfix = patchMethod.IsDefined(typeof(HPPostfixAttribute), false);
                bool isTranspiler = patchMethod.IsDefined(typeof(HPTranspilerAttribute), false);

                if (isPrefix) fixTypeCount++;
                if (isPostfix) fixTypeCount++;
                if (isTranspiler) fixTypeCount++;
                if (fixTypeCount == 0) return;
                if (fixTypeCount > 1)
                    throw new InvalidOperationException("Method cannot have more than one fix type attribute");

                Type targetType;
                if (ReferenceEquals(hpHelper.TargetType, null))
                {
                    targetType = AccessTools.TypeByName(hpHelper.TargetTypeName);
                }
                else
                {
                    targetType = hpHelper.TargetType;
                }
                if (ReferenceEquals(targetType, null))
                    throw new ArgumentException($"Could not find type: {hpHelper.TargetTypeName ?? "null"}");
                Type[] parameterTypes = hpHelper.ParameterTypes;
                if (parameterTypes == null)
                {
                    if (hpHelper.TargetUseNoParameter)
                    {
                        parameterTypes = new Type[0];
                    }
                    else
                    {
                        parameterTypes = GetParameterTypes(targetType, hpHelper.MethodName);
                    }
                }

                MethodBase originalMethod = targetType.GetMethod(hpHelper.MethodName, AccessTools.all, null, parameterTypes, null)
                 ?? throw new MissingMethodException($"Method {hpHelper.MethodName} not found in {targetType}");

                var harmonyMethod = new HarmonyMethod(patchMethod);

                harmony.Patch(
                    original: originalMethod,
                    prefix: isPrefix ? harmonyMethod : null,
                    postfix: isPostfix ? harmonyMethod : null,
                    transpiler: isTranspiler ? harmonyMethod : null
                );
            }
            catch (Exception ex)
            {
                LogError($"Error processing method {patchMethod.Name}: " + ex.ToString());
            }
        }
        public static Type[] GetParameterTypes(Type type, string methodName)
        {
            MethodInfo[] methods = type.GetMethods(AccessTools.all);
            List<MethodInfo> methodList = new List<MethodInfo>();
            foreach (var method in methods)
            {
                if (method.Name == methodName)
                {
                    methodList.Add(method);
                }
            }
            methods = methodList.ToArray();

            if (methods.Length == 0)
            {
                throw new ArgumentException($"The method '{methodName}' does not exist in type '{type.Name}'.");
            }

            if (methods.Length > 1)
            {
                throw new AmbiguousMatchException($"The method '{methodName}' is ambiguous in type '{type.Name}'.");
            }
            ParameterInfo[] parameters = methods[0].GetParameters();
            List<Type> parameterList = new List<Type>();
            foreach (var parameter in parameters)
            {
                parameterList.Add(parameter.ParameterType);
            }
            return parameterList.ToArray();
        }
    }
}
