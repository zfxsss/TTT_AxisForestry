using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTTCommunication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace TTTCommunication.Tests
{
    [TestClass()]
    public class CommTests
    {
        [TestMethod()]
        public async Task StartServerTest()
        {
            var tokenSrc = new CancellationTokenSource();

            try
            {
                var comm = new Comm();
                await comm.StartServer(tokenSrc.Token);
            }
            catch(Exception ex)
            {
                throw ex;
            }
            finally
            {
                tokenSrc.Dispose();
            }
        }
    }
}