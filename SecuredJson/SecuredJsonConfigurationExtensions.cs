using System;
using System.IO;
using Microsoft.Extensions.FileProviders;
using DBGang.Configuration.SecuredJson;

namespace Microsoft.Extensions.Configuration
{
    public static class SecuredJsonConfigurationExtensions
    {
        public static Stream CreateWriteStream(this IFileInfo file)
        {
            return new FileStream(file.PhysicalPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite, 1);
        }

        public static IConfigurationBuilder AddSecuredJsonFile(this IConfigurationBuilder builder, string path)
        {
            return AddSecuredJsonFile(builder, provider: null, path: path, passPhrase: AppDomain.CurrentDomain.FriendlyName, optional: false, reloadOnChange: false);
        }

        public static IConfigurationBuilder AddSecuredJsonFile(this IConfigurationBuilder builder, string path, string passPhrase)
        {
            return AddSecuredJsonFile(builder, provider: null, path: path, passPhrase: passPhrase, optional: false, reloadOnChange: false);
        }

        public static IConfigurationBuilder AddSecuredJsonFile(this IConfigurationBuilder builder, string path, string passPhrase, bool optional)
        {
            return AddSecuredJsonFile(builder, provider: null, path: path, passPhrase: passPhrase, optional: optional, reloadOnChange: false);
        }

        public static IConfigurationBuilder AddSecuredJsonFile(this IConfigurationBuilder builder, string path, string passPhrase, bool optional, bool reloadOnChange)
        {
            return AddSecuredJsonFile(builder, provider: null, path: path, passPhrase: passPhrase, optional: optional, reloadOnChange: reloadOnChange);
        }

        public static IConfigurationBuilder AddSecuredJsonFile(this IConfigurationBuilder builder, IFileProvider provider, string path, string passPhrase, bool optional, bool reloadOnChange)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            return builder.AddSecuredJsonFile(source =>
            {
                source.FileProvider = provider;
                source.Path = path;
                source.PassPhrase = passPhrase;
                source.Optional = optional;
                source.ReloadOnChange = reloadOnChange;
                source.ResolveFileProvider();
            });
        }

        public static IConfigurationBuilder AddSecuredJsonFile(this IConfigurationBuilder builder, Action<SecuredJsonConfigurationSource> configureSource)
            => builder.Add(configureSource);
    }
}
