using System.Collections.Generic;

namespace NewGameMode.Diffculty
{
    public static class DifficultyManager
    {
        private static DifficultyStruct nowDifficulty;
        private static int nowIndex;
        private static Dictionary<int, DifficultyStruct> difficultyList = [];
        public static DifficultyStruct GetDifficulty(int index)
        {
            if (difficultyList.ContainsKey(index))
            {
                return difficultyList[index];
            }
            else
            {
                Harmony_Patch.LogError("DifficultyManager:GetDifficulty index is out of range.");
                return null;
            }
        }
        public static DifficultyStruct GetNowDifficulty()
        {
            return nowDifficulty;
        }
        public static int GetNowDifficultyIndex()
        {
            return nowIndex;
        }
        public static void SetDifficulty(int index)
        {
            if (difficultyList.ContainsKey(index))
            {
                nowIndex = index;
                nowDifficulty = difficultyList[index];
            }
            else
            {
                Harmony_Patch.LogError("DifficultyManager:SetDifficulty index is out of range.");
            }
        }
        public static void Init()
        {
            DifficultyDataReader.Init();
            SetDifficulty(0);
        }
        public static void AddDifficulty(int index, DifficultyStruct difficultyStruct)
        {
            difficultyList.Add(index, difficultyStruct);
        }
    }
}
