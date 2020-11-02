using Microsoft.Extensions.Configuration;

namespace DBGang.Configuration.SecuredJson
{
    public class SecuredJsonConfigurationSource : FileConfigurationSource
    {
        public string PassPhrase { get; set; }

        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            EnsureDefaults(builder);
            return new SecuredJsonConfigurationProvider(this);
        }
    }
}
