using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using InsuranceClientPortal.Models;
using InsuranceClientPortal.ViewModel;
using Microsoft.Extensions.Configuration;
using System.IO;
using InsuranceClientPortal.Helpers;
using Newtonsoft.Json;

namespace InsuranceClientPortal.Controllers
{
    public class HomeController : Controller
    {
        public IConfiguration configuration { get; set; }

        public HomeController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet(Name = "CreateCustomer")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost(Name = "CreateCustomer")]
        public async Task<IActionResult> Create(CustomerViewModel customer)
        {
            if (ModelState.IsValid)
            {
                var customerId = Guid.NewGuid().ToString();

                var tempFile = Path.GetTempFileName();
                using (var fs = new FileStream(tempFile, FileMode.Create, FileAccess.Write))
                {
                    await customer.Image.CopyToAsync(fs);
                }

                var fileName = Path.GetFileName(customer.Image.FileName);
                var tempPath = Path.GetDirectoryName(tempFile);
                var imagePath = Path.Combine(tempPath, string.Concat(customerId, "_", fileName));
                System.IO.File.Move(tempFile, imagePath);

                StorageHelper storageHelper = new StorageHelper();
                storageHelper.StorageConnectionString = configuration.GetConnectionString("StorageConnection");
                var imageUrl = await storageHelper.UploadFileAsync(imagePath, "images");
                System.IO.File.Delete(imagePath);

                storageHelper.TableConnectionString = configuration.GetConnectionString("TableConnection");
                Customer customerEntity = new Customer(customerId, customer.InsuranceType);
                customerEntity.Amount = customer.Amount;
                customerEntity.Name = customer.Name;
                customerEntity.Email = customer.Email;
                customerEntity.AppDate = customer.AppDate;
                customerEntity.Premium = customer.Premium;
                customerEntity.EndDate = customer.EndDate;
                customerEntity.ImageUrl = imageUrl;

                Customer newCustomer = await storageHelper.SaveInsuranceDetailsAsync(customerEntity, "customers");

                String messateText = JsonConvert.SerializeObject(newCustomer);
                await storageHelper.SendMessageAsync(messateText, "insurance-queue");
            }
            else
            {
                return BadRequest();
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
