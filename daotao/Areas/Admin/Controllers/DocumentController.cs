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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharpDocx;
using it.Areas.Admin.Models.Docs;
using System.Text.Json;
using System.Text.Json.Serialization;
using Spire.Xls;

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
        public IActionResult Index(string type)
        {
            if (type == null)
                return NotFound();
            ViewData["type"] = type;
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
        public async Task<IActionResult> CreateJD(DocumentModel DocumentModel, int employee_id, int version)
        {
            if (ModelState.IsValid)
            {
                EmployeeModel EmployeeModel = _context.EmployeeModel.Where(d => d.id == employee_id)
                    .Include(d => d.department)
                    .Include(d => d.replace_to)
                    //.ThenInclude(d => d.employee_replace)
                    .Include(d => d.report_to)
                    //.ThenInclude(d => d.employee_report)
                    .Include(d => d.job_previous)
                    .Include(d => d.responsibilities)
                    .Include(d => d.workgroups)
                    .ThenInclude(d => d.workgroup)
                    .FirstOrDefault();
                if (EmployeeModel == null)
                {
                    return Ok("Failed");
                }
                //return Ok(EmployeeModel);
                var settings = new Newtonsoft.Json.JsonSerializerSettings();
                // This tells your serializer that multiple references are okay.
                settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                string json = JsonConvert.SerializeObject(EmployeeModel, settings);

                JObject obj = JObject.Parse(json);
                obj["version"] = version;
                DocumentModel.title = "Bản mô tả công việc - " + EmployeeModel.FullName + " - Ấn bản " + version + "";
                DocumentModel.type_id = 1;
                DocumentModel.created_at = DateTime.Now;
                DocumentModel.E_data = obj;
                _context.Add(DocumentModel);
                await _context.SaveChangesAsync();

                var viewPath = "private/word/template/JD.docx";
                var documentPath = "/private/word/jd/" + DocumentModel.id + ".docx";

                var model = new JdDoc();
                model.employee_id = employee_id;
                model.version = version;
                model.employee = EmployeeModel;

                var report_to = EmployeeModel.report_to.Select(d => d.employee_report_id).ToList();
                var replace_to = EmployeeModel.replace_to.Select(d => d.employee_replace_id).ToList();
                model.report_to = _context.EmployeeModel.Where(d => report_to.Contains(d.id)).ToList();
                model.replace_to = _context.EmployeeModel.Where(d => replace_to.Contains(d.id)).ToList();

                //return Ok(model);
                try
                {
                    var document = DocumentFactory.Create(viewPath, model);
                    document.Generate("." + documentPath);
                }
                catch (SharpDocxCompilationException ex)
                {
                    return Ok(ex.Errors);
                }




                DocumentModel.file_url = documentPath;
                _context.Update(DocumentModel);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index), new { type = "jd" });
            }
            return Ok(ModelState);
        }
        // GET: Admin/Document/Create
        public IActionResult CreateTP()
        {
            ViewData["employee"] = _context.EmployeeModel.Where(u => u.deleted_at == null).Select(a => new SelectListItem()
            {
                Value = a.id.ToString(),
                Text = a.FullName
            }).ToList();

            ViewData["sops"] = _context.ProcedureModel.Where(u => u.deleted_at == null).Select(a => new SelectListItem()
            {
                Value = a.id.ToString(),
                Text = a.code + " - " + a.name
            }).ToList();
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateTP(int employee_id, int type, List<int> procedure_version_id, List<int> type_train, List<DateTime> date_theory, List<DateTime> date_reality)
        {
            //return Ok(ModelState);
            EmployeeModel EmployeeModel = _context.EmployeeModel.Where(d => d.id == employee_id)
                    .Include(d => d.department)
                    .FirstOrDefault();
            if (EmployeeModel == null)
            {
                return Ok("Failed");
            }
            //return Ok(EmployeeModel);
            var settings = new Newtonsoft.Json.JsonSerializerSettings();
            // This tells your serializer that multiple references are okay.
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            string json = JsonConvert.SerializeObject(EmployeeModel, settings);

            JObject obj = JObject.Parse(json);
            obj["type"] = type;
            obj["procedure_version_id"] = System.Text.Json.JsonSerializer.Serialize(procedure_version_id);
            obj["type_train"] = System.Text.Json.JsonSerializer.Serialize(type_train);
            obj["date_theory"] = System.Text.Json.JsonSerializer.Serialize(date_theory);
            obj["date_reality"] = System.Text.Json.JsonSerializer.Serialize(date_reality);
            DocumentModel DocumentModel = new DocumentModel();
            DocumentModel.title = "Kế hoạch huấn luyện nhân viên - " + EmployeeModel.FullName;
            DocumentModel.type_id = 2;
            DocumentModel.created_at = DateTime.Now;
            DocumentModel.E_data = obj;
            _context.Add(DocumentModel);
            await _context.SaveChangesAsync();

            var viewPath = "private/excel/template/010006.13_07 - Employee training plan_ Effec. 20.06.2021.xlsx";
            var documentPath = "/private/excel/tp/" + DocumentModel.id + ".xlsx";
            Workbook workbook = new Workbook();
            workbook.LoadFromFile(viewPath);
            Worksheet sheet = workbook.Worksheets[0];
            sheet.Range["C4"].Text = EmployeeModel.FullName;
            var type_text = "";
            var type_text_en = "";
            switch (type)
            {
                case 1:
                    type_text = "Nhân viên mới";
                    type_text_en = "New employee";
                    break;
                case 2:
                    type_text = "Trở lại sau thời gian dài vắng mặt";
                    type_text_en = "Return after longer absence";
                    break;
                case 3:
                    type_text = "Thay đổi vị trí công việc";
                    type_text_en = "Change of Job Position";
                    break;
            }
            ExcelFont fontItalic1 = workbook.CreateFont();
            fontItalic1.IsItalic = true;
            fontItalic1.Size = 10;
            fontItalic1.FontName = "Arial";
            fontItalic1.IsBold = true;

            RichText richText1 = sheet.Range["C3"].RichText;

            richText1.Text = EmployeeModel.department.name + "\n" + EmployeeModel.department.name_en;
            richText1.SetFont(EmployeeModel.department.name.Length, richText1.Text.Length, fontItalic1);

            RichText richText = sheet.Range["C5"].RichText;
            richText.Text = type_text + " / " + type_text_en;
            richText.SetFont(type_text.Length + 1, richText.Text.Length, fontItalic1);
            //sheet.Range["C3"] = "";
            var row_c = 1;
            var cur_r = 9;
            for (int key = 0; key < procedure_version_id.Count; ++key)
            {

                var item = procedure_version_id[key];
                var procedure_version = _context.ProcedureVersionModel.Where(d => d.id == item).Include(d => d.procedure).ThenInclude(d => d.department).FirstOrDefault();
                if (procedure_version == null)
                {
                    continue;
                }
                var type_train_c = type_train[key];
                var type_train_text = "";
                var type_train_text_en = "";
                switch (type_train_c)
                {
                    case 1:
                        type_train_text = "Hỏi - Đáp";
                        type_train_text_en = "Oral examination";
                        break;
                    case 2:
                        type_text = "Kết quả thể hiện thực tế";
                        type_text_en = "Performane";
                        break;
                }
                var date_theory_c = date_theory[key];
                //var date_reality_c = date_reality[key];
                sheet.InsertRow(cur_r + row_c + 1);
                sheet.Copy(sheet.Range["A" + (cur_r + row_c) + ":J" + (cur_r + row_c)], sheet.Range["A" + (cur_r + row_c + 1) + ":J" + (cur_r + row_c + 1)], true);
                var row = sheet.Rows[cur_r + row_c - 1];
                ExcelFont fontItalic2 = workbook.CreateFont();
                fontItalic2.IsItalic = true;
                fontItalic2.Size = 11;
                fontItalic2.FontName = "Arial";

                RichText richText2 = row.Cells[1].RichText;
                var text_vn = procedure_version.procedure.code + " - " + procedure_version.procedure.name;
                var text_en = procedure_version.procedure.code + " - " + procedure_version.procedure.name_en;
                richText2.Text = text_vn + "\n" + text_en;
                richText2.SetFont(text_vn.Length, richText2.Text.Length, fontItalic2);


                RichText richText3 = row.Cells[3].RichText;
                richText3.Text = type_train_text + "\n" + type_train_text_en;
                richText3.SetFont(type_train_text.Length, richText3.Text.Length, fontItalic2);

                row.Cells[0].Value = row_c.ToString();
                //row.Cells[1].Value = procedure_version.procedure.code + " - " + procedure_version.procedure.name;
                row.Cells[2].Value = EmployeeModel.department.symbol;
                //row.Cells[3].Value = type_train_text;
                row.Cells[4].Value = date_theory_c.ToString("dd-MM-yyyy");
                row.Cells[6].Value = procedure_version.version.ToString();
                row.AutoFitRows();
                row_c++;

            }
            sheet.DeleteRow(cur_r + row_c);

            workbook.SaveToFile("." + documentPath, ExcelVersion.Version2013);


            DocumentModel.file_url = documentPath;
            _context.Update(DocumentModel);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { type = "tp" });
        }

        public IActionResult CreateTM()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateTM(string code)
        {
            //return Ok(EmployeeModel);
            var settings = new Newtonsoft.Json.JsonSerializerSettings();
            // This tells your serializer that multiple references are okay.
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            string json = JsonConvert.SerializeObject(new { }, settings);

            JObject obj = JObject.Parse(json);
            obj["code"] = code;
            DocumentModel DocumentModel = new DocumentModel();
            DocumentModel.title = "Bảng nhu cầu huấn luyện";
            DocumentModel.type_id = 3;
            DocumentModel.created_at = DateTime.Now;
            DocumentModel.E_data = obj;
            _context.Add(DocumentModel);
            await _context.SaveChangesAsync();

            var viewPath = "private/excel/template/010006.13_06 -Training matrix_Effec. 20.06.22.xlsx";
            var documentPath = "/private/excel/tp/" + DocumentModel.id + ".xlsx";
            Workbook workbook = new Workbook();
            workbook.LoadFromFile(viewPath);
            Worksheet sheet = workbook.Worksheets[0];

            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user = await UserManager.GetUserAsync(currentUser); // Get usee
            var department_id = (int)user.department_id;
            var department = _context.DepartmentModel.Find(department_id);
            ExcelFont fontItalic = workbook.CreateFont();
            fontItalic.IsItalic = true;
            fontItalic.IsBold = true;
            fontItalic.Size = 11;
            fontItalic.FontName = "Arial";

            RichText richText1 = sheet.Range["C3"].RichText;

            richText1.Text = department.name + "\n" + department.name_en;
            richText1.SetFont(department.name.Length, richText1.Text.Length, fontItalic);
            sheet.Range["C4"].Text = code;
            var workgroups = _context.WorkgroupModel
                .Where(d => d.deleted_at == null && d.department_id == department_id)
                .Include(d => d.procedures)
                .ThenInclude(d => d.procedure)
                .ToList();
            if (workgroups.Count < 10)
            {

            }
            else
            {
                for (int i = 0; i < workgroups.Count - 10; i++)
                {
                    sheet.InsertRow(21);
                    sheet.Copy(sheet.Range["A22:Z22"], sheet.Range["A21:Z21"], true);
                    sheet.SetRowHeight(21, 35);

                    sheet.InsertColumn(7, 1, InsertOptionsType.FormatAsAfter);
                }

            }
            ExcelFont fontItalic1 = workbook.CreateFont();
            fontItalic1.IsItalic = true;
            fontItalic1.Size = 11;
            fontItalic1.FontName = "Arial";
            var cur_r = 11;
            var row_c = 1;
            foreach (var workgroup in workgroups)
            {
                var r = cur_r + row_c;
                sheet.Range["D" + r].Text = row_c.ToString();
                var name = workgroup.name + "\n" + workgroup.name_en;
                RichText richText = sheet.Range["E" + r].RichText;
                richText.Text = name;
                richText.SetFont(workgroup.name.Length, richText.Text.Length, fontItalic1);

                row_c++;
            }


            var list_workgroup = workgroups.Select(d => d.id).ToList();
            var items = _context.WorkgroupProcedureModel
                    .Where(d => list_workgroup.Contains(d.workgroup_id))
                    .Include(d => d.procedure)
                    .Where(d => d.procedure.deleted_at == null)
                    .GroupBy(d => d.procedure_id)
                    .Select(s => new
                    {
                        key = s.Key,
                        value = s.FirstOrDefault()
                    })
                    .ToList();

            row_c = 1;
            cur_r = 7;
            var dic = new Dictionary<int, int>();
            for (int key = 0; key < items.Count; ++key)
            {
                var item = items[key];
                sheet.InsertRow(cur_r + row_c + 1);
                sheet.Copy(sheet.Range["A" + (cur_r + row_c) + ":Z" + (cur_r + row_c)], sheet.Range["A" + (cur_r + row_c + 1) + ":Z" + (cur_r + row_c + 1)], true);
                var row = sheet.Rows[cur_r + row_c - 1];

                dic[item.key] = cur_r + row_c - 1;
                row.Cells[0].Value = row_c.ToString();
                row.Cells[1].Value = item.value.procedure.code;


                RichText richText2 = row.Cells[2].RichText;
                var text_vn = item.value.procedure.code + " - " + item.value.procedure.name;
                var text_en = item.value.procedure.code + " - " + item.value.procedure.name_en;
                richText2.Text = text_vn + "\n" + text_en;
                richText2.SetFont(text_vn.Length, richText2.Text.Length, fontItalic1);

                row.AutoFitRows();
                row_c++;

            }
            sheet.DeleteRow(cur_r + row_c);

            var cur_c = 3;
            foreach (var workgroup in workgroups)
            {

                sheet.Rows[6].Columns[cur_c].Value = (cur_c - 2).ToString();
                var procedures = workgroup.procedures;
                foreach (var procedure in procedures)
                {
                    var procedure_id = procedure.procedure_id;
                    var pro = procedure.procedure;
                    var row = dic[procedure_id];
                    var type = string.IsNullOrEmpty(pro.type) ? "" : pro.type.Replace(",", "");
                    var frequency = string.IsNullOrEmpty(pro.frequency) ? "" : pro.frequency.Replace(",", "");
                    var val = type + frequency;
                    sheet.Rows[row].Columns[cur_c].Value = val;
                }
                cur_c++;
            }

            workbook.SaveToFile("." + documentPath, ExcelVersion.Version2013);


            DocumentModel.file_url = documentPath;
            _context.Update(DocumentModel);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { type = "tm" });
        }

        public IActionResult CreateEL()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateEL(string version)
        {
            //return Ok(EmployeeModel);
            var settings = new Newtonsoft.Json.JsonSerializerSettings();
            // This tells your serializer that multiple references are okay.
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            string json = JsonConvert.SerializeObject(new { }, settings);

            JObject obj = JObject.Parse(json);
            obj["version"] = version;
            DocumentModel DocumentModel = new DocumentModel();
            DocumentModel.title = "Danh sách nhân sự của bộ phận - Ấn bản số " + version;
            DocumentModel.type_id = 6;
            DocumentModel.created_at = DateTime.Now;
            DocumentModel.E_data = obj;
            _context.Add(DocumentModel);
            await _context.SaveChangesAsync();

            var viewPath = "private/excel/template/010006.13_10 - Employee master list_Effec. 20.06.221.xlsx";
            var documentPath = "/private/excel/tp/" + DocumentModel.id + ".xlsx";
            Workbook workbook = new Workbook();
            workbook.LoadFromFile(viewPath);
            Worksheet sheet = workbook.Worksheets[0];

            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var user = await UserManager.GetUserAsync(currentUser); // Get usee
            var department_id = (int)user.department_id;
            var department = _context.DepartmentModel.Find(department_id);
            ExcelFont fontItalic = workbook.CreateFont();
            fontItalic.IsItalic = true;
            fontItalic.IsBold = true;
            fontItalic.Size = 11;
            fontItalic.FontName = "Arial";

            RichText richText1 = sheet.Range["C3"].RichText;

            richText1.Text = department.name + "\n" + department.name_en;
            richText1.SetFont(department.name.Length, richText1.Text.Length, fontItalic);
            sheet.Range["F3"].Text = version;
            var items = _context.EmployeeModel.Where(d => d.department_id == department_id).Include(d => d.replace_to).ToList();

            var row_c = 1;
            var cur_r = 5;
            for (int key = 0; key < items.Count; ++key)
            {

                ExcelFont fontItalic1 = workbook.CreateFont();
                fontItalic1.IsItalic = true;
                fontItalic1.Size = 11;
                fontItalic1.FontName = "Arial";


                var item = items[key];
                sheet.InsertRow(cur_r + row_c + 1);
                sheet.Copy(sheet.Range["A" + (cur_r + row_c) + ":Z" + (cur_r + row_c)], sheet.Range["A" + (cur_r + row_c + 1) + ":Z" + (cur_r + row_c + 1)], true);
                var row = sheet.Rows[cur_r + row_c - 1];

                row.Cells[0].Value = row_c.ToString();
                row.Cells[1].Value = item.FullName;
                row.Cells[2].Value = item.msnv;


                RichText richText2 = row.Cells[3].RichText;
                richText2.Text = item.education + "\n" + item.education_en;
                if (item.education != null)
                    richText2.SetFont(item.education.Length, richText2.Text.Length, fontItalic1);

                RichText richText3 = row.Cells[4].RichText;
                richText3.Text = item.position + "\n" + item.position_en;
                if (item.position != null)
                    richText3.SetFont(item.position.Length, richText3.Text.Length, fontItalic1);

                RichText richText4 = row.Cells[5].RichText;
                richText4.Text = item.work_group + "\n" + item.work_group_en;
                if (item.work_group != null)
                    richText4.SetFont(item.work_group.Length, richText4.Text.Length, fontItalic1);

                var replace_to = item.replace_to.Select(d => d.employee_replace_id).ToList();
                var employee_replace = _context.EmployeeModel.Where(d => replace_to.Contains(d.id)).ToList();
                RichText richText5 = row.Cells[6].RichText;
                foreach (var replace in employee_replace)
                {
                    richText5.Text += replace.FullName + "\n";
                }
                row.AutoFitRows();
                row_c++;

            }
            sheet.DeleteRow(cur_r + row_c);


            workbook.SaveToFile("." + documentPath, ExcelVersion.Version2013);


            DocumentModel.file_url = documentPath;
            _context.Update(DocumentModel);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { type = "el" });
        }


        public IActionResult CreateDTP()
        {
            ViewData["sops"] = _context.ProcedureModel.Where(u => u.deleted_at == null).Select(a => new SelectListItem()
            {
                Value = a.id.ToString(),
                Text = a.code + " - " + a.name
            }).ToList();
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateDTP(int type, List<int> procedure_version_id, List<int> type_train, List<DateTime> date_theory, List<DateTime> date_reality)
        {

            //return Ok(EmployeeModel);
            var settings = new Newtonsoft.Json.JsonSerializerSettings();
            // This tells your serializer that multiple references are okay.
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            string json = JsonConvert.SerializeObject(new { }, settings);

            JObject obj = JObject.Parse(json);
            obj["type"] = type;
            obj["procedure_version_id"] = System.Text.Json.JsonSerializer.Serialize(procedure_version_id);
            obj["type_train"] = System.Text.Json.JsonSerializer.Serialize(type_train);
            obj["date_theory"] = System.Text.Json.JsonSerializer.Serialize(date_theory);
            obj["date_reality"] = System.Text.Json.JsonSerializer.Serialize(date_reality);
            DocumentModel DocumentModel = new DocumentModel();
            DocumentModel.title = " - ";
            DocumentModel.type_id = 2;
            DocumentModel.created_at = DateTime.Now;
            DocumentModel.E_data = obj;
            _context.Add(DocumentModel);
            await _context.SaveChangesAsync();

            var viewPath = "private/excel/template/010006.13_09 - Department training program_Effec. 20.06.221.xlsx";
            var documentPath = "/private/excel/tp/" + DocumentModel.id + ".xlsx";
            Workbook workbook = new Workbook();
            workbook.LoadFromFile(viewPath);
            Worksheet sheet = workbook.Worksheets[0];
            ExcelFont fontItalic1 = workbook.CreateFont();
           

            workbook.SaveToFile("." + documentPath, ExcelVersion.Version2013);


            DocumentModel.file_url = documentPath;
            _context.Update(DocumentModel);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { type = "tp" });
        }

        // GET: Admin/Document/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            if (_context.DocumentModel == null)
            {
                return Problem("Entity set 'ItContext.DocumentModel'  is null.");
            }
            var DocumentModel = await _context.DocumentModel.FindAsync(id);
            var type = "";
            switch (DocumentModel.type_id)
            {
                case 1:
                    type = "jd";
                    break;
                case 2:
                    type = "tp";
                    break;
                case 3:
                    type = "tm";
                    break;
                case 6:
                    type = "el";
                    break;
            }
            if (DocumentModel != null)
            {
                DocumentModel.deleted_at = DateTime.Now;
                _context.DocumentModel.Update(DocumentModel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { type = type });
        }

        [HttpPost]
        public async Task<JsonResult> Table()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var type_id_string = Request.Form["type_id"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int type_id = type_id_string != null ? Convert.ToInt32(type_id_string) : 1;
            var customerData = (from tempcustomer in _context.DocumentModel.Where(u => u.deleted_at == null && u.type_id == type_id) select tempcustomer);
            int recordsTotal = customerData.Count();
            //if (!string.IsNullOrEmpty(searchValue))
            //{
            //    customerData = customerData.Where(m => m.title.Contains(searchValue));
            //}
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
                var file = "";
                if (record.file_url != null)
                {
                    file = "<a href='" + record.file_url + "' class='btn btn-secondary btn-xs'><i class='fas fa-file-excel mr-2'></i>Tải về</a>";
                }
                var data1 = new
                {
                    action = "<div class='btn-group'><a href='/admin/" + _type + "/delete/" + record.id + "' class='btn btn-danger btn-sm' title='Xóa?' data-type='confirm'>'"
                        + "<i class='fas fa-trash-alt'>"
                        + "</i>"
                        + "</a></div>",
                    id = "<a href='#'>" + record.id + "</a>",
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
