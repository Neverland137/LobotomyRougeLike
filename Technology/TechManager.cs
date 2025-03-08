using DG.Tweening;
using Harmony;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using UnityEngine;

namespace NewGameMode
{
    public class TechManager
    {
        private static TechManager _instance;

        public Dictionary<string, Sprite> TechSpriteDic;
        public Dictionary<int, TechInfo> TechDic;
        public Dictionary<int, TechModel> HadTechDic;
        public List<TechModel> HadTechList;
        public List<TechInfo> TechList;
        private int _nextInstanceId;
        public static TechManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TechManager();
                }
                return _instance;
            }
        }
        public static Dictionary<int, TechInfo> LoadSingleXmlInfo(XmlDocument document)
        {
            Dictionary<int, TechInfo> dictionary = [];
            IEnumerator enumerator = document.SelectSingleNode("meme_list").SelectNodes("meme").GetEnumerator();
            try
            {
                try
                {
                    while (enumerator.MoveNext())
                    {
                        object obj = enumerator.Current;
                        XmlNode xmlNode = (XmlNode)obj;
                        TechInfo techInfo = new();

                        int id = int.Parse(xmlNode.Attributes.GetNamedItem("id").InnerText);
                        techInfo.id = id;
                        XmlNode NameNode = xmlNode.SelectSingleNode("name");
                        if (NameNode != null)
                        {
                            techInfo.localizeData.Add("name", NameNode.InnerText.Trim()); //名字在xml文件里的标签名（不是标签内容
                        }
                        else
                        {
                            techInfo.localizeData.Add("name", id + "name");
                        }

                        XmlNode DescNode = xmlNode.SelectSingleNode("desc");
                        if (DescNode != null)
                        {
                            techInfo.localizeData.Add("desc", DescNode.InnerText.Trim()); //描述在xml文件里的标签名（不是标签内容
                        }

                        XmlNode EffectDescNode = xmlNode.SelectSingleNode("effect_desc");
                        if (EffectDescNode != null)
                        {
                            techInfo.localizeData.Add("effect_desc", EffectDescNode.InnerText.Trim()); //特殊描述在xml文件里的标签名（不是标签内容
                        }
                        XmlNode SpriteNode = xmlNode.SelectSingleNode("sprite");
                        if (SpriteNode != null)
                        {
                            techInfo.sprite_name = SpriteNode.InnerText.Trim();
                            techInfo.sprite = instance.TechSpriteDic[techInfo.sprite_name];
                        }

                        techInfo.DependTechs = [];
                        IEnumerator enumeratorDepend = xmlNode.SelectNodes("depend_tech").GetEnumerator();
                        try
                        {
                            while (enumeratorDepend.MoveNext())
                            {
                                var currentDependTech = (XmlNode)enumeratorDepend.Current;
                                int value = 0;
                                if (int.TryParse(currentDependTech.InnerText.Trim(), out value))
                                {
                                    value = int.Parse(currentDependTech.InnerText.Trim());
                                    techInfo.DependTechs.Add(value);
                                }
                            }
                        }
                        finally
                        {
                            IDisposable disposable;
                            if ((disposable = (enumeratorDepend as IDisposable)) != null)
                            {
                                disposable.Dispose();
                            }
                        }

                        XmlNode ScriptNode = xmlNode.SelectSingleNode("script");
                        if (ScriptNode != null)
                        {
                            techInfo.script = ScriptNode.InnerText;
                        }

                        dictionary.Add(techInfo.id, techInfo);
                        instance.TechList.Add(techInfo);
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
                Harmony_Patch.LogErrorEx(ex);
            }
            return dictionary;
        }
        public static void LoadAllInfo()
        {
            try
            {
                LobotomyBaseMod.ModDebug.Log("RougeLike TECH Load 1");
                XmlDocument xmlDocument = new XmlDocument();
                Dictionary<int, TechInfo> TechInfoDic = [];
                Dictionary<string, Sprite> TechSprite = [];

                //需要先加载贴图
                foreach (ModInfo modInfo in (Add_On.instance).ModList)
                {
                    DirectoryInfo TechDir = EquipmentDataLoader.CheckNamedDir(modInfo.modpath, "Tech");

                    bool hasSpriteDir = TechDir != null && Directory.Exists(TechDir.FullName + "/Sprite");
                    if (hasSpriteDir)
                    {
                        DirectoryInfo TechSpriteDir = new(TechDir.FullName + "/Sprite");
                        bool isDirEmpty = TechSpriteDir.GetFiles().Length == 0;
                        if (!isDirEmpty)
                        {
                            foreach (FileInfo fileInfo in TechSpriteDir.GetFiles())
                            {
                                bool flag5 = fileInfo.Name.Contains(".png");
                                if (flag5)
                                {
                                    Texture2D tex = new(128, 128);
                                    tex.LoadImage(File.ReadAllBytes(fileInfo.FullName));

                                    //写错了，记得改
                                    Sprite sprite = Sprite.Create(tex, new Rect(0f, 0f, (float)tex.width, (float)tex.height), new Vector2(0.5f, 0.5f));
                                    //按小数点拆分文件名，取第一串字符串
                                    string name = fileInfo.Name.Split(new char[] { '.' })[0];
                                    TechSprite.Add(name, sprite);
                                }
                            }
                        }
                    }
                }
                instance.TechSpriteDic = TechSprite;
                foreach (KeyValuePair<string, Sprite> kvp in instance.TechSpriteDic)
                {
                    Harmony_Patch.logger.Debug("Load All Tech Debug Key: " + kvp.Key);
                }

                LobotomyBaseMod.ModDebug.Log("RougeLike TECH Load 2");

                foreach (ModInfo modInfo in (Add_On.instance).ModList)
                {
                    DirectoryInfo TechDir = EquipmentDataLoader.CheckNamedDir(modInfo.modpath, "Tech");

                    bool hasTxtsDir = TechDir != null && Directory.Exists(TechDir.FullName + "/txts");
                    if (hasTxtsDir)
                    {
                        DirectoryInfo txtsDir = new DirectoryInfo(TechDir.FullName + "/txts");
                        bool isTxtsDirEmpty = txtsDir.GetFiles().Length != 0;
                        if (isTxtsDirEmpty)
                        {
                            foreach (FileInfo fileInfo in txtsDir.GetFiles())
                            {
                                bool isVaildFile = fileInfo.Name.Contains(".txt") || fileInfo.Name.Contains(".xml");
                                if (isVaildFile)
                                {
                                    XmlDocument xmlDoc = new();
                                    xmlDoc.Load(fileInfo.FullName);
                                    foreach (KeyValuePair<int, TechInfo> keyValuePair in LoadSingleXmlInfo(xmlDoc))
                                    {
                                        bool isKeyExist = TechInfoDic.ContainsKey(keyValuePair.Key);
                                        if (isKeyExist)
                                        {
                                            //如果id重复了，就移除旧的，添加新的
                                            TechInfoDic.Remove(keyValuePair.Key);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                foreach (KeyValuePair<int, TechInfo> pair in instance.TechDic)
                {
                    instance.TechList.Add(pair.Value);
                }
                instance.TechDic = TechInfoDic;
            }
            catch (Exception ex)
            {
                Harmony_Patch.logger.Error(ex);
            }
        }
        public TechModel CreateTechModel(int id)
        {
            TechModel techModel = new();
            try
            {
                foreach (KeyValuePair<int, TechInfo> pair in instance.TechDic)
                {
                    if (pair.Value.id == id)
                    {
                        techModel.metaInfo = pair.Value;

                        techModel.instanceId = instance._nextInstanceId;
                        object obj = null;

                        foreach (Assembly assembly in Add_On.instance.AssemList)
                        {
                            foreach (Type type2 in assembly.GetTypes())
                            {
                                bool isTrueScript = type2.Name == pair.Value.script;
                                if (isTrueScript)
                                {
                                    obj = Activator.CreateInstance(type2);
                                }
                            }
                        }

                        if (obj is TechScriptBase)
                        {
                            techModel.script = (TechScriptBase)obj;
                        }
                        instance.HadTechDic.Add(_nextInstanceId, techModel);
                        instance.HadTechList.Add(techModel);
                        instance._nextInstanceId++;
                    }
                }
            }
            catch (Exception ex)
            {
                Harmony_Patch.LogErrorEx(ex);
            }

            return techModel;
        }
        public void RemoveTechModel(int id)
        {
            foreach (TechModel meme in instance.HadTechList)
            {
                if (meme.metaInfo.id == id)
                {
                    instance.HadTechDic.Remove(meme.instanceId);
                    instance.HadTechList.Remove(meme);
                    break;
                }
            }
        }
    }
}
