using Join;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace TransactionAppletaApi
{
    [RoutePrefix("api/_cud")]
    //[Security.AuthorizationRequired]
    public class CudController : BaseController
    {
        /// <summary>
        /// 插入/更新
        /// http://localhost:64665/api/_cud/createAndUpdate/tableName
        /// </summary>
        [HttpPost]
        [Route("createAndUpdate/{tableName}")]
        public IHttpActionResult CreteoAndUpdateTable(string tablename, [FromBody]JToken json)
        {
            return this.TryReturn<object>(() =>
            {
                try
                {
                    //执行sql
                    using (var x = Join.Dal.MySqlProvider.X())
                    {
                        var dicJson = json.ToJsonString();
                        var dict = dicJson.JsonToDictionary();
                        var kid = dict.GetValue("KID");
                        var sql = "";
                        //new
                        if (kid == "-1")
                        {
                            var ra = new Random();
                            var keys = "";
                            var values = "";
                            if (tablename.ToUpper() != "B_ORDER_MSG_DETAILS" && tablename.ToUpper() != "B_MESSAGE")
                            {
                                keys = "`CODE`,`IS_DELETE`,`CRT_TIME`";
                                values = "'" + DateTime.Now.ToString("yyyyMMddHHmmss") + ra.Next(1000, 9999) + "',0,'" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'";
                            }
                            else
                            {
                                keys = "`IS_DELETE`,`CRT_TIME`";
                                values = "0,'" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'";
                            }
                            foreach (var item in dict)
                            {
                                if (item.Key != "KID")
                                {
                                    keys = keys.AppendSql(item.Key, "`", true);
                                    values = values.AppendSql(item.Value.ToString(), "'", true);
                                }
                            }
                            sql = string.Format(@"insert into {0} ({1}) values ({2})", tablename, keys, values);
                        }
                        //update
                        else
                        {
                            var updateSql = "";
                            foreach (var item in dict)
                            {
                                if (item.Key != "KID")
                                {
                                    if (updateSql != "")
                                        updateSql = updateSql + "`" + item.Key + "`='" + item.Value.ToString() + "',";
                                    else
                                        updateSql = "`" + item.Key + "`='" + item.Value.ToString() + "',";
                                }
                            }
                            updateSql = updateSql.Substring(0, updateSql.Length - 1);

                            sql = string.Format(@"update {0} set {1} where kid='{2}'", tablename, updateSql, kid);
                        }
                        var dt = x.ExecuteSqlCommand(sql);
                        ////执行扩展逻辑
                        //switch (tablename)
                        //{
                        //    case "b_order":
                        //        ExcuteInsertOrderEx(kid);
                        //        break;
                        //    default:
                        //        break;
                        //}
                        return new { Table = dt, IS_SUCCESS = true, MSG = "" };
                    }
                }
                catch (Exception ex)
                {
                    return new { Table = "", IS_SUCCESS = false, MSG = ex.Message };
                }
            });
        }

        /// <summary>
        /// 删除
        /// http://localhost:64665/api/_cud/del/tableName
        /// </summary>
        [HttpPost]
        [Route("del/{tableName}")]
        public IHttpActionResult DelTable(string tablename, [FromBody]JToken json)
        {
            return this.TryReturn<object>(() =>
            {
                try
                {
                    var dicJson = json.ToJsonString();
                    var dict = dicJson.JsonToDictionary();
                    var kid = dict.GetValue("KID");

                    //执行扩展逻辑
                    switch (tablename.ToUpper())
                    {
                        //case "B_PRODUCT_LIST":
                        //    var result = ExcuteDelOrderEx(tablename, kid);
                        //    if (result == false)
                        //        return new { Table = "", IS_SUCCESS = false, MSG = "产品状态为已售卖，不可删除" };
                        //    break;
                        default:
                            break;
                    }
                    //执行sql
                    using (var x = Join.Dal.MySqlProvider.X())
                    {
                        var sql = string.Format(@"update {0} set is_delete=1 where KID='{1}'", tablename, kid);
                        var dt = x.ExecuteSqlCommand(sql);

                        return new { Table = dt, IS_SUCCESS = true, MSG = "" };
                    }
                }
                catch (Exception ex)
                {
                    return new { Table = "", IS_SUCCESS = false, MSG = ex.Message };
                }
            });
        }
    }
}
