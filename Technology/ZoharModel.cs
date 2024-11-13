using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGameMode
{
    public static class ZoharModel
    {
        private static int _zohar = 0;

        public static int GetZohar()
        {
            return _zohar;
        }

        public static void AddZohar(int value)
        {
            _zohar += value;
        }

        public static void SetZohar(int value)
        {
            _zohar = value;
        }
        public static void LoadZoharData()
        {
            if (!File.Exists(Harmony_Patch.path + "/Save/ZoharData.dat"))
            {
                SaveZoharData();
                return;
            }
            var DictionaryData = SaveUtil.ReadSerializableFile(Harmony_Patch.path + "/Save/ZoharData.dat");
            _zohar = (int)DictionaryData["ZoharValue"];
        }
        public static void SaveZoharData()
        {
            try
            {
                if (GlobalGameManager.instance.gameMode == Harmony_Patch.rougeLike)
                {
                    Dictionary<string, object> dictionary = new Dictionary<string, object>
                    {
                        {"saveVer", "ver1" },
                        {"ZoharValue", _zohar},
                    };
                    SaveUtil.WriteSerializableFile(Harmony_Patch.path + "/Save/ZoharData.dat", dictionary);
                }
            }

            catch (Exception ex)
            {
                Harmony_Patch.LogError("SaveZoharData Error: " + ex.ToString());
            }
        }
    }
}
