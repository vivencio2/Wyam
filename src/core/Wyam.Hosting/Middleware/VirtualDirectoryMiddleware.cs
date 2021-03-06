﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Wyam.Hosting.Middleware
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    /// <summary>
    /// Implements OWIN support for mapping virtual directories.
    /// </summary>
    public class VirtualDirectoryMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _virtualDirectory;

        public VirtualDirectoryMiddleware(RequestDelegate next, string virtualDirectory)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }
            if (virtualDirectory == null)
            {
                throw new ArgumentNullException(nameof(virtualDirectory));
            }

            if (!virtualDirectory.StartsWith("/"))
            {
                virtualDirectory = "/" + virtualDirectory;
            }
            virtualDirectory = virtualDirectory.TrimEnd('/');

            _next = next;
            _virtualDirectory = virtualDirectory;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.ToString().StartsWith(_virtualDirectory))
            {
                string realPath = context.Request.Path.ToString().Substring(_virtualDirectory.Length);
                if (realPath == string.Empty)
                {
                    realPath = "/";
                }
                context.Request.Path = new PathString(realPath);
                context.Request.PathBase = new PathString(_virtualDirectory);
                await _next(context);
            }

            // This isn't under our virtual directory, so it should be a not found
            context.Response.StatusCode = 404;
            await context.Response.WriteAsync(string.Empty);
        }
    }
}