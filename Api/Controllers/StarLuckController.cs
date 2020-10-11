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
    [RoutePrefix("api/_star")]
    //[Security.AuthorizationRequired]
    public class StarLuckController : BaseController
    {
        #region 计算星运
        /// <summary>
        /// 计算星运
        /// http://localhost:64665/api/_cud/createPartner
        /// </summary>
        [HttpPost]
        [Route("calculation")]
        public IHttpActionResult Calculation([FromBody]JToken json)
        {
            return this.TryReturn<object>(() =>
            {
                try
                {
                    using (var x = Join.Dal.MySqlProvider.X())
                    {
                        var jtoken = json.AsDynamic();
                        decimal li = jtoken.li;
                        decimal ren = jtoken.ren;
                        decimal lv = jtoken.lv;
                        decimal kuang = jtoken.kuang;
                        //横向数值=仁/（厉+仁）
                        var hValue = ren / (li + ren);
                        var hengValue = (int)(hValue * 100) / 100.00m;
                        //纵向数值=律/（律+狂）
                        var zValue = lv / (lv + kuang);
                        var zongValue = (int)(zValue * 100) / 100.00m;

                        //拉取所有星运区间
                        var dataList = StarLuckData.X.dataModels;
                        var datas = dataList.Where(p => p.X_BEGAN <= hengValue && p.X_END >= hengValue && p.Y_BEGAN <= zongValue && p.Y_END >= zongValue);
                        var isExit = datas.Select(p => p.C_NAME).Distinct();
                        if (isExit.Count() > 1)
                        {
                            return new { Table = "", IS_SUCCESS = false, MSG = "计算错误，存在重复数据" };
                        }
                        else if (isExit.Count() == 1)
                        {
                            var returnTable = new object();
                            var issucess = true;
                            var msg = "";
                            foreach (var item in datas)
                            {
                                var data = item;
                                //计算阶段
                                var result = data.DETAILS.Where(p => p.X_BEGAN <= hengValue && p.X_END >= hengValue && p.Y_BEGAN <= zongValue && p.Y_END >= zongValue).FirstOrDefault();
                                if (result != null)
                                {
                                    issucess = true;
                                    msg = "";
                                    var text1 = "";
                                    var text2 = "";
                                    if (data.C_NAME == "破军")
                                    {
                                        switch (result.NAME)
                                        {
                                            case "利照":
                                                text1 = "如果提升至旺相阶段需要：";
                                                text2 = "提升仁、律，降低厉、狂";
                                                break;
                                            case "旺相":
                                                text1 = "如果提升至耀升阶段需要：";
                                                text2 = "提升仁、律，降低厉、狂";
                                                break;
                                            case "耀升":
                                                text1 = "如需要转换为另一种星运";
                                                text2 = "请点击上方星运转换界面";
                                                break;
                                        }
                                    }
                                    else if (data.C_NAME == "紫薇")
                                    {
                                        switch (result.NAME)
                                        {
                                            case "利照":
                                                text1 = "如果提升至旺相阶段需要：";
                                                text2 = "提升仁、狂，降低厉、律";
                                                break;
                                            case "旺相":
                                                text1 = "如果提升至耀升阶段需要：";
                                                text2 = "提升仁、狂，降低厉、律";
                                                break;
                                            case "耀升":
                                                text1 = "如需要转换为另一种星运";
                                                text2 = "请点击上方星运转换界面";
                                                break;
                                        }
                                    }
                                    else if (data.C_NAME == "贪狼")
                                    {
                                        switch (result.NAME)
                                        {
                                            case "利照":
                                                text1 = "如果提升至旺相阶段需要：";
                                                text2 = "提升厉、律，降低仁、狂";
                                                break;
                                            case "旺相":
                                                text1 = "如果提升至耀升阶段需要：";
                                                text2 = "提升厉、律，降低仁、狂";
                                                break;
                                            case "耀升":
                                                text1 = "如需要转换为另一种星运";
                                                text2 = "请点击上方星运转换界面";
                                                break;
                                        }
                                    }
                                    else if (data.C_NAME == "七煞")
                                    {
                                        switch (result.NAME)
                                        {
                                            case "利照":
                                                text1 = "如果提升至旺相阶段需要：";
                                                text2 = "提升厉、狂，降低仁、律";
                                                break;
                                            case "旺相":
                                                text1 = "如果提升至耀升阶段需要：";
                                                text2 = "提升厉、狂，降低仁、律";
                                                break;
                                            case "耀升":
                                                text1 = "如需要转换为另一种星运";
                                                text2 = "请点击上方星运转换界面";
                                                break;
                                        }
                                    }
                                    else if (data.C_NAME == "天同")
                                    {
                                        switch (result.NAME)
                                        {
                                            case "利照":
                                                text1 = "如果提升至旺相阶段需要：";
                                                text2 = "尽可能让仁和厉、律和狂数值相等";
                                                break;
                                            case "旺相":
                                                text1 = "如果提升至耀升阶段需要：";
                                                text2 = "尽可能让仁和厉、律和狂数值相等";
                                                break;
                                            case "耀升":
                                                text1 = "如需要转换为另一种星运";
                                                text2 = "请点击上方星运转换界面";
                                                break;
                                        }
                                    }
                                    returnTable = new { C_NAME = data.C_NAME, NAME = result.NAME, TEXT1 = text1, TEXT2 = text2 };
                                    //var returnTable = new { C_NAME = data.C_NAME, NAME = result.NAME };
                                }
                                else
                                {
                                    if (returnTable.ToString() == "System.Object")
                                    {
                                        issucess = false;
                                        msg = "未查询到符合区间的数据";
                                    }
                                }
                            }
                            return new { Table = returnTable, IS_SUCCESS = issucess, MSG = msg };
                        }
                        else
                        {
                            return new { Table = "", IS_SUCCESS = false, MSG = "计算错误，未获取到符合区间的数据" };
                        }
                    }
                }
                catch (Exception ex)
                {
                    return new { Table = "", IS_SUCCESS = false, MSG = ex.Message };
                }
            });
        }
        #endregion

        #region 变更阶段
        /// <summary>
        /// 变更阶段
        /// api/_cud/changePhase
        /// </summary>
        [HttpPost]
        [Route("changePhase")]
        public IHttpActionResult ChangePhase([FromBody]JToken json)
        {
            return this.TryReturn<object>(() =>
            {
                try
                {
                    using (var x = Join.Dal.MySqlProvider.X())
                    {
                        var jtoken = json.AsDynamic();
                        string before = jtoken.before;
                        string after = jtoken.after;

                        var returnResult = "";

                        if (before == "破军")
                        {
                            switch (after)
                            {
                                case "紫薇":
                                    returnResult = "提升狂，降低律";
                                    break;
                                case "贪狼":
                                    returnResult = "提升厉，降低仁";
                                    break;
                                case "七煞":
                                    returnResult = "提升狂、厉，降低仁、律";
                                    break;
                                case "天同":
                                    returnResult = "尽可能让仁和厉、律和狂数值相等";
                                    break;
                            }
                        }
                        else if (before == "紫薇")
                        {
                            switch (after)
                            {
                                case "破军":
                                    returnResult = "提升律，降低狂";
                                    break;
                                case "贪狼":
                                    returnResult = "提升厉、律，降低仁、狂";
                                    break;
                                case "七煞":
                                    returnResult = "提升厉，降低仁";
                                    break;
                                case "天同":
                                    returnResult = "尽可能让仁和厉、律和狂数值相等";
                                    break;
                            }
                        }
                        else if (before == "贪狼")
                        {
                            switch (after)
                            {
                                case "破军":
                                    returnResult = "提升仁，降低厉";
                                    break;
                                case "紫薇":
                                    returnResult = "提升仁、狂，降低厉、律";
                                    break;
                                case "七煞":
                                    returnResult = "提升狂，降低律";
                                    break;
                                case "天同":
                                    returnResult = "尽可能让仁和厉、律和狂数值相等";
                                    break;
                            }
                        }
                        else if (before == "七煞")
                        {
                            switch (after)
                            {
                                case "破军":
                                    returnResult = "提升仁、律，降低厉、狂";
                                    break;
                                case "紫薇":
                                    returnResult = "提升仁，降低厉";
                                    break;
                                case "贪狼":
                                    returnResult = "提升律，降低狂";
                                    break;
                                case "天同":
                                    returnResult = "尽可能让仁和厉、律和狂数值相等";
                                    break;
                            }
                        }
                        else if (before == "天同")
                        {
                            switch (after)
                            {
                                case "破军":
                                    returnResult = "提升仁、律，降低厉、狂";
                                    break;
                                case "紫薇":
                                    returnResult = "提升仁、狂，降低厉、律";
                                    break;
                                case "贪狼":
                                    returnResult = "提升厉、律，降低仁、狂";
                                    break;
                                case "七煞":
                                    returnResult = "提升厉、狂，降低仁、律";
                                    break;
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(returnResult))
                        {
                            var returnTable = new { RESULT = returnResult };
                            return new { Table = returnTable, IS_SUCCESS = true, MSG = "" };
                        }
                        return new { Table = "", IS_SUCCESS = false, MSG = "未查询到转换" };
                    }
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
