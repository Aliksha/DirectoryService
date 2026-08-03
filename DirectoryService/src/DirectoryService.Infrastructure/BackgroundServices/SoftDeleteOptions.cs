using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryService.Infrastructure.BackgroundServices
{
    public class SoftDeleteOptions
    {
        public const string Position = "SoftDeleteSettings";

        public int IntervalHours { get; set; } = 24;
        public int ExpirationDays { get; set; } = 180;
        public int BatchSize { get; set; } = 500;
    }
}
