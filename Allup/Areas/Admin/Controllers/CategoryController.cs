using Allup.DAL;
using Allup.Models;
using Fiorello.Extentions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Allup.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;


        public CategoryController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        public IActionResult Index()
        {
            return View(_context.Categories.Where(ct => ct.IsDelete == false).Include(c => c.Parent).OrderByDescending(ctr => ctr.Id));
        }

        public IActionResult Create()
        {
            GetMainCategory();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category, int? MainId)
        {
            GetMainCategory();
            if (!ModelState.IsValid) return View();
            if (category.IsMain)
            {
                bool isExist = _context.Categories.Where(c => c.IsDelete == false && c.IsMain == true)
                    .Any(c => c.Name.Trim().ToLower() == category.Name.Trim().ToLower());

                if (isExist)
                {
                    ModelState.AddModelError("Name", "This category already exist");
                    return View();
                }
                if (category.Photos == null)
                {
                    ModelState.AddModelError("", "You have to upload image");
                    return View();
                }

                if (!category.Photos.IsImage())
                {
                    ModelState.AddModelError("", "Please select just image type");
                    return View();
                }

                if (!category.Photos.MaxSize(200))
                {
                    ModelState.AddModelError("", "Image size can be less than 200 KB");
                    return View();
                }

                string folder = Path.Combine("assets", "images");
                string fileName = await category.Photos.SaveImageAsync(_env.WebRootPath, folder);
                category.Image = fileName;
                category.IsDelete = false;
            }
            else
            {
                if (MainId == null)
                {
                    ModelState.AddModelError("", "Select Main Category");
                    return View();
                }
                Category mainCtg = _context.Categories.Where(c => c.IsDelete == false && c.IsMain == true).Include(c => c.Children)
                    .FirstOrDefault(c => c.Id == MainId);
                if (mainCtg == null)
                {
                    ModelState.AddModelError("", "Select Main Category");
                    return View();
                }

                bool isExist = mainCtg.Children.Any(cC => cC.Name.Trim().ToLower() == category.Name.Trim().ToLower());

                if (isExist)
                {
                    ModelState.AddModelError("", $"This category already exist in {mainCtg.Name}");
                    return View();
                }

                category.Parent = mainCtg;
            }

            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Detail(int? id)
        {
            if (id == null) return NotFound();
            Category category = _context.Categories.Where(c => c.IsDelete == false)
                .Include(c => c.Children).Include(c => c.Parent)
                    .FirstOrDefault(c => c.Id == id);
            if (category == null) return NotFound();
            return View(category);
        }

        public async Task<IActionResult> Update(int? id)
        {
            GetMainCategory();
            if (id == null) return NotFound();
            Category categories = _context.Categories.Where(c => c.IsDelete == false)
                .FirstOrDefault(c => c.Id == id);
            if (categories == null) return NotFound();
            categories.IsDelete = false;
            await _context.SaveChangesAsync();
            return View(categories);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int? id, Category category, int? Main_Id)
        {
            GetMainCategory();
            if (id == null) return NotFound();
            if (category == null) return NotFound();

            Category categ = await _context.Categories.FindAsync(id);

            //there's a bug
            //if (categ.IsMain)
            //{
            //    bool isExist = _context.Categories.Where(c => c.IsDelete == false)
            //        .Any(c => c.Name.Trim().ToLower() == category.Name.Trim().ToLower());

            //    if (isExist)
            //    {
            //        ModelState.AddModelError("Name", "This category already exist");
            //        return View();
            //    }
            //}

            if (categ.IsMain && category.Photos != null)
            {
                string folder = Path.Combine("assets", "images");
                string fileName = await category.Photos.SaveImageAsync(_env.WebRootPath, folder);
                categ.Image = fileName;
            }
            if (!categ.IsMain)
            {
                Category mainCtg = _context.Categories.Where(c => c.IsDelete == false && c.IsMain == true).Include(c => c.Children)
                    .FirstOrDefault(c => c.Id == Main_Id);
                if (mainCtg == null)
                {
                    ModelState.AddModelError("", "Select Main Category");
                    return View();
                }

                bool isExist = mainCtg.Children.Any(cC => cC.Name.Trim().ToLower() == category.Name.Trim().ToLower());

                if (isExist)
                {
                    ModelState.AddModelError("", $"This category already exist in {mainCtg.Name}");
                    return View();
                }

                categ.Parent = mainCtg;
            }
            
            categ.Name = category.Name;
            //categ.Parent.Name = category.Parent.Name;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
            //return Json(category.Photos);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null) return NotFound();
            Category category = _context.Categories.Where(c => c.IsDelete == false)
                .Include(c => c.Children).Include(c=>c.Parent)
                    .FirstOrDefault(c => c.Id == id);
            if (category == null) return NotFound();
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeletePost(int? id)
        {
            if (id == null) return NotFound();
            Category category = _context.Categories.Where(c => c.IsDelete == false)
                .Include(c => c.Children)
                    .FirstOrDefault(c => c.Id == id);
            if (category == null) return NotFound();

            category.IsDelete = true;
            if(category.Children.Count > 0)
            {
                foreach (Category cat in category.Children)
                {
                    cat.IsDelete = true;
                }
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private void GetMainCategory()
        {
            ViewBag.MainCategory = _context.Categories.Where(ctr => ctr.IsDelete == false && ctr.IsMain == true).ToList();
        }
    }
}
