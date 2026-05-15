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

    public IActionResult ForgotPassword() => View();

    [HttpPost]
    public async Task<IActionResult> ForgotPassword(string Email)
    {
        try
        {
            await _supabase.InitializeAsync();
            
            // Define a URL de redirecionamento para a página de ResetPassword
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var resetUrl = $"{baseUrl}/Auth/ResetPassword";
            
            var options = new Supabase.Gotrue.ResetPasswordForEmailOptions(Email) { RedirectTo = resetUrl };
            await _supabase.Auth.ResetPasswordForEmail(options);
            
            ViewBag.SuccessMessage = "Se o e-mail estiver cadastrado, você receberá um link para redefinir sua senha.";
            return View();
        }
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = "Erro ao processar solicitação: " + ex.Message;
            return View();
        }
    }

    public IActionResult ResetPassword() => View();

    [HttpPost]
    public async Task<IActionResult> ResetPassword(string Password, string AccessToken)
    {
        try
        {
            await _supabase.InitializeAsync();
            
            // Define a sessão usando o token recebido do link de recuperação. 
            // O parâmetro refreshToken pode ser vazio para links de recuperação.
            await _supabase.Auth.SetSession(AccessToken, ""); 

            var attrs = new Supabase.Gotrue.UserAttributes { Password = Password };
            await _supabase.Auth.Update(attrs);

            // Opcional: fazer logout após trocar a senha para forçar novo login
            await _supabase.Auth.SignOut();

            ViewBag.SuccessMessage = "Senha alterada com sucesso! Você já pode fazer login.";
            return View("Index");
        }
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = "Erro ao alterar senha: " + ex.Message;
            return View();
        }
    }

    public async Task<IActionResult> Logout()
    {
        try { await _supabase.Auth.SignOut(); } catch { }
        return RedirectToAction("Index", "Explorar");
    }
}
