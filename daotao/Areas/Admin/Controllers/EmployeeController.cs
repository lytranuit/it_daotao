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
    public class EmployeeController : BaseController
    {
        private UserManager<UserModel> UserManager;
        private string _type = "Employee";
        public EmployeeController(ItContext context, UserManager<UserModel> UserMgr) : base(context)
        {
            ViewData["controller"] = _type;
            UserManager = UserMgr;
        }
        // GET: Admin/Employee
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
            ViewData["workgroups"] = _context.WorkgroupModel.Where(u => u.deleted_at == null && u.department_id == department_id).Select(a => new SelectListItem()
            {
                Value = a.id.ToString(),
                Text = a.name
            }).ToList();
            return View();
        }

        // POST: Admin/Workgroup/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Create(EmployeeModel EmployeeModel, List<int> workgroups, List<int> report_to, List<int> replace_to)
        {
            if (ModelState.IsValid)
            {
                System.Security.Claims.ClaimsPrincipal currentUser = this.User;
                var user = await UserManager.GetUserAsync(currentUser); // Get usee
                EmployeeModel.department_id = (int)user.department_id;
                EmployeeModel.created_at = DateTime.Now;
                _context.Add(EmployeeModel);
                await _context.SaveChangesAsync();
                if (workgroups != null)
                {
                    foreach (var key in workgroups)
                    {
                        EmployeeWorkgroupModel EmployeeWorkgroupModel = new EmployeeWorkgroupModel() { workgroup_id = key, employee_id = EmployeeModel.id };
                        _context.Add(EmployeeWorkgroupModel);
                    }

                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));
            }
            return Ok(ModelState);
        }
        // POST: Admin/Employee/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // GET: Admin/Workgroup/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            if (id == null || _context.EmployeeModel == null)
            {
                return NotFound();
            }

            var EmployeeModel = _context.EmployeeModel
                .Where(d => d.id == id)
                .Include(d => d.workgroups)
                .Include(d => d.replace_to)
                .Include(d => d.report_to)
                .Include(d => d.responsibilities)
                .Include(d => d.job_previous)
                .FirstOrDefault();
            if (EmployeeModel == null)
            {
                return NotFound();
            }

            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user = await UserManager.GetUserAsync(currentUser); // Get usee
            var department_id = (int)user.department_id;
            ViewData["workgroups"] = _context.WorkgroupModel.Where(u => u.deleted_at == null && u.department_id == department_id).Select(a => new SelectListItem()
            {
                Value = a.id.ToString(),
                Text = a.name
            }).ToList();
            ViewData["employees"] = _context.EmployeeModel.Where(u => u.deleted_at == null && u.department_id == department_id && u.id != id).Select(a => new SelectListItem()
            {
                Value = a.id.ToString(),
                Text = a.FullName
            }).ToList();
            ViewData["daotao"] = _context.RecordTrainModel.Where(d => d.employee_id == id && d.deleted_at == null).ToList();
            return View(EmployeeModel);
        }

        // POST: Admin/Workgroup/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Edit(int id, EmployeeModel EmployeeModel, List<int> workgroups, List<int> report_to, List<int> replace_to, List<EmployeeJobPreviousModel>? list_job_previous, List<EmployeeResponsibilitiesModel>? list_responsibilities)
        {

            if (id != EmployeeModel.id)
            {
                return NotFound();
            }


            var EmployeeModel_old = await _context.EmployeeModel.FindAsync(id);
            EmployeeModel_old.updated_at = DateTime.Now;


            foreach (string key in HttpContext.Request.Form.Keys)
            {
                var prop = EmployeeModel_old.GetType().GetProperty(key);
                var prop_new = EmployeeModel.GetType().GetProperty(key);
                //if (key == "keyword")
                //{
                //    var type1 = "";
                //}
                if (prop != null)
                {
                    string temp = Request.Form[key].FirstOrDefault();
                    var value = prop.GetValue(EmployeeModel_old, null);
                    var value_new = prop.GetValue(EmployeeModel, null);
                    if (value == null && value_new == null)
                        continue;

                    var type = value != null ? value.GetType() : value_new.GetType();


                    if (type == typeof(int))
                    {
                        int val = Int32.Parse(temp);
                        prop.SetValue(EmployeeModel_old, val);
                    }
                    else if (type == typeof(string))
                    {
                        prop.SetValue(EmployeeModel_old, temp);
                    }
                    else if (type == typeof(DateTime))
                    {
                        if (string.IsNullOrEmpty(temp))
                        {
                            prop.SetValue(EmployeeModel_old, null);
                        }
                        else
                        {
                            DateTime.TryParse(temp, out DateTime val);
                            prop.SetValue(EmployeeModel_old, val);
                        }
                    }
                }
            }
            _context.Update(EmployeeModel_old);
            await _context.SaveChangesAsync();

            //procedures
            /////XÓA và edit lại
            var EmployeeWorkgroupModel_old = _context.EmployeeWorkgroupModel.Where(d => d.employee_id == id).ToList();
            _context.RemoveRange(EmployeeWorkgroupModel_old);
            await _context.SaveChangesAsync();
            if (workgroups != null && workgroups.Count > 0)
            {
                foreach (var key in workgroups)
                {
                    EmployeeWorkgroupModel EmployeeWorkgroupModel = new EmployeeWorkgroupModel() { employee_id = id, workgroup_id = key };
                    _context.Add(EmployeeWorkgroupModel);
                }

                await _context.SaveChangesAsync();
            }
            ///Replace TO
            /////XÓA và edit lại
            var EmployeeReplaceModel_old = _context.EmployeeReplaceModel.Where(d => d.employee_id == id).ToList();
            _context.RemoveRange(EmployeeReplaceModel_old);
            await _context.SaveChangesAsync();
            if (replace_to != null && replace_to.Count > 0)
            {
                foreach (var key in replace_to)
                {
                    EmployeeReplaceModel EmployeeReplaceModel = new EmployeeReplaceModel() { employee_id = id, employee_replace_id = key };
                    _context.Add(EmployeeReplaceModel);
                }

                await _context.SaveChangesAsync();
            }
            ///Report TO
            /////XÓA và edit lại
            var EmployeeReportModel_old = _context.EmployeeReportModel.Where(d => d.employee_id == id).ToList();
            _context.RemoveRange(EmployeeReportModel_old);
            await _context.SaveChangesAsync();
            if (report_to != null && report_to.Count > 0)
            {
                foreach (var key in report_to)
                {
                    EmployeeReportModel EmployeeReportModel = new EmployeeReportModel() { employee_id = id, employee_report_id = key };
                    _context.Add(EmployeeReportModel);
                }

                await _context.SaveChangesAsync();
            }
            ///Responsibilities
            /////XÓA và edit lại
            var EmployeeResponsibilitiesModel_old = _context.EmployeeResponsibilitiesModel.Where(d => d.employee_id == id).ToList();
            _context.RemoveRange(EmployeeResponsibilitiesModel_old);
            await _context.SaveChangesAsync();
            if (list_responsibilities != null && list_responsibilities.Count > 0)
            {
                foreach (var EmployeeResponsibilitiesModel in list_responsibilities)
                {
                    _context.Add(EmployeeResponsibilitiesModel);
                }

                await _context.SaveChangesAsync();
            }
            ///Job Previous 
            /////XÓA và edit lại
            var EmployeeJobPreviousModel_old = _context.EmployeeJobPreviousModel.Where(d => d.employee_id == id).ToList();
            _context.RemoveRange(EmployeeJobPreviousModel_old);
            await _context.SaveChangesAsync();
            if (list_job_previous != null && list_job_previous.Count > 0)
            {
                foreach (var EmployeeJobPreviousModel in list_job_previous)
                {
                    _context.Add(EmployeeJobPreviousModel);
                }

                await _context.SaveChangesAsync();
            }
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
            var customerData = (from tempcustomer in _context.EmployeeModel.Where(d => d.deleted_at == null && d.department_id == department_id) select tempcustomer);
            int recordsTotal = customerData.Count();
            if (!string.IsNullOrEmpty(searchValue))
            {
                customerData = customerData.Where(m => m.FullName.Contains(searchValue) || m.msnv.Contains(searchValue) || m.email.Contains(searchValue));
            }
            int recordsFiltered = customerData.Count();
            var datapost = customerData.Skip(skip).Take(pageSize).ToList();
            var data = new ArrayList();
            foreach (var record in datapost)
            {
                DateTime date = DateTime.Now;

                string date_work = "";
                if (record.date_work != null)
                {
                    date = (DateTime)record.date_work;
                    date_work = date.ToString("yyyy-MM-dd");
                }
                string birthday = "";
                if (record.birthday != null)
                {
                    date = (DateTime)record.birthday;
                    birthday = date.ToString("yyyy-MM-dd");
                }
                var data1 = new
                {
                    action = "<div class='btn-group'><a href='/admin/" + _type + "/delete/" + record.id + "' class='btn btn-danger btn-sm' title='Xóa?' data-type='confirm'>'"
                        + "<i class='fas fa-trash-alt'>"
                        + "</i>"
                        + "</a></div>",
                    id = "<a href='/admin/employee/edit/" + record.id + "'><i class='fas fa-pencil-alt mr-2'></i> " + record.id + "</a>",
                    name = record.FullName,
                    msnv = record.msnv,
                    email = record.email,
                    birthday = birthday,
                    date_work = date_work,
                    position = record.position,
                };
                data.Add(data1);
            }
            var jsonData = new { draw = draw, recordsFiltered = recordsFiltered, recordsTotal = recordsTotal, data = data };
            return Json(jsonData);
        }


        [HttpPost]
        public async Task<JsonResult> getprocedurebyworkgroups()
        {
            try
            {
                var workgroups = Request.Form["workgroups[]"].ToList();
                var items = _context.WorkgroupProcedureModel
                    .Where(d => workgroups.Contains(d.workgroup_id.ToString()))
                    .Include(d => d.procedure)
                    .ThenInclude(d => d.versions.Where(e => e.deleted_at == null))
                    .Where(d => d.procedure.deleted_at == null)
                    .GroupBy(d => d.procedure_id)
                    .Select(s => new
                    {
                        key = s.Key,
                        value = s.FirstOrDefault()
                    })
                    .ToList();
                return Json(new { success = true, items = items });
            }
            catch (Exception ex)
            {
                return Json(new { success = false });
            }

        }

        [HttpPost]
        public async Task<JsonResult> getprocedurebyemployee()
        {
            try
            {

                var employee_id_string = Request.Form["employee_id"].FirstOrDefault();
                int employee_id = employee_id_string != null ? Convert.ToInt32(employee_id_string) : 0;
                var employee = _context.EmployeeModel.Where(d => d.id == employee_id).Include(d => d.workgroups).FirstOrDefault();
                var workgroups = employee.workgroups.Select(d => d.workgroup_id).ToList();
                var items = _context.WorkgroupProcedureModel
                    .Where(d => workgroups.Contains(d.workgroup_id))
                    .Include(d => d.procedure)
                    .ThenInclude(d => d.versions.Where(e => e.deleted_at == null))
                    .Where(d => d.procedure.deleted_at == null)
                    .GroupBy(d => d.procedure_id)
                    .Select(s => new
                    {
                        key = s.Key,
                        value = s.FirstOrDefault()
                    })
                    .ToList();
                return Json(new { success = true, items = items });
            }
            catch (Exception ex)
            {
                return Json(new { success = false });
            }

        }
    }
}
