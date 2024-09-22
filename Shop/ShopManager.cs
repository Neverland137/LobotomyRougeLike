using Harmony;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NewGameMode
{
    public class ShopManager
    {
        /// <summary>
        /// 商店中所有可以购买的等级为1的模因。
        /// </summary>
        public static Dictionary<int, MemeInfo> shopMemeVer1 = new Dictionary<int, MemeInfo>();
        /// <summary>
        /// 商店中所有可以购买的等级为2模因。
        /// </summary>
        public static Dictionary<int, MemeInfo> shopMemeVer2 = new Dictionary<int, MemeInfo>();
        /// <summary>
        /// 当前商店商品列表。
        /// </summary>
        public static List<ShopProduct> NowShopProducts;
        /// <summary>
        /// 商品价格折扣。
        /// </summary>
        public static float ProductDiscount = 1.00f;
        /// <summary>
        /// 刷新商店价格。
        /// </summary>
        public static int RefreshPrice = 50;
        /// <summary>
        /// 购买商品。
        /// </summary>
        /// <param name="shopProduct">商品对象</param>
        public static void BuyProduct(ShopProduct shopProduct)
        {
            if (NowShopProducts.Contains(shopProduct))
            {
                WonderModel.instance.Pay(shopProduct.GetPrice());
                MemeManager.instance.CreateMemeModel(shopProduct.GetMemeInfo().id);
                shopProduct.SetBought(true);
            }
        }
        /// <summary>
        /// 更改商品折扣。
        /// </summary>
        /// <param name="productDiscount">折扣</param>
        public static void ChangeDiscount(int productDiscount)
        {
            ProductDiscount = productDiscount;
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
                        {"saveVer", "ver1" },
                        {"nowShopProducts", NowShopProducts},
                        {"productDiscount", ProductDiscount },
                        {"refreshPrice", RefreshPrice }
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
                NowShopProducts = GenerateShopProducts(12);
                SaveShopData();
            }
            if (shopData != null && shopData["refreshPrice"] != null)
            {
                RefreshPrice = (int)shopData["refreshPrice"];
            }
            if (shopData != null && shopData["productDiscount"] != null)
            {
                ProductDiscount = (int)shopData["productDiscount"];
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
                shopMemeVer1 = MemeManager.instance.all_dic
                    .Where(dic => dic.Value.boss == false)
                    .Where(dic => dic.Value.grade == 1)
                    .ToDictionary(dic => dic.Key, dic => dic.Value);
                shopMemeVer1 = MemeManager.instance.all_dic
                    .Where(dic => dic.Value.boss == false)
                    .Where(dic => dic.Value.grade == 2)
                    .ToDictionary(dic => dic.Key, dic => dic.Value);
            }
            else
            {
                RGDebug.Log("MemeManager.all_dic is null. can't init shop meme.");
            }
        }
        public static void RefreshShop()
        {
            WonderModel.instance.Pay(RefreshPrice);
            NowShopProducts = GenerateShopProducts(12);
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
            Dictionary<int, MemeInfo> tempMemeVer1 = shopMemeVer1
                .Where(dic => dic.Value.duplicate == true || !nowMemeInfo.ContainsValue(dic.Value))
                .ToDictionary(dic => dic.Key, dic => dic.Value);
            Dictionary<int, MemeInfo> tempMemeVer2 = shopMemeVer2
                .Where(dic => dic.Value.duplicate == true || !nowMemeInfo.ContainsValue(dic.Value))
                .ToDictionary(dic => dic.Key, dic => dic.Value);
            List<ShopProduct> shopProducts = new List<ShopProduct>();
            Random random = new Random();
            while (shopProducts.Count < maxCount)
            {
                int[] weight = { ShopProb.MemeVer1Prob, ShopProb.MemeVer2Prob, ShopProb.MemeYEProb };
                int index = Extension.WeightedRandomChoice(weight);
                if (index == 0)
                {
                    int memeIndex = random.Next(tempMemeVer1.Count);
                    MemeInfo selectedMeme = tempMemeVer1[index];
                    ShopProduct shopProduct = new ShopProduct((int)(selectedMeme.price * ProductDiscount), selectedMeme, false);
                    shopProducts.Add(shopProduct);
                    tempMemeVer1.Remove(index);
                }
                else if (index == 1)
                {
                    int memeIndex = random.Next(tempMemeVer2.Count);
                    MemeInfo selectedMeme = tempMemeVer2[index];
                    ShopProduct shopProduct = new ShopProduct((int)(selectedMeme.price * ProductDiscount), selectedMeme, false);
                    shopProducts.Add(shopProduct);
                    tempMemeVer2.Remove(index);
                }
                else
                {
                    MemeInfo YEMeme = MemeManager.GetMemeInfo(10001);
                    ShopProduct shopProduct = new ShopProduct((int)(YEMeme.price * ProductDiscount), YEMeme, false);
                    shopProducts.Add(shopProduct);
                }
            }
            return shopProducts;
        }
    }
}
