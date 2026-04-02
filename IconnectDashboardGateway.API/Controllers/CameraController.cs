using Microsoft.AspNetCore.Mvc;

namespace IconnectDashboardGateway.API.Controllers
{
    public class CameraController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
