using Microsoft.AspNetCore.Mvc;
using it.Data;
using System.Collections;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Identity;
using it.Areas.Admin.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace it.Areas.Admin.Controllers
{
    public class RecordController : BaseController
    {
        private UserManager<UserModel> UserManager;
        private string _type = "Record";
        public RecordController(ItContext context, UserManager<UserModel> UserMgr) : base(context)
        {
            ViewData["controller"] = _type;
            UserManager = UserMgr;
        }
        // GET: Admin/Record
        public async Task<IActionResult> Index()
        {
            return View();
        }


        // GET: Admin/Workgroup/Create
        public async Task<IActionResult> Create()
        {
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user = await UserManager.GetUserAsync(currentUser); // Get usee
            var department_id = (int)user.department_id;

            ViewData["procedure_versions"] = _context.ProcedureVersionModel.Where(u => u.deleted_at == null).Select(a => new SelectListItem()
            {
                Value = a.id.ToString(),
                Text = a.code + " - " + a.name
            }).ToList();

            ViewData["employee"] = _context.EmployeeModel.Where(u => u.deleted_at == null).Select(a => new SelectListItem()
            {
                Value = a.id.ToString(),
                Text = a.msnv + " - " + a.FullName
            }).ToList();

            return View();
        }

        // POST: Admin/Workgroup/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Create(RecordModel RecordModel, List<int> procedure_versions, List<int> employee)
        {
            if (ModelState.IsValid)
            {
                System.Security.Claims.ClaimsPrincipal currentUser = this.User;
                var user = await UserManager.GetUserAsync(currentUser); // Get usee
                RecordModel.department_id = (int)user.department_id;
                RecordModel.created_at = DateTime.Now;
                _context.Add(RecordModel);
                await _context.SaveChangesAsync();
                if (procedure_versions != null && employee != null)
                {
                    foreach (var key in procedure_versions)
                    {
                        foreach (var key1 in employee)
                        {
                            RecordTrainModel RecordTrainModel = new RecordTrainModel() { record_id = RecordModel.id, employee_id = key1, procedure_version_id = key, is_pass = true, created_at = DateTime.Now };
                            _context.Add(RecordTrainModel);
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));
            }
            return Ok(ModelState);
        }
        // POST: Admin/Record/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // GET: Admin/Workgroup/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            if (id == null || _context.RecordModel == null)
            {
                return NotFound();
            }

            var RecordModel = _context.RecordModel
                .Where(d => d.id == id)
                .Include(d => d.trains)
                .FirstOrDefault();
            if (RecordModel == null)
            {
                return NotFound();
            }

            ViewData["procedure_versions"] = _context.ProcedureVersionModel.Where(u => u.deleted_at == null).Select(a => new SelectListItem()
            {
                Value = a.id.ToString(),
                Text = a.code + " - " + a.name
            }).ToList();

            ViewData["employee"] = _context.EmployeeModel.Where(u => u.deleted_at == null).Select(a => new SelectListItem()
            {
                Value = a.id.ToString(),
                Text = a.msnv + " - " + a.FullName
            }).ToList();
            return View(RecordModel);
        }

        // POST: Admin/Workgroup/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Edit(int id, RecordModel RecordModel, List<int> procedure_versions, List<int> employee)
        {

            if (id != RecordModel.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {

                    var RecordModel_old = await _context.RecordModel.FindAsync(id);
                    RecordModel_old.updated_at = DateTime.Now;


                    foreach (string key in HttpContext.Request.Form.Keys)
                    {
                        var prop = RecordModel_old.GetType().GetProperty(key);
                        var prop_new = RecordModel.GetType().GetProperty(key);
                        //if (key == "keyword")
                        //{
                        //    var type1 = "";
                        //}
                        if (prop != null)
                        {
                            string temp = Request.Form[key].FirstOrDefault();
                            var value = prop.GetValue(RecordModel_old, null);
                            var value_new = prop.GetValue(RecordModel, null);
                            if (value == null && value_new == null)
                                continue;

                            var type = value != null ? value.GetType() : value_new.GetType();


                            if (type == typeof(int))
                            {
                                int val = Int32.Parse(temp);
                                prop.SetValue(RecordModel_old, val);
                            }
                            else if (type == typeof(string))
                            {
                                prop.SetValue(RecordModel_old, temp);
                            }
                            else if (type == typeof(DateTime))
                            {
                                if (string.IsNullOrEmpty(temp))
                                {
                                    prop.SetValue(RecordModel_old, null);
                                }
                                else
                                {
                                    DateTime.TryParse(temp, out DateTime val);
                                    prop.SetValue(RecordModel_old, val);
                                }
                            }
                        }
                    }
                    _context.Update(RecordModel_old);
                    await _context.SaveChangesAsync();
                    ///Xóa
                    var RecordTrainModel_old = _context.RecordTrainModel.Where(d => d.record_id == id).ToList();
                    _context.RemoveRange(RecordTrainModel_old);
                    await _context.SaveChangesAsync();
                    if (procedure_versions != null && employee != null)
                    {
                        foreach (var key in procedure_versions)
                        {
                            foreach (var key1 in employee)
                            {
                                RecordTrainModel RecordTrainModel = new RecordTrainModel() { record_id = RecordModel.id, employee_id = key1, procedure_version_id = key, is_pass = true, created_at = DateTime.Now };
                                _context.Add(RecordTrainModel);
                            }
                        }

                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {

                }
                return RedirectToAction(nameof(Index));
            }
            return View(RecordModel);
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
            var customerData = (from tempcustomer in _context.RecordModel.Where(d => d.deleted_at == null && d.department_id == department_id) select tempcustomer);
            int recordsTotal = customerData.Count();
            if (!string.IsNullOrEmpty(searchValue))
            {
                customerData = customerData.Where(m => m.name.Contains(searchValue));
            }
            int recordsFiltered = customerData.Count();
            var datapost = customerData.Skip(skip).Take(pageSize).ToList();
            var data = new ArrayList();
            foreach (var record in datapost)
            {
                DateTime date_1 = DateTime.Now;

                string date = "";
                if (record.date != null)
                {
                    date_1 = (DateTime)record.date;
                    date = date_1.ToString("yyyy-MM-dd");
                }
                var data1 = new
                {
                    action = "<div class='btn-group'><a href='/admin/" + _type + "/delete/" + record.id + "' class='btn btn-danger btn-sm' title='Xóa?' data-type='confirm'>'"
                        + "<i class='fas fa-trash-alt'>"
                        + "</i>"
                        + "</a></div>",
                    id = "<a href='/admin/Record/edit/" + record.id + "'><i class='fas fa-pencil-alt mr-2'></i> " + record.id + "</a>",
                    name = record.name,
                    date = date
                };
                data.Add(data1);
            }
            var jsonData = new { draw = draw, recordsFiltered = recordsFiltered, recordsTotal = recordsTotal, data = data };
            return Json(jsonData);
        }
    }
}
