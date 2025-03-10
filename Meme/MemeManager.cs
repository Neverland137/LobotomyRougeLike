using DG.Tweening;
using NewGameMode.Meme;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;
using static CreatureGenerate.CreatureGenerateData;
using static Steamworks.InventoryItem;
using static WorkerSprite.WorkerSpriteSaveData;

namespace NewGameMode
{
    public class MemeManager
    {
        private static MemeManager _instance;
        public static GameObject memeScene = new GameObject();
        public static Sprite memeOutlook;
        public static float originScrollY = 0;
        public static float originScrollHeight = 0;

        public int _nextInstanceId = 0;
        /// <summary>
        /// 包含模组在内的所有模因,key是模因id
        /// </summary>
        public Dictionary<int, MemeInfo> all_dic = [];
        /// <summary>
        /// 本局肉鸽目前拥有的模因,key是实例id
        /// </summary>
        public Dictionary<int, MemeModel> current_dic = [];

        /// <summary>
        /// 模因的贴图合集，key是贴图名称。用来在加载模因时传递模因贴图
        /// </summary>
        public Dictionary<string, Sprite> sprite_dic = [];
        public List<MemeInfo> all_list = [];
        /// <summary>
        /// 模因生效顺序由此决定
        /// </summary>
        public List<MemeModel> current_list = [];
        public List<MemeInfo> uninhand_list = [];

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
            Dictionary<int, MemeInfo> dictionary = [];
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

                        XmlNode xmlNode5 = xmlNode.SelectSingleNode("special_desc");
                        if (xmlNode5 != null)
                        {
                            memeInfo.localizeData.Add("special_desc", xmlNode5.InnerText.Trim()); //特殊描述在xml文件里的标签名（不是标签内容
                        }
                        XmlNode xmlNode60 = xmlNode.SelectSingleNode("sprite");
                        if (xmlNode60 != null)
                        {
                            memeInfo.sprite_name = xmlNode60.InnerText.Trim();
                            memeInfo.sprite = instance.sprite_dic[memeInfo.sprite_name];
                        }

