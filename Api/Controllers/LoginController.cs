using Join;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http;

namespace TransactionAppletaApi
{
    [RoutePrefix("api/_login")]
    //[Security.AuthorizationRequired]
    public class LoginController : BaseController
    {
        /// <summary>
        /// 校验是否自动登录  JSCODE登录
        /// http://localhost:64665/api/_login/doAutoLogin
        /// </summary>
        [HttpPost]
        [Route("doAutoLogin")]
        public IHttpActionResult DoAutoLogin([FromBody]JToken json)
        {
            return this.TryReturn<object>(() =>
            {
                try
                {
                    var jsn = json.AsDynamic();
                    string jsCode = jsn.jsCode;
                    //根据JsCode换取OpenId
                    var jsCode2Session = WxPayData.GetOpenId(jsCode);
                    var openId = jsCode2Session.openid;
                    if (openId == "")
                    {
                        return new { Table = "", IS_SUCCESS = false, MSG = "JsCode失效，未获取到OpenId" };
                    }
                    else
                    {
                        using (var x = Join.Dal.MySqlProvider.X())
                        {
                            //根据OPEN_ID去数据库查询登录信息
                            var searchSql = "select * from a_user where open_id='" + openId + "'";
                            var openIdDt = x.ExecuteSqlCommand(searchSql);
                            x.Close();
                            //如果根据OPEN_ID查询不到，不能自动登录
                            if (openIdDt.Tables[0].Rows.Count > 0)
                            {
                                return new { Table = openIdDt.Tables[0], IS_SUCCESS = true, MSG = "" };
                            }
                            else
                            {
                                return new { Table = "", IS_SUCCESS = false, MSG = "请先登录/注册" };
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    return new { Table = "", IS_SUCCESS = false, MSG = ex.Message };
                }
            });
        }

        /// <summary>
        /// 手机号登录/注册
        /// http://localhost:64665/api/_login/doLogin
        /// </summary>
        [HttpPost]
        [Route("doLogin")]
        public IHttpActionResult DoLogin([FromBody]JToken json)
        {
            return this.TryReturn<object>(() =>
            {
                try
                {
                    var jsn = json.AsDynamic();
                    string jsCode = jsn.jsCode;
                    string baseName = jsn.name;
                    var baseStr = Convert.FromBase64String(baseName);
                    var name = Encoding.UTF8.GetString(baseStr);
                    string url = jsn.url;
                    //根据JsCode换取OpenId
                    var jsCode2Session = WxPayData.GetOpenId(jsCode);
                    var openId = jsCode2Session.openid;
                    if (openId == "" || openId == null)
                    {
                        return new { Table = "", IS_SUCCESS = false, MSG = "JsCode失效，未获取到OpenId" };
                    }
                    else
                    {
                        using (var x = Join.Dal.MySqlProvider.X())
                        {
                            //根据OpenId去数据库查询登录信息
                            var searchSql = "select * from a_user where is_delete=0 and OPEN_ID='" + openId + "'";
                            var dt = x.ExecuteSqlCommand(searchSql);
                            //如果未查询到数据，更新OPEN_ID
                            if (dt.Tables[0].Rows.Count == 0)
                            {
                                var insertSql = string.Format(@"insert into a_user (`CODE`,`NAME`,`OPEN_ID`,`IMG_URL`,`REGIST_DATE`,`IS_DELETE`,`IS_SA`,`CRT_TIME`) 
                                                            values('{0}','{1}','{2}','{3}','{4}',0,0,'{5}')"
                                                            , DateTime.Now.ToString("yyyyMMddHHmmss"), name, openId,
                                                             url, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                x.ExecuteSqlCommand(insertSql);
                            }
                            //根据OPEN_ID去数据库查询登录信息
                            var searchResultSql = "select * from a_user where is_delete=0 and OPEN_ID='" + openId + "'";
                            var sdt = x.ExecuteSqlCommand(searchResultSql);
                            x.Close();
                            return new { Table = sdt.Tables[0], IS_SUCCESS = true, MSG = "" };
                        }
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
