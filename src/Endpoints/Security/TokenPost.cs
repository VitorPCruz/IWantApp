namespace IWantApp.Endpoints.Security;

public class TokenPost
{
    public static string Template => "/token";
    public static string[] Methods => new string[] { HttpMethod.Post.ToString() };
    public static Delegate Handle => Action;

    [AllowAnonymous]
    public static IResult Action(
        LoginRequest loginRequest,
        IConfiguration configurations,
        UserManager<IdentityUser> userManager,
        ILogger<TokenPost> log)
    {
        log.LogInformation("Getting Token");
        log.LogWarning("WARNING");

        var user = userManager.FindByEmailAsync(loginRequest.Email).Result;

        if (user == null) return Results.BadRequest();

        if (!userManager.CheckPasswordAsync(user, loginRequest.Password).Result)
            return Results.BadRequest();

        var claims = userManager.GetClaimsAsync(user).Result;

        var key = Encoding.ASCII.GetBytes(configurations["JwtBearerTokenSettings:SecretKey"]);

        var subject = new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.Email, loginRequest.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
        });

        subject.AddClaims(claims);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = subject,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            ),
            Audience = configurations["JwtBearerTokenSettings:Audience"],
            Issuer = configurations["JwtBearerTokenSettings:Issuer"],
            Expires = DateTime.UtcNow.AddHours(1)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return Results.Ok(new { token = tokenHandler.WriteToken(token) });
    }
}