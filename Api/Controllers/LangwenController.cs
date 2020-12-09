using Join;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Http;
using System.Web.Script.Serialization;
using System.Linq;
using static TransactionAppletaApi.LangWenImgUrlData;
using System.IO;

namespace TransactionAppletaApi
{
    [RoutePrefix("api/_langwen")]
    public class LangwenController : BaseController
    {
        #region 金色琅纹选择
        /// <summary>
        /// 金色琅纹选择
        /// api/_langwen/goldSelect
        /// </summary>
        [HttpPost]
        [Route("goldSelect")]
        public IHttpActionResult GoldSelect([FromBody]JToken json)
        {
            return this.TryReturn<object>(() =>
            {
                try
                {
                    using (var x = Join.Dal.MySqlProvider.X())
                    {
                        var jtoken = json.AsDynamic();
                        //琅纹名称
                        string name = jtoken.name;
                        //琅纹等级
                        string level = jtoken.level;
                        //琅纹数量
                        decimal num = jtoken.num;
                        //钥匙点卷
                        decimal keyPrice = jtoken.price;
                        if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(level))
                        {
                            var goldLangwen = LangWenData.X.goldLangwenModels;
                            //查询琅纹
                            var langwenData = goldLangwen.Select().Where(p => p["name"].ToString() == name).SingleOrDefault();
                            if (langwenData != null)
                            {
                                //基础属性
                                //单个琅纹升级所需数量
                                var upgradeNum = int.Parse(langwenData[level].ToString());
                                //单个宝箱开出数量
                                var treasureNum = int.Parse(langwenData["OPEN_NUMBER"].ToString());
                                //绑点价格
                                var bindPrice = decimal.Parse(langwenData["PRICE"].ToString());

                                //计算属性
                                //总琅纹升级所需数量
                                var totalNum = upgradeNum * num;
                                //总需要宝箱数量 = 总钥匙数量
                                decimal totalTreasure = 0;
                                if (totalNum % treasureNum == 0)
                                    totalTreasure = totalNum / treasureNum;
                                else
                                    totalTreasure = Math.Floor(totalNum / treasureNum) + 1;
                                //总宝箱价格
                                var totalPrice = totalTreasure * bindPrice;
                                //总钥匙价格
                                var totalKeyPrice = totalTreasure * keyPrice;
                                //属性
                                var attrStr = langwenData["attr"].ToString();
                                //计算属性功力
                                var backModel = GoldLangwenSum(attrStr, level);
                                var returnTable = new { attrStr = backModel.str, skill = Math.Floor(backModel.skill), keyNumber = totalTreasure, totalNum = totalNum, totalKeyPrice = totalKeyPrice + totalPrice };
                                return new { Table = returnTable, IS_SUCCESS = true, MSG = "" };
                            }
                        }
                        return new { Table = "", IS_SUCCESS = false, MSG = "未查询到琅纹数据" };
                    }
                }
                catch (Exception ex)
                {
                    return new { Table = "", IS_SUCCESS = false, MSG = ex.Message };
                }
            });
        }


        public GoldModel GoldLangwenSum(string attrStr, string level)
        {
            var model = new GoldModel();
            var goldLangwenAttrData = LangWenData.X.goldLangwenAttrModels;
            var attrList = attrStr.Split(',');
            //属性
            var str = "";
            //功力
            var skill = 0.00m;
            foreach (var item in attrList)
            {
                //根据属性+等级查询对应数值
                var attrData = goldLangwenAttrData.Select().Where(p => p["name"].ToString() == item).SingleOrDefault();
                if (attrData != null)
                {
                    var levelValue = decimal.Parse(attrData[level].ToString());
                    var strLevelValue = "";
                    //如果是会心增伤/会心减伤，暗伤增伤/暗伤减伤，×100拼接百分号
                    if (item == "会心增伤" || item == "会心减伤" || item == "暗伤增伤" || item == "暗伤减伤")
                    {
                        strLevelValue = (levelValue * 100).ToString().TrimEnd('0') + "%";
                        skill += CalculationSkill(item, levelValue) * 10;
                    }
                    //否则取整
                    else
                    {
                        strLevelValue = Math.Floor(levelValue).ToString();
                        skill += CalculationSkill(item, levelValue);

                    }
                    //计算功力
                    //属性
                    str += item + "+" + strLevelValue + ",";
                }
            }
            str = str.Substring(0, str.Length - 1);
            model.str = str;
            model.skill = skill;
            return model;
        }

        public class GoldModel
        {
            public string str { get; set; }
            public decimal skill { get; set; }
        }

