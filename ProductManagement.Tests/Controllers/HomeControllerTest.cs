using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProductManagement;
using ProductManagement.Controllers;
using ProductManagement.Models;


namespace ProductManagement.Tests.Controllers
{
    [TestClass]
    public class HomeControllerTest
    {
        private string currentFilter;
        private string sortOrder;
        private string searchString;
        private int? page;
        private int? id;
        private int? pid = 29006;

        [TestMethod]
        public void Login()
        {
            // Arrange
            HomeController controller = new HomeController();
            tblLogin login = new tblLogin()
            {
                emailid = "akshayraj@gmail.com",
                password = "123456"
            };
            // Act
            ViewResult result = controller.Login(login) as ViewResult;

            // Assert
            Assert.AreEqual("akshayraj", result.ViewBag.Message);
            Assert.IsNotNull(result);
        }
        [TestMethod]
        public void Index()
        {
            // Arrange
            ProductsController controller = new ProductsController();

            // Act
            ViewResult result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }
        [TestMethod]
        public void List()
        {
            // Arrange
            ProductsController controller = new ProductsController();

            // Act
            ViewResult result = controller.List(sortOrder,currentFilter,searchString, page)as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }
        [TestMethod]
        public void Create()
        {
            // Arrange
            ProductsController controller = new ProductsController();
            Product product = new Product()
            {
                Id = 10,
                Name = "Shoes",
                Category = "Common",
                Price = 1000,
                Quantity = 1,
                Short_desc = "Shoes",
                Small_img = "image"
            };

            // Act
            ViewResult result = controller.Create(product, null, null) as ViewResult;
            
            // Assert
            Assert.IsNotNull(result);
        }
        [TestMethod]
        public void Edit()
        {
            // Arrange
            ProductsController controller = new ProductsController();
            
            // Act
            ViewResult result = controller.Edit(1) as ViewResult;

            // Assert
            Assert.IsNull(result);
        }


    }
}
