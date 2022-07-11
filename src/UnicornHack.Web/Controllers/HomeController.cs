using Microsoft.AspNetCore.Mvc;
using UnicornHack.Models;

namespace UnicornHack.Controllers;

public class HomeController : Controller
{
    [Route("")]
    public IActionResult Index() => View(new CharacterModel());

    //
    // GET: /Game?Name
    [HttpGet("Game")]
    public IActionResult Game(CharacterModel model)
    {
        if (model.Name == null)
        {
            return RedirectToAction(nameof(Index));
        }

        // TODO: Validate name and that the character is not dead.
        return View(nameof(Game), model.Name);
    }

    [Route("Error")]
    public IActionResult Error()
    {
        ViewData["RequestId"] = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        return View();
    }
}
