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

namespace it.Areas.Admin.Controllers
{
    public class WorkgroupController : BaseController
    {
        private UserManager<UserModel> UserManager;
        private string _type = "Workgroup";
        public WorkgroupController(ItContext context, UserManager<UserModel> UserMgr) : base(context)
        {
            ViewData["controller"] = _type;
            UserManager = UserMgr;
        }

        // GET: Admin/Workgroup
        public IActionResult Index()
        {
            return View();
        }


        // GET: Admin/Workgroup/Create
        public IActionResult Create()
        {
            ViewData["sops"] = _context.ProcedureModel.Where(u => u.deleted_at == null).Select(a => new SelectListItem()
            {
                Value = a.id.ToString(),
                Text = a.code
            }).ToList();
            return View();
        }

        // POST: Admin/Workgroup/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Create(WorkgroupModel WorkgroupModel, List<int>? procedures)
        {
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user = await UserManager.GetUserAsync(currentUser); // Get usee
            WorkgroupModel.department_id = (int)user.department_id;
            WorkgroupModel.created_at = DateTime.Now;
            _context.Add(WorkgroupModel);
            await _context.SaveChangesAsync();
            if (procedures != null)
            {
                foreach (var sop_id in procedures)
                {
                    WorkgroupProcedureModel WorkgroupProcedureModel = new WorkgroupProcedureModel() { workgroup_id = WorkgroupModel.id, procedure_id = sop_id };
                    _context.Add(WorkgroupProcedureModel);
                }

                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
            //return Ok(ModelState);
        }

        // GET: Admin/Workgroup/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null || _context.WorkgroupModel == null)
            {
                return NotFound();
            }

            var WorkgroupModel = _context.WorkgroupModel
                .Where(d => d.id == id)
                .Include(d => d.procedures)
                .FirstOrDefault();
            if (WorkgroupModel == null)
            {
                return NotFound();
            }

            ViewData["sops"] = _context.ProcedureModel.Where(u => u.deleted_at == null).Select(a => new SelectListItem()
            {
                Value = a.id.ToString(),
                Text = a.code
            }).ToList();
            return View(WorkgroupModel);
        }

        // POST: Admin/Workgroup/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Edit(int id, WorkgroupModel WorkgroupModel, List<int>? procedures)
        {

            if (id != WorkgroupModel.id)
            {
                return NotFound();
            }

            var WorkgroupModel_old = await _context.WorkgroupModel.FindAsync(id);
            WorkgroupModel_old.updated_at = DateTime.Now;

            foreach (string key in HttpContext.Request.Form.Keys)
            {
                var prop = WorkgroupModel_old.GetType().GetProperty(key);

                dynamic val = Request.Form[key].FirstOrDefault();

                if (prop != null)
                {
                    prop.SetValue(WorkgroupModel_old, val);
                }
            }
            _context.Update(WorkgroupModel_old);
            await _context.SaveChangesAsync();

            //procedures
            /////XÓA và edit lại
            var WorkgroupProcedureModel_old = _context.WorkgroupProcedureModel.Where(d => d.workgroup_id == id).ToList();
            _context.RemoveRange(WorkgroupProcedureModel_old);
            await _context.SaveChangesAsync();
            if (procedures != null && procedures.Count > 0)
            {
                foreach (var sop_id in procedures)
                {
                    WorkgroupProcedureModel WorkgroupProcedureModel = new WorkgroupProcedureModel() { workgroup_id = id, procedure_id = sop_id };
                    _context.Add(WorkgroupProcedureModel);
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }


        // GET: Admin/Workgroup/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            if (_context.WorkgroupModel == null)
            {
                return Problem("Entity set 'ItContext.WorkgroupModel'  is null.");
            }
            var WorkgroupModel = await _context.WorkgroupModel.FindAsync(id);
            if (WorkgroupModel != null)
            {
                WorkgroupModel.deleted_at = DateTime.Now;
                _context.WorkgroupModel.Update(WorkgroupModel);
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
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user = await UserManager.GetUserAsync(currentUser); // Get usee
            var department_id = (int)user.department_id;
            var customerData = (from tempcustomer in _context.WorkgroupModel.Where(u => u.deleted_at == null && u.department_id == department_id) select tempcustomer);
            int recordsTotal = customerData.Count();
            customerData = customerData.Where(m => m.deleted_at == null);
            if (!string.IsNullOrEmpty(searchValue))
            {
                customerData = customerData.Where(m => m.name.Contains(searchValue));
            }
            int recordsFiltered = customerData.Count();
            customerData = customerData.Include(m => m.department);
            var datapost = customerData.Skip(skip).Take(pageSize).ToList();
            var data = new ArrayList();
            foreach (var record in datapost)
            {
                var department = record.department;

                var data1 = new
                {
                    action = "<div class='btn-group'><a href='/admin/" + _type + "/delete/" + record.id + "' class='btn btn-danger btn-sm' title='Xóa?' data-type='confirm'>'"
                        + "<i class='fas fa-trash-alt'>"
                        + "</i>"
                        + "</a></div>",
                    id = "<a href='/admin/" + _type + "/edit/" + record.id + "'><i class='fas fa-pencil-alt mr-2'></i> " + record.id + "</a>",
                    name = record.name,
                    department = department.name
                };
                data.Add(data1);
            }
            var jsonData = new { draw = draw, recordsFiltered = recordsFiltered, recordsTotal = recordsTotal, data = data };
            return Json(jsonData);
        }
    }
}
