using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace SimpleInject.SourceGenerators
{

    [Generator]
    public sealed class InjectionFieldSrcGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is not SyntaxReceiver receiver)
                return;

            //Log($"#### STARTING assembly:{context.Compilation.AssemblyName} candidate {receiver.CandidateClasses.Count} ####");

            var generatedStructs = new HashSet<string>();
            var generatedClasses = new HashSet<string>();

            var services = new List<INamedTypeSymbol>();
            var signals = new List<INamedTypeSymbol>();

            var iServiceSymbol = context.Compilation.GetTypeByMetadataName("Injector.IService");
            var iSignalSymbol = context.Compilation.GetTypeByMetadataName("Injector.ISignal");

            foreach (var classDecl in receiver.CandidateClasses)
            {
                signals.Clear();
                var semanticModel = context.Compilation.GetSemanticModel(classDecl.SyntaxTree);

                if (semanticModel.GetDeclaredSymbol(classDecl) is not INamedTypeSymbol classSymbol)
                    continue;

                if (generatedClasses.Contains(classSymbol.Name))
                    continue;

                generatedClasses.Add(classSymbol.Name);

                var injectAttr = classSymbol.GetAttributes()
                    .FirstOrDefault(a =>
                        a.AttributeClass?.ToDisplayString() == "Injector.InjectAttribute");

                if (injectAttr == null)
                    continue;

                var userClassNameSpace = classSymbol.ContainingNamespace.ToDisplayString();

                var fieldNamespaces = new HashSet<string>();

                foreach (var arg in injectAttr.ConstructorArguments)
                {
                    foreach (var element in arg.Values)
                    {
                        if (element.Value is not INamedTypeSymbol typeSymbol)
                            continue;

                        var key = typeSymbol.ToDisplayString();
                        //Log($"#### Try to generate struct {key} STARTING assembly:{context.Compilation.AssemblyName} ####");
                        if (generatedStructs.Contains(key))
                            continue;

                        bool isService = typeSymbol.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, iServiceSymbol)) || SymbolEqualityComparer.Default.Equals(typeSymbol, iServiceSymbol);
                        bool isSignal = typeSymbol.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, iSignalSymbol)) || SymbolEqualityComparer.Default.Equals(typeSymbol, iSignalSymbol);

                        if (isService)
                        {
                            if (generatedStructs.Add(key))
                                GenerateStruct(context, typeSymbol, classSymbol, fieldNamespaces);
                        }
                        else if (isSignal)
                        {
                            signals.Add(typeSymbol);
                        }
                    }
                }

                // GeneratePartial(context, classSymbol, injectAttr, fieldNamespaces);
                foreach (var typeSymbol in services.Concat(signals))
                {
                    fieldNamespaces.Add(typeSymbol.ContainingNamespace.ToDisplayString());
                    foreach (var iface in typeSymbol.AllInterfaces)
                        fieldNamespaces.Add(iface.ContainingNamespace.ToDisplayString());
                }

                GeneratePartial(context, classSymbol, injectAttr, fieldNamespaces, signals);
            }
        }

        // =========================================================
        // STRUCT PROXY
        // =========================================================
        private void GenerateStruct(
            GeneratorExecutionContext context,
            INamedTypeSymbol interfaceSymbol,
            INamedTypeSymbol classSymbol,
            HashSet<string> fieldNamespaceHash)
        {

            var fullInterfaceName = interfaceSymbol.ToDisplayString();
            var interfacesToImplement = new List<INamedTypeSymbol> { interfaceSymbol };
            interfacesToImplement.AddRange(interfaceSymbol.AllInterfaces.Where(i => !SymbolEqualityComparer.Default.Equals(i, interfaceSymbol)));
            var interfaceName = interfaceSymbol.Name;

            var cleanName = interfaceName.StartsWith("I") &&
                            interfaceName.Length > 1
                ? interfaceName.Substring(1)
                : interfaceName;

            var structName = cleanName + "InjectionField";
            var usingNamespaceName = interfaceSymbol.ContainingNamespace.ToDisplayString();

            var allNamespaces = interfacesToImplement.Select(i => i.ContainingNamespace.ToDisplayString()).Distinct();

            var classNamespace = classSymbol.ContainingNamespace.ToDisplayString()
;
            fieldNamespaceHash.Add(classNamespace);
            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated>");
            sb.AppendLine("// Generated by InjectionFieldSrcGenerator");
            sb.AppendLine($"// Target Class Assembly: {interfaceSymbol.ContainingAssembly.Name}");
            sb.AppendLine($"// Assembly: {context.Compilation.AssemblyName}");
            sb.AppendLine("// </auto-generated>");
            sb.AppendLine();
            sb.AppendLine($"using Injector;");
            foreach (var ns in allNamespaces)
                sb.AppendLine($"using {ns};");
            sb.AppendLine($"namespace {classNamespace}");
            sb.AppendLine("{");
            sb.AppendLine("    public readonly struct Singleton" + structName + " : " + string.Join(", ", interfacesToImplement.Select(i => i.ToDisplayString())));
            sb.AppendLine("    {");
            // sb.AppendLine("        public static " + structName + " Create()");
            // sb.AppendLine($"            => new  {structName}(ServiceLocator.Instance.Resolve<{fullInterfaceName}>());");
            sb.AppendLine();
            sb.AppendLine("        private static " + fullInterfaceName + " _serviceCache ;");
            sb.AppendLine($"        private {fullInterfaceName} _service =>  _serviceCache ??= ServiceLocator.Instance.Resolve<{fullInterfaceName}>();");
            sb.AppendLine();
            // sb.AppendLine("        public " + structName + "(" + fullInterfaceName + " service)");
            // sb.AppendLine("        {");
            // sb.AppendLine("            _service = service;");
            // sb.AppendLine("        }");

            // =====================
            // MÉTODOS
            // =====================
            foreach (var iface in interfacesToImplement)
            {
                foreach (var method in iface.GetMembers().OfType<IMethodSymbol>())
                {
                    if (method.MethodKind != MethodKind.Ordinary)
                        continue;

                    var returnType = method.ReturnType.ToDisplayString();
                    var methodName = method.Name;

                    var genericParams = method.IsGenericMethod
                        ? "<" + string.Join(", ", method.TypeParameters.Select(t => t.Name)) + ">"
                        : "";

                    var parameters = string.Join(", ",
                        method.Parameters.Select(p =>
                        {
                            string modifier = "";
                            if (p.RefKind == RefKind.Ref) modifier = "ref ";
                            else if (p.RefKind == RefKind.Out) modifier = "out ";
                            else if (p.RefKind == RefKind.In) modifier = "in ";

                            return modifier + p.Type.ToDisplayString() + " " + p.Name;
                        }));

                    var arguments = string.Join(", ",
                        method.Parameters.Select(p =>
                        {
                            string modifier = "";
                            if (p.RefKind == RefKind.Ref) modifier = "ref ";
                            else if (p.RefKind == RefKind.Out) modifier = "out ";
                            else if (p.RefKind == RefKind.In) modifier = "in ";

                            return modifier + p.Name;
                        }));

                    sb.AppendLine();
                    sb.AppendLine("        public " + returnType + " " + methodName + genericParams + "(" + parameters + ")");
                    sb.AppendLine("            => _service." + methodName + genericParams + "(" + arguments + ");");
                }
            }

            // =====================
            // PROPRIEDADES
            // =====================
            foreach (var iface in interfacesToImplement)
            {
                foreach (var prop in iface.GetMembers().OfType<IPropertySymbol>())
                {
                    sb.AppendLine();
                    sb.AppendLine("        public " + prop.Type.ToDisplayString() + " " + prop.Name);
                    sb.AppendLine("        {");

                    if (prop.GetMethod != null)
                        sb.AppendLine("            get => _service." + prop.Name + ";");

                    if (prop.SetMethod != null)
                        sb.AppendLine("            set => _service." + prop.Name + " = value;");

                    sb.AppendLine("        }");
                }
            }

            // =====================
            // EVENTOS
            // =====================
            foreach (var iface in interfacesToImplement)
            {
                foreach (var ev in iface.GetMembers().OfType<IEventSymbol>())
                {
                    sb.AppendLine();
                    sb.AppendLine("        public event " + ev.Type.ToDisplayString() + " " + ev.Name);
                    sb.AppendLine("        {");
                    sb.AppendLine("            add => _service." + ev.Name + " += value;");
                    sb.AppendLine("            remove => _service." + ev.Name + " -= value;");
                    sb.AppendLine("        }");
                }
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");
            //Log(sb.ToString());
            context.AddSource(structName + $"_{context.Compilation.AssemblyName}" + ".g.cs",
                SourceText.From(sb.ToString(), Encoding.UTF8));

        }

        // =========================================================
        // PARTIAL CLASS
        // =========================================================

        private void GeneratePartial(
            GeneratorExecutionContext context,
            INamedTypeSymbol classSymbol,
            AttributeData attribute,
            HashSet<string> fieldNamespaceCollection,
            List<INamedTypeSymbol> signals)
        {
            var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
            var className = classSymbol.Name;
            var iSignalSymbol = context.Compilation.GetTypeByMetadataName("Injector.ISignal");
            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated>");
            sb.AppendLine("// Generated by InjectionFieldSrcGenerator");
            sb.AppendLine($"// Target Class Assembly: {classSymbol.ContainingAssembly.Name}");
            sb.AppendLine($"// Assembly: {context.Compilation.AssemblyName}");
            sb.AppendLine("// </auto-generated>");
            foreach (var fieldNamespace in fieldNamespaceCollection)
            {
                sb.AppendLine($"using {fieldNamespace};");
            }

            sb.AppendLine("namespace " + namespaceName);
            sb.AppendLine("{");
            sb.AppendLine("    public partial class " + className);
            sb.AppendLine("    {");

            foreach (var arg in attribute.ConstructorArguments)
            {
                foreach (var element in arg.Values)
                {
                    if (element.Value is not INamedTypeSymbol typeSymbol)
                        continue;

                    bool isSignal = typeSymbol.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, iSignalSymbol)) || SymbolEqualityComparer.Default.Equals(typeSymbol, iSignalSymbol);
                    if (isSignal)
                    {
                        continue;
                    }

                    var interfaceName = typeSymbol.Name;


                    var cleanName = interfaceName.StartsWith("I") &&
                                    interfaceName.Length > 1
                        ? interfaceName.Substring(1)
                        : interfaceName;


                    var structName = $"Singleton{cleanName}InjectionField";
                    var propertyName = cleanName;

                    sb.AppendLine();
                    // sb.AppendLine("        private " + structName + " " + propertyName + " { get; } = " + structName + ".Create();");
                    sb.AppendLine("        private " + structName + " " + propertyName + " { get; }");
                }
            }

            foreach (var typeSymbol in signals)
            {
                var typeName = typeSymbol.Name;
                var fullTypeName = typeSymbol.ToDisplayString();
                var fieldNamespaceName = typeSymbol.ContainingNamespace.ToDisplayString();

                sb.AppendLine();
                sb.AppendLine("        public " + fullTypeName + " " + typeName);
                sb.AppendLine("        {");
                sb.AppendLine("            get => Locator.Instance.SignalLocator.GetSignal<" + fullTypeName + ">();");
                sb.AppendLine("        }");
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            // Log(sb.ToString());

            context.AddSource($"{className}_{context.Compilation.AssemblyName}.Inject.g.cs",
                SourceText.From(sb.ToString(), Encoding.UTF8));
        }

        // =========================================================
        // SYNTAX RECEIVER
        // =========================================================

        private class SyntaxReceiver : ISyntaxReceiver
        {
            public List<ClassDeclarationSyntax> CandidateClasses { get; }
                = new List<ClassDeclarationSyntax>();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                var classDecl = syntaxNode as ClassDeclarationSyntax;
                if (classDecl == null)
                    return;

                if (classDecl.AttributeLists.Count > 0)
                    CandidateClasses.Add(classDecl);


            }
        }

        private static readonly string LogPath =
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    "inject_generator.log");

        public void Log(string message)
        {
            try
            {
                if (!File.Exists(LogPath))
                    File.Create(LogPath);

                var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] - {i++} - {message}{Environment.NewLine}";
                File.AppendAllText(LogPath, line, Encoding.UTF8);
            }
            catch
            {

            }
        }

        int i;
    }
}