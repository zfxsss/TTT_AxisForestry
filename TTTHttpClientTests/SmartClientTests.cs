using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTTHttpClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTTHttpClient.Tests
{
    [TestClass()]
    public class SmartClientTests
    {
        [TestMethod()]
        public void GetMyRecommendationTest()
        {
            var x = SmartClient.GetMyRecommendation("---------", "O").Result;
        }
    }
}