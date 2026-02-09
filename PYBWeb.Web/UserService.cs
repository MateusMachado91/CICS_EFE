using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using PYBWeb.Domain.Interfaces;

public class UserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    public string UserName
{
    get
    {
        string? userName = null;

        var ctx = _httpContextAccessor.HttpContext;
        if (ctx == null)
        {
            Console.WriteLine("[DEBUG] HttpContext é NULL - Tentando Windows Identity");
            
            // ✅ FALLBACK: Tentar obter do Windows Identity
            try
            {
                var windowsIdentity = System.Security.Principal.WindowsIdentity.GetCurrent();
                if (windowsIdentity != null)
                {
                    userName = windowsIdentity.Name;
                    Console.WriteLine($"[DEBUG] Usuário Windows: {userName}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Erro ao obter Windows Identity: {ex.Message}");
            }

            return RemoveDomainPrefix(userName ?? "SISTEMA");
        }

        Console.WriteLine($"[DEBUG] HttpContext.User.Identity.IsAuthenticated: {ctx.User?.Identity?.IsAuthenticated}");
        Console.WriteLine($"[DEBUG] HttpContext.User.Identity.Name: '{ctx.User?.Identity?.Name}'");

        if (ctx.User?.Identity?.IsAuthenticated == true)
        {
            userName = ctx.User.Identity?.Name;
            Console.WriteLine($"[DEBUG] Identity.Name: '{userName}'");

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
            Console.WriteLine("[DEBUG] Usuário NÃO autenticado.");
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