        /// <summary>
        /// 计算功力
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public decimal CalculationSkill(string name, decimal value)
        {
            var backResult = 0.00m;
            switch (name)
            {
                case "力道":
                    backResult = value * 1.6m;
                    break;
                case "气劲":
                    backResult = value * 1.6m;
                    break;
                case "根骨":
                    backResult = value * 1.6m;
                    break;
                case "身法":
                    backResult = value * 1.6m;
                    break;
                case "攻击":
                    backResult = value * 2m;
                    break;
                case "气血":
                    backResult = value * 0.08m;
                    break;
                case "外功防御":
                    backResult = value * 0.64m;
                    break;
                case "内功防御":
                    backResult = value * 0.64m;
                    break;
                case "会心":
                    backResult = value * 0.8m;
                    break;
                case "会心抗性":
                    backResult = value * 0.8m;
                    break;
                case "暗伤":
                    backResult = value * 0.8m;
                    break;
                case "暗伤抗性":
                    backResult = value * 0.8m;
                    break;
                case "破招":
                    backResult = value * 1.6m;
                    break;
                case "拆招":
                    backResult = value * 1.6m;
                    break;
                case "会心增伤":
                    backResult = value * 2.24m * 100;
                    break;
                case "暗伤增伤":
                    backResult = value * 2.24m * 100;
                    break;
                case "会心减伤":
                    backResult = value * 1.28m * 100;
                    break;
                case "暗伤减伤":
                    backResult = value * 1.28m * 100;
                    break;
            }
            return backResult;
        }
        #endregion

