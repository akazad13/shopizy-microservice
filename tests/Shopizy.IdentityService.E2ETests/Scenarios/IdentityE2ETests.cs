using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Shopizy.IdentityService.Application.Contracts;
using Shopizy.IdentityService.Domain.Enums;
using Shopizy.IdentityService.E2ETests.Fixtures;

namespace Shopizy.IdentityService.E2ETests.Scenarios;

public sealed class IdentityE2ETests : IClassFixture<IdentityWebApplicationFactory>
{
    private readonly HttpClient _client;

    public IdentityE2ETests(IdentityWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task E2E_Scenario01_UserRegistration_Login_And_ProfileRetrieval()
    {
        // Step 1: Register new customer with strong 12+ character password
        var registerRequest = new RegisterRequest(
            Email: "e2e.customer1@shopizy.test",
            Password: "SuperSecretPassword123!",
            FirstName: "E2E",
            LastName: "Customer",
            Role: UserRole.Customer);

        var regResponse = await _client.PostAsJsonAsync("/api/v1/identity/register", registerRequest);
        regResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var regData = await regResponse.Content.ReadFromJsonAsync<AuthResponse>();
        regData.Should().NotBeNull();
        regData!.AccessToken.Should().NotBeNullOrWhiteSpace();
        regData.RefreshToken.Should().NotBeNullOrWhiteSpace();
        regData.User.Email.Should().Be("e2e.customer1@shopizy.test");
        regData.User.Role.Should().Be("Customer");

        // Step 2: Log in with the registered credentials
        var loginRequest = new LoginRequest(
            Email: "e2e.customer1@shopizy.test",
            Password: "SuperSecretPassword123!");

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/identity/login", loginRequest);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginData = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        loginData.Should().NotBeNull();
        loginData!.AccessToken.Should().NotBeNullOrWhiteSpace();

        // Step 3: Fetch current user profile with Bearer token
        using var profileRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/identity/me");
        profileRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginData.AccessToken);

        var profileResponse = await _client.SendAsync(profileRequest);
        profileResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var profile = await profileResponse.Content.ReadFromJsonAsync<UserResponse>();
        profile.Should().NotBeNull();
        profile!.Email.Should().Be("e2e.customer1@shopizy.test");
        profile.FirstName.Should().Be("E2E");
        profile.LastName.Should().Be("Customer");
        profile.Role.Should().Be("Customer");
    }

    [Fact]
    public async Task E2E_Scenario02_RoleBasedAccessControl_CustomerForbidden_AdminAllowed()
    {
        // Step 1: Register and login as regular Customer
        var customerRegister = new RegisterRequest(
            Email: "customer.rbac@shopizy.test",
            Password: "SuperSecretPassword123!",
            FirstName: "Normal",
            LastName: "Buyer",
            Role: UserRole.Customer);

        var customerRes = await _client.PostAsJsonAsync("/api/v1/identity/register", customerRegister);
        customerRes.StatusCode.Should().Be(HttpStatusCode.Created);
        var customerData = await customerRes.Content.ReadFromJsonAsync<AuthResponse>();

        // Attempt to access user directory with Customer token -> Must be 403 Forbidden
        using var forbiddenRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/identity/users");
        forbiddenRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", customerData!.AccessToken);

        var forbiddenResponse = await _client.SendAsync(forbiddenRequest);
        forbiddenResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // Step 2: Register and login as StoreAdmin
        var adminRegister = new RegisterRequest(
            Email: "admin.rbac@shopizy.test",
            Password: "AdminPassword2026!#Strong",
            FirstName: "Store",
            LastName: "Admin",
            Role: UserRole.StoreAdmin);

        var adminRes = await _client.PostAsJsonAsync("/api/v1/identity/register", adminRegister);
        adminRes.StatusCode.Should().Be(HttpStatusCode.Created);
        var adminData = await adminRes.Content.ReadFromJsonAsync<AuthResponse>();

        // Access user directory with StoreAdmin token -> Must be 200 OK
        using var adminRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/identity/users");
        adminRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminData!.AccessToken);

