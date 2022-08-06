using Microsoft.AspNetCore.Mvc;
using it.Data;
using System.Collections;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using it.Areas.Admin.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using NPOI.SS.UserModel;
using System.Text;
using NPOI.XSSF.UserModel;
using System.Text.RegularExpressions;
using System.Globalization;
using NPOI.XWPF.UserModel;

namespace it.Areas.Admin.Controllers
{
    public class ProcedureController : BaseController
    {
        private string _type = "Procedure";
        private UserManager<UserModel> UserManager;
        public ProcedureController(ItContext context, UserManager<UserModel> UserMgr) : base(context)
        {
            ViewData["controller"] = _type;
            UserManager = UserMgr;
        }

        // GET: Admin/Procedure
        public async Task<IActionResult> Index()
        {
            return View();
        }
        // GET: procedure/Create
        public ActionResult Create()
        {

            ViewData["departments"] = _context.DepartmentModel.Where(d => d.deleted_at == null).Select(a => new SelectListItem()
            {
                Value = a.id.ToString(),
                Text = a.name
            }).ToList();
            return View();
        }

        // POST: procedure/Create


        [HttpPost]
        public async Task<IActionResult> Create(ProcedureModel ProcedureModel)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    ProcedureModel.created_at = DateTime.Now;
                    _context.Add(ProcedureModel);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
                catch (DbUpdateException ex)
                {
                    SqlException innerException = ex.InnerException as SqlException;
                    if (innerException != null && (innerException.Number == 2627 || innerException.Number == 2601))
                    {

                        return Ok("Field bị trùng!");
                    }
                    else
                    {
                        throw;
                    }

                }

            }
            return Ok(ModelState);
        }


        // GET: procedure/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            ViewData["departments"] = _context.DepartmentModel.Where(d => d.deleted_at == null).Select(a => new SelectListItem()
            {
                Value = a.id.ToString(),
                Text = a.name
            }).ToList();
            var Object = await _context.ProcedureModel.FindAsync(id);
            var workgroups_list_id = _context.WorkgroupProcedureModel.Where(d => d.procedure_id == id).Select(a => a.workgroup_id).ToList();
            var employee_list_id = _context.EmployeeWorkgroupModel.Where(d => workgroups_list_id.Contains(d.workgroup_id)).Select(a => a.employee_id).Distinct().ToList();
            var employee = _context.EmployeeModel.Where(d => employee_list_id.Contains(d.id) && d.deleted_at == null).ToList();
            var version = _context.ProcedureVersionModel.Where(d => d.procedure_id == id && d.deleted_at == null).Select(a => a.id).ToList();
            //var daotao = _context.RecordTrainModel.Where()
            foreach (var e in employee)
            {
                e.trains = _context.RecordTrainModel.Where(d => d.deleted_at == null && d.employee_id == e.id && version.Contains(d.procedure_version_id)).Include(d => d.ProcedureVersion).ToList();
            }
            ViewData["employee"] = employee;
            //return Ok(employee);

            return View(Object);
        }
        // POST: Admin/Procedure/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Edit(int id, ProcedureModel ProcedureModel)
        {

            var ProcedureModel_old = await _context.ProcedureModel.FindAsync(id);
            ProcedureModel_old.updated_at = DateTime.Now;
            ProcedureModel_old.type = ProcedureModel.type;
            ProcedureModel_old.frequency = ProcedureModel.frequency;
            //ProcedureModel_old.type = 
            //return Ok(ProcedureModel);
            foreach (string key in HttpContext.Request.Form.Keys)
            {
                var prop = ProcedureModel_old.GetType().GetProperty(key);
                var prop_new = ProcedureModel.GetType().GetProperty(key);
                //if (key == "keyword")
                //{
                //    var type1 = "";
                //}
                if (prop != null)
                {
                    string temp = Request.Form[key].FirstOrDefault();
                    var value = prop.GetValue(ProcedureModel_old, null);
                    var value_new = prop.GetValue(ProcedureModel, null);
                    if (value == null && value_new == null)
                        continue;

                    var type = value != null ? value.GetType() : value_new.GetType();


                    if (type == typeof(int))
                    {
                        int val = Int32.Parse(temp);
                        prop.SetValue(ProcedureModel_old, val);
                    }
                    else if (type == typeof(string))
                    {
                        prop.SetValue(ProcedureModel_old, temp);
                    }
                    else if (type == typeof(DateTime))
                    {
                        if (string.IsNullOrEmpty(temp))
                        {
                            prop.SetValue(ProcedureModel_old, null);
                        }
                        else
                        {
                            DateTime.TryParse(temp, out DateTime val);
                            prop.SetValue(ProcedureModel_old, val);
                        }
                    }
                    else if (type == typeof(List<string>))
                    {
                        var list = Request.Form[key].ToList();
                        prop.SetValue(ProcedureModel_old, list);
                    }
                }
            }
            //return Ok(ProcedureModel_old);
            _context.Update(ProcedureModel_old);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
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

            var customerData = (from tempcustomer in _context.ProcedureModel.Where(d => d.deleted_at == null) select tempcustomer);
            int recordsTotal = customerData.Count();
            if (!string.IsNullOrEmpty(searchValue))
            {
                customerData = customerData.Where(m => m.name.Contains(searchValue) || m.code.Contains(searchValue));
            }
            int recordsFiltered = customerData.Count();
            var datapost = customerData.Skip(skip).Take(pageSize).ToList();
            var data = new ArrayList();
            foreach (var record in datapost)
            {
                var data1 = new
                {
                    action = "<div class='btn-group'><a href='/admin/procedure/delete/" + record.id + "' class='btn btn-danger btn-sm' title='Xóa?' data-type='confirm'>'"
                        + "<i class='fas fa-trash-alt'>"
                        + "</i>"
                        + "</a></div>",
                    id = "<a href='/admin/procedure/edit/" + record.id + "'><i class='fas fa-pencil-alt mr-2'></i> " + record.id + "</a>",
                    name = record.name,
                    code = record.code,
                };
                data.Add(data1);
            }
            var jsonData = new { draw = draw, recordsFiltered = recordsFiltered, recordsTotal = recordsTotal, data = data };
            return Json(jsonData);
        }
        [HttpPost]
        public async Task<JsonResult> TableVersion()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var procedure_id_string = Request.Form["procedure_id"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int procedure_id = procedure_id_string != null ? Convert.ToInt32(procedure_id_string) : 0;
            var customerData = (from tempcustomer in _context.ProcedureVersionModel.Where(d => d.deleted_at == null && d.procedure_id == procedure_id) select tempcustomer);
            int recordsTotal = customerData.Count();
            if (!string.IsNullOrEmpty(searchValue))
            {
                customerData = customerData.Where(m => m.name.Contains(searchValue) || m.code.Contains(searchValue) || m.version.ToString() == searchValue);
            }
            int recordsFiltered = customerData.Count();
            var datapost = customerData.Skip(skip).Take(pageSize).ToList();
            var data = new ArrayList();
            foreach (var record in datapost)
            {
                var need_train = "<span class='badge badge-md badge-soft-danger'>Không</span>";
                if (record.need_train)
                {
                    need_train = "<span class='badge badge-md badge-soft-success'>Có</span>";
                }
                DateTime date = DateTime.Now;

                string date_approve = "";
                if (record.date_approve != null)
                {
                    date = (DateTime)record.date_approve;
                    date_approve = date.ToString("yyyy-MM-dd");
                }
                string date_effect = "";
                if (record.date_effect != null)
                {
                    date = (DateTime)record.date_effect;
                    date_effect = date.ToString("yyyy-MM-dd");
                }
                var data1 = new
                {
                    action = "<div class='btn-group'><a href='/admin/procedure/DeleteVersion/" + record.id + "' class='btn btn-danger btn-sm' title='Xóa?' data-type='confirm'>'"
                        + "<i class='fas fa-trash-alt'>"
                        + "</i>"
                        + "</a></div>",
                    id = "<a href='#' class='editSop' data-id='" + record.id + "' data-version='" + record.version + "' data-date_effect='" + date_effect + "' data-date_approve='" + date_approve + "' data-need_train='" + record.need_train + "' data-toggle='modal' data-animation='bounce' data-target='.modal-sop'><i class='fas fa-pencil-alt mr-2'></i> " + record.id + "</a>",
                    name = record.name,
                    code = record.code,
                    date_approve = date_approve,
                    date_effect = date_effect,
                    need_train = need_train
                };
                data.Add(data1);
            }
            var jsonData = new { draw = draw, recordsFiltered = recordsFiltered, recordsTotal = recordsTotal, data = data };
            return Json(jsonData);
        }
        [HttpPost]
        public async Task<JsonResult> createUpdateVersion(ProcedureVersionModel ProcedureVersionModel)
        {

            try
            {
                var ProcedureModel = await _context.ProcedureModel.FindAsync(ProcedureVersionModel.procedure_id);

                if (ProcedureVersionModel.id > 0)
                {
                    var ProcedureVersionModel_old = await _context.ProcedureVersionModel.FindAsync(ProcedureVersionModel.id);

                    ProcedureVersionModel_old.name = ProcedureModel.name;
                    ProcedureVersionModel_old.code = ProcedureModel.code + "." + ProcedureVersionModel.version;
                    ProcedureVersionModel_old.updated_at = DateTime.Now;
                    //return Ok(ProcedureVersionModel);
                    foreach (string key in HttpContext.Request.Form.Keys)
                    {
                        var prop = ProcedureVersionModel_old.GetType().GetProperty(key);
                        var prop_new = ProcedureVersionModel.GetType().GetProperty(key);
                        //if (key == "keyword")
                        //{
                        //    var type1 = "";
                        //}
                        if (prop != null)
                        {
                            string temp = Request.Form[key].LastOrDefault();
                            var value = prop.GetValue(ProcedureVersionModel_old, null);
                            var value_new = prop.GetValue(ProcedureVersionModel, null);
                            if (value == null && value_new == null)
                                continue;

                            var type = value != null ? value.GetType() : value_new.GetType();


                            if (type == typeof(int))
                            {
                                int val = Int32.Parse(temp);
                                prop.SetValue(ProcedureVersionModel_old, val);
                            }
                            else if (type == typeof(string))
                            {
                                prop.SetValue(ProcedureVersionModel_old, temp);
                            }
                            else if (type == typeof(Boolean))
                            {
                                Boolean val;
                                Boolean.TryParse(temp, out val);
                                prop.SetValue(ProcedureVersionModel_old, val);
                            }
                            else if (type == typeof(DateTime))
                            {
                                if (string.IsNullOrEmpty(temp))
                                {
                                    prop.SetValue(ProcedureVersionModel_old, null);
                                }
                                else
                                {
                                    DateTime.TryParse(temp, out DateTime val);
                                    prop.SetValue(ProcedureVersionModel_old, val);
                                }
                            }
                        }
                    }
                    //return Ok(ProcedureVersionModel_old);
                    _context.Update(ProcedureVersionModel_old);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    ProcedureVersionModel.name = ProcedureModel.name;
                    ProcedureVersionModel.code = ProcedureModel.code + "." + ProcedureVersionModel.version;
                    ProcedureVersionModel.created_at = DateTime.Now;
                    _context.Add(ProcedureVersionModel);
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true });
            }
            catch (DbUpdateException ex)
            {
                SqlException innerException = ex.InnerException as SqlException;
                if (innerException != null && (innerException.Number == 2627 || innerException.Number == 2601))
                {

                    return Json(new { success = false, messsage = "" });
                }
                else
                {
                    throw;
                }

            }
        }


        // GET: Admin/Procedure/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            if (_context.ProcedureModel == null)
            {
                return Problem("Entity set 'ItContext.ProcedureModel'  is null.");
            }
            var ProcedureModel = await _context.ProcedureModel.FindAsync(id);
            if (ProcedureModel != null)
            {
                ProcedureModel.deleted_at = DateTime.Now;
                _context.ProcedureModel.Update(ProcedureModel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Procedure/DeleteVersion/5
        public async Task<IActionResult> DeleteVersion(int id)
        {
            if (_context.ProcedureVersionModel == null)
            {
                return Problem("Entity set 'ItContext.ProcedureVersionModel'  is null.");
            }
            var ProcedureVersionModel = await _context.ProcedureVersionModel.FindAsync(id);
            if (ProcedureVersionModel != null)
            {
                ProcedureVersionModel.deleted_at = DateTime.Now;
                _context.ProcedureVersionModel.Update(ProcedureVersionModel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> sop()
        {
            return Ok();
            FileStream fs = new FileStream("./private/excel/sop/List of SOPs database.xlsx", FileMode.Open);

            // Khởi tạo workbook để đọc
            XSSFWorkbook wb = new XSSFWorkbook(fs);
            int numberofsheet = wb.NumberOfSheets;
            Console.OutputEncoding = Encoding.UTF8; // để xuất ra console tv có dấu
            Console.WriteLine("Thông tin từ file Excel");
            for (var i = 1; i < numberofsheet; i++)
            {
                ISheet sheet = wb.GetSheetAt(i);

                // đọc sheet này bắt đầu từ row 2 (0, 1 bỏ vì tiêu đề)

                var lastrow = sheet.LastRowNum;
                // nếu vẫn chưa gặp end thì vẫn lấy data
                Console.WriteLine(lastrow);
                for (int rowIndex = 2; rowIndex < lastrow; rowIndex++)
                {
                    // lấy row hiện tại
                    var nowRow = sheet.GetRow(rowIndex);
                    if (nowRow == null)
                        continue;
                    if (nowRow.Cells.All(d => d.CellType == CellType.Blank)) break;
                    // vì ta dùng 3 cột A, B, C => data của ta sẽ như sau
                    //int numcount = nowRow.Cells.Count;
                    //for(int y = 0;y<numcount - 1 ;y++)
                    var cellSOP = nowRow.GetCell(1);
                    var cellname = nowRow.GetCell(2);
                    var celldate_approve = nowRow.GetCell(3);
                    var celldate_effect = nowRow.GetCell(4);
                    var celldepartment = nowRow.GetCell(13);
                    if (cellname == null || cellSOP == null)
                    {
                        continue;
                    }
                    var SOP = "";
                    var name = "";
                    var date_approve = new DateTime();
                    var date_effect = new DateTime();
                    var department_name = "";
                    if (cellSOP.CellType == CellType.String)
                    {
                        SOP = cellSOP.StringCellValue;
                    }
                    else if (cellSOP.CellType == CellType.Numeric)
                    {
                        SOP = cellSOP.NumericCellValue.ToString();
                    }
                    if (cellname.CellType == CellType.String)
                    {
                        name = cellname.StringCellValue;
                    }
                    else if (cellname.CellType == CellType.Numeric)
                    {
                        name = cellname.NumericCellValue.ToString();
                    }


                    if (celldepartment.CellType == CellType.String)
                    {
                        department_name = celldepartment.StringCellValue;
                    }
                    else if (celldepartment.CellType == CellType.Numeric)
                    {
                        department_name = celldepartment.NumericCellValue.ToString();
                    }
                    else if (celldepartment.CellType == CellType.Formula)
                    {
                        department_name = celldepartment.CellFormula;
                    }
                    if (name == "" || SOP == "")
                    {
                        continue;
                    }
                    if (celldate_approve != null)
                    {
                        if (celldate_approve.CellType == CellType.Numeric)
                        {
                            date_approve = celldate_approve.DateCellValue;
                        }
                        else if (celldate_approve.CellType == CellType.String)
                        {
                            bool success = DateTime.TryParseExact(celldate_approve.StringCellValue, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date_approve);
                        }
                    }
                    if (celldate_effect != null)
                    {
                        if (celldate_effect.CellType == CellType.Numeric)
                        {
                            date_effect = celldate_effect.DateCellValue;
                        }
                        else if (celldate_effect.CellType == CellType.String)
                        {
                            bool success = DateTime.TryParseExact(celldate_effect.StringCellValue, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date_effect);
                        }
                    }
                    var export = SOP.Split(".");
                    var code = "";
                    var version = 0;
                    if (export.Length > 0)
                    {
                        code = export[0];
                        version = Int32.Parse(export[1]);
                    }

                    string pattern = "/\r\n|\n|\r/";
                    Regex rgx = new Regex(pattern);
                    var export_name = rgx.Split(name);
                    var name_vn = name;
                    var name_en = "";
                    if (export_name.Length > 1)
                    {
                        name_vn = export_name[0];
                        name_en = export_name[1];
                    }
                    // Xuất ra thông tin lên màn hình
                    Console.WriteLine("MS: {0} | version: {1} ", code, version);
                    Console.WriteLine("name: {0} ", name_vn);
                    Console.WriteLine("date approve: {0} ", date_approve.ToString("yyyy-MM-dd"));
                    Console.WriteLine("date effect: {0} ", date_effect.ToString("yyyy-MM-dd"));
                    Console.WriteLine("department: {0} ", department_name);

                    ProcedureModel ProcedureModel = new ProcedureModel { code = code, name = name_vn, name_en = name_en, created_at = DateTime.Now };

                    var department = _context.DepartmentModel.Where(d => d.symbol == department_name).FirstOrDefault();
                    if (department != null)
                        ProcedureModel.department_id = department.id;
                    _context.Add(ProcedureModel);
                    _context.SaveChanges();

                    ProcedureVersionModel ProcedureVersionModel = new ProcedureVersionModel
                    {
                        code = SOP,
                        version = version,
                        name = name_vn,
                        name_en = name_en,
                        procedure_id = ProcedureModel.id,
                        date_approve = date_approve,
                        date_effect = date_effect,
                        need_train = true,
                        created_at = DateTime.Now
                    };
                    _context.Add(ProcedureVersionModel);
                    _context.SaveChanges();
                }
            }
            return Ok(1);
        }
        [HttpPost]
        public async Task<JsonResult> get(int id)
        {
            try
            {
                var procedure = _context.ProcedureModel.Where(d => d.id == id).Include(d => d.versions).FirstOrDefault();

                return Json(new { success = true, procedure = procedure });
            }
            catch (Exception ex)
            {
                return Json(new { success = false });
            }

        }
    }
}
