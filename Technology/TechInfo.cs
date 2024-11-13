using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewGameMode
{
    public class TechInfo
    {
        public int id = -1;
        public string sprite_name = "UNKNOWN";
        public Sprite sprite = new Sprite();

        public List<int> DependTechs = new();

        public Dictionary<string, string> localizeData = new Dictionary<string, string>();

        public string script = string.Empty;

        public bool GetLocalizedText(string region, out string output)
        {
            string empty = string.Empty;
            output = string.Empty;
            if (this.localizeData.TryGetValue(region, out empty))
            {
                string text = LocalizeTextDataModel.instance.GetText(empty);
                output = text;
                return true;
            }
            return false;
        }
    }
}
