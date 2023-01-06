using AspNetCoreHero.ToastNotification.Abstractions;
using DiChoSaiGon.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiChoSaiGon.Controllers
{
    public class WishListController : Controller
    {
        public INotyfService _notyfService { get; }
        private readonly dbMarketsContext _context;
        public WishListController(dbMarketsContext context, INotyfService notyfService)
        {
            _notyfService = notyfService;
            _context = context;
        }
        [Route("/addwl")]
        [HttpPost]
        public IActionResult AddWishList(int productID)
        {
            if(HttpContext.Session.GetString("CustomerId")!=null)
            {
                WishList wishList = new WishList();
                int customerid = Convert.ToInt32(HttpContext.Session.GetString("CustomerId"));
                var checkwl = _context.WishLists.AsNoTracking().FirstOrDefault(x => x.CustomerId == customerid && x.ProductId == productID);
                if (checkwl == null)
                {
                    try
                    {

                        var Is_product = _context.Products.AsNoTracking().FirstOrDefault(x => x.ProductId == productID);
                        wishList.ProductName = Is_product.ProductName;
                        wishList.ProductId = Is_product.ProductId;
                        wishList.Thumb = Is_product.Thumb;
                        wishList.Price = Convert.ToInt32(Is_product.Price);
                        wishList.Active = Is_product.Active;
                        wishList.CustomerId = customerid;
                        _context.WishLists.Add(wishList);
                        _context.SaveChangesAsync();
                        //_notyfService.Success("Thêm thành công");
                        return Json(new { success = true });



                    }
                    catch
                    {
                        //_notyfService.Error("Thêm thất bại");
                        return Json(new { success = false });
                    }
                }
                else
                {
                    //_notyfService.Error("Thêm thất bại");
                    return Json(new { success = false });
                }
            }
            else
            {
                return RedirectToAction("Login", "Accounts");
            }
           
   
        }
        
        [Route("wishlist.html", Name = "Wishlist")]
        public IActionResult Index()
        {
            var Wislist = _context.WishLists
                .AsNoTracking()
                .Where(x=>x.CustomerId==Convert.ToInt32(HttpContext.Session.GetString("CustomerId"))) 
                .ToList();
            return View(Wislist);
        }
        [HttpPost]
        [Route("/api/wishlist/delete")]
        public IActionResult DeleteWishList(int productID)
        {
            
            var checkwishList = _context.WishLists
               .AsNoTracking()
               .FirstOrDefault(x => x.CustomerId == Convert.ToInt32(HttpContext.Session.GetString("CustomerId")) && x.ProductId == productID);
            
            _context.WishLists.Remove(checkwishList);
            _context.SaveChangesAsync();
            return Json(new { success = true });

        }
    }
}
