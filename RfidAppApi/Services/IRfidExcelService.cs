using RfidAppApi.DTOs;

namespace RfidAppApi.Services
{
    /// <summary>
    /// Service interface for RFID Excel operations
    /// </summary>
    public interface IRfidExcelService
    {
        /// <summary>
        /// Upload RFID data from Excel file
        /// </summary>
        /// <param name="uploadDto">Upload request containing Excel file and options</param>
        /// <param name="clientCode">Client code for the operation</param>
        /// <returns>Upload response with processing results</returns>
        Task<RfidExcelUploadResponseDto> UploadRfidFromExcelAsync(RfidExcelUploadDto uploadDto, string clientCode);
        
        /// <summary>
        /// Process RFID data from Excel rows
        /// </summary>
        /// <param name="rfidRows">List of RFID data from Excel</param>
        /// <param name="clientCode">Client code for the operation</param>
        /// <param name="updateExisting">Whether to update existing RFID codes</param>
        /// <param name="createNew">Whether to create new RFID codes</param>
        /// <returns>Processing results</returns>
        Task<RfidExcelUploadResponseDto> ProcessRfidRowsAsync(List<RfidExcelRowDto> rfidRows, string clientCode, bool updateExisting = true, bool createNew = true);
        
        /// <summary>
        /// Validate RFID data from Excel
        /// </summary>
        /// <param name="rfidRows">List of RFID data to validate</param>
        /// <returns>Validation results with any errors</returns>
        Task<List<string>> ValidateRfidDataAsync(List<RfidExcelRowDto> rfidRows);
        
        /// <summary>
        /// Generate sample Excel template for RFID upload
        /// </summary>
        /// <returns>Excel file as byte array</returns>
        Task<byte[]> GenerateExcelTemplateAsync();
    }
}
