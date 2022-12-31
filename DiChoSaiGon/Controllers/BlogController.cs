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
    public class BlogController : Controller
    {
        private readonly dbMarketsContext _context;

        public BlogController(dbMarketsContext context)
        {

            _context = context;
        }
        [Route("blog.html",Name ="Blog")]
        public IActionResult Index(int? page)
        {

            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 20;
            var IsTindangs = _context.TinDangs
                .AsNoTracking()
                .OrderByDescending(x => x.PostId);
            PagedList<TinDang> models = new PagedList<TinDang>(IsTindangs, pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            return View(models);
        }
        [Route("/tin-tuc/{Alias}-{id}.html", Name = "TinDetails")]
        public IActionResult Details(int id)
        {


            var tinDang = _context.TinDangs
                .AsTracking()
                .SingleOrDefault(x=>x.PostId==id);
               
            if (tinDang == null)
            {
                return RedirectToAction("Index");
            }
            var IsBaivietlienquan = _context.TinDangs
                .AsTracking()
                .Where(x => x.Published == true && x.PostId != id)
                .Take(3)
                .OrderByDescending(x=>x.CreatedDate)
                .ToList();
            ViewBag.Baivietlienquan = IsBaivietlienquan;
            return View(tinDang);
        }
    }
}
