using Harmony;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGameMode.Meme
{
    public class Meme_Patch
    {
        public static GameMode rougeLike = (GameMode)666666;
        public Meme_Patch(HarmonyInstance instance)
        {
            int num = 0;
            try
            {
                instance.Patch(typeof(GameStaticDataLoader).GetMethod("LoadStaticData"), null, new HarmonyMethod(typeof(Meme_Patch).GetMethod("LoadAllInfo"))); num++;

                instance.Patch(typeof(ConsoleScript).GetMethod("GetHmmCommand", AccessTools.all), new HarmonyMethod(typeof(Meme_Patch).GetMethod("GetAllCommand", AccessTools.all)), null, null); num++;

                instance.Patch(typeof(GameManager).GetMethod("StartStage"), null, new HarmonyMethod(typeof(Meme_Patch).GetMethod("Meme_OnStageStart"))); num++;
                instance.Patch(typeof(GameManager).GetMethod("Release", AccessTools.all), null, new HarmonyMethod(typeof(Meme_Patch).GetMethod("Meme_OnStageRelease"))); num++;
            }
            catch (Exception ex)
            {
                File.WriteAllText(Harmony_Patch.path + "/MemePatchError.txt", num + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }
        public static void LoadAllInfo()
        {
            MemeManager.LoadAllInfo();
        }

        public static bool GetAllCommand(string cmd, ref string __result)
        {

            string[] array = cmd.Split(new char[]
            {
                ' '
            });

            if (array.Length != 4)
            {
                return true;
            }

            string flag = array[0].ToLower();
            string type = array[1].ToLower().Trim();//指定操作目标，比如模因和奇思
            string type2 = array[2].ToLower().Trim();//指定具体要做的操作，比如添加和移除
            int value = Convert.ToInt32(array[3].ToLower().Trim());

            if (flag == "ykmt")
            {
                if (type == "meme")
                {
                    if (type2 == "add")
                    {
                        MemeManager.instance.CreateMemeModel(value);
                        __result = "";
                        return false;
                    }
                    else if (type2 == "remove")
                    {
                        MemeManager.instance.RemoveMemeModel(value);
                        __result = "";
                        return false;
                    }
                }

            }

            return true;
        }


        public static void Meme_OnStageStart()
        {
            MemeManager.instance.OnStageStart();
        }
        public static void Meme_OnStageRelease()
        {
            MemeManager.instance.OnStageRelease();
        }

    }
}
