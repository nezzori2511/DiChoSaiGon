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
    public class PageController : Controller
    {
        private readonly dbMarketsContext _context;
        public PageController(dbMarketsContext context)
        {

            _context = context;
        }
        [Route("page.html", Name = "page")]
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
        [Route("/page/{Alias}", Name = "PageDetails")]
        public IActionResult Details(string Alias)
        {

            if (string.IsNullOrEmpty(Alias)) return RedirectToAction("Index","Home");
            var page = _context.Pages
                .AsTracking()
                .SingleOrDefault(x => x.Alias == Alias);

            if (page == null)
            {
                return RedirectToAction("Index","Home");
            }
           
            return View(page);
        }
    }
}
