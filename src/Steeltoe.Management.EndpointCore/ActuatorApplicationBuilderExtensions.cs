using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Text;

namespace Steeltoe.Management.Endpoint
{
    public static class ActuatorApplicationBuilderExtensions
    {
        public static void UseActuators(this IApplicationBuilder app)
        {
            //app.UseCors(builder =>
            //{
            //    builder.AllowAnyOrigin()
            //    .WithMethods("GET", "POST")
            //    .WithHeaders("Authorization", "X-Cf-App-Instance", "Content-Type");
            //});

            //app.UseAcuatorSecurity();
            //app.UseDiscoveryActuator();

            //app.UseInfoActuator();
            //app.UseHealthActuator();
  
        }
    }
}
