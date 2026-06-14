using FtDSharp.CodeGen.Utils;
using Scriban;

namespace FtDSharp.CodeGen.Generators;

public class TemplateRenderer
{
    private readonly Template _interfaceTemplate;
    private readonly Template _facadeTemplate;
    private readonly Template _logicalInterfacesTemplate;
    private readonly Template _blockFactoryTemplate;
    private readonly Template _blocksProviderInterfaceTemplate;
    private readonly Template _blocksApiTemplate;
    private readonly Template _blocksProviderImplTemplate;
    private readonly Template _missilePartInterfaceTemplate;
    private readonly Template _missilePartFacadeTemplate;
    private readonly Template _missilePartEnumsTemplate;
    private readonly Template _missilePartFactoryTemplate;

    public TemplateRenderer()
    {
        _interfaceTemplate = LoadTemplate("Interface.scriban");
        _facadeTemplate = LoadTemplate("Facade.scriban");
        _logicalInterfacesTemplate = LoadTemplate("LogicalInterfaces.scriban");
        _blockFactoryTemplate = LoadTemplate("BlockFactory.scriban");
        _blocksProviderInterfaceTemplate = LoadTemplate("BlocksProviderInterface.scriban");
        _blocksApiTemplate = LoadTemplate("BlocksApi.scriban");
        _blocksProviderImplTemplate = LoadTemplate("BlocksProviderImpl.scriban");
        _missilePartInterfaceTemplate = LoadTemplate("MissilePartInterface.scriban");
        _missilePartFacadeTemplate = LoadTemplate("MissilePartFacade.scriban");
        _missilePartEnumsTemplate = LoadTemplate("MissilePartEnums.scriban");
        _missilePartFactoryTemplate = LoadTemplate("MissilePartFactory.scriban");
    }

    private static Template LoadTemplate(string name)
    {
        var assembly = typeof(TemplateRenderer).Assembly;
        var resourceName = $"FtDSharp.CodeGen.Templates.{name}";

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Template not found: {resourceName}");
        using var reader = new StreamReader(stream);
        var content = reader.ReadToEnd();

        return Template.Parse(content, resourceName);
    }

    private static string Render(Template template, object model) =>
        template.Render(model, ScribanMemberNaming.ToSnakeCase);

    public string RenderInterface(Models.BlockDefinition block) =>
        Render(_interfaceTemplate, BlockTemplateModels.CreateInterfaceModel(block));

    public string RenderFacade(Models.BlockDefinition block) =>
        Render(_facadeTemplate, BlockTemplateModels.CreateFacadeModel(block));

    public string RenderLogicalInterfaces(List<LogicalInterfaceDefinition> definitions, List<Models.BlockDefinition> blocks) =>
        Render(_logicalInterfacesTemplate, BlockTemplateModels.CreateLogicalInterfacesModel(definitions, blocks));

    public string RenderBlockFactory(List<Models.BlockDefinition> blocks) =>
        Render(_blockFactoryTemplate, BlockTemplateModels.CreateBlockFactoryModel(blocks));

    public string RenderBlocksProviderInterface(List<Models.BlockDefinition> blocks) =>
        Render(_blocksProviderInterfaceTemplate, BlockTemplateModels.CreateBlocksAccessorModel(blocks));

    public string RenderBlocksApi(List<Models.BlockDefinition> blocks) =>
        Render(_blocksApiTemplate, BlockTemplateModels.CreateBlocksAccessorModel(blocks));

    public string RenderBlocksProviderImpl(List<Models.BlockDefinition> blocks) =>
        Render(_blocksProviderImplTemplate, BlockTemplateModels.CreateBlocksAccessorModel(blocks));

    public string RenderMissilePartInterface(MissilePartDefinition part) =>
        Render(_missilePartInterfaceTemplate, MissileTemplateModels.CreateInterfaceModel(part));

    public string RenderMissilePartFacade(MissilePartDefinition part) =>
        Render(_missilePartFacadeTemplate, MissileTemplateModels.CreateFacadeModel(part));

    public string RenderMissilePartEnums(List<GeneratedEnum> enums) =>
        Render(_missilePartEnumsTemplate, MissileTemplateModels.CreateEnumsModel(enums));

    public string RenderMissilePartFactory(List<MissilePartDefinition> parts) =>
        Render(_missilePartFactoryTemplate, MissileTemplateModels.CreateFactoryModel(parts));
}
