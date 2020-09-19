using Microsoft.AspNetCore.Mvc;

namespace ClaytonsWeb2
{

    public class AdminController : Controller {

        [HttpGet("/admin/allcategories")]
        public IActionResult AllCategories()
        {
            return View("Views/Admin/Categories.cshtml");
        }
    }
}