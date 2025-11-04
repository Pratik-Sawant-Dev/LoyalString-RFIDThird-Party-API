using OfficeOpenXml;
using RfidAppApi.DTOs;
using RfidAppApi.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Diagnostics;

namespace RfidAppApi.Services
{
    /// <summary>
    /// Service for Product Excel operations including upload, processing, and template generation
    /// Optimized for bulk processing (100k+ products)
    /// </summary>
    public class ProductExcelService : IProductExcelService
    {
        private readonly IClientService _clientService;
        private readonly IUserFriendlyProductService _productService;
        private readonly ILogger<ProductExcelService> _logger;

        public ProductExcelService(
            IClientService clientService,
            IUserFriendlyProductService productService,
            ILogger<ProductExcelService> logger)
        {
            _clientService = clientService;
            _productService = productService;
            _logger = logger;
            
            // Set EPPlus license context for non-commercial use
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        /// <summary>
        /// Generate Excel template for product upload
        /// </summary>
        public async Task<byte[]> GenerateExcelTemplateAsync()
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Products");

            // Headers
            var headers = new[]
            {
                "ItemCode*",
                "CategoryName*",
                "BranchName*",
                "CounterName*",
                "ProductName*",
                "DesignName*",
                "PurityName*",
                "RfidCode",
                "BoxDetails",
                "GrossWeight",
                "StoneWeight",
                "DiamondHeight",
                "NetWeight",
                "Size",
                "StoneAmount",
                "DiamondAmount",
                "HallmarkAmount",
                "MakingPerGram",
                "MakingPercentage",
                "MakingFixedAmount",
                "MRP",
                "Status",
                "CustomFields (JSON)"
            };

            // Write headers
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[1, i + 1].Value = headers[i];
                worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                worksheet.Cells[1, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
            }

            // Add example rows
            var exampleRow1 = new object[]
            {
                "JWL001",
                "Rings",
                "Main Branch",
                "Counter 1",
                "Diamond Ring",
                "Classic",
                "18K",
                "RFID001",
                "Premium velvet box",
                5.5,
                0.5,
                2.5,
                4.8,
                18,
                15000,
                25000,
                500,
                100,
                15,
                2000,
                65000,
                "Active",
                "{\"Ring Size\":\"7\",\"Band Width\":\"4mm\",\"Setting Type\":\"Prong\",\"Has Certificate\":\"true\"}"
            };

            var exampleRow2 = new object[]
            {
                "JWL002",
                "Necklaces",
                "Main Branch",
                "Counter 1",
                "Gold Necklace",
                "Traditional",
                "22K",
                "RFID002",
                "Elegant box",
                10.5,
                0.8,
                3.0,
                9.2,
                null,
                12000,
                18000,
                600,
                110,
                16,
                2500,
                75000,
                "Active",
                "{\"Chain Length\":\"18 inches\",\"Clasp Type\":\"Lobster Claw\",\"Pendant Size\":\"Large\"}"
            };

            // Write example rows
            for (int i = 0; i < exampleRow1.Length; i++)
            {
                worksheet.Cells[2, i + 1].Value = exampleRow1[i];
            }

            for (int i = 0; i < exampleRow2.Length; i++)
            {
                worksheet.Cells[3, i + 1].Value = exampleRow2[i];
            }

            // Set column widths
            worksheet.Column(1).Width = 15;  // ItemCode
            worksheet.Column(2).Width = 15;  // CategoryName
            worksheet.Column(3).Width = 15;  // BranchName
            worksheet.Column(4).Width = 15;  // CounterName
            worksheet.Column(5).Width = 20;  // ProductName
            worksheet.Column(6).Width = 15;  // DesignName
            worksheet.Column(7).Width = 10;  // PurityName
            worksheet.Column(8).Width = 15;  // RfidCode
            worksheet.Column(9).Width = 20;  // BoxDetails
            worksheet.Column(23).Width = 50; // CustomFields

            // Add data validation instructions in a separate sheet
            var instructionsSheet = package.Workbook.Worksheets.Add("Instructions");
            instructionsSheet.Cells[1, 1].Value = "Product Excel Upload Instructions";
            instructionsSheet.Cells[1, 1].Style.Font.Bold = true;
            instructionsSheet.Cells[1, 1].Style.Font.Size = 14;

            instructionsSheet.Cells[3, 1].Value = "Required Fields (marked with *):";
            instructionsSheet.Cells[3, 1].Style.Font.Bold = true;
            instructionsSheet.Cells[4, 1].Value = "• ItemCode - Unique product identifier";
            instructionsSheet.Cells[5, 1].Value = "• CategoryName - Product category (e.g., Rings, Necklaces)";
            instructionsSheet.Cells[6, 1].Value = "• BranchName - Branch name";
            instructionsSheet.Cells[7, 1].Value = "• CounterName - Counter name";
            instructionsSheet.Cells[8, 1].Value = "• ProductName - Product type (e.g., Ring, Necklace)";
            instructionsSheet.Cells[9, 1].Value = "• DesignName - Design name";
            instructionsSheet.Cells[10, 1].Value = "• PurityName - Purity (e.g., 18K, 22K, 24K)";

            instructionsSheet.Cells[12, 1].Value = "Custom Fields (JSON Format):";
            instructionsSheet.Cells[12, 1].Style.Font.Bold = true;
            instructionsSheet.Cells[13, 1].Value = "Enter custom fields as JSON object. Example:";
            instructionsSheet.Cells[14, 1].Value = "{\"Ring Size\":\"7\",\"Band Width\":\"4mm\",\"Has Certificate\":\"true\"}";
            instructionsSheet.Cells[14, 1].Style.Font.Color.SetColor(System.Drawing.Color.DarkBlue);

            instructionsSheet.Cells[16, 1].Value = "Notes:";
            instructionsSheet.Cells[16, 1].Style.Font.Bold = true;
            instructionsSheet.Cells[17, 1].Value = "• Delete example rows before uploading your data";
            instructionsSheet.Cells[18, 1].Value = "• ItemCode must be unique";
            instructionsSheet.Cells[19, 1].Value = "• Supported file formats: .xlsx";
            instructionsSheet.Cells[20, 1].Value = "• Maximum file size: 50MB";
            instructionsSheet.Cells[21, 1].Value = "• Can process up to 100,000 products efficiently";

            instructionsSheet.Column(1).Width = 80;

            return await Task.FromResult(package.GetAsByteArray());
        }

