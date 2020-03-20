using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MNepalProject.DAL;
using MNepalProject.Models;
using MNepalProject.Migrations;
using MNepalProject.IRepository;
using MNepalProject.Repository;

namespace MNepalProject.Controllers
{
    public class MNBankTablesController : Controller
    {
        private MNepalDBContext db = new MNepalDBContext();

        private IMNBankTableRepository mnBankTableRepository;

        public MNBankTablesController()
        {
            this.mnBankTableRepository = new MNBankTableRepository(new PetaPoco.Database(MNepalDBConnectionStringProvider.GetConnection()));
        }

        // GET: Banks
        public ActionResult Index()
        {
            var dataContext = new PetaPoco.Database(MNepalDBConnectionStringProvider.GetConnection());
            var banks = dataContext.Query<MNBankTable>("Select * from MNBankTable");
            return View(banks);
        }

        // GET: Banks/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var dataContext = new PetaPoco.Database(MNepalDBConnectionStringProvider.GetConnection());
            MNBankTable bank = dataContext.Single<MNBankTable>("Select * from MNBankTable where BankCode=@0", id);
            
            if (bank == null)
            {
                return HttpNotFound();
            }
            return View(bank);
        }

        // GET: Banks/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Banks/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(MNBankTable bank)
        {
            if (ModelState.IsValid)
            {
                var dataContext = new PetaPoco.Database(MNepalDBConnectionStringProvider.GetConnection());
                dataContext.Insert("MNBankTable", "BankCode", false, bank);
                return RedirectToAction("Index");
            }

            return View(bank);
        }
        // GET: Banks/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var dataContext = new PetaPoco.Database(MNepalDBConnectionStringProvider.GetConnection());
            MNBankTable bank = dataContext.Single<MNBankTable>("Select * from MNBankTable where BankCode=@0", id);
           
            if (bank == null)
            {
                return HttpNotFound();
            }
            return View(bank);
        }

        // POST: Banks/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(MNBankTable bank)
        {
            if (ModelState.IsValid)
            {
                var dataContext = new PetaPoco.Database(MNepalDBConnectionStringProvider.GetConnection());
                dataContext.Update(bank);
                return RedirectToAction("Index");
            }
            return View(bank);
        }

        // GET: Banks/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            var dataContext = new PetaPoco.Database(MNepalDBConnectionStringProvider.GetConnection());
            MNBankTable bank = dataContext.Single<MNBankTable>("Select * from MNBankTable where BankCode=@0", id);
            dataContext.Delete(bank);
            if (bank == null)
            {
                return HttpNotFound();
            }
            return View(bank);
        }

        // POST: Banks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var dataContext = new PetaPoco.Database(MNepalDBConnectionStringProvider.GetConnection());
            MNBankTable bank = dataContext.Single<MNBankTable>("Select * from MNBankTable where BankCode=@0", id);
            dataContext.Delete(bank);
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public MNBankTable GetBankName(string bankCode)
        {
            MNBankTable mnbankInfo = new MNBankTable();
            mnbankInfo = mnBankTableRepository.GetBankDetails(bankCode);
            if (mnbankInfo.BankName != "")
            {
                return mnbankInfo;
            }
            else
            {
                return mnbankInfo;
            }

        }


        public MNBankTable GetBankAcc(string clientCode)
        {
            MNBankTable mnbankInfo = new MNBankTable();
            mnbankInfo = mnBankTableRepository.GetBankAcDetails(clientCode);
            if (mnbankInfo.BankAccountNumber != "")
            {
                return mnbankInfo;
            }
            else
            {
                return mnbankInfo;
            }

        }

    }
}
