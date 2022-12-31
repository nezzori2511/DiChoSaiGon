using DiChoSaiGon.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PagedList.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiChoSaiGon.Controllers
{
  
    public class ProductController : Controller
    {
        private readonly dbMarketsContext _context;
        public ProductController(dbMarketsContext context)
        {
            _context = context;
        }
        [Route("shop.html", Name = "ShopProduct")]
        public IActionResult Index(int? page )
        {
            try
            {
                var pageNumber = page == null || page <= 0 ? 1 : page.Value;
                var pageSize = 5;
                var IsProducts = _context.Products
                    .AsNoTracking()
                    .OrderByDescending(x => x.DateCreated);
                PagedList<Product> models = new PagedList<Product>(IsProducts, pageNumber, pageSize);
                ViewBag.CurrentPage = pageNumber;
                return View(models);
            }
            catch
            {
                return RedirectToAction("Index", "Home");
            }
          
        }
        [Route("/{Alias}", Name = "ListProduct")]
        public IActionResult List(string Alias, int page =1)
        {
            try
            {
                var pageSize = 5;
                var danhmuc = _context.Categories.AsNoTracking().SingleOrDefault(x=>x.Alias==Alias);
                var IsProducts = _context.Products
                    .AsNoTracking()
                    .Where(x => x.CatId == danhmuc.CatId)
                    .OrderByDescending(x => x.DateCreated);
                PagedList<Product> models = new PagedList<Product>(IsProducts, page, pageSize);
                ViewBag.CurrentPage = page;
                ViewBag.CurrentCat = danhmuc;
                return View(models);
            }
          
             catch
            {
                return RedirectToAction("Index", "Home");
            }
        }
        [Route("/{Alias}-{id}.html", Name = "ProductDetails")]
        public IActionResult Detail(int id)
        {
            var product = _context.Products.Include(x => x.Cat).FirstOrDefault(x => x.ProductId == id);
            if(product==null)
            {   
                
                return RedirectToAction("Index","Home");
            }
            var IsProdtuct = _context.Products
                .AsNoTracking()
                .Where(x => x.CatId == product.CatId && x.ProductId!=id && x.Active==true)
                .Take(4)
                .OrderByDescending(x=>x.DateCreated)
                .ToList();
            ViewBag.SanPham = IsProdtuct;
            return View(product);
        }
    }
}
