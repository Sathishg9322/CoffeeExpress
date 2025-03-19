using CoffeeShop.Data;
using CoffeeShop.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System;
using System.Numerics;

namespace CoffeeShop.Controllers
{
    public class MenuController : Controller
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public MenuController(ApplicationDbContext dbContext, IWebHostEnvironment webHostEnvironment)
        {
            this.dbContext = dbContext;
            _webHostEnvironment = webHostEnvironment;

        }


        public IActionResult Add(Guid ? Id)
        {
            if (Id == null)  
            {
                return View(new Menu());
            }

            var menu = dbContext.Menu.Find(Id);
            if (menu == null)
            {
                return NotFound();
            }
            return View(menu);
        }

        [HttpPost]
        public async Task<IActionResult> Add(Menu viewModel, IFormFile? imageFile)
        {            
            if (ModelState.IsValid)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "MenuImages");

                var existingMenu = dbContext.Menu.AsNoTracking().FirstOrDefault(m => m.Id == viewModel.Id);

                    if (existingMenu != null)
                    {
                        viewModel.ImagePath = existingMenu.ImagePath;  // Retain existing image path
                    }
                

                if (imageFile != null)
                {
                    if (!string.IsNullOrEmpty(existingMenu?.ImagePath))
                    {
                        string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, existingMenu.ImagePath.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        imageFile.CopyTo(fileStream);
                    }

                    viewModel.ImagePath = "/MenuImages/" + uniqueFileName;
                }

                if(viewModel.Id == Guid.Empty)
                {
                    dbContext.Menu.Add(viewModel);
                }

                else
                {
                    dbContext.Menu.Update(viewModel);
                }
                await dbContext.SaveChangesAsync();

