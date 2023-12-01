using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XploreAutomationIntegrations {

    public class AddedTestRunItem {
        public int Index { get; set; }
        public LastTestResult LastTestResult { get; set; }
    }

    public class LastTestResult {
        public int TestCaseId { get; set; }
        public string AssignedTo { get; set; }
    }

    public class AddingTestcasesToTestcyclePayload {
        public int TestRunId { get; set; }
        public List<AddedTestRunItem> AddedTestRunItems { get; set; }
        public List<object> UpdatedTestRunItems { get; set; }
        public List<object> UpdatedTestRunItemsIndexes { get; set; }
        public List<object> DeletedTestRunItems { get; set; }
        public bool AutoReorder { get; set; }

    }
}
