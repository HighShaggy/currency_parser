using System;

namespace CbrApp.Models
{
    public class ProcessedDateEntity
    {
        public DateTime Date { get; set; }
        public ProcessedDateStatus Status { get; set; }
    }
    public enum ProcessedDateStatus
    {
        Ok,
        Empty,
    }
}
