using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EndlessRealms.Core.Utility;
public class EndlessRealmsException: ApplicationException
{
    public EndlessRealmsException(string message) : base(message) { }    
    public EndlessRealmsException(string message, Exception innerException) : base(message, innerException) { }
}
