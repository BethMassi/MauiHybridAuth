# .NET MAUI Blazor Hybrid and Web App Auth Template
Fork with added dotnet new template for the Microsft MAUI Hybrid with Blazor Identity sample.
See the 'mudblazor' branch for a custom solution template using MudBlazor.

This project demonstrates how to build .NET MAUI Blazor Hybrid and Web Apps that shares common UI and also provides *authentication*. It uses ASP.NET Core Identity local accounts but you can use this pattern for any authentication provider you need to call from a MAUI Blazor Hybrid client.

## Installing and using the template
1. Clone the repository.
1. Make sure you have [.NET 9 installed and the MAUI workload](https://learn.microsoft.com/en-us/dotnet/maui/get-started/installation?view=net-maui-9.0&tabs=vswin).
1. Open a terminal in the root folder and type ```dotnet new install .```
1. Create a new project in your repo directory from the terminal using ```dotnet new maui-auth -n {name of project}```

## Tour of the important parts
### Shared UI 
The shared UI is in the `MauiHybridAuth.Shared` project. This project contains the Razor components that are shared between the MAUI and Blazor Web projects (Home, Counter and Weather pages). The `Counter.razor` and `Weather.razor` pages are protected by the `[Authorize]` attribute so you cannot navigate to them unless you are logged in. 

```code
@page "/counter"
@using Microsoft.AspNetCore.Authorization
@attribute [Authorize]
```
### Routing in the MAUI & Blazor apps
The `Routes.razor` uses the `AuthorizeRouteView` to route users appropriately based on their authentication status. If a user is not authenticated, they are redirected to the `Login` page. 

```code
<AuthorizeRouteView RouteData="@routeData" DefaultLayout="@typeof(Layout.MainLayout)">
    <Authorizing>
        Authorizing...
    </Authorizing>
    <NotAuthorized>
        <Login />
    </NotAuthorized>
</AuthorizeRouteView>  
```

The `NavManu.razor` components contain the navigation menu that uses `AuthorizationView` to show/hide links based on the user's authentication status.

```code
<AuthorizeView>
    <NotAuthorized>
       <!-- Navlinks that display when not logged in -->    
    </NotAuthorized>
    <Authorized>
        <!-- Navlinks that display when logged in -->    
    </Authorized>               
</AuthorizeView>
```
### Setting up the server
The Blazor Web app contains all the pages and uses the `SignInManager` framework class to manage logins and users. All of this is generated automatically when you create a Blazor Web project and select to use Authentication with Individual accounts. In order for the MAUI client (or any external client) to authenticate, the ASP.NET Identity endpoints need to be exposed. In the `Program.cs` file this is set up with the call to `AddIdentityEnpoints` and `MapIdentityApi`. NOTE: You'll need to remove the generated call to `.AddIdentityCookies` on `.AddAuthentication`. It is not necessary when calling `.AddIdentityEndpoints` and will result in an error. 

```code
// Add Auth services used by the Web app
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
});
 
builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

//Needed for external clients to log in
builder.Services.AddIdentityApiEndpoints<ApplicationUser>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

//Register needed elements for authentication:
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, AuthorizationMiddlewareResultHandler>();

var app = builder.Build();

//Needed for external clients to log in
app.MapIdentityApi<ApplicationUser>();
```
### Logging in from the MAUI client
The `Login.razor` page is where the user logs in. The `Login` page injects the `ICustomAuthenticationStateProvider` and uses the `AuthenticationStateProvider` to authenticate the user and redirect them to the home page if successful. When the state changes, the `AuthorizeView` reacts and will show the appropriate pages/links based on the user's authentication status.

```code
//Called on valid submit
private async Task LoginUser()
{
    await AuthStateProvider.LogInAsync(LoginModel);

    if (AuthStateProvider.LoginStatus != LoginStatus.Success)
    {
        //Show error message
        loginFailureHidden = false;
        return;
    }        
        
    Navigation.NavigateTo(""); //Root URL
}
```
NOTE: This sample only implements Login and Logout pages on the MAUI client but you could build Register and other management pages against the exposed endpoints for more functionality. For more information on identity endpoints see [How to use Identity to secure a Web API backend for SPAs](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-api-authorization)

### MauiAuthenticationStateProvider
The `ICustomAuthenticationStateProvider` interface is implemented by the `MauiAuthenticationStateProvider` class in the `MauiHybridAuth` MAUI project. This class is responsible for managing the user's authentication state and providing the `AuthenticationState` to the app. The `MauiAuthenticationStateProvider` class uses the `HttpClient` to make requests to the server to authenticate the user. See the official documentation for more information on [ASP.NET Core Blazor Hybrid authentication and authorization](https://learn.microsoft.com/en-us/aspnet/core/blazor/hybrid/security/?view=aspnetcore-8.0&pivots=maui).

```code
public interface ICustomAuthenticationStateProvider 
{
    public LoginStatus LoginStatus { get; set; }
    Task<AuthenticationState> GetAuthenticationStateAsync();
    Task LogInAsync(LoginModel loginModel);
    void Logout();
}
```
This class also handles calling localhost via the emulators and simulators for easy testing. See the [official documentation](https://learn.microsoft.com/dotnet/maui/data-cloud/local-web-services) for information on what you need to do to be able to call local services from emulators and simulators. This class could also use the `SecureStorage` API to store the user's token securely on the device, or handle any other platform specific functionality if needed.

### MAUI MauiProgram.cs
The MAUI project's `MauiProgram.cs` file is where the `MauiAuthenticationStateProvider` is registered with the DI container. It also needs to register the Authorization core components where things like `AuthorizeView` are defined.

```code
 // This is the core functionality
builder.Services.AddAuthorizationCore();
// This is our custom provider
builder.Services.AddScoped<ICustomAuthenticationStateProvider, MauiAuthenticationStateProvider>();
// Use our custom provider when the app needs an AuthenticationStateProvider
builder.Services.AddScoped<AuthenticationStateProvider>(s 
    => (MauiAuthenticationStateProvider)s.GetRequiredService<ICustomAuthenticationStateProvider>());
```
