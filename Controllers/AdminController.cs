using Microsoft.AspNetCore.Mvc;

namespace ClaytonsWeb2
{

    public class AdminController : Controller {

        [HttpGet("/admin/allcategories")]
        public IActionResult AllCategories()
        {
            return View("Views/Admin/Categories.cshtml");
        }

        [HttpGet("/admin/presearchlist/{category_id}")]
        public IActionResult AllPresearchList(int category_id)
        {
            var model = new PreSearchListModel() {CategoryId = category_id};
            return View("Views/Admin/PresearchListView.cshtml", model);
        }

        [HttpGet("/admin/presearchmaster/{category_id}")]
        public FileResult AllPresearchMaster(int category_id)
        {
            var model = new PreSearchListModel() {CategoryId = category_id};
            return View("Views/Admin/PresearchListMaster.cshtml", model).ToString();
        }

    }
}