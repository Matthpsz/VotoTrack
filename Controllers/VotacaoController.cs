using Microsoft.AspNetCore.Mvc;
using VotoTrack.Data;

namespace VotoTrack.Controllers
{
    public class VotacaoController : Controller
    {
        public async Task<IActionResult> Detalhes(int id)
        {
            // Lógica para carregar o "Mapa de Votos" (os 513 quadradinhos coloridos)
            return View();
        }
    }
}