                return RedirectToAction("Menu");
              
            }
           
            return View(viewModel);
        }

        public async Task<IActionResult>Menu()
        {
            var item = dbContext.Menu.ToList();
            return View(item);
        }

        public async Task<IActionResult> Delete(Guid Id)
        {
            var item = dbContext.Menu.Find(Id);

            dbContext.Menu.Remove(item);
            dbContext.SaveChanges();

            return RedirectToAction("Menu");
        }
        public async Task<IActionResult> MenuList()
        {
            var item = await dbContext.Menu.ToListAsync();

            // Grouping category

            var groupedItem = item.GroupBy(item => item.Category).ToDictionary(a => a.Key, a => a.ToList());
            
            return View(groupedItem);
        }

        public async Task<IActionResult> GetCategory(string category)
        {
            var item = await dbContext.Menu.ToListAsync();

            var itemsInCategory = dbContext.Menu.Where(i => i.Category == category).GroupBy(item => item.Category).ToDictionary(a => a.Key, a => a.ToList());
            return PartialView("_Menu", itemsInCategory);
        }

        public async Task<IActionResult> GetItemQuantity(Guid id)
        {
            var item = await dbContext.Menu.FindAsync(id);
            return PartialView("_ItemButton", item);
        }
       
        [HttpPost]
        public async Task<IActionResult> IncreaseQuantity(Guid id)
        {
            var item = await dbContext.Menu.FindAsync(id);
            if (item != null)
            {
                item.Quantity++;
                dbContext.SaveChanges();
            }

            var order = new OrderId
            {
                Name = "Name",
                Datetime = DateTime.Now,
                Item = item.Item,
                Price = item.Price,
                Quantity = item.Quantity
            };

            await dbContext.OrderIds.AddAsync(order);
            await dbContext.SaveChangesAsync();

            return PartialView("_ItemButton", item);
        }

        [HttpPost]
        public async Task<IActionResult> DecreaseQuantity(Guid id)
        {
            var item = await dbContext.Menu.FindAsync(id);
            if (item != null && item.Quantity > 0)
            {
                item.Quantity--;
                dbContext.SaveChanges();
            }

            var order = new OrderId
            {
                Name = "Name",
                Datetime = DateTime.Now,
                Item = item.Item,
                Price = item.Price,
                Quantity = item.Quantity
            };

            await dbContext.OrderIds.AddAsync(order);
            await dbContext.SaveChangesAsync();
            return PartialView("_ItemButton", item);
        }

        [HttpPost]
        public  async Task<IActionResult> AddItem(Guid id)
        {
            var item = await dbContext.Menu.FindAsync(id);
            if (item != null)
            {
                item.Quantity = 1;
                dbContext.SaveChanges();
            }

            var order = new OrderId
            {
                Name = "Name",
                Datetime = DateTime.Now,
                Item = item.Item,
                Price = item.Price,
                Quantity = item.Quantity
            };

            await dbContext.OrderIds.AddAsync(order);
            await dbContext.SaveChangesAsync();

            return PartialView("_ItemButton", item);
        }


        //[HttpPost]
        //public async Task<IActionResult> ItemQuantity(Guid id, string actionType)
        //{

        //    var item = await dbContext.Menu.FindAsync(id);
        //    if (item != null)
        //    {
        //        if (actionType == "Increment")
        //            item.Quantity++;
        //        else if (actionType == "Decrement" && item.Quantity > 0)
        //            item.Quantity--; 
        //    }

        //    var order = new OrderId
        //    {
        //        Name = "Name",
        //        Datetime = DateTime.Now,
        //        Item = item.Item,
        //        Price = item.Price,
        //        Quantity = item.Quantity
        //    };
        //    await dbContext.OrderIds.AddAsync(order);
        //    await dbContext.SaveChangesAsync();

        //    var itemsInCategory = dbContext.Menu.GroupBy(item => item.Category).ToDictionary(a => a.Key, a => a.ToList());
            
            
        //    return View("_Menu",itemsInCategory);
        //}

        public async Task<IActionResult> Cart()
        {
            var item = dbContext.OrderIds.Where(a => a.Quantity > 0) .GroupBy(a => a.Item)
                .Select(b => b.OrderByDescending(a => a.Datetime).FirstOrDefault()).ToList();

            return View(item);
        }
        
        public async Task<IActionResult> RemoveItem(string a)
        {
            var item = dbContext.OrderIds.Where(i => i.Item.Contains(a)).ToList();

                dbContext.OrderIds.RemoveRange(item);

            var items = dbContext.Menu.FirstOrDefault(o => o.Item == a);

            if (items != null)
            {
                items.Quantity = 0;
            }

            await dbContext.SaveChangesAsync();

            return RedirectToAction("Cart");
        }

        public async Task<IActionResult> PlaceOrder(decimal total, string items)
        {
            var username = User.Identity.Name;
            var user = await dbContext.Users.Where(i => i.EmailId.Contains(username)).ToListAsync();

            if (!String.IsNullOrEmpty(username))
            {
                ViewBag.Name = user.FirstOrDefault().Name;

                string[] mode = { "Cash", "Upi" };
                Random random = new Random();
                int index = random.Next(mode.Length);

                ViewBag.Random = mode[index];

                var item = dbContext.OrderIds.GroupBy(a => a.Item).Select(b => new OrderId
                {
                    Id = b.First().Id,
                    Item = b.Key,
                    Quantity = b.Max(a => a.Quantity),
                    Price = b.First().Price
                }).ToList();

                ViewBag.Id = item.First().Id;

                var order = new History
                {
                    OrderId = item.First().Id,
                    Datetime = DateTime.Now,
                    Item = items,
                    Name = user.FirstOrDefault().Name,
                    Payment = mode[index],
                    Total = (int)total
                };
                dbContext.History.Add(order);
                await dbContext.SaveChangesAsync();

                var remove = await dbContext.OrderIds.ToListAsync();
                dbContext.OrderIds.RemoveRange(remove);

                dbContext.Database.ExecuteSqlRaw("UPDATE Menu SET Quantity = 0");

                await dbContext.SaveChangesAsync();

                return View(item);
            }

            else
            {
                return View();
            }
        }
       
        public async Task<IActionResult> Cancel()
        {
            var item = await dbContext.OrderIds.ToListAsync();

                dbContext.OrderIds.RemoveRange(item);

            dbContext.Database.ExecuteSqlRaw("UPDATE Menu SET Quantity = 0");

            await dbContext.SaveChangesAsync();
           
            return RedirectToAction("Cart");
        }

        public async Task<IActionResult> OrderHistory(string searchString, string sortOrderId, string sortOrderPay, string sortOrderTotal, int page = 1 )
        {
            var username = User.Identity.Name;
            var user = dbContext.Users.FirstOrDefault(u => u.EmailId == username);

            var item = await dbContext.History.ToListAsync();

            if (user != null)
            {
                if (user.Role == "Customer")
                {

                    item = item.Where(n => n.Name.Equals(user.Name)).ToList();
                }

                else if (user.Role == "Admin")
                {
                    item = item.ToList();
                }
            }
                // Search

                if (!String.IsNullOrEmpty(searchString))
                {
                    item = item.Where(n => n.Name.ToLower().Contains(searchString.ToLower())
                    || n.OrderId.ToString().Contains(searchString) || n.Datetime.ToString().Contains(searchString)).ToList();

                    ViewBag.SearchString = item;
                }

                // Sorting
                ViewBag.SortOrderId = sortOrderId == "id_desc" ? "" : "id_desc";
                ViewBag.SortOrderPay = sortOrderPay == "pay_desc" ? "pay_asc" : "pay_desc";
                ViewBag.SortOrderTotal = sortOrderTotal == "total_desc" ? "total_asc" : "total_desc";

                switch (sortOrderId)
                {
                    case "id_desc":
                        item = item.OrderByDescending(i => i.OrderId).ToList();
                        break;
                    default:
                        item = item.OrderBy(i => i.OrderId).ToList();
                        break;
                }
                switch (sortOrderPay)
                {
                    case "pay_desc":
                        item = item.OrderByDescending(p => p.Payment).ToList();
                        break;
                    case "pay_asc":
                        item = item.OrderBy(i => i.Payment).ToList();
                        break;
                }
                switch (sortOrderTotal)
                {
                    case "total_desc":
                        item = item.OrderByDescending(t => t.Total).ToList();
                        break;
                    case "total_asc":
                        item = item.OrderBy(i => i.Total).ToList();
                        break;
                }

                // Pagination
                ViewBag.CurrentSearch = searchString;
                ViewBag.CurrentSortOrder = sortOrderId;
                ViewBag.CurrentSortPay = sortOrderPay;
                ViewBag.CurrentSortTotal = sortOrderTotal;

                var paginateditem = item.Skip((page - 1) * 8).Take(8).ToList();

                ViewBag.TotalPages = (int)Math.Ceiling((decimal)item.Count() / 8);
                ViewBag.CurrentPage = page;

                return View(paginateditem);
            
            
        }

        public async Task<IActionResult> OrderDetail(int id)
        {
            var item = await dbContext.History.Where(i => i.Id == id).ToListAsync();

            //var formatteditem =  item.Select(order => new History
            //{
            //    Item = string.Join("<br />", order.Item.Split(',').Select(item => item.Trim()))
            //}).ToList();
            return View(item);
        }

        public async Task<IActionResult> Contact()
        {
            return View();  
        }
    }
}
