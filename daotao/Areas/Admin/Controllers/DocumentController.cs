using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using it.Areas.Admin.Models;
using it.Data;
using System.Collections;
using Microsoft.AspNetCore.Identity;
using SharpDocx;

namespace it.Areas.Admin.Controllers
{
    public class DocumentController : BaseController
    {
        private UserManager<UserModel> UserManager;
        private string _type = "Document";
        public DocumentController(ItContext context, UserManager<UserModel> UserMgr) : base(context)
        {
            ViewData["controller"] = _type;
            UserManager = UserMgr;
        }

        // GET: Admin/Document
        public IActionResult Index()
        {
            return View();
        }


        // GET: Admin/Document/Create
        public IActionResult CreateJD()
        {
            ViewData["employee"] = _context.EmployeeModel.Where(u => u.deleted_at == null).Select(a => new SelectListItem()
            {
                Value = a.id.ToString(),
                Text = a.FullName
            }).ToList();

            return View();
        }

        // POST: Admin/Document/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> CreateJD(DocumentModel DocumentModel)
        {
            if (ModelState.IsValid)
            {
                DocumentModel.created_at = DateTime.Now;
                _context.Add(DocumentModel);
                await _context.SaveChangesAsync();

                var viewPath = "./private/word/template/JD.docx";
                var documentPath = "./private/word/jd/" + DocumentModel.id + ".docx";
                var model = "";

                var document = DocumentFactory.Create(viewPath, model);


                document.Generate(documentPath);


                return RedirectToAction(nameof(Index));
            }
            return Ok(ModelState);
        }

        // GET: Admin/Document/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            if (_context.DocumentModel == null)
            {
                return Problem("Entity set 'ItContext.DocumentModel'  is null.");
            }
            var DocumentModel = await _context.DocumentModel.FindAsync(id);
            if (DocumentModel != null)
            {
                DocumentModel.deleted_at = DateTime.Now;
                _context.DocumentModel.Update(DocumentModel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<JsonResult> Table()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            var customerData = (from tempcustomer in _context.DocumentModel.Where(u => u.deleted_at == null) select tempcustomer);
            int recordsTotal = customerData.Count();
            customerData = customerData.Where(m => m.deleted_at == null);
            if (!string.IsNullOrEmpty(searchValue))
            {
                customerData = customerData.Where(m => m.title.Contains(searchValue));
            }
            int recordsFiltered = customerData.Count();
            var datapost = customerData.Skip(skip).Take(pageSize).ToList();
            var data = new ArrayList();
            foreach (var record in datapost)
            {
                DateTime date = DateTime.Now;

                string created_at = "";
                if (record.created_at != null)
                {
                    date = (DateTime)record.created_at;
                    created_at = date.ToString("yyyy-MM-dd");
                }
                var file = "<a href='" + record.file_url + "'></a>";
                var data1 = new
                {
                    action = "<div class='btn-group'><a href='/admin/" + _type + "/delete/" + record.id + "' class='btn btn-danger btn-sm' title='Xóa?' data-type='confirm'>'"
                        + "<i class='fas fa-trash-alt'>"
                        + "</i>"
                        + "</a></div>",
                    id = "<a href='#'><i class='fas fa-pencil-alt mr-2'></i> " + record.id + "</a>",
                    name = record.title,
                    file = file,
                    date = created_at
                };
                data.Add(data1);
            }
            var jsonData = new { draw = draw, recordsFiltered = recordsFiltered, recordsTotal = recordsTotal, data = data };
            return Json(jsonData);
        }
    }
}
