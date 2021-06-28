using ExcelDataReader;
using ExcelUploader2.ExtensionMethod;
using ExcelUploader2.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ExcelUploader2.Controllers
{
    public class RiderController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public RiderController()
        {
            _dbContext = new ApplicationDbContext();
        }
        // GET: Rider
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult SA()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> ImportFile(HttpPostedFileBase importFile)
        {
            if (importFile == null) return Json(new { Status = 0, Message = "No File Selected" });

            try
            {
                var fileData = GetDataFromCSVFile(importFile.InputStream);

                var dtEmployee = fileData.ToList();
                foreach(var record in dtEmployee)
                {
                    await _dbContext.Database.ExecuteSqlCommandAsync($@"
with i as (
insert into asset_management.till_numbers (till_number_id,till_number) 
values (uuid_generate_v4(),{record.TillNumber}) returning till_number_id
),
j as (insert into ""identity"".users (till_number_id,foreign_user_id,first_name,middle_name,last_name,phone_number,user_name,registered_on,updated_on,user_id)
select till_number_id, '{record.IdNumber}', '{record.FirstName}','{record.MiddleName}', '{record.LastName}','‌{record.RiderPhone}',
'{record.EmployeeNumber}', now(), now(), uuid_generate_v4() from i returning user_id
),
k as (
insert into asset_management.assets(asset_id, device_id, asset_name, line_number, asset_type_id, asset_status_id)
values(uuid_generate_v4(), '', 'RiderPhone', '{record.RiderPhone}', 'f1e3ce94-d83b-4324-9395-7fa7dbcd7655', '9ec71758-f4cd-4930-acdb-04130579bbef') returning asset_id
), l as (
insert into asset_management.asset_users(user_id, asset_id)
select user_id,(select asset_id from k) from j
)
insert into ""identity"".user_roles(user_id, role_id)
select user_id,4 from j
;
                    ");
                }

                return Json(new { Status = 1, Message = "File Imported Successfully " });
            }
            catch (Exception ex)
            {
                return Json(new { Status = 0, Message = ex.Message });
            }
        }
        [HttpPost]
        public async Task<ActionResult> ImportFile1(HttpPostedFileBase importFile)
        {
            if (importFile == null) return Json(new { Status = 0, Message = "No File Selected" });

            try
            {
                var fileData = GetDataFromCSVFile1(importFile.InputStream);

                var dtEmployee = fileData.ToList();
                foreach (var record in dtEmployee)
                {
                    await _dbContext.Database.ExecuteSqlCommandAsync($@"
with j as (insert into ""identity"".users (foreign_user_id,first_name,middle_name,last_name,phone_number,user_name,registered_on,updated_on,user_id)
values('{record.PhoneNumber}', '{record.FirstName}','{record.MiddleName}', '{record.LastName}','‌{record.PhoneNumber}',
'{record.PhoneNumber}', now(), now(), uuid_generate_v4()) returning user_id   
),
k as (
insert into asset_management.assets(asset_id, device_id, asset_name, line_number, asset_type_id, asset_status_id)
values(uuid_generate_v4(), '', 'SaPhone', '{record.PhoneNumber}', 'f1e3ce94-d83b-4324-9395-7fa7dbcd7655', '9ec71758-f4cd-4930-acdb-04130579bbef') returning asset_id
), l as (
insert into asset_management.asset_users(user_id, asset_id)
select user_id,(select asset_id from k) from j
)
insert into ""identity"".user_roles(user_id, role_id)
select user_id,3 from j
;
                    ");
                }

                return Json(new { Status = 1, Message = "File Imported Successfully " });
            }
            catch (Exception ex)
            {
                return Json(new { Status = 0, Message = ex.Message });
            }
        }
        private List<Rider> GetDataFromCSVFile(Stream stream)
        {
            var riderList = new List<Rider>();
            try
            {
                using (var reader = ExcelReaderFactory.CreateCsvReader(stream))
                {
                    var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
                    {
                        ConfigureDataTable = _ => new ExcelDataTableConfiguration
                        {
                            UseHeaderRow = true // To set First Row As Column Names    
                        }
                    });

                    if (dataSet.Tables.Count > 0)
                    {
                        var dataTable = dataSet.Tables[0];
                        foreach (DataRow objDataRow in dataTable.Rows)
                        {

                            if (objDataRow.ItemArray.All(x => string.IsNullOrEmpty(x?.ToString()))) continue;
                            riderList.Add(new Rider()
                            {
                                FirstName = objDataRow["Fname"].ToString(),
                                MiddleName= objDataRow["Mname"].ToString(),
                                LastName = objDataRow["Lname"].ToString(),
                                EmployeeNumber = objDataRow["EmployeeNo"].ToString(),
                                IdNumber = objDataRow["IdNo"].ToString(),
                                TillNumber = Convert.ToInt32(objDataRow["TillNo"].ToString()),
                                BikeRegistration = objDataRow["BikeReg"].ToString(),
                                RiderPhone = objDataRow["PhoneNo"].ToString()
                            });
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return riderList;
        }
        private List<ShopAttendant> GetDataFromCSVFile1(Stream stream)
        {
            var riderList = new List<ShopAttendant>();
            try
            {
                using (var reader = ExcelReaderFactory.CreateCsvReader(stream))
                {
                    var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
                    {
                        ConfigureDataTable = _ => new ExcelDataTableConfiguration
                        {
                            UseHeaderRow = true // To set First Row As Column Names    
                        }
                    });

                    if (dataSet.Tables.Count > 0)
                    {
                        var dataTable = dataSet.Tables[0];
                        foreach (DataRow objDataRow in dataTable.Rows)
                        {

                            if (objDataRow.ItemArray.All(x => string.IsNullOrEmpty(x?.ToString()))) continue;
                            riderList.Add(new ShopAttendant()
                            {
                                FirstName = objDataRow["Fname"].ToString(),
                                MiddleName = objDataRow["Mname"].ToString(),
                                LastName = objDataRow["Lname"].ToString(),
                                PhoneNumber= objDataRow["PhoneNumber"].ToString(),
                            });
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return riderList;
        }
    }
}