using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace IWantApp.Endpoints.Employees;
public class EmployeePost
{
    public static string Template => "/employees";
    public static string[] Methods => new string[] { HttpMethod.Post.ToString() };
    public static Delegate Handle => Action;

    public static IResult Action(
        EmployeeRequest employeeRequest,
        HttpContext http,
        UserManager<IdentityUser> userManager)
    {
        var userId = http.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
        var newUser = new IdentityUser { UserName = employeeRequest.Email, Email = employeeRequest.Email };
        var result = userManager.CreateAsync(newUser, employeeRequest.Password).Result;

        if (!result.Succeeded)
            return Results.ValidationProblem(result.Errors.ConvertToProblemDetails());

        var userClaims = new List<Claim>
        {
            new Claim("EmployeeCode", employeeRequest.EmployeeCode),
            new Claim("Name", employeeRequest.Name),
            new Claim("CreatedBy", userId)

        };

        var claimResult = userManager.AddClaimsAsync(newUser, userClaims).Result;

        if (!claimResult.Succeeded)
            return Results.BadRequest(result.Errors.First());

        return Results.Created($"/employees/{newUser.Id}", newUser.Id);
    }
}