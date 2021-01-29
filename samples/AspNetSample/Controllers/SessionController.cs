using Microsoft.AspNetCore.Mvc;
using SuperSocket;
using System.Linq;
using System.Text;

namespace AspNetSample.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SessionController : ControllerBase
    {
        readonly ISessionContainer sessionContainer;
        public SessionController(ISessionContainer sessionContainer)
        {
            this.sessionContainer = sessionContainer;
        }
        [HttpGet]
        public IActionResult IndexAsync()
        {
            return Ok(sessionContainer.GetSessions());
        }
        public IActionResult BroadcastMessage(string message)
        {
            try
            {
                sessionContainer.GetSessions().ToList().ForEach(async x => await x.SendAsync(Encoding.UTF8.GetBytes(message)));
            }
            catch
            {
                return new StatusCodeResult(500);
            }
            return Ok();
        }
    }
}
