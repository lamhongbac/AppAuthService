using MSAUtility;
using Xunit;

namespace AppAuth.Tests
{
    public class SecurityUtilityTests
    {
        [Theory]
        [InlineData("Password123!", true)]
        [InlineData("password", false)] // No upper, no special, too short
        [InlineData("PASSWORD123!", false)] // No lower
        [InlineData("Password!", false)] // No digit
        [InlineData("Short1!", false)] // Too short
        public void IsPasswordStrong_ShouldValidateCorrectly(string password, bool expected)
        {
            var (isValid, _) = SecurityUtility.IsPasswordStrong(password);
            Assert.Equal(expected, isValid);
        }

        [Fact]
        public void HashToken_ShouldProduceConsistentHash()
        {
            string plainText = "MySecretToken";
            string salt = SecurityUtility.GenerateSalt();

            string hash1 = SecurityUtility.HashToken(plainText, salt);
            string hash2 = SecurityUtility.HashToken(plainText, salt);

            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void TrackFailedAttempt_ShouldBlockAfterThreshold()
        {
            string key = "test_user";
            
            // Simulating 5 failed attempts
            for (int i = 0; i < 4; i++)
            {
                var (shouldBlock, _) = SecurityUtility.TrackFailedAttempt(key);
                Assert.False(shouldBlock);
            }

            var (finalBlock, _) = SecurityUtility.TrackFailedAttempt(key);
            Assert.True(finalBlock);
        }
    }
}
