﻿using Padaria.Repository.Entities;
using Padaria.Repository.Repository;
using System.Web.Mvc;
using System.Linq;
namespace Padaria.Web.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        private const int Success = 1, Insuccess = 0;
        private const bool Valid = true, Invalid = false;

        private CustomerRepository _customerRepository = null;
        [HttpGet]
        public ActionResult List() => View(InstantiateCustomerRepository().GetAlls());
        [HttpGet]
        public ActionResult Create() => View();
        [HttpPost]
        public ActionResult Create(Customer customer)
        {
            if (ModelState.IsValid == Valid)
            {
                switch (InstantiateCustomerRepository().Creates(customer))
                {
                    case Success:
                        return RedirectToAction(nameof(this.List));
                }
            }
            return View(customer);
        }
        [HttpGet]
        public ActionResult Delete(int CustomerID)
        {
            var costumerRepository = InstantiateCustomerRepository();
            Customer customer = this.GetCustomer(CustomerID, costumerRepository);
            if (customer.Equals(null))
                return HttpNotFound();
            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(Customer customer)
        {
            if (ModelState.IsValid == Valid)
            {
                switch (InstantiateCustomerRepository().Deletes(customer))
                {
                    case Success:
                        return RedirectToAction(nameof(this.List));                 
                }
            }
            return View(customer);
        }
        [HttpGet]
        public ActionResult Details(int customerID)
        {
            var customerRepository = InstantiateCustomerRepository();
            Customer customer = this.GetCustomer(customerID, customerRepository);
            return View(customer);
        }
        [HttpGet]
        public ActionResult Edit(int customerID)
        {
            var customerRepository = InstantiateCustomerRepository();
            Customer customer = this.GetCustomer(customerID, customerRepository);
            return View(customer);
        }
        [HttpPost]
        public ActionResult Edit(Customer customer)
        {
            if (ModelState.IsValid == Valid)
            {
                switch (InstantiateCustomerRepository().Updates(customer))
                {
                    case Success:
                        return RedirectToAction(nameof(this.List));
                }
            }
            return View(customer);
        }
        [HttpGet]
        //[ChildActionOnly]
        public PartialViewResult GetCustomerByName(string customerName)
        {
             return PartialView(InstantiateCustomerRepository().DataContext().Where(c=>c.Name.Contains(customerName)).ToList<Customer>());
        }
        private Customer GetCustomer(int CustomerID, CustomerRepository costumerRepository) 
            => costumerRepository?.GetByIDs(CustomerID);
       
        private CustomerRepository InstantiateCustomerRepository() 
            => _customerRepository = new CustomerRepository();


    }
}