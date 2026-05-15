using Microsoft.AspNetCore.Mvc;
using Supabase; // Certifique-se de usar o namespace do SDK
using VotoTrack.Models;

namespace VotoTrack.Controllers
{
    public class ExplorarController : Controller
    {
        private readonly Supabase.Client _supabase;

        // O .NET injeta o client aqui automaticamente
        public ExplorarController(Supabase.Client supabase)
        {
            _supabase = supabase;
        }

        public IActionResult Index()
        {
            // Se usuário já está logado, redireciona direto para o Dashboard
            var session = _supabase.Auth.CurrentSession;
            if (session != null)
            {
                return RedirectToAction("Publico", "Dashboard");
            }

            return View();
        }

        public IActionResult FAQ()
        {
            return View();
        }
    }
}