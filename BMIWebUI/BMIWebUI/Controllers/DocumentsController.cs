using System.Web.Mvc;

namespace BMIWebUI.Controllers
{
    public class DocumentsController : Controller
    {
        // GET: Documents
        public ActionResult Index()
        {
            ViewBag.CurrentUserEmail = "angad.dhawan@gmail.com";
            return View();
        }
    }
}