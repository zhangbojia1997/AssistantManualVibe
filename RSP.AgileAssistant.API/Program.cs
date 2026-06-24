using Microsoft.Extensions.Hosting;
using RSP.Common.MVC;

namespace RSP.AgileAssistant.API
{
    /// <summary>
    /// Application entry point. Bootstraps the RSP MVC host using the
    /// <see cref="Startup"/> configuration.
    /// </summary>
    public class Program : ProgramExtension
    {
        /// <summary>
        /// Builds and runs the web host.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        public static void Main(string[] args)
        {
            CreateHostBuilder<Startup>(args)
                .Build()
                .Run();
        }
    }
}
