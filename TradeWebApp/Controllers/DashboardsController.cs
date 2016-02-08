using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TradeWebApp.Models;
using System.Web;
using System.Web.Caching;
using System.Threading.Tasks;

namespace TradeWebApp.Controllers
{
    public class DashboardsController : ApiController
    {
        // GET: api/Dashboards
        [Route("api/Dashboards")]     
        public Dashboard Get()
        {
            Dashboard dashboard = new Dashboard();
            Cache cache = new Cache();
            dashboard = (Dashboard)HttpRuntime.Cache.Get("Dashboard");
            return dashboard;
        }

        // GET: api/Dashboards/5
        [Route("api/Dashboards/Start")]
        [AcceptVerbs("GET")]
        public object Start()
        {
            krakenManagement.Start();
            return new { error = "", message = "Player has been started"};       
        }

        // GET: api/Dashboards/5
        [Route("api/Dashboards/Pause")]
        [AcceptVerbs("GET")]
        public object Pause()
        {
            krakenManagement.Pause();
            return new { error = "", message = "Player has been paused" };         
        }

        // GET: api/Dashboards/5
        [Route("api/Dashboards/Stop")]
        [AcceptVerbs("GET")]
        public object Stop()
        {
            krakenManagement.Stop();            
            return new { error = "", message = "Player has been stopped" };
        }

        // GET: api/Dashboards/5
        [Route("api/Dashboards/Init")]
        [AcceptVerbs("GET")]
        public object Init()
        {
            Task.Run(() => krakenManagement.Initialize());
            return new { error = "", message = "Initialization in progress..." };
        }

        // GET: api/Dashboards/5
        [Route("api/Dashboards/InitTime")]
        [AcceptVerbs("GET")]
        public object InitTime()
        {
            int i = krakenManagement.InitializeTime;
            if (i < 40)
                return new { error  = "", message = "Initialization in progress...", time = i };
            else
              return new { error = "", message = "Initialization finished", time = i };
        }



        // POST: api/Dashboards
        public void Post([FromBody]string value)
        {

        }

        // PUT: api/Dashboards/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Dashboards/5
        public void Delete(int id)
        {
        }
    }
}
