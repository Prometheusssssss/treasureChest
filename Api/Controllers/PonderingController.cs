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
    [RoutePrefix("api/_pondering")]
    public class PonderingController : BaseController
    {
        #region 计算琢磨
        /// <summary>
        /// 计算琢磨
        /// api/_pondering/sumPondering
        /// </summary>
        [HttpPost]
        [Route("sumPondering")]
        public IHttpActionResult ChangePhase([FromBody]JToken json)
        {
            return this.TryReturn<object>(() =>
            {
                try
                {

                    var jtoken = json.AsDynamic();
                    int nowLevel = jtoken.nowLevel;
                    int preLevel = jtoken.preLevel;
                    int jingongLevel = jtoken.jingongLevel;
                    decimal price = jtoken.price;
                    //琢磨数据
                    var prederingList = PrederingData.X.predataList;

                    decimal backJGNum = 0.00m;
                    decimal backTotalJGNum = 0.00m;
                    decimal backYLNum = 0.00m;
                    decimal backTotalYLNum = 0.00m;
                    decimal backSYNum = 0.00m;
                    decimal backTotalSYNum = 0.00m;

                    //计算总
                    var diffNum = preLevel - nowLevel;
                    for (int i = 1; i <= diffNum; i++)
                    {
                        var level = nowLevel + i;
                        var levelData = prederingList.Where(p => p.LEVEL == level).FirstOrDefault();
                        //判断精工等级
                        if (jingongLevel == 2)
                            //所需2级精工=升级经验/15
                            backTotalJGNum += levelData.EXPERIENCE / 15;
                        else if (jingongLevel == 3)
                            //所需3级精工=升级经验/20
                            backTotalJGNum += levelData.EXPERIENCE / 20;
                        else if (jingongLevel == 4)
                            //所需4级精工=升级经验/25
                            backTotalJGNum += levelData.EXPERIENCE / 25;
                        else if (jingongLevel == 5)
                            //所需5级精工=升级经验/30
                            backTotalJGNum += levelData.EXPERIENCE / 30;
                        else if (jingongLevel == 6)
                            //所需6级精工=升级经验/35
                            backTotalJGNum += levelData.EXPERIENCE / 35;
                        else if (jingongLevel == 7)
                            //所需7级精工=升级经验/40
                            backTotalJGNum += levelData.EXPERIENCE / 40;
                        else if (jingongLevel == 8)
                            //所需8级精工=升级经验/45
                            backTotalJGNum += levelData.EXPERIENCE / 45;
                        //backTotalYLNum += backJGNum * price;
                        //碎银消耗=数据库取值
                        backTotalSYNum += levelData.SILVER;
                    }
                    //银两消耗=精工数量*单价
                    backTotalYLNum = backTotalJGNum * price;

                    backJGNum = backTotalJGNum;
                    backYLNum = backTotalYLNum;
                    backSYNum = backTotalSYNum;

                    //精工总消耗=当前等级到预计等级所有精工数量合计*12
                    backTotalJGNum = backTotalJGNum * 12;
                    //银两总消耗=当前等级到预计等级所有银两消耗*12
                    backTotalYLNum = backTotalYLNum * 12;
                    //碎银总消耗=当前等级到预计等级所有碎银总消耗*12
                    backTotalSYNum = backTotalSYNum * 12;

                    var returnResult = new
                    {
                        JGNum = Math.Round(backJGNum, 0),
                        TotalJGNum = Math.Round(backTotalJGNum, 0),
                        YLNum = Math.Round(backYLNum, 0),
                        TotalYLNum = Math.Round(backTotalYLNum, 0),
                        SYNum = Math.Round(backSYNum, 0),
                        TotalSYNum = Math.Round(backTotalSYNum, 0)
                    };
                    return new { Table = returnResult, IS_SUCCESS = true, MSG = "" };
                }
                catch (Exception ex)
                {
                    return new { Table = "", IS_SUCCESS = false, MSG = ex.Message };
                }
            });
        }
        #endregion
    }
}
