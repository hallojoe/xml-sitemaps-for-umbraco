using Umbraco.Cms.Core.Models;

namespace Casko.XmlSitemapsForUmbraco.Storage.UmbracoMedia;

public interface IUmbracoMediaFileAccessor
{
    public string? GetFilePath(IMedia media);

    public Stream OpenRead(string filePath);

    public void SetInitialFile(IMedia media, string fileName, Stream content);

}
