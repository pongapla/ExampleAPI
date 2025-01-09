using DPowerAPI.Data;
using DPowerAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Drawing;
using Microsoft.AspNetCore.Authorization;

namespace DPowerAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly DPowerAPIContext _context;
        private readonly M5LDbContext _m5lContext;

        // รับ Dependency Injection ของ DbContext ทั้งสอง
        public ProductsController(DPowerAPIContext context, M5LDbContext m5lContext)
        {
            _context = context;
            _m5lContext = m5lContext;
        }

        // POST api/products/upload
        [HttpPost("upload-excel")]
        public async Task<IActionResult> CreateProductFromExcel(IFormFile file)
        {
            
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            try
            {
               
                using (var package = new ExcelPackage(file.OpenReadStream()))
                {
                    var worksheet = package.Workbook.Worksheets[0];

                   
                    int rowCount = worksheet.Dimension.Rows;
                    for (int row = 3; row <= rowCount; row++)
                    {
                        string productCode = worksheet.Cells[row, 1].Text;
                        string costPriceText = worksheet.Cells[row, 2].Text;
                        string colorName = worksheet.Cells[row, 3].Text;

                        
                        var stk = await _m5lContext.STK.FirstOrDefaultAsync(s => s.STKcode == productCode);
                        if (stk == null)
                        {
                            
                            continue;
                        }

                        
                        var color = await _context.Colors.FirstOrDefaultAsync(c => c.Color_Name == colorName);
                        if (color == null)
                        {
                            
                            continue;
                        }

                        
                        decimal costPrice;
                        if (!decimal.TryParse(costPriceText, out costPrice))
                        {
                            
                            continue;
                        }

                       
                        var existingProduct = await _context.Products
                            .Where(p => p.Product_ID == stk.STKautoNo)
                            .FirstOrDefaultAsync();

                        if (existingProduct != null)
                        {
                           
                            existingProduct.Cost_Price = costPrice;
                            existingProduct.Color_ID = color.ID;
                            existingProduct.CreatedAt = DateTime.UtcNow;
                        }
                        else
                        {
                            
                            var product = new Products
                            {
                                Product_ID = stk.STKautoNo, 
                                Cost_Price = costPrice, 
                                Color_ID = color.ID, 
                                CreatedAt = DateTime.UtcNow 
                            };

                            _context.Products.Add(product);
                        }

                        
                        await _context.SaveChangesAsync();
                    }
                }

                return Ok("Products uploaded successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred: {ex.Message}");
            }
        }

        // POST api/products/add-product
        [HttpPost("add-product")]
        public async Task<IActionResult> AddProduct([FromBody] ProductCreateDTO productDTO)
        {
            try
            {
                // ค้นหา Product_ID จาก Product_Name
                var product = await _m5lContext.STK
                    .FirstOrDefaultAsync(s => s.STKcode == productDTO.Product_Code);

                if (product == null)
                {
                    return BadRequest("Product not found.");
                }

                // ค้นหา Color_ID จาก Color_Name
                var color = await _context.Colors
                    .FirstOrDefaultAsync(c => c.ID == productDTO.Color_ID);

                if (color == null)
                {
                    return BadRequest("Color not found.");
                }

                // สร้างและบันทึกข้อมูล Product
                var newProduct = new Products
                {
                    Product_ID = product.STKautoNo, // ใช้ Product_ID จากฐานข้อมูล
                    Cost_Price = productDTO.Cost_Price,
                    Color_ID = color.ID,            // ใช้ Color_ID จากฐานข้อมูล
                    CreatedAt = DateTime.UtcNow
                };

                _context.Products.Add(newProduct);
                await _context.SaveChangesAsync();

                return Ok("Product added successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred: {ex.Message}");
            }
        }



        // GET api/products/get-product
        [HttpGet("get-product/{productId}")]
        public async Task<IActionResult> GetProduct(int productId)
        {
            try
            {
                
                var product = await _context.Products
                    .Include(p => p.Color)
                    .FirstOrDefaultAsync(p => p.ID == productId);

                if (product == null)
                {
                    return NotFound("Product not found.");
                }

                
                var stk = await _m5lContext.STK.FirstOrDefaultAsync(s => s.STKautoNo == product.Product_ID);

                
                var productDTO = new ProductDTO
                {
                    ID = product.ID,
                    Product_Code = stk?.STKcode,
                    Product_ID = stk?.STKautoNo,
                    Product_Name = stk?.STKdescT1,
                    Cost_Price = product.Cost_Price,
                    Color_Name = product.Color?.Color_Name,
                    Color_ID = product.Color_ID,
                    CreatedAt = product.CreatedAt
                };

                return Ok(productDTO);
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("get-all-products")]
        public async Task<IActionResult> GetAllProducts()
        {
            try
            {
                
                var products = await _context.Products
                    .Include(p => p.Color)
                    .ToListAsync();

                
                var stkData = await _m5lContext.STK.ToListAsync();

                
                var productDTOs = products.Select(product =>
                {
                   
                    var stk = stkData.FirstOrDefault(s => s.STKautoNo == product.Product_ID);

                    // คืนค่าเป็น ProductDTO
                    return new ProductDTO
                    {
                        ID = product.ID,
                        Product_Code = stk?.STKcode,
                        Product_Name = stk?.STKdescT1,
                        Product_ID = stk?.STKautoNo,
                        Cost_Price = product.Cost_Price,
                        Color_Name = product.Color?.Color_Name,
                        Color_ID = product.Color_ID,
                        CreatedAt = product.CreatedAt
                    };
                }).ToList();

                return Ok(productDTOs);
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred: {ex.Message}"); 
            }
        }

        // PUT api/products/update-product/5
        [HttpPut("update-product/{productId}")]
        public async Task<IActionResult> UpdateProduct(int productId, [FromBody] ProductDTO updatedProduct)
        {
            try
            {
                
                var product = await _context.Products
                    .Include(p => p.Color)
                    .FirstOrDefaultAsync(p => p.ID == productId);

                if (product == null)
                {
                    return NotFound("Product not found.");
                }

                
                var color = await _context.Colors.FirstOrDefaultAsync(c => c.ID == updatedProduct.Color_ID);
                if (color == null)
                {
                    return BadRequest("Color not found.");
                }

                updatedProduct.Product_ID = product.Product_ID;

                var stk = await _m5lContext.STK.FirstOrDefaultAsync(s => s.STKautoNo == updatedProduct.Product_ID);
                if (stk == null)
                {
                    return BadRequest("Product code (STK) not found.");
                }

                product.Product_ID = updatedProduct.Product_ID;
                product.Cost_Price = updatedProduct.Cost_Price;
                product.Color_ID = updatedProduct.Color_ID;
                product.CreatedAt = DateTime.UtcNow;

                
                await _context.SaveChangesAsync();

                
                var updatedProductDTO = new ProductDTO
                {
                    ID = product.ID,
                    Product_Code = stk?.STKcode,
                    Product_ID = stk?.STKautoNo,
                    Product_Name = stk?.STKdescT1,
                    Cost_Price = product.Cost_Price,
                    Color_Name = color?.Color_Name,
                    Color_ID = product.Color_ID,
                    CreatedAt = product.CreatedAt
                };

                return Ok(updatedProductDTO);
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred: {ex.Message}");
            }
        }

        // DELETE api/products/delete-product/{productId}
        [HttpDelete("delete-product/{productId}")]
        public async Task<IActionResult> DeleteProduct(int productId)
        {
            try
            {
                
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.ID == productId);

                if (product == null)
                {
                    return NotFound("Product not found.");
                }

              
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                return Ok("Product deleted successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred: {ex.Message}");
            }
        }

    }
}
