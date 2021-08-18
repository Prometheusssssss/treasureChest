using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace TransactionAppletaApi
{
    public class BanggongData : BizServiceBase<BanggongData>
    {
        public List<BanggongModel> banggongDataList { get; set; }

        #region L.加载方法
        public void Initial()
        {
            using (var x = Join.Dal.MySqlProvider.X())
            {
                var selSql = "select * from B_BANGGONG_SUM where is_delete=0";
                var table = x.ExecuteSqlCommand(selSql);
                this.banggongDataList = new List<BanggongModel>();
                foreach (DataRow item in table.Tables[0].Rows)
                {
                    var model = new BanggongModel();
                    model.NAME = item["NAME"].ToString();
                    model.LEVEL = int.Parse(item["LEVEL"].ToString());
                    model.BANGGONG = decimal.Parse(item["EXPERIENCE"].ToString());
                    model.SILVER = decimal.Parse(item["EXPERIENCE"].ToString());
                    model.TISHENG = int.Parse(item["SILVER"].ToString());
                    model.GONGLI = int.Parse(item["SILVER"].ToString());
                    this.banggongDataList.Add(model);
                }
            }
        }
        #endregion
    }

    public class BanggongModel
    {
        public string NAME { get; set; }
        public int LEVEL { get; set; }
        public decimal BANGGONG { get; set; }
        public decimal SILVER { get; set; }
        public decimal TISHENG { get; set; }
        public decimal GONGLI { get; set; }
    }
}