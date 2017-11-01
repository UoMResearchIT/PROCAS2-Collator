using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using PROCAS2.Data;
using PROCAS2.Data.Entities;
using PROCAS2.Services.Utility;

namespace PROCAS2.Controllers
{
    public class UserAdminController : Controller
    {
        
        private IPROCAS2UserManager _procas2UserManager;

        public UserAdminController(IPROCAS2UserManager procas2UserManager)
        {
            _procas2UserManager = procas2UserManager;
        }

        // GET: UserAdmin
        public ActionResult Index()
        {

            // TODO: Add in user listing datatable

            return View();
        }

        // GET: UserAdmin/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: UserAdmin/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: UserAdmin/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: UserAdmin/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: UserAdmin/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: UserAdmin/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: UserAdmin/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
