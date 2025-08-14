namespace RfidAppApi.DTOs
{
    public class RfidDto
    {
        public string RFIDCode { get; set; } = string.Empty;
        public string EPCValue { get; set; } = string.Empty;
        public string ClientCode { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class CreateRfidDto
    {
        public string RFIDCode { get; set; } = string.Empty;
        public string EPCValue { get; set; } = string.Empty;
        public string ClientCode { get; set; } = string.Empty;
    }

    public class UpdateRfidDto
    {
        public string? EPCValue { get; set; }
        public bool? IsActive { get; set; }
    }
} 