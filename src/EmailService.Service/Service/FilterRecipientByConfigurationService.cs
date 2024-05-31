using EmailService.Domain;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace EmailService.Service
{
    public class FilterRecipientByConfigurationService : IFilterRecipientService
    {
        private readonly FilterRecipientOptions filterRecipientOptions;
        public FilterRecipientByConfigurationService(
            IOptions<FilterRecipientOptions> filterRecipientOptions)
        {
            this.filterRecipientOptions = filterRecipientOptions.Value;
        }

        public Task<bool> FilterRecipientAsync(EmailMessage emailMessage)
        {
            var blockList = filterRecipientOptions?.BlockList ?? [];
            var allowList = filterRecipientOptions?.AllowList ?? [];

            var blockHashSet = new HashSet<string>(blockList, StringComparer.OrdinalIgnoreCase);
            var allowHashSet = new HashSet<string>(allowList, StringComparer.OrdinalIgnoreCase);

            emailMessage.To.RemoveAll(t => IsMatched(blockHashSet, t) || !IsMatched(allowHashSet, t));

            if (emailMessage.To.Count == 0)
            {
                return Task.FromResult(false);
            }

            if (emailMessage.Cc != null && emailMessage.Cc.Count > 0)
            {
                emailMessage.Cc.RemoveAll(t => IsMatched(blockHashSet, t) || !IsMatched(allowHashSet, t));
            }
            if (emailMessage.Bcc != null && emailMessage.Bcc.Count > 0)
            {
                emailMessage.Bcc.RemoveAll(t => IsMatched(blockHashSet, t) || !IsMatched(allowHashSet, t));
            }

            return Task.FromResult(true);
        }

        private static bool IsMatched(HashSet<string> patterns, string email)
        {
            if (patterns.Contains("*"))
            {
                return true;
            }

            foreach (var pattern in patterns)
            {
                string regexPattern = Regex.Escape(pattern).Replace("*", ".*");
                if (Regex.IsMatch(email, regexPattern, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1)))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
