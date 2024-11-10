using NewGameMode.Diffculty;
using Steamworks.Ugc;
using System;
using System.Collections.Generic;

namespace NewGameMode
{
    public static class FurnaceManager
    {
        public static readonly Dictionary<int, int[]> MemeWonderTable = new()
        {
            { 1, new int[] { 20, 200 } },
            { 2, new int[] { 50, 500 } },
            { 3, new int[] { 100, 1000 } }
        };
        public static readonly Dictionary<int, int> MemeBoomAdderTable = new()
        {
            {1, 0 },
            {2, 3 },
            {3, 7 }
        };
        public static readonly int[] BaseBoomProb = [3, 15];
        public static readonly int[] UpLevel1RecipeProb = [70, 90];
        /// <summary>
        /// 合成模因。
        /// </summary>
        /// <param name="memeModel1">第一个模因</param>
        /// <param name="memeModel2">第二个模因</param>
        /// <param name="memeLevel">模因等级</param>
        /// <param name="spendWonder">付出的奇思</param>
        public static void RecipeMeme(MemeModel memeModel1, MemeModel memeModel2, int memeLevel, int spendWonder)
        {
            int afterLevel = CalculateMemeLevel(memeLevel, spendWonder);
            MemeManager.instance.RemoveMemeModel(memeModel1.metaInfo.id);
            MemeManager.instance.RemoveMemeModel(memeModel2.metaInfo.id);
            MemeManager.instance.CreateMemeModel(GetRandomMemeUnInhand(afterLevel).id);
            WonderModel.instance.Pay(spendWonder);
        }
        public static MemeInfo GetRandomMemeUnInhand(int memeLevel)
        {
            List<MemeInfo> templist = new();
            foreach (var meme in MemeManager.instance.uninhand_list)
            {
                if (meme.grade == memeLevel)
                {
                    templist.Add(meme);
                }
            }
            int randomIndex = UnityEngine.Random.Range(0, templist.Count);
            var randomItem = templist[randomIndex];
            return randomItem;
        }
        /// <summary>
        /// 计算模因炸炉和升一级与升二级概率，可用于UI
        /// </summary>
        /// <param name="memeLevel">模因等级</param>
        /// <param name="spendWonder">投入奇思数</param>
        /// <returns></returns>
        public static GetRandomMemeUnInhandReturn CalcuateMemeProb(int memeLevel, int spendWonder)
        {
            var nowDifficulty = DifficultyManager.GetNowDifficulty();
            var result = new GetRandomMemeUnInhandReturn();
            float boomAdder = nowDifficulty.FurnaceBoomAdder();
            float boomProb = CalculatePercentageInRange(spendWonder, MemeWonderTable[memeLevel][0], MemeWonderTable[memeLevel][1]) * (BaseBoomProb[1] - BaseBoomProb[0]) + BaseBoomProb[0] + MemeBoomAdderTable[memeLevel] + boomAdder * 100;
            // 检查炸炉概率范围
            if (boomProb < BaseBoomProb[0])
            {
                boomProb = BaseBoomProb[0];
            }
            else if (boomProb > BaseBoomProb[1])
            {
                boomProb = BaseBoomProb[1];
            }
            result.BoomProb = (int)Math.Round(boomProb);
            if (memeLevel != 3)
            {
                float UpLevel1RecipeProbAdder = nowDifficulty.UpLevel1RecipeProbAdder();
                float UpLevel1Prob = CalculatePercentageInRange(spendWonder, MemeWonderTable[memeLevel][0], MemeWonderTable[memeLevel][1]) * (UpLevel1RecipeProb[1] - UpLevel1RecipeProb[0]) + UpLevel1RecipeProb[0] + UpLevel1RecipeProbAdder * 100;
                result.UpLevel1Prob = (int)Math.Round(UpLevel1Prob);
                result.UpLevel2Prob = (int)Math.Round(100 - UpLevel1Prob);
            }
            else
            {
                result.UpLevel1Prob = 100;
                result.UpLevel2Prob = 0;
            }
            return result;
        }
        /// <summary>
        /// 计算两个模因合成后的模因等级
        /// </summary>
        /// <param name="memeLevel">模因等级</param>
        /// <param name="spendWonder">投入奇思数</param>
        /// <returns>合成的模因等级，若返回-1，则表示炸炉</returns>
        public static int CalculateMemeLevel(int memeLevel, int spendWonder)
        {
            var nowDifficulty = DifficultyManager.GetNowDifficulty();
            float boomAdder = nowDifficulty.FurnaceBoomAdder();
            float boomProb = CalculatePercentageInRange(spendWonder, MemeWonderTable[memeLevel][0], MemeWonderTable[memeLevel][1]) * (BaseBoomProb[1] - BaseBoomProb[0]) + BaseBoomProb[0] + MemeBoomAdderTable[memeLevel] + boomAdder * 100;
            // 检查炸炉概率范围
            if (boomProb < BaseBoomProb[0])
            {
                boomProb = BaseBoomProb[0];
            }
            else if (boomProb > BaseBoomProb[1])
            {
                boomProb = BaseBoomProb[1];
            }
            if (UnityEngine.Random.Range(0f, 100f) < boomProb)
            {
                // 炸咯
                return -1;
            }
            else if (memeLevel != 3)
            {
                float UpLevel1RecipeProbAdder = nowDifficulty.UpLevel1RecipeProbAdder();
                float UpLevel1Prob = CalculatePercentageInRange(spendWonder, MemeWonderTable[memeLevel][0], MemeWonderTable[memeLevel][1]) * (UpLevel1RecipeProb[1] - UpLevel1RecipeProb[0]) + UpLevel1RecipeProb[0] + UpLevel1RecipeProbAdder * 100;
                if (UnityEngine.Random.Range(0f, 100f) < UpLevel1Prob)
                {
                    return memeLevel + 1;
                }
                return memeLevel + 2;
            }
            else
            {
                return memeLevel + 1;
            }
        }
        public static float CalculatePercentageInRange(int number, int min, int max)
        {
            if (max == min)
            {
                throw new ArgumentException("最大值和最小值不能相等。");
            }
            if (number < min) return 0;
            if (number > max) return 1;

            return (number - min) / (max - min);
        }
    }
    public class GetRandomMemeUnInhandReturn
    {
        public int BoomProb { get; set; }
        public int UpLevel1Prob { get; set; }
        public int UpLevel2Prob { get; set; }
    }
}
