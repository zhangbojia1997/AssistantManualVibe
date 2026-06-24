using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using RSP.AgileAssistant.Business.Auth;
using RSP.AgileAssistant.Business.Consts;
using RSP.Common;
using RSP.Common.MVC;

namespace RSP.AgileAssistant.API
{
    /// <summary>
    /// Application startup. Registers IOC dependencies on top of the RSP MVC
    /// framework defaults.
    /// </summary>
    public class Startup : MVCStartup
    {
        /// <summary>
        /// Initializes the startup with the hosting environment.
        /// </summary>
        /// <param name="env">The web hosting environment.</param>
        public Startup(IWebHostEnvironment env)
            : base(env)
        {
        }

        /// <summary>
        /// Registers application IOC dependencies.
        /// </summary>
        public override void RegisterIOC()
        {
            base.RegisterIOC();

            string secret = this.Configuration[ConfigConsts.JWT_SECRET] ?? string.Empty;

            this.IOCInstance.RegisterInstance<ITokenService, TokenService>(
                IocConsts.TOKEN_SERVICE,
                IOCType.Singleton,
                new Dictionary<string, object> { { "secret", secret } });
        }
    }
}