                        memeInfo.requires = new List<MemeRequire>();
                        if (xmlNode.SelectNodes("require") != null)
                        {
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

        /// <summary>
        /// 读取所有xml
        /// </summary>
        public static void LoadAllInfo()
        {
            try
            {
                LobotomyBaseMod.ModDebug.Log("RougeLike Load 1");
                XmlDocument xmlDocument = new XmlDocument();

                /*
                bool flag = !File.Exists(Harmony_Patch.path + "/Meme/txts/BaseMeme.txt");

                string xml = File.ReadAllText(Harmony_Patch.path + "/Meme/txts/BaseMeme.txt");
                xmlDocument.LoadXml(xml);

                //dictionary = LoadSingleXmlInfo(xmlDocument);
                */
                Dictionary<int, MemeInfo> dictionary = [];
                Dictionary<string, Sprite> dictionary2 = [];

                //以上为加载肉鸽自带的模因

                //需要先加载贴图
                foreach (ModInfo modInfo in ((Add_On)Add_On.instance).ModList)
                {
                    ModInfo modInfo2 = (ModInfo)modInfo;
                    DirectoryInfo directoryInfo = EquipmentDataLoader.CheckNamedDir(modInfo2.modpath, "Meme");//在模组里找叫Meme的文件夹


                    bool flag7 = directoryInfo != null && Directory.Exists(directoryInfo.FullName + "/Sprite");//在Meme文件夹里找Sprite
                    if (flag7)
                    {
                        DirectoryInfo directoryInfo2 = new DirectoryInfo(directoryInfo.FullName + "/Sprite");
                        bool flag4 = directoryInfo2.GetFiles().Length != 0;//看这个文件夹是不是空的
                        if (flag4)
                        {
                            //bool flag4 = modInfo2.modid == string.Empty;
                            if (true)//把modid相关的东西注释掉了
                            {
                                foreach (FileInfo fileInfo in directoryInfo2.GetFiles())//遍历Sprite文件夹中的所有文件
                                {
                                    bool flag5 = fileInfo.Name.Contains(".png");
                                    if (flag5)
                                    {
                                        Texture2D tex = new Texture2D(128, 128);
                                        tex.LoadImage(File.ReadAllBytes(fileInfo.FullName));

                                        //写错了，记得改
                                        Sprite sprite = Sprite.Create(tex, new Rect(0f, 0f, (float)tex.width, (float)tex.height), new Vector2(0.5f, 0.5f));
                                        string name = fileInfo.Name.Split(new char[] { '.' })[0];//按小数点拆分文件名，取第一串字符串
                                        dictionary2.Add(name, sprite);
                                    }
                                }
                            }
                        }
                    }
                }
                instance.sprite_dic = dictionary2;
                foreach (KeyValuePair<string, Sprite> kvp in instance.sprite_dic)
                {
                    Harmony_Patch.YKMTLogInstance.Debug("Load All Meme Sprite Debug Key: " + kvp.Key);
                }

                LobotomyBaseMod.ModDebug.Log("RougeLike Load 2");

                //再加载模因本身
                foreach (ModInfo modInfo in ((Add_On)Add_On.instance).ModList)
                {
                    ModInfo modInfo2 = (ModInfo)modInfo;
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
                                        foreach (KeyValuePair<int, MemeInfo> keyValuePair in LoadSingleXmlInfo(xmlDocument2))
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

                foreach (KeyValuePair<int, MemeInfo> kvp in instance.all_dic)
                {
                    Harmony_Patch.YKMTLogInstance.Debug("Load All Meme Debug Key: " + kvp.Value.script.ToString());
                }

                foreach (KeyValuePair<int, MemeInfo> pair in instance.all_dic)
                {
                    instance.all_list.Add(pair.Value);
                    instance.uninhand_list.Add(pair.Value);
                }
                LobotomyBaseMod.ModDebug.Log("RougeLike Load 4");
            }
            catch (Exception ex)
            {
                Harmony_Patch.YKMTLogInstance.Error(ex);
            }
        }

        public void LoadData(Dictionary<string, object> dic) //不用写存储，存储已经在Harmony_Patch的SaveRougeLikeDayData里了
        {
            GameUtil.TryGetValue<Dictionary<int, MemeModel>>(dic, "meme", ref instance.current_dic);
            if (instance.current_dic.Count == 0)
            {
                return;
            }
            foreach (KeyValuePair<int, MemeModel> pair in MemeManager.instance.current_dic)
            {
                instance.current_list.Add(pair.Value);
            }
        }

        public GameObject InitMemeButton(MemeModel memeModel)
        {
            GameObject memeButton = null;
            int num = 0;
            try
            {
                if (GlobalGameManager.instance.gameMode == Harmony_Patch.rougeLike)
                {
                    Harmony_Patch.LogInfo("InitMemeButtonStart");
                    string name_id;
                    memeModel.metaInfo.localizeData.TryGetValue("name", out name_id);
                    string desc_id;
                    memeModel.metaInfo.localizeData.TryGetValue("desc", out desc_id);
                    string special_desc_id;
                    memeModel.metaInfo.localizeData.TryGetValue("special_desc", out special_desc_id);

                    AssetBundle buttonBundle = AssetBundle.LoadFromFile(Harmony_Patch.path + "/AssetsBundle/single_meme_button"); num++;
                    memeButton = UnityEngine.Object.Instantiate(buttonBundle.LoadAsset<GameObject>("SingleMemeButton")); num++;
                    buttonBundle.Unload(false);//关闭AB包，但是保留已加载的资源
                    memeButton.transform.SetParent(memeScene.transform.Find("MemeButtons").Find("Buttons")); num++;
                    memeButton.transform.localScale = new Vector3(1f, 1f, 1f); num++;

                    //设置按钮本身的文字、音效和移动效果
                    memeButton.transform.Find("Text").gameObject.AddComponent<LocalizeTextLoadScriptWithOutFontLoadScript>().id = name_id;
                    memeButton.transform.Find("Text").gameObject.AddComponent<FontLoadScript>();
                    memeButton.transform.Find("Image").gameObject.GetComponent<Image>().sprite = instance.sprite_dic[memeModel.metaInfo.sprite_name]; ; num++;
                    memeButton.GetComponent<UnityEngine.UI.Image>().sprite = MemeManager.memeOutlook;
                    memeButton.AddComponent<UniversalButtonIntereaction>().pointerEnterSound = true;
                    memeButton.AddComponent<MemeButtonMovement>();


                    //点击后设置详情
                    GameObject detail = memeScene.transform.Find("WonderandDetail").Find("Detail").gameObject; num++;
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
                        detail.transform.Find("Image").gameObject.GetComponent<Image>().sprite = instance.sprite_dic[memeModel.metaInfo.sprite_name];

                    }); num++;

                    //按钮和模因互相对应
                    memeButton.AddComponent<MemeButtonData>().memeModel = memeModel; num++;
                    memeButton.SetActive(true); num++;

                    memeModel.button = memeButton; num++;

                    //更新滑动区域
                    RectTransform rect = memeScene.transform.Find("MemeButtons").Find("Buttons").gameObject.GetComponent<RectTransform>();
                    if (current_list.Count / 4f > 3)
                    {
                        //多出来的行数
                        int lines = (int)(Math.Ceiling(current_list.Count / 4f) - 3);
                        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, originScrollY - 120 * lines);
                        rect.sizeDelta = new Vector2(rect.sizeDelta.x, originScrollHeight + 240 * lines);
                    }
                    else
                    {
                        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, originScrollY);
                        rect.sizeDelta = new Vector2(rect.sizeDelta.x, originScrollHeight);
                    }
                    Harmony_Patch.LogInfo("InitMemeButtonEnd");
                }
            }
            catch (Exception e)
            {
                Harmony_Patch.YKMTLogInstance.Error(e);
            }

