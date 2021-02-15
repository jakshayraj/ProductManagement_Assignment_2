using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ProductManagement.Models;
using PagedList;
using log4net;

namespace ProductManagement.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        // For the loging
        private static readonly ILog Log = LogManager.GetLogger(typeof(ProductsController));
        // Entity for the database
        private ProdcutsEntities db = new ProdcutsEntities();
        List<string> categories = new List<string> { "Mobile", "Shoes", "Electonics", "Cloths", "Accesories" };
        List<int> quantities = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        // GET: Products/Index
	[Route("Products/Index")]
        public ActionResult Index()
        {
            return View();
        }

        // GET: Products/List
        // For display the list of product, searching and sorting
	[Route("Products/List")]
        public ActionResult List(string sortOrder, string currentFilter, string searchString, int? page)
        {
            // For searching of product
            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            ViewBag.CurrentFilter = searchString;
            var products = from s in db.Products
                           select s;
            if (!String.IsNullOrEmpty(searchString))
            {
                products = products.Where(s => s.Name.Contains(searchString)
                                       || s.Category.Contains(searchString));
            }

            // For sorting of product
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";
            switch (sortOrder)
            {
                case "name_desc":
                    products = products.OrderByDescending(s => s.Name);
                    break;
                case "Date":
                    products = products.OrderBy(s => s.Category);
                    break;
                case "date_desc":
                    products = products.OrderByDescending(s => s.Category);
                    break;
                default:
                    products = products.OrderBy(s => s.Name);
                    break;
            }

            // For paging
            int pageSize = 3;
            int pageNumber = (page ?? 1);
            return View(products.ToPagedList(pageNumber, pageSize));

        }

        // POST: Products/List
        [HttpPost]
        public ActionResult List(string[] ids)
        {
            if (ids == null || ids.Length == 0)
            {
                //Throw error
                ModelState.AddModelError("", "No item selected to delete");
                return View();
            }
            //  For the multiple delete
            //Bind the task collection into list
            List<int> TaskIds = ids.Select(x => Int32.Parse(x)).ToList();
            for (var i = 0; i < TaskIds.Count(); i++)
            {
                Product product = db.Products.Find(TaskIds[i]);
                //Remove the record from the database
                string smallImage = Request.MapPath(product.Small_img);
                string largeImage = Request.MapPath(product.Large_img);
                db.Entry(product).State = EntityState.Deleted;
                try
                {
                    if (db.SaveChanges() > 0)
                    {
                        // Delete the small image
                        if (System.IO.File.Exists(smallImage))
                        {
                            System.IO.File.Delete(smallImage);
                        }
                        // Delete the large image
                        if (System.IO.File.Exists(largeImage))
                        {
                            System.IO.File.Delete(largeImage);
                        }
                        Log.Info("Product delete Successffuly");
                        return RedirectToAction("List");
                    }
                }
                // Catch the Exception
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                    ModelState.AddModelError("", "Error in product deletion");
                }
            }
            return RedirectToAction("List");
        }
	[Route("Products/Details")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }
        // GET: Products/Create
	[Route("Products/Create")]
        public ActionResult Create()
        {
            ViewBag.categories = categories;
            ViewBag.quantities = quantities;
            return View();
        }

        // POST: Products/Create
        // Add the product
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Category,Price,Quantity,Short_desc,Long_desc,Small_img,Large_img")] Product product, HttpPostedFileBase SmallImagefile, HttpPostedFileBase LargeImagefile)
        {
            // Exception handling
            try
            {
                if (ModelState.IsValid)
                {
                    var allowedExtensions = new[] {
                    ".Jpg", ".png", ".jpg", "jpeg"
                };
                    // Only small image is uploaded
                    // Create the small image path
                    string smallfilename = Path.GetFileName(SmallImagefile.FileName);
                    string _filename1 = DateTime.Now.ToString("yymmssfff") + smallfilename;
                    string smallfileextension = Path.GetExtension(SmallImagefile.FileName);
                    // Add the image path in the database
                    product.Small_img = "~/Images/" + _filename1;
                    string smallImagepath = Path.Combine(Server.MapPath("~/Images/"), _filename1);
                    // Check the file extension
                    if (!allowedExtensions.Contains(smallfileextension))
                    {
                        ModelState.AddModelError("", "Invalid type of file");
                    }
                    // Check the file size
                    else if (SmallImagefile.ContentLength >= 1000000)
                    {
                        ModelState.AddModelError("", "Invalid size of file");
                    }
                    else
                    {
                        // Large file is also uploaded
                        if (LargeImagefile != null)
                        {
                            // For the large file : create the file path and store in database
                            string largefilename = Path.GetFileName(LargeImagefile.FileName);
                            string _filename2 = DateTime.Now.ToString("yymmssfff") + largefilename;
                            string largefileextension = Path.GetExtension(LargeImagefile.FileName);
                            product.Large_img = "~/Images/" + _filename2;
                            string largeImagepath = Path.Combine(Server.MapPath("~/Images/"), _filename2);
                            // Check the large file extension
                            if (!allowedExtensions.Contains(largefileextension))
                            {
                                ModelState.AddModelError("", "Invalid type of file");
                            }
                            // Check the large file size
                            else if (LargeImagefile.ContentLength >= 1000000)
                            {
                                ModelState.AddModelError("", "Invalid size of file");
                            }
                            else
                            {
                                // Store the data in database
                                db.Products.Add(product);
                                if (db.SaveChanges() > 0)
                                {
                                    SmallImagefile.SaveAs(smallImagepath);
                                    LargeImagefile.SaveAs(largeImagepath);
                                    Log.Info("Product added Successffuly");
                                    ModelState.Clear();
                                    return RedirectToAction("List");
                                }
                            }
                        }
                        else
                        {
                            // Store the data in database for only small image uploaded
                            db.Products.Add(product);
                            if (db.SaveChanges() > 0)
                            {
                                SmallImagefile.SaveAs(smallImagepath);
                                ViewBag.msg = "Record Added";
                                ModelState.Clear();
                                Log.Info("Product added Successffuly");
                                return RedirectToAction("List");
                            }
                        }
                    }
                }
            }
            // Catch the exception
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                ModelState.AddModelError("", "Error in product uploading");
            }
            ViewBag.categories = categories;
            ViewBag.quantities = quantities;
            return View(product);
        }

        // GET: Products/Edit
        // Edit the product
	[Route("Products/Edit")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            // Product not found
            if (product == null)
            {
                return HttpNotFound();
            }
            // Store the image data in the session variables
            Session["SmallImage"] = product.Small_img;
            Session["LargeImage"] = product.Large_img;
            ViewBag.categories = categories;
            ViewBag.quantities = quantities;
            return View(product);
        }

        // POST: Products/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Category,Price,Quantity,Short_desc,Long_desc,Small_img,Large_img")] Product product, HttpPostedFileBase SmallImagefile, HttpPostedFileBase LargeImagefile)
        {
            // Exception Handling
            try
            {
                if (ModelState.IsValid)
                {
                    // For small or large image or both uploaded
                    if (SmallImagefile != null || LargeImagefile != null)
                    {
                        var allowedExtensions = new[] {
                        ".Jpg", ".png", ".jpg", "jpeg"
                         };
                        // For only smallimage uploaded
                        if (SmallImagefile != null && LargeImagefile == null)
                        {
                            // Same as product addition
                            string smallfilename = Path.GetFileName(SmallImagefile.FileName);
                            string _filename1 = DateTime.Now.ToString("yymmssfff") + smallfilename;
                            string smallfileextension = Path.GetExtension(SmallImagefile.FileName);
                            product.Small_img = "~/Images/" + _filename1;
                            string smallImagepath = Path.Combine(Server.MapPath("~/Images/"), _filename1);

                            if (!allowedExtensions.Contains(smallfileextension))
                            {
                                ModelState.AddModelError("", "Invalid type of file");
                            }
                            else if (SmallImagefile.ContentLength >= 1000000)
                            {
                                ModelState.AddModelError("", "Invalid size of file");
                            }
                            else
                            {
                                // Use session varible for not uploaded image to store the old data
                                product.Large_img = Session["LargeImage"] != null ? Request.MapPath(Session["LargeImage"].ToString()) : null;
                                db.Entry(product).State = EntityState.Modified;
                                string oldsmallpath = Session["SmallImage"] != null ? Request.MapPath(Session["SmallImage"].ToString()) : null;
                                // Update the data
                                if (db.SaveChanges() > 0)
                                {
                                    SmallImagefile.SaveAs(smallImagepath);
                                    // Delete the old file
                                    if (System.IO.File.Exists(oldsmallpath))
                                    {
                                        System.IO.File.Delete(oldsmallpath);
                                    }
                                    Log.Info("Product edit Successffuly");
                                    return RedirectToAction("List");
                                }
                            }
                        }
                        // For only large image uploaded
                        else if (LargeImagefile != null && SmallImagefile == null)
                        {
                            string largefilename = Path.GetFileName(LargeImagefile.FileName);
                            string _filename2 = DateTime.Now.ToString("yymmssfff") + largefilename;
                            string largefileextension = Path.GetExtension(LargeImagefile.FileName);
                            product.Large_img = "~/Images/" + _filename2;
                            string largeImagepath = Path.Combine(Server.MapPath("~/Images/"), _filename2);

                            if (!allowedExtensions.Contains(largefileextension))
                            {
                                ModelState.AddModelError("", "Invalid type of file");
                            }
                            else if (LargeImagefile.ContentLength >= 1000000)
                            {
                                ModelState.AddModelError("", "Invalid size of file");
                            }
                            else
                            {
                                // Use session varible for not uploaded image to store the old data
                                product.Small_img = Session["SmallImage"].ToString();
                                db.Entry(product).State = EntityState.Modified;
                                string oldlargepath = Session["LargeImage"] != null ? Request.MapPath(Session["LargeImage"].ToString()) : null;
                                // Update the data
                                if (db.SaveChanges() > 0)
                                {
                                    LargeImagefile.SaveAs(largeImagepath);
                                    // Delete the old file
                                    if (System.IO.File.Exists(oldlargepath))
                                    {
                                        System.IO.File.Delete(oldlargepath);
                                    }
                                    Log.Info("Product edit Successffuly");
                                    return RedirectToAction("List");
                                }
                            }
                        }
                        // Both image uploaded
                        else if (SmallImagefile != null && LargeImagefile != null)
                        {
                            string smallfilename = Path.GetFileName(SmallImagefile.FileName);
                            string _filename1 = DateTime.Now.ToString("yymmssfff") + smallfilename;
                            string smallfileextension = Path.GetExtension(SmallImagefile.FileName);
                            product.Small_img = "~/Images/" + _filename1;
                            string smallImagepath = Path.Combine(Server.MapPath("~/Images/"), _filename1);

                            string largefilename = Path.GetFileName(LargeImagefile.FileName);
                            string _filename2 = DateTime.Now.ToString("yymmssfff") + largefilename;
                            string largefileextension = Path.GetExtension(LargeImagefile.FileName);
                            product.Large_img = "~/Images/" + _filename2;
                            string largeImagepath = Path.Combine(Server.MapPath("~/Images/"), _filename2);

                            if (allowedExtensions.Contains(smallfileextension))
                            {
                                ModelState.AddModelError("", "Invalid type of file");
                            }
                            else if (SmallImagefile.ContentLength <= 1000000)
                            {
                                ModelState.AddModelError("", "Invalid size of file");
                            }
                            if (!allowedExtensions.Contains(largefileextension))
                            {
                                ModelState.AddModelError("", "Invalid type of file");
                            }
                            else if (LargeImagefile.ContentLength >= 1000000)
                            {
                                ModelState.AddModelError("", "Invalid size of file");
                            }
                            else
                            {
                                db.Entry(product).State = EntityState.Modified;
                                string oldsmallpath = Session["SmallImage"] != null ? Request.MapPath(Session["SmallImage"].ToString()) : null;
                                string oldlargepath = Session["LargeImage"] != null ? Request.MapPath(Session["LargeImage"].ToString()) : null;
                                // Update the data with both image
                                if (db.SaveChanges() > 0)
                                {
                                    SmallImagefile.SaveAs(smallImagepath);
                                    LargeImagefile.SaveAs(largeImagepath);
                                    // Delete the old file
                                    if (System.IO.File.Exists(oldsmallpath))
                                    {
                                        System.IO.File.Delete(oldsmallpath);
                                    }
                                    if (System.IO.File.Exists(oldlargepath))
                                    {
                                        System.IO.File.Delete(oldlargepath);
                                    }
                                    Log.Info("Product edit Successffuly");
                                    return RedirectToAction("List");
                                }
                            }
                        }
                    }
                    else
                    {
                        // Not any of the image is uploaded
                        // Session variable for store the old data
                        product.Small_img = Session["SmallImage"].ToString();
                        product.Large_img = Session["LargeImage"] != null ? Request.MapPath(Session["LargeImage"].ToString()) : null;
                        db.Entry(product).State = EntityState.Modified;
                        // Update the data
                        if (db.SaveChanges() > 0)
                        {
                            Log.Info("Product edit Successffuly");
                            return RedirectToAction("List");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                ModelState.AddModelError("", "Error in product editing");
            }
            ViewBag.categories = categories;
            ViewBag.quantities = quantities;
            return View(product);
        }


        // GET: Products/Delete
        // Delete the product
	[Route("Products/Delete")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            string smallImage = Request.MapPath(product.Small_img);
            string largeImage = Request.MapPath(product.Large_img);
            // Delete from the database
            db.Entry(product).State = EntityState.Deleted;
            if (db.SaveChanges() > 0)
            {
                // Delete the images
                if (System.IO.File.Exists(smallImage))
                {
                    System.IO.File.Delete(smallImage);
                }
                if (System.IO.File.Exists(largeImage))
                {
                    System.IO.File.Delete(largeImage);
                }
                Log.Info("Product deleted Successffuly");
                return RedirectToAction("List");
            }
            return View(product);
        }
    }
}
