using DiChoSaiGon.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using DiChoSaiGon.Helpper;
using DiChoSaiGon.Extension;
using DiChoSaiGon.ModelViews;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using AspNetCoreHero.ToastNotification.Abstractions;

namespace DiChoSaiGon.Controllers
{
    [Authorize]
    public class AccountsController : Controller
    {
        private readonly dbMarketsContext _context;
        public INotyfService _notyfService { get; }
        public AccountsController(dbMarketsContext context, INotyfService notyfService)
        {
            _notyfService = notyfService;
            _context = context;
        }
        [Route("tai-khoan-cua-toi.html", Name = "Taikhoan")]
        public IActionResult Dashboard()
        {

            var taikhoanID = HttpContext.Session.GetString("CustomerId");
            if (taikhoanID != null)
            {
                int tk = Convert.ToInt32(taikhoanID);
                /*int taikhoanID2 = Convert.ToInt32(taikhoanID);*/
                var khachhang = _context.Customers.AsNoTracking().SingleOrDefault(x => x.CustomerId==Convert.ToInt32(taikhoanID));
                if (khachhang != null)
                {
                    var IsDonHang = _context.Orders
                       /* .Include(x => x.TransactStatus)*/
                        .AsNoTracking()
                        .Where(x => x.CustomerId == tk)
                        .OrderByDescending(x => x.OrderDate)
                        .Include(x => x.TransactStatus)
                        .ToList();
                    ViewBag.IsDonHang = IsDonHang;
                    return View(khachhang);
                }

            }
            return RedirectToAction("Login", "Accounts");
        }
        [AcceptVerbs("GET", "POST")]
        public IActionResult ValidatePhone(string Phone)
        {
            try
            {
                var khachhang = _context.Customers.AsNoTracking().SingleOrDefault(x => x.Phone.ToLower() == Phone.ToLower());
                if (khachhang != null)
                    return Json(data: "Số điện thoại : " + Phone + " đã được sử dụng");


                return Json(data: true);

            }
            catch
            {
                return Json(data: true);
            }
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ValidateEmail(string Email)
        {
            try
            {
                var khachhang = _context.Customers.AsNoTracking().SingleOrDefault(x => x.Email.ToLower() == Email.ToLower());
                if (khachhang != null)
                    return Json(data: "Email : " + Email + " đã được sử dụng");
                return Json(data: true);
            }
            catch
            {
                return Json(data: true);
            }
        }
        [HttpGet]
        [AllowAnonymous]
        [Route("dang-ky.html", Name = "DangKy")]
        public ActionResult DangKyTaiKhoan()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [Route("dang-ky.html", Name = "DangKy")]
        public async Task<IActionResult> DangKyTaiKhoan(RegisterVM taikhoan)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string salt = Utilities.GetRandomKey();
                    Customer khachhang = new Customer
                    {
                        FullName = taikhoan.FullName,
                        Phone = taikhoan.Phone,
                        Email = taikhoan.Email,
                        Password = (taikhoan.Password + salt.Trim().ToMD5()),
                        Active = true,
                        Salt = salt,
                        CreateDate = DateTime.Now

                    };
                    try
                    {
                        _context.Add(khachhang);
                        await _context.SaveChangesAsync();
                        _notyfService.Success("Đăng ký thành công");
                        HttpContext.Session.SetString("CustomerId", khachhang.CustomerId.ToString());
                        var taikhoanID = HttpContext.Session.GetString("CustomerId");
                        var claims = new List<Claim> {
                            new Claim(ClaimTypes.Name,khachhang.FullName),
                            new Claim("CustomerId",khachhang.CustomerId.ToString())
                        };
                        ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "login");
                        ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                        await HttpContext.SignInAsync(claimsPrincipal);
                        return RedirectToAction("Dashboard", "Accounts");



                    }
                    catch
                    {
                        return RedirectToAction("DangKyTaiKhoan", "Accounts");
                    }
                }


            }
            catch
            {
                return RedirectToAction("DangkyTaiKhoan", "Accounts");
            }
            return View(taikhoan);
        }
        [AllowAnonymous]
        [Route("dang-nhap.html", Name = "DangNhap")]
        public IActionResult Login(string returnUrl = null)
        {
            var taikhoanID = HttpContext.Session.GetString("CustomerId");
            if (taikhoanID != null)
            {
                return RedirectToAction("Dashboard", "Accounts");
            }
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [Route("dang-nhap.html", Name = "DangNhap")]
        public async Task<IActionResult> Login(LoginViewModel customer, string returnUrl)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    bool isEmail = Utilities.IsValidEmail(customer.UserName);
                    if (!isEmail) return View(customer);

                    var khachhang = _context.Customers.AsNoTracking().SingleOrDefault(x => x.Email.Trim() == customer.UserName);

                    /*if (khachhang == null) return RedirectToAction("DangkyTaiKhoan");*/
                    string pass = customer.Password + khachhang.Salt.Trim().ToMD5();
                    if (khachhang.Password != pass && khachhang!=null)
                    {
                        if (khachhang.Active == false)
                        {
                            return RedirectToAction("ThongBao", "Accounts");
                        }
                        _notyfService.Success("Thông tin đăng nhập chưa chính xác");
                        return View(customer);
                       
                    }
                    //kiem tra xem account co bi disable hay khong

                    else
                    {
                        //Luu Session MaKh
                        HttpContext.Session.SetString("CustomerId", khachhang.CustomerId.ToString());
                        var taikhoanID = HttpContext.Session.GetString("CustomerId");

                        //Identity
                        var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, khachhang.FullName),
                        new Claim("CustomerId", khachhang.CustomerId.ToString())
                    };
                        ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "login");
                        ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                        await HttpContext.SignInAsync(claimsPrincipal);
                        _notyfService.Success("Đăng nhập thành công");

                        return RedirectToAction("Dashboard", "Accounts");
                    }

                   
                    /*     if (string.IsNullOrEmpty(returnUrl))
                         {
                             return RedirectToAction("Dashboard", "Accounts");
                         }
                         else
                         {
                             return Redirect(returnUrl);
                         }*/
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("DangkyTaiKhoan", "Accounts");
            }
            return View(customer);
        }
        [HttpGet]
        [Route("dang-xuat.html", Name = "Logout")]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync();
            HttpContext.Session.Remove("CustomerId");
            return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            try
            {
                var taikhoanID = HttpContext.Session.GetString("CustomerId");
                if (taikhoanID == null)
                {
                    return RedirectToAction("Login", "Accounts");
                }
                var taikhoan = _context.Customers.Find(Convert.ToInt32( taikhoanID));
                if (taikhoan == null) return RedirectToAction("Login", "Accounts");
                var pass = model.PasswordNow.Trim() + taikhoan.Salt.Trim().ToMD5();
                if (pass == taikhoan.Password)
                {
                    string passnew = model.Password.Trim() + taikhoan.Salt.Trim().ToMD5();
                    taikhoan.Password = passnew;
                    _context.Update(taikhoan);
                    _context.SaveChanges();
                    _notyfService.Success("Thông tin tài khoản đã được update");
                    return RedirectToAction("Dashboard", "Accounts");




                }
                else
                {
                    _notyfService.Error("Sai mật khẩu ");
                    return RedirectToAction("Dashboard", "Accounts");
                }
            }
            catch
            {
                _notyfService.Error("Thay đổi mật khẩu không thành công");
                return RedirectToAction("Dashboard", "Accounts");
            }
            _notyfService.Error("Thay đổi mật khẩu không thành công");
            return RedirectToAction("Dashboard", "Accounts");
        }
    }
}
