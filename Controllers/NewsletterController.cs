using Microsoft.AspNetCore.Mvc;

namespace ClaytonsWeb2
{

public class NewsletterController : Controller {

//    [HttpGet("newsletters/{filename}")]
//    public IActionResult GetDocument(string filename) {      
//        return PhysicalFile($"/home/kg0/ClaytonsFiles/seqsk/Newsletters/{filename}", "application/pdf");
//    }

    // https://jakeydocs.readthedocs.io/en/latest/mvc/views/overview.html

    [HttpGet("/islands")]
    public IActionResult Islands()
    {
        var pd = new PreDefined();
        pd.CategoryId = 1;
        pd.CategoryLabel = "Islands";
        return View("Views/Newsletter/PreDefined.cshtml", pd);
    }

    [HttpGet("/bays")]
    public IActionResult Bays()
    {
        var pd = new PreDefined();
        pd.CategoryId = 2;
        pd.CategoryLabel = "Bays";
        return View("Views/Newsletter/PreDefined.cshtml", pd);
    }

    [HttpGet("/rivers")]
    public IActionResult Rivers()
    {
        var pd = new PreDefined();
        pd.CategoryId = 3;
        pd.CategoryLabel = "Rivers";
        return View("Views/Newsletter/PreDefined.cshtml", pd);
    }

    [HttpGet("/animals")]
    public IActionResult Animals()
    {
        var pd = new PreDefined();
        pd.CategoryId = 4;
        pd.CategoryLabel = "Animals";
        return View("Views/Newsletter/PreDefined.cshtml", pd);
    }

    [HttpGet("/towns")]
    public IActionResult Towns()
    {
        var pd = new PreDefined();
        pd.CategoryId = 5;
        pd.CategoryLabel = "Towns";
        return View("Views/Newsletter/PreDefined.cshtml", pd);
    }

    [HttpGet("/international")]
    public IActionResult International()
    {
        var pd = new PreDefined();
        pd.CategoryId = 6;
        pd.CategoryLabel = "International locations";
        return View("Views/Newsletter/PreDefined.cshtml", pd);
    }

    [HttpGet("/beaches")]
    public IActionResult Beaches()
    {
        var pd = new PreDefined();
        pd.CategoryId = 7;
        pd.CategoryLabel = "Beaches";
        return View("Views/Newsletter/PreDefined.cshtml", pd);
    }

    [HttpGet("/lakes")]
    public IActionResult Lakes()
    {
        var pd = new PreDefined();
        pd.CategoryId = 8;
        pd.CategoryLabel = "Lakes";
        return View("Views/Newsletter/PreDefined.cshtml", pd);
    }

    [HttpGet("/other")]
    public IActionResult Other()
    {
        var pd = new PreDefined();
        pd.CategoryId = 9;
        pd.CategoryLabel = "Other locations";
        return View("Views/Newsletter/PreDefined.cshtml", pd);
    }

    [HttpGet("/categorysearch/{category_id}/{search_id}")]
    public IActionResult CategorySearch(int category_id, int search_id)
    {
        var cs = new CategorySearch();
        cs.CategoryId = category_id;
        cs.SearchId = search_id;
        return View("Views/Newsletter/CategorySearchResult.cshtml", cs);
    }

//    [HttpGet("newsletters/search")]
//    public IActionResult Search()
//    {
//        var ws = new WordSearch();
//        ws.SearchTerm = "";
//        return View("Views/Newsletter/WordSearchResult.cshtml", ws);
//    }

    [HttpGet("/search")]
    public IActionResult Search(string search_term)
    {
        var ws = new WordSearch();
        ws.SearchTerm = search_term != null ? search_term : "";
        return View("Views/Newsletter/WordSearchResult.cshtml", ws);
    }
}
}