using OfficeOpenXml;
using RfidAppApi.DTOs;
using RfidAppApi.Models;
using RfidAppApi.Data;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace RfidAppApi.Services
{
    /// <summary>
    /// Service for RFID Excel operations including upload, processing, and template generation
    /// </summary>
    public class RfidExcelService : IRfidExcelService
    {
        private readonly IClientService _clientService;
        private readonly ILogger<RfidExcelService> _logger;

        public RfidExcelService(IClientService clientService, ILogger<RfidExcelService> logger)
        {
            _clientService = clientService;
            _logger = logger;
            
            // Set EPPlus license context for non-commercial use
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        /// <summary>
        /// Upload RFID data from Excel file
        /// </summary>
        public async Task<RfidExcelUploadResponseDto> UploadRfidFromExcelAsync(RfidExcelUploadDto uploadDto, string clientCode)
        {
            try
            {
                _logger.LogInformation("Starting RFID Excel upload for client: {ClientCode}", clientCode);

                // Read Excel file and extract RFID data
                var rfidRows = await ReadExcelFileAsync(uploadDto.ExcelFile);
                
                // Validate the data
                var validationErrors = await ValidateRfidDataAsync(rfidRows);
                if (validationErrors.Any())
                {
                    return new RfidExcelUploadResponseDto
                    {
                        TotalRowsProcessed = rfidRows.Count,
                        ErrorRows = validationErrors.Count,
                        Errors = validationErrors,
                        Summary = $"Validation failed with {validationErrors.Count} errors"
                    };
                }

                // Process the RFID rows
                var result = await ProcessRfidRowsAsync(rfidRows, clientCode, uploadDto.UpdateExisting, uploadDto.CreateNew);
                
                _logger.LogInformation("RFID Excel upload completed for client: {ClientCode}. Processed: {Processed}, Created: {Created}, Updated: {Updated}, Errors: {Errors}", 
                    clientCode, result.TotalRowsProcessed, result.NewRfidsCreated, result.ExistingRfidsUpdated, result.ErrorRows);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during RFID Excel upload for client: {ClientCode}", clientCode);
                throw;
            }
        }

        /// <summary>
        /// Process RFID data from Excel rows
        /// </summary>
        public async Task<RfidExcelUploadResponseDto> ProcessRfidRowsAsync(List<RfidExcelRowDto> rfidRows, string clientCode, bool updateExisting = true, bool createNew = true)
        {
            var response = new RfidExcelUploadResponseDto
            {
                TotalRowsProcessed = rfidRows.Count
            };

            using var context = await _clientService.GetClientDbContextAsync(clientCode);

            foreach (var row in rfidRows)
            {
                try
                {
                    // Check if RFID code already exists
                    var existingRfid = await context.Rfids
                        .FirstOrDefaultAsync(r => r.RFIDCode == row.RFIDCode);

                    if (existingRfid != null)
                    {
                        if (updateExisting)
                        {
                            // Update existing RFID with new EPC value
                            existingRfid.EPCValue = row.EPCValue;
                            existingRfid.IsActive = true;
                            context.Rfids.Update(existingRfid);
                            
                            response.ExistingRfidsUpdated++;
                            response.ProcessedRows.Add(row);
                            _logger.LogDebug("Updated existing RFID: {RFIDCode} with EPC: {EPCValue}", row.RFIDCode, row.EPCValue);
                        }
                        else
                        {
                            response.Errors.Add($"RFID code {row.RFIDCode} already exists and updates are disabled");
                            response.ErrorRows++;
                        }
                    }
                    else
                    {
                        if (createNew)
                        {
                            // Create new RFID
                            var newRfid = new Rfid
                            {
                                RFIDCode = row.RFIDCode,
                                EPCValue = row.EPCValue,
                                ClientCode = clientCode,
                                IsActive = true,
                                CreatedOn = DateTime.UtcNow
                            };

                            context.Rfids.Add(newRfid);
                            
                            response.NewRfidsCreated++;
                            response.ProcessedRows.Add(row);
                            _logger.LogDebug("Created new RFID: {RFIDCode} with EPC: {EPCValue}", row.RFIDCode, row.EPCValue);
                        }
                        else
                        {
                            response.Errors.Add($"RFID code {row.RFIDCode} does not exist and creation is disabled");
                            response.ErrorRows++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    var errorMessage = $"Error processing RFID {row.RFIDCode}: {ex.Message}";
                    response.Errors.Add(errorMessage);
                    response.ErrorRows++;
                    _logger.LogError(ex, "Error processing RFID row: {RFIDCode}", row.RFIDCode);
                }
            }

            // Save all changes to database
            if (response.ProcessedRows.Any())
            {
                await context.SaveChangesAsync();
            }

            // Generate summary
            response.Summary = GenerateSummary(response);

            return response;
        }

        /// <summary>
        /// Validate RFID data from Excel
        /// </summary>
        public async Task<List<string>> ValidateRfidDataAsync(List<RfidExcelRowDto> rfidRows)
        {
            var errors = new List<string>();

            // Check for duplicate RFID codes within the Excel file
            var duplicateRfidCodes = rfidRows
                .GroupBy(r => r.RFIDCode)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            foreach (var duplicateCode in duplicateRfidCodes)
            {
                errors.Add($"Duplicate RFID code found in Excel: {duplicateCode}");
            }

            // Check for duplicate EPC values within the Excel file
            var duplicateEpcValues = rfidRows
                .GroupBy(r => r.EPCValue)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            foreach (var duplicateEpc in duplicateEpcValues)
            {
                errors.Add($"Duplicate EPC value found in Excel: {duplicateEpc}");
            }

            // Validate individual rows
            for (int i = 0; i < rfidRows.Count; i++)
            {
                var row = rfidRows[i];
                var rowNumber = i + 2; // Excel rows start from 1, and we have header row

                if (string.IsNullOrWhiteSpace(row.RFIDCode))
                {
                    errors.Add($"Row {rowNumber}: RFID Code is required");
                }
                else if (row.RFIDCode.Length > 50)
                {
                    errors.Add($"Row {rowNumber}: RFID Code cannot exceed 50 characters");
                }

                if (string.IsNullOrWhiteSpace(row.EPCValue))
                {
                    errors.Add($"Row {rowNumber}: EPC Value is required");
                }
                else if (row.EPCValue.Length > 100)
                {
                    errors.Add($"Row {rowNumber}: EPC Value cannot exceed 100 characters");
                }
            }

            return errors;
        }

        /// <summary>
        /// Generate sample Excel template for RFID upload
        /// </summary>
        public async Task<byte[]> GenerateExcelTemplateAsync()
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("RFID Upload Template");

            // Add headers
            worksheet.Cells[1, 1].Value = "RFID Code";
            worksheet.Cells[1, 2].Value = "EPC Value";

            // Style headers
            using (var range = worksheet.Cells[1, 1, 1, 2])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                range.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            }

            // Add sample data
            worksheet.Cells[2, 1].Value = "RFID001";
            worksheet.Cells[2, 2].Value = "EPC123456789";

            worksheet.Cells[3, 1].Value = "RFID002";
            worksheet.Cells[3, 2].Value = "EPC987654321";

            // Add instructions
            worksheet.Cells[5, 1].Value = "Instructions:";
            worksheet.Cells[6, 1].Value = "1. Column 1: Enter RFID Code (max 50 characters)";
            worksheet.Cells[7, 1].Value = "2. Column 2: Enter EPC Value (max 100 characters)";
            worksheet.Cells[8, 1].Value = "3. If RFID Code exists, EPC Value will be updated";
            worksheet.Cells[9, 1].Value = "4. If RFID Code doesn't exist, new record will be created";
            worksheet.Cells[10, 1].Value = "5. Remove sample rows before uploading";

            // Style instructions
            using (var range = worksheet.Cells[5, 1, 10, 1])
            {
                range.Style.Font.Bold = true;
                range.Style.Font.Color.SetColor(System.Drawing.Color.DarkBlue);
            }

            // Auto-fit columns
            worksheet.Cells.AutoFitColumns();

            return await package.GetAsByteArrayAsync();
        }

        /// <summary>
        /// Read Excel file and extract RFID data
        /// </summary>
        private async Task<List<RfidExcelRowDto>> ReadExcelFileAsync(IFormFile file)
        {
            var rfidRows = new List<RfidExcelRowDto>();

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
                var rfidCode = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                var epcValue = worksheet.Cells[row, 2].Value?.ToString()?.Trim();

                // Skip empty rows
                if (string.IsNullOrWhiteSpace(rfidCode) && string.IsNullOrWhiteSpace(epcValue))
                {
                    continue;
                }

                rfidRows.Add(new RfidExcelRowDto
                {
                    RFIDCode = rfidCode ?? string.Empty,
                    EPCValue = epcValue ?? string.Empty
                });
            }

            return rfidRows;
        }

        /// <summary>
        /// Generate summary message for the response
        /// </summary>
        private string GenerateSummary(RfidExcelUploadResponseDto response)
        {
            var summary = new StringBuilder();
            
            summary.Append($"Successfully processed {response.TotalRowsProcessed} rows. ");
            
            if (response.NewRfidsCreated > 0)
            {
                summary.Append($"Created {response.NewRfidsCreated} new RFID codes. ");
            }
            
            if (response.ExistingRfidsUpdated > 0)
            {
                summary.Append($"Updated {response.ExistingRfidsUpdated} existing RFID codes. ");
            }
            
            if (response.ErrorRows > 0)
            {
                summary.Append($"Encountered {response.ErrorRows} errors. ");
            }

            return summary.ToString().Trim();
        }
    }
}
