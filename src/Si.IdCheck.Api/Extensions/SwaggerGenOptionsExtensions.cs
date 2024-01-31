﻿using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;
// ReSharper restore CheckNamespace

public static class SwaggerGenOptionsExtensions
{
    /// <summary>
    ///     If xml comments exist, display in swagger ui.
    /// </summary>
    public static SwaggerGenOptions IncludeXmlCommentsIfExists(this SwaggerGenOptions options, Assembly assembly)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));

        if (assembly == null) throw new ArgumentNullException(nameof(assembly));

        var filePath = Path.ChangeExtension(assembly.Location, ".xml");

        options.IncludeXmlCommentsIfExists(filePath);

        return options;
    }

    /// <summary>
    ///     If xml comments exist, display in swagger ui.
    /// </summary>
    public static bool IncludeXmlCommentsIfExists(this SwaggerGenOptions options, string filePath)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));

        if (filePath == null) throw new ArgumentNullException(nameof(filePath));

        if (!File.Exists(filePath)) return false;

        options.IncludeXmlComments(filePath);

        return true;
    }
}