            return memeButton;
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

                        if (obj is MemeScriptBase)
                        {
                            memeModel.script = (MemeScriptBase)obj; num++;
                        }
                        instance.current_dic.Add(_nextInstanceId, memeModel); num++;
                        instance.current_list.Add(memeModel);
                        instance.uninhand_list.Remove(memeModel.metaInfo);
                        instance._nextInstanceId++; num++;

                        memeModel.script.SetModel(memeModel);
                        memeModel.script.OnGet(); num++;

                        InitMemeButton(memeModel);
                        //初始化模因对应的模因按钮

                        break;
                    }
                }

                Harmony_Patch.dayResult[3]++;
            }
            
            catch (Exception ex)
            {
                File.WriteAllText(Harmony_Patch.path + "/CreateMemeError.txt", num + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace);
            }
            

            return memeModel;
        }

        public void RemoveMemeModel(int id)
        {
            int num = 0;
            try
            {
                foreach (MemeModel meme in instance.current_list)
                {
                    if (meme.metaInfo.id == id)
                    {
                        instance.current_dic.Remove(meme.instanceId);num++;
                        instance.current_list.Remove(meme); num++;
                        instance.uninhand_list.Add(meme.metaInfo); num++;
                        meme.script.OnRelease(); num++;

                        GameObject.Destroy(meme.button);
                        //更新滑动区域
                        RectTransform rect = memeScene.transform.Find("MemeButtons").Find("Buttons").gameObject.GetComponent<RectTransform>();
                        if (current_list.Count / 4f > 3)
                        {
                            //多出来的行数
                            int lines = (int)(Math.Ceiling(current_list.Count / 4f) - 3);
                            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, originScrollY - 120 * lines);
                            rect.sizeDelta = new Vector2(rect.sizeDelta.x, originScrollHeight + 240 * lines);
                        }
                        else
                        {
                            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, originScrollY);
                            rect.sizeDelta = new Vector2(rect.sizeDelta.x, originScrollHeight);
                        }

                        Harmony_Patch.YKMTLogInstance.Info("RemoveMemeEnd : " + meme.metaInfo.sprite_name);
                        foreach (MemeModel current_meme in instance.current_list)
                        {
                            Harmony_Patch.YKMTLogInstance.Info("CurrentMeme : " + current_meme.metaInfo.sprite_name);
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Harmony_Patch.YKMTLogInstance.Error(ex);
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
        public void OnCancelWeapon(UnitModel actor)
        {
            foreach (MemeModel meme in current_list)
            {
                meme.script.OnCancelWeapon(actor);
            }
        }
        public void OnAttackStart(UnitModel actor, UnitModel target)
        {
            foreach (MemeModel meme in current_list)
            {
                meme.script.OnAttackStart(actor, target);
            }
        }
        public void OnAttackEnd(UnitModel actor, UnitModel target)
        {
            foreach (MemeModel meme in current_list)
            {
                meme.script.OnAttackEnd(actor, target);
            }
        }
        public void OnKillTargetWorker(UnitModel actor, UnitModel target)
        {
            foreach (MemeModel meme in current_list)
            {
                meme.script.OnKillTargetWorker(actor, target);
            }
        }

        public void OnGiveDamage(UnitModel actor, UnitModel target, ref DamageInfo dmg)
        {
            foreach (MemeModel meme in current_list)
            {
                meme.script.OnGiveDamage(actor, target, ref dmg);
            }
        }

        public void OnGiveDamageAfter(UnitModel actor, UnitModel target, DamageInfo dmg)
        {
            foreach (MemeModel meme in current_list)
            {
                meme.script.OnGiveDamageAfter(actor, target, dmg);
            }
        }

        public void WorkerTakeDamage(UnitModel actor, UnitModel victim, DamageInfo dmg)
        {
            foreach (MemeModel meme in current_list)
            {
                meme.script.WorkerTakeDamage(actor, victim, ref dmg);
            }
        }

        public void OnWorkerTakeDamage_After(float value, UnitModel actor, UnitModel victim, RwbpType type)
        {
            foreach (MemeModel meme in current_list)
            {
                meme.script.OnWorkerTakeDamage_After(value, actor, victim, type);
            }
        }

        public void CreatureTakeDamage(UnitModel actor, UnitModel victim, DamageInfo dmg)
        {
            foreach (MemeModel meme in current_list)
            {
                meme.script.CreatureTakeDamage(actor, victim, ref dmg);
            }
        }

        public void OnCreatureTakeDamage_After(float value, UnitModel actor, UnitModel victim, RwbpType type)
        {
            foreach (MemeModel meme in current_list)
            {
                meme.script.OnCreatureTakeDamage_After(value, actor, victim, type);
            }
        }

        public void GetDefense(UnitModel actor)
        {
            foreach (MemeModel meme in current_list)
            {
                meme.script.GetDefense(actor);
            }
        }

        public void GetDamageFactor()
        {
            foreach (MemeModel meme in current_list)
            {
                meme.script.GetDamageFactor();
            }
        }

        public void GetWorkerDamage(WorkerModel actor)
        {
            foreach (MemeModel meme in current_list)
            {
                meme.script.GetWorkerDamage(actor);
            }
        }

        public void OnFixedUpdate()
        {
            foreach (MemeModel meme in current_list)
            {
                meme.script.OnFixedUpdate();
            }
        }

        public void GetWorkProbSpecialBonus(UnitModel actor, SkillTypeInfo skill)
        {
            foreach (MemeModel meme in current_list)
            {
                meme.script.GetWorkProbSpecialBonus(actor, skill);
            }
        }

        public void OnHeal(bool isMental, float amount)
        {
            foreach (MemeModel meme in current_list)
            {
                meme.script.OnHeal(isMental, amount);
            }
        }

        public void OnCreatureSuppressed(CreatureModel creature)
        {
            foreach (MemeModel meme in current_list)
            {
                meme.script.OnCreatureSuppressed(creature);
            }
        }

        public void OnWorkerDie(WorkerModel worker)
        {
            foreach (MemeModel meme in current_list)
            {
                meme.script.OnWorkerDie(worker);
            }
        }

        public int BulletAdder()
        {
            int num = 0;
            foreach (MemeModel meme in current_list)
            {
                num += meme.script.BulletAdder();
            }
            return num;
        }
        public int MaxEnergyAdder()
        {
            int num = 0;
            foreach (MemeModel meme in current_list)
            {
                num += meme.script.MaxEnergyAdder();
            }
            return num;
        }
        public float MaxEnergyTimes()
        {
            float num = 1f;
            foreach (MemeModel meme in current_list)
            {
                num *= meme.script.MaxEnergyTimes();
            }
            return num;
        }
        public int AgentDamageAdder()
        {
            int num = 0;
            foreach (MemeModel meme in current_list)
            {
                num += meme.script.AgentDamageAdder();
            }
            return num;
        }
        public float AgentDamageTimes()
        {
            float num = 1f;
            foreach (MemeModel meme in current_list)
            {
                num *= meme.script.AgentDamageTimes();
            }
            return num;
        }
        public int OverloadAdder()
        {
            int num = 0;
            foreach (MemeModel meme in current_list)
            {
                num += meme.script.OverloadAdder();
            }
            return num;
        }
        public float WorkSuccessAdder()
        {
            float num = 0f;
            foreach (MemeModel meme in current_list)
            {
                num += meme.script.WorkSuccessAdder();
            }
            return num;
        }
        public float CreatureMaxHPTimes()
        {
            float num = 1f;
            foreach (MemeModel meme in current_list)
            {
                num *= meme.script.CreatureMaxHPTimes();
            }
            return num;
        }
        public int CreatureTiredTimeAdder()
        {
            int num = 0;
            foreach (MemeModel meme in current_list)
            {
                num += meme.script.CreatureTiredTimeAdder();
            }
            return num;
        }
        public float FurnaceBoomAdder()
        {
            float num = 0f;
            foreach (MemeModel meme in current_list)
            {
                num += meme.script.FurnaceBoomAdder();
            }
            return num;
        }
        public float UpLevel1RecipeProbAdder()
        {
            float num = 0f;
            foreach (MemeModel meme in current_list)
            {
                num += meme.script.UpLevel1RecipeProbAdder();
            }
            return num;
        }
    }
}
