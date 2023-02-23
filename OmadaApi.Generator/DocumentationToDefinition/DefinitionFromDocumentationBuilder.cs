namespace OmadaApi.Generator.DocumentationToDefinition;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using OmadaApi.Generator.ApiDefinition;
using OmadaApi.Generator.ApiDocumentationReader;

internal class DefinitionFromDocumentationBuilder
{
    private readonly IPathTokenizer pathTokenizer;
    private readonly Dictionary<PermissionLevel, ApiClassDefinition> classDefinitions = new();

    public DefinitionFromDocumentationBuilder(IPathTokenizer? pathTokenizer = null)
    {
        this.pathTokenizer = pathTokenizer ?? new PathTokenizer();
    }

    public void AddDocumentation(ApiDocumentation apiDocumentation)
    {
        foreach (var section in apiDocumentation.Sections)
        {
            this.AddSection(section);
        }
    }

    public void AddSection(ApiDocumentationSection apiDocumentationSection)
    {
        foreach (var endpoint in apiDocumentationSection.Endpoints)
        {
            this.AddEndpoint(endpoint);
        }
    }

    public void AddEndpoint(ApiEndpointDocumentation apiEndpointDocumentation)
    {
        var permissionLevel = this.GetPermissionLevel(apiEndpointDocumentation);
        ApiClassDefinition root;
        if (!this.classDefinitions.TryGetValue(permissionLevel, out root))
        {
            string rootName = this.GetPermissionLevelClassNamePrefix(permissionLevel) + "OmadaController";
            root = new ApiClassDefinition(rootName);
            this.classDefinitions[permissionLevel] = root;
        }

        var tokens = this.pathTokenizer.GetTokens(apiEndpointDocumentation.Path).ToImmutableList();

        this.AddEndpoint(root, tokens, apiEndpointDocumentation);
    }

    private void AddEndpoint(ApiClassDefinition @class, ImmutableList<PathToken> tokens, ApiEndpointDocumentation apiEndpointDocumentation)
    {
        bool isLast = tokens.Count == 1;

        var token = tokens.First();
        string name = this.ToTitleCase(token.Name);
        string permissionLevelPrefix = this.GetPermissionLevelClassNamePrefix(this.GetPermissionLevel(apiEndpointDocumentation));
        ApiClassDefinition? nextClass = null;

        if (!isLast)
        {
            if (tokens[0].HasParameter)
            {
                name = this.ToSingular(name);
                string methodName = "Get" + name;
                var existing = @class.ClassMethods.FirstOrDefault(cm => cm.Name == methodName);
                if (existing is null)
                {
                    // Got default, so must be a new entry
                    string returnedClassName = permissionLevelPrefix + name;
                    var newClass = new ApiClassDefinition(returnedClassName);
                    existing = new(methodName, token.ParameterName!, newClass);
                    @class.ClassMethods.Add(existing);
                }

                nextClass = existing.ClassDefinition;
            }
            else
            {
                var existing = @class.ClassProperties.FirstOrDefault(cp => cp.Name == name);
                if (existing is null)
                {
                    // Got default, so must be new entry
                    string returnedClassName = permissionLevelPrefix + name;
                    var newClass = new ApiClassDefinition(returnedClassName);
                    existing = new(name, newClass);
                }

                nextClass = existing.ClassDefinition;
            }
        }

        if (nextClass != null)
        {
            this.AddEndpoint(nextClass, tokens.RemoveAt(0), apiEndpointDocumentation);
        }
    }

    private string ToTitleCase(string name)
    {
        return char.ToUpperInvariant(name[0]) + name.Substring(1);
    }

    private string ToSingular(string pluralName)
    {
        if (!pluralName.EndsWith("s"))
        {
            throw new NotSupportedException("Too bad, need more advanced calculation of singular names");
        }

        return pluralName;
    }

    private PermissionLevel GetPermissionLevel(ApiEndpointDocumentation apiEndpointDocumentation)
    {
        if (apiEndpointDocumentation.Permissions.IndexOf("All levels", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return PermissionLevel.All;
        }

        if (apiEndpointDocumentation.HttpMethod.Method == "GET")
        {
            return PermissionLevel.Read;
        }

        return PermissionLevel.Admin;
    }

    private string GetPermissionLevelClassNamePrefix(PermissionLevel permissionLevel)
    {
        return permissionLevel switch
        {
            PermissionLevel.All => "Unauthenticated",
            PermissionLevel.Read => "ReadAuthorized",
            PermissionLevel.Admin => "AdminAuthorized",
            _ => throw new NotSupportedException("Unknown permission level: " + permissionLevel),
        };
    }
}
