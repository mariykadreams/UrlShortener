
using FakeItEasy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NuGet.Common;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using UrlShortener.API.Controllers;
using UrlShortener.API.Models;
using Xunit;

public class AccountControllerTests
{
    private readonly UserManager<IdentityUser> _fakeUserManager;
    private readonly RoleManager<IdentityRole> _fakeRoleManager;
    private readonly IConfiguration _fakeConfiguration;
    private readonly AccountController _controller;

    public AccountControllerTests()
    {
        // Create fakes for the dependencies
        _fakeUserManager = A.Fake<UserManager<IdentityUser>>(options => options.WithArgumentsForConstructor(
            new object[] { A.Fake<IUserStore<IdentityUser>>(), null, null, null, null, null, null, null, null }
        ));

        _fakeRoleManager = A.Fake<RoleManager<IdentityRole>>(options => options.WithArgumentsForConstructor(
            new object[] { A.Fake<IRoleStore<IdentityRole>>(), null, null, null, null }
        ));

        _fakeConfiguration = A.Fake<IConfiguration>();

        // Set up IConfiguration values for JWT
        A.CallTo(() => _fakeConfiguration["Jwt:Issuer"]).Returns("test_issuer");
        A.CallTo(() => _fakeConfiguration["Jwt:ExpiryMinutes"]).Returns("60");
        A.CallTo(() => _fakeConfiguration["Jwt:Key"]).Returns(Convert.ToBase64String(Encoding.UTF8.GetBytes("thisisverylongsecretkeyforjwttestingpurposes"))); // Must be long enough

        // Create the controller with the fakes
        _controller = new AccountController(_fakeUserManager, _fakeRoleManager, _fakeConfiguration);
    }

