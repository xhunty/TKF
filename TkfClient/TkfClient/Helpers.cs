using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tinkoff.InvestApi.V1;

namespace TkfClient
{

    public static class Helpers
    {
        private const decimal NanoFactor = 1_000_000;
        public static decimal GetDecimalQuotation(Quotation quotation)
        {
            return quotation.Units + quotation.Nano / NanoFactor;
        }
    }
}
