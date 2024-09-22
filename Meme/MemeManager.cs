using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

namespace NewGameMode
{
    public class MemeManager
    {
        private static MemeManager _instance;

        private int _nextInstanceId = 0;
        /// <summary>
        /// 包含模组在内的所有模因,key是模因id
        /// </summary>
        public Dictionary<int, MemeInfo> all_dic = new Dictionary<int, MemeInfo>();
        /// <summary>
        /// 本局肉鸽目前拥有的模因,key是实例id
        /// </summary>
        public Dictionary<int, MemeModel> current_dic = new Dictionary<int, MemeModel>();
        public List<MemeInfo> all_list = new List<MemeInfo>();
        public List<MemeModel> current_list = new List<MemeModel>();

        public static MemeManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MemeManager();
                }
                return _instance;
            }
        }
        public static MemeInfo GetMemeInfo(int id)
        {
            if (instance.all_dic.ContainsKey(id))
            {
                return instance.all_dic[id];
            }
            return null;
        }
        /// <summary>
        /// 读取单个xml中的所有模因信息
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        public static Dictionary<int, MemeInfo> LoadSingleXmlInfo(XmlDocument document)
        {
            Dictionary<int, MemeInfo> dictionary = new Dictionary<int, MemeInfo>();

            IEnumerator enumerator = document.SelectSingleNode("meme_list").SelectNodes("meme").GetEnumerator();
            try
            {
                try
                {

                    while (enumerator.MoveNext())
                    {
                        object obj = enumerator.Current;
                        XmlNode xmlNode = (XmlNode)obj;
                        MemeInfo memeInfo = new MemeInfo();

                        int num = int.Parse(xmlNode.Attributes.GetNamedItem("id").InnerText);
                        memeInfo.id = num;
                        XmlNode xmlNode2 = xmlNode.SelectSingleNode("name");
                        if (xmlNode2 != null)
                        {
                            memeInfo.localizeData.Add("name", xmlNode2.InnerText.Trim()); //名字在xml文件里的标签名（不是标签内容
                        }
                        else
                        {
                            memeInfo.localizeData.Add("name", num + "name");
                        }


                        XmlNode xmlNode4 = xmlNode.SelectSingleNode("desc");
                        if (xmlNode4 != null)
                        {
                            memeInfo.localizeData.Add("desc", xmlNode4.InnerText.Trim()); //描述在xml文件里的标签名（不是标签内容
                        }


                        memeInfo.requires = new List<MemeRequire>();
                        IEnumerator enumerator2 = xmlNode.SelectNodes("require").GetEnumerator();
                        try
                        {
                            while (enumerator2.MoveNext())
                            {
                                object obj2 = enumerator2.Current;
                                XmlNode xmlNode7 = (XmlNode)obj2;
                                string innerText2 = xmlNode7.Attributes.GetNamedItem("type").InnerText; //需求的类型

                                int value = 0;
                                if (int.TryParse(xmlNode7.InnerText.Trim(), out value))
                                {
                                    value = int.Parse(xmlNode7.InnerText.Trim());
                                }


                                MemeRequire memeRequire = new MemeRequire();
                                if (innerText2 == "day")
                                {
                                    memeRequire.type = MemeRequireType.DAY;
                                    memeRequire.value = value;
                                }
                                if (innerText2 == "equip")
                                {
                                    memeRequire.type = MemeRequireType.EQUIP;
                                    memeRequire.value = value;
                                }
                                if (innerText2 == "abnormality")
                                {
                                    memeRequire.type = MemeRequireType.ABNORMALITY;
                                    memeRequire.value = value;
                                }
                                if (innerText2 == "meme")
                                {
                                    memeRequire.type = MemeRequireType.MEME;
                                    memeRequire.value = value;
                                }
                                if (innerText2 == "satisfyall")
                                {
                                    if (xmlNode7.InnerText.Trim() == "true")
                                    {
                                        memeInfo.satisfy_all = true;
                                    }
                                }

                                memeInfo.requires.Add(memeRequire);
                            }
                        }
                        finally
                        {
                            IDisposable disposable;
                            if ((disposable = (enumerator2 as IDisposable)) != null)
                            {
                                disposable.Dispose(); //需要在读取完require这个enumrator后释放它
                            }
                        }

                        XmlNode xmlNode10 = xmlNode.SelectSingleNode("duplicate");
                        if (xmlNode10 != null)
                        {
                            if (xmlNode10.InnerText.Trim() == "true")
                            {
                                memeInfo.duplicate = true;
                            }
                        }
                        XmlNode xmlNode11 = xmlNode.SelectSingleNode("curse");
                        if (xmlNode11 != null)
                        {
                            if (xmlNode11.InnerText.Trim() == "true")
                            {
                                memeInfo.curse = true;
                            }
                        }
                        XmlNode xmlNode12 = xmlNode.SelectSingleNode("boss");
                        if (xmlNode12 != null)
                        {
                            if (xmlNode12.InnerText.Trim() == "true")
                            {
                                memeInfo.boss = true;
                            }
                        }
                        XmlNode xmlNode13 = xmlNode.SelectSingleNode("suit");
                        if (xmlNode13 != null)
                        {
                            if (xmlNode13.InnerText.Trim() != null)
                            {
                                memeInfo.suit = int.Parse(xmlNode13.InnerText.Trim());
                            }
                        }

                        XmlNode xmlNode6 = xmlNode.SelectSingleNode("script");
                        if (xmlNode6 != null)
                        {
                            memeInfo.script = xmlNode6.InnerText;
                        }

                        XmlNode xmlNode9 = xmlNode.SelectSingleNode("grade");
                        if (xmlNode9 != null)
                        {
                            if (xmlNode9.InnerText.Trim() != null)
                            {
                                memeInfo.grade = int.Parse(xmlNode9.InnerText.Trim());
                            }
                        }

                        XmlNode xmlNodePrice = xmlNode.SelectSingleNode("price");
                        if (xmlNodePrice != null)
                        {
                            if (xmlNodePrice.InnerText.Trim() != null)
                            {
                                memeInfo.price = int.Parse(xmlNodePrice.InnerText.Trim());
                            }
                        }

                        dictionary.Add(memeInfo.id, memeInfo);
                    }
                }
                finally
                {
                    IDisposable disposable2;
                    if ((disposable2 = (enumerator as IDisposable)) != null)
                    {
                        disposable2.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                File.WriteAllText(Harmony_Patch.path + "/LoadSingleXmlInfoError.txt", Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace);
            }
            return dictionary;
        }

        public static void LoadAllInfo()
        {
            try
            {
                LobotomyBaseMod.ModDebug.Log("RougeLike Load 1");
                XmlDocument xmlDocument = new XmlDocument();
                bool flag = !File.Exists(Harmony_Patch.path + "/Meme/txts/BaseMeme.txt");

                string xml = File.ReadAllText(Harmony_Patch.path + "/Meme/txts/BaseMeme.txt");
                xmlDocument.LoadXml(xml);
                Dictionary<int, MemeInfo> dictionary = LoadSingleXmlInfo(xmlDocument);
                //Dictionary<string, Dictionary<int, MemeInfo>> dictionary2 = new Dictionary<string, Dictionary<int, MemeInfo>>();//还在嵌套
                LobotomyBaseMod.ModDebug.Log("RougeLike Load 2");
                foreach (global::ModInfo modInfo in ((global::Add_On)global::Add_On.instance).ModList)
                {
                    ModInfo modInfo2 = (global::ModInfo)modInfo;
                    DirectoryInfo directoryInfo = EquipmentDataLoader.CheckNamedDir(modInfo2.modpath, "Meme");//在模组里找叫Meme的文件夹
                    bool flag2 = directoryInfo != null && Directory.Exists(directoryInfo.FullName + "/txts");//在Meme文件夹里找txt
                    if (flag2)
                    {
                        DirectoryInfo directoryInfo2 = new DirectoryInfo(directoryInfo.FullName + "/txts");
                        bool flag3 = directoryInfo2.GetFiles().Length != 0;//看这个txt是不是空的
                        if (flag3)
                        {
                            //bool flag4 = modInfo2.modid == string.Empty;
                            if (true)//把modid相关的东西注释掉了
                            {
                                foreach (FileInfo fileInfo in directoryInfo2.GetFiles())
                                {
                                    bool flag5 = fileInfo.Name.Contains(".txt") || fileInfo.Name.Contains(".xml");
                                    if (flag5)
                                    {
                                        XmlDocument xmlDocument2 = new XmlDocument();
                                        xmlDocument2.LoadXml(File.ReadAllText(fileInfo.FullName));//把txt加载成xml
                                        foreach (KeyValuePair<int, MemeInfo> keyValuePair in LoadSingleXmlInfo(xmlDocument))
                                        {
                                            //读取xml里的所有模因信息
                                            bool flag6 = dictionary.ContainsKey(keyValuePair.Key);
                                            if (flag6)
                                            {
                                                //如果id重复了，就移除旧的，添加新的
                                                dictionary.Remove(keyValuePair.Key);
                                            }
                                            dictionary.Add(keyValuePair.Key, keyValuePair.Value);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                LobotomyBaseMod.ModDebug.Log("RougeLike Load 3");
                instance.all_dic = dictionary;
                foreach (KeyValuePair<int, MemeInfo> pair in instance.all_dic)
                {
                    instance.all_list.Add(pair.Value);
                }
                LobotomyBaseMod.ModDebug.Log("RougeLike Load 4");
            }
            catch (Exception ex)
            {
                LobotomyBaseMod.ModDebug.Log("RougeLike Load Error - " + ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        public void LoadData(Dictionary<string, object> dic) //不用写存储，存储已经在Harmony_Patch的SaveRougeLikeDayData里了
        {
            GameUtil.TryGetValue<Dictionary<int, MemeModel>>(dic, "meme", ref instance.current_dic);
            if (instance.current_dic.Count == 0)
            {
                return;
            }
            foreach (KeyValuePair<int, MemeModel> pair in instance.current_dic)
            {
                instance.current_list.Add(pair.Value);
            }
        }
        public MemeModel CreateMemeModel(int id)
        {
            MemeModel memeModel = new MemeModel();

            int num = 0;
            try
            {
                foreach (KeyValuePair<int, MemeInfo> pair in instance.all_dic)
                {
                    if (pair.Value.id == id)
                    {
                        memeModel.metaInfo = pair.Value; num++;

                        memeModel.instanceId = instance._nextInstanceId; num++;

                        //Type type = Type.GetType(pair.Value.script); num++;
                        object obj = null; num++;

                        foreach (Assembly assembly in Add_On.instance.AssemList)//获取script字符串所指定的类型
                        {
                            foreach (Type type2 in assembly.GetTypes())
                            {
                                bool flag5 = type2.Name == pair.Value.script;
                                if (flag5)
                                {
                                    obj = Activator.CreateInstance(type2);
                                }
                            }
                        }

                        //if (!Type.Equals(type, null))
                        {
                            //obj = Activator.CreateInstance(type); num++; File.AppendAllText(Harmony_Patch.path + "/CreateMeme0.txt", "HasType"+ Environment.NewLine);
                        }
                        if (obj is MemeScriptBase)
                        {
                            memeModel.script = (MemeScriptBase)obj; num++; File.AppendAllText(Harmony_Patch.path + "/CreateMeme0.txt", "IsMeme" + Environment.NewLine);
                        }

                        instance.current_dic.Add(_nextInstanceId, memeModel); num++;
                        instance.current_list.Add(memeModel);
                        instance._nextInstanceId++; num++;

                        memeModel.script.OnGet(); num++;
                        break;

                    }
                }
            }
            catch (Exception ex)
            {
                File.WriteAllText(Harmony_Patch.path + "/CreateMeme.txt", num.ToString());
                File.WriteAllText(Harmony_Patch.path + "/CreateMemeError.txt", num + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace);
            }

            return memeModel;
        }

        public void RemoveMemeModel(int id)
        {
            MemeModel memeModel = new MemeModel();

            foreach (MemeModel meme in instance.current_list)
            {
                if (meme.metaInfo.id == id)
                {
                    current_dic.Remove(meme.instanceId);
                    current_list.Remove(memeModel);

                    meme.script.OnRelease();
                    break;

                }
            }
        }

        public void OnGet() //会使所有模因都触发刚入手时的效果！慎用！
        {
            foreach (MemeModel meme in current_list)
            {
                meme.script.OnGet();
            }
        }
        public void OnRelease() //会使所有模因都触发消失时的效果！慎用！
        {
            foreach (MemeModel meme in current_list)
            {
                meme.script.OnRelease();
            }
        }
        public void OnStageStart()
        {
            foreach (MemeModel meme in current_list)
            {
                meme.script.OnStageStart();
            }
        }
        public void OnStageRelease()
        {
            foreach (MemeModel meme in current_list)
            {
                meme.script.OnStageRelease();
            }
        }

        public void OnPrepareWeapon(UnitModel actor)
        {
            foreach (MemeModel meme in current_list)
            {
                meme.script.OnPrepareWeapon(actor);
            }
        }
    }
}
