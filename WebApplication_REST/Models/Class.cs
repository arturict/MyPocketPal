using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

public class SessionMiddleware
{
    private readonly RequestDelegate _next;

    public SessionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        if (!context.Request.Cookies.ContainsKey("SessionId"))
        {
            // Erstellen Sie eine neue Session-ID und speichern Sie sie im Cookie
            var sessionId = Guid.NewGuid().ToString();
            context.Response.Cookies.Append("SessionId", sessionId);
        }

        await _next(context);
    }
}
