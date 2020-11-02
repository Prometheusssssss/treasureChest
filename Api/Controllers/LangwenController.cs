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
                                var attrList = attrStr.Split(',');
                                //根据属性+等级查询对应数值
                                var goldLangwenAttrData = LangWenData.X.goldLangwenAttrModels;
                                //属性
                                var str = "";
                                //功力
                                var skill = 0.00m;
                                foreach (var item in attrList)
                                {
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
                                var returnTable = new { attrStr = str, skill = Math.Floor(skill), keyNumber = totalTreasure, totalNum = totalNum, totalKeyPrice = totalKeyPrice + totalPrice };
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
                    backResult = value * 2.15m * 100;
                    break;
                case "暗伤增伤":
                    backResult = value * 2.15m * 100;
                    break;
                case "会心减伤":
                    backResult = value * 2.15m * 100;
                    break;
                case "暗伤减伤":
                    backResult = value * 2.15m * 100;
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
                                //功力
                                var skill = decimal.Parse(langwenData["SKILL"].ToString());
                                //等级琅纹数量
                                var upNum = int.Parse(langwenData["UP_NUM"].ToString());
                                //琅纹碎片数量
                                var fragementsNum = int.Parse(langwenData["FRAGMENTS_NUM"].ToString());
                                var returnTable = new { attrStr = str, skill = Math.Floor(skill), langwenNum = upNum * num, totalFragementsNum = fragementsNum * num };
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
                            //腰带/内衬
                            var belt = row["BELT"].ToString();
                            var beltLangwenNameList = belt.Split('+').ToList();
                            var beltNameImgList = nameGetImgUrl(beltLangwenNameList, langwenImgUrlNameList);
                            var beltName1ImgList = nameGetImgUrl(beltLangwenNameList, langwenImgUrlNameList);
                            result.Add(new MainModel()
                            {
                                name = "腰带",
                                details = beltNameImgList
                            });
                            result.Add(new MainModel()
                            {
                                name = "内衬",
                                details = beltName1ImgList
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
    }
}
