using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using RMS.Data;
using RMS.Models;
using RMS.Models.ViewModels;
using System.Security.Claims;

namespace RMS.Controllers
{
    public class AccountController : Controller
    {
        private AppDbContext _appcontext;
        public AccountController(AppDbContext appDbContext)
        {
            _appcontext = appDbContext;
        }
        public IActionResult Index()
        {
            return View(_appcontext.UserAccounts.ToList());
        }
        public IActionResult Registration() 
        {
            return View();
        }

        [HttpPost]
        public IActionResult Registration(RegViewModel model)
        {
            if (ModelState.IsValid)
            {
                UserAccount account = new UserAccount();
                account.Email = model.Email;
                account.Password = model.Password;
                account.FirstName = model.FirstName;
                account.LastName = model.LastName;
                account.UserName = model.UserName;

                try
                {
                    _appcontext.UserAccounts.Add(account);
                    _appcontext.SaveChanges();

                    ModelState.Clear();
                    ViewBag.Message = $"{account.FirstName} {account.LastName} registered successfully. Please Login.";

                }
                catch (DbUpdateException ex)
                {

                    ModelState.AddModelError("", "Please enter unique Email or Password.");
                    return View(model);
                }
                return View();
            }

            return View(model);
        }

        public IActionResult Login() 
        {
            return View();
        }


        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            { 
              var user = _appcontext.UserAccounts.Where(x => (x.UserName == model.UserName || x.Email == model.UserName) && x.Password == model.Password).FirstOrDefault();
                if (user != null)
                {
                    //Success, Create Cookie
                    Claim claim = new(ClaimTypes.Name, user.Email);
                    var claims = new List<Claim>
                    {
                        claim,
                        new("Name", user.FirstName),
                        new Claim(ClaimTypes.Role, "User"),
                    };   

                    var claimsIdentity = new ClaimsIdentity(claims,CookieAuthenticationDefaults.AuthenticationScheme);
                    HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                    return RedirectToAction("SecurePage");
               
                }
                else 
                {
                    ModelState.AddModelError("", "UserName/Email or Password is not correct");
                }
            }
                return View(model);

        }

        public IActionResult LogOut()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index");
        }

        [Authorize]
        public IActionResult SecurePage() 
        {
            ViewBag.Name = HttpContext.User.Identity.Name;
            return View();
        }
    }

}
