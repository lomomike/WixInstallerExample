using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace ApplicationService.Common
{
    [ServiceContract]
    public interface IApplicationService
    {
        [OperationContract]
        Guid OpenNewSession();

        [OperationContract]
        string GetContextIndentificator(Guid session);
    }

   
}
