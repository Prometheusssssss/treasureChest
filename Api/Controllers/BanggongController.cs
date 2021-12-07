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
    [RoutePrefix("api/_banggong")]
    public class BanggongController : BaseController
    {
        #region 计算帮贡
        /// <summary>
        /// 计算帮贡
        /// api/_pondering/sumPondering
        /// </summary>
        [HttpPost]
        [Route("sumBanggong")]
        public IHttpActionResult ChangePhase([FromBody]JToken json)
        {
            return this.TryReturn<object>(() =>
            {
                try
                {
                    var jtoken = json.AsDynamic();
                    string name = jtoken.name;
                    int nowLevel = jtoken.nowLevel;
                    int preLevel = jtoken.preLevel;
                    //帮贡数据
                    var banggongList = BanggongData.X.banggongDataList;
                    decimal backTotalBGNum = 0.00m;
                    decimal backTotalSYNum = 0.00m;
                    decimal backTotalTSNum = 0.00m;
                    decimal backTotalGLNum = 0.00m;
                    //计算总
                    var diffNum = preLevel - nowLevel;
                    for (int i = 1; i <= diffNum; i++)
                    {
                        var level = nowLevel + i;
                        var levelData = banggongList.Where(p => p.LEVEL == level && p.NAME == name).FirstOrDefault();
                        //数据库取值
                        backTotalSYNum += levelData.SILVER;
                        backTotalBGNum += levelData.BANGGONG;
                        backTotalTSNum += levelData.TISHENG;
                        backTotalGLNum += levelData.GONGLI;
                    }
                    var returnResult = new
                    {
                        BGNum = Math.Round(backTotalBGNum, 0),
                        TSNum = Math.Round(backTotalTSNum, 0),
                        SYNum = Math.Round(backTotalSYNum, 0),
                        GLNum = Math.Round(backTotalGLNum, 0),
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
