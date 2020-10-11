using Join;
using Newtonsoft.Json.Linq;
using System;
using System.Web.Http;

namespace TransactionAppletaApi
{
    [RoutePrefix("api/_search")]
    //[Security.AuthorizationRequired]
    public class SearchController : BaseController
    {
        #region X.成员方法[测试]
        /// <summary>
        /// 测试
        /// </summary>
        [HttpGet]
        [Route("test/{str}")]
        public string Post(string str)
        {
            return str;
        }
        #endregion

        /// <summary>
        /// 通用查询表 
        /// http://localhost:64665/api/_search/defaultSearch/a_game_setting?filter={"PARENT_ID":null}
        /// </summary>
        [HttpGet]
        [Route("defaultSearch/{tableName}")]
        public IHttpActionResult GetTable(string tableName, string filter)
        {
            return this.TryReturn<object>(() =>
            {
                try
                {
                    var wsql = "";
                    if (filter != "{}")
                    {
                        var jsn = filter.ToJToken();
                        wsql = jsn.ToWhereSql();
                    }
                    var backResult = SearchHelper.SearchTable(tableName, wsql);
                    return new { Table = backResult.Tables[0], IS_SUCCESS = true, MSG = "" };

                }
                catch (Exception ex)
                {
                    return new { Table = "", IS_SUCCESS = false, MSG = ex.Message };
                }
            });
        }

        /// <summary>
        /// POST获取数据
        /// http://localhost:64665/api/_search/postSearch
        /// </summary>
        [HttpPost]
        [Route("postSearch")]
        public IHttpActionResult PostSearch([FromBody]JToken json)
        {
            return this.TryReturn<object>(() =>
            {
                try
                {
                    var jtoken = json.AsDynamic();
                    string tableName = jtoken.tableName;
                    int page = jtoken.page;
                    int limit = jtoken.limit;
                    JToken filter = jtoken.filters;
                    string orderByField = jtoken.orderByField;
                    string isDesc = jtoken.isDesc;
                    var wsql = filter.ToFilterSql();
                    //构造take
                    var backResult = SearchHelper.SearchTable(tableName, wsql, page, limit, orderByField, isDesc);
                    return new { Table = backResult.Tables[0], IS_SUCCESS = true, MSG = "" };
                }
                catch (Exception ex)
                {
                    return new { Table = "", IS_SUCCESS = false, MSG = ex.Message };
                }

            });
        }

        /// <summary>
        /// 奇遇查询
        /// http://localhost:64665/api/_search/postQiyuSearch
        /// </summary>
        [HttpPost]
        [Route("postQiyuSearch")]
        public IHttpActionResult postQiyuSearch([FromBody]JToken json)
        {
            return this.TryReturn<object>(() =>
            {
                try
                {
                    var jtoken = json.AsDynamic();
                    string groups = jtoken.groups;
                    string name = jtoken.name;
                    string orderByField = jtoken.orderByField;
                    string isDesc = jtoken.isDesc;
                    //执行sql
                    using (var x = Join.Dal.MySqlProvider.X())
                    {
                        string sql = "";
                        if (string.IsNullOrWhiteSpace(name))
                        {
                            sql = string.Format(@"SELECT a.KID ,B.KID AS DETAIL_ID,a.GROUPS,a.NAME,b.CODE,B.IS_ENABLE,B.CLUE,B.`OPENING_CONDITIONS`,B.`COORDINATE`,B.`ENDING`,B.`FILLER`,B.`CONTRIBUTOR`,B.`IS_DELETE`
                                        ,B.`CRT_TIME`,B.`FILLER_ID`,B.`CONTRIBUTOR_ID`,B.`COORDINATE_URL` FROM `b_adventure_strategy_category` AS a LEFT JOIN `b_adventure_strategy` AS b
                                        ON a.NAME=b.NAME AND b.is_enable=1 where a.groups='{0}'
                                        ", groups);
                        }
                        else
                        {
                            sql = string.Format(@"SELECT a.KID,B.KID AS DETAIL_ID,a.GROUPS,a.NAME,b.CODE,B.IS_ENABLE,B.CLUE,B.`OPENING_CONDITIONS`,B.`COORDINATE`,B.`ENDING`,B.`FILLER`,B.`CONTRIBUTOR`,B.`IS_DELETE`
                                        ,B.`CRT_TIME`,B.`FILLER_ID`,B.`CONTRIBUTOR_ID`,B.`COORDINATE_URL` FROM `b_adventure_strategy_category` AS a LEFT JOIN `b_adventure_strategy` AS b
                                        ON a.NAME=b.NAME AND b.is_enable=1 where a.groups='{0}' and a.name='{1}'
                                        ", groups, name);
                        }
                        //排序
                        if (orderByField != "" && orderByField != null)
                        {
                            if (isDesc == "1")
                                sql = sql + " order by " + orderByField + " desc";
                            else
                                sql = sql + " order by " + orderByField;
                        }
                        WxPayData wx = new WxPayData();
                        wx.WriteLogFile("执行postQiyuSearch SQL" + sql);
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
