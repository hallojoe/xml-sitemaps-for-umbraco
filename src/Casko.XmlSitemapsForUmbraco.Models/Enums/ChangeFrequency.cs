using System.Xml.Serialization;

namespace Casko.XmlSitemapsForUmbraco.Models.Enums;

public enum ChangeFrequency
{
    [XmlEnum("none")]
    None,
    [XmlEnum("always")]
    Always,
    [XmlEnum("hourly")]
    Hourly,
    [XmlEnum("daily")]
    Daily,
    [XmlEnum("weekly")]
    Weekly,
    [XmlEnum("monthly")]
    Monthly,
    [XmlEnum("yearly")]
    Yearly,
    [XmlEnum("never")]
    Never
}