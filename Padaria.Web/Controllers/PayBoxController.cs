﻿using Padaria.Repository.Entities;
using Padaria.Repository.Repository;
using Padaria.Web.Constants;
using Padaria.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;


namespace Padaria.Web.Controllers
{
    [Authorize]
    public class PayBoxController : Controller
    {

        public PayBoxRepository _payBoxRepository { get; } = new PayBoxRepository();
        public ProductRepository _productRepository { get; } = new ProductRepository();
        private SaleWithActiveControlsRepository _saleWithActiveControlsRrepository { get; } = new SaleWithActiveControlsRepository();
        private ControlRepository _controlRepository { get; } = new ControlRepository();
        private UserRepository _userRepository { get; } = new UserRepository();
        private SaleRepository _saleRepository { get; } = new SaleRepository();
        private static List<InsertProductViewModel> list = new List<InsertProductViewModel>();
        private TypeOfPaymentRepository _typeOfPaymentRepository { get; } = new TypeOfPaymentRepository();
        private StockRepository _stockRepository { get; } = new StockRepository();

        [ChildActionOnly]
        public PartialViewResult GetValue()
        {
            return PartialView(_payBoxRepository.GetValue());
        }
        [HttpGet]
        public ActionResult PayBox() => View(new PayBoxViewModel
        {
            TypeOfPayment = GetTypeOfPayment()
        });

        [HttpGet]
        public JsonResult GetFullValue()
        {
            var fullSale = list.Select(c => c.FullSale);
            return Json(new { Result = fullSale.Sum() }, JsonRequestBehavior.AllowGet);
        }
        private Sale GetFullSale()
        {
            var fullIncome = list.Select(c => c.FullIncome);
            var fullSale = list.Select(c => c.FullSale);
            return new Sale
            {
                Date = DateTime.Now,
                FullIncome = fullIncome.Sum(),
                FullSale = fullSale.Sum(),
                UserID = GetCurrentUser(name:Login.User_Name)

            };


        }

        private int GetCurrentUser(string name) => _userRepository.GetUserIDWithName(name);
       

        [HttpGet]
        public ActionResult InsertProduct(InsertProductViewModel insertProductViewModel)
        {
            Product product = _productRepository.DataContext.Product.FirstOrDefault(c => c.Code == insertProductViewModel.Product.Code);
            if (product != null)
            {
                insertProductViewModel.Product = product;
                insertProductViewModel.FullSale = product.SalePrice * insertProductViewModel.Quantity;
                insertProductViewModel.FullIncome = product.SalePrice * insertProductViewModel.Quantity - (product.PurchasePrice * insertProductViewModel.Quantity);
                list.Add(insertProductViewModel);
                return PartialView(list);
            }
            return PartialView("Alert");
        }
        [HttpGet]
        public ActionResult GetControlItens(InsertProductViewModel insertProductViewModel)
        {

            Controls control = _controlRepository.DataContext.FirstOrDefault(c => c.Code == insertProductViewModel.ControlsCode);
            if (control != null)
            {
                foreach (var item in _saleWithActiveControlsRrepository.DataContext.Where(c => c.Controls.Code == control.Code))
                {
                    InsertProductViewModel productViewModel = new InsertProductViewModel();
                    {
                        productViewModel.ControlsCode = control.Code;
                        productViewModel.Product = _productRepository.GetByIds(item.ProductID);
                        productViewModel.FullSale = item.FullPrice;
                        productViewModel.Quantity = item.Quantity;
                        productViewModel.FullIncome = item.FullPrice - (productViewModel.Product.PurchasePrice * item.Quantity);



                    }
                    list.Add(productViewModel);

                }
                return PartialView(viewName: nameof(this.InsertProduct), model: list);
            }
            return PartialView("Alert");

        }
        [HttpGet]
        public JsonResult SaveSale(Sale sale)
        {
            int typeOfPaymentID = sale.TypeOfPaymentID;
            sale = GetFullSale();
            sale.TypeOfPaymentID = typeOfPaymentID;
            _saleRepository.Creates(sale);
            return Json(new {SaleID = sale.SaleID },JsonRequestBehavior.AllowGet);
            
        }
        [HttpGet]
        public ActionResult FinishSale(int typeOfPayment)
        {
            RemoveStock();
            PayBox pB = GetCurrentPayBox();
            decimal? fullValue = GetCurrentValuePlusCurrentValuePaid(typeOfPayment, pB);
            var payBox = new PayBox
            {
                PayBoxID = pB == null ? 0 : pB.PayBoxID,
                UserID = GetCurrentUser(Login.User_Name),
                Value = fullValue

            };
            int result = payBox.PayBoxID == 0 ? _payBoxRepository.Creates(GetCurrentUser(Login.User_Name)) : _payBoxRepository.Update(payBox);            
            list.Clear();
            return PartialView("Alert");
        }

        private decimal? GetCurrentValuePlusCurrentValuePaid(int typeOfPayment, PayBox pB)
        {
            return (GerValueWhenPaidWithMoney(typeOfPayment: typeOfPayment) + pB.Value);
        }

        private PayBox GetCurrentPayBox()
        {
            return _payBoxRepository._dataContext.PayBox.FirstOrDefault();
        }

        private void RemoveStock()
        {
            foreach (var prod in list)
            {
                Stock stock = _stockRepository.GetByIDs(prod.Product.ProductID);
                stock.Quantity = prod.Quantity;
                _stockRepository.RemoveStock(stock);
            }
        }

        private decimal GerValueWhenPaidWithMoney(int typeOfPayment)
        {
             string type = _typeOfPaymentRepository.GetByIDs(typeOfPayment).Type;
            if (type == TypeOfPaymentEnum.Money.ToString())
            {
                var value = list.Select(c => c.FullSale);
                return value.Sum();
            }
            return 0;
           
        }

        private SelectList GetTypeOfPayment(int typeOfRegistrationID = 0)
        {
            return new SelectList(items: _payBoxRepository._dataContext.TypeOfPayment.ToList()
                                 , dataTextField: "Type"
                                 , dataValueField: "TypeOfPaymentID"
                                 , selectedValue: typeOfRegistrationID);
        }
        //protected override void Dispose(bool disposing)
        //{
        //    //if (disposing == fal)
        //    //{

        //    //}
        //    //if (disposing)
        //    //{
        //    //    _payBoxRepository._dataContext.Dispose();
        //    //    _productRepository.DataContext.Dispose();
        //    //    //list.Clear();
        //    //}
        //    //base.Dispose(disposing);
        //}
    }
}