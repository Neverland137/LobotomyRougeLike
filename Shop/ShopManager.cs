using DG.Tweening;
using Harmony;
using NewGameMode.Meme;
using Steamworks.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace NewGameMode
{
    public class ShopManager
    {
        public static GameObject shopScene;

        /// <summary>
        /// 商店中所有可以购买的等级为1的模因，key是模因id。
        /// </summary>
        public static Dictionary<int, MemeInfo> shopMemeVer1 = new Dictionary<int, MemeInfo>();
        /// <summary>
        /// 商店中所有可以购买的等级为2模因，key是模因id。
        /// </summary>
        public static Dictionary<int, MemeInfo> shopMemeVer2 = new Dictionary<int, MemeInfo>();
        /// <summary>
        /// 商店中所有可以购买的诅咒模因，key是模因id。
        /// </summary>
        public static Dictionary<int, MemeInfo> shopMemeCurse = new Dictionary<int, MemeInfo>();

        public static int[] specialMemeid = { 1, 2, 3 };//1：随机A武，2：随机A甲，3：随机员工
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

        public static int specialCnt = 4;
        public static int defaultMemeId = 100;


        public static void InitShopScene()
        {
            try
            {
                //对于memeScene的基础设置
                AssetBundle bundle = AssetBundle.LoadFromFile(Harmony_Patch.path + "/AssetsBundle/universal_meme_scene");
                shopScene = UnityEngine.Object.Instantiate(bundle.LoadAsset<GameObject>("UniversalMemeScene"));
                shopScene.transform.SetParent(GameStatusUI.GameStatusUI.Window.gameObject.transform.Find("Canvas"));
                shopScene.transform.localPosition = new Vector3(0, 0, 0);
                shopScene.transform.localScale = new Vector3(0, 0, 1);
                //shopScene.AddComponent<Canvas>();
                shopScene.SetActive(false);
                bundle.Unload(false);//关闭AB包，但是保留已加载的资源

                //对于子对象的详细设置，例如哪些按钮应当播放哪些音效，以及对于文字的设置
                //离开按钮
                GameObject exitButton = shopScene.transform.Find("ExitButton").gameObject;
                UniversalButtonIntereaction btIn1 = exitButton.AddComponent<UniversalButtonIntereaction>();
                btIn1.pointerEnterSound = false;
                //关闭商店页面时，详情页也一并隐藏，避免下次打开商店后出现上次商店的商品
                exitButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => {
                    btIn1.Click(true, false, false);
                    shopScene.transform.DOScale(new Vector3(0, 0, 1), 1).SetEase(Ease.OutExpo).SetUpdate(true).OnComplete(() => {
                        shopScene.SetActive(false);
                        shopScene.transform.Find("WonderandDetail").Find("Detail").gameObject.SetActive(false);
                    });
                });
                exitButton.transform.Find("Text").gameObject.AddComponent<LocalizeTextLoadScriptWithOutFontLoadScript>().id = "Shop_ExitButton";
                exitButton.transform.Find("Text").gameObject.AddComponent<FontLoadScript>();
                //单个模因按钮
                //GameObject singleMemeButton = MemeManager.memeScene.transform.Find("MemeButtons").Find("Buttons").GetChild(0).gameObject;

                //详情页的文本设置
                GameObject detail = shopScene.transform.Find("WonderandDetail").Find("Detail").gameObject;
                detail.transform.Find("Name").gameObject.AddComponent<LocalizeTextLoadScriptWithOutFontLoadScript>();
                detail.transform.Find("Name").gameObject.AddComponent<FontLoadScript>();
                detail.transform.Find("Desc").gameObject.AddComponent<LocalizeTextLoadScriptWithOutFontLoadScript>();
                detail.transform.Find("Desc").gameObject.AddComponent<FontLoadScript>();
                detail.transform.Find("ScrollSpecialDesc").Find("SpecialDesc").gameObject.AddComponent<LocalizeTextLoadScriptWithOutFontLoadScript>();
                detail.transform.Find("ScrollSpecialDesc").Find("SpecialDesc").gameObject.AddComponent<FontLoadScript>();
                detail.SetActive(false);
                //涉及价格的不能加那俩组件
                /*
                detail.transform.Find("BuyButton").Find("Text").gameObject.AddComponent<LocalizeTextLoadScriptWithOutFontLoadScript>();
                detail.transform.Find("BuyButton").Find("Text").gameObject.AddComponent<FontLoadScript>();
                */
                //奇思设置

                GameObject wonder = shopScene.transform.Find("WonderandDetail").Find("Wonder").gameObject;
                wonder.transform.Find("Text").gameObject.AddComponent<LocalizeTextLoadScriptWithOutFontLoadScript>();
                wonder.transform.Find("Text").gameObject.AddComponent<UpdateWonder>();


                //购买按钮设置：价格和音效需要在点击模因按钮后再设置
                GameObject buy = shopScene.transform.Find("WonderandDetail").Find("Detail").Find("BuyButton").gameObject;
                UniversalButtonIntereaction btIn2 = buy.AddComponent<UniversalButtonIntereaction>();
                btIn2.pointerEnterSound = true;
                buy.transform.Find("Text").gameObject.AddComponent<LocalizeTextLoadScriptWithOutFontLoadScript>();

                //刷新按钮设置
                GameObject refresh = shopScene.transform.Find("RefreshButton").gameObject;
                UniversalButtonIntereaction btIn3 = refresh.AddComponent<UniversalButtonIntereaction>();
                btIn3.pointerEnterSound = true;
                refresh.transform.Find("Text").gameObject.AddComponent<LocalizeTextLoadScriptWithOutFontLoadScript>();
                refresh.transform.Find("Text").gameObject.GetComponent<Text>().text = LocalizeTextDataModel.instance.GetText("Shop_Refresh") + LocalizeTextDataModel.instance.GetText("Meme_Wonder") + "-" + RefreshPrice.ToString();
                refresh.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => 
                {
                    if (RefreshShop())
                    {
                        refresh.GetComponent<UniversalButtonIntereaction>().Click(true, false, true);
                    }
                    else
                    {
                        refresh.GetComponent<UniversalButtonIntereaction>().Click(false, true, true);
                    }
                });

                InitShopMeme();
                InitShop();
            }
            catch (Exception ex)
            {
                Harmony_Patch.YKMTLogInstance.Error(ex);
            }
        }

        /// <summary>
        /// 购买商品。
        /// </summary>
        /// <param name="shopProduct">商品对象</param>
        public static bool BuyProduct(ShopProduct shopProduct)
        {
            if (NowShopProducts.Contains(shopProduct) && !shopProduct.IsBought())
            {
                if (WonderModel.instance.Pay(shopProduct.GetPrice()))
                {
                    MemeManager.instance.CreateMemeModel(shopProduct.GetMemeInfo().id);
                    shopProduct.SetBought(true);
                    string name_id = "";
                    shopProduct.GetMemeInfo().localizeData.TryGetValue("name", out name_id);
                    Harmony_Patch.LogInfo("BuyProduct:" + name_id);
                    return true;
                }
            }
            return false;
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
            /*
            Dictionary<string, object> shopData = LoadShopData();
            if (shopData != null && shopData["nowShopProducts"] != null)
            {
                NowShopProducts = shopData["nowShopProducts"] as List<ShopProduct>;
            }
            else
            */
            {
                NowShopProducts = GenerateShopProducts(12);
                ShowShopProducts(NowShopProducts);
                SaveShopData();
            }
            /*
            if (shopData != null && shopData["refreshPrice"] != null)
            {
                RefreshPrice = (int)shopData["refreshPrice"];
            }
            if (shopData != null && shopData["productDiscount"] != null)
            {
                ProductDiscount = (int)shopData["productDiscount"];
            }
            */
        }
        /// <summary>
        /// 初始化商店可以购买的所有模因。
        /// </summary>
        public static void InitShopMeme()
        {
            // 需要满足：不是boss模因，等级为1或2，不是随机员工和随机装备，或者是诅咒模因。
            if (MemeManager.instance.all_dic != null)
            {
                foreach (var dic in MemeManager.instance.all_dic)
                {
                    if (!dic.Value.boss && dic.Value.grade == 1 && !dic.Value.curse && !specialMemeid.Contains(dic.Value.id))
                    {
                        shopMemeVer1.Add(dic.Key, dic.Value);
                    }
                    if (!dic.Value.boss && dic.Value.grade == 2 && !dic.Value.curse && !specialMemeid.Contains(dic.Value.id))
                    {
                        shopMemeVer2.Add(dic.Key, dic.Value);
                    }
                    if (!dic.Value.boss && dic.Value.curse)//诅咒模因
                    {
                        shopMemeCurse.Add(dic.Key, dic.Value);
                    }
                }
            }
            else
            {
                Harmony_Patch.YKMTLogInstance.Error("MemeManager.all_dic is null. can't init shop meme.");
            }
        }

        /// <summary>
        /// 返回是否刷新成功
        /// </summary>
        /// <returns></returns>
        public static bool RefreshShop()
        {
            if (WonderModel.instance.Pay(RefreshPrice))
            {
                NowShopProducts.Clear();
                NowShopProducts = GenerateShopProducts(12);
                ShowShopProducts(NowShopProducts);
                return true;
            }
            return false;
        }
        /// <summary>
        /// 生成商店商品列表。
        /// </summary>
        /// <param name="maxCount">生成多少商品，诅咒要不要无视叠加情况生成？（目前没做）</param>
        /// <returns></returns>
        public static List<ShopProduct> GenerateShopProducts(int maxCount, bool randomEquip = true, bool randomAgent = true, bool curse = true)
        {
            // 获取当前所有Meme的信息
            int num = 0;
            List<ShopProduct> shopProducts = new List<ShopProduct>();
            List<ShopProduct> shopProducts_sp = new List<ShopProduct>();
            try
            {
                Dictionary<int, MemeInfo> nowMemeInfo = [];//模因id和模因信息，商店刷新用模因信息判断
                foreach (var dic in MemeManager.instance.current_dic)
                {
                    try
                    {
                        nowMemeInfo.Add(dic.Key, dic.Value.metaInfo);
                    }
                    finally
                    {
                        //如果不加这个try会导致nowMemeInfo里重复添加同一个模因而报错，后续程序不执行，商店刷新0个物品
                    }
                }
                num++;
                // 过滤掉已经拥有的模因（duplicate为true也可以通过）
                Dictionary<int, MemeInfo> tempMemeVer1 = [];
                foreach (var dic in shopMemeVer1)
                {
                    if ((dic.Value.duplicate == true || !nowMemeInfo.ContainsValue(dic.Value)) && dic.Value.CheckRequire())
                    {
                        tempMemeVer1.Add(dic.Key, dic.Value);//模因id和信息
                    }
                }
                num++;
                Dictionary<int, MemeInfo> tempMemeVer2 = [];
                foreach (var dic in shopMemeVer2)
                {
                    if (dic.Value.duplicate == true || !nowMemeInfo.ContainsValue(dic.Value) && dic.Value.CheckRequire())
                    {
                        tempMemeVer2.Add(dic.Key, dic.Value);//模因id和信息
                    }
                }
                num++;
                Dictionary<int, MemeInfo> tempMemeCurse = [];
                foreach (var dic in shopMemeCurse)
                {
                    if (dic.Value.duplicate == true || !nowMemeInfo.ContainsValue(dic.Value) && dic.Value.CheckRequire())
                    {
                        tempMemeCurse.Add(dic.Key, dic.Value);//模因id和信息
                    }
                }
                num++;

                while (shopProducts.Count < maxCount - specialCnt)//此处生成普通模因：一级二级和热水壶
                {
                    int[] weight = { ShopProb.MemeVer1Prob, ShopProb.MemeVer2Prob, ShopProb.MemeYEProb };
                    int index = Extension.WeightedRandomChoice(weight);//返回weight的索引
                    if (index == 0 && tempMemeVer1.Count > 0)//等级为1的商品
                    {
                        int memeIndex = Harmony_Patch.customRandom.NextInt(0, tempMemeVer1.Count);
                        int memeId = tempMemeVer1.Keys.ToArray()[memeIndex];
                        MemeInfo selectedMeme = tempMemeVer1[memeId];
                        ShopProduct shopProduct = new ShopProduct((int)(selectedMeme.price * ProductDiscount), selectedMeme, false);
                        shopProducts.Add(shopProduct);
                        tempMemeVer1.Remove(memeId);
                    }
                    else if (index == 1 && tempMemeVer2.Count > 0)//等级为2的商品
                    {
                        int memeIndex = Harmony_Patch.customRandom.NextInt(0, tempMemeVer2.Count);
                        int memeId = tempMemeVer2.Keys.ToArray()[memeIndex];
                        MemeInfo selectedMeme = tempMemeVer2[memeId];
                        ShopProduct shopProduct = new ShopProduct((int)(selectedMeme.price * ProductDiscount), selectedMeme, false);
                        shopProducts.Add(shopProduct);
                        tempMemeVer2.Remove(memeId);
                    }
                    else//以撒的牛奶
                    {
                        MemeInfo YEMeme = MemeManager.GetMemeInfo(defaultMemeId);
                        ShopProduct shopProduct = new ShopProduct((int)(YEMeme.price * ProductDiscount), YEMeme, false);
                        shopProducts.Add(shopProduct);
                    }
                }
                num++;

                while (shopProducts_sp.Count < specialCnt)//此处生成特殊模因：随机A武，随机A甲，随机员工，诅咒
                {
                    int[] weight = { ShopProb.MemeRandomEquipProb, ShopProb.MemeRandomAgentProb, ShopProb.MemeCurseProb };
                    int index = Extension.WeightedRandomChoice(weight);//返回weight的索引
                    if (index == 0)//随机装备
                    {
                        if (Harmony_Patch.customRandom.NextFloat() <= 0.5f)//A武
                        {
                            MemeInfo selectedMeme;
                            MemeManager.instance.all_dic.TryGetValue(1, out selectedMeme);
                            ShopProduct shopProduct = new ShopProduct((int)(selectedMeme.price * ProductDiscount), selectedMeme, false);
                            shopProducts_sp.Add(shopProduct);
                        }
                        else//A甲
                        {
                            MemeInfo selectedMeme;
                            MemeManager.instance.all_dic.TryGetValue(2, out selectedMeme);
                            ShopProduct shopProduct = new ShopProduct((int)(selectedMeme.price * ProductDiscount), selectedMeme, false);
                            shopProducts_sp.Add(shopProduct);
                        }
                    }
                    else if (index == 1)//随机员工
                    {
                        MemeInfo selectedMeme;
                        MemeManager.instance.all_dic.TryGetValue(3, out selectedMeme);
                        ShopProduct shopProduct = new ShopProduct((int)(selectedMeme.price * ProductDiscount), selectedMeme, false);
                        shopProducts_sp.Add(shopProduct);
                    }
                    else if (index == 2 && tempMemeCurse.Count > 0)//诅咒
                    {
                        int memeIndex = Harmony_Patch.customRandom.NextInt(0, tempMemeCurse.Count);
                        int memeId = tempMemeCurse.Keys.ToArray()[memeIndex];
                        MemeInfo selectedMeme = tempMemeCurse[memeId];
                        ShopProduct shopProduct = new ShopProduct((int)(selectedMeme.price * ProductDiscount), selectedMeme, false);
                        shopProducts_sp.Add(shopProduct);
                        tempMemeCurse.Remove(memeId);
                    }
                    else//以撒的牛奶，理论上来说不会生成
                    {
                        MemeInfo YEMeme = MemeManager.GetMemeInfo(defaultMemeId);
                        ShopProduct shopProduct = new ShopProduct((int)(YEMeme.price * ProductDiscount), YEMeme, false);
                        shopProducts_sp.Add(shopProduct);
                    }
                }
                shopProducts.AddRange(shopProducts_sp);
                num++;
            }
            catch (Exception ex)
            {
                Harmony_Patch.YKMTLogInstance.Error(num.ToString());
                Harmony_Patch.YKMTLogInstance.Error(ex);
            }
            return shopProducts;
        }

        public static void ShowShopProducts(List<ShopProduct> products)
        {
            int num = 0;
            try
            {
                Transform parentTransform = shopScene.transform.Find("MemeButtons").Find("Buttons");
                for (int i = 0; i < parentTransform.childCount; i++)//遍历并摧毁商店页面中所有商品按钮
                {
                    GameObject.Destroy(parentTransform.GetChild(i).gameObject);
                }

                Harmony_Patch.LogInfo("Show" + products.Count.ToString() + "ShopProducts" );

                foreach (ShopProduct shopProduct in products)
                {
                    string name_id;
                    shopProduct.GetMemeInfo().localizeData.TryGetValue("name", out name_id);
                    string desc_id;
                    shopProduct.GetMemeInfo().localizeData.TryGetValue("desc", out desc_id);
                    string special_desc_id;
                    shopProduct.GetMemeInfo().localizeData.TryGetValue("special_desc", out special_desc_id);

                    AssetBundle buttonBundle = AssetBundle.LoadFromFile(Harmony_Patch.path + "/AssetsBundle/single_meme_button"); num++;
                    GameObject memeButton = UnityEngine.Object.Instantiate(buttonBundle.LoadAsset<GameObject>("SingleMemeButton")); num++;
                    buttonBundle.Unload(false);//关闭AB包，但是保留已加载的资源
                    memeButton.SetActive(true);
                    memeButton.transform.SetParent(shopScene.transform.Find("MemeButtons").Find("Buttons")); num++;
                    memeButton.transform.localScale = new Vector3(1f, 1f, 1f); num++;

                    //设置按钮本身的文字、图片、音效和移动效果
                    memeButton.transform.Find("Text").gameObject.AddComponent<LocalizeTextLoadScriptWithOutFontLoadScript>().id = name_id;
                    memeButton.transform.Find("Text").gameObject.AddComponent<FontLoadScript>();
                    memeButton.transform.Find("Image").gameObject.GetComponent<UnityEngine.UI.Image>().sprite = shopProduct.GetMemeInfo().sprite; num++;
                    memeButton.GetComponent<UnityEngine.UI.Image>().sprite = MemeManager.memeOutlook;
                    memeButton.AddComponent<UniversalButtonIntereaction>().pointerEnterSound = true;


                    //点击后设置详情
                    GameObject detail = shopScene.transform.Find("WonderandDetail").Find("Detail").gameObject; num++;
                    memeButton.GetComponent<UnityEngine.UI.Button>().onClick.RemoveAllListeners();
                    memeButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>
                    {
                        memeButton.GetComponent<UniversalButtonIntereaction>().Click(true, false, true);
                        detail.SetActive(true);
                        detail.transform.localScale = new Vector3(0, 0, 1);
                        detail.transform.DOScale(new Vector3(1, 1, 1), 0.3f).SetEase(Ease.OutExpo).SetUpdate(true);
                        //设置文字
                        detail.transform.Find("Name").gameObject.GetComponent<LocalizeTextLoadScriptWithOutFontLoadScript>().SetText(name_id);
                        detail.transform.Find("Desc").gameObject.GetComponent<LocalizeTextLoadScriptWithOutFontLoadScript>().SetText(desc_id);
                        detail.transform.Find("ScrollSpecialDesc").Find("SpecialDesc").gameObject.GetComponent<LocalizeTextLoadScriptWithOutFontLoadScript>().SetText(special_desc_id);
                        //设置图片
                        detail.transform.Find("Image").gameObject.GetComponent<UnityEngine.UI.Image>().sprite = shopProduct.GetMemeInfo().sprite;
                        //设置购买按钮的文字和点击逻辑
                        GameObject buy = detail.transform.Find("BuyButton").gameObject;
                        if (shopProduct.GetPrice() > 0)
                        {
                            buy.transform.Find("Text").gameObject.GetComponent<Text>().text = LocalizeTextDataModel.instance.GetText("Shop_Buy") + LocalizeTextDataModel.instance.GetText("Meme_Wonder") + "-" + shopProduct.GetPrice().ToString();
                        }
                        else
                        {
                            buy.transform.Find("Text").gameObject.GetComponent<Text>().text = LocalizeTextDataModel.instance.GetText("Shop_Buy") + LocalizeTextDataModel.instance.GetText("Meme_Wonder") + "+" + shopProduct.GetPrice().ToString();
                        }
                        buy.GetComponent<UnityEngine.UI.Button>().onClick.RemoveAllListeners();
                        buy.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>
                        {
                            Harmony_Patch.LogInfo("BuyProduct:" + name_id + "    Id:" + shopProduct.GetMemeInfo().id);
                            if (BuyProduct(shopProduct))
                            {
                                buy.GetComponent<UniversalButtonIntereaction>().Click(true, false, true);
                                GameObject.Destroy(memeButton);
                                LayoutRebuilder.ForceRebuildLayoutImmediate(shopScene.GetComponentInChildren<GridLayoutGroup>().transform.parent as RectTransform);
                            }
                            else
                            {
                                buy.GetComponent<UniversalButtonIntereaction>().Click(false, true, true);
                            }
                        });
                    }); num++;
                }
                Harmony_Patch.LogInfo("ShowShopProductsEnd");
            }
            catch (Exception ex)
            {
                Harmony_Patch.YKMTLogInstance.Error(ex);
            }
        }
    }
}