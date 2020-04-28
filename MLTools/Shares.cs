using System;
using System.Collections.Generic;
using System.Text;

namespace MLNET
{
    public class Shares
    {
        public string 日期 { get; set; }
        public string 股票代码 { get; set; }
        public string 名称 { get; set; }
        public float 收盘价 { get; set; }
        public float 最高价 { get; set; }
        public float 最低价 { get; set; }
        public float 开盘价 { get; set; }
        public float 前收盘 { get; set; }
        public float 涨跌额 { get; set; }
        public float 涨跌幅 { get; set; }
        public float 换手率 { get; set; }
        public long 成交量 { get; set; }
        public long 成交金额 { get; set; }
        public long 总市值 { get; set; }
        public long 流通市值 { get; set; }

    }
}
