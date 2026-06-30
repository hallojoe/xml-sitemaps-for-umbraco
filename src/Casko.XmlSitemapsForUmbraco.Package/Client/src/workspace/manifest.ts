import { UMB_ADVANCED_SETTINGS_MENU_ALIAS } from "@umbraco-cms/backoffice/settings";
import { UMB_WORKSPACE_CONDITION_ALIAS } from "@umbraco-cms/backoffice/workspace";

const XML_SITEMAPS_WORKSPACE_ALIAS = "Casko.XmlSitemapsForUmbraco.Workspace.XmlSitemaps";
const XML_SITEMAPS_ENTITY_TYPE = "casko-xml-sitemaps";

export const manifests: Array<UmbExtensionManifest> = [
  {
    type: "workspace",
    kind: "default",
    name: "XML Sitemaps Workspace",
    alias: XML_SITEMAPS_WORKSPACE_ALIAS,
    meta: {
      entityType: XML_SITEMAPS_ENTITY_TYPE,
      headline: "XML Sitemaps",
    },
  },
  {
    type: "workspaceView",
    name: "XML Sitemaps Configuration Workspace View",
    alias: "Casko.XmlSitemapsForUmbraco.WorkspaceView.Configuration",
    element: () => import("./configuration-workspace-view.element.js"),
    weight: 500,
    meta: {
      label: "Configuration",
      pathname: "configuration",
      icon: "icon-list",
    },
    conditions: [
      {
        alias: UMB_WORKSPACE_CONDITION_ALIAS,
        match: XML_SITEMAPS_WORKSPACE_ALIAS,
      },
    ],
  },
  {
    type: "menuItem",
    name: "XML Sitemap Settings Menu Item",
    alias: "Casko.XmlSitemapsForUmbraco.MenuItem.XmlSitemap",
    weight: 500,
    meta: {
      label: "XML Sitemaps",
      icon: "icon-code",
      entityType: XML_SITEMAPS_ENTITY_TYPE,
      menus: [UMB_ADVANCED_SETTINGS_MENU_ALIAS],
    },
  },
];
