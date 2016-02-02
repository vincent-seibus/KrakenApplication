using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using TradeWebApp.Models;
using System.Net.Http;

namespace TradeWebApp.Controllers
{
    public class DashboardController : Controller
    {

        public string GetDasboard()
        {
            Dashboard dashboard = new Dashboard();
          
            return Json(dashboard).ToString();
        }
    }
}
