using BMIWebUI.Models;
using CommonWebUtils;
using System.Web.Mvc;

namespace BMIWebUI.Controllers {
    public class SignInPlaceController : Controller
    {
        // GET: SignInPlace
        public ActionResult Index(string @event, string envelopeId)
        {
            ViewBag.Event = @event;
            ViewBag.EnvelopeId = envelopeId;
            return View();
        }
    }
}