        /// <summary>
        /// Upload products from Excel file
        /// Optimized for bulk processing
        /// </summary>
        public async Task<ProductExcelUploadResponseDto> UploadProductsFromExcelAsync(
            ProductExcelUploadDto uploadDto, 
            string clientCode)
        {
            var stopwatch = Stopwatch.StartNew();
            var response = new ProductExcelUploadResponseDto();

            try
            {
                _logger.LogInformation("Starting Product Excel upload for client: {ClientCode}", clientCode);

                // Read Excel file
                var productRows = await ReadExcelFileAsync(uploadDto.ExcelFile);
                response.TotalRowsProcessed = productRows.Count;

                _logger.LogInformation("Read {Count} rows from Excel file", productRows.Count);

                // Process in batches for better performance
                const int batchSize = 1000;
                var batches = productRows
                    .Select((row, index) => new { row, index })
                    .GroupBy(x => x.index / batchSize)
                    .Select(g => g.Select(x => x.row).ToList())
                    .ToList();

                _logger.LogInformation("Processing {BatchCount} batches of {BatchSize} products each", batches.Count, batchSize);

                foreach (var batch in batches)
                {
                    try
                    {
                        // Convert to UserFriendlyCreateProductDto
                        var productsToCreate = new List<UserFriendlyCreateProductDto>();

                        foreach (var row in batch)
                        {
                            try
                            {
                                var productDto = MapExcelRowToProductDto(row);
                                
                                // Check if product exists (if updateExisting is false, skip)
                                if (!uploadDto.UpdateExisting)
                                {
                                    using var context = await _clientService.GetClientDbContextAsync(clientCode);
                                    var exists = await context.ProductDetails
                                        .AnyAsync(p => p.ItemCode == productDto.ItemCode);
                                    if (exists)
                                    {
                                        response.SkippedRows++;
                                        if (!uploadDto.SkipErrors)
                                        {
                                            response.Errors.Add(new ExcelErrorDto
                                            {
                                                RowNumber = row.ExcelRowNumber,
                                                ItemCode = row.ItemCode,
                                                ErrorMessage = "Product with this ItemCode already exists. Set UpdateExisting=true to update."
                                            });
                                        }
                                        continue;
                                    }
                                }

                                productsToCreate.Add(productDto);
                            }
                            catch (Exception ex)
                            {
                                response.ErrorRows++;
                                response.Errors.Add(new ExcelErrorDto
                                {
                                    RowNumber = row.ExcelRowNumber,
                                    ItemCode = row.ItemCode,
                                    ErrorMessage = ex.Message
                                });

                                if (!uploadDto.SkipErrors)
                                {
                                    throw;
                                }
                            }
                        }

                        // Bulk create products
                        if (productsToCreate.Any())
                        {
                            var bulkDto = new BulkCreateProductsDto
                            {
                                Products = productsToCreate
                            };

                            var bulkResponse = await _productService.CreateBulkProductsAsync(bulkDto, clientCode);
                            response.SuccessfullyCreated += bulkResponse.SuccessfullyCreated;
                            response.ErrorRows += bulkResponse.Failed;

                            // Add bulk errors to response
                            foreach (var error in bulkResponse.Errors)
                            {
                                response.Errors.Add(new ExcelErrorDto
                                {
                                    RowNumber = 0,
                                    ItemCode = "",
                                    ErrorMessage = error
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing batch");
                        response.ErrorRows += batch.Count;
                        if (!uploadDto.SkipErrors)
                        {
                            throw;
                        }
                    }
                }

                stopwatch.Stop();
                response.ProcessingTime = stopwatch.Elapsed;
                response.Summary = $"Processed {response.TotalRowsProcessed} rows. " +
                    $"Created: {response.SuccessfullyCreated}, " +
                    $"Errors: {response.ErrorRows}, " +
                    $"Time: {response.ProcessingTime.TotalSeconds:F2} seconds";

                _logger.LogInformation("Product Excel upload completed. {Summary}", response.Summary);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Product Excel upload for client: {ClientCode}", clientCode);
                stopwatch.Stop();
                response.ProcessingTime = stopwatch.Elapsed;
                response.Summary = $"Error: {ex.Message}";
                throw;
            }
        }

        /// <summary>
        /// Read and parse Excel file to extract product data
        /// </summary>
        public async Task<List<ProductExcelRowDto>> ReadExcelFileAsync(IFormFile file)
        {
            var productRows = new List<ProductExcelRowDto>();

            using var stream = file.OpenReadStream();
            using var package = new ExcelPackage(stream);
            
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
            {
                throw new InvalidOperationException("No worksheets found in the Excel file");
            }

            var rowCount = worksheet.Dimension?.Rows ?? 0;
            if (rowCount < 2) // Need at least header + 1 data row
            {
                throw new InvalidOperationException("Excel file must contain at least one data row");
            }

            // Start from row 2 (skip header)
            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    // Skip empty rows
                    var itemCode = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                    if (string.IsNullOrWhiteSpace(itemCode))
                    {
                        continue;
                    }

                    var productRow = new ProductExcelRowDto
                    {
                        ExcelRowNumber = row,
                        ItemCode = itemCode,
                        CategoryName = worksheet.Cells[row, 2].Value?.ToString()?.Trim() ?? string.Empty,
                        BranchName = worksheet.Cells[row, 3].Value?.ToString()?.Trim() ?? string.Empty,
                        CounterName = worksheet.Cells[row, 4].Value?.ToString()?.Trim() ?? string.Empty,
                        ProductName = worksheet.Cells[row, 5].Value?.ToString()?.Trim() ?? string.Empty,
                        DesignName = worksheet.Cells[row, 6].Value?.ToString()?.Trim() ?? string.Empty,
                        PurityName = worksheet.Cells[row, 7].Value?.ToString()?.Trim() ?? string.Empty,
                        RfidCode = worksheet.Cells[row, 8].Value?.ToString()?.Trim(),
                        BoxDetails = worksheet.Cells[row, 9].Value?.ToString()?.Trim(),
                        GrossWeight = ConvertToFloat(worksheet.Cells[row, 10].Value),
                        StoneWeight = ConvertToFloat(worksheet.Cells[row, 11].Value),
                        DiamondHeight = ConvertToFloat(worksheet.Cells[row, 12].Value),
                        NetWeight = ConvertToFloat(worksheet.Cells[row, 13].Value),
                        Size = ConvertToInt(worksheet.Cells[row, 14].Value),
                        StoneAmount = ConvertToDecimal(worksheet.Cells[row, 15].Value),
                        DiamondAmount = ConvertToDecimal(worksheet.Cells[row, 16].Value),
                        HallmarkAmount = ConvertToDecimal(worksheet.Cells[row, 17].Value),
                        MakingPerGram = ConvertToDecimal(worksheet.Cells[row, 18].Value),
                        MakingPercentage = ConvertToDecimal(worksheet.Cells[row, 19].Value),
                        MakingFixedAmount = ConvertToDecimal(worksheet.Cells[row, 20].Value),
                        Mrp = ConvertToDecimal(worksheet.Cells[row, 21].Value),
                        Status = worksheet.Cells[row, 22].Value?.ToString()?.Trim() ?? "Active",
                        CustomFieldsJson = worksheet.Cells[row, 23].Value?.ToString()?.Trim()
                    };

                    productRows.Add(productRow);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error reading row {RowNumber} from Excel", row);
                    // Continue processing other rows
                }
            }

            return await Task.FromResult(productRows);
        }

        /// <summary>
        /// Map Excel row DTO to UserFriendlyCreateProductDto
        /// </summary>
        private UserFriendlyCreateProductDto MapExcelRowToProductDto(ProductExcelRowDto row)
        {
            var productDto = new UserFriendlyCreateProductDto
            {
                ItemCode = row.ItemCode,
                CategoryName = row.CategoryName,
                BranchName = row.BranchName,
                CounterName = row.CounterName,
                ProductName = row.ProductName,
                DesignName = row.DesignName,
                PurityName = row.PurityName,
                RfidCode = row.RfidCode,
                BoxDetails = row.BoxDetails,
                GrossWeight = row.GrossWeight,
                StoneWeight = row.StoneWeight,
                DiamondHeight = row.DiamondHeight,
                NetWeight = row.NetWeight,
                Size = row.Size,
                StoneAmount = row.StoneAmount,
                DiamondAmount = row.DiamondAmount,
                HallmarkAmount = row.HallmarkAmount,
                MakingPerGram = row.MakingPerGram,
                MakingPercentage = row.MakingPercentage,
                MakingFixedAmount = row.MakingFixedAmount,
                Mrp = row.Mrp,
                Status = row.Status ?? "Active"
            };

            // Parse custom fields from JSON
            if (!string.IsNullOrWhiteSpace(row.CustomFieldsJson))
            {
                try
                {
                    var customFields = JsonSerializer.Deserialize<Dictionary<string, object>>(row.CustomFieldsJson);
                    productDto.CustomFields = customFields;
                }
                catch (JsonException ex)
                {
                    throw new InvalidOperationException($"Invalid CustomFields JSON at row {row.ExcelRowNumber}: {ex.Message}");
                }
            }

            return productDto;
        }

        /// <summary>
        /// Helper methods for type conversion
        /// </summary>
        private float? ConvertToFloat(object? value)
        {
            if (value == null) return null;
            if (value is double d) return (float)d;
            if (float.TryParse(value.ToString(), out float f)) return f;
            return null;
        }

        private int? ConvertToInt(object? value)
        {
            if (value == null) return null;
            if (value is double d) return (int)d;
            if (int.TryParse(value.ToString(), out int i)) return i;
            return null;
        }

        private decimal? ConvertToDecimal(object? value)
        {
            if (value == null) return null;
            if (value is double d) return (decimal)d;
            if (decimal.TryParse(value.ToString(), out decimal dec)) return dec;
            return null;
        }
    }
}

