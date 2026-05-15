using Microsoft.AspNetCore.Mvc;
using Supabase;
using System.Threading.Tasks;
using System;
using VotoTrack.Models;

public class AuthController : Controller
{
    private readonly Client _supabase;

    public AuthController(Client supabase)
    {
        _supabase = supabase;
    }

    public IActionResult Index() => View();

    [HttpPost]
    public async Task<IActionResult> Register(string FullName, string Email, string Password)
    {
        try
        {
            var options = new Supabase.Gotrue.SignUpOptions
            {
                Data = new System.Collections.Generic.Dictionary<string, object>
                {
                    { "full_name", FullName }
                }
            };

            await _supabase.InitializeAsync();

            var session = await _supabase.Auth.SignUp(Email, Password, options);

            if (session == null || session.User == null)
            {
                ViewBag.ErrorMessage = "Cadastro realizado, mas a sessÃ£o nÃ£o foi retornada (talvez exija confirmaÃ§Ã£o de e-mail).";
                return View("Index");
            }

            // Tenta inserir no user_profiles
            var profile = new UserProfile
            {
                Id = session.User.Id,
                NomeExibicao = FullName,
                TemaPreferido = "Geral"
            };
            
            try 
            {
                var result = await _supabase.From<UserProfile>().Insert(profile);
                if (result.Models.Count == 0)
                {
                    ViewBag.ErrorMessage = "UsuÃ¡rio criado na autenticaÃ§Ã£o, mas bloqueado pelo RLS ao salvar no user_profiles. Verifique as polÃ­ticas (RLS) no Supabase.";
                    return View("Index");
                }
            } 
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Erro ao salvar perfil: " + ex.Message;
                return View("Index");
            }

            return RedirectToAction("Publico", "Dashboard");
        }
        catch (Exception ex)
        {
            // Em caso de erro, retorna para a tela com uma mensagem (pode ser ajustado no frontend depois)
            ViewBag.ErrorMessage = "Erro ao realizar cadastro: " + ex.Message;
            return View("Index");
        }
    }

    [HttpPost]
    public async Task<IActionResult> Login(string Email, string Password)
    {
        try
        {
            var session = await _supabase.Auth.SignIn(Email, Password);

            if (session == null || session.User == null)
            {
                ViewBag.ErrorMessage = "Credenciais inválidas ou conta não existe.";
                return View("Index");
            }

            return RedirectToAction("Publico", "Dashboard");
        }
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = "Erro ao realizar login: " + ex.Message;
            return View("Index");
        }
    }

    public async Task<IActionResult> Logout()
    {
        try { await _supabase.Auth.SignOut(); } catch { }
        return RedirectToAction("Index", "Explorar");
    }
}
