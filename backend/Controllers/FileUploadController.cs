using Microsoft.AspNetCore.Mvc;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using backend.Data;

namespace backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FileUploadController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FileUploadController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("upload")]
        public IActionResult Upload()
        {
            try
            {
                var file = Request.Form.Files[0];
                if (file.Length > 0)
                {
                    using (var reader = new StreamReader(file.OpenReadStream()))
                    {
                        var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
                        var records = csvReader.GetRecords<Student>().ToList();

                        int validCount = 0;
                        int invalidCount = 0;

                        foreach (var record in records)
                        {
                            if (IsValidRecord(record))
                            {
                                var existingStudent = _context.Students.FirstOrDefault(s => s.StudentId == record.StudentId);
                                if (existingStudent == null)
                                {
                                    _context.Students.Add(record);
                                }
                                else
                                {
                                    existingStudent.Name = record.Name;
                                    existingStudent.Class = record.Class;
                                    existingStudent.Email = record.Email;
                                    existingStudent.MobileNumber = record.MobileNumber;
                                    existingStudent.MarksObtained = record.MarksObtained;
                                }
                                validCount++;
                            }
                            else
                            {
                                invalidCount++;
                                // Handle invalid record 
                            }
                        }

                        _context.SaveChanges();

                        return Ok(new { ValidCount = validCount, InvalidCount = invalidCount });
                    }
                }
                return BadRequest("No file uploaded.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            try
            {
                var student = _context.Students.Find(id);
                if (student == null)
                {
                    return NotFound($"Student with ID {id} not found.");
                }

                _context.Students.Remove(student);
                _context.SaveChanges();

                return Ok($"Student with ID {id} deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
     [HttpGet("valid-records")]
        public IActionResult GetValidRecords()
        {
            try
            {
                var validRecords = _context.Students.Where(IsValidRecord).ToList();
                return Ok(validRecords);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        private bool IsValidRecord(Student record)
        {
            // Validate each field of the record
            if (!int.TryParse(record.StudentId, out _))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(record.Name))
            {
                return false;
            }

            if (!Regex.IsMatch(record.Class, "^[a-zA-Z]+$"))
            {
                return false;
            }

            if (!IsValidEmail(record.Email))
            {
                return false;
            }

            if (!Regex.IsMatch(record.MobileNumber, "^[0-9]{10}$"))
            {
                return false;
            }

            if (record.MarksObtained < 0 || record.MarksObtained > 100)
            {
                return false;
            }

            return true;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
