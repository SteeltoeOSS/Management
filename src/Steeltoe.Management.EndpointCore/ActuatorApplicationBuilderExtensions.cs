using Microsoft.AspNetCore.Builder;
using Steeltoe.Management.Endpoint.Discovery;
using Steeltoe.Management.Endpoint.Health;
using Steeltoe.Management.Endpoint.Info;
using System;
using System.Collections.Generic;
using System.Text;

namespace Steeltoe.Management.Endpoint
{
    public static class ActuatorApplicationBuilderExtensions
    {
        public static void UseActuators(this IApplicationBuilder app)
        {
            app.UseCors(builder =>
            {
                builder.AllowAnyOrigin()
                .WithMethods("GET", "POST")
                .WithHeaders("Authorization", "X-Cf-App-Instance", "Content-Type");
            });

           // app.UseAcuatorSecurity();
            app.UseDiscoveryActuator();

            app.UseInfoActuator();
            app.UseHealthActuator();

        }
    }
}
