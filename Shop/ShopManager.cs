using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace NewGameMode
{
    public class ShopManager
    {
        /// <summary>
        /// 商店中所有可以购买的模因。
        /// </summary>
        public static Dictionary<int, MemeInfo> shopMeme = new Dictionary<int, MemeInfo>();
        /// <summary>
        /// 当前商店商品列表。
        /// </summary>
        public static List<ShopProduct> NowShopProducts;
        /// <summary>
        /// 购买商品。
        /// </summary>
        /// <param name="shopProduct">商品对象</param>
        public static void BuyProduct(ShopProduct shopProduct)
        {
            if (NowShopProducts.Contains(shopProduct))
            {
                WonderModel.instance.Pay(shopProduct.GetPrice());
                shopProduct.SetBought(true);
            }
        }
        /// <summary>
        /// 保存商店存档信息。
        /// </summary>
        /// <returns></returns>
        public static void SaveShopData()
        {
            try
            {
                if (GlobalGameManager.instance.gameMode == Harmony_Patch.rougeLike)
                {
                    Dictionary<string, object> dictionary = new Dictionary<string, object>
                    {
                        { "saveVer", "ver1" },
                        { "nowShopProducts", NowShopProducts}
                    };
                    SaveUtil.WriteSerializableFile(Harmony_Patch.path + "/Save/ShopData.dat", dictionary);
                }
            }

            catch (Exception ex)
            {
                File.WriteAllText(Harmony_Patch.path + "/SaveShopDataError.txt", ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }
        /// <summary>
        /// 加载商店存档信息。
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, object> LoadShopData()
        {
            return SaveUtil.ReadSerializableFile(Harmony_Patch.path + "/Save/ShopData.dat");
        }
        /// <summary>
        /// 进入游戏后初始化商店。
        /// </summary>
        public static void InitShop()
        {
            Dictionary<string, object> shopData = LoadShopData();
            if (shopData != null && shopData["nowShopProducts"] != null)
            {
                NowShopProducts = shopData["nowShopProducts"] as List<ShopProduct>;
            }
            else
            {
                NowShopProducts = GenerateShopProducts(8);
                SaveShopData();
            }
        }
        /// <summary>
        /// 初始化商店可以购买的所有模因。
        /// </summary>
        public static void InitShopMeme()
        {
            // .Where 用来过滤不符合要求的模因。
            // .ToDictionary 用来将过滤后的模因转换为字典。
            // 需要满足：不是boss模因，等级为1或2。
            // 使用了Linq语法。
            if (MemeManager.instance.all_dic != null)
            {
                shopMeme = MemeManager.instance.all_dic
                    .Where(dic => dic.Value.boss == false)
                    .Where(dic => dic.Value.grade == 1 || dic.Value.grade == 2)
                    .ToDictionary(dic => dic.Key, dic => dic.Value);
            }
            else
            {
                RGDebug.Log("MemeManager.all_dic is null. can't init shop meme.");
            }
        }
        /// <summary>
        /// 生成商店商品列表。
        /// </summary>
        /// <param name="maxCount">生成多少商品</param>
        /// <returns></returns>
        public static List<ShopProduct> GenerateShopProducts(int maxCount)
        {
            // 获取当前所有Meme的信息
            Dictionary<int, MemeInfo> nowMemeInfo = MemeManager.instance.current_dic
                .ToDictionary(dic => dic.Key, dic => dic.Value.metaInfo);
            // 过滤掉已经拥有的模因（duplicate为true也可以通过）
            Dictionary<int, MemeInfo> tempMeme = shopMeme
                .Where(dic => dic.Value.duplicate == true || !nowMemeInfo.ContainsValue(dic.Value))
                .ToDictionary(dic => dic.Key, dic => dic.Value);
            List<ShopProduct> shopProducts = new List<ShopProduct>();
            Random random = new Random();
            while (shopProducts.Count < maxCount && tempMeme.Count > 0)
            {
                int index = random.Next(tempMeme.Count);
                MemeInfo selectedMeme = tempMeme[index];
                ShopProduct shopProduct = new ShopProduct(selectedMeme.price, selectedMeme, false);
                shopProducts.Add(shopProduct);
                tempMeme.Remove(index);
            }
            return shopProducts;
        }
    }
}
