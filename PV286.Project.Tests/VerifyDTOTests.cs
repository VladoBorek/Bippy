using BusinessLayer.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PV286.Project.Tests
{
    [TestFixture]
    public class VerifyDTOTests
    {
        [Test]
        public void ToString_WhenValid_ReturnsOK()
        {
            var dto = new VerifyDTO { IsValid = true };

            Assert.That(dto.ToString(), Is.EqualTo("OK"));
        }

        [Test]
        public void ToString_WhenInvalid_ReturnsNOK()
        {
            var dto = new VerifyDTO { IsValid = false };

            Assert.That(dto.ToString(), Is.EqualTo("NOK"));
        }

        [Test]
        public void Property_IsAssignedCorrectly()
        {
            var dto = new VerifyDTO { IsValid = true };

            Assert.That(dto.IsValid, Is.True);
        }
    }
}
