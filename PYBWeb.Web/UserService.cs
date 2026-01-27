using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Components.Authorization;
using PYBWeb.Domain.Interfaces;

public class UserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AuthenticationStateProvider? _authenticationStateProvider;

    public UserService(IHttpContextAccessor httpContextAccessor, AuthenticationStateProvider? authenticationStateProvider = null)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _authenticationStateProvider = authenticationStateProvider;
    }

    public string UserName
    {
        get
        {
            string? userName = null;

            var ctx = _httpContextAccessor.HttpContext;

            if (ctx != null)
            {
                Console.WriteLine($"[DEBUG] HttpContext.User.Identity.IsAuthenticated: {ctx.User?.Identity?.IsAuthenticated}");
                Console.WriteLine($"[DEBUG] HttpContext.User.Identity.Name: '{ctx.User?.Identity?.Name}'");

                if (ctx.User?.Identity?.IsAuthenticated == true)
                {
                    // Primeiro tenta Identity.Name (padrão)
                    userName = ctx.User.Identity?.Name;
                    Console.WriteLine($"[DEBUG] Identity.Name: '{userName}'");

                    // Se Identity.Name estiver vazio, tenta claims comuns
                    if (string.IsNullOrEmpty(userName))
                    {
                        var upn = ctx.User.FindFirst(ClaimTypes.Upn)?.Value;
                        var preferred = ctx.User.FindFirst("preferred_username")?.Value;
                        var claimName = ctx.User.FindFirst(ClaimTypes.Name)?.Value;

                        Console.WriteLine($"[DEBUG] ClaimTypes.Upn: '{upn}'");
                        Console.WriteLine($"[DEBUG] preferred_username: '{preferred}'");
                        Console.WriteLine($"[DEBUG] ClaimTypes.Name: '{claimName}'");

                        userName = upn ?? preferred ?? claimName;
                    }
                }
                else
                {
                    Console.WriteLine("[DEBUG] Usuário NÃO autenticado no HttpContext.");
                }
            }
            else
            {
                Console.WriteLine("[DEBUG] HttpContext é NULL. Tentando AuthenticationStateProvider...");

                try
                {
                    if (_authenticationStateProvider != null)
                    {
                        var authState = _authenticationStateProvider.GetAuthenticationStateAsync().GetAwaiter().GetResult();
                        var user = authState?.User;
                        Console.WriteLine($"[DEBUG] AuthenticationState.User.Identity.IsAuthenticated: {user?.Identity?.IsAuthenticated}");
                        Console.WriteLine($"[DEBUG] AuthenticationState.User.Identity.Name: '{user?.Identity?.Name}'");

                        if (user?.Identity?.IsAuthenticated == true)
                        {
                            userName = user.Identity?.Name;
                            if (string.IsNullOrEmpty(userName))
                            {
                                var upn = user.FindFirst(ClaimTypes.Upn)?.Value;
                                var preferred = user.FindFirst("preferred_username")?.Value;
                                var claimName = user.FindFirst(ClaimTypes.Name)?.Value;
                                userName = upn ?? preferred ?? claimName;
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("[DEBUG] AuthenticationStateProvider não disponível.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[DEBUG] Erro ao obter AuthenticationState: {ex.Message}");
                }
            }

            // Último fallback: Thread principal (Circuito) / ClaimsPrincipal.Current
            if (string.IsNullOrEmpty(userName))
            {
                try
                {
                    var current = System.Threading.Thread.CurrentPrincipal as ClaimsPrincipal;
                    if (current?.Identity?.IsAuthenticated == true)
                    {
                        userName = current.Identity?.Name;
                    }
                }
                catch { }
            }

            Console.WriteLine($"[DEBUG] Valor final de userName: '{userName}'");
            return userName ?? string.Empty;
        }
        set
        {
            // setter sem efeito
        }
    }

    // Remove domínio se existir (ex: "CORP\usuario" -> "usuario")
    public string UserNameSemDominio => RemoveDomainPrefix(UserName);

    public string GetCurrentUser() => UserName;

    private static string RemoveDomainPrefix(string name)
    {
        if (string.IsNullOrEmpty(name)) return string.Empty;

        // trata formatos "DOMINIO\usuario" e "usuario@dominio"
        var idx = name.IndexOf('\\');
        if (idx >= 0 && idx < name.Length - 1)
            return name.Substring(idx + 1);

        // se for UPN, retorna antes do @
        var atIdx = name.IndexOf('@');
        if (atIdx > 0)
            return name.Substring(0, atIdx);

        return name;
    }
}