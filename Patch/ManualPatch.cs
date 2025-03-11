using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NewGameMode
{
    public class ManualPatch
    {
        public static void PatchMethods(HarmonyInstance instance)
        {
            try
            {
                int num = 0;
                instance.Patch(typeof(CreatureOverloadManager).GetMethod("ActivateOverload", BindingFlags.NonPublic | BindingFlags.Instance), new HarmonyMethod(typeof(EnergyAndOverload_Patch).GetMethod("SetOverloadMultiply")), null, null); num++;

                instance.Patch(typeof(CreatureOverloadManager).GetMethod("ActivateOverload", BindingFlags.NonPublic | BindingFlags.Instance), null, new HarmonyMethod(typeof(EnergyAndOverload_Patch).GetMethod("CallRandomEvent")), null); num++;
            }
            catch (Exception ex)
            {
                Harmony_Patch.logger.Error("Error while patching manual methods: " + ex.Message);
            }
        }
    }
}
