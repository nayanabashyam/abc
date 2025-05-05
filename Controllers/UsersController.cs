using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public UsersController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private List<User> LoadUsersFromJson()
    {
        var filePath = _configuration["UserDataPath"];

        if (!System.IO.File.Exists(filePath))
        {
            throw new FileNotFoundException("user.json not found at: " + filePath);
        }

        var json = System.IO.File.ReadAllText(filePath);
        var users = JsonSerializer.Deserialize<List<User>>(json);

        return users ?? new List<User>();
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginModel model)
    {
        var users = LoadUsersFromJson();

        var user = users.FirstOrDefault(u => u.Email == model.Email && u.Password == model.Password);

        if (user == null)
            return Unauthorized(new { message = "Invalid email or password." });

        var token = GenerateJwtToken(user);
        return Ok(new { token });
    }

    private string GenerateJwtToken(User user)
    {
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
        var claims = new[] 
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim(JwtRegisteredClaimNames.Email, user.Password),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var creds = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