        #region 紫色琅纹选择
        /// <summary>
        /// 紫色琅纹选择
        /// api/_langwen/purpleSelect
        /// </summary>
        [HttpPost]
        [Route("purpleSelect")]
        public IHttpActionResult PurpleSelect([FromBody]JToken json)
        {
            return this.TryReturn<object>(() =>
            {
                try
                {
                    using (var x = Join.Dal.MySqlProvider.X())
                    {
                        var jtoken = json.AsDynamic();
                        //琅纹名称
                        string name = jtoken.name;
                        //琅纹等级
                        string level = jtoken.level;
                        //琅纹数量
                        decimal num = jtoken.num;
                        if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(level))
                        {
                            var purpleLangwen = LangWenData.X.purpleLangwenModels;
                            //查询琅纹
                            var langwenData = purpleLangwen.Select().Where(p => p["name"].ToString() == name && p["LEVEL"].ToString() == level).SingleOrDefault();
                            if (langwenData != null)
                            {
                                var backModel = purpleSum(langwenData);
                                var returnTable = new { attrStr = backModel.str, skill = Math.Floor(backModel.skill), langwenNum = backModel.upNum * num, totalFragementsNum = backModel.fragementsNum * num };
                                return new { Table = returnTable, IS_SUCCESS = true, MSG = "" };
                            }
                        }
                        return new { Table = "", IS_SUCCESS = false, MSG = "未查询到琅纹数据" };
                    }
                }
                catch (Exception ex)
                {
                    return new { Table = "", IS_SUCCESS = false, MSG = ex.Message };
                }
            });
        }

        //紫色琅纹计算
        public PurpleSumModel purpleSum(DataRow langwenData)
        {
            var backModel = new PurpleSumModel();
            //属性
            var attrStr = langwenData["attr"].ToString();
            var attrList = attrStr.Split(',');
            //琅纹属性
            var str = "";
            foreach (var item in attrList)
            {
                //属性
                var value = langwenData[item].ToString();
                string attrName = "";
                switch (item)
                {
                    case "LD":
                        attrName = "力道";
                        break;
                    case "QJ":
                        attrName = "气劲";
                        break;
                    case "SF":
                        attrName = "身法";
                        break;
                    case "GG":
                        attrName = "根骨";
                        break;
                    case "HX":
                        attrName = "会心";
                        break;
                    case "GJ":
                        attrName = "攻击";
                        break;
                    case "QX":
                        attrName = "气血";
                        break;
                    case "HXKX":
                        attrName = "会心抗性";
                        break;
                    case "ASKX":
                        attrName = "暗伤抗性";
                        break;
                    case "AS":
                        attrName = "暗伤";
                        break;
                    default:
                        break;
                }
                str += attrName + "+" + value + ",";
            }
            str = str.Substring(0, str.Length - 1);
            backModel.str = str;
            backModel.skill = decimal.Parse(langwenData["SKILL"].ToString());
            backModel.upNum = int.Parse(langwenData["UP_NUM"].ToString());
            backModel.fragementsNum = int.Parse(langwenData["FRAGMENTS_NUM"].ToString());
            return backModel;
        }

        public class PurpleSumModel
        {
            public string str { get; set; }//属性
            public decimal skill { get; set; }//功力
            public int upNum { get; set; }//等级琅纹数量
            public int fragementsNum { get; set; }//琅纹碎片数量
        }
        #endregion

        #region 琅纹推荐
        /// <summary>
        /// 琅纹推荐
        /// api/_langwen/langwenRecommend
        /// </summary>
        [HttpPost]
        [Route("langwenRecommend")]
        public IHttpActionResult LangwenRecommend([FromBody]JToken json)
        {
            return this.TryReturn<object>(() =>
            {
                try
                {
                    using (var x = Join.Dal.MySqlProvider.X())
                    {
                        var jtoken = json.AsDynamic();
                        //职业
                        string zhiye = jtoken.zhiye;
                        //氪金程度
                        string kejin = jtoken.kejin;
                        //阶段
                        //string jieduan = jtoken.jieduan;
                        //琅纹图片名称对照表
                        var langwenImgUrlNameList = LangWenImgUrlData.X.langwenImgUrlNameModel;
                        var selectSql = string.Format("select * from b_langwen_recommend where OCCUPATION like '%{0}%' and level='{1}'", zhiye, kejin);
                        var table = x.ExecuteSqlCommand(selectSql);
                        var tuijianTable = table.Tables[0];
                        if (tuijianTable.Rows.Count > 0)
                        {
                            var result = new List<MainModel>();

                            var row = tuijianTable.Rows[0];
                            //主副武器
                            var arm = row["ARMS"].ToString();
                            var armLangwenNameList = arm.Split('+').ToList();
                            var armNameImgList = nameGetImgUrl(armLangwenNameList, langwenImgUrlNameList);
                            var armNameImg1List = nameGetImgUrl(armLangwenNameList, langwenImgUrlNameList);
                            result.Add(new MainModel()
                            {
                                name = "主武器",
                                details = armNameImgList
                            });
                            result.Add(new MainModel()
                            {
                                name = "副武器",
                                details = armNameImg1List
                            });
                            //项链/戒指
                            var necklace = row["NECKLACE"].ToString();
                            var necklaceLangwenNameList = necklace.Split('+').ToList();
                            var necklaceNameImgList = nameGetImgUrl(necklaceLangwenNameList, langwenImgUrlNameList);
                            var necklaceName1ImgList = nameGetImgUrl(necklaceLangwenNameList, langwenImgUrlNameList);
                            result.Add(new MainModel()
                            {
                                name = "项链",
                                details = necklaceNameImgList
                            });
                            result.Add(new MainModel()
                            {
                                name = "戒指",
                                details = necklaceName1ImgList
                            });
                            //护腕
                            var bracer = row["BRACER"].ToString();
                            var bracerLangwenNameList = bracer.Split('+').ToList();
                            var bracerNameImgList = nameGetImgUrl(bracerLangwenNameList, langwenImgUrlNameList);
                            result.Add(new MainModel()
                            {
                                name = "护腕",
                                details = bracerNameImgList
                            });
                            //外套/鞋子
                            var coat = row["COAT"].ToString();
                            var coatLangwenNameList = coat.Split('+').ToList();
                            var coatNameImgList = nameGetImgUrl(coatLangwenNameList, langwenImgUrlNameList);
                            var coatName1ImgList = nameGetImgUrl(coatLangwenNameList, langwenImgUrlNameList);
                            result.Add(new MainModel()
                            {
                                name = "外套",
                                details = coatNameImgList
                            });
                            result.Add(new MainModel()
                            {
                                name = "鞋子",
                                details = coatName1ImgList
                            });
                            //手镯/玉佩
                            var bracelet = row["BRACELET"].ToString();
                            var braceletLangwenNameList = bracelet.Split('+').ToList();
                            var braceletNameImgList = nameGetImgUrl(braceletLangwenNameList, langwenImgUrlNameList);
                            var braceletName1ImgList = nameGetImgUrl(braceletLangwenNameList, langwenImgUrlNameList);
                            result.Add(new MainModel()
                            {
                                name = "手镯",
                                details = braceletNameImgList
                            });
                            result.Add(new MainModel()
                            {
                                name = "玉佩",
                                details = braceletName1ImgList
                            });
                            //帽子
                            var hat = row["HAT"].ToString();
                            var hatLangwenNameList = hat.Split('+').ToList();
                            var hatNameImgList = nameGetImgUrl(hatLangwenNameList, langwenImgUrlNameList);
                            result.Add(new MainModel()
                            {
                                name = "帽子",
                                details = hatNameImgList
                            });
                            //腰带
                            var belt = row["BELT"].ToString();
                            var beltLangwenNameList = belt.Split('+').ToList();
                            var beltNameImgList = nameGetImgUrl(beltLangwenNameList, langwenImgUrlNameList);
                            var beltName1ImgList = nameGetImgUrl(beltLangwenNameList, langwenImgUrlNameList);
                            result.Add(new MainModel()
                            {
                                name = "腰带",
                                details = beltNameImgList
                            });
                            // 内衬
                            var neichen = row["NEICHENG"].ToString();
                            var neichenLangwenNameList = neichen.Split('+').ToList();
                            var neichenNameImgList = nameGetImgUrl(neichenLangwenNameList, langwenImgUrlNameList);
                            var neichenName1ImgList = nameGetImgUrl(neichenLangwenNameList, langwenImgUrlNameList);
                            result.Add(new MainModel()
                            {
                                name = "内衬",
                                details = neichenName1ImgList
                            });

                            var remark = row["REMARK"].ToString();


                            //构造返回值
                            var returnTable = new
                            {
                                list = result,
                                remark = remark
                            };
                            return new { Table = returnTable, IS_SUCCESS = true, MSG = "" };
                        }
                        return new { Table = "", IS_SUCCESS = false, MSG = "未查询到琅纹数据" };
                    }
                }
                catch (Exception ex)
                {
                    return new { Table = "", IS_SUCCESS = false, MSG = ex.Message };
                }
            });
        }

        /// <summary>
        /// 根据名称数组获取图片数组
        /// </summary>
        public List<DetailModel> nameGetImgUrl(List<string> nameList, List<LangwenImgUrlNameModel> list)
        {
            List<DetailModel> backResult = new List<DetailModel>();
            foreach (var item in nameList)
            {
                var langwenData = list.Where(p => p.NAME == item).FirstOrDefault();
                if (langwenData != null)
                    backResult.Add(new DetailModel() { name = item, url = langwenData.URL });
            }
            return backResult;
        }

        public class MainModel
        {
            public string name { get; set; }
            public List<DetailModel> details { get; set; }
        }

        public class DetailModel
        {
            public string name { get; set; }
            public string url { get; set; }
        }
        #endregion

        #region 琅纹模拟器
        /// <summary>
        /// 琅纹模拟器
        /// api/_langwen/langwenResonance
        /// </summary>
        [HttpPost]
        [Route("langwenResonance")]
        public IHttpActionResult LangwenResonance([FromBody]JToken json)
        {
            return this.TryReturn<object>(() =>
            {
                var jtoken = json.AsDynamic();
                string zhuwuqi = jtoken.zhuwuqi;  //主武器
                string fuwuqi = jtoken.fuwuqi;  //副武器
                string xianglian = jtoken.xianglian;  //项链
                string jiezhi = jtoken.jiezhi;  //戒指
                string huwan = jtoken.huwan;  //护腕

                string waitao = jtoken.waitao;  //外套
                string xiezi = jtoken.xiezi;  //鞋子
                string shouzhuo = jtoken.shouzhuo;  //手镯
                string yupei = jtoken.yupei;  //玉佩
                string maozi = jtoken.maozi;  //帽子

                string yaodai = jtoken.yaodai;  //腰带
                string neicheng = jtoken.neicheng;  //内衬

                decimal keyPrice = jtoken.keyPrice;  //钥匙金额
                string userId = jtoken.userId;  //钥匙金额

                //扣减次数
                //执行sql
                using (var x = Join.Dal.MySqlProvider.X())
                {
                    //查看剩余可用次数
                    var selCountSql = "SELECT * FROM A_USER WHERE KID='" + userId + "'";
                    var selCountResult = x.ExecuteSqlCommand(selCountSql).Tables[0];
                    var isConinue = true;
                    if (selCountResult.Rows.Count > 0)
                    {
                        var row = selCountResult.Rows[0];
                        var sumNum = decimal.Parse(row["COUNT_NUMBER"].ToString());
                        var isPermanent = row["IS_PERMANENT"].ToString();
                        if (isPermanent == "False")
                        {
                            if (sumNum <= 0)
                                isConinue = false;
                        }
                    }
                    else
                    {
                        isConinue = false;
                    }
                    if (isConinue == false)
                        return new { Table = "", IS_SUCCESS = false, MSG = "没有剩余可计算次数，请充值或看广告。" };
                }
                var list = new List<LangwenResonanceModel>();
                if (zhuwuqi != "")
                    list = ChangeModel(list, "zhuwuqi", zhuwuqi);
                if (fuwuqi != "")
                    list = ChangeModel(list, "fuwuqi", fuwuqi);
                if (xianglian != "")
                    list = ChangeModel(list, "xianglian", xianglian);
                if (jiezhi != "")
                    list = ChangeModel(list, "jiezhi", jiezhi);
                if (huwan != "")
                    list = ChangeModel(list, "huwan", huwan);
                if (waitao != "")
                    list = ChangeModel(list, "waitao", waitao);
                if (xiezi != "")
                    list = ChangeModel(list, "xiezi", xiezi);
                if (shouzhuo != "")
                    list = ChangeModel(list, "shouzhuo", shouzhuo);
                if (yupei != "")
                    list = ChangeModel(list, "yupei", yupei);
                if (maozi != "")
                    list = ChangeModel(list, "maozi", maozi);
                if (yaodai != "")
                    list = ChangeModel(list, "yaodai", yaodai);
                if (neicheng != "")
                    list = ChangeModel(list, "neicheng", neicheng);

                //金色琅纹属性表
                var goldLangwenData = LangWenData.X.goldLangwenModels;
                //紫色琅纹属性表
                var purpleLangwen = LangWenData.X.purpleLangwenModels;
                //所有的琅纹属性加成汇总
                var totalLangwenAttrCountList = new List<AttrModel>();
                //所有的琅纹功力汇总
                var totalGongli = 0.00m;
                //循环所有的琅纹
                var goldLangwenList = new List<LangwenModel>();
                foreach (var item in list)
                {
                    var strDic = new Dictionary<string, decimal>();
                    var skillSum = 0.00m;
                    foreach (var buweiLangwen in item.buwenList)
                    {
                        if (buweiLangwen.color == "金" && buweiLangwen.name != "【天】皇天" && buweiLangwen.name != "【地】厚土")
                        {
                            //添加到计算数据中去
                            var isExitGold = goldLangwenList.Where(p => p.langwenName == buweiLangwen.name && p.langwenLevel == buweiLangwen.level).FirstOrDefault();
                            if (isExitGold != null)
                                isExitGold.langwenNum += 1;
                            else
                                goldLangwenList.Add(new LangwenModel() { langwenName = buweiLangwen.name, langwenLevel = buweiLangwen.level, langwenNum = 1.00m });
                            //根据属性+等级查询对应数值
                            var langwenData = goldLangwenData.Select().Where(p => p["name"].ToString() == buweiLangwen.name).SingleOrDefault();
                            if (langwenData != null)
                            {
                                //属性
                                var attrStr = langwenData["attr"].ToString();
                                //计算属性功力
                                var backModel = GoldLangwenSum(attrStr, "LEVEL_" + buweiLangwen.level);
                                //汇总功力
                                skillSum += backModel.skill;
                                //汇总属性值
                                var attrList = backModel.str.Split(',');
                                foreach (var attr in attrList)
                                {
                                    var attrs = attr.Split('+');
                                    var val = attrs[1].ToString();
                                    if (val.Contains('%'))
                                    {
                                        val = val.Replace("%", "");
                                    }
                                    if (strDic.ContainsKey(attrs[0]))
                                    {
                                        var value = decimal.Parse(strDic[attrs[0]].ToString());
                                        strDic[attrs[0]] = value + decimal.Parse(val);
                                    }
                                    else
                                    {
                                        strDic.Add(attrs[0], decimal.Parse(val));
                                    }
                                }
                            }
                        }
                        //紫色
                        else
                        {
                            //查询琅纹
                            var langwenData = purpleLangwen.Select().Where(p => p["name"].ToString() == buweiLangwen.name && p["LEVEL"].ToString() == buweiLangwen.level).SingleOrDefault();
                            if (langwenData != null)
                            {
                                var backModel = purpleSum(langwenData);
                                //汇总功力
                                skillSum += backModel.skill;
                                //汇总属性值
                                var attrList = backModel.str.Split(',');
                                foreach (var attr in attrList)
                                {
                                    var attrs = attr.Split('+');
                                    if (strDic.ContainsKey(attrs[0]))
                                    {
                                        var value = decimal.Parse(strDic[attrs[0]].ToString());
                                        strDic[attrs[0]] = value + decimal.Parse(attrs[1].ToString());
                                    }
                                    else
                                    {
                                        strDic.Add(attrs[0], decimal.Parse(attrs[1].ToString()));
                                    }
                                }
                            }
                        }
                    }
                    var gongmingSkill = 0.00m;
                    //共鸣加成
                    var sumDic = sumGongming(item, strDic, out gongmingSkill);
                    //汇总数据
                    foreach (var dic in sumDic)
                    {
                        var isExit = totalLangwenAttrCountList.Where(p => p.Key == dic.Key).FirstOrDefault();
                        if (isExit != null)
                        {
                            isExit.Value += dic.Value;
                        }
                        else
                        {
                            totalLangwenAttrCountList.Add(new AttrModel() { Key = dic.Key, Value = dic.Value });
                        }
                    }
                    totalGongli += skillSum;
                    totalGongli += gongmingSkill;

                    item.skill = skillSum + gongmingSkill;

                    var str = "";
                    foreach (var strd in sumDic)
                    {
                        var val = Math.Floor(strd.Value).ToString();
                        if (strd.Key == "会心增伤" || strd.Key == "会心减伤" || strd.Key == "暗伤增伤" || strd.Key == "暗伤减伤")
                        {
                            var val1 = Math.Round(strd.Value, 3).ToString();
                            val = val1 + "%";
                        }
                        if (val != "0")
                            str += strd.Key + "+" + val + ",";
                    }
                    if (str != "")
                        str = str.Substring(0, str.Length - 1);
                    item.attrStr = str;
                }

                //构造返回Model
                var returnModel = new ReturnMainModel();
                returnModel.detailList = new List<ReturDetialModel>();
                foreach (var item in list)
                {
                    var detailModel = new ReturDetialModel();
                    detailModel.buwei = item.buwei;
                    detailModel.attrStr = item.attrStr;
                    detailModel.skill = Math.Floor(item.skill);
                    returnModel.detailList.Add(detailModel);
                }
                //汇总所有的属性值
                //var totalAttrStr = "";
                var firstStr = "";
                var secondStr = "";
                var threeStr = "";
                var foreStr = "";
                var fiveStr = "";
                foreach (var strd in totalLangwenAttrCountList)
                {
                    var val = Math.Floor(strd.Value).ToString();
                    var item = strd.Key;
                    if (item == "会心增伤" || item == "会心减伤" || item == "暗伤增伤" || item == "暗伤减伤")
                    {
                        var val1 = Math.Round(strd.Value, 3).ToString();
                        val = val1 + "%";
                    }
                    if (val != "0")
                    {
                        if (item == "破招" || item == "拆招" || item == "攻击" || item == "气血")
                            firstStr += item + "+" + val + ",";
                        else if (item == "会心增伤" || item == "暗伤增伤" || item == "会心减伤" || item == "暗伤减伤")
                            secondStr += item + "+" + val + ",";
                        else if (item == "力道" || item == "气劲" || item == "会心" || item == "暗伤")
                            threeStr += item + "+" + val + ",";
                        else if (item == "身法" || item == "根骨" || item == "外功防御" || item == "内功防御")
                            foreStr += item + "+" + val + ",";
                        else if (item == "会心抗性" || item == "暗伤抗性" || item == "会心减伤" || item == "暗伤减伤")
                            fiveStr += item + "+" + val + ",";
                    }
                }
                if (firstStr != "")
                    firstStr = firstStr.Substring(0, firstStr.Length - 1);
                if (secondStr != "")
                    secondStr = secondStr.Substring(0, secondStr.Length - 1);
                if (threeStr != "")
                    threeStr = threeStr.Substring(0, threeStr.Length - 1);
                if (foreStr != "")
                    foreStr = foreStr.Substring(0, foreStr.Length - 1);
                if (fiveStr != "")
                    fiveStr = fiveStr.Substring(0, fiveStr.Length - 1);

                //根据所有的金色琅纹计算碎片/钥匙/宝箱数
                //按name+level 去重汇总数量
                var listLastModel = new List<ReturnLastList>();
                var totalKey = 0.00m;
                var totalPrice = 0.00m;
                foreach (var item in goldLangwenList)
                {
                    var backModel = sunGoldData(item.langwenName, item.langwenLevel, item.langwenNum, keyPrice);
                    var isExit = listLastModel.Where(p => p.name == backModel.name).FirstOrDefault();
                    if (isExit != null)
                    {
                        isExit.price += backModel.price;
                        isExit.keyNumber += backModel.keyNumber;
                    }
                    else
                    {
                        listLastModel.Add(backModel);
                    }
                    totalKey += backModel.keyNumber;
                    totalPrice += backModel.price;
                }

                returnModel.firstStr = firstStr;
                returnModel.sencondStr = secondStr;
                returnModel.threeStr = threeStr;
                returnModel.foreStr = foreStr;
                returnModel.fiveStr = fiveStr;
                returnModel.totalSkill = Math.Floor(totalGongli);

                var intKeys = 0;
                var intPrice = 0;
                if (totalKey.ToString() != "0.00") intKeys = int.Parse(Math.Floor(totalKey).ToString());
                if (totalPrice.ToString() != "0.00") intPrice = int.Parse(Math.Floor(totalPrice).ToString());
                returnModel.totalKeysXiaofei = "钥匙总数量：" + intKeys + "把, 总消费：" + intPrice + "点";
                returnModel.detailStr = listLastModel.ToJsonString();

                //扣减次数
                //执行sql
                using (var x = Join.Dal.MySqlProvider.X())
                {
                    var selNumSql = "select * from a_user where kid='" + userId + "'";
                    var table = x.ExecuteSqlCommand(selNumSql).Tables[0];
                    if (table.Rows.Count > 0)
                    {
                        var row = table.Rows[0];
                        var sumNum = decimal.Parse(row["COUNT_NUMBER"].ToString());
                        var isPermanent = row["IS_PERMANENT"].ToString();
                        if (isPermanent == "False")
                        {
                            sumNum = sumNum - 1;
                        }
                        //更新扣减次数
                        var updateSql = "update a_user set COUNT_NUMBER=" + sumNum + " where kid='" + userId + "'";
                        this.WriteLogFile("用户：" + userId + " 琅纹计算消耗次数-1，剩余次数:" + sumNum);
                        x.ExecuteSqlCommand(updateSql);
                    }
                }
                return new { Table = returnModel, IS_SUCCESS = true, MSG = "" };
            });
        }

        //计算共鸣
        public Dictionary<string, decimal> sumGongming(LangwenResonanceModel langwenReson, Dictionary<string, decimal> attrDic, out decimal totalGongli)
        {
            totalGongli = 0.00m;
            var modelList = new List<AttrModel>();
            foreach (var item in attrDic)
            {
                modelList.Add(new AttrModel() { Key = item.Key, Value = item.Value });
            }
            //所有应该计算的共鸣属性
            var allAttr = "攻击,破招,气血,拆招";
            var allAttrList = allAttr.Split(',');
            foreach (var item in allAttrList)
            {
                var isExit = modelList.Where(p => p.Key == item).FirstOrDefault();
                if (isExit == null)
                {
                    modelList.Add(new AttrModel() { Key = item, Value = 0 });
                }
            }

            var resultDic = new Dictionary<string, decimal>();
            var buwenList = langwenReson.buwenList;
            var purpleNum = buwenList.Where(p => p.color == "紫").Count();
            var goldNum = buwenList.Where(p => p.color == "金").Count();
            var level = buwenList.Select(p => int.Parse(p.level)).Distinct().Min();
            //查询数据库，获取共鸣
            using (var x = Join.Dal.MySqlProvider.X())
            {
                //根据部位判断是攻击还是防御
                var gjBuWeiStr = "zhuwuqi，fuwuqi，xianglian，jiezhi，huwan，yaodai";
                var gjBuWeiList = gjBuWeiStr.Split('，');
                var fyBuWeiStr = "waitao，xiezi，shouzhuo，yupei，maozi，neicheng";
                var fyBuWeiList = fyBuWeiStr.Split('，');
                var ATTR = "";
                if (gjBuWeiList.Contains(langwenReson.buwei))
                    ATTR = "ATTR = '攻击类'";
                else if (fyBuWeiList.Contains(langwenReson.buwei))
                    ATTR = "ATTR = '防御类'";
                else
                    ATTR = "1=1";
                var selectSql = "SELECT * FROM b_langwen_resonance where LEVEL='" + level + "' and PURPLE_NUMBER='" + purpleNum + "' and GOLD_NUMBER='" + goldNum + "' and " + ATTR + "";
                var table = x.ExecuteSqlCommand(selectSql);
                var gongmingTable = table.Tables[0];
                if (gongmingTable.Rows.Count > 0)
                {
                    var gongmingData = gongmingTable.Rows[0];
                    foreach (var item in modelList)
                    {
                        var sumValue = item.Value;
                        //如果是需要参与运算的属性
                        if (GongMingData.X.allAttrList.Contains(item.Key))
                        {
                            //攻击类
                            if (gjBuWeiList.Contains(langwenReson.buwei))
                            {
                                if (item.Key == "攻击")
                                {
                                    var gongjiValue = decimal.Parse(gongmingData["GJ"].ToString());
                                    sumValue += gongjiValue;
                                    var skill = sumGongmingSkill(item.Key, gongjiValue);
                                    totalGongli += skill;
                                }
                                if (item.Key == "破招")
                                {
                                    var pozhaoValue = decimal.Parse(gongmingData["PZ"].ToString());
                                    var pozhaoShuxingValue = decimal.Parse(gongmingData["PZJC"].ToString());
                                    sumValue += item.Value * pozhaoShuxingValue;
                                    sumValue += pozhaoValue;
                                    var skill = sumGongmingSkill(item.Key, item.Value * pozhaoShuxingValue + pozhaoValue);
                                    totalGongli += skill;
                                }
                                if (item.Key == "会心" || item.Key == "暗伤" || item.Key == "会心增伤" || item.Key == "暗伤增伤" || item.Key == "攻击" || item.Key == "破招")
                                {
                                    var beforValue = decimal.Parse(gongmingData["GJSX"].ToString());
                                    sumValue += item.Value * beforValue;
                                    var skill = sumGongmingSkill(item.Key, item.Value * beforValue);
                                    totalGongli += skill;
                                }
                                if (item.Key == "会心" || item.Key == "暗伤" || item.Key == "会心增伤" || item.Key == "暗伤增伤" ||
                                   item.Key == "会心抗性" || item.Key == "暗伤抗性" || item.Key == "会心减伤" || item.Key == "暗伤减伤")
                                {
                                    var beforValue = decimal.Parse(gongmingData["TSSX"].ToString());
                                    sumValue += item.Value * beforValue;
                                    var skill = sumGongmingSkill(item.Key, item.Value * beforValue);
                                    totalGongli += skill;
                                }
                                if (item.Key == "力道" || item.Key == "气劲")
                                {
                                    var ldqjMarkUp = decimal.Parse(gongmingData["LD_QJ"].ToString());
                                    sumValue += item.Value * ldqjMarkUp;
                                    var skill = sumGongmingSkill(item.Key, item.Value * ldqjMarkUp);
                                    totalGongli += skill;
                                }
                                else
                                {

                                }
                            }
                            //防御类
                            else if (fyBuWeiList.Contains(langwenReson.buwei))
                            {
                                if (item.Key == "气血")
                                {
                                    var beforValue = decimal.Parse(gongmingData["QX"].ToString());
                                    sumValue += beforValue;
                                    var skill = sumGongmingSkill(item.Key, beforValue);
                                    totalGongli += skill;
                                }
                                if (item.Key == "拆招")
                                {
                                    var beforValue = decimal.Parse(gongmingData["CZ"].ToString());
                                    var chaizhaoShuxingValue = decimal.Parse(gongmingData["CZJC"].ToString());
                                    sumValue += item.Value * chaizhaoShuxingValue;
                                    sumValue += beforValue;
                                    var skill = sumGongmingSkill(item.Key, item.Value * chaizhaoShuxingValue + beforValue);
                                    totalGongli += skill;
                                }
                                if (item.Key == "内功防御" || item.Key == "外功防御" || item.Key == "气血" || item.Key == "会心抗性"
                                     || item.Key == "暗伤抗性" || item.Key == "会心减伤" || item.Key == "暗伤减伤" || item.Key == "拆招")
                                {
                                    var beforValue = decimal.Parse(gongmingData["FYSX"].ToString());
                                    sumValue += item.Value * beforValue;
                                    var skill = sumGongmingSkill(item.Key, item.Value * beforValue);
                                    totalGongli += skill;
                                }
                                if (item.Key == "会心" || item.Key == "暗伤" || item.Key == "会心增伤" || item.Key == "暗伤增伤" ||
                                    item.Key == "会心抗性" || item.Key == "暗伤抗性" || item.Key == "会心减伤" || item.Key == "暗伤减伤")
                                {
                                    var beforValue = decimal.Parse(gongmingData["TSSX"].ToString());
                                    sumValue += item.Value * beforValue;
                                    var skill = sumGongmingSkill(item.Key, item.Value * beforValue);
                                    totalGongli += skill;
                                }
                                if (item.Key == "根骨" || item.Key == "身法")
                                {
                                    var ldqjMarkUp = decimal.Parse(gongmingData["GG_SF"].ToString());
                                    sumValue += item.Value * ldqjMarkUp;
                                    var skill = sumGongmingSkill(item.Key, item.Value * ldqjMarkUp);
                                    totalGongli += skill;
                                }
                            }
                        }
                        item.Value = sumValue;
                    }
                }
            }
            foreach (var item in modelList)
            {
                resultDic.Add(item.Key, item.Value);
            }
            return resultDic;
        }

        public decimal sumGongmingSkill(string name, decimal value)
        {
            var skill = 0.00m;
            //如果是会心增伤/会心减伤，暗伤增伤/暗伤减伤，×100拼接百分号
            if (name == "会心增伤" || name == "会心减伤" || name == "暗伤增伤" || name == "暗伤减伤")
                skill += CalculationSkill(name, value / 100) * 10;
            //否则取整
            else
                skill += CalculationSkill(name, value);
            return skill;
        }

        //计算金色琅纹数据
        public ReturnLastList sunGoldData(string name, string level, decimal num, decimal keyPrice)
        {
            var goldLangwen = LangWenData.X.goldLangwenModels;
            //查询琅纹
            var langwenData = goldLangwen.Select().Where(p => p["name"].ToString() == name).SingleOrDefault();
            if (langwenData != null)
            {
                //基础属性
                //单个琅纹升级所需数量
                var upgradeNum = int.Parse(langwenData["LEVEL_" + level].ToString());
                //单个宝箱开出数量
                var treasureNum = int.Parse(langwenData["OPEN_NUMBER"].ToString());
                //绑点价格
                var bindPrice = decimal.Parse(langwenData["PRICE"].ToString());

                //总琅纹升级所需数量
                var totalNum = upgradeNum * num;
                //总需要宝箱数量 = 总钥匙数量
                decimal totalTreasure = 0;
                if (totalNum % treasureNum == 0)
                    totalTreasure = totalNum / treasureNum;
                else
                    totalTreasure = Math.Floor(totalNum / treasureNum) + 1;
                //总宝箱价格
                var totalPrice = totalTreasure * bindPrice;
                //总钥匙价格
                var totalKeyPrice = totalTreasure * keyPrice;
                //总消费
                var totalAllPrice = totalPrice + totalKeyPrice;
                var returnModel = new ReturnLastList()
                {
                    name = name,
                    keyNumber = totalTreasure,
                    price = totalAllPrice
                };
                return returnModel;
            }
            return null;
        }

        public class ReturnLastList
        {
            public string name { get; set; }
            public decimal keyNumber { get; set; }
            public decimal price { get; set; }
        }

        public class AttrModel
        {
            public string Key { get; set; }
            public decimal Value { get; set; }
        }

        public class LangwenModel
        {
            public string langwenName { get; set; }
            public string langwenLevel { get; set; }
            public decimal langwenNum { get; set; }
        }

        public class ReturnMainModel
        {
            public string firstStr { get; set; }
            public string sencondStr { get; set; }
            public string threeStr { get; set; }
            public string foreStr { get; set; }
            public string fiveStr { get; set; }
            public decimal totalSkill { get; set; }
            public string totalKeysXiaofei { get; set; }
            public string detailStr { get; set; }
            public List<ReturDetialModel> detailList { get; set; }
        }

        public class ReturDetialModel
        {
            public string buwei { get; set; }
            public string attrStr { get; set; }
            public decimal skill { get; set; }
        }

        public class LangwenResonanceModel
        {
            public string buwei { get; set; }
            public string attrStr { get; set; }
            public decimal skill { get; set; }
            public List<LangwenResonanceBuWeiModel> buwenList { get; set; }
        }

        public class LangwenResonanceBuWeiModel
        {
            public string name { get; set; }
            public string level { get; set; }
            public string color { get; set; }
        }


        public List<LangwenResonanceModel> ChangeModel(List<LangwenResonanceModel> list, string buwei, string strs)
        {
            //拆分选择琅纹
            var strList = strs.Split(',');
            var model = new LangwenResonanceModel();
            model.buwenList = new List<LangwenResonanceBuWeiModel>();
            model.buwei = buwei;
            foreach (var item in strList)
            {
                var detaiModel = new LangwenResonanceBuWeiModel();
                //拆分等级跟琅纹名称数组
                var itemList = item.Split('_');
                detaiModel.name = itemList[0];
                detaiModel.level = itemList[1];
                detaiModel.color = itemList[2];
                model.buwenList.Add(detaiModel);
            }
            list.Add(model);
            return list;
        }

        #region WriteLogFile
        public void WriteLogFile(string input)
        {
            DateTime now = DateTime.Now;
            var date = now.Year + "" + now.Month + "" + now.Day;
            var time = now.ToLongTimeString();
            var application = System.Web.HttpContext.Current.Server.MapPath("/LangwenSum/" + date + "/");

            if (Directory.Exists(application) == false)
            //如果不存在就创建file文件夹
            {
                Directory.CreateDirectory(application);
            }

            var fileName = application + "wxp" + ".txt";
            FileStream fs = new FileStream(fileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            try
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    //开始写入               
                    string str = time + "----" + input;
                    sw.WriteLine(str);

                    //清空缓冲区
                    sw.Flush();
                    sw.Close();
                    //关闭流
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                fs.Close();
            }
        }
        #endregion
        #endregion
    }
}
