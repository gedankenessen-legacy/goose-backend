using Goose.API.Utils.Validators;
using NUnit.Framework;

namespace Goose.Tests.Application.UnitTests.Validators
{
    [TestFixture]
    public class PasswordValidatorAttributeTests
    {
        [Test]
        public void MinLengthIsValidTest()
        {
            string password = "123456";

            var attr = new PasswordValidatorAttribute(6, false);

            var result = attr.IsValid(password);

            Assert.That(result, Is.True, $"{nameof(PasswordValidatorAttribute)} returned false for a valid password.");
        }

        [Test]
        public void MinLengthIsNotValidTest()
        {
            string password = "12345";

            var attr = new PasswordValidatorAttribute(6, false);

            var result = attr.IsValid(password);

            Assert.That(result, Is.False, $"{nameof(PasswordValidatorAttribute)} returned true for a invalid password.");
        }

        [Test]
        public void ContainsNumberIsValidTest()
        {
            string password = "123456a";

            var attr = new PasswordValidatorAttribute(6, true);

            var result = attr.IsValid(password);

            Assert.That(result, Is.True, $"{nameof(PasswordValidatorAttribute)} returned false for a valid password.");
        }

        [Test]
        public void ContainsNumberIsNotValidTest()
        {
            string password = "abcdef";

            var attr = new PasswordValidatorAttribute(6, true);

            var result = attr.IsValid(password);

            Assert.That(result, Is.False, $"{nameof(PasswordValidatorAttribute)} returned true for a invalid password.");
        }
    }
}
