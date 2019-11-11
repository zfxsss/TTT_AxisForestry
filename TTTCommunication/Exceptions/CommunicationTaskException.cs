using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTTCommunication.Exceptions
{
    /// <summary>
    /// 
    /// </summary>
    public class CommunicationTaskException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ex"></param>
        public CommunicationTaskException(Exception ex) : base("CommunicationTaskException Raised", ex)
        {
        }
    }
}
