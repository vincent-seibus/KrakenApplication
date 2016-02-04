using KrakenService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace TradeWebApp.Controllers
{
    public class PlayerStateController : ApiController
    {
        // GET: api/PlayerState
        [Route("api/PlayerState")]     
        public IEnumerable<string> Get()
        {
            List<string> list = new List<string>();
            foreach (string name in PlayerState.GetNames(typeof(PlayerState)))
            {
                list.Add(name);
            }

            return list;
        }

        // GET: api/PlayerState/5
        public object Get(int id)
        {
            return krakenManagement.ChangePlayerState(id);             
        }

        // POST: api/PlayerState
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/PlayerState/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/PlayerState/5
        public void Delete(int id)
        {
        }
    }
}
