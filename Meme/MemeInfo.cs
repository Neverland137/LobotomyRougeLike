using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewGameMode
{
    [Serializable]
    public class MemeInfo
    {
        public int id = -1;
        public string sprite_name = "UNKNOWN";

        [NonSerialized]
        public Sprite sprite = new Sprite();

        public List<MemeRequire> requires;
        public bool satisfy_all = false;

        public bool duplicate = false;
        public bool curse = false;
        public bool boss = false;
        public int suit = 0;

        public string script = "";
        public int grade = 1;

        public int price = 0;

        public Dictionary<string, string> localizeData = new Dictionary<string, string>();

        [NonSerialized]
        public string modid;



        public override bool Equals(object obj)
        {
            return Equals(obj as MemeInfo);
        }

        public bool Equals(MemeInfo info)
        {
            if (this.id == info.id)
            {
                if (this.sprite_name == info.sprite_name)
                {
                    if (this.satisfy_all == info.satisfy_all)
                    {
                        if (this.duplicate == info.duplicate)
                        {
                            if (this.curse == info.curse)
                            {
                                if (this.boss == info.boss)
                                {
                                    if (this.suit == info.suit)
                                    {
                                        if (this.script == info.script && this.grade == info.grade && this.price == info.price)
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }
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

        /// <summary>
        /// 满足条件时返回真
        /// </summary>
        /// <returns></returns>
        public bool CheckRequire()
        {
            if (requires == null || requires.Count == 0)
            {
                return true;
            }
            foreach (MemeRequire require in requires)//检查每一条模因需求是否满足
            {
                if (require.type == MemeRequireType.DAY)
                {
                    if (PlayerModel.instance.GetDay() + 1 >= require.value)//如果满足需求，直接返回false
                    {
                        require.check = true;
                    }
                }
                if (require.type == MemeRequireType.EQUIP)
                {
                    foreach (EquipmentModel equipment in InventoryModel.Instance.equipList)
                    {
                        if (equipment.metaInfo.id == require.value)//如果需求满足，跳过本条需求
                        {
                            require.check = true;
                        }
                    }
                }
                if (require.type == MemeRequireType.ABNORMALITY)
                {
                    foreach (CreatureModel creature in CreatureManager.instance.GetCreatureList())
                    {
                        if (creature.metaInfo.id == require.value)//如果需求满足，跳过本条需求
                        {
                            require.check = true;
                        }
                    }
                }
                if (require.type == MemeRequireType.MEME)
                {
                    foreach (KeyValuePair<int, MemeModel> pair in MemeManager.instance.current_dic)
                    {
                        if (pair.Value.metaInfo.id == require.value)//如果模因的id正确
                        {
                            require.check = true;
                        }
                    }
                }
            }

            if (satisfy_all)
            {
                foreach (MemeRequire require in requires)
                {
                    if (require.check == false)
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                foreach (MemeRequire require in requires)
                {
                    if (require.check == true)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
    }
}
