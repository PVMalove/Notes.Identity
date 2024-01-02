using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Notes.Identity.Models;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Notes.Identity.Controllers;

public class AuthController : Controller
{
    private readonly SignInManager<AppUser> signInManager;
    private readonly UserManager<AppUser?> userManager;
    private readonly IIdentityServerInteractionService interactionService;

    public AuthController(SignInManager<AppUser> signInManager, UserManager<AppUser?> userManager,
        IIdentityServerInteractionService interactionService)
    {
        this.signInManager = signInManager;
        this.userManager = userManager;
        this.interactionService = interactionService;
    }

    [HttpGet]
    public IActionResult Login(string returnUrl)
    {
        LoginViewModel viewModel = new LoginViewModel
        {
            ReturnUrl = returnUrl
        };
        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel viewModel)
    {
        if (!ModelState.IsValid)
            return View(viewModel);

        AppUser? user = await userManager.FindByNameAsync(viewModel.UserName);

        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "User not found");
            return View(viewModel);
        }

        SignInResult result = await signInManager
            .PasswordSignInAsync(viewModel.UserName, viewModel.Password, false, false);

        if (result.Succeeded)
        {
            return Redirect(viewModel.ReturnUrl);
        }
        ModelState.AddModelError(string.Empty, "Invalid login");
        return View(viewModel);
    }

    [HttpGet]
    public IActionResult Register(string returnUrl)
    {
        RegisterViewModel viewModel = new RegisterViewModel
        {
            ReturnUrl = returnUrl
        };
        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel viewModel)
    {
        if(!ModelState.IsValid)
            return View(viewModel);
        
        AppUser user = new AppUser
        {
            UserName = viewModel.UserName
        };
        
        IdentityResult result = await userManager.CreateAsync(user, viewModel.Password);
        if (result.Succeeded)
        {
            await signInManager.SignInAsync(user, false);
            return Redirect(viewModel.ReturnUrl);
        }
        ModelState.AddModelError(string.Empty, "Error occurred");
        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Logout(string logoutId)
    {
        await signInManager.SignOutAsync();
        LogoutRequest? logoutRequest = await interactionService.GetLogoutContextAsync(logoutId);
        return Redirect(logoutRequest.PostLogoutRedirectUri);
    }
}