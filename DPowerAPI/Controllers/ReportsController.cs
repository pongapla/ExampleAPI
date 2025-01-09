using DPowerAPI.Data;
using DPowerAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace DPowerAPI.Controllers;
[Authorize]
[Route("api/[controller]")]
[ApiController]
public class ReportsController : ControllerBase
{
    private readonly DPowerAPIContext _context;

    public ReportsController(DPowerAPIContext context)
    {
        _context = context;
    }

    #region
    // Get api/reports/get-report-stock
    [HttpGet("get-report-stock")]
    public async Task<IActionResult> GetReportStock()
    {
        try
        {

            var inventorySummary = await _context.InventorySummaries
                 .FromSqlRaw("SELECT [Group], Total_QTY FROM vw_InventorySummary")
                 .ToListAsync();


            return Ok(inventorySummary);

        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error: " + ex.Message);
        }
    }
    #endregion

    #region
    // Get api/report/get-stock-cost-report
    [HttpGet("get-stock-cost-report")]
    public async Task<IActionResult> GetReportStockCost()
    {
        try
        {
            // สร้างการเชื่อมต่อกับฐานข้อมูล
            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "EXEC [dbo].[GetStockCostReport]";
            command.CommandType = System.Data.CommandType.Text;

            using (var reader = await command.ExecuteReaderAsync())
            {
                var result = new List<ReportStockCost>();

                // อ่านชื่อคอลัมน์ทั้งหมดในผลลัพธ์ (คอลัมน์จะประกอบไปด้วย Group และสี)
                var columnNames = Enumerable.Range(0, reader.FieldCount)
                                            .Select(reader.GetName)
                                            .ToList();

                // ตรวจสอบว่าในผลลัพธ์มีคอลัมน์ Group และสีหรือไม่
                if (!columnNames.Contains("Group") || !columnNames.Any(c => c != "Group"))
                {
                    return BadRequest("The expected columns (Group, colors) are missing from the result.");
                }

                // อ่านข้อมูลจาก DbDataReader
                while (await reader.ReadAsync())
                {
                    var group = reader["Group"].ToString();
                    var report = result.FirstOrDefault(r => r.Group == group);

                    // ถ้าไม่พบ Group ให้สร้างใหม่
                    if (report == null)
                    {
                        report = new ReportStockCost { Group = group, Colors = new Dictionary<string, decimal?>() };
                        result.Add(report);
                    }

                    // เพิ่มข้อมูลสีลงใน Dictionary
                    foreach (var color in columnNames.Where(c => c != "Group"))
                    {
                        var colorValue = reader[color];
                        decimal? cost = colorValue != DBNull.Value ? Convert.ToDecimal(colorValue) : (decimal?)null;
                        report.Colors[color] = cost;
                    }
                }

                return Ok(result);
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error: " + ex.Message);
        }
    }

    #endregion
}