    // --- Register Tests ---
    [Fact]
    public async Task Register_ReturnsOk_WhenRegistrationSucceedsAndRoleIsCreatedAndAssigned()
    {
        // Arrange
        var model = new Register { Username = "testuser", Email = "test@example.com", Password = "Password123!" };
        A.CallTo(() => _fakeUserManager.CreateAsync(A<IdentityUser>._, model.Password))
            .Returns(IdentityResult.Success);
        A.CallTo(() => _fakeRoleManager.RoleExistsAsync("User")).Returns(false);
        A.CallTo(() => _fakeRoleManager.CreateAsync(A<IdentityRole>._))
            .Returns(IdentityResult.Success);
        A.CallTo(() => _fakeUserManager.AddToRoleAsync(A<IdentityUser>._, "User"))
            .Returns(IdentityResult.Success);

        // Act
        var result = await _controller.Register(model);

        // Assert
        A.CallTo(() => _fakeUserManager.CreateAsync(A<IdentityUser>.That.Matches(u => u.UserName == model.Username && u.Email == model.Email), model.Password))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeRoleManager.RoleExistsAsync("User")).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeRoleManager.CreateAsync(A<IdentityRole>.That.Matches(r => r.Name == "User"))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeUserManager.AddToRoleAsync(A<IdentityUser>._, "User")).MustHaveHappenedOnceExactly();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        // Сериалізація + парсинг
        string json = JsonSerializer.Serialize(okResult.Value);
        using var doc = JsonDocument.Parse(json);
        string message = doc.RootElement.GetProperty("message").GetString();

        Assert.Equal("User registered successfully with role 'User'", message);

    }

    [Fact]
    public async Task Register_ReturnsOk_WhenRegistrationSucceedsAndRoleAlreadyExists()
    {
        // Arrange
        var model = new Register { Username = "testuser", Email = "test@example.com", Password = "Password123!" };
        A.CallTo(() => _fakeUserManager.CreateAsync(A<IdentityUser>._, model.Password))
            .Returns(IdentityResult.Success);
        A.CallTo(() => _fakeRoleManager.RoleExistsAsync("User")).Returns(true); // Role exists
        A.CallTo(() => _fakeUserManager.AddToRoleAsync(A<IdentityUser>._, "User"))
            .Returns(IdentityResult.Success);

        // Act
        var result = await _controller.Register(model);

        // Assert
        A.CallTo(() => _fakeUserManager.CreateAsync(A<IdentityUser>._, model.Password)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeRoleManager.RoleExistsAsync("User")).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeRoleManager.CreateAsync(A<IdentityRole>._)).MustNotHaveHappened(); // Role creation skipped
        A.CallTo(() => _fakeUserManager.AddToRoleAsync(A<IdentityUser>._, "User")).MustHaveHappenedOnceExactly();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        string json = System.Text.Json.JsonSerializer.Serialize(okResult.Value);
        using var doc = System.Text.Json.JsonDocument.Parse(json);
        string message = doc.RootElement.GetProperty("message").GetString();

        Assert.Equal("User registered successfully with role 'User'", message);

    }

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenRegistrationFails()
    {
        // Arrange
        var model = new Register { Username = "testuser", Email = "test@example.com", Password = "Password123!" };
        var errors = new IdentityError[] { new IdentityError { Description = "Password too short." } };
        A.CallTo(() => _fakeUserManager.CreateAsync(A<IdentityUser>._, model.Password))
            .Returns(IdentityResult.Failed(errors));

        // Act
        var result = await _controller.Register(model);

        // Assert
        A.CallTo(() => _fakeUserManager.CreateAsync(A<IdentityUser>._, model.Password)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeRoleManager.RoleExistsAsync(A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _fakeUserManager.AddToRoleAsync(A<IdentityUser>._, A<string>._)).MustNotHaveHappened();

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(errors, badRequestResult.Value);
    }

    // --- Login Tests ---
    [Fact]
    public async Task Login_ReturnsOkWithToken_WhenCredentialsAreValid()
    {
        // Arrange
        var model = new Login { Username = "testuser", Password = "Password123!" };
        var user = new IdentityUser { Id = "user123", UserName = "testuser", Email = "test@example.com" };
        var roles = new List<string> { "User", "Admin" };

        A.CallTo(() => _fakeUserManager.FindByNameAsync(model.Username)).Returns(user);
        A.CallTo(() => _fakeUserManager.CheckPasswordAsync(user, model.Password)).Returns(true);
        A.CallTo(() => _fakeUserManager.GetRolesAsync(user)).Returns(roles);

        // Act
        var result = await _controller.Login(model);

        // Assert
        A.CallTo(() => _fakeUserManager.FindByNameAsync(model.Username)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeUserManager.CheckPasswordAsync(user, model.Password)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeUserManager.GetRolesAsync(user)).MustHaveHappenedOnceExactly();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        // Сериализуем Value в JSON и парсим
        string json = System.Text.Json.JsonSerializer.Serialize(okResult.Value);
        using var doc = System.Text.Json.JsonDocument.Parse(json);

        // Получаем токен из JSON
        string token = doc.RootElement.GetProperty("token").GetString();

        Assert.False(string.IsNullOrEmpty(token));

        // Проверяем JWT токен
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        Assert.True(jwtToken.Claims.Any(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.UserName));
        Assert.True(jwtToken.Claims.Any(c => c.Type == ClaimTypes.NameIdentifier && c.Value == user.Id));
        Assert.True(jwtToken.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "User"));
        Assert.True(jwtToken.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "Admin"));
    }



    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenUserNotFound()
    {
        // Arrange
        var model = new Login { Username = "nonexistentuser", Password = "Password123!" };
        A.CallTo(() => _fakeUserManager.FindByNameAsync(model.Username)).Returns((IdentityUser)null);

        // Act
        var result = await _controller.Login(model);

        // Assert
        A.CallTo(() => _fakeUserManager.FindByNameAsync(model.Username)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeUserManager.CheckPasswordAsync(A<IdentityUser>._, A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _fakeUserManager.GetRolesAsync(A<IdentityUser>._)).MustNotHaveHappened();

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenPasswordIsIncorrect()
    {
        // Arrange
        var model = new Login { Username = "testuser", Password = "WrongPassword!" };
        var user = new IdentityUser { UserName = "testuser" };
        A.CallTo(() => _fakeUserManager.FindByNameAsync(model.Username)).Returns(user);
        A.CallTo(() => _fakeUserManager.CheckPasswordAsync(user, model.Password)).Returns(false); // Incorrect password

        // Act
        var result = await _controller.Login(model);

        // Assert
        A.CallTo(() => _fakeUserManager.FindByNameAsync(model.Username)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeUserManager.CheckPasswordAsync(user, model.Password)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeUserManager.GetRolesAsync(A<IdentityUser>._)).MustNotHaveHappened();

        Assert.IsType<UnauthorizedResult>(result);
    }

    // --- AddRole Tests ---
    [Fact]
    public async Task AddRole_ReturnsOk_WhenRoleDoesNotExistAndIsCreated()
    {
        // Arrange
        string roleName = "Admin";
        A.CallTo(() => _fakeRoleManager.RoleExistsAsync(roleName)).Returns(false);
        A.CallTo(() => _fakeRoleManager.CreateAsync(A<IdentityRole>._))
            .Returns(IdentityResult.Success);

        // Act
        var result = await _controller.AddRole(roleName);

        // Assert
        A.CallTo(() => _fakeRoleManager.RoleExistsAsync(roleName)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeRoleManager.CreateAsync(A<IdentityRole>.That.Matches(r => r.Name == roleName))).MustHaveHappenedOnceExactly();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        // Сериализация и парсинг JSON для безопасного получения свойства "message"
        string json = System.Text.Json.JsonSerializer.Serialize(okResult.Value);
        using var doc = System.Text.Json.JsonDocument.Parse(json);
        string message = doc.RootElement.GetProperty("message").GetString();

        Assert.Equal("Role added successfully", message);
    }


    [Fact]
    public async Task AddRole_ReturnsBadRequest_WhenRoleAlreadyExists()
    {
        // Arrange
        string roleName = "Admin";
        A.CallTo(() => _fakeRoleManager.RoleExistsAsync(roleName)).Returns(true); // Role exists

        // Act
        var result = await _controller.AddRole(roleName);

        // Assert
        A.CallTo(() => _fakeRoleManager.RoleExistsAsync(roleName)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeRoleManager.CreateAsync(A<IdentityRole>._)).MustNotHaveHappened(); // Role creation skipped

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Role already exists", badRequestResult.Value);
    }

    [Fact]
    public async Task AddRole_ReturnsBadRequest_WhenRoleCreationFails()
    {
        // Arrange
        string roleName = "Admin";
        var errors = new IdentityError[] { new IdentityError { Description = "Invalid role name." } };
        A.CallTo(() => _fakeRoleManager.RoleExistsAsync(roleName)).Returns(false);
        A.CallTo(() => _fakeRoleManager.CreateAsync(A<IdentityRole>._))
            .Returns(IdentityResult.Failed(errors));

        // Act
        var result = await _controller.AddRole(roleName);

        // Assert
        A.CallTo(() => _fakeRoleManager.RoleExistsAsync(roleName)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeRoleManager.CreateAsync(A<IdentityRole>.That.Matches(r => r.Name == roleName))).MustHaveHappenedOnceExactly();

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(errors, badRequestResult.Value);
    }

    // --- AssignRole Tests ---
    [Fact]
    public async Task AssignRole_ReturnsOk_WhenUserAndRoleExistAndAssignmentSucceeds()
    {
        // Arrange
        var model = new UserRole { Username = "testuser", Role = "Admin" };
        var user = new IdentityUser { UserName = "testuser" };

        A.CallTo(() => _fakeUserManager.FindByNameAsync(model.Username)).Returns(user);
        A.CallTo(() => _fakeUserManager.AddToRoleAsync(user, model.Role))
            .Returns(IdentityResult.Success);

        // Act
        var result = await _controller.AssignRole(model);

        // Assert
        A.CallTo(() => _fakeUserManager.FindByNameAsync(model.Username)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeUserManager.AddToRoleAsync(user, model.Role)).MustHaveHappenedOnceExactly();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        // Сериализуем Value в JSON и парсим, чтобы получить message
        string json = System.Text.Json.JsonSerializer.Serialize(okResult.Value);
        using var doc = System.Text.Json.JsonDocument.Parse(json);
        string message = doc.RootElement.GetProperty("message").GetString();

        Assert.Equal("Role assigned successfully", message);
    }


    [Fact]
    public async Task AssignRole_ReturnsBadRequest_WhenUserNotFound()
    {
        // Arrange
        var model = new UserRole { Username = "nonexistentuser", Role = "Admin" };
        A.CallTo(() => _fakeUserManager.FindByNameAsync(model.Username)).Returns((IdentityUser)null);

        // Act
        var result = await _controller.AssignRole(model);

        // Assert
        A.CallTo(() => _fakeUserManager.FindByNameAsync(model.Username)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeUserManager.AddToRoleAsync(A<IdentityUser>._, A<string>._)).MustNotHaveHappened();

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("User not found", badRequestResult.Value);
    }

    [Fact]
    public async Task AssignRole_ReturnsBadRequest_WhenAssignmentFails()
    {
        // Arrange
        var model = new UserRole { Username = "testuser", Role = "InvalidRole" };
        var user = new IdentityUser { UserName = "testuser" };
        var errors = new IdentityError[] { new IdentityError { Description = "Role does not exist." } };

        A.CallTo(() => _fakeUserManager.FindByNameAsync(model.Username)).Returns(user);
        A.CallTo(() => _fakeUserManager.AddToRoleAsync(user, model.Role))
            .Returns(IdentityResult.Failed(errors));

        // Act
        var result = await _controller.AssignRole(model);

        // Assert
        A.CallTo(() => _fakeUserManager.FindByNameAsync(model.Username)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeUserManager.AddToRoleAsync(user, model.Role)).MustHaveHappenedOnceExactly();

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(errors, badRequestResult.Value);
    }
}