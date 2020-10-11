using Join;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Http;
using System.Web.Script.Serialization;
using System.Linq;

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
                                        }
                                        //否则取整
                                        else
                                        {
                                            strLevelValue = Math.Floor(levelValue).ToString();
                                        }
                                        //计算功力
                                        skill += CalculationSkill(item, levelValue);
                                        //属性
                                        str += item + "+" + strLevelValue + ",";
                                    }
                                }
                                str = str.Substring(0, str.Length - 1);
                                var returnTable = new { attrStr = str, skill = Math.Floor(skill), keyNumber = totalTreasure, totalTreasurePrice = totalPrice, totalKeyPrice = totalKeyPrice };
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
                    backResult = value * 2.15m;
                    break;
                case "暗伤增伤":
                    backResult = value * 2.15m;
                    break;
                case "会心减伤":
                    backResult = value * 2.15m;
                    break;
                case "暗伤减伤":
                    backResult = value * 2.15m;
                    break;
            }
            return backResult;
        }
        #endregion
    }
}