        var adminResponse = await _client.SendAsync(adminRequest);
        adminResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var usersList = await adminResponse.Content.ReadFromJsonAsync<List<UserResponse>>();
        usersList.Should().NotBeNull();
        usersList!.Should().Contain(u => u.Email == "admin.rbac@shopizy.test");
    }

    [Fact]
    public async Task E2E_Scenario03_TokenRefreshLifecycle()
    {
        // Step 1: Register and obtain refresh token
        var register = new RegisterRequest(
            Email: "refresh.flow@shopizy.test",
            Password: "SuperSecretPassword123!",
            FirstName: "Refresh",
            LastName: "User");

        var regResponse = await _client.PostAsJsonAsync("/api/v1/identity/register", register);
        regResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var initialAuth = await regResponse.Content.ReadFromJsonAsync<AuthResponse>();

        // Step 2: Exchange refresh token for fresh access token
        var refreshRequest = new RefreshTokenRequest(initialAuth!.RefreshToken);
        var refreshResponse = await _client.PostAsJsonAsync("/api/v1/identity/refresh", refreshRequest);
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var refreshedAuth = await refreshResponse.Content.ReadFromJsonAsync<AuthResponse>();
        refreshedAuth.Should().NotBeNull();
        refreshedAuth!.AccessToken.Should().NotBeNullOrWhiteSpace();
        refreshedAuth.RefreshToken.Should().NotBeNullOrWhiteSpace();
        refreshedAuth.RefreshToken.Should().NotBe(initialAuth.RefreshToken); // Rotated token

        // Step 3: Attempting to use the old/revoked refresh token fails with 401
        var replayResponse = await _client.PostAsJsonAsync("/api/v1/identity/refresh", refreshRequest);
        replayResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task E2E_Scenario04_FaultInjection_And_ProblemDetails()
    {
        // Step 1: Weak password rejected with 400 Bad Request
        var weakPassRequest = new RegisterRequest(
            Email: "weakpass@shopizy.test",
            Password: "short", // Only 5 characters, violates 12-char policy
            FirstName: "Weak",
            LastName: "Pass");

        var weakResponse = await _client.PostAsJsonAsync("/api/v1/identity/register", weakPassRequest);
        weakResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problem = await weakResponse.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Title.Should().Be("Password.TooShort");

        // Step 2: Duplicate email registration returns 409 Conflict
        var validRequest = new RegisterRequest(
            Email: "duplicate@shopizy.test",
            Password: "SuperSecretPassword123!",
            FirstName: "Original",
            LastName: "User");

        var firstCreate = await _client.PostAsJsonAsync("/api/v1/identity/register", validRequest);
        firstCreate.StatusCode.Should().Be(HttpStatusCode.Created);

        var duplicateCreate = await _client.PostAsJsonAsync("/api/v1/identity/register", validRequest);
        duplicateCreate.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var conflictProblem = await duplicateCreate.Content.ReadFromJsonAsync<ProblemDetails>();
        conflictProblem.Should().NotBeNull();
        conflictProblem!.Title.Should().Be("User.AlreadyExists");

        // Step 3: Accessing /me without Bearer token returns 401 Unauthorized
        var unauthResponse = await _client.GetAsync("/api/v1/identity/me");
        unauthResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task E2E_Scenario05_CustomerDataIsolation_CrossCustomerAccessForbidden()
    {
        // Step 1: Register Customer A
        var customerAReq = new RegisterRequest(
            Email: "isolation.user.a@shopizy.test",
            Password: "Password123!AStrong",
            FirstName: "User",
            LastName: "A");
        var resA = await _client.PostAsJsonAsync("/api/v1/identity/register", customerAReq);
        resA.StatusCode.Should().Be(HttpStatusCode.Created);
        var authA = await resA.Content.ReadFromJsonAsync<AuthResponse>();

        // Step 2: Register Customer B
        var customerBReq = new RegisterRequest(
            Email: "isolation.user.b@shopizy.test",
            Password: "Password123!BStrong",
            FirstName: "User",
            LastName: "B");
        var resB = await _client.PostAsJsonAsync("/api/v1/identity/register", customerBReq);
        resB.StatusCode.Should().Be(HttpStatusCode.Created);
        var authB = await resB.Content.ReadFromJsonAsync<AuthResponse>();

        // Step 3: Customer A attempts to inspect Customer B's profile -> Must return 403 Forbidden
        using var crossUserRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/identity/users/{authB!.User.Id}");
        crossUserRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authA!.AccessToken);

        var crossUserResponse = await _client.SendAsync(crossUserRequest);
        crossUserResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var problem = await crossUserResponse.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Title.Should().Be("User.Forbidden");

        // Step 4: Admin registers and accesses Customer B's profile -> Must return 200 OK
        var adminReq = new RegisterRequest(
            Email: "isolation.admin@shopizy.test",
            Password: "AdminPassword123!Strong",
            FirstName: "Admin",
            LastName: "User",
            Role: UserRole.StoreAdmin);
        var adminRes = await _client.PostAsJsonAsync("/api/v1/identity/register", adminReq);
        adminRes.StatusCode.Should().Be(HttpStatusCode.Created);
        var adminAuth = await adminRes.Content.ReadFromJsonAsync<AuthResponse>();

        using var adminViewRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/identity/users/{authB.User.Id}");
        adminViewRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminAuth!.AccessToken);

        var adminViewResponse = await _client.SendAsync(adminViewRequest);
        adminViewResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var userBProfile = await adminViewResponse.Content.ReadFromJsonAsync<UserResponse>();
        userBProfile.Should().NotBeNull();
        userBProfile!.Email.Should().Be("isolation.user.b@shopizy.test");
    }

    [Fact]
    public async Task E2E_Scenario06_IdempotencyHeader_PreventsDuplicateRegistration()
    {
        // Step 1: Prepare registration with an Idempotency-Key header
        var idempotencyKey = Guid.NewGuid().ToString();
        var requestPayload = new RegisterRequest(
            Email: "idempotent.user@shopizy.test",
            Password: "IdempotentPassword123!",
            FirstName: "Idempotent",
            LastName: "Tester");

        using var firstMsg = new HttpRequestMessage(HttpMethod.Post, "/api/v1/identity/register")
        {
            Content = JsonContent.Create(requestPayload)
        };
        firstMsg.Headers.Add("Idempotency-Key", idempotencyKey);

        var firstResponse = await _client.SendAsync(firstMsg);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var firstAuth = await firstResponse.Content.ReadFromJsonAsync<AuthResponse>();
        firstAuth.Should().NotBeNull();

        // Step 2: Resend identical request with same Idempotency-Key
        using var secondMsg = new HttpRequestMessage(HttpMethod.Post, "/api/v1/identity/register")
        {
            Content = JsonContent.Create(requestPayload)
        };
        secondMsg.Headers.Add("Idempotency-Key", idempotencyKey);

        var secondResponse = await _client.SendAsync(secondMsg);

        // Assert: Cached response returned, status 201 Created (NOT 409 Conflict!), header X-Cache-Lookup: HIT
        secondResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        secondResponse.Headers.Should().Contain(h => h.Key == "X-Cache-Lookup" && h.Value.Contains("HIT"));

        var secondAuth = await secondResponse.Content.ReadFromJsonAsync<AuthResponse>();
        secondAuth.Should().NotBeNull();
        secondAuth!.User.Id.Should().Be(firstAuth!.User.Id);
    }
}
