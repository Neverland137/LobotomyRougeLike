using Harmony;
using HPHelper;
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
        [HPHelper(typeof(DeployUI), "Init")]
        [HPPostfix]
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
        [HPHelper(typeof(GameManager), "StartGame")]
        [HPPostfix]
        public static void CallRandomChallenge()
        {
            if (GlobalGameManager.instance.gameMode == rougeLike)
            {
                if (PlayerModel.instance.GetDay() + 1 == 36)
                {
                    ///////////
                    List<SefiraEnum> challenges = new List<SefiraEnum>(new SefiraEnum[] { SefiraEnum.GEBURAH});
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
