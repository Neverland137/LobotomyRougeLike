using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NewGameMode
{
    public class Challenge_Patch
    {
        public static GameMode rougeLike = (GameMode)666666;
        public Challenge_Patch(HarmonyInstance instance)
        {
            instance.Patch(typeof(DeployUI).GetMethod("Init"), null, new HarmonyMethod(typeof(Challenge_Patch).GetMethod("DeployUI_Init")));
            instance.Patch(typeof(GameManager).GetMethod("StartGame", AccessTools.all), null, new HarmonyMethod(typeof(Challenge_Patch).GetMethod("CallRandomChallenge")), null);
        }
        public static void DeployUI_Init()
        {
            if (GlobalGameManager.instance.gameMode == rougeLike)
            {
                if (PlayerModel.instance.GetDay() + 1 == 40)
                {
                    SefiraBossUI.Instance.OnEnterSefiraBossSession();
                }
            }
        }
        public static void CallRandomChallenge()
        {
            if (GlobalGameManager.instance.gameMode == rougeLike)
            {
                if (PlayerModel.instance.GetDay() + 1 == 40)
                {
                    ///////////
                    List<SefiraEnum> challenges = new List<SefiraEnum>(new SefiraEnum[] { SefiraEnum.GEBURAH, SefiraEnum.BINAH });
                    foreach (Assembly assembly in Add_On.instance.AssemList)
                    {
                        if (assembly.Location.ToLower().Contains("bluearchive"))
                        {
                            challenges.Add((SefiraEnum)1782);
                        }
                    }
                    SefiraBossManager.Instance.SetActivatedBoss(challenges[Harmony_Patch.customRandom.NextInt(0, challenges.Count)]);
                    ///////////
                    SefiraBossManager.Instance.OnStageStart();
                }
            }
        }
    }
}
