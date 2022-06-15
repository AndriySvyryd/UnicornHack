using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSharpScriptSerialization;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;
using UnicornHack.Utils.DataLoading;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace UnicornHack.Editor;

public class CSClassSerializer : CSScriptSerializer
{
    private Solution _solution;
    private string _directory;
    private (ImmutableArray<DiagnosticAnalyzer>, ImmutableArray<CodeFixProvider>) _analyzersAndFixers;

    public CSClassSerializer() : base(typeof(object))
    {
    }

    public override ExpressionSyntax GetCreation(object obj) => throw new NotImplementedException();

    public async Task InitializeAsync(Assembly referencedAssembly, string directory)
    {
        _directory = directory;
        var workspace = new AdhocWorkspace();
        var editorConfigText = SourceText.From(
            await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, ".editorconfig")),
            Encoding.UTF8);

        var projectId = ProjectId.CreateNewId();
        var editorConfigDocument = DocumentInfo.Create(
            DocumentId.CreateNewId(projectId),
            name: ".editorconfig",
            loader: TextLoader.From(TextAndVersion.Create(editorConfigText, VersionStamp.Create())),
            filePath: Path.Combine(directory, ".editorconfig"));

        var workspacePath = Path.Combine(AppContext.BaseDirectory, "AnalyzerProject.csproj");
        var exitCode = await CreateProcess("dotnet", $"restore \"{workspacePath}\"");
        Debug.Assert(0 == exitCode);

        MSBuildLocator.RegisterDefaults();
        var analyzerWorkspace = MSBuildWorkspace.Create(new Dictionary<string, string>(StringComparer.Ordinal)
        {
            { "AlwaysCompileMarkupFilesInSeparateDomain", bool.FalseString },
        });

        await analyzerWorkspace.OpenProjectAsync(workspacePath).ConfigureAwait(false);

        var analyzerProject = analyzerWorkspace.CurrentSolution.Projects.Single();

        var projectName = "InMemoryProject";
        var project = workspace.AddProject(ProjectInfo.Create(
                projectId,
                VersionStamp.Create(),
                projectName, projectName,
                LanguageNames.CSharp,
                compilationOptions: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                metadataReferences: analyzerProject.MetadataReferences.Concat(
                    new[] { MetadataReference.CreateFromFile(referencedAssembly.Location) }),
                analyzerReferences: analyzerProject.AnalyzerReferences)
            .WithAnalyzerConfigDocuments(ImmutableArray.Create(editorConfigDocument)));
        _solution = project.Solution;

        _analyzersAndFixers = LoadAnalyzersAndFixers(project.AnalyzerReferences);
    }

    public static Task<int> CreateProcess(string executable, string arguments)
    {
        var process = new Process();
        var tcs = new TaskCompletionSource<int>();

        process.EnableRaisingEvents = true;
        process.StartInfo = new ProcessStartInfo(executable, arguments)
        {
            CreateNoWindow = true, UseShellExecute = false
        };

        process.Exited += (_, _) =>
        {
            // We must call WaitForExit to make sure we've received all OutputDataReceived/ErrorDataReceived calls
            // or else we'll be returning a list we're still modifying. For paranoia, we'll start a task here rather
            // than enter right back into the Process type and start a wait which isn't guaranteed to be safe.
            Task.Run(() =>
            {
                process.WaitForExit();
                tcs.TrySetResult(process.ExitCode);
            });
        };

        process.Start();

        return tcs.Task;
    }

    public static (ImmutableArray<DiagnosticAnalyzer>, ImmutableArray<CodeFixProvider>) LoadAnalyzersAndFixers(
        IReadOnlyList<AnalyzerReference> analyzerReferences)
    {
        var assemblies = new[]
        {
            Assembly.Load("Microsoft.CodeAnalysis.Features"),
            Assembly.Load("Microsoft.CodeAnalysis.CSharp.Features")
        };

        var analyzerAssemblies = analyzerReferences
            .Select(reference => reference is AnalyzerFileReference analyzerFileReference
                ? analyzerFileReference.GetAssembly()
                : Assembly.Load(reference.FullPath!));

        var types = assemblies
            .Concat(analyzerAssemblies)
            .SelectMany(assembly => assembly.GetTypes()
                .Where(type => !type.GetTypeInfo().IsInterface &&
                               !type.GetTypeInfo().IsAbstract &&
                               !type.GetTypeInfo().ContainsGenericParameters))
            .ToList();

        var codeFixProviders = types
            .Where(t => typeof(CodeFixProvider).IsAssignableFrom(t))
            .Select(type => type.TryCreateInstance<CodeFixProvider>(out var instance) ? instance : null)
            .Where(p => p != null)
            .ToImmutableArray();

        var diagnosticAnalyzers = types
            .Where(t => typeof(DiagnosticAnalyzer).IsAssignableFrom(t))
            .Select(type => type.TryCreateInstance<DiagnosticAnalyzer>(out var instance) ? instance : null)
            .Where(d => d != null)
            .ToImmutableArray();

        return new(diagnosticAnalyzers, codeFixProviders);
    }

    public async Task<string> SerializeAsync(
        object obj,
        string targetPropertyName,
        string targetNamespace,
        string targetClassName)
    {
        var expression = CompilationUnit()
            .WithUsings(List(CSScriptLoaderHelpers.Namespaces.Select(n => UsingDirective(ParseName(n)))))
            .WithMembers(
                SingletonList<MemberDeclarationSyntax>(
                    FileScopedNamespaceDeclaration(ParseName(targetNamespace))
                        .WithMembers(
                            SingletonList<MemberDeclarationSyntax>(ClassDeclaration(targetClassName)
                                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword),
                                    Token(SyntaxKind.StaticKeyword),
                                    Token(SyntaxKind.PartialKeyword)))
                                .WithMembers(SingletonList<MemberDeclarationSyntax>(
                                    FieldDeclaration(
                                            VariableDeclaration(GetTypeSyntax(obj.GetType()))
                                                .WithVariables(SingletonSeparatedList(
                                                    VariableDeclarator(Identifier(
                                                            CSScriptLoaderHelpers.GenerateIdentifier(
                                                                targetPropertyName)))
                                                        .WithInitializer(
                                                            EqualsValueClause(GetCreationExpression(obj))))))
                                        .WithModifiers(
                                            TokenList(Token(SyntaxKind.PublicKeyword),
                                                Token(SyntaxKind.StaticKeyword),
                                                Token(SyntaxKind.ReadOnlyKeyword)))))))));

        var project = _solution.Projects.Single();

        var documentId = DocumentId.CreateNewId(project.Id, targetClassName + ".cs");
        var solution = _solution.AddDocument(documentId, targetClassName + ".cs",
            expression,
            filePath: Path.Combine(_directory, targetClassName + ".cs"));

        var document = solution.GetProject(project.Id)!.GetDocument(documentId)!;

        document = await Formatter.FormatAsync(document, await document.GetOptionsAsync());
        document = await Formatter.OrganizeImportsAsync(document);
        var sourceText = await EnsureEndingEmptyLine(document);

        solution = solution.WithDocumentText(document.Id, sourceText, PreservationMode.PreserveIdentity);
        var result = await ApplyCodeFixes(solution.GetProject(project.Id)!.GetDocument(document.Id));

        _solution = _solution.RemoveDocument(document!.Id);
        return result.ToString();
    }

    private static async Task<SourceText> EnsureEndingEmptyLine(Document document)
    {
        var sourceText = await document.GetTextAsync();

        var lastLine = sourceText.Lines[^1];
        if (!lastLine.Span.IsEmpty)
        {
            var finalNewlineSpan = new TextSpan(lastLine.End, 0);
            var addNewlineChange = new TextChange(finalNewlineSpan, Environment.NewLine);
            sourceText = sourceText.WithChanges(addNewlineChange);
        }

        return sourceText;
    }

    private async Task<SourceText> ApplyCodeFixes(Document document)
    {
        var project = document.Project;
        var solution = project.Solution;

        var diagnostics = await GetDiagnostics(project, document.FilePath, _analyzersAndFixers.Item1);

        var fixersById = diagnostics.Select(d => d.Id).Distinct().ToDictionary(
            id => id,
            id => _analyzersAndFixers.Item2
                .Where(fixer
                    => (id == "IDE0005" && fixer.FixableDiagnosticIds.Contains("RemoveUnnecessaryImportsFixable"))
                       || fixer.FixableDiagnosticIds.Contains(id)));
        foreach (var codeFixes in fixersById)
        {
            foreach (var codeFix in codeFixes.Value)
            {
                var diagnosticId = codeFixes.Key;
                var fixAllProvider = codeFix.GetFixAllProvider();
                if (fixAllProvider?.GetSupportedFixAllScopes().Contains(FixAllScope.Document) != true)
                {
                    continue;
                }

                var diagnosticsList = (await GetDiagnostics(project, document!.FilePath, _analyzersAndFixers.Item1))
                    .Where(diagnostic => diagnostic.Id == diagnosticId)
                    .ToList();

                if (diagnosticsList.Count == 0)
                {
                    continue;
                }

                CodeAction action = null;
                var context = new CodeFixContext(document, diagnosticsList[0],
                    (a, _) => action ??= a,
                    CancellationToken.None);

                await codeFix.RegisterCodeFixesAsync(context).ConfigureAwait(false);

                var fixAllContext = new FixAllContext(
                    document: document,
                    codeFixProvider: codeFix,
                    scope: FixAllScope.Document,
                    codeActionEquivalenceKey: action?.EquivalenceKey!,
                    diagnosticIds: new[] { diagnosticId },
                    fixAllDiagnosticProvider: new DiagnosticProviderAdapter(
                        new Dictionary<Project, List<Diagnostic>> { { project, diagnosticsList } }),
                    cancellationToken: CancellationToken.None);

                var fixAllAction = await fixAllProvider.GetFixAsync(fixAllContext).ConfigureAwait(false);
                if (fixAllAction is null)
                {
                    continue;
                }

                var operations =
                    await fixAllAction.GetOperationsAsync(CancellationToken.None).ConfigureAwait(false);
                var applyChangesOperation = operations.OfType<ApplyChangesOperation>().SingleOrDefault();
                if (applyChangesOperation is null)
                {
                    continue;
                }

                var changedSolution = applyChangesOperation.ChangedSolution;
                if (changedSolution.GetChanges(solution).GetProjectChanges().Any())
                {
                    solution = changedSolution;
                    project = solution.GetProject(project.Id);
                    document = project!.GetDocument(document.Id);
                }
            }
        }

        _solution = solution;
        return await document!.GetTextAsync();
    }

    private static async Task<IEnumerable<Diagnostic>> GetDiagnostics(
        Project project, string filePath, ImmutableArray<DiagnosticAnalyzer> analyzers)
    {
        var compilation = await project.GetCompilationAsync();

        var analyzerOptions = new CompilationWithAnalyzersOptions(
            project.AnalyzerOptions,
            onAnalyzerException: null,
            concurrentAnalysis: true,
            logAnalyzerExecutionTime: false,
            reportSuppressedDiagnostics: false);

        var diagnostics =
            await compilation!.WithAnalyzers(analyzers, analyzerOptions).GetAnalyzerDiagnosticsAsync();

        return diagnostics.Where(diagnostic => !diagnostic.IsSuppressed
                                               && diagnostic.Severity >= DiagnosticSeverity.Warning
                                               && diagnostic.Location.IsInSource
                                               && diagnostic.Location.SourceTree != null
                                               && diagnostic.Location.SourceTree.FilePath == filePath);
    }

    private class DiagnosticProviderAdapter : FixAllContext.DiagnosticProvider
    {
        private static Task<IEnumerable<Diagnostic>> EmptyDiagnosticResult
            => Task.FromResult(Enumerable.Empty<Diagnostic>());

        private readonly IReadOnlyDictionary<Project, List<Diagnostic>> _diagnosticsByProject;

        internal DiagnosticProviderAdapter(IReadOnlyDictionary<Project, List<Diagnostic>> diagnosticsByProject)
        {
            _diagnosticsByProject = diagnosticsByProject;
        }

        public override Task<IEnumerable<Diagnostic>> GetAllDiagnosticsAsync(Project project,
            CancellationToken cancellationToken)
            => GetProjectDiagnosticsAsync(project, cancellationToken);

        public override async Task<IEnumerable<Diagnostic>> GetDocumentDiagnosticsAsync(Document document,
            CancellationToken cancellationToken)
        {
            var projectDiagnostics = await GetProjectDiagnosticsAsync(document.Project, cancellationToken);
            return projectDiagnostics
                .Where(diagnostic => diagnostic.Location.SourceTree?.FilePath == document.FilePath)
                .ToImmutableArray();
        }

        public override Task<IEnumerable<Diagnostic>> GetProjectDiagnosticsAsync(Project project,
            CancellationToken cancellationToken)
            => _diagnosticsByProject.ContainsKey(project)
                ? Task.FromResult<IEnumerable<Diagnostic>>(_diagnosticsByProject[project])
                : EmptyDiagnosticResult;
    }
}
