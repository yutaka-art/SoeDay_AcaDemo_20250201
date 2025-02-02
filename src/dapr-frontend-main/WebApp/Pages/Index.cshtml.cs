using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public string AppVersion { get; set; } = Environment.GetEnvironmentVariable("APP_VERSION") ?? "unknown";

        private readonly ILogger<IndexModel> Logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            this.Logger = logger;
        }

        public void OnGet()
        {

        }
    }
}