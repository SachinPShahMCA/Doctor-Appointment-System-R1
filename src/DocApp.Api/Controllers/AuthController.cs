using DocApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DocApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthController> _logger;

    public AuthController(ApplicationDbContext db, IConfiguration config, ILogger<AuthController> logger)
    {
        _db = db;
        _config = config;
        _logger = logger;
    }

    /// <summary>
    /// Login and receive a JWT token.
    /// Uses X-Tenant-ID header to scope to the right tenant.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, [FromHeader(Name = "X-Tenant-ID")] string tenantId = "demo")
    {
        // Find user within tenant (bypass global filter as this is auth)
        var user = await _db.AppUsers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower() && u.TenantId == tenantId);

        if (user is null)
        {
            _logger.LogWarning("Login failed: user {Email} not found in tenant {TenantId}", request.Email, tenantId);
            return Unauthorized(new { message = "Invalid email or password." });
        }

        // Verify password via raw lookup
        var hashRecord = await _db.Database
            .SqlQueryRaw<string>($"""SELECT "PasswordHash" FROM "UserPasswords" WHERE "UserId" = '{user.Id}'""")
            .FirstOrDefaultAsync();

        if (hashRecord is null) return Unauthorized(new { message = "Invalid email or password." });

        var hasher = new PasswordHasher<object>();
        var result = hasher.VerifyHashedPassword(new object(), hashRecord, request.Password);
        if (result == PasswordVerificationResult.Failed)
            return Unauthorized(new { message = "Invalid email or password." });

        var token = GenerateJwt(user.Id.ToString(), user.Email, user.Role.ToString(), tenantId);

        return Ok(new LoginResponse(
            Token: token,
            Email: user.Email,
            FullName: user.FullName,
            Role: user.Role.ToString(),
            TenantId: tenantId,
            ExpiresAt: DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpiryMinutes"] ?? "60"))
        ));
    }

    /// <summary>Get current user info from JWT.</summary>
    [HttpGet("me")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public IActionResult Me()
    {
        return Ok(new
        {
            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
            Email = User.FindFirstValue(ClaimTypes.Email),
            Role = User.FindFirstValue(ClaimTypes.Role),
            TenantId = User.FindFirstValue("tenant_id")
        });
    }

    private string GenerateJwt(string userId, string email, string role, string tenantId)
    {
        var jwt = _config.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["SecretKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role),
            new Claim("tenant_id", tenantId),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(jwt["ExpiryMinutes"] ?? "60")),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public sealed record LoginRequest(string Email, string Password);
public sealed record LoginResponse(string Token, string Email, string FullName, string Role, string TenantId, DateTime ExpiresAt);
