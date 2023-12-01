using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XploreAutomationIntegrations {
    public class TestCyclePayload {

        public int ProjectId { get; set; }
        public int FolderId { get; set; }
        public string Name { get; set; }
        public int StatusId { get; set; }
        public string PlannedStartDate { get; set; }
        public string PlannedEndDate { get; set; }
        public string Owner { get; set; }

    }
}
