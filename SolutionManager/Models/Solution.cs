using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolutionManager.Models
{
    public class Solution
    {
        public string EntityName { get { return "solution"; } }
        public Guid SolutionId { get; set; }
        public string UniqueName { get; set; }
        public string FriendlyName { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }

        #region Constructor

        public Solution()
        {

        }

        #endregion Constructor
    }
}
