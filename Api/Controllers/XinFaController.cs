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
    [RoutePrefix("api/_xinfa")]
    public class XinFaController : BaseController
    {
        #region 计算心法
        /// <summary>
        /// 计算心法
        /// api/_xinfa/sumXinFa
        /// </summary>
        [HttpPost]
        [Route("sumXinFa")]
        public IHttpActionResult SumXinFa([FromBody]JToken json)
        {
            return this.TryReturn<object>(() =>
            {
                try
                {

                    var jtoken = json.AsDynamic();
                    int renNow = jtoken.renNow;
                    int renTarget = jtoken.renTarget;
                    int diNow = jtoken.diNow;
                    int diTarget = jtoken.diTarget;
                    int tianNow = jtoken.tianNow;
                    int tianTarget = jtoken.tianTarget;
                    //心法数据
                    var xinfaList = XinFaData.X.xinfaList;

                    decimal lanseNum = 0.00m;
                    decimal ziseNum = 0.00m;
                    decimal jinseNum = 0.00m;

                    //人
                    if (renNow != 0 && renTarget != 0)
                    {
                        for (int i = renNow + 1; i <= renTarget; i++)
                        {
                            var data = xinfaList.Where(p => p.LEVEL == i).SingleOrDefault();
                            if (data != null)
                            {
                                lanseNum += data.REN_UPNUM;
                            }
                        }
                    }

                    //地
                    if (diNow != 0 && diTarget != 0)
                    {
                        for (int i = diNow + 1; i <= diTarget; i++)
                        {
                            var data = xinfaList.Where(p => p.LEVEL == i).SingleOrDefault();
                            if (data != null)
                            {
                                ziseNum += data.DI_UPNUM;
                            }
                        }
                    }

                    //天
                    if (tianNow != 0 && tianTarget != 0)
                    {
                        for (int i = tianNow + 1; i <= tianTarget; i++)
                        {
                            var data = xinfaList.Where(p => p.LEVEL == i).SingleOrDefault();
                            if (data != null)
                            {
                                jinseNum += data.TIAN_UPNUM;
                            }
                        }
                    }
                    var returnResult = new
                    {
                        lanseNum = Math.Ceiling(lanseNum / 10),
                        ziseNum = Math.Ceiling(ziseNum / 50),
                        jinseNum = Math.Ceiling(jinseNum / 300),
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
