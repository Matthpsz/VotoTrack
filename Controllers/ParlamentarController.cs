using Microsoft.AspNetCore.Mvc;
using VotoTrack.Data;
using VotoTrack.Models;

namespace VotoTrack.Controllers
{
    public class ParlamentarController : Controller
    {
        [Route("Parlamentar/Detalhes/{id}")]
        public async Task<IActionResult> Detalhes(int id)
        {
            // Aqui você buscaria os dados da API da Câmara e cruzaria com o Supabase
            // Simulando busca de favoritos do usuário logado
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Favoritar(int deputadoId)
        {
            var fav = new UserFavorite { DeputadoId = deputadoId, UserId = "id-do-usuario-logado" };
            await SupabaseClientProvider.Client.From<UserFavorite>().Insert(fav);
            return Ok();
        }
    }
}