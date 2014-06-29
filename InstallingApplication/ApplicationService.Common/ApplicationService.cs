using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace ApplicationService.Common
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "ApplicationService" in both code and config file together.
    public class ApplicationService : IApplicationService
    {
        public Guid OpenNewSession()
        {
            return Guid.NewGuid();
        }

        public string GetContextIndentificator(Guid session)
        {
            var rand = new Random();
            return session + "_" + rand.Next();
        }
    }
}
