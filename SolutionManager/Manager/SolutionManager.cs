using Microsoft.Xrm.Sdk;
using SolutionManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolutionManager.Manager
{
    public class MySolutionManager
    {
        
        public Solution ConvertToSolution(Entity entity)
        {
            Solution s = new Solution();
            s.SolutionId = entity.Id;
            s.FriendlyName = entity.GetAttributeValue<string>("friendlyname");
            s.UniqueName = entity.GetAttributeValue<string>("uniquename");
            s.Description = entity.GetAttributeValue<string>("description");
            s.Version = entity.GetAttributeValue<string>("version");
            s.InstalledOn = (string)entity.GetAttributeValue<DateTime>("installedon").ToString();
            
            return s;
        }
    }
}
