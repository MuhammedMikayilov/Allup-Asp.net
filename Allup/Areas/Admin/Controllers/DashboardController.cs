using Allup.DAL;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Allup.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View(_context.Categories.Where(ct=> ct.IsDelete==false).OrderByDescending(ctr=>ctr.IsMain==true));
        }

        public IActionResult Create()
        {
            ViewBag.MainCtg = _context.Categories.Where(ctr => ctr.IsDelete == false && ctr.IsMain == true).ToList();
            return View();
        }
    }
}
