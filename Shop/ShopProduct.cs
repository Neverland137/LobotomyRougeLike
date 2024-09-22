using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGameMode
{
    // 既然用的都是模因信息，我为什么还要写这个类
    public class ShopProduct
    {
        public ShopProduct(int price, MemeInfo shopMemeInfo, bool alreadyBuy = false)
        {
            Price = price;
            ShopMemeInfo = shopMemeInfo;
            alreadyBuy = false;
        }
        /// <summary>
        /// 商品价格
        /// </summary>
        private int Price { get; }
        /// <summary>
        /// 模因信息
        /// </summary>
        private MemeInfo ShopMemeInfo { get; }
        private bool AlreadyBuy { get; set; }

        /// <summary>
        /// 获取此商品的价格
        /// </summary>
        /// <returns></returns>
        public int GetPrice()
        {
            return Price; 
        }
        /// <summary>
        /// 获取模因信息
        /// </summary>
        /// <returns></returns>
        public MemeInfo GetMemeInfo()
        {
            return ShopMemeInfo;
        }
        /// <summary>
        /// 获取此商品是否已经购买
        /// </summary>
        public bool IsBought()
        {
            return AlreadyBuy;
        }
        /// <summary>
        /// 设置此商品购买状态
        /// </summary>
        /// <param name="isBought">是否被购买</param>
        public void SetBought(bool isBought)
        {
            AlreadyBuy = isBought;
        }
    }
    public class ShopProb
    {
        public static int MemeVer1Prob = 699;
        public static int MemeVer2Prob = 300;
        public static int MemeYEProb = 1;
        public static void ChangeProb(int memeVer1Prob, int memeVer2Prob, int memeYEProb)
        {
            MemeVer1Prob = memeVer1Prob;
            MemeVer2Prob = memeVer2Prob;
            MemeYEProb = memeYEProb;
        }
    }
}
