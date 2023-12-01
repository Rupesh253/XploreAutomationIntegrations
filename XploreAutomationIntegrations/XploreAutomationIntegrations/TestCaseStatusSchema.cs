using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XploreAutomationIntegrations {
    internal class TestCaseStatusSchema {
        public static string UserKey { get; set; }
        public static string ProjectId { get; set; }
        public string TestCaseId { get; set; }
        public string ExecutionId { get; set; }
        public string Key { get; set; }

        public string ExecutionDate { get; set; }
        public string? Status { get; set; }
        public string? ImageLocation { get; set; }
        public string? Exception { get; set; }
    }
}
