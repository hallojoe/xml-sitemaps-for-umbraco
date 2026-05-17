export const manifests: Array<UmbExtensionManifest> = [
  {
    name: "Umbraco Xml Sitemaps Entrypoint",
    alias: "Casko.XmlSitemapsForUmbraco.Entrypoint",
    type: "backofficeEntryPoint",
    js: () => import("./entrypoint.js"),
  },
];

